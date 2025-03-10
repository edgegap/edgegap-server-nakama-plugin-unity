using Edgegap.NakamaServersPlugin;
using UnityEngine;

public class EdgegapServerNakamaHandler : MonoBehaviour
{
    public string AuthenticationToken = "no-auth";
    public bool DontDestroyOnSceneLoad = true;
    public bool DeleteInstanceOnQuit = true;

    public ServerAgent<InstanceBaseMetadata> ServerAgent;

    void Awake()
    {
        ServerAgent = new ServerAgent<InstanceBaseMetadata>(this, AuthenticationToken);
        ServerAgent.Initialize();

        if (DontDestroyOnSceneLoad)
        {
            DontDestroyOnLoad(this);
        }
    }

    void FixedUpdate()
    {
        ServerAgent.FixedUpdate();
    }

    public void OnApplicationQuit()
    {
        if (DeleteInstanceOnQuit)
        {
            ServerAgent.StopInstance();
        }
    }
}
