using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Cipher
{
    public static class WinAPI
    {
        // ============================================
        // EXACT P/Invoke declarations from EzInjector
        // ============================================

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        // ============================================
        // UI-related P/Invoke (for Window_Loaded)
        // ============================================

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref AccentPolicy accent);

        // ============================================
        // UI Structures
        // ============================================

        [StructLayout(LayoutKind.Sequential)]
        public struct AccentPolicy
        {
            public AccentState AccentState;
            public uint AccentFlags;
            public uint GradientColor;
            public uint AnimationId;
        }

        public enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
            ACCENT_ENABLE_HOSTBACKDROP = 5,
            ACCENT_INVALID_STATE = 6
        }

        // ============================================
        // EXACT constants from EzInjector
        // ============================================

        private const int PROCESS_CREATE_THREAD = 0x0002;
        private const int PROCESS_QUERY_INFORMATION = 0x0400;
        private const int PROCESS_VM_OPERATION = 0x0008;
        private const int PROCESS_VM_WRITE = 0x0020;
        private const int PROCESS_VM_READ = 0x0010;
        private const uint MEM_COMMIT = 0x00001000;
        private const uint MEM_RESERVE = 0x00002000;
        private const uint PAGE_READWRITE = 4;

        // ============================================
        // Process Management
        // ============================================

        public static int FindProcessByName(string processName)
        {
            try
            {
                string name = processName.Replace(".exe", "").Trim();
                if (string.IsNullOrEmpty(name))
                    return 0;

                var processes = Process.GetProcessesByName(name);
                if (processes.Length > 0)
                    return processes[0].Id;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ FindProcessByName error: {ex.Message}");
            }
            return 0;
        }

        public static bool IsProcessRunning(string processName)
        {
            return FindProcessByName(processName) != 0;
        }

        // ============================================
        // EXACT COPY of EzInjector's InjectDll method
        // ============================================

        public static bool InjectDLL(int processId, string dllPath)
        {
            System.Diagnostics.Debug.WriteLine($"💉 InjectDLL called with:");
            System.Diagnostics.Debug.WriteLine($"   DLL Path: {dllPath}");
            System.Diagnostics.Debug.WriteLine($"   Process ID: {processId}");

            IntPtr hProcess = IntPtr.Zero;
            IntPtr allocatedMemory = IntPtr.Zero;
            IntPtr loadLibraryAddr = IntPtr.Zero;
            IntPtr hThread = IntPtr.Zero;

            try
            {
                // EXACT same OpenProcess call
                hProcess = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION |
                                      PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ,
                                      false, processId);

                if (hProcess == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    System.Diagnostics.Debug.WriteLine($"❌ OpenProcess failed. Error: {error}");
                    return false;
                }
                System.Diagnostics.Debug.WriteLine($"✅ OpenProcess succeeded");

                // EXACT same GetProcAddress
                loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
                if (loadLibraryAddr == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    System.Diagnostics.Debug.WriteLine($"❌ GetProcAddress failed. Error: {error}");
                    return false;
                }
                System.Diagnostics.Debug.WriteLine($"✅ LoadLibraryA address: 0x{loadLibraryAddr.ToInt64():X}");

                // EXACT same memory allocation
                byte[] dllPathBytes = Encoding.ASCII.GetBytes(dllPath + "\0");
                uint allocSize = (uint)((dllPathBytes.Length + 1) * Marshal.SizeOf(typeof(byte)));

                allocatedMemory = VirtualAllocEx(hProcess, IntPtr.Zero, allocSize,
                                                MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

                if (allocatedMemory == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    System.Diagnostics.Debug.WriteLine($"❌ VirtualAllocEx failed. Error: {error}");
                    return false;
                }
                System.Diagnostics.Debug.WriteLine($"✅ Allocated memory at: 0x{allocatedMemory.ToInt64():X}");

                // EXACT same WriteProcessMemory
                IntPtr bytesWritten;
                bool writeResult = WriteProcessMemory(hProcess, allocatedMemory, dllPathBytes,
                                                     (uint)dllPathBytes.Length, out bytesWritten);

                if (!writeResult || bytesWritten.ToInt64() != dllPathBytes.Length)
                {
                    int error = Marshal.GetLastWin32Error();
                    System.Diagnostics.Debug.WriteLine($"❌ WriteProcessMemory failed. Error: {error}");
                    return false;
                }
                System.Diagnostics.Debug.WriteLine($"✅ Wrote {bytesWritten} bytes to process memory");

                // EXACT same CreateRemoteThread
                hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, loadLibraryAddr,
                                            allocatedMemory, 0, IntPtr.Zero);

                if (hThread == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    System.Diagnostics.Debug.WriteLine($"❌ CreateRemoteThread failed. Error: {error}");
                    return false;
                }
                System.Diagnostics.Debug.WriteLine($"✅ Created remote thread: 0x{hThread.ToInt64():X}");

                // EXACT same WaitForSingleObject
                var threadResult = WaitForSingleObject(hThread, 10000);

                if (threadResult == 0x00000080 || threadResult == 0x00000102 || threadResult == 0xFFFFFFFF)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Thread result: {threadResult} - may still load");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Thread completed successfully");
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Injection exception: {ex.Message}");
                return false;
            }
            finally
            {
                // EXACT same cleanup
                if (hThread != IntPtr.Zero)
                {
                    CloseHandle(hThread);
                    System.Diagnostics.Debug.WriteLine($"✅ Closed thread handle");
                }
                if (hProcess != IntPtr.Zero)
                {
                    CloseHandle(hProcess);
                    System.Diagnostics.Debug.WriteLine($"✅ Closed process handle");
                }
            }
        }
    }
}