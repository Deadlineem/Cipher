using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cipher
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<ModItem> _mods = new ObservableCollection<ModItem>();
        private ModItem _selectedMod;
        private string _statusMessage = "✅ Ready to Download";
        private string appDataPath;
        private string modsFilePath;
        private CancellationTokenSource _injectionCancellationToken;
        private Timer _processMonitorTimer;
        private bool _isInitialLoad = true;
        private bool _isRefreshing = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<ModItem> Mods
        {
            get => _mods;
            set { _mods = value; OnPropertyChanged(); }
        }

        public ModItem SelectedMod
        {
            get => _selectedMod;
            set { _selectedMod = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Cipher"
            );
            modsFilePath = Path.Combine(appDataPath, "mods.json");

            InitializeApp();
            StartProcessMonitoring();
        }

        private void InitializeApp()
        {
            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);

            LoadMods();

            // Only add sample data if no mods exist AND it's the first load
            if (Mods.Count == 0 && _isInitialLoad)
            {
                AddSampleData();
                SaveMods();
                _isInitialLoad = false;
            }

            // Auto-update mods on startup ONLY if DLL already exists
            Task.Run(async () => await AutoUpdateMods());

            UpdateModCount();
            StatusMessage = $"✅ Ready - {Mods.Count} mods loaded";
        }

        private async Task AutoUpdateMods()
        {
            await Task.Delay(1000);

            int updatedCount = 0;
            foreach (var mod in Mods)
            {
                // Skip if no download URL
                if (string.IsNullOrEmpty(mod.DownloadUrl))
                    continue;

                // CRITICAL: ONLY auto-update if the DLL already exists
                bool dllExists = !string.IsNullOrEmpty(mod.DllPath) && File.Exists(mod.DllPath);

                if (!dllExists)
                {
                    // Skip auto-update - user needs to download manually first
                    System.Diagnostics.Debug.WriteLine($"ℹ️ Skipping auto-update for {mod.Name} - DLL not downloaded yet");
                    continue;
                }

                // If DLL exists, check for updates
                if (mod.Status != ModStatus.Missing)
                {
                    mod.Status = ModStatus.Updating;
                    Dispatcher.Invoke(() => RefreshModList());

                    try
                    {
                        // Ensure folder exists
                        if (string.IsNullOrEmpty(mod.DllPath))
                        {
                            string gameFolderName = Path.GetFileNameWithoutExtension(mod.GameTask);
                            if (string.IsNullOrWhiteSpace(gameFolderName))
                                gameFolderName = "UnknownGame";

                            string gameFolderPath = Path.Combine(appDataPath, gameFolderName);
                            string fileName = Fetch.GetFileNameFromUrl(mod.DownloadUrl);
                            if (string.IsNullOrWhiteSpace(fileName))
                                fileName = mod.Name.Replace(" ", "") + ".dll";

                            mod.DllPath = Path.Combine(gameFolderPath, fileName);
                        }

                        string folderPath = Path.GetDirectoryName(mod.DllPath);
                        if (!string.IsNullOrEmpty(folderPath) && !Directory.Exists(folderPath))
                            Directory.CreateDirectory(folderPath);

                        bool success = await Fetch.DownloadFileAsync(mod.DownloadUrl, mod.DllPath);

                        if (success)
                        {
                            if (Fetch.IsFileBlockedByAntivirus(mod.DllPath))
                            {
                                mod.Status = ModStatus.Error;
                            }
                            else
                            {
                                mod.Status = ModStatus.Updated;
                                updatedCount++;
                            }
                        }
                        else
                        {
                            mod.Status = ModStatus.Error;
                        }
                    }
                    catch
                    {
                        mod.Status = ModStatus.Error;
                    }

                    Dispatcher.Invoke(() => RefreshModList());
                }
            }

            if (updatedCount > 0)
            {
                Dispatcher.Invoke(() =>
                {
                    StatusMessage = $"✅ Auto-updated {updatedCount} mods on startup";
                });
            }
            SaveMods();
        }

        private void AddSampleData()
        {
            Mods.Add(new ModItem
            {
                Id = 1,
                Name = "Terminus (RDR2)",
                GameTask = "RDR2.exe",
                Status = ModStatus.Ready,
                DownloadUrl = "https://github.com/Deadlineem/HorseMenu/releases/download/nightly/Terminus.dll"
            });
            Mods.Add(new ModItem
            {
                Id = 2,
                Name = "Chronix (GTA 5 Legacy)",
                GameTask = "GTA5.exe",
                Status = ModStatus.Ready,
                DownloadUrl = "https://github.com/Deadlineem/Chronix/releases/download/nightly/Chronix.dll"
            });
            Mods.Add(new ModItem
            {
                Id = 3,
                Name = "ChronixV2 (GTA 5 Enhanced)",
                GameTask = "GTA5_Enhanced.exe",
                Status = ModStatus.Ready,
                DownloadUrl = "https://github.com/Deadlineem/ChronixV2/releases/download/nightly/ChronixV2.dll"
            });
        }

        private void StartProcessMonitoring()
        {
            _processMonitorTimer = new Timer(CheckProcesses, null, 0, 3000);
        }

        private void CheckProcesses(object state)
        {
            if (_isRefreshing) return;
            _isRefreshing = true;

            try
            {
                bool anyChanged = false;

                foreach (var mod in Mods)
                {
                    bool isRunning = WinAPI.IsProcessRunning(mod.GameTask);
                    ProcessStatus newState = isRunning ? ProcessStatus.Found : ProcessStatus.NotFound;

                    if (mod.ProcessState != newState)
                    {
                        mod.ProcessState = newState;
                        anyChanged = true;
                    }
                }

                if (anyChanged)
                {
                    Dispatcher.Invoke(() =>
                    {
                        ModListBox.Items.Refresh();
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Process monitoring error: {ex.Message}");
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        private void RefreshModList()
        {
            OnPropertyChanged(nameof(Mods));
            ModListBox?.Items?.Refresh();
            UpdateModCount();
        }

        private void UpdateModCount()
        {
            ModCountText.Text = $"📊 {Mods.Count} mods listed";
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var handle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                var accent = new WinAPI.AccentPolicy();
                accent.AccentState = WinAPI.AccentState.ACCENT_ENABLE_BLURBEHIND;
                WinAPI.SetWindowCompositionAttribute(handle, ref accent);
            }
            catch { }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _processMonitorTimer?.Dispose();
            _injectionCancellationToken?.Cancel();
            SaveMods();
            Application.Current.Shutdown();
        }

        private async void AddModButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddModDialog();
            if (dialog.ShowDialog() == true)
            {
                string gameFolderName = Path.GetFileNameWithoutExtension(dialog.GameTask);
                if (string.IsNullOrWhiteSpace(gameFolderName))
                    gameFolderName = "UnknownGame";

                string gameFolderPath = Path.Combine(appDataPath, gameFolderName);

                string fileName = Fetch.GetFileNameFromUrl(dialog.DownloadUrl);
                if (string.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    fileName = dialog.ModName.Replace(" ", "") + ".dll";
                }

                if (!fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    fileName += ".dll";

                string outputPath = Path.Combine(gameFolderPath, fileName);

                int newId = 1;
                foreach (var mod in Mods)
                    if (mod.Id >= newId) newId = mod.Id + 1;

                if (!Directory.Exists(gameFolderPath))
                    Directory.CreateDirectory(gameFolderPath);

                var newMod = new ModItem
                {
                    Id = newId,
                    Name = dialog.ModName,
                    GameTask = dialog.GameTask,
                    DownloadUrl = dialog.DownloadUrl,
                    DllPath = outputPath,
                    Status = ModStatus.Ready
                };

                Mods.Add(newMod);
                RefreshModList();
                SaveMods();
                StatusMessage = $"✅ Added: {newMod.Name}";
            }
        }

        private async void DownloadModButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var mod = button?.Tag as ModItem;
            if (mod == null) return;

            if (string.IsNullOrEmpty(mod.DownloadUrl))
            {
                StatusMessage = $"❌ No download URL for {mod.Name}";
                return;
            }

            mod.Status = ModStatus.Updating;
            RefreshModList();
            StatusMessage = $"⬇️ Downloading {mod.Name}...";

            try
            {
                if (string.IsNullOrEmpty(mod.DllPath))
                {
                    string gameFolderName = Path.GetFileNameWithoutExtension(mod.GameTask);
                    if (string.IsNullOrWhiteSpace(gameFolderName))
                        gameFolderName = "UnknownGame";

                    string gameFolderPath = Path.Combine(appDataPath, gameFolderName);
                    string fileName = Fetch.GetFileNameFromUrl(mod.DownloadUrl);
                    if (string.IsNullOrWhiteSpace(fileName))
                        fileName = mod.Name.Replace(" ", "") + ".dll";

                    mod.DllPath = Path.Combine(gameFolderPath, fileName);
                }

                string folderPath = Path.GetDirectoryName(mod.DllPath);
                if (!string.IsNullOrEmpty(folderPath) && !Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                bool success = await Fetch.DownloadFileAsync(mod.DownloadUrl, mod.DllPath);

                if (success)
                {
                    if (Fetch.IsFileBlockedByAntivirus(mod.DllPath))
                    {
                        mod.Status = ModStatus.Error;
                        StatusMessage = $"⚠️ {mod.Name} blocked by antivirus!";
                    }
                    else
                    {
                        mod.Status = ModStatus.Updated;
                        StatusMessage = $"✅ Downloaded: {mod.Name}";
                    }
                }
                else
                {
                    mod.Status = ModStatus.Error;
                    StatusMessage = $"❌ Failed to download {mod.Name}";
                }
            }
            catch (Exception ex)
            {
                mod.Status = ModStatus.Error;
                StatusMessage = $"❌ Download error: {ex.Message}";
            }

            RefreshModList();
            SaveMods();
        }

        private async void InjectModButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var mod = button?.Tag as ModItem;
            if (mod == null) return;

            SelectedMod = mod;
            await Task.Delay(50);
            InjectButton_Click(sender, e);
        }

        private void InjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMod == null)
            {
                StatusMessage = "⚠️ Select a mod first!";
                return;
            }

            if (!IsAdministrator())
            {
                var result = MessageBox.Show(
                    "Cipher is not running as Administrator.\n\n" +
                    "DLL injection requires administrator privileges.\n\n" +
                    "Would you like to restart Cipher as Administrator?",
                    "Administrator Required",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    RestartAsAdmin();
                }
                return;
            }

            if (SelectedMod.Status != ModStatus.Updated)
            {
                StatusMessage = $"⚠️ {SelectedMod.Name} is not downloaded. Click 'Download' first!";
                return;
            }

            if (string.IsNullOrEmpty(SelectedMod.DllPath) || !File.Exists(SelectedMod.DllPath))
            {
                SelectedMod.Status = ModStatus.Missing;
                SaveMods();
                RefreshModList();
                StatusMessage = $"❌ DLL file missing: {SelectedMod.DllPath}";
                return;
            }

            _injectionCancellationToken?.Cancel();
            _injectionCancellationToken = new CancellationTokenSource();

            StartInjection(SelectedMod, _injectionCancellationToken.Token);
        }

        private async void StartInjection(ModItem mod, CancellationToken cancellationToken)
        {
            try
            {
                bool gameRunning = WinAPI.IsProcessRunning(mod.GameTask);
                StatusMessage = gameRunning ?
                    $"💉 {mod.Name} - Game is running, injecting now..." :
                    $"⏳ {mod.Name} - Waiting for {mod.GameTask} to start...";

                if (!gameRunning)
                {
                    int processId = await WaitForProcess(mod.GameTask, cancellationToken);
                    if (processId == 0)
                    {
                        StatusMessage = $"⏰ {mod.Name} - Timeout waiting for {mod.GameTask}";
                        return;
                    }
                }

                if (cancellationToken.IsCancellationRequested) return;

                await Task.Delay(500);

                if (cancellationToken.IsCancellationRequested) return;

                int pid = WinAPI.FindProcessByName(mod.GameTask);
                if (pid == 0)
                {
                    StatusMessage = $"❌ {mod.Name} - {mod.GameTask} not found!";
                    return;
                }

                StatusMessage = $"💉 {mod.Name} - Injecting into {mod.GameTask} (PID: {pid})...";

                bool success = WinAPI.InjectDLL(pid, mod.DllPath);

                StatusMessage = success ?
                    $"✅ {mod.Name} injected successfully into {mod.GameTask}!" :
                    $"❌ {mod.Name} - Injection failed!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Injection error: {ex.Message}";
            }
        }

        private async Task<int> WaitForProcess(string processName, CancellationToken cancellationToken)
        {
            const int maxWaitSeconds = 60;
            int elapsed = 0;

            while (elapsed < maxWaitSeconds)
            {
                if (cancellationToken.IsCancellationRequested) return 0;

                int pid = WinAPI.FindProcessByName(processName);
                if (pid != 0) return pid;

                await Task.Delay(1000);
                elapsed++;
            }

            return 0;
        }

        private bool IsAdministrator()
        {
            try
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        private void RestartAsAdmin()
        {
            try
            {
                var process = new Process();
                process.StartInfo.FileName = System.Reflection.Assembly.GetEntryAssembly().Location;
                process.StartInfo.Verb = "runas";
                process.StartInfo.UseShellExecute = true;
                process.Start();

                SaveMods();
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to restart as Administrator.\n\nPlease manually run Cipher as Administrator.",
                    "Restart Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMod == null)
            {
                StatusMessage = "⚠️ Select a mod first!";
                return;
            }

            if (MessageBox.Show($"Remove '{SelectedMod.Name}'?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                string removedName = SelectedMod.Name;

                if (!string.IsNullOrEmpty(SelectedMod.DllPath) && File.Exists(SelectedMod.DllPath))
                {
                    try { File.Delete(SelectedMod.DllPath); } catch { }
                }

                Mods.Remove(SelectedMod);
                SelectedMod = null;
                RefreshModList();
                SaveMods();
                StatusMessage = $"🗑 Removed: {removedName}";
            }
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
                var button = sender as Button;
                if (button != null) button.Content = "❐";
            }
            else
            {
                this.WindowState = WindowState.Normal;
                var button = sender as Button;
                if (button != null) button.Content = "☐";
            }
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AboutDialog();
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        private void GitHubButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://github.com/Deadlineem/Cipher");
            }
            catch
            {
                MessageBox.Show("Unable to open GitHub page.\nPlease visit:\nhttps://github.com/Deadlineem/Cipher",
                                "GitHub Link", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ShowExclusionGuide_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new HelpDialog();
            dialog.ExclusionPath = appDataPath;
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        private void ModListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedMod = ModListBox.SelectedItem as ModItem;
            if (SelectedMod != null)
                StatusMessage = $"Selected: {SelectedMod.Name}";
        }

        private void ModListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedMod != null)
                InjectButton_Click(sender, e);
        }

        private void SaveMods()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
                    MaxDepth = 64
                };
                var json = JsonSerializer.Serialize(Mods, options);
                File.WriteAllText(modsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Failed to save mods: {ex.Message}");
            }
        }

        private void LoadMods()
        {
            try
            {
                if (File.Exists(modsFilePath))
                {
                    var json = File.ReadAllText(modsFilePath);
                    var loaded = JsonSerializer.Deserialize<List<ModItem>>(json);

                    if (loaded != null)
                    {
                        Mods.Clear();
                        foreach (var mod in loaded)
                            Mods.Add(mod);

                        RefreshModList();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Failed to load mods: {ex.Message}");
                Mods.Clear();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}