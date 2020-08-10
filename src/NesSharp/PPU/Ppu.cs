using NesSharp.CPU;
using NesSharp.Extensions;
using NesSharp.PPU.Registers;
using System;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NesSharp.PPU
{
    class Ppu: Clockeable
    {
        private const int NtscMasterClockCycle = 5;
        private const int Width = 256;
        private const int Height = 240;

        #region NES color palette
        private static readonly int[] SystemColorPalette = new int[]
        {
            -9079435,
            -14214257,
            -16777045,
            -12124001,
            -7405449,
            -5570541,
            -5832704,
            -8451328,
            -12374272,
            -16759040,
            -16756480,
            -16761065,
            -14991521,
            -16777216,
            -16777216,
            -16777216,
            -4408132,
            -16747537,
            -14468113,
            -8191757,
            -4259649,
            -1638309,
            -2413824,
            -3453169,
            -7638272,
            -16738560,
            -16733440,
            -16739525,
            -16743541,
            -16777216,
            -16777216,
            -16777216,
            -1,
            -12599297,
            -10512385,
            -5796867,
            -558081,
            -34889,
            -34973,
            -25797,
            -803009,
            -8137965,
            -11542709,
            -10946408,
            -16716837,
            -16777216,
            -16777216,
            -16777216,
            -1,
            -5511169,
            -3680257,
            -2634753,
            -14337,
            -14373,
            -16461,
            -9301,
            -6237,
            -1835101,
            -5508161,
            -4980785,
            -6291469,
            -16777216,
            -16777216,
            -16777216
        };
        #endregion
        
        private readonly NmiTrigger _nmi;
        private readonly PpuBus _ppuBus;
        private readonly int[] _frameBuffer = new int[Width * Height];
        private readonly int[] _scanlineOamBuffer = new int[32];
        private readonly int[] _spriteCounters = new int[8];
        private readonly int[] _spriteAttributes = new int[8];
        private readonly int[] _spriteLowPlaneTiles = new int[8];
        private readonly int[] _spriteHighPlaneTiles = new int[8];

        private byte _dataBuffer = 0;
        private bool _isSpriteZeroInBuffer;
        private uint _framesRendered = 0;
        private int _fineX;
        private int _cycles = 0;
        private int _scanline = -1;
        private int _lowBackgroundShiftRegister;
        private int _highBackgroundShiftRegister;
        private int _lowAttributeShiftRegister;
        private int _highAttributeShiftRegister;
        private int _tileId;
        private int _attribute;
        private int _blockId;
        private int _lowPixelsRow;
        private int _highPixelsRow;

        private bool IsRenderingEnabled => Mask.RenderBackground || Mask.RenderSprites;

        /// <summary>
        /// Controls the state of the address latch (false = high byte; true = low byte).
        /// </summary>
        private bool _addressLatch = false;

        private int _spriteBufferIndex = 0;
        private int _spriteY;
        private int _spriteTileIndex;
        private bool _flipSpriteVertically;
        private bool _flipSpriteHorizontally;
        private bool _isEmptySprite;

        #region Registers
        /// <summary>
        /// PPU Control register.
        /// </summary>
        public PpuControl Control { get; } = new PpuControl();

        /// <summary>
        /// PPU Mask register.
        /// </summary>
        public PpuMask Mask { get; } = new PpuMask();

        /// <summary>
        /// PPU Status register.
        /// </summary>
        public PpuStatus Status { get; } = new PpuStatus();

        /// <summary>
        /// Object Attribute Memory (OAM) address register.
        /// </summary>
        public byte OamAddress { get; set; }

        /// <summary>
        /// OAM data register.
        /// </summary>
        public byte OamData
        {
            get => _oamBuffer[OamAddress];
            set
            {
                _oamBuffer[OamAddress] = value;

                // OAM address gets incremented by one when data is written
                OamAddress++;
            }
        }

        /// <summary>
        /// Scrolling position register.
        /// </summary>
        public byte Scroll
        {
            set
            {
                if (!_addressLatch) // w is 0
                {
                    T.CoarseX = value >> 3;
                    _fineX = value & 7;

                    _addressLatch = true; // Flips to the low byte state
                }
                else // w is 1
                {
                    T.CoarseY = value >> 3;
                    T.FineY = value & 7;

                    _addressLatch = false; // Flips to the high byte state
                }
            }
        }

        /// <summary>
        /// Address where the data should be either written or read.
        /// </summary>
        public byte PpuAddress
        {
            set
            {
                if (!_addressLatch) // w is 0
                {
                    value = (byte)(value & 0x3F);
                    T.Loopy = (T.Loopy & 0x00FF) | (value << 8);

                    _addressLatch = true; // Flips to the low byte state
                }
                else // w is 1
                {
                    T.Loopy = (T.Loopy & 0x7F00) | value;
                    V.Loopy = T.Loopy;

                    _addressLatch = false; // Flips to the high byte state
                }
            }
        }

        /// <summary>
        /// The PPU data.
        /// </summary>
        public byte PpuData
        {
            get
            {
                // Reads the data buffered (from previous read request)
                byte data = _dataBuffer;

                // Updates the buffer with the data allocated in the compiled address
                _dataBuffer = _ppuBus.Read(V.Address);

                /* If the compiled address does not overlap the color palette address range, then return
                 * the data read from the buffer; otherwise return the data read from the address right away
                 */
                if (V.Loopy >= 0x3F00)
                    data = _dataBuffer;

                IncrementVRamAddress();

                return data;
            }
            set
            {
                _ppuBus.Write(V.Address, value);
                IncrementVRamAddress();
            }
        }

        /*Loopy registers*/

        public PpuLoopy V { get; } = new PpuLoopy();
        public PpuLoopy T { get; } = new PpuLoopy();

        #endregion

        public int[] Frame => _frameBuffer;

        /// <summary>
        /// The PPU OAM (it's 256 bytes long, capable of store 64 sprites, sprite data is 4 bytes long).
        /// </summary>
        private readonly byte[] _oamBuffer = new byte[256];

        public Ppu(PpuBus ppuBus, NmiTrigger nmiTrigger)
        {
            _ppuBus = ppuBus;
            _nmi = nmiTrigger;
        }

        /// <summary>
        /// Step over a frame pixel.
        /// </summary>
        private void Step()
        {
            // Pre-render scanline (in the NTSC frame diagram it's labeled as scanline 261)
            if (_scanline >= -1 && _scanline < 240)
            {
                // Update shift registers by shifting one position to the left
                if (Mask.RenderBackground && (_cycles >= 2 && _cycles <= 257) || (_cycles >= 322 && _cycles <= 337))
                    ShiftBackgroundRegisters();

                if (_scanline == -1)
                    PreRender();
                else
                    RenderPixel();

                //if (_cycles >= 1 && _cycles <= 257)
                //    ShiftSpriteRegisters();

                // Background rendering process
                if ((_cycles >= 1 && _cycles <= 256) || (_cycles >= 321 && _cycles <= 336))
                    EvaluateBackground();

                // Sprite Evaluation
                if (_scanline >= 0)
                    EvaluateSprites();

                // Increments the horizontal component in the V register
                if (IsRenderingEnabled && ((_cycles >= 1 && _cycles <= 256) || (_cycles >= 328 && _cycles <= 336)) && _cycles % 8 == 0)
                    V.IncrementHorizontalPosition();

                // Increments the vertical component in the V register
                if (IsRenderingEnabled && _cycles == 256)
                    V.IncrementVerticalPosition();

                // Copy the horizontal component from T register into V register
                if (IsRenderingEnabled && _cycles == 257)
                    CopyHorizontalPositionToV();

                // During cycles elapesed between 257 and 320 (inclusive), the OAM address is set to 0
                if (_cycles >= 257 && _cycles <= 320)
                    OamAddress = 0;
            }
            else if (_scanline >= 241 && _scanline < 261)
                VerticalBlankPeriod();

            MasterClockCycles += NtscMasterClockCycle;
            _cycles++;
            if (_cycles >= 341)
            {
                _cycles = 0;
                _spriteBufferIndex = 0;
                _scanline++;

                if (_scanline > 260)
                    ResetFrameRenderingStatus();
            }
        }

        public override void RunUpTo(int masterClockCycles)
        {
            while (MasterClockCycles <= masterClockCycles)
                Step();
        }

        /// <summary>
        /// Resets the address latch used by the PPU address register and PPU scroll register.
        /// </summary>
        public void ResetAddressLatch() => _addressLatch = false;

        /// <summary>
        /// Resets the frame rendering status.
        /// </summary>
        private void ResetFrameRenderingStatus()
        {
            _scanline = -1;
            _framesRendered++;

            // When rendering is enabled and the next frame generated is odd, the idle tick will be skipped
            if (IsRenderingEnabled && _framesRendered % 2 != 0)
            {
                _cycles = 1;
                MasterClockCycles += NtscMasterClockCycle;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ParseBackgroundPaletteAddress(int paletteId, int colorIndex) => 0x3F00 + paletteId * 4 + colorIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ParsePalette(int attribute, int blockId)
        {
            int lowBit, highBit;

            switch (blockId)
            {
                case 0: // Top left
                    highBit = (attribute & 0b00000010) == 0b00000010 ? 1 : 0;
                    lowBit = (attribute & 0b00000001) == 0b00000001 ? 1 : 0;
                    break;
                case 1: // Top right
                    highBit = (attribute & 0b00001000) == 0b00001000 ? 1 : 0;
                    lowBit = (attribute & 0b00000100) == 0b00000100 ? 1 : 0;
                    break;
                case 2: // Bottom left
                    highBit = (attribute & 0b00100000) == 0b00100000 ? 1 : 0;
                    lowBit = (attribute & 0b00010000) == 0b00010000 ? 1 : 0;
                    break;
                case 3: // Bottom right
                    highBit = (attribute & 0b10000000) == 0b10000000 ? 1 : 0;
                    lowBit = (attribute & 0b01000000) == 0b01000000 ? 1 : 0;
                    break;
                default:
                    throw new InvalidOperationException($"The given block ID is invalid: ${blockId}.");
            }

            return lowBit | (highBit << 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ParseBlock(int coarseX, int coarseY)
        {
            var x = coarseX % 4;
            var y = coarseY % 4;

            int blockId;
            if (x <= 1 && y <= 1)
                blockId = 0;
            else if (x <= 3 && y <= 1)
                blockId = 1;
            else if (x <= 1 && y <= 3)
                blockId = 2;
            else if (x <= 3 && y <= 3)
                blockId = 3;
            else
                throw new InvalidOperationException($"Can not identify the Block ID based on the coarses specified: Coarse X:{coarseX} Coarse Y:{coarseY}");

            return blockId;
        }

        /// <summary>
        /// Increments the compiled address that points to a VRAM location.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void IncrementVRamAddress() => V.Loopy += Control.VRamAddressIncrement;

        /// <summary>
        /// Checks if the sprite is in the Y-axis range.
        /// </summary>
        /// <param name="y">The top position of the sprite in the Y-axis.</param>
        /// <returns>True if it is in the range, otherwise false.</returns>
        private bool IsSpriteInRange(int y)
        {
            int spriteHeight = Control.SpriteSize ? 16 : 8;

            return _scanline >= y && _scanline < (y + spriteHeight);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EvaluateBackground()
        {
            int stage = _cycles % 8;
            switch (stage)
            {
                // Load high background pattern table byte
                case 0:
                    _highPixelsRow = _ppuBus.ReadCharacterRom((ushort)(Control.BackgroundPatternTableAddress + (_tileId * 16) + V.FineY + 8));
                    break;
                case 1:
                    // Load background shift registers
                    {
                        /*Note: When jumping to next scanline, in the first cycle i reload the low byte of all registers
                         * with the data that i already had in the latches from the last "useful" 8 cycles of previous scanline (remember that
                         * at the ending of each scanline, we load the shift registers with the data of the the first 2 tiles of the next scanline).
                         */

                        // Load background shift registers
                        _lowBackgroundShiftRegister = (_lowBackgroundShiftRegister & 0xFF00) | _lowPixelsRow;
                        _highBackgroundShiftRegister = (_highBackgroundShiftRegister & 0xFF00) | _highPixelsRow;

                        // Load attribute shift registers
                        int palette = ParsePalette(_attribute, _blockId); // 2 bit value

                        // The same bit is propagated to all bits in the attribute shift register
                        bool lowBit = palette.GetBit(0);
                        if (lowBit)
                            _lowAttributeShiftRegister = (_lowAttributeShiftRegister & 0xFF00) | 0xFF;
                        else
                            _lowAttributeShiftRegister &= 0xFF00;

                        bool highBit = palette.GetBit(1);
                        if (highBit)
                            _highAttributeShiftRegister = (_highAttributeShiftRegister & 0xFF00) | 0xFF;
                        else
                            _highAttributeShiftRegister &= 0xFF00;
                    }
                    break;
                // Fetch nametable byte
                case 2:
                    _tileId = _ppuBus.ReadNametable((ushort)(0x2000 | (V.Loopy & 0x0FFF)));
                    break;
                // Fetch attribute table byte
                case 4:
                    _attribute = _ppuBus.ReadNametable((ushort)(0x23C0 | (V.Loopy & 0x0C00) | ((V.Loopy >> 4) & 0x38) | ((V.Loopy >> 2) & 0x07)));
                    _blockId = ParseBlock(V.CoarseX, V.CoarseY);
                    break;
                // Fetch low background pattern table byte
                case 6:
                    _lowPixelsRow = _ppuBus.ReadCharacterRom((ushort)(Control.BackgroundPatternTableAddress + (_tileId * 16) + V.FineY));
                    break;
            }
        }

        private void EvaluateSprites()
        {
            // Initializes to $FF the OAM buffer
            if (_cycles == 1)
            {
                Array.Fill(_scanlineOamBuffer, 0xFF);
            }
            // Fill the secondary (scanline) oam buffer at once
            else if (_cycles == 64)
            {
                FillScanlineOamBuffer();
            }
            else if (_cycles >= 257 && _cycles <= 320)
            {
                int stage = (_cycles - 1) & 7;
                switch (stage)
                {
                    // Parse the Y coordinate
                    case 0:
                        _spriteY = _scanline - _scanlineOamBuffer[_spriteBufferIndex * 4];
                        _isEmptySprite = _scanlineOamBuffer[_spriteBufferIndex * 4] == 0xFF;
                        break;
                    case 1:
                        _spriteTileIndex = _scanlineOamBuffer[(_spriteBufferIndex * 4) + 1];
                        break;
                    // Parse attributes (palette, flip horizontally, flip vertically, priority)
                    case 2:
                        int attributes = _scanlineOamBuffer[(_spriteBufferIndex * 4) + 2];
                        _spriteAttributes[_spriteBufferIndex] = attributes;
                        _flipSpriteHorizontally = attributes.GetBit(6);
                        _flipSpriteVertically = attributes.GetBit(7);
                        break;
                    case 3:
                        _spriteCounters[_spriteBufferIndex] = _scanlineOamBuffer[(_spriteBufferIndex * 4) + 3];
                        break;
                    case 5:
                        ReadSpriteLowPlane();
                        break;
                    case 7:
                        ReadSpriteHighPlane();
                        _spriteBufferIndex++;
                        break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetTallSpritePatternTableAddress() => _spriteTileIndex.GetBit(0) ? 0x1000 : 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReadSpriteLowPlane()
        {
            // fetch sprite low tile
            int lowPlane = 0;
            if (!_isEmptySprite)
            {
                // 8 x 16 sprites
                if (Control.SpriteSize)
                {
                    int patternTableAddress = GetTallSpritePatternTableAddress();
                    int spriteId = _spriteTileIndex & 0xFE;
                    int y = _flipSpriteVertically ? (15 - _spriteY) : _spriteY;

                    // top half
                    if (y < 8)
                    {
                        lowPlane = _ppuBus.ReadCharacterRom((ushort)(patternTableAddress + (spriteId * 16) + y));

                    } // bottom half
                    else
                    {
                        //spriteId++; // bottom half tile is next to the top half tile in the pattern table
                        lowPlane = _ppuBus.ReadCharacterRom((ushort)(patternTableAddress + ((spriteId + 1) * 16) + (y - 8)));
                    }

                } // 8 x 8 sprites
                else
                {
                    int flipOffset = _flipSpriteVertically ? (7 - _spriteY) : _spriteY;

                    lowPlane = _ppuBus.ReadCharacterRom((ushort)(Control.SpritePatternTableAddress + (_spriteTileIndex * 16) + flipOffset));
                }

                if (_flipSpriteHorizontally)
                {
                    lowPlane = lowPlane.MirrorBits().Byte();
                }
            }

            _spriteLowPlaneTiles[_spriteBufferIndex] = lowPlane;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReadSpriteHighPlane()
        {
            // fetch sprite high tile
            int highPlane = 0;

            if (!_isEmptySprite)
            {
                // 8 x 16 sprites
                if (Control.SpriteSize)
                {
                    int patternTableAddress = GetTallSpritePatternTableAddress();
                    int spriteId = _spriteTileIndex & 0xFE;
                    int y = _flipSpriteVertically ? (15 - _spriteY) : _spriteY;

                    // top half
                    if (y < 8)
                    {
                        highPlane = _ppuBus.ReadCharacterRom((ushort)(patternTableAddress + (spriteId * 16) + y + 8));

                    } // bottom half
                    else
                    {
                        //spriteId++; // bottom half tile is next to the top half tile in the pattern table
                        highPlane = _ppuBus.ReadCharacterRom((ushort)(patternTableAddress + ((spriteId + 1) * 16) + (y - 8) + 8));
                    }

                } // 8 x 8 sprites
                else
                {
                    int flipOffset = _flipSpriteVertically ? (7 - _spriteY) : _spriteY;

                    highPlane = _ppuBus.ReadCharacterRom((ushort)(Control.SpritePatternTableAddress + (_spriteTileIndex * 16) + flipOffset + 8));
                }

                if (_flipSpriteHorizontally)
                {
                    highPlane = highPlane.MirrorBits().Byte();
                }
            }

            _spriteHighPlaneTiles[_spriteBufferIndex] = highPlane;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FillScanlineOamBuffer()
        {
            int bufferIndex = 0;
            int spritesBuffered = 0;
            _isSpriteZeroInBuffer = false;

            for (int n = 0; n < _oamBuffer.Length; n += 4)
            {
                int spriteYPos = _oamBuffer[n];
                if (IsSpriteInRange(spriteYPos))
                {
                    if (spritesBuffered < 8)
                    {
                        _scanlineOamBuffer[bufferIndex] = spriteYPos;
                        _scanlineOamBuffer[bufferIndex + 1] = _oamBuffer[n + 1];
                        _scanlineOamBuffer[bufferIndex + 2] = _oamBuffer[n + 2];
                        _scanlineOamBuffer[bufferIndex + 3] = _oamBuffer[n + 3];

                        bufferIndex += 4;
                        spritesBuffered++;

                        if (n == 0 && !_isSpriteZeroInBuffer)
                            _isSpriteZeroInBuffer = true;
                    }
                    else if (!Status.SpriteOverflow) // An overflow has ocurred then!
                    {
                        Status.SpriteOverflow = true;
                        break;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ShiftBackgroundRegisters()
        {
            _lowBackgroundShiftRegister <<= 1;
            _highBackgroundShiftRegister <<= 1;
            _lowAttributeShiftRegister <<= 1;
            _highAttributeShiftRegister <<= 1;
        }

        private void ShiftSpriteRegisters()
        {
            if (Mask.RenderSprites)
            {
                for (int i = 0; i < _spriteCounters.Length; i++)
                {
                    if (_spriteCounters[i] > 0)
                    {
                        _spriteCounters[i]--;
                    }
                    else
                    {
                        _spriteLowPlaneTiles[i] <<= 1;
                        _spriteHighPlaneTiles[i] <<= 1;
                    }
                }
            }
        }

        /// <summary>
        /// Copy the bits related to horizontal position from T register to transfer into V register.
        /// </summary>
        private void CopyHorizontalPositionToV()
        {
            V.CoarseX = T.CoarseX;
            V.Nametable = (V.Nametable & 2) | (T.Nametable & 1);
        }

        /// <summary>
        /// Copy the bits related to vertical position from T register to transfer into V register.
        /// </summary>
        private void CopyVerticalPositionToV()
        {
            V.CoarseY = T.CoarseY;
            V.FineY = T.FineY;
            V.Nametable = (V.Nametable & 1) | ((T.Nametable & 2) << 1);
        }

        private void PreRender()
        {
            if (_cycles == 1)
            {
                Status.SpriteOverflow = false;
                Status.SpriteZeroHit = false;
                Status.VerticalBlank = false;
            }
            else if (IsRenderingEnabled && _cycles >= 280 && _cycles <= 304)
            {
                CopyVerticalPositionToV();
            }
        }

        private void RenderPixel()
        {
            if (_cycles < 1 || _cycles > 256)
                return;
            
            // When a pixel equals 0, it means the pixel is transparent. When pixel is different than 0, then pixel is opaque
            int backgroundPixel = 0;
            int backgroundPalette = 0;

            if (Mask.RenderBackground)
            {
                int pixelMux = 0x8000 >> _fineX;
                var lsbPixelColorIdx = (_lowBackgroundShiftRegister & pixelMux) == pixelMux ? 1 : 0;
                var msbPixelColorIdx = (_highBackgroundShiftRegister & pixelMux) == pixelMux ? 1 : 0;

                backgroundPixel = lsbPixelColorIdx | (msbPixelColorIdx << 1);

                var lsbPaletteIdx = (_lowAttributeShiftRegister & pixelMux) == pixelMux ? 1 : 0;
                var msbPaletteIdx = (_highAttributeShiftRegister & pixelMux) == pixelMux ? 1 : 0;

                backgroundPalette = lsbPaletteIdx | (msbPaletteIdx << 1);
            }

            int spritePixel = 0;
            int spritePalette = 0;
            bool spritePriority = false;
            bool spriteZeroRendering = false;

            if (Mask.RenderSprites)
            {
                for (int i = 0; i < 8; i++)
                {
                    // If counter equals to 0, then the sprite became active
                    int diff = _cycles - _spriteCounters[i];
                    if (diff >= 0 && diff < 8)
                    {
                        int mask = 0x80 >> diff;
                        int pixelLowBit = (_spriteLowPlaneTiles[i] & mask) == mask ? 1 : 0;
                        int pixelHighBit = (_spriteHighPlaneTiles[i] & mask) == mask ? 1 : 0;

                        spritePixel = pixelLowBit | (pixelHighBit << 1);
                        spritePalette = (_spriteAttributes[i] & 3) + 4; // Add 4 because the sprite palettes are from 4-7
                        spritePriority = (_spriteAttributes[i] & 0x20) == 0x20;

                        // Sprite pixel is opaque
                        if (spritePixel != 0)
                        {
                            // When the pixel of the sprite 0 is opaque, we must set the flag of sprite zero hit
                            if (_isSpriteZeroInBuffer && i == 0 && backgroundPixel != 0)
                                spriteZeroRendering = true;

                            break;
                        }
                    }
                }
            }

            int pixel = 0;
            int palette = 0;

            if (backgroundPixel == 0 && spritePixel != 0)
            {
                pixel = spritePixel;
                palette = spritePalette;
            }else if (backgroundPixel != 0 && spritePixel == 0)
            {
                pixel = backgroundPixel;
                palette = backgroundPalette;
            }else if (backgroundPixel != 0 && spritePixel != 0)
            {
                // Behind background
                if (spritePriority)
                {
                    pixel = backgroundPixel;
                    palette = backgroundPalette;
                }
                // In front of background
                else
                {
                    pixel = spritePixel;
                    palette = spritePalette;
                }

                if (_isSpriteZeroInBuffer && spriteZeroRendering && Mask.RenderBackground && Mask.RenderSprites)
                {
                    // If either sprites or background must be hidden (in the first 8 pixels), then don't set the sprite zero hit unless the first 8 pixels has been rendered
                    if (!Mask.RenderLeftSideBackground || !Mask.RenderLeftSideSprites)
                    {
                        if (_cycles > 8)
                            Status.SpriteZeroHit = true;
                    }
                    else
                    {
                        Status.SpriteZeroHit = true;
                    }
                }
            }

            _frameBuffer[(_cycles - 1) + _scanline * 256] = GetPaletteColor(palette, pixel);
        }

        /// <summary>
        /// Retrieves the color given the palette and its color entry.
        /// </summary>
        /// <param name="palette">The color palette (0-7).</param>
        /// <param name="colorIndex">The color index (0-4, where 0 means transparent color: background color).</param>
        /// <returns>The color from the palette.</returns>
        private int GetPaletteColor(int palette, int colorIndex)
        {
            int paletteColorAddress = ParseBackgroundPaletteAddress(palette, colorIndex);
            int paletteColor = _ppuBus.ReadPalette((ushort)paletteColorAddress);

            return SystemColorPalette[paletteColor];
        }

        private void VerticalBlankPeriod()
        {
            if (_scanline == 241 && _cycles == 1)
            {
                Status.VerticalBlank = true;
                if (Control.TriggerNmi)
                    _nmi.Invoke();
            }
        }
    }
}
