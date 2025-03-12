using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Edgegap.NakamaServersPlugin
{
    public class InstanceBaseMetadata
    {
        [JsonProperty("game_server")]
        public GameServerMetadata GameServer;

        public InstanceBaseMetadata()
        {
            GameServer = new GameServerMetadata();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class GameServerMetadata
    {
        [JsonProperty("ARBITRIUM_REQUEST_ID")]
        public string DeploymentID;

        [JsonProperty("ARBITRIUM_PUBLIC_IP")]
        public string PublicIP;

        [JsonProperty("ARBITRIUM_DEPLOYMENT_TAGS")]
        public List<string> Tags;

        [JsonProperty("ARBITRIUM_DEPLOYMENT_LOCATION")]
        public LocationDTO Location;

        [JsonProperty("ARBITRIUM_PORTS_MAPPING")]
        public PortMappingEnvDTO PortMapping;

        public GameServerMetadata()
        {
            DeploymentID = Environment.GetEnvironmentVariable("ARBITRIUM_REQUEST_ID");
            PublicIP = Environment.GetEnvironmentVariable("ARBITRIUM_PUBLIC_IP");
            Tags = TryDeserialize<List<string>>("ARBITRIUM_DEPLOYMENT_TAGS");
            Location = TryDeserialize<LocationDTO>("ARBITRIUM_DEPLOYMENT_LOCATION");
            PortMapping = TryDeserialize<PortMappingEnvDTO>("ARBITRIUM_PORTS_MAPPING");
        }

        public static T TryDeserialize<T>(string key)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(Environment.GetEnvironmentVariable(key));
            }
            catch
            {
                return default;
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class LocationDTO
    {
        [JsonProperty("city")]
        public string City;

        [JsonProperty("country")]
        public string Country;

        [JsonProperty("continent")]
        public string Continent;

        [JsonProperty("administrative_division")]
        public string AdministrativeDivision;

        [JsonProperty("timezone")]
        public string Timezone;

        [JsonProperty("latitude")]
        public float Latitude;

        [JsonProperty("longitude")]
        public float Longitude;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class PortMappingEnvDTO
    {
        [JsonProperty("ports")]
        public Dictionary<string, PortMappingDTO> Ports;
    }

    public class PortMappingDTO
    {
        [JsonProperty("internal")]
        public string Internal;

        [JsonProperty("external")]
        public string External;

        [JsonProperty("protocol")]
        public string Protocol;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
