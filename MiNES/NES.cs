using MiNES.Rom;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiNES
{
    class NES
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
    }
}
