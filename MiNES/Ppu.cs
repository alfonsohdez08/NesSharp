using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MiNES
{
    public class Ppu
    {
        private readonly PpuBus _ppuBus;
        private readonly CpuBus _cpuBus;

        private Bitmap[][] _patternTables;
        //private Bitmap[][] _nameTables;

        public ushort Address { get; set; }

        public byte ControlRegister { get; set; }
        public byte MaskRegister { get; set; }
        public byte ScrollRegister { get; set; }
        // ... and so on


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

        public Ppu(PpuBus ppuBus, CpuBus cpuBus)
        {
            _ppuBus = ppuBus;
            _cpuBus = cpuBus;

            Initialize();
            SetPatternTables();
        }

        /// <summary>
        /// Sets the pattern tables (background and foreground tiles).
        /// </summary>
        private void SetPatternTables()
        {
            _patternTables = new Bitmap[2][];

            _patternTables[0] = GetBitmapPatternTable();
            _patternTables[1] = GetBitmapPatternTable(false);

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

        /// <summary>
        /// Initializes the PPU (stabilize its state).
        /// </summary>
        private void Initialize()
        {
            /* When powering up the PPU, the status register (located at $2002) will
             * be set to 0xA0 (10100000).
             * 
             * Bit 7: 0 = the PPU is not in the V-Blank area (Vertical Blank is the area non visible of the screen); 1 = the PPU is in the V-Blank area
             * Bit 6: Sprite hit (more research about this)
             * Bit 5: Sprite overflow (more than 8 sprites appears in a scanline)
             */
            _cpuBus.Write(0x2002, 0xA0);
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
            
        }
        
        public Bitmap DrawBackgroundTiles()
        {
            Bitmap bitmap = GetNESScreen();
            
            int x = 0, y = 0;

            int tiles = 0;

            for (ushort address = 0x0000; address < 0x1000; address += 0x0010)
            //for (ushort address = 0x1000; address < 0x2000; address += 0x0010)
            {
                ushort tileAddress = address;
                int yTile = y;

                /* Process a tile (8 x 8 pixels)
                 * For each iteration, we process a row of pixels for a tile.
                 */
                for (int i = 0; i < 8; i++)
                {
                    int xTitle = x;

                    byte lowBitRow = _ppuBus.Read(tileAddress);
                    byte highBitRow = _ppuBus.Read((ushort)(tileAddress + 0x0008));

                    byte[] pixels = new byte[8];

                    // Iterates over each bit of the byte for extract each bit
                    for (int j = 0; j < 8; j++)
                    {
                        int mask = 1 << j;
                        int lowBit = (lowBitRow & mask) == mask ? 1 : 0;
                        int highBit = (highBitRow & mask) == mask ? 1 : 0;

                        byte pixelVal = (byte)(lowBit | (highBit << 1));
                        
                        pixels[(pixels.Length - 1) - j] = pixelVal;
                    }

                    // Draws the tile's row of pixels
                    for (int j = 0; j < pixels.Length; j++)
                    {
                        byte colorIndex = pixels[j];

                        bitmap.SetPixel(xTitle, yTile, Color.FromArgb(GetColor(colorIndex)));
                        xTitle++;
                    }

                    // Moves to the next address that denotes the next row of pixels for the current tile
                    tileAddress++;

                    // Advances to the next set of pixels that within the height range
                    yTile++;
                }

                tiles++;
                if (tiles >= 32)
                {
                    x = 0;
                    y += 8;

                    tiles = 0;
                }
                else
                {
                    x += 8;
                }
            }

            return bitmap;
        }

        private static int GetColor(byte index)
        {
            switch(index)
            {
                case 0:
                    return Color.White.ToArgb();
                case 1:
                    return Color.Red.ToArgb();
                case 2:
                    return Color.Blue.ToArgb();
                case 3:
                    return Color.Green.ToArgb();
                default:
                    throw new InvalidOperationException($"The index {index} does not have a color associated with it.");
            }
        }



        private static Bitmap GetNESScreen() => new Bitmap(256, 240);
    }
}
