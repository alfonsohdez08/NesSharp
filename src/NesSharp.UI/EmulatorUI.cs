using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace NesSharp.UI
{
    public partial class EmulatorUI : Form
    {
        private const string NesFilesDialogFilter = "nes files (*.nes)|*.nes";

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
        private readonly object _locker = new object();

        private PictureBox _screen;
        private CancellationTokenSource _cancellationTokenSource;

        private delegate void PaintScreen(int[] frame);

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
                Width = Width,
                Height = Height,
                Location = new Point(0, menuStrip.Location.Y + 25),
                Image = GetBlackScreen(Width, Height),
            };

            Controls.Add(_screen);
        }

        private static Image GetBlackScreen(int width, int height)
        {
            var bitmap = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bitmap.SetPixel(x, y, Color.Black);
                }
            }

            return bitmap;
        }

        private void OpenRomSelectionDialog(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = NesFilesDialogFilter;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                    StartEmulation(openFileDialog.FileName);
            }
        }

        private void StartEmulation(string gamePath)
        {
            _cancellationTokenSource?.Cancel();

            _joypad.ResetJoypadState();
            Cartridge cartridge = Cartridge.LoadCartridge(gamePath);
            var nes = new NES(cartridge, _joypad);

            _cancellationTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(() => RunGame(nes, _cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
        }

        private void RunGame(NES nes, CancellationToken cancellationToken)
        {
            bool abortEmulation = false;
            var paintGameScreen = new PaintScreen(DrawImage);

            var stopwatch = new Stopwatch();
            while(!abortEmulation)
            {
                stopwatch.Restart();
                for (int i = 0; i < 60; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        abortEmulation = true;
                        break;
                    }

                    var frame = nes.Frame();
                    _screen.Invoke(paintGameScreen, new object[] { frame });
                }

                stopwatch.Stop();

                Console.WriteLine($"{stopwatch.ElapsedMilliseconds} ms elapsed to process 60 frames per second.");
            }
        }

        unsafe private void DrawImage(int[] frame)
        {
            lock(_locker)
            {
                fixed (int* framePointer = frame)
                {
                    var bitmap = new SKBitmap(256, 240);
                    bitmap.SetPixels((IntPtr)framePointer);

                    _screen.Image = bitmap.ToBitmap();
                }
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

        private void EmulatorUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
