using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
#if ADDRESSABLE
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

public abstract class DataTableUtility
{
    static Dictionary<string, DataTableUtility> DataTableDictionary = new Dictionary<string, DataTableUtility>();

    public abstract string TableAssetName { get; }

#if ADDRESSABLE
    protected virtual AssetType AssetType { get => AssetType.Addressable; }
#else
    protected virtual AssetType AssetType { get => AssetType.Resources; }
#endif

    protected int currentRowIndex;

    public DataTableUtility() { }

    public virtual void StartInject()
    { 
        
    }

    public abstract void InjectLine(params string[] currentLineTextList);

    protected virtual void EndInject()
    {
        
    }

    public static T GetDataTable<T>() where T : DataTableUtility, new()
    {
        T t = new T();
        if (DataTableDictionary.TryGetValue(typeof(T).Name, out DataTableUtility data))
        {
            t = data as T;
            return t;
        }
        else {
            return null;
        }
    }

    public static void LoadDataTable<T>(Action<T> callback = null) where T: DataTableUtility, new()
    {
        T t = new T();
        string key = typeof(T).Name;
        if (DataTableDictionary.TryGetValue(key, out DataTableUtility obj))
            Debug.LogWarning($"DataTableUtility: {typeof(T).Name} has loaded.");
        else
            LoadDataTable(t, callback);
    }

    public static void LoadAllDataTable(Action<DataTableUtility> dataTableCallback = null, Action endLoadCallback = null, string[] ignoreTypeName = null)
    {
        Type[] types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes().Where(y => typeof(DataTableUtility).IsAssignableFrom(y) && y.IsClass && !y.IsAbstract)).ToArray();
        MethodInfo method = typeof(DataTableUtility).GetMethod("LoadDataTable");
        int typeLength = types.Length + 1;
        Action<DataTableUtility> callback = (obj) =>
        {
            if (obj != null)
                dataTableCallback?.Invoke(obj);
            if (--typeLength <= 0)
                endLoadCallback?.Invoke();
        };
        for (int i = 0; i < types.Length; i++)
        {
            if (ignoreTypeName == null || !ignoreTypeName.Contains(types[i].Name))
                method.MakeGenericMethod(types[i]).Invoke(null, new object[] { callback });
            else
                typeLength--;
        }
        callback.Invoke(null);
    }

    static async void LoadDataTable<T>(T data, Action<T> callbcak = null) where T : DataTableUtility
    {
        switch (data.AssetType)
        {
            case AssetType.Resources:
                {
                    TextAsset txt = Resources.Load<TextAsset>(data.TableAssetName);
                    data.StartInject();
                    data.InjectData(txt.text);
                    data.EndInject();
                    string key = data.GetType().Name;
                    if (!DataTableDictionary.ContainsKey(key))
                        DataTableDictionary.Add(key, data);
                    callbcak?.Invoke(data);
                    break;
                }
#if ADDRESSABLE
            case AssetType.Addressable:
                {
                    AsyncOperationHandle<TextAsset> asyncTextAssetLoad = Addressables.LoadAssetAsync<TextAsset>(data.TableAssetName);
                    await asyncTextAssetLoad.Task;
                    if (asyncTextAssetLoad.Result == null)
                        throw new NullReferenceException($"DataTableUtility: {data.TableAssetName} is null asset.");
                    data.StartInject();
                    data.InjectData(asyncTextAssetLoad.Result.text);
                    data.EndInject();
                    string key = data.GetType().Name;
                    if (!DataTableDictionary.ContainsKey(key))
                        DataTableDictionary.Add(key, data);
                    callbcak?.Invoke(data);
                    Addressables.Release(asyncTextAssetLoad);
                    break;
                }
#endif
        }
    }

    void InjectData(string text) {
        string[] lineText = text.Trim().Split('\n');
        currentRowIndex = -1;
        for (int i = 0; i < lineText.Length; i++)
        {
            string item = lineText[i];
            if (!item.StartsWith("#"))
            {
                currentRowIndex++;
                InjectLine(item.Trim().Split('\t'));
            }
        }
    }
}