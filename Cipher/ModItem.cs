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

    // NEW: Process status enum
    public enum ProcessStatus
    {
        NotFound = 0,
        Found = 1
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
                    case ModStatus.Updated: return "✅ Up to Date";
                    case ModStatus.Ready: return "🟢 Download Ready";
                    case ModStatus.Updating: return "⏳ Updating...";
                    case ModStatus.Missing: return "❌ Missing";
                    case ModStatus.Error: return "⚠️ Error";
                    default: return "✅ Up to Date";
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
                    case ModStatus.Updated: return new SolidColorBrush(Color.FromRgb(100, 182, 250));
                    case ModStatus.Ready: return new SolidColorBrush(Color.FromRgb(137, 180, 250));
                    case ModStatus.Updating: return new SolidColorBrush(Color.FromRgb(249, 226, 175));
                    case ModStatus.Missing: return new SolidColorBrush(Color.FromRgb(243, 139, 168));
                    case ModStatus.Error: return new SolidColorBrush(Color.FromRgb(243, 139, 168));
                    default: return new SolidColorBrush(Color.FromRgb(166, 227, 161));
                }
            }
        }

        // NEW: Process status properties (replace the old ones)
        private ProcessStatus _processState = ProcessStatus.NotFound;

        [JsonIgnore]
        public ProcessStatus ProcessState
        {
            get => _processState;
            set
            {
                if (_processState != value)
                {
                    _processState = value;
                    // Update the old properties for backward compatibility
                    IsGameRunning = (value == ProcessStatus.Found);
                    ProcessStatusText = (value == ProcessStatus.Found) ? "✅ Found! Game is Running" : "🔍 Searching for Running Process";
                }
            }
        }

        // Keep these for backward compatibility with existing code
        [JsonIgnore]
        public string ProcessStatusText { get; set; } = "🔍 Searching for Running Process";

        [JsonIgnore]
        public Brush ProcessColor
        {
            get
            {
                if (IsGameRunning)
                    return new SolidColorBrush(Color.FromRgb(166, 227, 161)); // Green
                return new SolidColorBrush(Color.FromRgb(243, 139, 168)); // Red
            }
        }

        [JsonIgnore]
        public bool IsGameRunning { get; set; } = false;
    }
}