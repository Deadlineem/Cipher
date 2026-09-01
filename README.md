# 🔐 Cipher - Mod Manager, Downloader & Injector

![GitHub Downloads](https://img.shields.io/github/downloads/Deadlineem/Cipher/total)
![License](https://img.shields.io/github/license/Deadlineem/Cipher)
![Platform](https://img.shields.io/badge/platform-x64%20%7C%20x86-blue)
![Version](https://img.shields.io/badge/version-1.0.29-green)

<div align="center">
  <table>
    <tr>
      <td><img width="400" alt="image" src="https://github.com/user-attachments/assets/464e3adb-7607-4c93-b197-95f46582233d"/></td>
      <td><img width="400" alt="image" src="https://github.com/user-attachments/assets/10b2c3fa-48b5-454a-9917-46642ccd5cc8" /></td>
      <td><img width="400" alt="image" src="https://github.com/user-attachments/assets/2e01909d-904e-48ef-bfd6-3cf057457d6e" /></td>
    </tr>
  </table>
</div>

## 📖 Overview

**Cipher** is a modern, open-source Windows application designed to simplify the process of managing, downloading, and injecting mod DLLs into games. Built with WPF and C#, Cipher provides a sleek, dark-themed user interface that makes mod management effortless.

Whether you're a casual gamer or a dedicated modder, Cipher streamlines the entire workflow - from downloading mods to injecting them into your favorite games.

---

## ✨ Features

### 🎮 Core Features
- **📦 Mod Management** - Add, remove, and organize your mods with ease
- **⬇️ One-Click Downloads** - Download mod DLLs directly from URLs
- **💉 DLL Injection** - Inject mods into running game processes using LoadLibraryA
- **🔍 Process Detection** - Automatically detects when games are running
- **⏳ Smart Injection** - Waits for games to launch before injecting (with countdown)
- **🔄 Auto-Update** - Automatically checks for mod updates on startup
- **📊 Real-Time Status** - Visual indicators for download and injection status
- **🎮 Game Launching** - Launch your games directly from Cipher with saved launch paths

### 🔒 Security & Compatibility
- **🛡️ Antivirus Guidance** - Built-in help for handling false positives
- **💻 x64 & x86 Support** - Both architectures available for maximum compatibility
- **📁 Persistent Storage** - Saves mods to `%APPDATA%\Cipher\mods.json`

---

## 🖥️ System Requirements

| Requirement | Specification |
|-------------|---------------|
| **OS** | Windows 10 / Windows 11 |
| **.NET Framework** | 4.7.2 or higher (included in Windows) |
| **Processor** | Any x64 or x86 compatible CPU |
| **Privileges** | Administrator rights required for mod injection |
| **Anti-Virus** | Exclusions for `%APPDATA%\Cipher` & `Cipher_x64.exe`/`Cipher_x86.exe` REQUIRED |

---

## 📦 Installation

### Option 1: Download the Release

1. Visit the [Releases](https://github.com/Deadlineem/Cipher/releases) page
2. Download the correct version for your system:
   - **[Cipher_x64.zip](https://github.com/Deadlineem/Cipher/releases/download/nightly/Cipher_x64.exe)** - For 64-bit games (recommended for most users)
   - **[Cipher_x86.zip](https://github.com/Deadlineem/Cipher/releases/download/nightly/Cipher_x86.exe)** - For 32-bit games
3. Add an Exclusion to your Anti-Virus software for `Cipher_(x86 | x64).exe` `(It uses CreateRemoteThread() so it sends false positives, its safe and open source, check the code!)`
4. Run `Cipher_x64.exe` OR `Cipher_x86.exe` as Administrator (right-click → "Run as administrator")
5. Add an Exclusion to your Anti-Virus software for `%APPDATA%\Cipher`

### Option 2: Build from Source

1. Clone the repository
```bash
git clone https://github.com/Deadlineem/Cipher.git
```

2. Open the solution in Visual Studio 2022
3. Select your target platform (x64 or x86)
4. Build → Rebuild Solution
5. The output will be in `Cipher\bin\{Platform}\Release\`

---

## 🛠️ How to Use Cipher

### NOTE: You must add an exclusion to your antivirus software to use Cipher or download DLL's 
- **Exclusions for `%APPDATA%\Cipher` & `Cipher_x64.exe/Cipher_x86.exe` are REQUIRED!**
### Step 1: Run as Administrator
- **Right-click** `Cipher.exe` → **Run as administrator**

### Step 2: Add a Mod
1. Click **"➕ Add Mod"**
2. Fill in:
   - **Mod Name**: A descriptive name for your mod
   - **Game Task**: The game's executable name (e.g., `RDR2.exe`, `GTA5.exe`)
   - **Download URL**: Direct URL to the `.dll` file
3. Click **"Add Mod"**
4. The mod will appear in your list with a status of "🟢 Ready"

### Step 3: Download the Mod
1. Find your mod in the list
2. Click the **"⬇ Download"** button next to the mod
3. The status will update to "⏳ Updating..." while downloading
4. Once complete, the status will change to "✅ Updated"

### Step 4: Launch Your Game
1. Find your mod in the list
2. Click the **"🎮 Start"** button next to the mod
3. If it's your first time launching the game:
   - A dialog will appear asking for the game path or launcher protocol
   - Enter the path (e.g., `C:\Program Files\Steam\steamapps\common\RDR2\RDR2.exe`)
   - Or enter a launcher protocol (e.g., `steam://rungameid/1404210`)
   - Check "Remember this location" to save it for future launches
4. The game will launch, and Cipher will automatically detect the running process

### Step 5: Inject the Mod
1. Once your game is running, click the **"💉 Inject"** button next to the mod
2. Cipher will detect the game process and inject the DLL
3. On successful injection, status will show "✅ Injected!"

### Managing Mods
- **🗑 Delete**: Remove a mod and its associated DLL
- **⟳ Refresh**: Update the list and check for missing DLLs

---

## 📁 Where Does Cipher Store Data?

Cipher saves your mods and downloaded DLLs in:
```
%APPDATA%\Cipher\
├── mods.json          # Your mod configuration
├── launch_paths.json  # Saved game launch paths
├── [GameName]\         # Game-specific folders
│   └── [ModName].dll   # Downloaded mod DLLs
```

You can access this folder by pressing `Win + R` and typing `%APPDATA%\Cipher`.

---

## 🔧 Building from Source

### Prerequisites
- Visual Studio 2022 with "Desktop development with .NET" workload
- .NET Framework 4.7.2 SDK

### Steps
1. Clone the repository
2. Open `Cipher.sln` in Visual Studio 2022
3. Select your target platform (x64 or x86)
4. Build → Rebuild Solution

---

## 🤝 Contributing

Contributions are welcome!

### Development Setup
1. Fork the repository
2. Create a feature branch
3. Build and test your changes
4. Update the `Ver` (version) information in `MainWindow.xaml.cs`
5. Submit a pull request

### Areas for Improvement
- Additional injection methods (Manual Map, Thread Hijacking)

---

## 📝 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## ⚠️ Disclaimer

**Cipher is provided "as is" without any warranties.** Use at your own risk. The developer assumes no liability for:
- Damage to your system or game files
- Account bans from using mods
- Compatibility issues with anti-cheat systems
- Loss of game progress or data

Always backup your game files before using mods and never mod games online.

---

## 📞 Support

| Resource | Link |
|----------|------|
| **Issues** | [GitHub Issues](https://github.com/Deadlineem/Cipher/issues) |

---

## 🙏 Acknowledgments

- The open-source modding community
- Contributors to the .NET and WPF frameworks
- All mod developers who make gaming more enjoyable

---

## 🔗 Quick Links

[![GitHub](https://img.shields.io/badge/GitHub-Repository-black)](https://github.com/Deadlineem/Cipher)
[![MIT License](https://img.shields.io/badge/License-MIT-green)](https://opensource.org/licenses/MIT)
[![Downloads](https://img.shields.io/github/downloads/Deadlineem/Cipher/total)](https://github.com/Deadlineem/Cipher/releases)

---

<div align="center">
  <sub>Built with ❤️ by the Cipher Team</sub>
</div>
