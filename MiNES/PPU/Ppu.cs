using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

namespace MiNES.PPU
{

    internal class PpuControl: Register<byte>
    {
        public PpuControl()
        {

        }


        public bool GetNmi() => (GetValue() & 0x80) == 0x80;

        public void SetNmi(bool val)
        {
            byte reg = GetValue();

            Ppu.Bit(7, val, ref reg);
            SetValue(reg);
        }

        public byte GetPatternTableAddress() => (byte)((GetValue() & 0x10) == 0x10 ? 1 : 0);

        public void SetPatterTableAddressBit(bool val)
        {
            byte reg = GetValue();

            Ppu.Bit(4, val, ref reg);
            SetValue(reg);
        }

        public bool GetVRamAddressIncrement() => (GetValue() & 0x04) == 0x04;

        public void SetVramAddressIncrement(bool val)
        {
            byte reg = GetValue();

            Ppu.Bit(2, val, ref reg);
            SetValue(reg);
        }

        public byte GetNametableAddress() => (byte)(GetValue() & 0x03);

        public void SetNametableAddress(byte val)
        {
            SetValue((byte)((GetValue() >> 2) | val));
        }
    }

    internal class PpuMask : Register<byte>
    {

    }

    internal class PpuStatus: Register<byte>
    {
        public PpuStatus(): base(0xA0)
        {

        }


        public bool GetVerticalBlank() => (GetValue() & 0x80) == 0x80;

        public void SetVerticalBlank(bool val)
        {
            byte reg = GetValue();

            Ppu.Bit(7, val, ref reg);
            SetValue(reg);
        }
    }


    public class Ppu
    {
        private readonly PpuBus _ppuBus;

        /// <summary>
        /// Controls the state of the address latch (false = high byte; true = low byte).
        /// </summary>
        private bool _addressLatch = false;

        /// <summary>
        /// PPU Control register.
        /// </summary>
        internal PpuControl Control { get; set; } = new PpuControl();

        /// <summary>
        /// PPU Mask register.
        /// </summary>
        public byte Mask { get; set; }

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
        internal PpuStatus Status { get; set; } = new PpuStatus();

        /// <summary>
        /// Object Attribute Memory (OAM) address register.
        /// </summary>
        public byte OamAddress { get; set; }

        /// <summary>
        /// Object Attribute Memory (OAM) data register.
        /// </summary>
        public byte OamData { get; set; }

        /// <summary>
        /// PPU Scroll register.
        /// </summary>
        public byte Scroll { get; set; }


        /// <summary>
        /// The compiled address from the PPU Address register (this is known as Video RAM address or VRAM).
        /// </summary>
        private ushort _address = 0;


        /// <summary>
        /// Sets the address into the PPU address register.
        /// </summary>
        /// <param name="value">The value of the address (either high or low byte, depending on the latch).</param>
        public void SetAddress(byte value)
        {
            if (!_addressLatch)
            {
                _address = (ushort)(((_address | 0xFF00) ^ 0xFF00) | (value << 8));
                _addressLatch = true; // Flips to the low byte state
            }
            else
            {
                _address = (ushort)(((_address | 0x00FF) ^ 0x00FF) | value);
            }
        }


        private byte _dataBuffer = 0;

        ///// <summary>
        ///// PPU Data register.
        ///// </summary>
        //public byte Data
        //{
        //    get
        //    {
        //        // Reads the data buffered (from previous read request)
        //        byte data = _dataBuffer;

        //        // Updates the buffer with the data allocated in the compiled address
        //        _dataBuffer = _ppuBus.Read(_address);

        //        /* If the compiled address does not overlap the color palette address range, then return
        //         * the data read from the buffer; otherwise return the data read from the address right away
        //         */
        //        return _address >= 0x0000 && _address < 0x3F00 ? data : _dataBuffer;
        //    }
        //    set
        //    {
        //        _ppuBus.Write(_address, value);
        //    }
        //}


        /// <summary>
        /// The OAM DMA page.
        /// </summary>
        public byte OamDmaPage { get; set; }

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

        public Ppu(PpuBus ppuBus)
        {
            _ppuBus = ppuBus;

            _backgroundTiles = GetPatternTable();
            ResetFrame();
        }

        /// <summary>
        /// Resets the address latch used by the PPU address register and PPU scroll register.
        /// </summary>
        public void ResetAddressLatch() => _addressLatch = false;

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
                        byte paletteColorIdx = (byte)(lowBit | (highBit << 1));

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


        internal static void Bit(byte bitPos, bool value, ref byte register)
        {
            int mask = 1 << bitPos;

            int result;
            if (value) // enable/turn on/set the bit
                result = register | mask;
            else // disable/turn off the bit
                result = ((register | mask) ^ mask); // Just in case the bit still ON

            register = (byte)result;
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
            _dataBuffer = _ppuBus.Read(_address);

            /* If the compiled address does not overlap the color palette address range, then return
             * the data read from the buffer; otherwise return the data read from the address right away
             */
            if (_address >= 0x3F00)
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
            _ppuBus.Write(_address, val);
            IncrementVRamAddress();
        }

        /// <summary>
        /// Increments the compiled address that points to a VRAM location.
        /// </summary>
        private void IncrementVRamAddress()
        {
            // If bit 3 from control register is set, add 32 to VRAM address; otherwise 1
            //if ((Control & 0x0004) == 0x0004)
            if (Control.GetVRamAddressIncrement())
                _address += 32;
            else
                _address++;
        }

        ///// <summary>
        ///// Sets the pattern tables (background and foreground tiles).
        ///// </summary>
        //private void SetPatternTables()
        //{
        //    _patternTables = new Bitmap[2][];

        //    _patternTables[0] = GetBitmapPatternTable(false); // Left side of the pattern table it's for foreground tiles (sprites)
        //    _patternTables[1] = GetBitmapPatternTable(); // Right side of the pattern table it's for the background tiles

        //    Bitmap[] GetBitmapPatternTable(bool backgroundTiles = true)
        //    {
        //        Bitmap[] tilesBitmap = new Bitmap[256];

        //        int tiles = 0;

        //        ushort address = 0x0000;
        //        ushort lastAddress = 0x1000;

        //        if (!backgroundTiles)
        //        {
        //            address = 0x1000;
        //            lastAddress = 0x2000;
        //        }

        //        for (; address < lastAddress; address += 16)
        //        {
        //            Bitmap tileBitmap = new Bitmap(8, 8);

        //            // Process an entire tile (row by row, where each row represents a string of pixels)
        //            for (int i = 0; i < 8; i++)
        //            {
        //                byte lowBitsRow = _ppuBus.Read(address);
        //                byte highBitsRow = _ppuBus.Read((ushort)(address + 0x0008)); // high bit plane is offset 8 positions away

        //                // Iterate over each bit within both set of bits (bytes) for draw the tile bitmap
        //                for (int j = 0; j < 8; j++)
        //                {
        //                    int mask = 1 << j;
        //                    int lowBit = (lowBitsRow & mask) == mask ? 1 : 0;
        //                    int highBit = (highBitsRow & mask) == mask ? 1 : 0;

        //                    // A 2 bit value
        //                    byte paletteColorIdx = (byte)(lowBit | (highBit << 1));

        //                    tileBitmap.SetPixel(i, j, Color.FromArgb(paletteColorIdx));
        //                }
        //            }

        //            tilesBitmap[tiles] = tileBitmap;
        //            tiles++;
        //        }

        //        return tilesBitmap;
        //    }
        //}

        private int _cycles = 0;
        private int _scanline = -1;
        private Bitmap _frame;

        public Bitmap FrameBuffer { get; private set; }


        /// <summary>
        /// Count how many frames has been rendered.
        /// </summary>
        private byte _framesRendered = 1;

        /// <summary>
        /// Draws the frame.
        /// </summary>
        public void Draw()
        {
            /*
             * The PPU has only 2kb of RAM: the nametables. The nametables are changed based on the
             * execution of the program. The pattern tables are allocated in the cartridge memory (CHR-ROM).
             */

            /*
             * There are 240 visible scanlines. Each pixel within each scanline is drawn in each clock cycle.
             * For draw a scanline (background purpose), this is the process:
             * 1. Identify the tile currently working on: looking up the tile index from the name table (the _address field points to the nametables: be careful, it must
             * check the PPUCNTRL register to see which nametable i should look up... mirroring).
             * 2. Once identify which is the tile that I should be work on in the next 8 cycles (1 cycle = 1 px); determine its color index within the color pallete by
             * looking up the tile from the pattern table (this is again is controlled by some register which would tell me which pattern table to look).
             * 3. Once the color index is identified, just take it from the color palette assigned to the block (2 x 2 tiles region).
             * 4. Draw the pixel in the bitmap object.
             * 
             * Very important: you must track the X and Y coordinates for draw the pixel and know when to go the next tile; when a scanline is done, must go to the next
             * row of pixels (next scanline). Also, be aware that you need 8 scalines for write a full row of 32 tiles.
             * 
             * Fine x: each pixel in X axis within a tile (from 0 to 7)
             * Fine Y: each pixel in Y axis within a tile (from 0 to 7)... for each scanline, this gets incremented
             * Coarse X: tiles processed horizontally (within a scanline)
             * Coarse Y: tiles processed vertically (within 8 scanlines)
             */


            /*
                When doing the scroll, there is a X offset and Y offset; each offset represents
                how many pixels we need to skip (either X axis or Y axis) for produce a "scroll", meaning
                that we will be crossing a nametable (horizontally if mirroing mode is vertical; or vertically if morring
                mode is horizontal). The challenge of scrolling is that we are fetching "offset" pixels from the next nametable tile,
                so this gets things really complicated, also we are leaving the initial nametable, so we are not rendering anymore some
                of the pixels of that nametable.

                Remember that the scroll happens per pixel, not per tile. Meaning, you push pixels, not the whole tile. That being said,
                you can scroll 256 pixels in the X axis (remember the NES resolution is 256 wide); meanwhile you can scroll 240 pixels
                in the Y axis (NES resolution is 240 high).
             */

            /*
             * The cycle 0 (first tick) the PPU is idle (it does nothing). However, when the frame rendered is 
             * odd, the cycle 0 gets ignored and go straigh to cycle 1.
             */
            //if (_cycles++ == 0 && _framesRendered % 2 == 0)
            //    return;
            if (_scanline == 0 && _cycles == 0)
                _cycles = 1;

            if (_scanline == -1 || _scanline == 261)
                PreRenderScanline();
            else if (_scanline >= 0 && _scanline <= 239)
                RenderVisibleScanlines();
            else if (_scanline == 240)
                PostRenderScanline();
            else if (_scanline >= 241 && _scanline <= 260)
                VerticalBlankArea();

            _cycles++;
            if (_cycles >= 341)
            {
                _cycles = 0;
                if (_scanline < 260)
                {
                    _scanline++;
                }
                else
                {
                    ResetCoordinates();
                    _scanline = -1;
                    _framesRendered++;
                    if (_framesRendered > 60)
                        _framesRendered = 1;

                    IsFrameCompleted = true;
                    FrameBuffer = _frame;
                    ResetFrame();
                }
            }
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

        private void LoadShiftRegisters()
        {
            byte n = 0;
            switch(n)
            {
                // Load nametable byte
                case 2:
                    break;
                // Load attribute table byte
                case 4:
                    break;
                // Load pattern table bit low (string of low bits: pixels within a tile)
                case 6:
                    break;
                // Load pattern table bit high
                case 8:
                    break;
            }
        }

        private void PreRenderScanline()
        {
            if (_scanline == 261 && _cycles == 1)
                Status.SetVerticalBlank(false);
        }

        private int _fineX;
        private int _fineY;

        private int _coarseX;
        private int _coarseY;


        private byte GetTileIndex()
        {
            /*
             * Each 8 cycles within a scanline we move to the next tile.
             * Each 8 scanline, we move to a row of tiles.
             * 
             * We perform some calculation to produce an offset that would be added to a base address (the first address of a nametable). At least for this
             * approach, we do NOT store this addition (base address + offset); instead, we update the offset periodically to produce an address; this address
             * would be used to lookup the tile ID (indexed to a pattern table) in the nametable.
             */
            byte xOffset = (byte)((_cycles - 1) / 8); // offset in X axis
            //int xOffset = ((_cycles - 1) / 8); // offset in X axis
            //if (xOffset > 31)
            //    Console.WriteLine();

            int yOffset = (_scanline / 8) * 32; // offset in Y axis (it's multipled by 32 for denote that we are skipping rows: a row = 32 tiles)

            ushort tileAddress = (ushort)(GetNametableBaseAddress() + (xOffset + yOffset));

            return _ppuBus.Read(tileAddress);
        }

        private ushort GetNametableBaseAddress()
        {
            byte nametable = Control.GetNametableAddress();

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

        private byte GetColorIndex(byte tileIndex)
        {
            //byte patternTable = Control.GetPatternTableAddress();

            // TODO: change this structure in the future
            //Bitmap tile = _patternTables[patternTable][tileIndex];
            Tile tile = _backgroundTiles[tileIndex];
            if (tileIndex > 0)
                Console.WriteLine();

            int xOffset = (_cycles - 1) / 8; // offset in X axis
            int yOffset = (_scanline / 8); // offset in Y axis (it's multipled by 32 for denote that we are skipping rows: a row = 32 tiles)

            int xOrigin = xOffset * 8;
            int yOrigin = yOffset * 8;

            byte colorIdx = tile.GetPixel(X - xOrigin, Y - yOrigin); // a 2 bit value
            //if (colorIdx > 3)
            //    Console.WriteLine();

            return colorIdx;
            //return tile.GetPixel(Y - yOrigin, X - xOrigin); // a 2 bit value
        }

        private byte GetAttribute()
        {
            int xOffset = (_cycles - 1) / 32;
            int yOffset = (_scanline / 32) * 8;

            int megaBlockOffset = xOffset + yOffset;
            ushort attributeEntryAddress = (ushort)(GetNametableBaseAddress() + 0x03C0 + megaBlockOffset);

            return _ppuBus.Read(attributeEntryAddress);
        }

        private void ParseCoordinatesForMegaBlock(out byte x, out byte y)
        {
            int xOffset = (_cycles - 1) / 32; // a number from 0 to 7
            //int yOffset = (_scanline / 32) * 8; // a number from 0 to 7
            int yOffset = (_scanline / 32); // a number from 0 to 7

            x = (byte)(xOffset * 32);
            y = (byte)(yOffset * 32);
        }

        private byte GetBlockId()
        {
            ParseCoordinatesForMegaBlock(out byte xBlockOrigin, out byte yBlockOrigin);

            int x = X - xBlockOrigin;
            int y = Y - yBlockOrigin;

            byte blockId; // from 1 up to 4
            if (x >= 0 && x < 16 && y >= 0 && y < 16) // Top left block
                blockId = 4;
            else if (x >= 16 && x < 32 && y >= 0 && y < 16) // Top right block
                blockId = 3;
            else if (x >= 0 && x < 16 && y >= 16 && y < 32) // Bottom left block
                blockId = 2;
            else if (x >= 16 && x < 32 && y >= 16 && y < 32) // Bottom right block
                blockId = 1;
            else
            {
                Console.WriteLine();
                throw new InvalidOperationException($"The given coordinates (${x},{y}) are invalid in terms of mega blocks.");
            }

            return blockId;
        }

        private int X => _cycles - 1;
        private int Y => _scanline;


        private static byte ParsePalette(byte attribute, byte blockId)
        {
            // TODO: the palette is a 2 bit value as well; ensure this

            /*
                        int mask = 1 << j;
                        int lowBit = (lowBitsRow & mask) == mask ? 1 : 0;
                        int highBit = (highBitsRow & mask) == mask ? 1 : 0;

                        // A 2 bit value
                        byte paletteColorIdx = (byte)(lowBit | (highBit << 1));             
             */

            byte palette;
            int lowBit;
            int highBit;

            switch(blockId)
            {
                case 1: // Bottom right
                    //palette = (byte)(attribute & (0b11000000));
                    highBit = (attribute & 0b10000000) == 0b10000000 ? 1 : 0;
                    lowBit = (attribute & 0b01000000) == 0b01000000 ? 1 : 0;

                    break;
                case 2: // Bottom left
                    //palette = (byte)(attribute & (0b00110000));
                    highBit = (attribute & 0b00100000) == 0b00100000 ? 1 : 0;
                    lowBit = (attribute & 0b00010000) == 0b00010000 ? 1 : 0;
                    break;
                case 3: // Top right
                    //palette = (byte)(attribute & (0b00001100));
                    highBit = (attribute & 0b00001000) == 0b00001000 ? 1 : 0;
                    lowBit = (attribute & 0b00000100) == 0b00000100 ? 1 : 0;
                    break;
                case 4: // Top left
                    //palette = (byte)(attribute & (0b00000011));
                    highBit = (attribute & 0b00000010) == 0b00000010 ? 1 : 0;
                    lowBit = (attribute & 0b00000001) == 0b00000001 ? 1 : 0;
                    break;
                default:
                    throw new InvalidOperationException($"The given block ID is invalid: ${blockId}.");
            }

            palette = (byte)(lowBit | (highBit << 1));

            if (palette > 3)
                Console.WriteLine();

            return palette;
        }

        private Color GetBackgroundColor(byte palette, byte colorIndex)
        {
            //byte paletteColor = _ppuBus.Read((ushort)(0x3F00 + (palette << 2) + colorIndex));
            ushort paletteColorAddress = ParseBackgroundPaletteAddress(palette, colorIndex);
            byte paletteColor = _ppuBus.Read(paletteColorAddress);
            if (paletteColor < 0 || paletteColor > SystemColorPalette.Length)
                throw new InvalidOperationException($"The given palette color does not exist: {paletteColor}.");

            return SystemColorPalette[paletteColor];
        }

        private static ushort ParseBackgroundPaletteAddress(byte paletteId, byte colorIndex)
        {
            ushort baseAddress = 0x3F00;
            var offset = (byte)((paletteId * 4) + colorIndex);

            return (ushort)(baseAddress + offset);
        }

        private void RenderVisibleScanlines()
        {
            // Do not paint a pixel if it's a cycle outside of the visible boundary
            if (_cycles < 1 || _cycles > 256)
                return;

            //var ids = new List<byte>();
            ////for (var u = 0x2000; u < 0x2400; u++)
            //for (var u = 0x2000; u < 0x23C0; u++)
            //    ids.Add(_ppuBus.Read((ushort)u));

            byte tileIdx = GetTileIndex(); // Identified Tile ID
            
            byte colorIndex = GetColorIndex(tileIdx); // Identified color index within the colors palette
            byte attribute = GetAttribute(); // Fetched the attribute entry
            byte blockId = GetBlockId(); // Identified the block within the "mega" block
            byte palette = ParsePalette(attribute, blockId); // Parsed the pallete id based on the block id and the attribute's entry

            //int x = _cycles - 1;
            //int y = _scanline;

            //If bit 3 from Mask register is set, it means we can render the background
            if ((Mask & 0x08) == 0x08)
                _frame.SetPixel(X, Y, GetBackgroundColor(palette, colorIndex));
            else
                _frame.SetPixel(X, Y, Color.Black);

            //_fineX++;
            //if (_fineX > 8)
            //{
            //    _fineX = 0;
            //    _coarseX++;
            //}

            //if (_coarseX > 31)
            //{
            //    _fineY++;
            //    _coarseX = 0;
            //}

            //if (_fineY > 7)
            //{
            //    _fineY = 0;
            //    _coarseY++;
            //}
        }

        private void ResetCoordinates()
        {
            _fineX = 0;
            _fineY = 0;
            _coarseX = 0;
            _coarseY = 0;
        }

        private void PostRenderScanline()
        {
            // Do nothing
        }

        private void VerticalBlankArea()
        {
            if (_scanline == 241 && _cycles == 1)
            {
                Status.SetVerticalBlank(true);
                if (Control.GetNmi())
                    NmiRequested = true;
                //NmiRequested = true;
            }
        }

        public byte[][] GetNametable()
        {
            ushort nametableAddress = GetNametableBaseAddress();
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
    }
}
