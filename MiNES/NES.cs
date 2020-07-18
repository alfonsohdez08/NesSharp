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

        //private ulong _masterClock;

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

                int cpuCyclesSpent;
                int totalPpuCycles;
                try
                {
                    cpuCyclesSpent = _cpu.Step() + _cpuCyclesLeftOver;
                    totalPpuCycles = (cpuCyclesSpent * 3) + _ppuCyclesLeftOver;
                }
                finally
                { 
                    _cpuCyclesLeftOver = 0;
                    _ppuCyclesLeftOver = 0;
                }
                
                for (int ppuCycles = 0; ppuCycles < totalPpuCycles; ppuCycles++)
                {
                    _ppu.Step();
                    //if(_ppu.FrameBuffer != null)
                    //{
                    //    ppuCycles++;
                    //    _ppuCyclesLeftover = (totalPpuCycles - ppuCycles) % 3;
                    //    _cpuCyclesLeftOver = (totalPpuCycles - ppuCycles)/3;

                    //    break;
                    //}

                }

            } while (null == null);

            Bitmap frame = null;
            //_ppu.DisposeBuffer();
            //_ppu.ResetFrame();

            return frame;
        }


        public int[] EmulateFrame()
        {
            do
            {
                if (_ppu.NmiRequested)
                {
                    // TODO: should i count the cycles spent by the NMI interrupt?
                    _cpu.NMI();
                    _ppu.NmiRequested = false;
                }

                int totalPpuCycles;
                try
                {
                    int cpuCyclesSpent = _cpu.Step() + _cpuCyclesLeftOver;
                    totalPpuCycles = (cpuCyclesSpent * 3) + _ppuCyclesLeftOver;
                }
                finally
                {
                    _cpuCyclesLeftOver = 0;
                    _ppuCyclesLeftOver = 0;
                }

                for (int ppuCycles = 0; ppuCycles < totalPpuCycles; ppuCycles++)
                {
                    _ppu.Step();
                    if (_ppu.IsFrameCompleted) // circuit breaker
                    {
                        ppuCycles++;
                        _ppuCyclesLeftOver = (totalPpuCycles - ppuCycles) % 3;
                        _cpuCyclesLeftOver = (totalPpuCycles - ppuCycles) / 3;

                        break;
                    }

                }

            } while (!_ppu.IsFrameCompleted);

            _ppu.ResetFrameRenderingStatus();

            return _ppu.Frame;
        }

        public byte[][] GetNametable0() => _ppu.GetNametable0();

        public byte[][] GetNametable2() => _ppu.GetNametable2();

        public Tile[] GetBackgroundTiles() => _ppu.BackgroundTiles;

        public Color[] GetPalettes() => _ppu.GetPalettes();
    }
}
