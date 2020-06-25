namespace MiNES
{


    public partial class Ppu
    {
        class Tile
        {
            private readonly byte[][] _grid;

            public Tile()
            {
                _grid = new byte[8][];
                for (int i = 0; i < _grid.Length; i++)
                    _grid[i] = new byte[8];
            }

            public void SetPixel(int x, int y, byte color)
            {
                _grid[x][y] = color;
            }

            public byte GetPixel(int x, int y)
            {
                return _grid[y][x];
            }
        }
    }
}
