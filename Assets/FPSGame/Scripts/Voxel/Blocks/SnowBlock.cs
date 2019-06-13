using System;

[Serializable]
public class SnowBlock : Block
{

    public SnowBlock()
        : base()
    {
        breakable = true;
    }

    public override Tile TexturePosition(Direction direction)
    {
        Tile tile = new Tile();

        tile.x = 2;
        tile.y = 2;

        return tile;
    }

    protected override string ToName()
    {
        return "SnowBlock";
    }
}