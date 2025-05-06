using Newtonsoft.Json;
using System.IO;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Helpers
{
    public static class JsonHelper
    {
        public static T LoadFromFile<T>(string path)
        {
            if (!File.Exists(path)) return default;
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static void SaveToFile<T>(string path, T obj)
        {
            var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }
}