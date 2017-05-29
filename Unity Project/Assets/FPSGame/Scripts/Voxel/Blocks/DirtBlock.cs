using System;

[Serializable]
public class DirtBlock : Block
{

    public DirtBlock()
        : base()
    {
        breakable = true;
    }

    public override Tile TexturePosition(Direction direction)
    {
        Tile tile = new Tile();

        tile.x = 1;
        tile.y = 0;

        return tile;
    }

    protected override string ToName()
    {
        return "DirtBlock";
    }
}