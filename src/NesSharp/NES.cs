using NesSharp.CPU;
using NesSharp.PPU;
using System;
using System.Collections.ObjectModel;

namespace NesSharp
{
    internal delegate void DMA(byte page, byte[] cpuRam);
    internal delegate void NmiTrigger();

    public class NES
    {
        private readonly Cpu _cpu;
        private readonly Ppu _ppu;

        public const int MasterClockCyclesInFrame = 341 * 262 * 5;
        public const int MasterClockCyclesBeforeVbl = ((341 * 242) + 2) * 5;

        public NES(Cartridge gameCartridge, Joypad joypad)
        {
            var ppuBus = new PpuBus(gameCartridge.CharacterRom, gameCartridge.GameMirroring);
            _ppu = new Ppu(ppuBus, new NmiTrigger(() => _cpu.NMI()));

            var cpuBus = new CpuBus(gameCartridge.ProgramRom, _ppu, joypad, new DMA(PerformDma), () => _cpu.MasterClockCycles);
            _cpu = new Cpu(cpuBus);
        }

        public int[] Frame()
        {
            _cpu.RunUpTo(MasterClockCyclesBeforeVbl);
            _ppu.RunUpTo(_cpu.MasterClockCycles);

            _cpu.RunUpTo(MasterClockCyclesInFrame);
            _ppu.RunUpTo(_cpu.MasterClockCycles);

            _cpu.MasterClockCycles -= MasterClockCyclesInFrame;
            _ppu.MasterClockCycles -= MasterClockCyclesInFrame;

            return _ppu.Frame;
        }

        internal void PerformDma(byte page, byte[] cpuRam)
        {
            ushort address = (ushort)(page << 8);
            for (int i = 0; i < 256; i++)
                _ppu.OamData = cpuRam[address++];

            _cpu.AddCycles(_cpu.CyclesElapsed % 2 == 0 ? 513 : 514);
        }
    }
}
