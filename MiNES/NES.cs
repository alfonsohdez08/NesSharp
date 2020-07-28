using MiNES.CPU;
using MiNES.PPU;

namespace MiNES
{
    public class NES
    {
        private readonly Cpu _cpu;
        private readonly Ppu _ppu;

        private int _ppuCyclesLeftOver;

        private int _cpuCyclesLeftOver;

        public NES(Cartridge gameCartridge, Joypad joypad)
        {
            var ppuBus = new PpuBus(gameCartridge.CharacterRom, gameCartridge.GameMirroring);
            _ppu = new Ppu(ppuBus);

            var cpuBus  = new CpuBus(gameCartridge.ProgramRom, _ppu, joypad);
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

                int cpuCyclesSpent = _cpu.Step();
                int totalPpuCycles = cpuCyclesSpent * 3;

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

        //public byte[][] GetNametable0() => _ppu.GetNametable0();

        //public byte[][] GetNametable2() => _ppu.GetNametable2();

        //public Tile[] GetBackgroundTiles() => _ppu.BackgroundTiles;

        //public Color[] GetPalettes() => _ppu.GetPalettes();
    }
}
