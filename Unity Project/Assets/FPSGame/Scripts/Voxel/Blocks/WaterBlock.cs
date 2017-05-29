using System;

[Serializable]
class WaterBlock : Block
{
    public WaterBlock()
        : base()
    {
        breakable = true;
        isWater = true;
    }

    public override Tile TexturePosition(Direction direction)
    {
        Tile tile = new Tile();

        tile.x = 0;
        tile.y = 0;

        return tile;
    }

    protected override string ToName()
    {
        return "WaterBlock";
    }
}
