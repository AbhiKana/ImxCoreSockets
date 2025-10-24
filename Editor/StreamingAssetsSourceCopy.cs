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
        private const string PACKAGE_NAME = "com.immersionx.imxcoresockets";
        private const string DEV_PACKAGE_FOLDER_NAME = "ImxCoreSockets";

        //[InitializeOnLoadMethod]
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
            var packagePath = GetPackageRootPath();
            if (packagePath == null)
            {
                Debug.LogError("[ImxCoreSockets] Could not find package info. Is the package name correct?");
                return;
            }

            string sourcePath = Path.Combine(packagePath, "Editor", "StreamingAssetsSource");

            if (!Directory.Exists(sourcePath))
            {
                Debug.LogWarning($"[ImxCoreSockets] Could not find StreamingAssets Source folder at: {sourcePath}");
                return;
            }

            string destinationPath = Path.Combine(Application.streamingAssetsPath); // Use a sub-folder to avoid conflicts

            if (!Directory.Exists(destinationPath)) Directory.CreateDirectory(destinationPath);

            foreach (string file in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
            {
                string relativePath = file.Substring(sourcePath.Length + 1);
                string destinationFile = Path.Combine(destinationPath, relativePath);

                if (forceOverwrite || !File.Exists(destinationFile))
                {
                    File.Copy(file, destinationFile, true);
                    Debug.Log($"[ImxCoreSockets] Copied StreamingAsset: {relativePath}");
                }
                AssetDatabase.Refresh();
            }
        }

        private static string GetPackageRootPath()
        {
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForPackageName(PACKAGE_NAME);
            if (packageInfo != null)
            {
                Debug.Log($"[ImxCoreSockets] Found UPM package at: {packageInfo.resolvedPath}");
                return packageInfo.resolvedPath;
            }
            Debug.LogWarning($"[ImxCoreSockets] UPM package not found. Assuming development environment and looking for folder: '{DEV_PACKAGE_FOLDER_NAME}' in Assets.");

            string devPath = Path.Combine(Application.dataPath, "Server", DEV_PACKAGE_FOLDER_NAME);

            if (Directory.Exists(devPath))
            {
                if (File.Exists(Path.Combine(devPath, "package.json")))
                {
                    Debug.Log($"[ImxCoreSockets] Found development package at: {devPath}");
                    return devPath;
                }
                else
                {
                    Debug.LogError($"[ImxCoreSockets] Found folder '{DEV_PACKAGE_FOLDER_NAME}' but it does not contain a package.json file.");
                }
            }
            else
            {
                Debug.LogError($"[ImxCoreSockets] Could not find development package folder at: {devPath}");
            }

            // If neither method works, we can't proceed.
            Debug.LogError($"[ImxCoreSockets] Could not locate package '{PACKAGE_NAME}' in any form.");
            return null;
        }


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