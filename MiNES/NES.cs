using MiNES.Rom;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MiNES
{
    public class NES
    {
        private readonly Cpu _cpu;
        private readonly Ppu _ppu;

        public Ppu Ppu => _ppu;

        public NES(byte[] gameCartridge)
        {
            iNESParser.ParseNesCartridge(gameCartridge, out Memory cpuMemory, out Memory ppuMemory, out Mirroring mirroring);

            var ppuBus = new PpuBus(ppuMemory, mirroring);
            _ppu = new Ppu(ppuBus);

            var cpuBus  = new CpuBus(cpuMemory, _ppu);
            _cpu = new Cpu(cpuBus);
        }

        /// <summary>
        /// Produces/emulates a frame (an image).
        /// </summary>
        /// <returns>A frame (an image).</returns>
        public Bitmap Frame()
        {
            do
            {
                if (_ppu.NmiRequested)
                {
                    _cpu.NMI();
                    _ppu.NmiRequested = false;
                }

                byte cpuCyclesSpent = _cpu.Step();
                for (int ppuCycles = 0; ppuCycles < cpuCyclesSpent * 3; ppuCycles++)
                {
                    _ppu.Draw();
                }

            } while (_ppu.FrameBuffer == null);

            Bitmap frame = (Bitmap)_ppu.FrameBuffer.Clone();
            _ppu.DisposeBuffer();
            //_ppu.ResetFrame();

            return frame;
        }

        public byte[][] GetNametable() => _ppu.GetNametable();
    }
}
