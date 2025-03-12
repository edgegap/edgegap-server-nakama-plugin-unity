using System.Collections;
using Edgegap.NakamaServersPlugin;
using UnityEngine;

public class SimpleTestHandler : Edgegap.NakamaServersPlugin.ServerHandler<InstanceBaseMetadata>
{
    public bool DontDestroyOnSceneLoad = true;
    public bool DeleteInstanceOnQuit = true;

    private ServerAgent<InstanceBaseMetadata> ServerAgent;

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
        AddUser("17338cf7-4528-4a44-b91b-2309f927c4f4");
        RemoveUser("jakub");
        yield return new WaitForSeconds(120f);
        RemoveUser("max");
        ServerAgent.StopInstance();
    }

    // end test

    public void Awake()
    {
        // this will allow players to connect, optionally call later when finished loading
        AcceptConnections();

        if (DontDestroyOnSceneLoad)
        {
            DontDestroyOnLoad(this);
        }
    }

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

    public void AcceptConnections()
    {
        ServerAgent = new ServerAgent<InstanceBaseMetadata>(this);

        try
        {
            ServerAgent.Initialize();
        }
        catch
        {
            // todo handle missing environment variables
        }
    }

    // called by ServerAgent.cs after each Nakama Instance Event processed

    public override void OnInstanceEvent(
        InstanceEventDTO<InstanceBaseMetadata> payload,
        string response,
        string error = null
    )
    {
        // todo hook up external server monitoring in case Nakama is unresponsive
    }

    public void AddUser(string userID)
    {
        if (ServerAgent is null)
        {
            throw new System.Exception("Can't add users before Server Agent initialized.");
        }
        ServerAgent.AddUser(userID);
    }

    public void RemoveUser(string userID)
    {
        if (ServerAgent is null)
        {
            throw new System.Exception("Can't remove users before Server Agent initialized.");
        }
        ServerAgent.RemoveUser(userID);
    }
}
