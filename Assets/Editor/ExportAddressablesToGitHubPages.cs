// ✅ [FINALIZED] Assets/Editor/ExportAddressablesToGitHubPages.cs
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.Diagnostics;
using System.IO;
using System.Linq;

public static class ExportAddressablesToGitHubPages
{
    private const string localExportPath = "docs/RemoteAssets";
    private const string remoteLoadPathExpectedPrefix = "https://";

    [MenuItem("Tools/Export Addressables to Pages")]
    public static void Export()
    {
        // ✅ 0. Проверяем RemoteLoadPath
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var profileSettings = settings.profileSettings;
        string loadPath = profileSettings.GetValueByName(settings.activeProfileId, "RemoteLoadPath");

        if (string.IsNullOrEmpty(loadPath) || !loadPath.StartsWith(remoteLoadPathExpectedPrefix))
        {
            EditorUtility.DisplayDialog("RemoteLoadPath Error",
                "RemoteLoadPath не начинается с https:// или не указан в профиле.\n\nПроверь Addressables → Profiles.",
                "OK");
            return;
        }

        // ✅ 1. Собираем Addressables
        AddressableAssetSettings.BuildPlayerContent();

        // ✅ 2. Копируем собранные файлы в docs/RemoteAssets
        string source = Path.Combine(Directory.GetCurrentDirectory(), "docs/RemoteAssets");
        string destination = Path.Combine(Directory.GetCurrentDirectory(), localExportPath);

        if (!Directory.Exists(source))
        {
            EditorUtility.DisplayDialog("Error", $"Addressables build folder not found:\n{source}", "OK");
            return;
        }

        if (!Directory.Exists(destination))
            Directory.CreateDirectory(destination);

        var files = Directory.GetFiles(source)
            .Where(f => !f.EndsWith(".DS_Store")) // 🔧 [NEW] Пропускаем системные файлы macOS
            .ToArray();

        if (files.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", $"No Addressable files found in {source}", "OK");
            return;
        }

        foreach (string file in files)
        {
            try
            {
                string destFile = Path.Combine(destination, Path.GetFileName(file));
                File.Copy(file, destFile, true);
                UnityEngine.Debug.Log($"Copied: {destFile}");
            }
            catch (IOException ex)
            {
                UnityEngine.Debug.LogWarning($"Skipped copying {file}: {ex.Message}"); // 🔧 [NEW] Без креша
            }
        }

        // ✅ 3. Коммитим и пушим
        RunGitCommand($"add {localExportPath}");
        RunGitCommand($"commit -m \"🚀 Update RemoteAssets\"");
        RunGitCommand($"push");

        EditorUtility.DisplayDialog("Success", "Addressables uploaded to GitHub Pages!", "OK");
    }

    private static void RunGitCommand(string command)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = command,
            WorkingDirectory = Directory.GetCurrentDirectory(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(psi);
        process.WaitForExit();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        UnityEngine.Debug.Log($"git {command}\n{output}\n{error}");
    }
}
