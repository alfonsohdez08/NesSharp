using MiNES.CPU;
using MiNES.PPU;
using System;

namespace MiNES
{
    public class NES
    {
        private readonly Cpu _cpu;
        private readonly Ppu _ppu;
        private readonly Joypad _joypad;

        private int _ppuCyclesLeftOver;

        private int _cpuCyclesLeftOver;

        internal Cpu Cpu => _cpu;
        internal Ppu Ppu => _ppu;
        internal Joypad Joypad => _joypad;

        public NES(Cartridge gameCartridge, Joypad joypad)
        {
            _joypad = joypad;

            var ppuBus = new PpuBus(gameCartridge.CharacterRom, gameCartridge.GameMirroring);
            _ppu = new Ppu(ppuBus, this);

            //var cpuBus  = new CpuBus(gameCartridge.ProgramRom, _ppu, joypad);
            var cpuBus = new CpuBus(gameCartridge.ProgramRom, this);
            _cpu = new Cpu(cpuBus);
        }

        private const int CpuCyclesInVbl = 2273;

        private int _cpuMasterClockTicks = 0;


        public int[] Frame()
        {
            int cpuClockTicks;

            do
            {
                // Run both CPU and PPU
                cpuClockTicks = _cpu.Step();
                for (int ppuClockTicks = 0; ppuClockTicks < cpuClockTicks * 3; ppuClockTicks++)
                    _ppu.Step();

            } while (!_ppu.IsIdle);

            cpuClockTicks = (_ppu.Cycles -  2)/3; // get the leftover of the ppu ticks once has been idle
            do
            {
                cpuClockTicks += _cpu.Step();
            } while (cpuClockTicks < CpuCyclesInVbl);

            _ppu.ResetFrameRenderingStatus();

            return _ppu.Frame;

            //int cpuClockTicks;
            //_cpuMasterClockTicks += (_ppu.SkipIdleTick ? 5 : 0);

            //do
            //{
            //    // Run both CPU and PPU
            //    cpuClockTicks = _cpu.Step();
            //    for (int ppuClockTicks = 0; ppuClockTicks < cpuClockTicks * 3; ppuClockTicks++)
            //        _ppu.Step();

            //    _cpuMasterClockTicks += cpuClockTicks * 15;
            //} while (_cpuMasterClockTicks < Ppu.MasterClockTicks);

            // stabilize the ppu

            // Run CPU for finish the frame itself


            //int cpuCyclesInVbl = 0;

            //do
            //{
            //    /*
            //        TODO: Get rid of this and see how i can actually invoke the interruption from the PPU.
            //        Also, when invoking the DMA, see how to do this directly and account the cycles elapsed, and just
            //        let the cpu.Step() method only for execute program CPU instructions                
            //     */
            //    int cpuCyclesSpent = _cpu.Step();
            //    int totalPpuCycles = cpuCyclesSpent * 3;

            //    if (!_ppu.IsIdle)
            //    {
            //        for (int ppuCycles = 0; ppuCycles < totalPpuCycles; ppuCycles++)
            //        {
            //            _ppu.Step();
            //            if (_ppu.IsIdle)
            //            {
            //                ppuCycles++;
            //                cpuCyclesInVbl = (totalPpuCycles - ppuCycles) / 3;
            //                break;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        cpuCyclesInVbl += cpuCyclesSpent;
            //    }

            //} while (cpuCyclesInVbl < CpuCyclesInVbl);

            //_ppu.ResetFrameRenderingStatus();

            //return _ppu.Frame;
        }

        internal void TriggerNmi() => _cpu.ExecuteNMI();
    }
}
