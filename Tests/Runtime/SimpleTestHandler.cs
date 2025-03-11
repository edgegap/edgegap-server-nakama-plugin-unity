using System.Collections;
using Edgegap.NakamaServersPlugin;
using UnityEngine;

public class EdgegapServerNakamaHandler
    : Edgegap.NakamaServersPlugin.ServerHandler<InstanceBaseMetadata>
{
    public string AuthenticationToken = "no-auth";
    public bool DontDestroyOnSceneLoad = true;
    public bool DeleteInstanceOnQuit = true;

    private ServerAgent<InstanceBaseMetadata> ServerAgent;

    public void Awake()
    {
        ServerAgent = new ServerAgent<InstanceBaseMetadata>(this, AuthenticationToken);
        ServerAgent.Initialize();

        if (DontDestroyOnSceneLoad)
        {
            DontDestroyOnLoad(this);
        }
    }

    // test
    public void Start()
    {
        StartCoroutine(TestMe());
    }

    IEnumerator TestMe()
    {
        AddUser("sam");
        yield return new WaitForSeconds(2f);
        AddUser("max");
        RemoveUser("sam");
        RemoveUser("jakub");
        yield return new WaitForSeconds(2f);
        RemoveUser("max");
        ServerAgent.StopInstance();
    }

    // end test

    public void FixedUpdate()
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

    // called by ServerAgent.cs after each Nakama Instance Event processed

    public override void OnInstanceEvent(
        InstanceEventDTO<InstanceBaseMetadata> payload,
        InstanceEventResponseDTO response,
        string error = null
    )
    {
        // todo hook up external server monitoring in case Nakama is unresponsive
    }

    public void AddUser(string userID)
    {
        ServerAgent.AddUser(userID);
    }

    public void RemoveUser(string userID)
    {
        ServerAgent.RemoveUser(userID);
    }
}
