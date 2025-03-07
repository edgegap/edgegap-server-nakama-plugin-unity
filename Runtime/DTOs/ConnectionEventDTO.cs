using System.Collections.Generic;
using Newtonsoft.Json;

namespace Edgegap.NakamaServersPlugin
{
    public class ConnectionEventDTO
    {
        [JsonProperty("instance_id")]
        public string InstanceID;

        [JsonProperty("connections")]
        public List<string> UserIDs;

        public ConnectionEventDTO(string instanceID, List<string> userIDs)
        {
            InstanceID = instanceID;
            UserIDs = userIDs;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
