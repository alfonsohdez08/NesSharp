using MiNES.CPU;
using MiNES.PPU;
using MiNES.Rom;
using System;
using System.Drawing;

namespace MiNES
{
    public class NES
    {
        private readonly Cpu _cpu;
        private readonly Ppu _ppu;

        private int _ppuCyclesLeftOver;

        private int _cpuCyclesLeftOver;

        public Ppu Ppu => _ppu;

        public NES(byte[] gameCartridge)
        {
            iNESParser.ParseNesCartridge(gameCartridge, out Memory cpuMemory, out Memory ppuMemory, out Mirroring mirroring);

            var ppuBus = new PpuBus(ppuMemory, mirroring);
            _ppu = new Ppu(ppuBus);

            var cpuBus  = new CpuBus(cpuMemory, _ppu);
            _cpu = new Cpu(cpuBus);
        }

        private const int CpuCyclesInVbl = 2273;

        public int[] Frame()
        {
            int cpuCyclesInVbl = 0;

            do
            {
                if (_ppu.NmiRequested)
                {
                    _cpu.NmiTriggered = true;
                    _ppu.NmiRequested = false;
                }

                int totalPpuCycles;
                int cpuCyclesSpent;
                try
                {
                    //cpuCyclesSpent = _cpu.Step() + _cpuCyclesLeftOver;
                    cpuCyclesSpent = _cpu.Step();
                    totalPpuCycles = cpuCyclesSpent * 3;
                    //totalPpuCycles = (cpuCyclesSpent * 3) + _ppuCyclesLeftOver;
                }
                finally
                {
                    //_cpuCyclesLeftOver = 0;
                    //_ppuCyclesLeftOver = 0;
                }

                if (!_ppu.IsIdle)
                {
                    for (int ppuCycles = 0; ppuCycles < totalPpuCycles; ppuCycles++)
                    {
                        _ppu.Step();
                        if (_ppu.IsIdle)
                        {
                            ppuCycles++;
                            cpuCyclesInVbl = (totalPpuCycles - ppuCycles) / 3;
                            break;
                        }

                        //if (_ppu.IsFrameCompleted) // circuit breaker
                        //{
                        //    ppuCycles++;
                        //    _ppuCyclesLeftOver = (totalPpuCycles - ppuCycles) % 3;
                        //    _cpuCyclesLeftOver = (totalPpuCycles - ppuCycles) / 3;

                        //    break;
                        //}

                    }
                }
                else
                {
                    cpuCyclesInVbl += cpuCyclesSpent;
                }

            } while (cpuCyclesInVbl < CpuCyclesInVbl);

            _ppu.ResetFrameRenderingStatus();

            return _ppu.Frame;
        }

        public byte[][] GetNametable0() => _ppu.GetNametable0();

        public byte[][] GetNametable2() => _ppu.GetNametable2();

        public Tile[] GetBackgroundTiles() => _ppu.BackgroundTiles;

        public Color[] GetPalettes() => _ppu.GetPalettes();
    }
}
