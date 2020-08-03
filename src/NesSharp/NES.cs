using NesSharp.CPU;
using NesSharp.PPU;
using System;

namespace NesSharp
{
    internal delegate void DMA(byte page, byte[] cpuRam);
    internal delegate void NmiTrigger();

    public class NES
    {
        //private const int CpuCyclesInVbl = 2273;
        private const int MasterClockTicksFrame = 446710;

        private readonly Cpu _cpu;
        private readonly Ppu _ppu;

        private int _masterClockTicks = 0;

        public NES(Cartridge gameCartridge, Joypad joypad)
        {
            var ppuBus = new PpuBus(gameCartridge.CharacterRom, gameCartridge.GameMirroring);
            _ppu = new Ppu(ppuBus, new NmiTrigger(TriggerNmi));

            var cpuBus = new CpuBus(gameCartridge.ProgramRom, _ppu, joypad, new DMA(PerformDma));
            _cpu = new Cpu(cpuBus);
        }

        public int[] Frame()
        {
            // Run the PPU and CPU together
            for (; !_ppu.IsIdle; _masterClockTicks += 5)
            {
                _ppu.Step();
                if (_masterClockTicks % 15 == 0)
                    _cpu.Step();
            }

            // Just run the CPU for the rest of the VBLANK scanlines (PPU is idle during VBLANK scanlines)
            for (; _masterClockTicks < MasterClockTicksFrame; _masterClockTicks += 15)
                _cpu.Step();

            // The ticks spill (if not 0, then that remaind is ticks for the next frame)
            _masterClockTicks -= MasterClockTicksFrame;

            // Reset the status of the PPU for render the next frame
            _ppu.ResetFrameRenderingStatus();

            return _ppu.Frame;
        }

        internal void TriggerNmi() => _cpu.NMI();

        internal void PerformDma(byte page, byte[] cpuRam)
        {
            ushort address = (ushort)(page << 8);
            for (int i = 0; i < 256; i++)
                _ppu.OamData = cpuRam[address++];

            _cpu.AddDmaCycles(_cpu.CyclesElapsed % 2 == 0 ? 513 : 514);
        }
    }
}
