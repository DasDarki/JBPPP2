using JBPPP2.Model;
using Newtonsoft.Json;

namespace JBPPP2.Controllers;

internal class DataController
{
    private const string GameDataUrl = "https://raw.githubusercontent.com/DerErizzle/JBPPP2/data/game_data.json";
    
    [Controller("GetGames")]
    internal static void GetGames(Window window, string rid)
    {
        var result = GetData<Dictionary<string, List<Game>>>(GameDataUrl);
        var games = result?["games"] ?? new List<Game>();
        
        window.SendResult(rid, games);
    }

    internal static string GetDataPath()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        path = Path.Combine(path, "JBPPP2");
        Directory.CreateDirectory(path);
        return path;
    }

    private static T? GetData<T>(string url)
    {
        using var client = new HttpClient();
        var response = client.GetAsync(url).GetAwaiter().GetResult();
        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        return JsonConvert.DeserializeObject<T>(json);
    }
}