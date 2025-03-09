using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HeroicEngine.Systems.DI
{
    [AttributeUsage(AttributeTargets.Field)]
    public class InjectAttribute : Attribute { };

    public interface IInjectable
    {
        void PostInject();
    }

    public class InjectionManager : MonoBehaviour
    {
        private static List<ISystem> SystemsContainer = new List<ISystem>();
        private static Dictionary<Type, IInjectable> ObjectsContainer = new Dictionary<Type, IInjectable>();

        public static bool ContainsSystem(Type systemType)
        {
            return SystemsContainer.FindIndex(x => systemType.IsInstanceOfType(x)) >= 0;
        }

        public static void RegisterSystem<T>(T systemInstance) where T : MonoBehaviour, ISystem
        {
            if (!SystemsContainer.Contains(systemInstance))
            {
                SystemsContainer.Add(systemInstance);

                foreach (ISystem sys in SystemsContainer)
                {
                    InjectTo(sys);
                }
            }
        }

        public static void UnregisterSystem<T>(T systemInstance) where T : MonoBehaviour, ISystem
        {
            if (SystemsContainer.Contains(systemInstance))
            {
                SystemsContainer.Remove(systemInstance);
            }
        }

        public static T CreateGameObject<T>() where T : MonoBehaviour, IInjectable
        {
            GameObject go = new GameObject();
            T obj = go.AddComponent<T>();

            InjectTo(obj);
            obj.PostInject();
            RegisterObject(obj);

            return obj;
        }

        public static T CreateObject<T>() where T : IInjectable
        {
            T obj = default;

            InjectTo(obj);
            obj.PostInject();
            RegisterObject(obj);
            
            return obj;
        }

        public static void RegisterObject<T>(T obj) where T : IInjectable
        {
            if (obj == null)
            {
                obj = default;
                InjectTo(obj);
            }

            Type type = typeof(T);

            if (!ObjectsContainer.ContainsKey(type))
            {
                ObjectsContainer.Add(type, obj);
            }
            else
            {
                ObjectsContainer[type] = obj;
            }

            foreach (ISystem sys in SystemsContainer)
            {
                InjectTo(sys);
            }

            foreach (var o in ObjectsContainer)
            {
                InjectTo(o.Value);
            }

            obj.PostInject();
        }

        public static void InjectTo<T>(T instance)
        {
            InjectTo(instance, typeof(T));
            InjectTo(instance, instance.GetType());
        }

        private static void InjectTo<T>(T instance, Type monoType)
        {
            FieldInfo[] objectFields = monoType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            for (int i = 0; i < objectFields.Length; i++)
            {
                InjectAttribute attribute = Attribute.GetCustomAttribute(objectFields[i], typeof(InjectAttribute)) as InjectAttribute;

                if (attribute != null)
                {
                    Type injectType = objectFields[i].FieldType;
                    ISystem system = SystemsContainer.Find(x => injectType.IsInstanceOfType(x));

                    if (system != null)
                    {
                        objectFields[i].SetValue(instance, system);
                    }
                    else if (ObjectsContainer.ContainsKey(injectType))
                    {
                        object obj = ObjectsContainer[injectType];

                        if (obj != null)
                        {
                            objectFields[i].SetValue(instance, obj);
                        }
                    }
                }
            }
        }

        private void Awake()
        {
            InitializeSystems();
            DontDestroyOnLoad(gameObject);
        }

        private static void InitializeSystems()
        {
            SystemsContainer.AddRange(FindObjectsOfType<MonoBehaviour>(true).OfType<ISystem>());

            foreach (ISystem sys in SystemsContainer)
            {
                InjectTo(sys);
                GameObject go = (sys as MonoBehaviour).gameObject;
                DontDestroyOnLoad(go);
            }
        }
    }
}