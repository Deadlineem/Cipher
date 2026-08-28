using System;
using System.Net;
using System.Threading.Tasks;
using System.IO;

namespace Cipher
{
    public static class Fetch
    {
        /// <summary>
        /// Downloads a file from a URL with proper headers
        /// </summary>
        public static async Task<bool> DownloadFileAsync(string url, string outputPath)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🌐 Attempting to download: {url}");
                System.Diagnostics.Debug.WriteLine($"📁 Output path: {outputPath}");

                // Ensure directory exists
                string directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    System.Diagnostics.Debug.WriteLine($"📁 Created directory: {directory}");
                }

                using (var client = new WebClient())
                {
                    // GitHub requires a valid User-Agent
                    client.Headers.Add("User-Agent", "Cipher-Mod-Manager/1.0");

                    // Accept any encoding
                    client.Headers.Add("Accept", "*/*");

                    // Handle redirects
                    client.Headers.Add("Accept-Encoding", "gzip, deflate, br");

                    // Download the file
                    await client.DownloadFileTaskAsync(new Uri(url), outputPath);

                    System.Diagnostics.Debug.WriteLine($"✅ Download successful: {outputPath}");
                    return true;
                }
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ WebException: {ex.Message}");

                if (ex.Response is HttpWebResponse response)
                {
                    System.Diagnostics.Debug.WriteLine($"📊 HTTP Status: {(int)response.StatusCode} - {response.StatusCode}");

                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ File not found (404): {url}");
                    }
                    else if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Access forbidden (403): {url}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ HTTP Error {response.StatusCode}: {url}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Download error: {ex.Message}");
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Unexpected error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"📋 Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Checks if a URL is accessible
        /// </summary>
        public static async Task<bool> IsUrlAccessibleAsync(string url)
        {
            try
            {
                var request = WebRequest.CreateHttp(url);
                request.Method = "HEAD";
                request.UserAgent = "Cipher-Mod-Manager/1.0";
                request.Timeout = 5000;

                using (var response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    return response != null && response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🔍 URL check failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the filename from a URL
        /// </summary>
        public static string GetFileNameFromUrl(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                string fileName = Path.GetFileName(uri.LocalPath);

                if (!string.IsNullOrEmpty(fileName))
                {
                    int queryIndex = fileName.IndexOf('?');
                    if (queryIndex > 0)
                        fileName = fileName.Substring(0, queryIndex);
                }

                return fileName;
            }
            catch
            {
                return "mod.dll";
            }
        }

        /// <summary>
        /// Checks if the downloaded file was blocked by antivirus
        /// </summary>
        public static bool IsFileBlockedByAntivirus(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                // Try to read the file - if we can't, it might be quarantined
                using (var fs = File.OpenRead(filePath))
                {
                    // If we can open it, it's not blocked
                    return false;
                }
            }
            catch (UnauthorizedAccessException)
            {
                // This often means antivirus has locked/quarantined the file
                return true;
            }
            catch (IOException)
            {
                // File might be locked by antivirus scanner
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a mod's DLL file exists on disk
        /// </summary>
        public static bool ModFileExists(ModItem mod)
        {
            if (mod == null || string.IsNullOrEmpty(mod.DllPath))
                return false;

            return File.Exists(mod.DllPath);
        }
    }
}