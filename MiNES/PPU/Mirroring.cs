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
        public uint Parse(uint address)
        {
            // Bits 11 and 13 controls the base address for Horizontal mirroring
            uint mask = 0x2800;
            uint baseAddress = address & mask;
            uint offset = address & 0x03FF;

            return baseAddress + offset;
        }
    }

    class VerticalMirroringParser : INametableAddressParser
    {
        public uint Parse(uint address)
        {
            // Bits 10 and 13 controls the base address for Vertical mirroring
            uint mask = 0x2400;
            uint baseAddress = address & mask;
            uint offset = address & 0x03FF;

            return baseAddress + offset;
        }
    }
}
