using JBPPP2.Controllers;
using Newtonsoft.Json;

namespace JBPPP2.Model;

internal class Config
{
    internal static Config? Instance { get; private set; }
    
    [JsonProperty("launchers")] 
    internal List<Launcher> Launchers { get; set; } = new();

    [JsonProperty("skipVersionCheck")]
    internal bool SkipVersionCheck { get; set; } = false;

    internal void Save()
    {
        var path = Path.Combine(DataController.GetDataPath(), "config.json");
        File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
    }

    internal static void Load()
    {
        var path = Path.Combine(DataController.GetDataPath(), "config.json");

        if (File.Exists(path))
        {
            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
            if (config != null)
            {
                Instance = config;
            }
        }

        Instance = new Config();
        Instance.Save();
    }
}