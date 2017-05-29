using System;

[Serializable]
public class RedBrickBlock : Block
{

    public RedBrickBlock()
        : base()
    {
        breakable = true;
    }

    public override Tile TexturePosition(Direction direction)
    { 
        Tile tile = new Tile();

        tile.x = 2;
        tile.y = 1;

        return tile;
    }

    protected override string ToName()
    {
        return "RedBrickBlock";
    }
}