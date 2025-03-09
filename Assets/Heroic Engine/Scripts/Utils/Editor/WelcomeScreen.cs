using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HeroicEngine.Utils.Editor
{
    public class WelcomeScreen : EditorWindow
    {
        private const string FirstLaunchPrefsKey = "FirstLaunch";
        private const string WeatherInstalledKey = "WeatherInstalled";
        private const string IgnoreWeatherKey = "IgnoreWeather";
        private const string RainPackageUrl = "https://assetstore.unity.com/packages/vfx/particles/environment/rain-maker-2d-and-3d-rain-particle-system-for-unity-34938";
        private const string RainMakerPackageName = "DigitalRuby.RainMaker";
        private const string RainMakerInstallHeader = "If you want to use weather system, you need to install Rain Maker package";

        // List of scene paths to add to Build Settings
        private readonly List<string> scenePaths = new(){
                "Assets/Heroic Engine/Example/Scenes/InitialScene.unity",
                "Assets/Heroic Engine/Example/Scenes/MainMenuScene.unity",
                "Assets/Heroic Engine/Example/Scenes/SampleSceneDuel.unity",
                "Assets/Heroic Engine/Example/Scenes/SampleSceneTopDown.unity",
                "Assets/Heroic Engine/Example/Scenes/TicTacToeSample.unity",
        };

        private Texture texture;
        private bool weatherEnabled = false;

        [MenuItem("Tools/HeroicEngine/Information and links", false, 101)]
        public static void ShowWindow()
        {
            GetWindow<WelcomeScreen>("Information");
            PlayerPrefs.SetInt(FirstLaunchPrefsKey, 0);
        }

        public static void TryShowWindow()
        {
            if (PlayerPrefs.GetInt(FirstLaunchPrefsKey, 1) == 1 || (PlayerPrefs.GetInt(WeatherInstalledKey, 0) == 0
                && PlayerPrefs.GetInt(IgnoreWeatherKey, 0) == 0))
            {
                GetWindow<WelcomeScreen>("Welcome");
                PlayerPrefs.SetInt(FirstLaunchPrefsKey, 0);
            }
        }

        private static bool DoesNamespaceExist(string targetNamespace)
        {
            // Get all loaded assemblies in the AppDomain
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    // Get all types defined in the assembly
                    Type[] types = assembly.GetTypes();

                    // Check if any type belongs to the specified namespace
                    if (types.Any(t => t.Namespace == targetNamespace))
                    {
                        return true;
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // Handle cases where types in the assembly can't be loaded (e.g., due to missing references)
                    Debug.LogWarning($"Unable to load types from assembly: {assembly.FullName}");
                }
            }

            return false;
        }

        private static void EnableWeather()
        {
            // Get the current build target group (e.g., Standalone, iOS, Android, etc.)
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

            // Get the existing scripting define symbols
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            // Check if the symbol is already added
            const string defineSymbol = "WEATHER_PACKAGE";
            if (!defines.Contains(defineSymbol))
            {
                // Add the symbol if not present
                defines = string.IsNullOrEmpty(defines) ? defineSymbol : $"{defines};{defineSymbol}";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);

                Debug.Log($"Scripting define symbol '{defineSymbol}' has been added for {buildTargetGroup}.");
            }
        }

        private bool AreDemoScenesSetUp()
        {
            if (EditorBuildSettings.scenes.Length == 0)
            {
                return false;
            }

            List<EditorBuildSettingsScene> buildScenes = new(EditorBuildSettings.scenes);

            foreach (var scenePath in scenePaths)
            {
                if (buildScenes.FindIndex(bs => bs.path == scenePath) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        private void SetUpDemoScenes()
        {
            // Create a list of EditorBuildSettingsScene
            var editorBuildSettingsScenes = new EditorBuildSettingsScene[scenePaths.Count];

            for (int i = 0; i < scenePaths.Count; i++)
            {
                // Ensure the scene exists at the specified path
                if (System.IO.File.Exists(scenePaths[i]))
                {
                    editorBuildSettingsScenes[i] = new EditorBuildSettingsScene(scenePaths[i], true);
                }
                else
                {
                    Debug.LogWarning($"Scene not found at path: {scenePaths[i]}");
                }
            }

            // Assign the scenes to the Build Settings
            EditorBuildSettings.scenes = editorBuildSettingsScenes;
            EditorSceneManager.OpenScene(scenePaths[0]);
        }

        private static void InstallPackage(string url)
        {
            // Parse the package name from the URL (adjust based on URL structure)
            string packageName = ParsePackageNameFromUrl(url);
            if (string.IsNullOrEmpty(packageName))
            {
                Debug.LogError("Invalid URL or unable to parse package name.");
                return;
            }

            string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            if (!File.Exists(manifestPath))
            {
                Debug.LogError("manifest.json not found.");
                return;
            }

            string manifestJson = File.ReadAllText(manifestPath);
            JObject manifest = JObject.Parse(manifestJson);

            JObject dependencies = (JObject)manifest["dependencies"];
            if (dependencies.ContainsKey(packageName))
            {
                Debug.Log($"Package '{packageName}' is already installed.");
                return;
            }

            // Add package to dependencies
            dependencies[packageName] = "latest"; // Replace "latest" with a specific version if needed.

            // Write back to manifest.json
            File.WriteAllText(manifestPath, manifest.ToString());
            Debug.Log($"Package '{packageName}' has been added. Unity will now refresh.");

            // Refresh assets
            AssetDatabase.Refresh();
        }

        private static string ParsePackageNameFromUrl(string url)
        {
            // Extract package name or ID from the URL
            // Example URL: https://assetstore.unity.com/packages/tools/ai/some-package-id
            // Return "com.unity.some-package-id" or similar if applicable
            // Adjust based on how your packages are named or provided.

            // Placeholder logic: parse based on your URL pattern
            string[] parts = url.Split('/');
            if (parts.Length > 0)
            {
                return parts[^1]; // Extract last part of the URL as package name
            }

            return null;
        }

        private void OnGUI()
        {
            GUIStyle headerStyle = new GUIStyle();
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.fontSize = 20;
            headerStyle.wordWrap = false;
            headerStyle.richText = true;

            GUIStyle headerStyleSmall = new GUIStyle();
            headerStyleSmall.fontStyle = FontStyle.Bold;
            headerStyleSmall.fontSize = 14;
            headerStyleSmall.wordWrap = true;
            headerStyleSmall.richText = true;

            EditorGUILayout.LabelField("Welcome to Heroic Engine!".ToColorizedString(Color.white), headerStyle);

            texture ??= AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Heroic Engine/Sprites/HeroicEngineBanner.png");
            GUILayout.Label(texture);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (PlayerPrefs.GetInt(IgnoreWeatherKey, 0) == 0)
            {
                if (!DoesNamespaceExist(RainMakerPackageName))
                {
                    EditorGUILayout.LabelField(RainMakerInstallHeader.ToColorizedString(Color.yellow), headerStyleSmall);
                    if (GUILayout.Button("Install Rain Maker"))
                    {
                        Application.OpenURL(RainPackageUrl);
                    }
                    if (GUILayout.Button("Skip"))
                    {
                        PlayerPrefs.SetInt(IgnoreWeatherKey, 1);
                    }
                    return;
                }
                else if (!weatherEnabled)
                {
                    EnableWeather();
                    weatherEnabled = true;
                    PlayerPrefs.SetInt(WeatherInstalledKey, 1);
                }
            }

            if (EditorSettings.defaultBehaviorMode == EditorBehaviorMode.Mode3D)
            {
                if (!DoesNamespaceExist(RainMakerPackageName))
                {
                    EditorGUILayout.LabelField(RainMakerInstallHeader.ToColorizedString(Color.yellow), headerStyleSmall);
                    if (GUILayout.Button("Install Rain Maker"))
                    {
                        Application.OpenURL(RainPackageUrl);
                    }
                }
                else if (!weatherEnabled)
                {
                    EnableWeather();
                    weatherEnabled = true;
                    PlayerPrefs.SetInt(WeatherInstalledKey, 1);
                }
            }

            if (!AreDemoScenesSetUp())
            {
                EditorGUILayout.LabelField("Click button below, if you want to see demo scenes");
                EditorGUILayout.Space();

                if (GUILayout.Button("Set Up Demo Scenes"))
                {
                    SetUpDemoScenes();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (GUILayout.Button("Read Documentation"))
            {
                Application.OpenURL("https://heroicsolo.gitbook.io/heroic-engine");
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Go to our Discord"))
            {
                Application.OpenURL("https://discord.gg/gTbzY4vhvD");
            }
        }
    }

    [InitializeOnLoad]
    public static class OpenEditorWindowOnLoad
    {
        static OpenEditorWindowOnLoad()
        {
            EditorApplication.delayCall += () =>
            {
                // Check if the window is already open to avoid duplicates
                if (EditorWindow.HasOpenInstances<WelcomeScreen>() == false)
                {
                    WelcomeScreen.TryShowWindow();
                }
            };
        }
    }
}