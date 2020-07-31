using MiNES.CPU;
using MiNES.Extensions;
using MiNES.PPU.Registers;
using System;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MiNES.PPU
{
    class Ppu
    {
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

        /// <summary>
        /// The number of the master clock ticks required in order to reach the tick 2 of the first vertical blank scanline.
        /// </summary>
        /// <remarks>
        /// It's used for determine up to which moment the PPU should be emulated.
        /// </remarks>
        public const int MasterClockTicks = 412625;

        /// <summary>
        /// Count how many frames has been rendered so far.
        /// </summary>
        public bool IsIdle;
        //public bool IsIdle { get; private set; }

        public int Cycles => _cycles;

        /// <summary>
        /// Object Attribute Memory (OAM) address register.
        /// </summary>
        public byte OamAddress;

        public bool IsFrameCompleted { get; private set; }

        public bool NmiRequested;

        public int[] Frame => _frameBuffer;

        /// <summary>
        /// PPU Control register.
        /// </summary>
        internal readonly PpuControl Control = new PpuControl();

        /// <summary>
        /// PPU Mask register.
        /// </summary>
        internal readonly PpuMask Mask = new PpuMask();

        /* When powering up the PPU, the status register (located at $2002) will
         * be set to 0xA0 (10100000).
         * 
         * Bit 7: 0 = the PPU is not in the V-Blank area (Vertical Blank is the area non visible of the screen); 1 = the PPU is in the V-Blank area
         * Bit 6: Sprite hit (more research about this)
         * Bit 5: Sprite overflow (more than 8 sprites appears in a scanline)
         */

        /// <summary>
        /// PPU Status register.
        /// </summary>
        internal readonly PpuStatus Status = new PpuStatus();

        internal readonly PpuLoopy V = new PpuLoopy();
        internal readonly PpuLoopy T = new PpuLoopy();

        private readonly PpuBus _ppuBus;
        private readonly int[] _frameBuffer = new int[Width * Height];

        /// <summary>
        /// The PPU OAM (it's 256 bytes long, capable of store 64 sprites, sprite data is 4 bytes long).
        /// </summary>
        public byte[] OamBuffer { get; set; } = new byte[256];

        private readonly int[] _scanlineOamBuffer = new int[32];

        private bool _isSpriteZeroInBuffer;

        private int _dataBuffer = 0;

        private uint _framesRendered = 1;
        
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

        private int[] _spriteCounters = new int[8];
        private int[] _spriteAttributes = new int[8];
        private int[] _spriteLowPlaneTiles = new int[8];
        private int[] _spriteHighPlaneTiles = new int[8];

        private int _spriteBufferIndex = 0;
        private int _spriteY;
        private int _spriteTileIndex;
        private bool _flipSpriteVertically;
        private bool _flipSpriteHorizontally;
        private bool _isEmptySprite;

        /// <summary>
        /// Denotes whether frame being rendered is odd or not.
        /// </summary>
        /// <remarks>
        /// Initially would be false because frame 1 would be the first frame to render.
        /// </remarks>
        private bool _isOddFrame = true;
        public bool SkipIdleTick => _isOddFrame && IsRenderingEnabled;



        private readonly NES _nes;

        public Ppu(PpuBus ppuBus, NES nes)
        {
            _ppuBus = ppuBus;
            _nes = nes;
            //ResetFrameRenderingStatus();
        }

        public void SetOamData(byte data)
        {
            OamBuffer[OamAddress] = data;

            // OAM address gets incremented by one when data is written
            OamAddress++;
        }

        public byte GetOamData() => OamBuffer[OamAddress];

        /// <summary>
        /// Resets the address latch used by the PPU address register and PPU scroll register.
        /// </summary>
        public void ResetAddressLatch()
        {
            _addressLatch = false;
        }

        /// <summary>
        /// Sets the address into the PPU address register.
        /// </summary>
        /// <param name="value">The value of the address (either high or low byte, depending on the latch).</param>
        public void SetAddress(int value)
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

        /// <summary>
        /// Sets the scroll into Loppy T register.
        /// </summary>
        /// <param name="value">The value of the register ("d" in nesdev docs).</param>
        public void SetScroll(byte value)
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

        /// <summary>
        /// Retrieves the data from the address set through the PPU address register.
        /// </summary>
        /// <returns>The data allocated in the address set through the PPU address register.</returns>
        public int GetPpuData()
        {
            // Reads the data buffered (from previous read request)
            int data = _dataBuffer;

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

        /// <summary>
        /// Stores a value in the address set through the PPU address register.
        /// </summary>
        /// <param name="val">The value that will be stored.</param>
        public void SetPpuData(byte val)
        {
            _ppuBus.Write(V.Address, val);
            IncrementVRamAddress();
        }

        public void Step()
        {
            // Cycle 0 does not do anything (it's idle)
            if (_cycles == 0)
            {
                _cycles = 1;
                return;
            }

            // Pre-render scanline (in the NTSC frame diagram it's labeled as scanline 261)
            if (_scanline >= -1 && _scanline < 240)
            {
                // Update shift registers by shifting one position to the left
                if ((_cycles >= 2 && _cycles <= 257) || (_cycles >= 322 && _cycles <= 337))
                    ShiftBackgroundRegisters();

                if (_scanline == -1)
                    PreRenderingScanline();
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
                //if (_cycles == 257)
                if (_cycles >= 257 && _cycles <= 320)
                    OamAddress = 0;
            }
            else if (_scanline >= 241 && _scanline < 261)
                VerticalBlankScanlines();

            _cycles++;
            if (_cycles >= 341 || (_cycles >= 340 && _scanline == -1 && _isOddFrame && IsRenderingEnabled)) // When is an odd frame and we are in pre render scanline, the scanline is 340 cycles long (only when rendering is enabled)
            {
                _cycles = 0;
                _spriteBufferIndex = 0;
                _scanline++;

                //if (_scanline < 260)
                //{
                //    _scanline++;
                //}
                //else
                //{
                //    //_scanline = -1;

                //    //Frames++;
                //    //_isOddFrame = Frames % 2 != 0;

                //    //IsFrameCompleted = true;
                //}
            }

        }

        /// <summary>
        /// Resets the frame rendering status.
        /// </summary>
        public void ResetFrameRenderingStatus()
        {
            _cycles = 0;
            _scanline = -1;
            _framesRendered++;
            _isOddFrame = _framesRendered % 2 != 0;
            IsIdle = false;
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
            int stage = _cycles & 7;
            switch (stage)
            {
                // Load high background pattern table byte
                case 0:
                    _highPixelsRow = _ppuBus.ReadCharacterRom((uint)(Control.BackgroundPatternTableAddress + (_tileId * 16) + V.FineY + 8));
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
                            _lowAttributeShiftRegister = _lowAttributeShiftRegister & 0xFF00;

                        bool highBit = palette.GetBit(1);
                        if (highBit)
                            _highAttributeShiftRegister = (_highAttributeShiftRegister & 0xFF00) | 0xFF;
                        else
                            _highAttributeShiftRegister = _highAttributeShiftRegister & 0xFF00;
                    }
                    break;
                // Fetch nametable byte
                case 2:
                    _tileId = _ppuBus.ReadNametable((uint)(0x2000 | (V.Loopy & 0x0FFF)));
                    break;
                // Fetch attribute table byte
                case 4:
                    _attribute = _ppuBus.ReadNametable((uint)(0x23C0 | (V.Loopy & 0x0C00) | ((V.Loopy >> 4) & 0x38) | ((V.Loopy >> 2) & 0x07)));
                    _blockId = ParseBlock(V.CoarseX, V.CoarseY);
                    break;
                // Fetch low background pattern table byte
                case 6:
                    _lowPixelsRow = _ppuBus.ReadCharacterRom((uint)(Control.BackgroundPatternTableAddress + (_tileId * 16) + V.FineY));
                    break;
            }
        }

        private void EvaluateSprites()
        {
            // Initializes to $FF the OAM buffer
            if (_cycles >= 1 && _cycles <= 64)
            {
                _scanlineOamBuffer[_cycles & 0x1F] = 0xFF;
            }
            else if (_cycles > 64 && _cycles <= 256)
            {
                // Fill the secondary oam buffer at once
                if (_cycles == 256)
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
                        lowPlane = _ppuBus.ReadCharacterRom((uint)(patternTableAddress + (spriteId * 16) + y));

                    } // bottom half
                    else
                    {
                        //spriteId++; // bottom half tile is next to the top half tile in the pattern table
                        lowPlane = _ppuBus.ReadCharacterRom((uint)(patternTableAddress + ((spriteId + 1) * 16) + (y - 8)));
                    }

                } // 8 x 8 sprites
                else
                {
                    int flipOffset = _flipSpriteVertically ? (7 - _spriteY) : _spriteY;

                    lowPlane = _ppuBus.ReadCharacterRom((uint)(Control.SpritePatternTableAddress + (_spriteTileIndex * 16) + flipOffset));
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
                        highPlane = _ppuBus.ReadCharacterRom((uint)(patternTableAddress + (spriteId * 16) + y + 8));

                    } // bottom half
                    else
                    {
                        //spriteId++; // bottom half tile is next to the top half tile in the pattern table
                        highPlane = _ppuBus.ReadCharacterRom((uint)(patternTableAddress + ((spriteId + 1) * 16) + (y - 8) + 8));
                    }

                } // 8 x 8 sprites
                else
                {
                    int flipOffset = _flipSpriteVertically ? (7 - _spriteY) : _spriteY;

                    highPlane = _ppuBus.ReadCharacterRom((uint)(Control.SpritePatternTableAddress + (_spriteTileIndex * 16) + flipOffset + 8));
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

            for (int n = 0; n < OamBuffer.Length; n += 4)
            {
                int spriteYPos = OamBuffer[n];
                if (IsSpriteInRange(spriteYPos))
                {
                    if (spritesBuffered < 8)
                    {
                        _scanlineOamBuffer[bufferIndex] = spriteYPos;
                        _scanlineOamBuffer[bufferIndex + 1] = OamBuffer[n + 1];
                        _scanlineOamBuffer[bufferIndex + 2] = OamBuffer[n + 2];
                        _scanlineOamBuffer[bufferIndex + 3] = OamBuffer[n + 3];

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
            if (Mask.RenderBackground)
            {
                _lowBackgroundShiftRegister <<= 1;
                _highBackgroundShiftRegister <<= 1;
                _lowAttributeShiftRegister <<= 1;
                _highAttributeShiftRegister <<= 1;
            }
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

        private void PreRenderingScanline()
        {
            if (_cycles == 1)
            {
                Status.SpriteOverflow = false;
                Status.SpriteZeroHit = false;
                Status.VerticalBlank = false;
            }
            else if (IsRenderingEnabled && _cycles >= 280 && _cycles <= 304)
            //else if (IsRenderingEnabled && _cycles == 280)
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
            int paletteColor = _ppuBus.ReadPalette((uint)paletteColorAddress);
            //byte paletteColor = _ppuBus.Read(paletteColorAddress);
            //if (paletteColor < 0 || paletteColor > SystemColorPalette.Length)
            //    throw new InvalidOperationException($"The given palette color does not exist: {paletteColor}.");

            return SystemColorPalette[paletteColor];
        }

        private void VerticalBlankScanlines()
        {
            if (_scanline == 241 && _cycles == 1)
            {
                Status.VerticalBlank = true;
                if (Control.TriggerNmi)
                    _nes.TriggerNmi();

                IsIdle = true;
            }
        }

        //private readonly Tile[] _backgroundTiles;

        //public Tile[] BackgroundTiles => _backgroundTiles;

        //private ushort GetNametableBaseAddress()
        //{
        //    throw new NotImplementedException();

        //    //byte nametable = Control.BaseNametableAddress;

        //    //ushort baseAddress;
        //    //switch (nametable)
        //    //{
        //    //    case 0:
        //    //        baseAddress = 0x2000;
        //    //        break;
        //    //    case 1:
        //    //        baseAddress = 0x2400;
        //    //        break;
        //    //    case 2:
        //    //        baseAddress = 0x2800;
        //    //        break;
        //    //    case 3:
        //    //        baseAddress = 0x2C00;
        //    //        break;
        //    //    default:
        //    //        throw new InvalidOperationException($"The given nametable is invalid: {nametable}.");
        //    //}

        //    //return baseAddress;
        //}

        //public byte[][] GetNametable0()
        //{
        //    ushort nametableAddress = GetNametableBaseAddress();

        //    byte[][] nametable = new byte[30][]; // 30 tiles high
        //    for (int i = 0; i < nametable.Length; i++)
        //    {
        //        nametable[i] = new byte[32]; // 32 tiles across
        //        for (int j = 0; j < nametable[i].Length; j++)
        //        {
        //            byte tileIdx = _ppuBus.Read(nametableAddress);
        //            nametable[i][j] = tileIdx;

        //            nametableAddress++;
        //        }
        //    }

        //    return nametable;
        //}

        //public byte[][] GetNametable2()
        //{
        //    ushort nametableAddress = 0x2800;
        //    //ushort finalAddress = (ushort)(nametableAddress + 0x03C0);

        //    byte[][] nametable = new byte[30][]; // 30 tiles high
        //    for (int i = 0; i < nametable.Length; i++)
        //    {
        //        nametable[i] = new byte[32]; // 32 tiles across
        //        for (int j = 0; j < nametable[i].Length; j++)
        //        {
        //            byte tileIdx = _ppuBus.Read(nametableAddress);
        //            nametable[i][j] = tileIdx;

        //            nametableAddress++;
        //        }
        //    }

        //    return nametable;
        //}

        //public Color[] GetPalettes()
        //{
        //    //var palettes = new Color[32];

        //    //for (ushort addr = 0x3F00, idx = 0; addr < 0x3F20; addr++, idx++)
        //    //    //palettes[idx] = SystemColorPalette[_ppuBus.Read(addr)];

        //    throw new NotImplementedException();
        //}
    }
}
