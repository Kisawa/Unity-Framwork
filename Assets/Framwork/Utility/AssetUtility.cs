using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framwork
{
    public abstract class AssetUtility : ReferenceManagment
    {
        static Dictionary<string, AssetUtility> PrefabUtilityDictionary = new Dictionary<string, AssetUtility>();

        public AssetUtility() { }

        protected virtual void StartInject() { }

        protected virtual void EndInject() { }

        public void Load(Action<AssetUtility> callback = null)
        {
            StartInject();
            FieldInfo[] fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            int count = fields.Length + 1;
            Action action = () =>
            {
                if (--count <= 0)
                {
                    EndInject();
                    callback?.Invoke(this);
                }
            };
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo item = fields[i];
                if (item.IsLiteral || (!typeof(Object).IsAssignableFrom(item.FieldType) && item.FieldType != typeof(ObjectPool)))
                {
                    count--;
                    continue;
                }
                if (item.IsDefined(typeof(ResourcesAttribute), true))
                {
                    ResourcesAttribute attribute = item.GetCustomAttribute(typeof(ResourcesAttribute), true) as ResourcesAttribute;
                    ResourcesLoad<Object>(attribute.Path, obj =>
                    {
                        AddReference(attribute.Path, AssetType.Resources);
                        if (item.FieldType == typeof(ObjectPool))
                            item.SetValue(this, Activator.CreateInstance(item.FieldType, new object[] { obj, 0, default }));
                        else
                            item.SetValue(this, obj);
                        action.Invoke();
                    });
                }
#if ADDRESSABLES
                else if (item.IsDefined(typeof(AddressablesAttribute), true))
                {
                    AddressablesAttribute attribute = item.GetCustomAttribute(typeof(AddressablesAttribute), true) as AddressablesAttribute;
                    AddressablesLoad<Object>(attribute.Path, obj =>
                    {
                        AddReference(attribute.Path, AssetType.Addressables);
                        if (item.FieldType == typeof(ObjectPool))
                            item.SetValue(this, Activator.CreateInstance(item.FieldType, new object[] { obj, 0, default }));
                        else
                            item.SetValue(this, obj);
                        action.Invoke();
                    });
                }
#endif
                else
                    count--;
            }
            action.Invoke();
        }

        public void Unload(bool refreshMemory = false)
        {
            FieldInfo[] fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            bool resource = false;
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo item = fields[i];
                if (item.IsLiteral || (!typeof(Object).IsAssignableFrom(item.FieldType) && item.FieldType != typeof(ObjectPool)))
                    continue;
                if (item.IsDefined(typeof(ResourcesAttribute), true))
                {
                    resource = true;
                    ResourcesAttribute attribute = item.GetCustomAttribute(typeof(ResourcesAttribute), true) as ResourcesAttribute;
                    SubReference(attribute.Path, AssetType.Resources);
                    if (item.FieldType == typeof(ObjectPool))
                    {
                        ObjectPool pool = (ObjectPool)item.GetValue(this);
                        pool.Clear();
                    }
                    item.SetValue(this, null);
                }
#if ADDRESSABLES
                else if (item.IsDefined(typeof(AddressablesAttribute), true))
                {
                    AddressablesAttribute attribute = item.GetCustomAttribute(typeof(AddressablesAttribute), true) as AddressablesAttribute;
                    SubReference(attribute.Path, AssetType.Addressables);
                    if (item.FieldType == typeof(ObjectPool))
                    {
                        ObjectPool pool = (ObjectPool)item.GetValue(this);
                        pool.Clear();
                    }
                    item.SetValue(this, null);
                }
#endif
            }
            PrefabUtilityDictionary.Remove(GetType().Name);
            if (resource && refreshMemory)
                Resources.UnloadUnusedAssets();
        }

        public static T GetAssetUtility<T>() where T: AssetUtility
        {
            PrefabUtilityDictionary.TryGetValue(typeof(T).Name, out AssetUtility utility);
            return utility as T;
        }

        public static void Load<T>(Action<T> callback = null) where T : AssetUtility, new()
        {
            string key = typeof(T).Name;
            if (PrefabUtilityDictionary.TryGetValue(key, out AssetUtility utility))
            {
                callback?.Invoke(utility as T);
                Debug.LogWarning($"PrefabUtility: {key} has injected.");
            }
            else
            {
                T t = new T();
                t.Load(callback as Action<AssetUtility>);
                PrefabUtilityDictionary.Add(key, t);
            }
        }

        public static void LoadAll(Action<AssetUtility> prefabUtilityCallback = null, Action endInjectCallback = null, string[] ignoreTypeNames = null)
        {
            Type[] types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes().Where(y => typeof(AssetUtility).IsAssignableFrom(y) && y.IsClass && !y.IsAbstract)).ToArray();
            MethodInfo method = typeof(AssetUtility).GetMethod("Load", BindingFlags.Public | BindingFlags.Static);
            int typeLength = types.Length + 1;
            Action<AssetUtility> callback = (obj) =>
            {
                if (obj != null)
                    prefabUtilityCallback?.Invoke(obj);
                if (--typeLength <= 0)
                    endInjectCallback?.Invoke();
            };
            for (int i = 0; i < types.Length; i++)
            {
                if (ignoreTypeNames == null || !ignoreTypeNames.Contains(types[i].Name))
                    method.MakeGenericMethod(types[i]).Invoke(null, new object[] { callback });
                else
                    typeLength--;
            }
            callback.Invoke(null);
        }

        public static void UnloadAll(bool refreshMemory = false)
        {
            foreach (var item in PrefabUtilityDictionary)
                item.Value.Unload();
            PrefabUtilityDictionary.Clear();
            if(refreshMemory)
                Resources.UnloadUnusedAssets();
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ResourcesAttribute : Attribute 
    {
        public string Path;
        public ResourcesAttribute(string saveName)
        {
            Path = saveName;
        }
    }

#if ADDRESSABLES
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class AddressablesAttribute : Attribute
    {
        public string Path;
        public AddressablesAttribute(string saveName)
        {
            Path = saveName;
        }
    }
#endif
}