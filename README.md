# 🔐 Cipher - Mod Manager, Downloader & Injector

![GitHub Release](https://img.shields.io/github/v/release/Deadlineem/Cipher)
![GitHub Downloads](https://img.shields.io/github/downloads/Deadlineem/Cipher/total)
![License](https://img.shields.io/github/license/Deadlineem/Cipher)
![Platform](https://img.shields.io/badge/platform-x64%20%7C%20x86-blue)

<div align="center">
  <img width="1919" height="1312" alt="image" src="https://github.com/user-attachments/assets/3c4c2bad-3038-4e77-a4a0-d4333dbca4af" />
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
| **RAM** | 4GB minimum |
| **Storage** | 1GB free space (for mods) |
| **Privileges** | Administrator rights required for injection |

---

## 📦 Installation

### Option 1: Download the Release

1. Visit the [Releases](https://github.com/Deadlineem/Cipher/releases) page
2. Download the correct version for your system:
   - **[Cipher_x64.zip](https://github.com/Deadlineem/Cipher/releases/latest)** - For 64-bit games (recommended for most users)
   - **[Cipher_x86.zip](https://github.com/Deadlineem/Cipher/releases/latest)** - For 32-bit games
3. Extract the ZIP file to any folder
4. Run `Cipher.exe` as Administrator (right-click → "Run as administrator")

### Option 2: Build from Source

1. Clone the repository
```bash
git clone https://github.com/Deadlineem/Cipher.git
