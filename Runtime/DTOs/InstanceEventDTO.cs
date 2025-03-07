using Newtonsoft.Json;

namespace Edgegap.NakamaServersPlugin
{
    public class InstanceEventDTO<T>
    {
        [JsonProperty("instance_id")]
        public string InstanceID;

        [JsonProperty("action")]
        public string InstanceAction;

        [JsonProperty("message")]
        public string Message;

        [JsonProperty("metadata")]
        public T Metadata;

        public InstanceEventDTO(string instanceID, string action, string message, T metadata)
        {
            InstanceID = instanceID;
            InstanceAction = action;
            Message = message;
            Metadata = metadata;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
