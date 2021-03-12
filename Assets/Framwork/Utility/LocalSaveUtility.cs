using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framwork
{
    public abstract class LocalSaveUtility
    {
        static Type[] allLocalSaveTypes;
        static Type[] AllLocalSaveTypes
        {
            get
            {
                if (allLocalSaveTypes == null)
                {
                    allLocalSaveTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes().Where(y => typeof(LocalSaveUtility).IsAssignableFrom(y) && y.IsClass && !y.IsAbstract)).ToArray();
                }
                return allLocalSaveTypes;
            }
        }

        static Dictionary<string, LocalSaveUtility> DataInstanceDictionary = new Dictionary<string, LocalSaveUtility>();

        public LocalSaveUtility() { }

        protected virtual void Init() { }

        public void Refresh()
        {
            Type type = GetType();
            MethodInfo method = typeof(LocalSaveUtility).GetMethod("Inject", BindingFlags.NonPublic | BindingFlags.Static);
            method.MakeGenericMethod(type).Invoke(null, new object[] { this, null, Activator.CreateInstance(type) });
        }

        public static T GetLocalData<T>() where T : LocalSaveUtility, new()
        {
            return Inject<T>();
        }

        public static LocalSaveUtility GetLocalData(Type type)
        {
            if (!typeof(LocalSaveUtility).IsAssignableFrom(type) || !type.IsClass || type.IsAbstract)
                return null;
            string key = type.Name;
            if (DataInstanceDictionary.TryGetValue(key, out LocalSaveUtility utility))
                return utility;
            else
            {
                MethodInfo method = typeof(LocalSaveUtility).GetMethod("Inject", BindingFlags.Public | BindingFlags.Static);
                utility = method.MakeGenericMethod(type).Invoke(null, null) as LocalSaveUtility;
                return utility;
            }
        }

        public static void RefreshHasInjected()
        {
            if (DataInstanceDictionary.Count == 0)
                return;
            MethodInfo method = typeof(LocalSaveUtility).GetMethod("Inject", BindingFlags.NonPublic | BindingFlags.Static);
            ES3Reader reader = ES3.StartLoad();
            foreach (var item in DataInstanceDictionary.Values)
            {
                Type type = item.GetType();
                method.MakeGenericMethod(type).Invoke(null, new object[] { item, reader, Activator.CreateInstance(type) });
            }
            reader.EndLoad();
        }

        public static T Inject<T>() where T : LocalSaveUtility, new()
        {
            string key = typeof(T).Name;
            if (DataInstanceDictionary.TryGetValue(key, out LocalSaveUtility utility))
            {
                Debug.LogWarning($"LocalSaveUtility: {key} has injected.");
                return utility as T;
            }
            else
            {
                T t = new T();
                Inject(t);
                DataInstanceDictionary.Add(t.GetType().Name, t);
                return t;
            }
        }

        public static void InjectAll(Action<LocalSaveUtility> injectSingleLocalDataCallback = null, string[] ignoreTypeNames = null)
        {
            MethodInfo method = typeof(LocalSaveUtility).GetMethod("Inject", BindingFlags.NonPublic | BindingFlags.Static);
            ES3Reader reader = ES3.StartLoad();
            for (int i = 0; i < AllLocalSaveTypes.Length; i++)
            {
                Type item = AllLocalSaveTypes[i];
                if (ignoreTypeNames == null || !ignoreTypeNames.Contains(item.Name))
                {
                    if (DataInstanceDictionary.ContainsKey(item.Name))
                        Debug.LogWarning($"LocalSaveUtility: {item.Name} has injected.");
                    else
                    {
                        LocalSaveUtility data = (LocalSaveUtility)Activator.CreateInstance(item);
                        method.MakeGenericMethod(item).Invoke(null, new object[] { data, reader, null });
                        DataInstanceDictionary.Add(item.Name, data);
                        injectSingleLocalDataCallback?.Invoke(data);
                    }
                }
            }
            reader.EndLoad();
        }

        static void Inject<T>(T data, ES3Reader reader = null, T defaultData = null) where T : LocalSaveUtility
        {
            bool justDispose = false;
            if (reader == null)
            {
                reader = ES3.StartLoad();
                justDispose = true;
            }
            FieldInfo[] fieldInfos = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            MethodInfo method = typeof(ES3).GetMethod("TryToLoad");
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                FieldInfo item = fieldInfos[i];
                if (item.IsLiteral)
                    continue;
                if (item.IsDefined(typeof(WaitingFreeToSaveAttribute), true))
                {
                    checkUnsafeType(item);
                    WaitingFreeToSaveAttribute attribute = item.GetCustomAttribute(typeof(WaitingFreeToSaveAttribute), true) as WaitingFreeToSaveAttribute;
                    object[] value = new object[] { reader, attribute.SaveName, null };
                    if ((bool)method.MakeGenericMethod(item.FieldType).Invoke(null, value))
                        item.SetValue(data, value[2]);
                    else if (defaultData != null)
                        item.SetValue(data, item.GetValue(defaultData));
                }
            }
            if (justDispose)
                reader.EndLoad();
            data.Init();
        }

        static void checkUnsafeType(FieldInfo item)
        {
            Type type = item.FieldType;
            if (item.IsDefined(typeof(UnsafeAttribute), true))
                addUnsafeType(type);
            if (item.IsDefined(typeof(DepthUnsafeAttribute), true))
                addUnsafeType(type, true);
        }

        static void addUnsafeType(Type type, bool depth = false)
        {
            if (!ES3.UnsafeTypeList.Contains(type))
                ES3.UnsafeTypeList.Add(type);
            if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                addUnsafeType(elementType, depth);
            }
            else if (type.IsGenericType)
            {
                Type[] genericTypes = type.GetGenericArguments();
                for (int i = 0; i < genericTypes.Length; i++)
                    addUnsafeType(genericTypes[i], depth);
            }
            if (depth)
            {
                FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                for (int i = 0; i < fieldInfos.Length; i++)
                    checkUnsafeType(fieldInfos[i]);
            }
        }

        public static void Save<T>() where T : LocalSaveUtility
        {
            string typeName = typeof(T).Name;
            if (!DataInstanceDictionary.ContainsKey(typeName))
                throw new Exception($"LocalSaveUtility: {typeName} dont inject.");
            T t = DataInstanceDictionary[typeName] as T;
            Save(t);
        }

        public static void SaveAll()
        {
            if (DataInstanceDictionary.Count == 0)
                return;
            ES3Writer writer = ES3.StartSave();
            for (int i = 0; i < AllLocalSaveTypes.Length; i++)
            {
                Type item = AllLocalSaveTypes[i];
                if (DataInstanceDictionary.TryGetValue(item.Name, out LocalSaveUtility data))
                    Save(data, writer);
            }
            writer.EndSave();
        }

        static void Save<T>(T data, ES3Writer writer = null) where T : LocalSaveUtility
        {
            bool justDispose = false;
            if (writer == null)
            {
                writer = ES3.StartSave();
                justDispose = true;
            }
            MethodInfo method = typeof(ES3).GetMethod("ToSave");
            FieldInfo[] fieldInfos = data.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                FieldInfo item = fieldInfos[i];
                if (item.IsLiteral)
                    continue;
                if (item.IsDefined(typeof(WaitingFreeToSaveAttribute), true))
                {
                    Type type = item.FieldType;
                    string res = NoDefaultConstructorTppeName(type);
                    if (res != "")
                    {
                        Debug.LogError($"LocalSaveUtility: Type of \"{res}\" dont have default constructors.");
                        continue;
                    }
                    var val = item.GetValue(data);
                    WaitingFreeToSaveAttribute attribute = item.GetCustomAttribute(typeof(WaitingFreeToSaveAttribute), true) as WaitingFreeToSaveAttribute;
                    method.MakeGenericMethod(type).Invoke(null, new object[] { writer, attribute.SaveName, val });
                }
            }
            if (justDispose)
                writer.EndSave();
        }

        public static string NoDefaultConstructorTppeName(Type type)
        {
            if (type.IsClass)
            {
                if (type.IsArray)
                    return NoDefaultConstructorTppeName(type.GetElementType());
                else if (type.Name == "String")
                    return "";
                else
                {
                    if (type.IsGenericType)
                    {
                        Type[] genericTypes = type.GetGenericArguments();
                        for (int i = 0; i < genericTypes.Length; i++)
                        {
                            string res = NoDefaultConstructorTppeName(genericTypes[i]);
                            if (res != "")
                                return res;
                        }
                    }
                    else
                        return type.GetConstructor(new Type[] { }) == null ? type.Name : "";
                }
            }
            return "";
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class WaitingFreeToSaveAttribute : Attribute
    {
        public string SaveName;
        public WaitingFreeToSaveAttribute(string saveName)
        {
            SaveName = saveName;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class UnsafeAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class DepthUnsafeAttribute : Attribute { }
}