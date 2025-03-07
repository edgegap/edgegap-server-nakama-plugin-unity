using Newtonsoft.Json;

namespace Edgegap.NakamaServersPlugin
{
    public class InstanceEventResponseDTO
    {
        [JsonProperty("result")]
        public string ResultWIP;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
