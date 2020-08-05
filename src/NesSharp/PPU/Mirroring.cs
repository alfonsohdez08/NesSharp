using System;

namespace NesSharp.PPU
{
    /// <summary>
    /// The nametables mirroring.
    /// </summary>
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
        ushort Parse(ushort address);
    }

    class HorizontalMirroringParser : INametableAddressParser
    {
        // Bits 11 and 13 controls the base address for Horizontal mirroring
        private const ushort Mask = 0x2800;

        public ushort Parse(ushort address) => (ushort)((address & Mask) + (address & 0x03FF));
    }

    class VerticalMirroringParser : INametableAddressParser
    {
        // Bits 10 and 13 controls the base address for Vertical mirroring
        private const ushort Mask = 0x2400;

        public ushort Parse(ushort address) => (ushort)((address & Mask) + (address & 0x03FF));
    }
}
