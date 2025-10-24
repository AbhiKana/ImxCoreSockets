// StreamingAssetsInstaller.cs
using UnityEngine;
using UnityEditor;
using System.IO;

namespace ImxCoreSockets.Editor
{
    public static class StreamingAssetsSourceCopy
    {
        // A unique key to store whether we've already run the import process.
        private const string IMPORT_KEY = "ImxCoreSockets_StreamingAssets_Imported";

        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            if (!SessionState.GetBool(IMPORT_KEY, false))
            {
                CopyStreamingAssets();
                SessionState.SetBool(IMPORT_KEY, true);
            }
        }

        public static void CopyStreamingAssets(bool forceOverwrite = false)
        {
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForPackageName("com.immersionx.imxcoresockets");
            if (packageInfo == null)
            {
                Debug.LogError("[ImxCoreSockets] Could not find package info. Is the package name correct?");
                return;
            }
            string packagePath = packageInfo.resolvedPath;
            string sourcePath = Path.Combine(packagePath, "Editor", "StreamingAssetsSource");

            if (!Directory.Exists(sourcePath))
            {
                Debug.LogWarning($"[ImxCoreSockets] Could not find StreamingAssets source folder at: {sourcePath}");
                return;
            }

            string destinationPath = Path.Combine(Application.streamingAssetsPath, "ImxCoreBaseUrl"); // Use a sub-folder to avoid conflicts

            if (!Directory.Exists(destinationPath)) Directory.CreateDirectory(destinationPath);

            foreach (string file in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
            {
                string relativePath = file.Substring(sourcePath.Length + 1);
                string destinationFile = Path.Combine(destinationPath, relativePath);
               
                if (!File.Exists(destinationFile))
                {
                    File.Copy(file, destinationFile, forceOverwrite);
                    Debug.Log($"[ImxCoreSockets] Copied StreamingAsset: {relativePath}");
                }
            }          
            AssetDatabase.Refresh();
        }

        // Add this inside the StreamingAssetsInstaller class

        [MenuItem("ImxCoreSockets/Tools/Sync Streaming Assets")]
        public static void SyncStreamingAssetsMenu()
        {           
            if (EditorUtility.DisplayDialog("Sync Streaming Assets",
                "This will copy all required assets from the package to your StreamingAssets folder, potentially overwriting any changes you've made. Are you sure?",
                "Yes, Sync", "Cancel"))
            {               
                CopyStreamingAssets(forceOverwrite: true);
                EditorUtility.DisplayDialog("Sync Complete", "Streaming Assets have been synced.", "OK");
            }
        }
    }
}