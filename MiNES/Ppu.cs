using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MiNES
{
    public class Ppu
    {
        private readonly PpuBus _ppuBus;

        private Bitmap[][] _patternTables;
        //private Bitmap[][] _nameTables;

        /// <summary>
        /// Controls the state of the address latch (false = high byte; true = low byte).
        /// </summary>
        private bool _addressLatch = false;

        /// <summary>
        /// PPU Control register.
        /// </summary>
        public byte Control { get; set; }

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
        public byte Status { get; set; } = 0xA0;

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
        /// PPU Address register.
        /// </summary>
        public byte Address
        {
            set
            {
                if (!_addressLatch)
                {
                    _address = (ushort)(((_address | 0xFF00) ^ 0xFF00) | (value << 8));
                    _addressLatch = true;
                }
                else
                {
                    _address = (ushort)(((_address | 0xFF) ^ 0xFF) | value);
                }
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
        private static readonly Color[] SystemColorPalette = new Color[]
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
        public Bitmap Frame { get; private set; }

        public bool NmiRequested { get; set; }

        public Ppu(PpuBus ppuBus)
        {
            _ppuBus = ppuBus;

            SetPatternTables();
        }

        public void ResetAddressLatch() => _addressLatch = false;

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

            // If bit 3 from control register is set, add 32 to VRAM address; otherwise 1
            if ((Control & 0x0004) == 0x0004)
                _address += 32;
            else
                _address++;

            return data;
        }

        public void SetPpuData(byte val)
        {
            _ppuBus.Write(_address, val);

            // If bit 3 from control register is set, add 32 to VRAM address; otherwise 1
            if ((Control & 0x0004) == 0x0004)
                _address += 32;
            else
                _address++;
        }

        /// <summary>
        /// Sets the pattern tables (background and foreground tiles).
        /// </summary>
        private void SetPatternTables()
        {
            _patternTables = new Bitmap[2][];

            _patternTables[0] = GetBitmapPatternTable(false); // Left side of the pattern table it's for foreground tiles (sprites)
            _patternTables[1] = GetBitmapPatternTable(); // Right side of the pattern table it's for the background tiles

            Bitmap[] GetBitmapPatternTable(bool backgroundTiles = true)
            {
                Bitmap[] tilesBitmap = new Bitmap[256];

                int tiles = 0;

                ushort address = 0x0000;
                ushort lastAddress = 0x1000;

                if (!backgroundTiles)
                {
                    address = 0x1000;
                    lastAddress = 0x2000;
                }

                for (; address < lastAddress; address += 16)
                {
                    Bitmap tileBitmap = new Bitmap(8, 8);

                    // Process an entire tile (row by row, where each row represents a string of pixels)
                    for (int i = 0; i < 8; i++)
                    {
                        byte lowBitsRow = _ppuBus.Read(address);
                        byte highBitsRow = _ppuBus.Read((ushort)(address + 0x0008)); // high bit plane is offset 8 positions away

                        // Iterate over each bit within both set of bits (bytes) for draw the tile bitmap
                        for (int j = 0; j < 8; j++)
                        {
                            int mask = 1 << j;
                            int lowBit = (lowBitsRow & mask) == mask ? 1 : 0;
                            int highBit = (highBitsRow & mask) == mask ? 1 : 0;

                            // A 2 bit value
                            byte paletteColorIdx = (byte)(lowBit | (highBit << 1));

                            tileBitmap.SetPixel(i, j, Color.FromArgb(paletteColorIdx));
                        }
                    }

                    tilesBitmap[tiles] = tileBitmap;
                    tiles++;
                }

                return tilesBitmap;
            }
        }


        private void ResetFrame() => _frame = new Bitmap(256, 240);


        private int _cycles = 0;
        private int _scanline = 0;
        private Bitmap _frame;


        /// <summary>
        /// Fetches the tile index from the nametable.
        /// </summary>
        /// <returns>The tile index that would be used for fetch the tile bitmap from the pattern table.</returns>
        private byte GetTileIndex()
        {

            throw new NotImplementedException();
        }

        private byte FindTilePixelByIndex(byte tileIndex)
        {
            throw new NotImplementedException();
        }

        private Color GetColor(byte palette, byte colorIndex)
        {
            throw new NotImplementedException();
        }
        

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
             * There are 240 scanlines. Each pixel within each scanline is drawn in each clock cycle.
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

            // TODO: take donkey kong game, and draw the nametable based on what i understand (following more or less the scanlines and cycles)
            byte tileIdx = GetTileIndex();
            byte tileColorIndex = FindTilePixelByIndex(tileIdx);
            byte palette = 0; // from attribute table

            _frame.SetPixel(_cycles, _scanline, GetColor(palette, tileColorIndex));

            _cycles++;
        }

        //public Bitmap DrawBackgroundTiles()
        //{
        //    Bitmap bitmap = GetNESScreen();
            
        //    int x = 0, y = 0;

        //    int tiles = 0;

        //    for (ushort address = 0x0000; address < 0x1000; address += 0x0010)
        //    //for (ushort address = 0x1000; address < 0x2000; address += 0x0010)
        //    {
        //        ushort tileAddress = address;
        //        int yTile = y;

        //        /* Process a tile (8 x 8 pixels)
        //         * For each iteration, we process a row of pixels for a tile.
        //         */
        //        for (int i = 0; i < 8; i++)
        //        {
        //            int xTitle = x;

        //            byte lowBitRow = _ppuBus.Read(tileAddress);
        //            byte highBitRow = _ppuBus.Read((ushort)(tileAddress + 0x0008));

        //            byte[] pixels = new byte[8];

        //            // Iterates over each bit of the byte for extract each bit
        //            for (int j = 0; j < 8; j++)
        //            {
        //                int mask = 1 << j;
        //                int lowBit = (lowBitRow & mask) == mask ? 1 : 0;
        //                int highBit = (highBitRow & mask) == mask ? 1 : 0;

        //                byte pixelVal = (byte)(lowBit | (highBit << 1));
                        
        //                pixels[(pixels.Length - 1) - j] = pixelVal;
        //            }

        //            // Draws the tile's row of pixels
        //            for (int j = 0; j < pixels.Length; j++)
        //            {
        //                byte colorIndex = pixels[j];

        //                bitmap.SetPixel(xTitle, yTile, Color.FromArgb(GetColor(colorIndex)));
        //                xTitle++;
        //            }

        //            // Moves to the next address that denotes the next row of pixels for the current tile
        //            tileAddress++;

        //            // Advances to the next set of pixels that within the height range
        //            yTile++;
        //        }

        //        tiles++;
        //        if (tiles >= 32)
        //        {
        //            x = 0;
        //            y += 8;

        //            tiles = 0;
        //        }
        //        else
        //        {
        //            x += 8;
        //        }
        //    }

        //    return bitmap;
        //}

        //private static int GetColor(byte index)
        //{
        //    switch(index)
        //    {
        //        case 0:
        //            return Color.White.ToArgb();
        //        case 1:
        //            return Color.Red.ToArgb();
        //        case 2:
        //            return Color.Blue.ToArgb();
        //        case 3:
        //            return Color.Green.ToArgb();
        //        default:
        //            throw new InvalidOperationException($"The index {index} does not have a color associated with it.");
        //    }
        //}

        //private static Bitmap GetNESScreen() => new Bitmap(256, 240);
    }
}
