using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HeroicEngine.Utils.Editor
{
    public class SpriteFromPrefabGenerator : EditorWindow
    {
        private GameObject prefab;
        private string savePath = "Assets/Heroic Engine/Sprites/Icons/";
        private int resolution = 512;
        private Color backgroundColor = Color.clear;
        private float shootOffsetY = 0f;
        private Texture2D generatedTexture;

        [MenuItem("Tools/HeroicEngine/Icon From Prefab Generator")]
        private static void OpenWindow()
        {
            GetWindow<SpriteFromPrefabGenerator>("Icon From Prefab");
        }

        private void OnGUI()
        {
            GUILayout.Label("Generate Icon from Prefab", EditorStyles.boldLabel);

            prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
            savePath = EditorGUILayout.TextField("Save Path", savePath);
            resolution = EditorGUILayout.IntField("Resolution", resolution);
            backgroundColor = EditorGUILayout.ColorField("Background Color", backgroundColor);
            shootOffsetY = EditorGUILayout.FloatField("Shoot Y Offset", shootOffsetY);

            if (GUILayout.Button("Generate Icon"))
            {
                if (prefab == null)
                {
                    Debug.LogError("No prefab selected.");
                    return;
                }

                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                generatedTexture = GenerateIcon();
            }

            if (generatedTexture != null)
            {
                Rect previewRect = GUILayoutUtility.GetRect(128, 128, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                EditorGUI.DrawPreviewTexture(previewRect, generatedTexture);
            }
        }

        private Texture2D GenerateIcon()
        {
            // Create a temporary scene
            var tempScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

            // Instantiate the prefab
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            if (instance == null)
            {
                Debug.LogError("Failed to instantiate prefab.");
                return null;
            }

            // Calculate bounds
            Bounds bounds = CalculateBounds(instance);

            // Create and setup the camera
            Camera camera = new GameObject("Camera").AddComponent<Camera>();
            camera.backgroundColor = backgroundColor;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.orthographic = true;

            // Calculate orthographic size to fit the object
            float maxDimension = Mathf.Max(bounds.size.x, bounds.size.y);
            camera.orthographicSize = maxDimension / 2f;

            // Position the camera to fit the object
            camera.transform.position = bounds.center + new Vector3(0, 0, -10);

            // Add directional light
            Light light = new GameObject("Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);

            // Render to texture
            RenderTexture renderTexture = new RenderTexture(resolution, resolution, 24);
            camera.targetTexture = renderTexture;
            camera.Render();

            // Convert to Texture2D
            RenderTexture.active = renderTexture;
            Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
            texture.Apply();

            string fullPath = savePath + $"{prefab.name}.png";

            // Save as PNG
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(fullPath, bytes);
            Debug.Log("Icon saved to: " + fullPath);

            // Cleanup
            RenderTexture.active = null;
            camera.targetTexture = null;

            DestroyImmediate(instance);
            DestroyImmediate(camera.gameObject);
            DestroyImmediate(light.gameObject);

            EditorSceneManager.CloseScene(tempScene, true);

            // Load and display the generated icon
            AssetDatabase.Refresh();

            Object asset = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
            Selection.activeObject = asset;

            return texture;
        }

        private Bounds CalculateBounds(GameObject obj)
        {
            Bounds bounds = new Bounds(obj.transform.position, Vector3.zero);
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }
    }
}