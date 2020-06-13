using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiNES.Emu
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            InitScreen();
        }

        private void InitScreen()
        {
            //var bitmap = new Bitmap(32, 32);
            //for (int row = 0; row < bitmap.Width; row++)
            //{
            //    for (int column = 0; column < bitmap.Height; column++)
            //    {
            //        bitmap.SetPixel(row, column, Color.FromArgb(row + 100, column + 100, row + column + 50));
            //    }
            //}

            var bitmap = new Bitmap(8, 8);


            for (int x = 0; x < bitmap.Width; x++)
            {
                bitmap.SetPixel(x, 0, Color.Blue);
            }

            //for (int y = 0; y < bitmap.Height; y++)
            //{
            //    bitmap.SetPixel(0, y, Color.Blue);
            //}


            //bitmap.SetPixel(1, 0, Color.Blue);
            //bitmap.SetPixel(2, 0, Color.Blue);

            //bitmap.SetPixel(3, 0, Color.Blue);

            //bitmap.SetPixel(4, 0, Color.Blue);



            GameScreen.Image = bitmap;
        }
    }
}
