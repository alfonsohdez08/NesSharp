using MiNES.CPU;
using MiNES.PPU;
using MiNES.Rom;
using System.Drawing;

namespace MiNES
{
    public class NES
    {
        private readonly Cpu _cpu;
        private readonly Ppu _ppu;

        public Ppu Ppu => _ppu;

        private ulong _masterClock;

        public NES(byte[] gameCartridge)
        {
            iNESParser.ParseNesCartridge(gameCartridge, out Memory cpuMemory, out Memory ppuMemory, out Mirroring mirroring);

            var ppuBus = new PpuBus(ppuMemory, mirroring);
            _ppu = new Ppu(ppuBus);

            var cpuBus  = new CpuBus(cpuMemory, _ppu);
            _cpu = new Cpu(cpuBus);
        }

        private int _ppuCyclesLeftover;

        /// <summary>
        /// Produces/emulates a frame (an image).
        /// </summary>
        /// <returns>A frame (an image).</returns>
        public Bitmap Frame()
        {
            do
            {
                // Finish the ppu cycles leftover
                while(_ppuCyclesLeftover > 0)
                {
                    _ppu.DrawPixel();
                    _ppuCyclesLeftover--;
                }

                if (_ppu.NmiRequested)
                {
                    _cpu.NMI();
                    _ppu.NmiRequested = false;
                }

                int cpuCyclesSpent = _cpu.Step();

                if (_ppu.DmaTriggered)
                {
                    byte[] oam = _cpu.GetOam(_ppu.OamCpuPage);
                    _ppu.SetOam(oam);
                    //for (int i = 0; i < oam.Length; i++)
                    //{
                    //    _ppu.SetOamData(oam[i]);
                    //}

                    // Condition when DMA is requested; if cycles number is odd, add an additional cycle
                    cpuCyclesSpent += (cpuCyclesSpent % 2 == 0 ? 513 : 514);
                    _ppu.DmaTriggered = false;
                }

                for (int ppuCycles = 0; ppuCycles < cpuCyclesSpent * 3; ppuCycles++)
                {
                    _ppu.DrawPixel();
                    if(_ppu.FrameBuffer != null)
                    {
                        _ppuCyclesLeftover = (cpuCyclesSpent * 3) - ppuCycles; // Is this substraction accurate?
                        break;
                    }

                }

            } while (_ppu.FrameBuffer == null);

            Bitmap frame = (Bitmap)_ppu.FrameBuffer.Clone();
            _ppu.DisposeBuffer();
            //_ppu.ResetFrame();

            return frame;
        }

        public byte[][] GetNametable0() => _ppu.GetNametable0();

        public byte[][] GetNametable2() => _ppu.GetNametable2();

        public Tile[] GetBackgroundTiles() => _ppu.BackgroundTiles;

        public Color[] GetPalettes() => _ppu.GetPalettes();
    }
}
