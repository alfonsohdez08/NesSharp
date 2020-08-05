using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace NesSharp.UI
{
    public partial class EmulatorUI : Form
    {
        private string _gamePath;
        private NES _nes;
        private int[] _currentFrame;
        private PictureBox _screen;

        private delegate void PaintScreen();

        private readonly Joypad _joypad = new Joypad();
        private readonly Dictionary<Keys, Button> _joypadMapping = new Dictionary<Keys, Button>()
        {
            {Keys.Z, Button.A },
            {Keys.X, Button.B },
            {Keys.Up, Button.Up },
            {Keys.Down, Button.Down },
            {Keys.Left, Button.Left },
            {Keys.Right, Button.Right },
            {Keys.Enter, Button.Start },
            {Keys.Space, Button.Select }
        };

        public EmulatorUI()
        {
            InitializeComponent();

            var menuStrip = new MenuStrip();

            var fileMenuItem = new ToolStripMenuItem("File");
            
            var openRomItem = new ToolStripMenuItem("Open");
            openRomItem.Click += OpenRomSelectionDialog;

            fileMenuItem.DropDownItems.Add(openRomItem);

            menuStrip.Items.Add(fileMenuItem);

            Controls.Add(menuStrip);

            _screen = new PictureBox
            {
                Width = 256,
                Height = 240,
                Location = new Point(0, menuStrip.Location.Y + 25),
                Image = GetBlackScreen()
            };

            Controls.Add(_screen);
        }

        private Image GetBlackScreen()
        {
            var bitmap = new Bitmap(Width, Height);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    bitmap.SetPixel(x, y, Color.Black);
                }
            }

            return bitmap;
        }

        private void OpenRomSelectionDialog(object o, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = @"C:\Users\ward\nes";
                openFileDialog.Filter = "nes files (*.nes)|*.nes|All files (*.*)|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _gamePath = openFileDialog.FileName;

                    StartEmulation();
                }
            }
        }

        private void StartEmulation()
        {
            Cartridge cartridge = Cartridge.LoadCartridge(_gamePath);
            _nes = new NES(cartridge, _joypad);
            
            Task.Factory.StartNew(RunGame, TaskCreationOptions.LongRunning);
        }

        private void RunGame()
        {
            var paintGameScreen = new PaintScreen(DrawImage);

            var stopwatch = new Stopwatch();
            while(true)
            {
                stopwatch.Restart();
                for (int i = 0; i < 60; i++)
                {
                    _currentFrame = _nes.Frame();
                    _screen.Invoke(paintGameScreen);
                }

                stopwatch.Stop();

                Console.WriteLine($"{stopwatch.ElapsedMilliseconds} ms elapsed to process 60 frames per second.");
            }
        }

        unsafe private void DrawImage()
        {
            fixed(int* framePointer = _currentFrame)
            {
                var bitmap = new SKBitmap(256, 240);
                bitmap.SetPixels((IntPtr)framePointer);

                _screen.Image = bitmap.ToBitmap();
            }
        }

        private void EmulatorUI_KeyDown(object sender, KeyEventArgs e)
        {
            if (_joypadMapping.TryGetValue(e.KeyCode, out Button buttonPressed))
                _joypad.PressButton(buttonPressed);
        }

        private void EmulatorUI_KeyUp(object sender, KeyEventArgs e)
        {
            if (_joypadMapping.TryGetValue(e.KeyCode, out Button buttonPressed))
                _joypad.ReleaseButton(buttonPressed);
        }
    }
}
