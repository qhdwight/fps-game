using System;

[Serializable]
public class StoneBlock : Block {

    public StoneBlock()
        : base()
    {
        breakable = true;
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
        return "StoneBlock";
    }
}
