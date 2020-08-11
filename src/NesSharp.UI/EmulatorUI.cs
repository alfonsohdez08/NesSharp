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

        private CancellationTokenSource _cancellationTokenSource;
        private readonly SKControl _gameScreen;

        private int[] _frameBuffer = new int[256 * 240];

        public EmulatorUI()
        {
            InitializeComponent();

            // The SKControl it's focused while rendering, so this would make 
            // sure the key event handlers reach the form first
            //this.KeyPreview = true;

            var menuStrip = new MenuStrip();

            var fileMenuItem = new ToolStripMenuItem("File");
            
            var openRomItem = new ToolStripMenuItem("Open");
            openRomItem.Click += OpenRomSelectionDialog;

            fileMenuItem.DropDownItems.Add(openRomItem);

            menuStrip.Items.Add(fileMenuItem);

            Controls.Add(menuStrip);

            _gameScreen = new SKControl()
            {
                Location = new Point(0, menuStrip.DisplayRectangle.Bottom + 1),
                Size = new Size(this.ClientSize.Width, this.ClientSize.Height)
            };
            _gameScreen.PaintSurface += OnPaintSurface;
            _gameScreen.KeyDown += EmulatorUI_KeyDown;
            _gameScreen.KeyUp += EmulatorUI_KeyUp;
            _gameScreen.PreviewKeyDown += EmulatorUI_PreviewKeyDown;

            Controls.Add(_gameScreen);
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

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;

            var bitmap = new SKBitmap(256, 240);
            unsafe
            {
                fixed (int* framePointer = _frameBuffer)
                {
                    bitmap.SetPixels((IntPtr)framePointer);
                }
            }
            var b = bitmap.Resize(new SKImageInfo(e.Info.Width, e.Info.Height), SKFilterQuality.None);
            canvas.DrawBitmap(b, SKPoint.Empty);

            canvas.Flush();
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

                    _frameBuffer = nes.Frame();
                    _gameScreen.Invalidate(); // triggers repainting for the control
                }

                stopwatch.Stop();

                Console.WriteLine($"{stopwatch.ElapsedMilliseconds} ms elapsed to process 60 frames per second.");
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

        private void EmulatorUI_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) => e.IsInputKey = true;

        private void EmulatorUI_Resize(object sender, EventArgs e)
        {
            _gameScreen.Size = new Size(this.ClientSize.Width, this.ClientSize.Height);
        }
    }
}
