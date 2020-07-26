using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace MiNES.Windows.Client
{
    public partial class EmulatorUI : Form
    {
        private byte[] _cartridgeRom;
        private NES _nes;
        private int[] _currentFrame;
        private PictureBox _screen;

        private delegate void PaintScreen();

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

            _screen = new PictureBox();
            _screen.Width = 256;
            _screen.Height = 240;
            Controls.Add(_screen);
        }

        private void OpenRomSelectionDialog(object o, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "C:\\";
                openFileDialog.Filter = "nes files (*.nes)|*.nes|All files (*.*)|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (var fileStream = new FileStream(openFileDialog.FileName, FileMode.Open))
                    {
                        _cartridgeRom = new byte[fileStream.Length];
                        fileStream.Read(_cartridgeRom, 0, (int)fileStream.Length);
                    }

                    StartEmulation();
                }
            }
        }

        private void StartEmulation()
        {
            _nes = new NES(_cartridgeRom);
            new TaskFactory().StartNew(RunGame, TaskCreationOptions.LongRunning);
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

        private void EmulatorUI_Load(object sender, EventArgs e)
        {

        }
    }
}
