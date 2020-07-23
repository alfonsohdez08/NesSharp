using MiNES.PPU;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MiNES.Emu.Debugger
{
    public partial class NametableDebugger : Form
    {

        public NametableDebugger()
        {
            InitializeComponent();
        }

        public void DrawNametable(byte[][] nametable, Tile[] backgroundTiles)
        {
            Bitmap screen = new Bitmap(256, 240);

            int yOffset = 0;
            for (int row = 0; row < 30; row++)
            {
                int x = 0;
                for (int column = 0; column < 32; column++)
                {
                    byte tileIdx = nametable[row][column];
                    Tile tile = backgroundTiles[tileIdx];

                    // Draw an entire tile (8x8 pixels)
                    for (int r = 0; r < 8; r++)
                    {
                        for (int c = 0; c < 8; c++)
                        {
                            byte pixel = tile.GetPixel(c, r);
                            Color color = GetColor(pixel);

                            screen.SetPixel(x + c, r + yOffset, color);
                        }
                    }

                    x += 8;
                }

                yOffset += 8;
            }

            Screen.Image = screen;
        }

        public void DrawPatternTable(Tile[] tiles)
        {
            Bitmap screen = new Bitmap(256, 240);

            int tilesDraw = 0;

            int yOffset = 0;
            for (int row = 0; row < 16; row++)
            {
                int x = 0;
                for (int column = 0; column < 16; column++)
                {
                    Tile tile = tiles[tilesDraw];

                    // Draw an entire tile (8x8 pixels)
                    for (int r = 0; r < 8; r++)
                    {
                        for (int c = 0; c < 8; c++)
                        {
                            byte pixel = tile.GetPixel(c, r);
                            Color color = GetColor(pixel);

                            screen.SetPixel(x + c, r + yOffset, color);
                        }
                    }

                    x += 8;
                    tilesDraw++;
                }

                yOffset += 8;
            }

            Screen.Image = screen;
        }

        //public void Draw()
        //{
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

        private static Color GetColor(byte index)
        {
            switch(index)
            {
                //case 0:
                //    return Color.Black;
                //case 1:
                //    return Ppu.SystemColorPalette[0x2c];
                //case 2:
                //    return Ppu.SystemColorPalette[0x38];
                //case 3:
                //    return Ppu.SystemColorPalette[0x12];
                default:
                    throw new InvalidOperationException($"The given color index {index} is not mapped to a color.");
            }
        }

        public void DrawPalettes(Color[] palettes)
        {
            var screen = new Bitmap(256, 32);

            //int x = 0, y = 0;

            int colorIndex = 0;

            int yOffset = 0;
            for (int row = 0; row < 2; row++)
            {
                int x = 0;
                for (int column = 0; column < 16; column++)
                {
                    //byte tileIdx = nametable[row][column];
                    //Tile tile = backgroundTiles[tileIdx];

                    // Draw an entire color tile (16 x 16 pixels)
                    for (int r = 0; r < 16; r++)
                    {
                        for (int c = 0; c < 16; c++)
                        {
                            //byte pixel = tile.GetPixel(c, r);
                            Color color = palettes[colorIndex];

                            screen.SetPixel(x + c, r + yOffset, color);
                        }
                    }

                    colorIndex++;

                    x += 16;
                }

                yOffset += 16;
            }


            //for (int colorIdx = 0; colorIdx < palettes.Length; colorIdx++)
            //{
            //    Color color = palettes[colorIdx];

            //    // Process row by row (pixel row)
            //    for (int r = 0; r < 16; r++)
            //    {
            //        // Process column by column
            //        for (int c = 0; c < 16; c++)
            //        {
            //            screen.SetPixel(x + c, y + r, color);
            //        }
            //    }

            //    if (x >= 240)
            //    {
            //        x = 0;
            //        y += 16;
            //    }
            //    else
            //    {
            //        x += 16;
            //        y = 0;
            //    }
            //}

            Screen.Image = screen;
        }
    }
}
