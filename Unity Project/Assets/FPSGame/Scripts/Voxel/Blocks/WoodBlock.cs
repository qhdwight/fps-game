using System;

[Serializable]
public class WoodBlock : Block {

    public WoodBlock() : base()
    {
        breakable = true;
    }

    public override Tile TexturePosition(Direction direction)
    {
        Tile tile = new Tile();

        tile.x = 3;
        tile.y = 2;

        return tile;
    }

    protected override string ToName()
    {
        return "WoodBlock";
    }
}
