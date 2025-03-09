using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace HeroicEngine.Utils.Data
{
#if UNITY_EDITOR
    public static class SOCollectionEditor
    {
        public static T CreateItem<T>(this SOCollection<T> collection, string path, params object[] args) where T : ConstructableSO
        {
            if (string.IsNullOrEmpty(path)) return null;
            T item = ScriptableObject.CreateInstance<T>();
            item.Construct(args);
            AssetDatabase.CreateAsset(item, path);
            AssetDatabase.SaveAssets();
            collection.RegisterItem(item);
            item.Initialize();
            return item;
        }
    }
#endif

    public class SOCollection<T> : ScriptableObject where T : ConstructableSO
    {
        [SerializeField] private List<T> items = new List<T>();

        public List<T> Items => items;

        public void RegisterItem(T item)
        {
            items.Add(item);
        }
    }
}
