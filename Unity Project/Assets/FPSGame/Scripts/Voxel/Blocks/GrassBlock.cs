﻿using System;

[Serializable]
public class GrassBlock : Block {

    public GrassBlock()
        : base()
    {
        breakable = true;
    }

    public override Tile TexturePosition(Direction direction)
    {
        Tile tile = new Tile();

        switch (direction)
        {
            case Direction.up:
                tile.x = 2;
                tile.y = 0;
                return tile;
            case Direction.down:
                tile.x = 1;
                tile.y = 0;
                return tile;
        }

        tile.x = 1;
        tile.y = 0;

        return tile;
    }

    protected override string ToName()
    {
        return "GrassBlock";
    }
}