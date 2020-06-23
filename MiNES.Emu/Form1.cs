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

        public Form1()
        {
            InitializeComponent();
            StartEmulation();
        }

        private void StartEmulation()
        {
            var nes = new NES(donkeyKongRom);

            Task.Factory.StartNew(() =>
            {
                while (true)
                    GameScreen.Image = nes.Frame();
            }, TaskCreationOptions.LongRunning);

        }
    }
}
