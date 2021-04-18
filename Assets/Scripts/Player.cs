
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public NetworkVariableString Nick = new NetworkVariableString(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone,
        ReadPermission = NetworkVariablePermission.Everyone
    });


    public override void NetworkStart()
    {
        Debug.Log("Network Start");
    }

    public void setNick(string nick)
    {
        Debug.Log("Nick: " + nick);
        Nick.Value = nick;
    }
    /*
    public void Move()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            var randomPosition = GetRandomPositionOnPlane();
            transform.position = randomPosition;
            Position.Value = randomPosition;
        }
        else
        {
            SubmitPositionRequestServerRpc();
        }
    }

    [ServerRpc]
    void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        Position.Value = GetRandomPositionOnPlane();
    }

    static Vector3 GetRandomPositionOnPlane()
    {
        return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
    }

    void Update()
    {
        transform.position = Position.Value;
    }*/
}