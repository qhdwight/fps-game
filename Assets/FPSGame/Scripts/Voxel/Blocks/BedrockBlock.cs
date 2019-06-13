using System;

[Serializable]
public class BedrockBlock : Block
{
    public BedrockBlock()
        : base()
    {
        breakable = false;
    }

    public override Tile TexturePosition(Direction direction)
    {
        Tile tile = new Tile();

        tile.x = 1;
        tile.y = 1;

        return tile;
    }

    protected override string ToName()
    {
        return "BedrockBlock";
    }
}
