# 🔐 Cipher - Mod Manager, Downloader & Injector

![GitHub Downloads](https://img.shields.io/github/downloads/Deadlineem/Cipher/total)
![License](https://img.shields.io/github/license/Deadlineem/Cipher)
![Platform](https://img.shields.io/badge/platform-x64%20%7C%20x86-blue)

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
