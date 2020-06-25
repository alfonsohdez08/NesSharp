using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiNES.Emu
{
    public partial class Form1 : Form
    {
        private static readonly string NesRootPath = Environment.GetEnvironmentVariable("NES", EnvironmentVariableTarget.Machine);
        private byte[] donkeyKongRom = File.ReadAllBytes(Path.Combine(NesRootPath, "donkey_kong.nes"));
        private byte[] superMarioBrosRom = File.ReadAllBytes(Path.Combine(NesRootPath, "super_mario_bros.nes"));

        private object _lockObject = new object();
        private bool _runEmulation = true;
        private NES nes;

        public bool RunEmulation
        {
            get
            {
                lock (_lockObject)
                {
                    return _runEmulation;
                }
            }
        }

        public Form1()
        {
            InitializeComponent();
            
            nes = new NES(donkeyKongRom);
            //EnableEmulation.Checked = true;

            StartEmulation();
        }

        private void StartEmulation()
        {
            //var nes = new NES(superMarioBrosRom);

            // TODO: see if i can add a debugger for see my nametables per frame

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (RunEmulation)
                    {
                        //GameScreen.Image = GetDummyBitmap();
                        GameScreen.Image = nes.Frame();
                    }

                }
            }, TaskCreationOptions.LongRunning);

        }

        private static Bitmap GetDummyBitmap()
        {
            var bitmap = new Bitmap(256, 240);
            bitmap.SetPixel(230, 1, Color.Red);

            return bitmap;
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {
            //NametableGrid.Colu
        }

        private void ResumeEmulation()
        {
            lock (_lockObject)
            {
                _runEmulation = true;
            }
        }

        private void StopEmulation()
        {
            lock (_lockObject)
            {
                _runEmulation = false;
            }

            DrawNametable();
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void DrawNametable()
        {
            byte[][] nametable = nes.GetNametable();

            //tableLayoutPanel1.Visible = true;
            //tableLayoutPanel1.Enabled = true;

            tableLayoutPanel1.Controls.Clear();
            for (int row = 0; row < nametable.Length; row++)
            {
                for (int column = 0; column < nametable[row].Length; column++)
                {
                    //Control control = tableLayoutPanel1.GetControlFromPosition(row, column);
                    //Control control = new Control();
                    Label label = new Label();
                    //label.Text = nametable[row][column].ToString("X");
                    label.Height = 20;
                    label.Width = 20;
                    label.Text = ParseHex(nametable[row][column]);
                    label.Visible = true;
                    label.ForeColor = Color.Black;
                    //control.Text = ParseHex(nametable[row][column]);

                    //tableLayoutPanel1.SetCellPosition(control, new TableLayoutPanelCellPosition(row, column));
                    //tableLayoutPanel1.SetCellPosition(label, new TableLayoutPanelCellPosition(row, column));
                    tableLayoutPanel1.Controls.Add(label, column, row);
                }
            }
        }

        private static string ParseHex(byte b) => $"{b.ToString("X").PadLeft(2, '0')}";

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (RunEmulation)
            {
                StopEmulation();
                ManageEmulation.Text = "Resume Emulation";
            }
            else
            {
                ResumeEmulation();
                ManageEmulation.Text = "Stop Emulation";
            }
        }
    }
}
