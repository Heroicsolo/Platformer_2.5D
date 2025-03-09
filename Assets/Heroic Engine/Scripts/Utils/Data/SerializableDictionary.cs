using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Utils.Data
{
    // Custom dictionary to be serialized in Unity
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys = new List<TKey>(); // List to hold dictionary keys
        [SerializeField]
        private List<TValue> values = new List<TValue>(); // List to hold dictionary values

        private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(); // The actual dictionary

        // Implement ISerializationCallbackReceiver interface
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();

            foreach (var kvp in dictionary)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            dictionary.Clear();

            for (int i = 0; i < System.Math.Min(keys.Count, values.Count); i++)
            {
                dictionary.Add(keys[i], values[i]);
            }
        }

        // Add key-value pair to the dictionary
        public void Add(TKey key, TValue value)
        {
            dictionary.Add(key, value);
        }

        // Remove key-value pair from the dictionary
        public bool Remove(TKey key)
        {
            return dictionary.Remove(key);
        }

        // Get the value by key
        public bool TryGetValue(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        // Check if the dictionary contains the key
        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        // Count of the dictionary entries
        public int Count => dictionary.Count;

        // Indexer to access dictionary values
        public TValue this[TKey key]
        {
            get => dictionary[key];
            set => dictionary[key] = value;
        }

        // Get the dictionary
        public Dictionary<TKey, TValue> GetDictionary()
        {
            return dictionary;
        }

        // Get all keys
        public List<TKey> GetKeys()
        {
            return keys;
        }

        // Get all values
        public List<TValue> GetValues()
        {
            return values;
        }
    }
}