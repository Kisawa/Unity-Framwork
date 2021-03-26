using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using LitJson;
using System.Threading.Tasks;

namespace Framwork
{
    public abstract class JsonDataUntility : ReferenceManagment
    {
        static Dictionary<string, JsonDataUntility> JsonDataDictionary = new Dictionary<string, JsonDataUntility>();

        public abstract string JsonAssetName { get; }

#if ADDRESSABLES
        public virtual AssetType AssetType { get => AssetType.Addressables; }
#else
        public virtual AssetType AssetType { get => AssetType.Resources; }
#endif

        public JsonDataUntility() { }

        protected virtual void StartInject() { }

        protected virtual void EndInject() { }

        public void Load(Action<JsonDataUntility> callback = null)
        {
            switch (AssetType)
            {
                case AssetType.Resources:
                    {
                        ResourcesLoad<TextAsset>(JsonAssetName, obj =>
                        {
                            if (obj == null)
                                throw new NullReferenceException($"JsonDataUntility - Asset {JsonAssetName} in {GetType().Name} is null");
                            AddReference(JsonAssetName, AssetType.Resources);
                            StartInject();
                            inject(obj.text);
                            EndInject();
                            SubReference(JsonAssetName, AssetType.Resources);
                            callback?.Invoke(this);
                        });
                        break;
                    }
#if ADDRESSABLES
                case AssetType.Addressables:
                    {
                        AddressablesLoad<TextAsset>(JsonAssetName, obj =>
                        {
                            if (obj == null)
                                throw new NullReferenceException($"JsonDataUntility - Asset {JsonAssetName} in {GetType().Name} is null");
                            AddReference(JsonAssetName, AssetType.Addressables);
                            StartInject();
                            inject(obj.text);
                            EndInject();
                            SubReference(JsonAssetName, AssetType.Addressables);
                            callback?.Invoke(this);
                        });
                        break;
                    }
#endif
            }
        }

        public static T GetJsonData<T>() where T : JsonDataUntility
        {
            JsonDataDictionary.TryGetValue(typeof(T).Name, out JsonDataUntility utility);
            return utility as T;
        }

        public static void LoadJsonData<T>(Action<T> callback = null) where T : JsonDataUntility, new()
        {
            string key = typeof(T).Name;
            if (JsonDataDictionary.TryGetValue(key, out JsonDataUntility utility))
            {
                callback?.Invoke(utility as T);
                Debug.LogWarning($"JsonDataUtility: {typeof(T).Name} has loaded.");
            }
            else
            {
                T t = new T();
                t.Load(callback as Action<JsonDataUntility>);
                JsonDataDictionary.Add(key, t);
            }
        }

        public static void LoadAllJsonData(Action<JsonDataUntility> jsonDataCallback = null, Action endLoadCallback = null, string[] ignoreTypeNames = null)
        {
            Type[] types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes().Where(y => typeof(JsonDataUntility).IsAssignableFrom(y) && y.IsClass && !y.IsAbstract)).ToArray();
            MethodInfo method = typeof(JsonDataUntility).GetMethod("LoadJsonData", BindingFlags.Public | BindingFlags.Static);
            int typeLength = types.Length + 1;
            Action<JsonDataUntility> callback = (obj) =>
            {
                if (obj != null)
                    jsonDataCallback?.Invoke(obj);
                if (--typeLength <= 0)
                    endLoadCallback?.Invoke();
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

        void inject(string jsonText)
        {
            JsonData data = JsonMapper.ToObject(jsonText);
            FieldInfo[] fieldInfos = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            Parallel.ForEach(fieldInfos, (item) =>
            {
                JsonData objData = data;
                if (item.IsDefined(typeof(JsonFieldGroupAttribute), true))
                {
                    if (item.IsDefined(typeof(JsonFieldAttribute), true))
                    {
                        JsonFieldAttribute jsonField = item.GetCustomAttribute(typeof(JsonFieldAttribute), true) as JsonFieldAttribute;
                        if (jsonField.ParentNames != null)
                        {
                            for (int i = 0; i < jsonField.ParentNames.Length; i++)
                                objData = checkJsonData(objData, jsonField.ParentNames[i]);
                        }
                    }
                    item.SetValue(this, newFieldData(item.FieldType, objData));
                }
                else if (item.IsDefined(typeof(JsonFieldAttribute), true))
                {
                    JsonFieldAttribute jsonField = item.GetCustomAttribute(typeof(JsonFieldAttribute), true) as JsonFieldAttribute;
                    if (jsonField.ParentNames != null)
                    {
                        for (int i = 0; i < jsonField.ParentNames.Length; i++)
                            objData = checkJsonData(objData, jsonField.ParentNames[i]);
                    }
                    objData = checkJsonData(objData, item.Name);
                    setFieldVal(this, item, objData);
                }
            });
        }

        void setFieldVal(object instance, FieldInfo fieldInfo, JsonData jsonData)
        {
            if (jsonData == null)
            {
                Debug.LogWarning($"{GetType().Name}: Field {fieldInfo.Name} of {fieldInfo.DeclaringType.Name} dont merge.");
                return;
            }
            if (fieldInfo.FieldType.IsPrimitive || fieldInfo.FieldType.Name == "String")
            {
                fieldInfo.SetValue(instance, Convert.ChangeType(jsonData.ToString(), fieldInfo.FieldType));
            }
            else if (fieldInfo.FieldType.IsArray)
            {
                if (jsonData.IsArray)
                {
                    Type arrayDataType = fieldInfo.FieldType.GetElementType();
                    Array arrayInstance = Array.CreateInstance(arrayDataType, jsonData.Count);
                    for (int i = 0; i < jsonData.Count; i++)
                        arrayInstance.SetValue(newFieldData(arrayDataType, jsonData[i]), i);
                    fieldInfo.SetValue(instance, arrayInstance);
                }
                else
                    Debug.LogWarning($"JsonDataUtility: {JsonAssetName} data of \"{fieldInfo.Name}\" is not a list.");
            }
            else if (fieldInfo.FieldType.Name == "List`1")
            {
                if (jsonData.IsArray)
                {
                    Type listDataType = fieldInfo.FieldType.GetGenericArguments()[0];
                    MethodInfo method = fieldInfo.FieldType.GetMethod("Add");
                    object listInstance = Activator.CreateInstance(fieldInfo.FieldType);
                    foreach (JsonData item in jsonData)
                        method.Invoke(listInstance, new object[] { newFieldData(listDataType, item) });
                    fieldInfo.SetValue(instance, listInstance);
                }
                else
                    Debug.LogWarning($"JsonDataUtility: {JsonAssetName} data of \"{fieldInfo.Name}\" is not a list.");
            }
            else
            {
                throw new Exception($"{GetType().Name} Type {fieldInfo.FieldType} cant be used.");
            }
        }

        object newFieldData(Type fieldType, JsonData jsonData)
        {
            if (!fieldType.IsArray && fieldType.IsClass && fieldType.GetConstructor(new Type[] { }) == null)
                throw new Exception($"{GetType().Name}: Type of \"{fieldType}\" dont have unreferenced constructors.");
            if (fieldType.IsPrimitive || fieldType.Name == "String" || fieldType.Name == "List`1" || fieldType.IsArray)
                throw new Exception($"{GetType().Name}: FieldGroup must be a custom data.");
            object instance = Activator.CreateInstance(fieldType);
            FieldInfo[] fieldInfos = fieldType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            Parallel.ForEach(fieldInfos, (item) =>
            {
                JsonData objData = jsonData;
                if (item.IsDefined(typeof(JsonFieldGroupAttribute), true))
                {
                    if (item.IsDefined(typeof(JsonFieldAttribute), true))
                    {
                        JsonFieldAttribute jsonField = item.GetCustomAttribute(typeof(JsonFieldAttribute), true) as JsonFieldAttribute;
                        if (jsonField.ParentNames != null)
                        {
                            for (int i = 0; i < jsonField.ParentNames.Length; i++)
                                objData = checkJsonData(objData, jsonField.ParentNames[i]);
                        }
                    }
                    item.SetValue(instance, newFieldData(item.FieldType, objData));
                }
                else if (item.IsDefined(typeof(JsonFieldAttribute), true))
                {
                    JsonFieldAttribute jsonField = item.GetCustomAttribute(typeof(JsonFieldAttribute), true) as JsonFieldAttribute;
                    if (jsonField.ParentNames != null)
                    {
                        for (int i = 0; i < jsonField.ParentNames.Length; i++)
                            objData = checkJsonData(objData, jsonField.ParentNames[i]);
                    }
                    objData = checkJsonData(objData, item.Name);
                    setFieldVal(instance, item, objData);
                }
                else
                {
                    objData = checkJsonData(jsonData, item.Name);
                    setFieldVal(instance, item, objData);
                }
            });
            return instance;
        }

        JsonData checkJsonData(JsonData data, string resKey)
        {
            if (data.IsObject)
            {
                if (data.ContainsKey(resKey))
                {
                    return data[resKey];
                }
                else
                {
                    foreach (string key in data.Keys)
                    {
                        JsonData obj = checkJsonData(data[key], resKey);
                        if (obj != null)
                            return obj;
                    }
                }
            }
            return null;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class JsonFieldAttribute : Attribute
    {
        public string[] ParentNames;
        public JsonFieldAttribute() { }
        public JsonFieldAttribute(params string[] parentNames)
        {
            ParentNames = parentNames;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class JsonFieldGroupAttribute : Attribute { }
}