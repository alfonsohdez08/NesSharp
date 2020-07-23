using MiNES.Emu.Debugger;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace MiNES.Emu
{
    public partial class Form1 : Form
    {
        private static readonly string NesRootPath = Environment.GetEnvironmentVariable("NES", EnvironmentVariableTarget.Machine);
        private byte[] donkeyKongRom = File.ReadAllBytes(Path.Combine(NesRootPath, "donkey_kong.nes"));
        private byte[] superMarioBrosRom = File.ReadAllBytes(Path.Combine(NesRootPath, "super_mario_bros.nes"));
        private byte[] nesTestRom = File.ReadAllBytes(Path.Combine(NesRootPath, "nestest.nes"));
        private byte[] iceClimbersRom = File.ReadAllBytes(Path.Combine(NesRootPath, "ice_climbers.nes"));
        private byte[] scanlineTestRom = File.ReadAllBytes(Path.Combine(NesRootPath, "scanline_a1.nes"));

        private byte[] spriteRamTestRom = File.ReadAllBytes(Path.Combine(NesRootPath, "sprite_ram.nes"));

        private byte[] vramTestRom = File.ReadAllBytes(Path.Combine(NesRootPath, "vbl_clear_time.nes"));

        private byte[] vblTestRom = File.ReadAllBytes(Path.Combine(NesRootPath, "vram_access.nes"));

        private byte[] oamReadTestRom = File.ReadAllBytes(Path.Combine(NesRootPath, "oam_read.nes"));

        private byte[] ppuSpriteHitTestRom = File.ReadAllBytes(Path.Combine(NesRootPath, "ppu_sprite_hit.nes"));

        private byte[] vblBasicsTestRom = File.ReadAllBytes(Path.Combine(NesRootPath, "01-vbl_basics.nes"));

        //private BackgroundWorker _backgroundWorker = new BackgroundWorker();

        private object _lockObject = new object();
        private bool _runEmulation = true;
        private NES nes;

        //private NametableDebugger _nametableDebugger;

        private delegate void UpdateGameScreen(int[] buffer);

        private readonly SKBitmap _gameScreen = new SKBitmap(256, 240);

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

        private void DrawTileBorders(ref Bitmap frame)
        {
            Color color = Color.White;

            for (int y = 0; y < 240; y += 8)
            {
                for (int x = 0; x < 256; x++)
                {
                    frame.SetPixel(x, y, color);
                }
            }

            for (int x = 0; x < 256; x += 8)
            {
                for (int y = 0; y < 240; y++)
                {
                    frame.SetPixel(x, y, color);
                }
            }
        }

        public Form1()
        {
            InitializeComponent();

            //nes = new NES(donkeyKongRom);
            nes = new NES(superMarioBrosRom);
            //nes = new NES(nesTestRom);
            //nes = new NES(iceClimbersRom);
            //nes = new NES(scanlineTestRom);

            //nes = new NES(spriteRamTestRom);

            //nes = new NES(vramTestRom);

            //nes = new NES(vblTestRom);

            //nes = new NES(oamReadTestRom);


            //nes = new NES(ppuSpriteHitTestRom);
            //nes = new NES(vblBasicsTestRom);

            //EnableEmulation.Checked = true;

            StartEmulation();
        }

        private void StartEmulation()
        {
            //Task.Factory.StartNew(() =>
            //{
            //    RunGame();
            //}, TaskCreationOptions.LongRunning);

            var thread = new Thread(new ThreadStart(RunGame));
            thread.Start();
        }

        private void RunGame()
        {
            var stopWatch = new Stopwatch();
            var updateScreen = new UpdateGameScreen(UpdateScreen);

            while (true)
            {
                if (RunEmulation)
                {
                    if (!stopWatch.IsRunning)
                        stopWatch.Restart();

                    for (int i = 0; i < 60; i++)
                    {
                        int[] frame = nes.Frame();
                        GameScreen.Invoke(updateScreen, new object[] { frame });
                    }
                    
                    stopWatch.Stop();
                    var ms = stopWatch.ElapsedMilliseconds;
                }

            }
        }

        private void UpdateScreen(int[] pixelsBuffer)
        {
            unsafe
            {
                fixed(int* ptr = pixelsBuffer)
                {
                    _gameScreen.SetPixels((IntPtr)ptr);
                }
            }

            /*
using (SKImage image = surface.Snapshot())
using (SKData data = image.Encode(SKEncodedImageFormat.Png, 100))
using (MemoryStream mStream = new MemoryStream(data.ToArray()))
{
    Bitmap bm = new Bitmap(mStream, false);
    pictureBox1.Image = bm;
}             
             */

            using (var image = SKImage.FromBitmap(_gameScreen))
            using (var data = image.Encode())
            using (var stream = new MemoryStream(data.ToArray()))
            {
                var bitmap = new Bitmap(stream);
                GameScreen.Image = bitmap;
            }
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {
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

            //DrawNametable();
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void DrawNametable()
        {
            byte[][] nametable = nes.GetNametable0();

            //tableLayoutPanel1.Visible = true;
            //tableLayoutPanel1.Enabled = true;

            //tableLayoutPanel1.Controls.Clear();
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
                    //tableLayoutPanel1.Controls.Add(label, column, row);
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

        private void button1_Click_1(object sender, EventArgs e)
        {
            // Only draw the nametable when the emulation is stopped
            if (!RunEmulation)
            {
                var nametableDebugger = new NametableDebugger();

                //nametableDebugger.DrawNametable(nes.GetNametable0(), nes.GetBackgroundTiles());
                nametableDebugger.DrawNametable(nes.GetNametable2(), nes.GetBackgroundTiles());
                nametableDebugger.Show();

                //var nametableDebugger = new NametableDebugger();
                //nametableDebugger.DrawNametable(nes.GetNametable());
                //nametableDebugger.Show();

                //if (_nametableDebugger == null)
                //    _nametableDebugger = new NametableDebugger();

                //_nametableDebugger.DrawNametable(nes.GetNametable());
                //_nametableDebugger.Show();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!RunEmulation)
            {
                var backgroundDebugger = new NametableDebugger();

                backgroundDebugger.DrawPatternTable(nes.GetBackgroundTiles());
                backgroundDebugger.Show();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!RunEmulation)
            {
                var backgroundDebugger = new NametableDebugger();
                Color[] palettes = nes.GetPalettes();

                backgroundDebugger.DrawPalettes(palettes);
                backgroundDebugger.Show();
            }
        }
    }
}
