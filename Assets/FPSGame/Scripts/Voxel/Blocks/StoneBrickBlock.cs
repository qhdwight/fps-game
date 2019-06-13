using System;

[Serializable]
public class StoneBrickBlock : Block {

    public StoneBrickBlock()
        : base()
    {
        breakable = true;
    }

    public override Tile TexturePosition(Direction direction)
    {
        Tile tile = new Tile();

        tile.x = 3;
        tile.y = 1;

        return tile;
    }

    protected override string ToName()
    {
        return "StoneBrickBlock";
    }
}