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

        private static Color GetColor(byte index)
        {
            switch(index)
            {
                case 0:
                    return Color.Black;
                case 1:
                    return Color.Red;
                case 2:
                    return Color.Green;
                case 3:
                    return Color.Blue;
                default:
                    throw new InvalidOperationException($"The given color index {index} is not mapped to a color.");
            }
        }
    }
}
