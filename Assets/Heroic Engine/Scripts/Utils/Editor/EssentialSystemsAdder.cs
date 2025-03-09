using HeroicEngine.Gameplay;
using HeroicEngine.Systems;
using HeroicEngine.Systems.Audio;
using HeroicEngine.Systems.DI;
using HeroicEngine.Systems.Events;
using HeroicEngine.Systems.Gameplay;
using HeroicEngine.Systems.Inputs;
using HeroicEngine.Systems.Localization;
using HeroicEngine.Systems.ScenesManagement;
using HeroicEngine.Systems.UI;
using HeroicEngine.Utils.Pooling;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

namespace HeroicEngine.Utils.Editor
{
    public static class EssentialSystemsAdder
    {
        private static readonly List<Type> _systemsTypes = new()
        {
            typeof(InjectionManager),
            typeof(PoolSystem),
            typeof(ScenesLoader),
            typeof(TimeManager),
            typeof(EventsManager),
            typeof(LocalizationManager),
            typeof(PlayerProgressionManager),
            typeof(HittablesManager),
            typeof(UIController),
            typeof(RandomEventsManager),
            typeof(CurrenciesManager),
            typeof(SoundsManager),
            typeof(MusicPlayer),
            typeof(InputManager),
            typeof(CameraController)
        };

        [MenuItem("Tools/HeroicEngine/Add Essential Systems to scene", false, 2)]
        public static void AddEssentialSystems()
        {
            _systemsTypes.ForEach(sys => InstantiateSystem(sys));
        }

        private static GameObject FindPrefabWithComponentType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            // Get all prefab assets in the project
            var requestResults = SearchService.Request($"t:{typeName}");

            foreach (var requestResult in requestResults.ToArray())
            {
                if (GlobalObjectId.TryParse(requestResult.id, out var id))
                {
                    string path = AssetDatabase.GUIDToAssetPath(id.assetGUID);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                    if (prefab != null && prefab.GetComponent(typeName) != null)
                    {
                        return prefab;
                    }
                }
            }

            return null;
        }

        private static void InstantiateSystem(Type systemType)
        {
            if (systemType != null)
            {
                var existingObject = UnityEngine.Object.FindAnyObjectByType(systemType);

                if (existingObject != null)
                {
                    return;
                }

                var prefab = FindPrefabWithComponentType(systemType.Name);

                if (prefab != null)
                {
                    var newObj = UnityEngine.Object.Instantiate(prefab);
                    newObj.name = systemType.Name;
                    return;
                }

                // Create a new GameObject and add the script as a component
                GameObject newObject = new GameObject(systemType.Name);
                newObject.AddComponent(systemType);
                AssetDatabase.Refresh();
            }
        }
    }
}