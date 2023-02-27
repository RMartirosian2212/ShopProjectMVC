using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ShopProject_Utility;

public static class SessionExtensions
{
    public static void Set<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T Get<T>(this ISession session, string key)
    {
        var value = session.Get(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }
}