using System.Text.Json.Serialization;
using System.Windows.Media;
using System;

namespace Cipher
{
    public enum ModStatus
    {
        Ready = 0,
        Updated = 1,
        Updating = 2,
        Missing = 3,
        Error = 4
    }

    public class ModItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string GameTask { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public string DllPath { get; set; } = "";
        public ModStatus Status { get; set; } = ModStatus.Ready;

        [JsonIgnore]
        public string DisplayName => $"📦 {Name}";

        [JsonIgnore]
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case ModStatus.Updated: return "✅ Updated";
                    case ModStatus.Ready: return "🟢 Ready";
                    case ModStatus.Updating: return "⏳ Updating...";
                    case ModStatus.Missing: return "❌ Missing";
                    case ModStatus.Error: return "⚠️ Error";
                    default: return "✅ Updated";
                }
            }
        }

        [JsonIgnore]
        public Brush StatusColor
        {
            get
            {
                switch (Status)
                {
                    case ModStatus.Updated: return new SolidColorBrush(Color.FromRgb(166, 227, 161));
                    case ModStatus.Ready: return new SolidColorBrush(Color.FromRgb(137, 180, 250));
                    case ModStatus.Updating: return new SolidColorBrush(Color.FromRgb(249, 226, 175));
                    case ModStatus.Missing: return new SolidColorBrush(Color.FromRgb(243, 139, 168));
                    case ModStatus.Error: return new SolidColorBrush(Color.FromRgb(243, 139, 168));
                    default: return new SolidColorBrush(Color.FromRgb(166, 227, 161));
                }
            }
        }

        [JsonIgnore]
        public string ProcessStatusText { get; set; } = "";

        [JsonIgnore]
        public Brush ProcessColor
        {
            get
            {
                if (string.IsNullOrEmpty(ProcessStatusText))
                    return new SolidColorBrush(Colors.Transparent);
                return new SolidColorBrush(Color.FromRgb(137, 180, 250));
            }
        }

        [JsonIgnore]
        public bool IsGameRunning { get; set; } = false;
    }
}