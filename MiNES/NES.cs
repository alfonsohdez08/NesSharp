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
        private readonly CpuBus _cpuBus;
        private readonly PpuBus _ppuBus;

        public Ppu Ppu => _ppu;

        public NES(byte[] gameCartridge)
        {
            iNESParser.ParseNesCartridge(gameCartridge, out Memory cpuMemory, out Memory ppuMemory);

            _cpuBus = new CpuBus(cpuMemory);
            _ppuBus = new PpuBus(ppuMemory);

            _cpu = new Cpu(_cpuBus);
            _ppu = new Ppu(_ppuBus, _cpuBus);
        }

        /// <summary>
        /// Produces/emulates a frame (an image).
        /// </summary>
        /// <returns>A frame (an image).</returns>
        public Bitmap Frame()
        {
            do
            {
                byte cpuCyclesSpent = _cpu.Step();
                for (int ppuCycles = 0; ppuCycles < cpuCyclesSpent * 3; ppuCycles++)
                    _ppu.Draw();

            } while (!_ppu.IsFrameCompleted);

            return _ppu.Frame;
        }
    }
}
