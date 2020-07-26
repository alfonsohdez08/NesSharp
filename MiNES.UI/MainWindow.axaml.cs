using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Threading;
using Avalonia.Media.Imaging;
using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MiNES.UI
{
    public class MainWindow : Window
    {
        private NES _nes;
        private Task _emulationTask;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Renderer.DrawFps = true;

            InitializeEmulation();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            Closed += OnCloseWindows;
        }

        private void OnCloseWindows(object sender, EventArgs args)
        {
        }

        private void InitializeEmulation()
        {
            var game = File.ReadAllBytes(@"C:\Users\ward\nes\scanline.nes");
            _nes = new NES(game);

            _emulationTask = new TaskFactory().StartNew(RunGame, TaskCreationOptions.LongRunning);
        }

        private void RunGame()
        {
            var stopwatch = new Stopwatch();
            while (true)
            {
                stopwatch.Restart();
                for (int i = 0; i < 60; i++)
                {
                    int[] rawFrame = _nes.Frame();
                    DrawImage(rawFrame);
                }

                stopwatch.Stop();

                var ms = stopwatch.ElapsedMilliseconds;
            }
        }

        unsafe void DrawImage(int[] bitmap)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                fixed (int* p = bitmap)
                {
                    var bitmap = new Bitmap(Avalonia.Platform.PixelFormat.Bgra8888, (IntPtr)p, new PixelSize(256, 240), new Vector(), 256 * sizeof(int));
                    this.Content = new Image { Source = bitmap };
                }
            });
        }
    }
}
