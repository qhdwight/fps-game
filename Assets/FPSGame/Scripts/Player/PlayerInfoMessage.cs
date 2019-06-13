using UnityEngine.Networking;

public class PlayerInfoMessage : MessageBase {

    public string username;

    public PlayerInfoMessage(string username)
    {
        this.username = username;
    }

    public override void Deserialize(NetworkReader reader)
    {
        username = reader.ReadString();
    }

    public override void Serialize(NetworkWriter writer)
    {
        writer.Write(username);
    }
}
