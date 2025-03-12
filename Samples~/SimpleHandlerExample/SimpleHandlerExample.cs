using Edgegap.NakamaServersPlugin;

public class SimpleHandlerExample : Edgegap.NakamaServersPlugin.ServerHandler<InstanceBaseMetadata>
{
    public bool DontDestroyOnSceneLoad = true;
    public bool DeleteInstanceOnQuit = true;

    private ServerAgent<InstanceBaseMetadata> ServerAgent;

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
