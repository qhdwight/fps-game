using System;

[Serializable]
public class FlagInfo
{
    public FlagInfo(WorldPosition pos, Team team)
    {
        this.pos = pos;
        this.team = team;
    }

    public WorldPosition pos;
    public Team team;
}
