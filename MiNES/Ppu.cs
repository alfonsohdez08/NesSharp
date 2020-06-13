using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MiNES
{
    class Ppu
    {
        private readonly PpuBus _ppuBus;
        private readonly CpuBus _cpuBus;

        public Ppu(PpuBus ppuBus, CpuBus cpuBus)
        {
            _ppuBus = ppuBus;
            _cpuBus = cpuBus;
        }
        
        public Bitmap DrawBackgroundTiles()
        {
            Bitmap bitmap = GetNESScreen();

            int x = 0, y = 0;

            for (ushort address = 0x0000; address < 0x1000; address += 0x0010)
            {
                ushort tileAddress = address;
                int yTile = y;

                /* Process a tile (8 x 8 pixels)
                 * For each iteration, we process a row of pixels for a tile.
                 */
                for (int i = 0; i < 8; i++)
                {
                    int xTitle = x;

                    byte lowPixelRow = _ppuBus.Read(tileAddress);
                    byte highPixelRow = _ppuBus.Read((ushort)(tileAddress + 0x0008));

                    byte[] pixels = new byte[8];

                    // Iterates over each bit of the byte for extract each bit
                    for (int j = 0; j < 8; j++)
                    {
                        int mask = 1 << j;
                        int lowBit = lowPixelRow & mask;
                        int highBit = highPixelRow & mask;

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

                x += 8;
                y += 8;
            }

            return bitmap;
        }

        private static int GetColor(byte index)
        {
            switch(index)
            {
                case 0:
                    return Color.Black.ToArgb();
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
