using System;

namespace MiNES.PPU
{
    public enum Mirroring
    {
        Horizontal = 0,
        Vertical
    }

    static class NametableMirroringResolver
    {
        public static INametableAddressParser GetAddressParser(Mirroring mirroring)
        {
            INametableAddressParser addressParser;

            switch(mirroring)
            {
                case Mirroring.Horizontal:
                    addressParser = new HorizontalMirroringParser();
                    break;
                case Mirroring.Vertical:
                    addressParser = new VerticalMirroringParser();
                    break;
                default:
                    throw new NotImplementedException();
            }

            return addressParser;
        }
    }

    interface INametableAddressParser
    {
        uint Parse(uint address);
    }

    class HorizontalMirroringParser : INametableAddressParser
    {
        // Bits 11 and 13 controls the base address for Horizontal mirroring
        private const uint Mask = 0x2800;

        public uint Parse(uint address)
        {
            uint baseAddress = address & Mask;
            uint offset = address & 0x03FF;

            return baseAddress + offset;
        }
    }

    class VerticalMirroringParser : INametableAddressParser
    {
        // Bits 10 and 13 controls the base address for Vertical mirroring
        private const uint Mask = 0x2400;

        public uint Parse(uint address)
        {
            uint baseAddress = address & Mask;
            uint offset = address & 0x03FF;

            return baseAddress + offset;
        }
    }
}
