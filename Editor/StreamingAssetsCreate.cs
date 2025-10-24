using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

namespace MyCustomPackage.Editor
{
    #region Main Installer Logic
    public static class StreamingAssetsCreate
    {
        public const string SETUP_COMPLETE_KEY = "ImxCoreSockets_BaseUrl_Create";

        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            if (!EditorPrefs.GetBool(SETUP_COMPLETE_KEY, false))
            {
                SetupDialog.ShowWindow();
            }
        }

        [MenuItem("ImxCoreSockets/Tools/Create/Overwrite Base Url")]
        public static void ShowConnectionSetup()
        {
            SetupDialog.ShowWindow();
        }
    }
    #endregion

    #region Setup Dialog Window
    public class SetupDialog : EditorWindow
    {
        private string ipAddress = "";
        private string screenName = "";
        private bool showError = false;

        public static void ShowWindow()
        {
            var window = GetWindow<SetupDialog>("Connection Setup");
            window.minSize = new Vector2(400, 200);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Setup Server Connection", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("Server IP Address(*):", EditorStyles.label);
            ipAddress = EditorGUILayout.TextField(ipAddress);

            GUILayout.Space(5);
            GUILayout.Label("Screen Name (Optional):", EditorStyles.label);
            screenName = EditorGUILayout.TextField(screenName);

            bool isIpValid = IsValidIP(ipAddress);
            if (!isIpValid && !string.IsNullOrEmpty(ipAddress))
            {
                showError = true;
            }
            else
            {
                showError = false;
            }

            if (showError)
            {
                EditorGUILayout.HelpBox("Invalid IP Address format. Please enter a valid IPv4 address (e.g., 192.168.1.10).", MessageType.Error);
            }
            GUILayout.FlexibleSpace();


            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(!isIpValid);
            if (GUILayout.Button("OK", GUILayout.Height(25)))
            {
                CreateBaseUrlFile();
                this.Close();
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Cancel", GUILayout.Height(25)))
            {
                this.Close();
            }
            EditorGUILayout.EndHorizontal();
        }

        private bool IsValidIP(string ip)
        {
            if (string.IsNullOrEmpty(ip)) return false;
            return Regex.IsMatch(ip, @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");
        }

        private void CreateBaseUrlFile()
        {
            string streamingAssetsPath = Application.streamingAssetsPath;
            if (!Directory.Exists(streamingAssetsPath))
            {
                Directory.CreateDirectory(streamingAssetsPath);
                Debug.Log($"[ImxCoreSockets] Created StreamingAssets folder at: {streamingAssetsPath}");
            }

            string filePath = Path.Combine(streamingAssetsPath, "baseurl.txt");

            string content = $"{ipAddress}#{screenName}";

            try
            {
                File.WriteAllText(filePath, content);
                Debug.Log($"[ImxCoreSockets] Successfully created 'baseurl.txt' with content: '{content}'");

                EditorPrefs.SetBool(StreamingAssetsCreate.SETUP_COMPLETE_KEY, true);
                AssetDatabase.Refresh();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ImxCoreSockets] Failed to create 'baseurl.txt'. Error: {e.Message}");
            }
        }
    }
    #endregion
}