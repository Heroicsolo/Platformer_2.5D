using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Utils.Pooling
{
    public class PoolSystem : MonoBehaviour
    {
        public static PoolSystem instance;

        private readonly Dictionary<string, Queue<IPooledObject>> pool = new Dictionary<string, Queue<IPooledObject>>();
        private readonly Dictionary<string, IPooledObject> prefabs = new Dictionary<string, IPooledObject>();
        private readonly List<GameObject> poolGOs = new List<GameObject>();

        private void Awake()
        {
            if (!instance)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        /// <summary>
        /// This method clears all created pools.
        /// </summary>
        /// <param name="destroyImmediately">If true, it will immediately destroy gameobjects in pools instead of simple Destroy calls</param>
        public static void ResetPools(bool destroyImmediately = true)
        {
            foreach (var go in instance.poolGOs.ToArray())
            {
                if (destroyImmediately)
                {
                    Object.DestroyImmediate(go);
                }
                else
                {
                    Object.Destroy(go);
                }
            }

            instance.poolGOs.Clear();

            foreach (var queue in instance.pool.Values)
            {
                queue.Clear();
            }
        }

        /// <summary>
        /// This method returns an available instance of given prefab from pool with name, moves this instance to given world position and sets certain parent transform.
        /// If parent is null, it leaves instance in previously assigned parent transform.
        /// </summary>
        /// <typeparam name="T">Type of object class</typeparam>
        /// <param name="prefab">Object prefab</param>
        /// <param name="name">Name of pool</param>
        /// <param name="pos">Target position</param>
        /// <param name="parent">Target parent transform</param>
        /// <returns>Instance of given prefab, taken from pool and placed in given position</returns>
        public static T GetInstanceAtPosition<T>(T prefab, string name, Vector3 pos, Transform parent = null) where T : MonoBehaviour, IPooledObject
        {
            Vector3 scale = prefab.transform.localScale;
            T ins = GetInstance(prefab, name);
            if (parent != null)
            {
                ins.transform.SetParent(parent);
            }
            ins.transform.position = pos;
            ins.transform.localScale = scale;
            ins.transform.rotation = Quaternion.identity;
            return ins;
        }

        /// <summary>
        /// This method returns an available instance of given prefab from pool with name, moves this instance to given world position, rotation and sets certain parent transform.
        /// If parent is null, it leaves instance in previously assigned parent transform.
        /// </summary>
        /// <typeparam name="T">Type of object class</typeparam>
        /// <param name="prefab">Object prefab</param>
        /// <param name="name">Name of pool</param>
        /// <param name="pos">Target position</param>
        /// <param name="rotation">Target rotation</param>
        /// <param name="parent">Target parent transform</param>
        /// <returns>Instance of given prefab, taken from pool and placed in given position, with applied rotation</returns>
        public static T GetInstanceAtPosition<T>(T prefab, string name, Vector3 pos, Quaternion rotation, Transform parent = null) where T : MonoBehaviour, IPooledObject
        {
            Vector3 scale = prefab.transform.localScale;
            T ins = GetInstance(prefab, name);
            if (parent != null)
            {
                ins.transform.SetParent(parent);
            }
            ins.transform.position = pos;
            ins.transform.localScale = scale;
            ins.transform.rotation = rotation;
            return ins;
        }

        /// <summary>
        /// This method returns an instance of class which is inherited from MonoBehaviour and IPooledObject, and presented by certain prefab.
        /// It tries to get such object from pool with assigned name, in case if such pool isn't found, it creates new pool with this name.
        /// </summary>
        /// <typeparam name="T">Type of object class</typeparam>
        /// <param name="prefab">Object prefab</param>
        /// <param name="name">Name of pool</param>
        /// <returns>Instance of given prefab, taken from pool</returns>
        public static T GetInstance<T>(T prefab, string name) where T : MonoBehaviour, IPooledObject
        {
            return instance.GetOrSpawnInstance(prefab, name);
        }

        /// <summary>
        /// This method returns an available instance of class which is inherited from MonoBehaviour and IPooledObject, and contained in pool with name.
        /// </summary>
        /// <typeparam name="T">Type of object class</typeparam>
        /// <param name="name">Name of pool</param>
        /// <returns>Available instance of given type, taken from pool</returns>
        public static T GetObj<T>(string name) where T : MonoBehaviour, IPooledObject
        {
            return instance.GetObject<T>(name);
        }

        /// <summary>
        /// This method returns gameobject obj of given type to pool. This gameobject will be immediately deactivated after this.
        /// </summary>
        /// <typeparam name="T">Type of object class</typeparam>
        /// <param name="obj">Object to return</param>
        public static void ReturnToPool<T>(T obj) where T : MonoBehaviour, IPooledObject
        {
            if (!Application.isPlaying)
            {
                Object.Destroy(obj.gameObject);
                return;
            }

            obj.gameObject.transform.SetParent(null);
            obj.gameObject.SetActive(false);

            string name = obj.GetName();

            if (instance.pool.ContainsKey(name) == false)
            {
                instance.pool.Add(name, new Queue<IPooledObject>());
            }

            instance.poolGOs.Add(obj.GetGameObject());
            instance.pool[obj.GetName()].Enqueue(obj);
        }

        private T GetOrSpawnInstance<T>(T prefab, string name) where T : MonoBehaviour, IPooledObject
        {
            if (pool.ContainsKey(name) == false)
            {
                pool.Add(name, new Queue<IPooledObject>());
            }

            var ins = pool[name].Count > 0 ? pool[name].Dequeue() : null;

            if (ins == null)
            {
                ins = SpawnObject(prefab);
            }
            else
            {
                instance.poolGOs.Remove(ins.GetGameObject());
            }

            var result = (T)ins;

            PrepareObj(result, name);

            return result;
        }

        private T GetObject<T>(string name) where T : MonoBehaviour, IPooledObject
        {
            var ins = pool[name].Count > 0 ? pool[name].Dequeue() : null;

            if (ins == null)
            {
                ins = SpawnObject((T)prefabs[name]);
            }
            else
            {
                instance.poolGOs.Remove(ins.GetGameObject());
            }

            var result = (T)ins;

            PrepareObj(result, name);

            return result;
        }

        private void PrepareObj<T>(T obj, string name) where T : MonoBehaviour, IPooledObject
        {
            obj.gameObject.SetActive(true);
            obj.SetName(name);
        }

        private T SpawnObject<T>(T prefab) where T : MonoBehaviour, IPooledObject
        {
            var ins = Object.Instantiate(prefab);
            ins.gameObject.SetActive(false);
            return ins;
        }
    }
}
