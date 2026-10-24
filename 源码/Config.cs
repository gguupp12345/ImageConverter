using System.IO;
using System.Text.Json;

namespace 图片转换器
{
    public class Config
    {
        public string SenderEmail { get; set; } = "";
        public string SenderPassword { get; set; } = "";

        public static Config Load(string path)
        {
            if (!File.Exists(path)) return new Config();
            try
            {
                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<Config>(json) ?? new Config();
            }
            catch
            {
                return new Config();
            }
        }

        public void Save(string path)
        {
            string json = JsonSerializer.Serialize(this);
            File.WriteAllText(path, json);
        }
    }
}