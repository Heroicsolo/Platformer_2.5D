using HeroicEngine.Utils.Editor;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HeroicEngine.Utils
{
    public class MobileBuildOptimizer : EditorWindow
    {
        private const long ThresholdAudioSizeInBytes = 200 * 1024; // 200 KB in bytes
        private const long MaxAudioSizeInBytes = 2048 * 1024; // 2 MB in bytes

        private float optimizationProgress = 0f;
        private string optimizationType = "";

        [MenuItem("Tools/HeroicEngine/Mobile Build Optimizer", false, 21)]
        public static void ShowWindow()
        {
            GetWindow<MobileBuildOptimizer>("Mobile Build Optimizer");
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Optimize Textures"))
            {
                OptimizeTextures();
            }

            if (GUILayout.Button("Optimize Models"))
            {
                OptimizeModels();
            }

            if (GUILayout.Button("Optimize Audio"))
            {
                OptimizeAudio();
            }

            if (GUILayout.Button("Optimize Project Settings"))
            {
                optimizationType = "Settings";
                OptimizeSettings();
                optimizationProgress = 1f;
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Optimize All"))
            {
                OptimizeAll();
            }

            if (optimizationProgress > 0f)
            {
                GUILayout.Space(20);

                if (optimizationProgress < 1f)
                {
                    GUILayout.Label($"Optimizing {optimizationType}: [{Mathf.CeilToInt(optimizationProgress * 100f)}%]");
                }
                else
                {
                    GUILayout.Label($"Optimizing {optimizationType}: DONE");
                }
            }
        }

        private void OptimizeTextures()
        {
            optimizationProgress = 0f;
            optimizationType = "Textures";

            // Get all texture assets in the project
            string[] textureGUIDs = AssetDatabase.FindAssets("t:Texture", new string[] { "Assets" });

            int idx = 0;

            foreach (string guid in textureGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

                if (textureImporter != null)
                {
                    // Ensure the texture is readable before processing
                    textureImporter.isReadable = true;
                    textureImporter.mipmapEnabled = true;

                    if (textureImporter.maxTextureSize > 1024)
                    {
                        // Apply resolution limit (1024x1024)
                        textureImporter.maxTextureSize = 1024;
                    }

                    // Enable crunch compression
                    textureImporter.textureCompression = TextureImporterCompression.Compressed;
                    textureImporter.crunchedCompression = true;
                    textureImporter.compressionQuality = 100;

                    // Reimport the texture to apply changes
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

                    Debug.Log($"Processed texture: {assetPath}");
                }
                else
                {
                    Debug.LogWarning($"Could not process texture at {assetPath}, not a valid texture importer.");
                }

                idx++;
                optimizationProgress = (float)idx / textureGUIDs.Length;
                Repaint();
            }

            // Refresh Asset Database to show changes
            AssetDatabase.Refresh();
            Debug.Log("Texture processing complete.");
        }

        private void OptimizeModels()
        {
            optimizationProgress = 0f;
            optimizationType = "Models";

            // Get all model assets in the project
            string[] modelGUIDs = AssetDatabase.FindAssets("t:Model", new string[] { "Assets" });

            int idx = 0;

            foreach (string guid in modelGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                ModelImporter modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;

                if (modelImporter != null)
                {
                    // Disable Tangents
                    modelImporter.importTangents = ModelImporterTangents.None;

                    // Enable Mesh Compression
                    modelImporter.meshCompression = ModelImporterMeshCompression.High;

                    // Reimport the model to apply changes
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

                    Debug.Log($"Processed model: {assetPath}");
                }
                else
                {
                    Debug.LogWarning($"Could not process model at {assetPath}, not a valid model importer.");
                }

                idx++;
                optimizationProgress = (float)idx / modelGUIDs.Length;
                Repaint();
            }

            // Refresh Asset Database to show changes
            AssetDatabase.Refresh();
            Debug.Log("Model processing complete.");
        }

        private void OptimizeAudio()
        {
            optimizationProgress = 0f;
            optimizationType = "Audio";

            // Get all audio assets in the project
            string[] audioGUIDs = AssetDatabase.FindAssets("t:AudioClip", new string[] { "Assets" });

            int idx = 0;

            foreach (string guid in audioGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                AudioImporter audioImporter = AssetImporter.GetAtPath(assetPath) as AudioImporter;

                if (audioImporter != null)
                {
                    // Check the size of the audio file
                    FileInfo fileInfo = new FileInfo(assetPath);
                    long fileSizeInBytes = fileInfo.Length;

                    AudioImporterSampleSettings sampleSettings = audioImporter.defaultSampleSettings;

                    // Set Load Type based on the file size
                    if (fileSizeInBytes < ThresholdAudioSizeInBytes)
                    {
                        // Set to "Decompress on Load"
                        sampleSettings.loadType = AudioClipLoadType.DecompressOnLoad;
                    }
                    else if (fileSizeInBytes < MaxAudioSizeInBytes)
                    {
                        // Set to "Compressed in Memory"
                        sampleSettings.loadType = AudioClipLoadType.CompressedInMemory;
                    }
                    else
                    {
                        // Set to "Streaming"
                        sampleSettings.loadType = AudioClipLoadType.Streaming;
                    }

                    audioImporter.defaultSampleSettings = sampleSettings;

                    // Reimport the audio asset to apply changes
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

                    Debug.Log($"Processed audio asset: {assetPath} (Size: {fileSizeInBytes / 1024f} KB)");
                }
                else
                {
                    Debug.LogWarning($"Could not process audio asset at {assetPath}, not a valid audio importer.");
                }

                idx++;
                optimizationProgress = (float)idx / audioGUIDs.Length;
                Repaint();
            }

            // Refresh the Asset Database to reflect changes
            AssetDatabase.Refresh();
            Debug.Log("Audio asset processing complete.");
        }

        private void OptimizeSettings()
        {
            PlayerSettings.Android.minifyRelease = true;
            PlayerSettings.bakeCollisionMeshes = true;
            SetIL2CPP();
        }

        private void SetIL2CPP()
        {
            // Make sure we are running this in an active build target (e.g., Standalone, Android, iOS)
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows ||
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSX ||
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ||
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                // Set the scripting backend to IL2CPP
                PlayerSettings.SetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup, ScriptingImplementation.IL2CPP);

                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    // Set the Android target architecture (you can customize this for other platforms)
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.X86_64;
                }

                // Log the changes
                Debug.Log("IL2CPP mode has been set for the current build target: " + EditorUserBuildSettings.activeBuildTarget);
            }
            else
            {
                Debug.LogWarning("IL2CPP can only be set for valid build targets (Standalone, Android, iOS).");
            }
        }

        private void OptimizeAll()
        {
            OptimizeTextures();
            OptimizeModels();
            OptimizeAudio();
            OptimizeSettings();
        }
    }
}