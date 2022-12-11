using Newtonsoft.Json.Linq;

namespace JBPPP2.Extensions;

internal static class SerializerExtensions
{
    internal static List<T> ToList<T>(this JArray array)
    {
        return array.Select(item => item.ToObject<T>()!).ToList();
    }
}