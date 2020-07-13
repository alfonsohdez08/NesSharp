using MiNES.Extensions;
using MiNES.PPU.Registers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

namespace MiNES.PPU
{
    public class Ppu
    {
        private readonly PpuBus _ppuBus;

        private int _fineX;
        private int _cycles = 0;
        private int _scanline = -1;
        private Bitmap _frame;

        /// <summary>
        /// This register is divided into 2 registers: the low byte (right side) correspond the PISO register, which is
        /// used for store the low plane for the next tile that will be rendered. The high byte (left side) correspond to a SIPO register,
        /// which is used for store the low plane of the current tile that will be rendered. PISO is a shift register of type Parallel In - Serial Out, which
        /// means the data is filled and it's output shift by shift (clock by clock). The SIPI is a shift register of type Serial In - Parallel Out, which means
        /// the data is inserted sequentially, and once the data is loaded, it's output at once (this one it's used by the MUX in order to identify which pixel will
        /// be drawn in the current cycle).
        /// </summary>
        private ushort _lowBackgroundShiftRegister;

        /// <summary>
        /// This register is divided into 2 registers: the low byte (right side) correspond the PISO register, which is
        /// used for store the high plane for the next tile that will be rendered. The high byte (left side) correspond to a SIPO register,
        /// which is used for store the high plane of the current tile that will be rendered. PISO is a shift register of type Parallel In - Serial Out, which
        /// means the data is filled and it's output shift by shift (clock by clock). The SIPI is a shift register of type Serial In - Parallel Out, which means
        /// the data is inserted sequentially, and once the data is loaded, it's output at once (this one it's used by the MUX in order to identify which pixel will
        /// be drawn in the current cycle).
        private ushort _highBackgroundShiftRegister;

        private ushort _lowAttributeShiftRegister;
        private ushort _highAttributeShiftRegister;

        private byte _tileId;
        
        private byte _attribute;
        private byte _blockId;

        private byte _lowPixelsRow;
        private byte _highPixelsRow;
        private bool _isRenderingEnabled => Mask.RenderBackground || Mask.RenderSprites;

        /// <summary>
        /// Count how many frames has been rendered.
        /// </summary>
        private byte _framesRendered = 1;

        public Bitmap FrameBuffer { get; private set; }

        /// <summary>
        /// Controls the state of the address latch (false = high byte; true = low byte).
        /// </summary>
        private bool _addressLatch = false;

        /// <summary>
        /// PPU Control register.
        /// </summary>
        internal Control ControlRegister { get; private set; } = new Control();

        /// <summary>
        /// PPU Mask register.
        /// </summary>
        internal Mask Mask { get; private set; } = new Mask();

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
        internal Status StatusRegister { get; set; } = new Status();

        /// <summary>
        /// Object Attribute Memory (OAM) address register.
        /// </summary>
        public byte OamAddress { get; set; }

        /// <summary>
        /// The PPU OAM (it's 256 bytes long, capable of store 64 sprites, sprite data is 4 bytes long).
        /// </summary>
        private byte[] _oam = new byte[256];

        private readonly byte[] _oamBuffer = new byte[32];

        public byte OamCpuPage { get; set; }
        public bool DmaTriggered { get; set; }

        public void SetOam(byte[] oam)
        {
            _oam = oam;
        }

        public void SetOamData(byte data)
        {
            //// Only writes to OAM Data port when rendering is disabled
            //if (!_isRenderingEnabled)
            //{
            //    /*  When Oam Address is divisible by 4, it means we attempting to write the position of the sprite in Y axis, so we must substract 1 from it because
            //     *  delayed scanline.
            //     */
            //    //int val = OamAddress % 4 == 0 ? data - 1 : data;
            //    //_oam[OamAddress] = (byte)val;
            //    _oam[OamAddress] = data;

            //    // OAM address gets incremented by one when data is written
            //    OamAddress++;
            //}

            // TODO: confirm this: writes to OAM data port occurs when rendering is disabled

            /*  When Oam Address is divisible by 4, it means we attempting to write the position of the sprite in Y axis, so we must substract 1 from it because
             *  delayed scanline.
             */
            //int val = OamAddress % 4 == 0 ? data - 1 : data;
            //_oam[OamAddress] = (byte)val;
            _oam[OamAddress] = data;

            // OAM address gets incremented by one when data is written
            OamAddress++;
        }

        public byte GetOamData()
        {
            // TODO: does a read during pre render and render scanlines increment the oam adddress?

            return _oam[OamAddress];
        }

        private byte _dataBuffer = 0;

        #region NES color palette
        public static readonly Color[] SystemColorPalette = new Color[]
        {
            Color.FromArgb(0x75, 0x75, 0x75),
            Color.FromArgb(0x27, 0x1B, 0x8F),
            Color.FromArgb(0x00, 0x00, 0xAB),
            Color.FromArgb(0x47, 0x00, 0x9F),
            Color.FromArgb(0x8F, 0x00, 0x77),
            Color.FromArgb(0xAB, 0x00, 0x13),
            Color.FromArgb(0xA7, 0x00, 0x00),
            Color.FromArgb(0x7F, 0x0B, 0x00),
            Color.FromArgb(0x43, 0x2F, 0x00),
            Color.FromArgb(0x00, 0x47, 0x00),
            Color.FromArgb(0x00, 0x51, 0x00),
            Color.FromArgb(0x00, 0x3F, 0x17),
            Color.FromArgb(0x1B, 0x3F, 0x5F),
            Color.FromArgb(0x00, 0x00, 0x00),
            Color.FromArgb(0x00, 0x00, 0x00),
            Color.FromArgb(0x00, 0x00, 0x00),
            Color.FromArgb(0xBC, 0xBC, 0xBC),
            Color.FromArgb(0x00, 0x73, 0xEF),
            Color.FromArgb(0x23, 0x3B, 0xEF),
            Color.FromArgb(0x83, 0x00, 0xF3),
            Color.FromArgb(0xBF, 0x00, 0xBF),
            Color.FromArgb(0xE7, 0x00, 0x5B),
            Color.FromArgb(0xDB, 0x2B, 0x00),
            Color.FromArgb(0xCB, 0x4F, 0x0F),
            Color.FromArgb(0x8B, 0x73, 0x00),
            Color.FromArgb(0x00, 0x97, 0x00),
            Color.FromArgb(0x00, 0xAB, 0x00),
            Color.FromArgb(0x00, 0x93, 0x3B),
            Color.FromArgb(0x00, 0x83, 0x8B),
            Color.FromArgb(0x00, 0x00, 0x00),
            Color.FromArgb(0x00, 0x00, 0x00),
            Color.FromArgb(0x00, 0x00, 0x00),
            Color.FromArgb(0xFF, 0xFF, 0xFF),
            Color.FromArgb(0x3F, 0xBF, 0xFF),
            Color.FromArgb(0x5F, 0x97, 0xFF),
            Color.FromArgb(0xA7, 0x8B, 0xFD),
            Color.FromArgb(0xF7, 0x7B, 0xFF),
            Color.FromArgb(0xFF, 0x77, 0xB7),
            Color.FromArgb(0xFF, 0x77, 0x63),
            Color.FromArgb(0xFF, 0x9B, 0x3B),
            Color.FromArgb(0xF3, 0xBF, 0x3F),
            Color.FromArgb(0x83, 0xD3, 0x13),
            Color.FromArgb(0x4F, 0xDF, 0x4B),
            Color.FromArgb(0x58, 0xF8, 0x98),
            Color.FromArgb(0x00, 0xEB, 0xDB),
            Color.FromArgb(0x00, 0x00, 0x00),
            Color.FromArgb(0x00, 0x00, 0x00),
            Color.FromArgb(0x00, 0x00, 0x00),
            Color.FromArgb(0xFF, 0xFF, 0xFF),
            Color.FromArgb(0xAB, 0xE7, 0xFF),
            Color.FromArgb(0xC7, 0xD7, 0xFF),
            Color.FromArgb(0xD7, 0xCB, 0xFF),
            Color.FromArgb(0xFF, 0xC7, 0xFF),
            Color.FromArgb(0xFF, 0xC7, 0xDB),
            Color.FromArgb(0xFF, 0xBF, 0xB3),
            Color.FromArgb(0xFF, 0xDB, 0xAB),
            Color.FromArgb(0xFF, 0xE7, 0xA3),
            Color.FromArgb(0xE3, 0xFF, 0xA3),
            Color.FromArgb(0xAB, 0xF3, 0xBF),
            Color.FromArgb(0xB3, 0xFF, 0xCF),
            Color.FromArgb(0x9F, 0xFF, 0xF3),
            Color.FromArgb(0x00, 0x00, 0x00),
            Color.FromArgb(0x00, 0x00, 0x00),
            Color.FromArgb(0x00, 0x00, 0x00)
        };
        #endregion

        public bool IsFrameCompleted { get; private set; }

        public bool NmiRequested { get; set; }

        private readonly Tile[] _backgroundTiles;

        public Tile[] BackgroundTiles => _backgroundTiles;

        internal LoopyRegister V { get; private set; } = new LoopyRegister();
        internal LoopyRegister T { get; private set; } = new LoopyRegister();

        public Ppu(PpuBus ppuBus)
        {
            _ppuBus = ppuBus;

            _backgroundTiles = GetPatternTable();
            ResetFrame();
        }

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
        public void SetAddress(byte value)
        {
            if (!_addressLatch) // w is 0
            {
                value = (byte)((value | 0xC0) ^ 0xC0);

                T.Value = (ushort)(((T.Value | 0x3F00) ^ 0x3F00) | (value << 8));
                T.Value = (ushort)((T.Value | 0x4000) ^ 0x4000); // Sets bit 14 to 0

                _addressLatch = true; // Flips to the low byte state
            }
            else // w is 1
            {
                T.Value = (ushort)(((T.Value | 0xFF) ^ 0xFF) | value);
                V.Value = T.Value;

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
                T.CoarseX = (byte)(value >> 3); 
                _fineX = (byte)(value & 7);

                _addressLatch = true; // Flips to the low byte state
            }
            else // w is 1
            {
                T.CoarseY = (byte)(value >> 3);
                T.FineY = (byte)(value & 7);

                _addressLatch = false; // Flips to the high byte state
            }
        }

        private Tile[] GetPatternTable(bool isBackgroundTile = true)
        {
            var patternTable = new Tile[256];

            ushort address = 0x0000;
            ushort lastAddress = 0x1000;
            if (isBackgroundTile)
            {
                address = 0x1000;
                lastAddress = 0x2000;
            }

            int tiles = 0;
            for (; address < lastAddress; address += 16)
            {
                ushort tileAddress = address;

                //Bitmap tileBitmap = new Bitmap(8, 8);
                var tile = new Tile();

                // Process an entire tile (row by row, where each row represents a string of pixels)
                for (int i = 0; i < 8; i++)
                {
                    byte lowBitsRow = _ppuBus.Read(tileAddress);
                    byte highBitsRow = _ppuBus.Read((ushort)(tileAddress + 8)); // high bit plane offset is 8 positions away

                    // Iterate over each bit within both set of bits (bytes) for draw the tile bitmap
                    for (int j = 0; j < 8; j++)
                    {
                        int mask = 1 << j;
                        int lowBit = (lowBitsRow & mask) == mask ? 1 : 0;
                        int highBit = (highBitsRow & mask) == mask ? 1 : 0;

                        // A 2 bit value
                        byte paletteColorIdx = (byte)(lowBit | highBit << 1);

                        //tile.SetPixel(i, j, paletteColorIdx);
                        tile.SetPixel(7 - j, i, paletteColorIdx);
                    }

                    tileAddress++;
                }

                patternTable[tiles] = tile;
                tiles++;
            }

            return patternTable;
        }

        /// <summary>
        /// Retrieves the data from the address set through the PPU address register.
        /// </summary>
        /// <returns>The data allocated in the address set through the PPU address register.</returns>
        public byte GetPpuData()
        {
            // Reads the data buffered (from previous read request)
            byte data = _dataBuffer;

            // Updates the buffer with the data allocated in the compiled address
            _dataBuffer = _ppuBus.Read(V.Address);

            /* If the compiled address does not overlap the color palette address range, then return
             * the data read from the buffer; otherwise return the data read from the address right away
             */
            if (V.Value >= 0x3F00)
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

        /// <summary>
        /// Increments the compiled address that points to a VRAM location.
        /// </summary>
        private void IncrementVRamAddress()
        {
            // If bit 3 from control register is set, add 32 to VRAM address; otherwise 1
            if (ControlRegister.VRamAddressIncrement)
                V.Value += 32;
            else
                V.Value++;
        }

        private byte _oamLatch;
        private byte _oamBufferIndex;
        private byte _spritesInBuffer;

        private byte[] _spriteXCounters = new byte[8];
        private byte[] _spriteAttributes = new byte[8];
        private byte[] _spriteLowPlaneTiles = new byte[8];
        private byte[] _spriteHighPlaneTiles = new byte[8];

        private byte _spriteBufferIndex = 0;
        private byte _spriteY;
        private byte _spriteTileIndex;
        private bool _flipSpriteVertically;
        private bool _flipSpriteHorizontally;
        private bool _emptySprite;

        public void DrawPixel()
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
                    PreRenderScanline();
                else
                    RenderScanlines();

                if (_cycles >= 1 && _cycles <= 257)
                    ShiftSpriteRegisters();

                // Background rendering process
                if ((_cycles >= 1 && _cycles <= 256) || (_cycles >= 321 && _cycles <= 336))
                {
                    var stage = _cycles % 8;
                    switch (stage)
                    {
                        // Load high background pattern table byte
                        case 0:
                            {
                                ushort patternTableAddress = (ushort)(ControlRegister.BackgroundPatternTableAddress ? 0x1000 : 0);
                                ushort pixelsRowAddress = (ushort)(patternTableAddress + (_tileId * 16) + V.FineY + 8);

                                _highPixelsRow = _ppuBus.Read(pixelsRowAddress);
                            }
                            break;
                        case 1:
                            // Load shift registers
                                LoadBackgroundShiftRegisters();
                            break;
                        // Fetch nametable byte
                        case 2:
                            {
                                ushort tileIdAddress = (ushort)(0x2000 | (V.Value & 0x0FFF));
                                _tileId = _ppuBus.Read(tileIdAddress);
                            }
                            break;
                        // Fetch attribute table byte
                        case 4:
                            {
                                ushort attributeEntryAddress = (ushort)(0x23C0 | (V.Value & 0x0C00) | ((V.Value >> 4) & 0x38) | ((V.Value >> 2) & 0x07));
                                
                                _attribute = _ppuBus.Read(attributeEntryAddress);
                                _blockId = ParseBlock(V.CoarseX, V.CoarseY);
                            }
                            break;
                        // Fetch low background pattern table byte
                        case 6:
                            {
                                ushort patternTableAddress = (ushort)(ControlRegister.BackgroundPatternTableAddress ? 0x1000 : 0);
                                ushort pixelsRowAddress = (ushort)(patternTableAddress + (_tileId * 16) + V.FineY);

                                _lowPixelsRow = _ppuBus.Read(pixelsRowAddress);
                            }
                            break;
                    }
                }

                // Sprite Evaluation
                if (_scanline >= 0)
                {
                    // Initializes to $FF the OAM buffer
                    if (_cycles >= 1 && _cycles <= 64)
                    {
                        _oamBuffer[_cycles % 32] = 0xFF;
                    }
                    else if (_cycles > 64 && _cycles <= 256)
                    {
                        // Read mode (odd cycles)
                        if (_cycles % 2 != 0)
                        {
                            _oamLatch = _oam[OamAddress];
                        }
                        // Write mode (even cycles)
                        else
                        {
                            var currentSpriteYPos = _oam[(OamAddress / 4) * 4];
                            bool isSpriteInRange = IsSpriteInRange(currentSpriteYPos);

                            if (_spritesInBuffer < 8)
                            {
                                _oamBuffer[_oamBufferIndex] = _oamLatch;
                            }
                            else if (!StatusRegister.SpriteOverflow)
                            {
                                if (isSpriteInRange)
                                    StatusRegister.SpriteOverflow = true;
                            }

                            if (isSpriteInRange && !StatusRegister.SpriteOverflow)
                            {
                                OamAddress++;
                                _oamBufferIndex++;

                                _spritesInBuffer = (byte)(_oamBufferIndex / 4);
                            }
                            else
                            {
                                OamAddress += 4;
                            }

                            // Workaround for set empty sprite slots to $FF
                            if (_cycles == 256)
                                for (int i = _spritesInBuffer * 4; i < _oamBuffer.Length; i++)
                                    _oamBuffer[i] = 0xFF;
                        }
                    }else if (_cycles >= 257 && _cycles <= 320)
                    {
                        var stage = (_cycles - 1) % 8;
                        switch (stage)
                        {
                            // Parse the Y coordinate
                            case 0:
                                _spriteY = (byte)(_scanline - _oamBuffer[_spriteBufferIndex * 4]);
                                _emptySprite = _oamBuffer[_spriteBufferIndex * 4] == 0xFF;
                                break;
                            case 1:
                                _spriteTileIndex = _oamBuffer[(_spriteBufferIndex * 4) + 1];
                                break;
                            // Parse attributes (palette, flip horizontally, flip vertically, priority)
                            case 2:
                                {
                                    byte attributes = _oamBuffer[(_spriteBufferIndex * 4) + 2];

                                    _spriteAttributes[_spriteBufferIndex] = attributes;

                                    _flipSpriteHorizontally = attributes.GetBit(6);
                                    _flipSpriteVertically = attributes.GetBit(7);
                                }
                                break;
                            case 3:
                                _spriteXCounters[_spriteBufferIndex] = _oamBuffer[(_spriteBufferIndex * 4) + 3];
                                break;
                            case 5:
                                {
                                    // fetch sprite low tile
                                    byte lowPlane = 0;

                                    if (!_emptySprite)
                                    {
                                        // 8 x 16 sprites
                                        if (ControlRegister.SpriteSize)
                                        {
                                            ushort patternTableAddress = (ushort)(_spriteTileIndex.GetBit(0) ? 0x1000 : 0);
                                            byte spriteId = (byte)(_spriteTileIndex & 0xFE);
                                            var y = _flipSpriteVertically ? (15 - _spriteY) : _spriteY;

                                            // top half
                                            if (y < 8)
                                            {
                                                lowPlane = _ppuBus.Read((ushort)(patternTableAddress + (spriteId * 16) + y));

                                            } // bottom half
                                            else
                                            {
                                                //spriteId++; // bottom half tile is next to the top half tile in the pattern table
                                                lowPlane = _ppuBus.Read((ushort)(patternTableAddress + ((spriteId + 1) * 16) + (y - 8)));
                                            }

                                        } // 8 x 8 sprites
                                        else
                                        {
                                            ushort patternTableAddress = (ushort)(ControlRegister.SpritesPatternTableAddress ? 0x1000 : 0);
                                            var flipOffset = _flipSpriteVertically ? (7 - _spriteY) : _spriteY;

                                            lowPlane = _ppuBus.Read((ushort)(patternTableAddress + (_spriteTileIndex * 16) + flipOffset));
                                        }

                                        if (_flipSpriteHorizontally)
                                        {
                                            lowPlane = Flip(lowPlane);

                                            //lowPlane.MirrorBits();
                                        }
                                    }

                                    _spriteLowPlaneTiles[_spriteBufferIndex] = lowPlane;
                                }
                                break;
                            case 7:
                                {
                                    // fetch sprite high tile
                                    byte highPlane = 0;

                                    if (!_emptySprite)
                                    {
                                        // 8 x 16 sprites
                                        if (ControlRegister.SpriteSize)
                                        {
                                            ushort patternTableAddress = (ushort)(_spriteTileIndex.GetBit(0) ? 0x1000 : 0);
                                            byte spriteId = (byte)(_spriteTileIndex & 0xFE);
                                            var y = _flipSpriteVertically ? (15 - _spriteY) : _spriteY;

                                            // top half
                                            if (y < 8)
                                            {
                                                highPlane = _ppuBus.Read((ushort)(patternTableAddress + (spriteId * 16) + y + 8));

                                            } // bottom half
                                            else
                                            {
                                                //spriteId++; // bottom half tile is next to the top half tile in the pattern table
                                                highPlane = _ppuBus.Read((ushort)(patternTableAddress + ((spriteId + 1) * 16) + (y - 8) + 8));
                                            }

                                        } // 8 x 8 sprites
                                        else
                                        {
                                            ushort patternTableAddress = (ushort)(ControlRegister.SpritesPatternTableAddress ? 0x1000 : 0);
                                            var flipOffset = _flipSpriteVertically ? (7 - _spriteY) : _spriteY;

                                            highPlane = _ppuBus.Read((ushort)(patternTableAddress + (_spriteTileIndex * 16) + flipOffset + 8));
                                        }

                                        if (_flipSpriteHorizontally)
                                        {
                                            highPlane = Flip(highPlane);

                                            //highPlane.MirrorBits();
                                        }
                                    }

                                    _spriteHighPlaneTiles[_spriteBufferIndex] = highPlane;

                                    _spriteBufferIndex++;
                                }
                                break;
                        }
                    }
                }

                // Increments the horizontal component in the V register
                if (_isRenderingEnabled && ((_cycles >= 1 && _cycles <= 256) || (_cycles >= 328 && _cycles <= 336)) && _cycles % 8 == 0)
                    IncrementHorizontalPosition();

                // Increments the vertical component in the V register
                if (_isRenderingEnabled && _cycles == 256)
                    IncrementVerticalPosition();

                // Copy the horizontal component from T register into V register
                if (_isRenderingEnabled && _cycles == 257)
                    CopyHorizontalPositionToV();

                // During cycles elapesed between 257 and 320 (inclusive), the OAM address is set to 0
                if (_cycles >= 257 && _cycles <= 320)
                    OamAddress = 0;
            }
            else if (_scanline == 240)
                PostRenderScanlines();
            else if (_scanline >= 241 && _scanline < 261)
                VerticalBlankScanlines();

            _cycles++;
            if (_cycles >= 341)
            {
                _cycles = 0;
                
                _oamBufferIndex = 0;
                _spritesInBuffer = 0;
                _spriteBufferIndex = 0;

                if (_scanline < 260)
                {
                    _scanline++;
                }
                else
                {
                    _scanline = -1;
                    _framesRendered++;
                    //if (_framesRendered > 60)
                    //    _framesRendered = 1;

                    IsFrameCompleted = true;
                    FrameBuffer = _frame;
                    ResetFrame();
                }
            }

        }

        private static byte Flip(byte _b)
        {
            int b = _b;

            b = (b & 0xF0) >> 4 | (b & 0x0F) << 4;
            b = (b & 0xCC) >> 2 | (b & 0x33) << 2;
            b = (b & 0xAA) >> 1 | (b & 0x55) << 1;

            return (byte)b;
        }

        /// <summary>
        /// Checks if the sprite is in the Y-axis range.
        /// </summary>
        /// <param name="y">The top position of the sprite in the Y-axis.</param>
        /// <returns>True if it is in the range, otherwise false.</returns>
        private bool IsSpriteInRange(byte y)
        {
            int spriteHeight = ControlRegister.SpriteSize ? 16 : 8;

            return _scanline >= y && _scanline < (y + spriteHeight);
        }

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
                for (int i = 0; i < _spriteXCounters.Length; i++)
                {
                    if (_spriteXCounters[i] > 0)
                    {
                        _spriteXCounters[i]--;
                    }
                    else
                    {
                        _spriteLowPlaneTiles[i] <<= 1;
                        _spriteHighPlaneTiles[i] <<= 1;
                    }
                }
            }
        }

        private void LoadBackgroundShiftRegisters()
        {
            /*Note: When jumping to next scanline, in the first cycle i reload the low byte of all registers
             * with the data that i already had in the latches from the last "useful" 8 cycles of previous scanline (remember that
             * at the ending of each scanline, we load the shift registers with the data of the the first 2 tiles of the next scanline).
             */

            // Load background shift registers
            _lowBackgroundShiftRegister = (ushort)(((_lowBackgroundShiftRegister | 0x00FF) ^ 0x00FF) | _lowPixelsRow);
            _highBackgroundShiftRegister = (ushort)(((_highBackgroundShiftRegister | 0x00FF) ^ 0x00FF) | _highPixelsRow);

            // Load attribute shift registers
            byte palette = ParsePalette(_attribute, _blockId); // 2 bit value

            // The same bit is propagated to all bits in the attribute shift register
            bool lowBit = palette.GetBit(0);
            if (lowBit)
                _lowAttributeShiftRegister = (ushort)(((_lowAttributeShiftRegister | 0x00FF) ^ 0x00FF) | 0xFF);
            else
                _lowAttributeShiftRegister = (ushort)(((_lowAttributeShiftRegister | 0x00FF) ^ 0x00FF));

            bool highBit = palette.GetBit(1);
            if (highBit)
                _highAttributeShiftRegister = (ushort)(((_highAttributeShiftRegister | 0x00FF) ^ 0x00FF) | 0xFF);
            else
                _highAttributeShiftRegister = (ushort)(((_highAttributeShiftRegister | 0x00FF) ^ 0x00FF));
        }

        private static byte ParseBlock(int coarseX, int coarseY)
        {
            var x = coarseX % 4;
            var y = coarseY % 4;

            byte blockId;
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
        /// Copy the bits related to horizontal position from T register to transfer into V register.
        /// </summary>
        private void CopyHorizontalPositionToV()
        {
            V.CoarseX = T.CoarseX;
            V.Value = (ushort)(((V.Value | 0x0400) ^ 0x0400) | (T.Value & 0x0400)); // copy bit 10 (nametable x) from T register
        }

        /// <summary>
        /// Copy the bits related to vertical position from T register to transfer into V register.
        /// </summary>
        private void CopyVerticalPositionToV()
        {
            V.CoarseY = T.CoarseY;
            V.FineY = T.FineY;

            V.Value = (ushort)(((V.Value | 0x0800) ^ 0x0800) | (T.Value & 0x0800)); // copy bit 11 (nametable y) from T register
        }

        /// <summary>
        /// Increments the vertical position in the V register (vertical positions are denoted by the bits 5-9 which denotes the coarse Y scroll, and the bits 12-14
        /// which denotes the pixels offset in the y axis within a tile: fine y).
        /// </summary>
        private void IncrementVerticalPosition()
        {
            byte fineY = (byte)((V.Value & 0x7000) >> 12);
            if (fineY < 7)
            {
                fineY++;
            }
            else
            {
                fineY = 0;

                // Increments coarse Y then
                byte coarseY = (byte)((V.Value & 0x03E0) >> 5);
                if (coarseY == 29)
                {
                    coarseY = 0;

                    byte yNametable = (byte)((V.Value & 0x0800) >> 11);
                    yNametable = (byte)~yNametable; // by toggling bit 11, we switch vertical nametable

                    V.Value = (ushort)(((V.Value | 0x0800) ^ 0x0800) | ((yNametable & 1) << 11));
                }
                else if (coarseY == 31)
                {
                    coarseY = 0;
                }
                else
                {
                    coarseY++;
                }

                V.Value = (ushort)(((V.Value | 0x03E0) ^ 0x03E0) | (coarseY << 5)); // Put coarse Y into the V register
            }

            V.Value = (ushort)(((V.Value | 0x7000) ^ 0x7000) | (fineY << 12)); // Put fineY into the V register
        }

        private void IncrementHorizontalPosition()
        {
            byte coarseX = (byte)(V.Value & 0x001F);
            if (coarseX == 31)
            {
                coarseX = 0;

                byte xNametable = (byte)((V.Value & 0x0400) >> 10);
                xNametable = (byte)~xNametable; // by toggling bit 10, we switch horizontal nametable

                V.Value = (ushort)(((V.Value | 0x0400) ^ 0x0400) | ((xNametable & 1) << 10));
            }
            else
            {
                coarseX++;
            }

            V.Value = (ushort)(((V.Value | 0x001F) ^ 0x001F) | coarseX);
        }

        private void PreRenderScanline()
        {
            if (_cycles == 1)
            {
                StatusRegister.SpriteOverflow = false;
                StatusRegister.SpriteZeroHit = false;
                StatusRegister.VerticalBlank = false;

                for (int i = 0; i < _spriteHighPlaneTiles.Length; i++)
                {
                    _spriteHighPlaneTiles[i] = 0;
                    _spriteLowPlaneTiles[i] = 0;
                }
            }
            else if (_isRenderingEnabled && _cycles >= 280 && _cycles <= 304)
            {
                CopyVerticalPositionToV();
            }
        }

        private void RenderScanlines()
        {
            if (_cycles < 1 || _cycles > 256)
                return;
            
            // When a pixel equals 0, it means the pixel is transparent. When pixel is different than 0, then pixel is opaque
            byte backgroundPixel = 0;
            byte backgroundPalette = 0;

            if (Mask.RenderBackground)
            {
                ushort pixelMux = (ushort)(0x8000 >> _fineX);
                var lsbPixelColorIdx = (_lowBackgroundShiftRegister & pixelMux) == pixelMux ? 1 : 0;
                var msbPixelColorIdx = (_highBackgroundShiftRegister & pixelMux) == pixelMux ? 1 : 0;

                backgroundPixel = (byte)(lsbPixelColorIdx | (msbPixelColorIdx << 1));

                var lsbPaletteIdx = (_lowAttributeShiftRegister & pixelMux) == pixelMux ? 1 : 0;
                var msbPaletteIdx = (_highAttributeShiftRegister & pixelMux) == pixelMux ? 1 : 0;

                backgroundPalette = (byte)((lsbPaletteIdx) | (msbPaletteIdx << 1));
            }

            byte spritePixel = 0;
            byte spritePalette = 0;
            bool spritePriority = false;

            if (Mask.RenderSprites)
            {
                for (int i = 0; i < _spriteXCounters.Length; i++)
                {
                    // If counter equals to 0, then the sprite became active
                    if (_spriteXCounters[i] == 0)
                    {
                        var pixelLowBit = (_spriteLowPlaneTiles[i] & 0x80) == 0x80 ? 1 : 0;
                        var pixelHighBit = (_spriteHighPlaneTiles[i] & 0x80) == 0x80 ? 1 : 0;

                        spritePixel = (byte)((pixelLowBit) | (pixelHighBit << 1));
                        spritePalette = (byte)((_spriteAttributes[i] & 3) + 4); // Add 4 because the sprite palettes are from 4-7
                        spritePriority = (_spriteAttributes[i] & 0x20) == 0x20;

                        // Sprite pixel is opaque
                        if (spritePixel != 0)
                        {
                            //// When the pixel of the sprite 0 is opaque, we must set the flag of sprite zero hit
                            if (i == 0 && backgroundPixel != 0)
                                StatusRegister.SpriteZeroHit = true;

                            break;
                        }
                    }
                }
            }

            byte pixel = 0;
            byte palette = 0;

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

                // TODO: see how to deal with sprite zero hit flag
            }

            _frame.SetPixel(_cycles - 1, _scanline, GetPaletteColor(palette, pixel));
        }


        public void DisposeBuffer()
        {
            FrameBuffer = null;
        }

        public void ResetFrame()
        {
            _frame = new Bitmap(256, 240);
            //IsFrameCompleted = false;
        }

        private ushort GetNametableBaseAddress()
        {
            byte nametable = ControlRegister.BaseNametableAddress;

            ushort baseAddress;
            switch (nametable)
            {
                case 0:
                    baseAddress = 0x2000;
                    break;
                case 1:
                    baseAddress = 0x2400;
                    break;
                case 2:
                    baseAddress = 0x2800;
                    break;
                case 3:
                    baseAddress = 0x2C00;
                    break;
                default:
                    throw new InvalidOperationException($"The given nametable is invalid: {nametable}.");
            }

            return baseAddress;
        }

        private static byte ParsePalette(byte attribute, byte blockId)
        {
            byte palette;
            int lowBit;
            int highBit;

            switch (blockId)
            {
                case 0: // Top left
                    //palette = (byte)(attribute & (0b00000011));
                    highBit = (attribute & 0b00000010) == 0b00000010 ? 1 : 0;
                    lowBit = (attribute & 0b00000001) == 0b00000001 ? 1 : 0;
                    break;
                case 1: // Top right
                    //palette = (byte)(attribute & (0b00001100));
                    highBit = (attribute & 0b00001000) == 0b00001000 ? 1 : 0;
                    lowBit = (attribute & 0b00000100) == 0b00000100 ? 1 : 0;
                    break;
                case 2: // Bottom left
                    //palette = (byte)(attribute & (0b00110000));
                    highBit = (attribute & 0b00100000) == 0b00100000 ? 1 : 0;
                    lowBit = (attribute & 0b00010000) == 0b00010000 ? 1 : 0;
                    break;
                case 3: // Bottom right
                    //palette = (byte)(attribute & (0b11000000));
                    highBit = (attribute & 0b10000000) == 0b10000000 ? 1 : 0;
                    lowBit = (attribute & 0b01000000) == 0b01000000 ? 1 : 0;
                    break;
                default:
                    throw new InvalidOperationException($"The given block ID is invalid: ${blockId}.");
            }

            palette = (byte)(lowBit | highBit << 1);

            return palette;
        }

        /// <summary>
        /// Retrieves the color given the palette and its color entry.
        /// </summary>
        /// <param name="palette">The color palette (0-7).</param>
        /// <param name="colorIndex">The color index (0-4, where 0 means transparent color: background color).</param>
        /// <returns></returns>
        private Color GetPaletteColor(byte palette, byte colorIndex)
        {
            //byte paletteColor = _ppuBus.Read((ushort)(0x3F00 + (palette << 2) + colorIndex));
            ushort paletteColorAddress = ParseBackgroundPaletteAddress(palette, colorIndex);
            byte paletteColor = _ppuBus.Read(paletteColorAddress);
            if (paletteColor < 0 || paletteColor > SystemColorPalette.Length)
                throw new InvalidOperationException($"The given palette color does not exist: {paletteColor}.");

            return SystemColorPalette[paletteColor];
        }

        private static ushort ParseBackgroundPaletteAddress(byte paletteId, byte colorIndex) => (ushort)(0x3F00 + paletteId * 4 + colorIndex);

        private void PostRenderScanlines()
        {
            // Do nothing
        }

        private void VerticalBlankScanlines()
        {
            if (_scanline == 241 && _cycles == 1)
            {
                StatusRegister.VerticalBlank = true;
                if (ControlRegister.GenerateNMI)
                    NmiRequested = true;
            }
        }

        public byte[][] GetNametable0()
        {
            ushort nametableAddress = GetNametableBaseAddress();

            byte[][] nametable = new byte[30][]; // 30 tiles high
            for (int i = 0; i < nametable.Length; i++)
            {
                nametable[i] = new byte[32]; // 32 tiles across
                for (int j = 0; j < nametable[i].Length; j++)
                {
                    byte tileIdx = _ppuBus.Read(nametableAddress);
                    nametable[i][j] = tileIdx;

                    nametableAddress++;
                }
            }

            return nametable;
        }

        public byte[][] GetNametable2()
        {
            ushort nametableAddress = 0x2800;
            //ushort finalAddress = (ushort)(nametableAddress + 0x03C0);

            byte[][] nametable = new byte[30][]; // 30 tiles high
            for (int i = 0; i < nametable.Length; i++)
            {
                nametable[i] = new byte[32]; // 32 tiles across
                for (int j = 0; j < nametable[i].Length; j++)
                {
                    byte tileIdx = _ppuBus.Read(nametableAddress);
                    nametable[i][j] = tileIdx;

                    nametableAddress++;
                }
            }

            return nametable;
        }

        public Color[] GetPalettes()
        {
            var palettes = new Color[32];

            for (ushort addr = 0x3F00, idx = 0; addr < 0x3F20; addr++, idx++)
                palettes[idx] = SystemColorPalette[_ppuBus.Read(addr)];

            return palettes;
        }
    }
}
