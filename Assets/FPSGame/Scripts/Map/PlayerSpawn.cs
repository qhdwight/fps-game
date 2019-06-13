using System;
using UnityEngine;

[Serializable]
public class PlayerSpawn {

    public PlayerSpawn(WorldPosition pos, Quaternion rot, Team team)
    {
        this.rot = new MyQuaternion(rot);
        this.pos = pos;
        this.team = team;
    }

    public WorldPosition pos;
    public MyQuaternion rot;
    public Team team;
}