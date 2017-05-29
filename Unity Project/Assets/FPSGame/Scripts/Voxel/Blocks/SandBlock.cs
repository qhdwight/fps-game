using System;

[Serializable]
public class SandBlock : Block
{

    public SandBlock()
        : base()
    {
        breakable = true;
    }

    public override Tile TexturePosition(Direction direction)
    {
        Tile tile = new Tile();

        tile.x = 0;
        tile.y = 1;

        return tile;
    }

    protected override string ToName()
    {
        return "SandBlock";
    }
}