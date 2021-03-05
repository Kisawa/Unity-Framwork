using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if ADDRESSABLE
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public static class GameObjectReference
{
    static Dictionary<string, AsyncOperationHandle<GameObject>> Handles = new Dictionary<string, AsyncOperationHandle<GameObject>>();
    static Dictionary<string, int> Reference = new Dictionary<string, int>();
    static Dictionary<GameObject, string> InstanceObj = new Dictionary<GameObject, string>();
    static List<string> LastingAssetNames = new List<string>();
    static Dictionary<string, Action> ReleaseAssetCallback = new Dictionary<string, Action>();

    public static void LoadGameObjectAssetAsync(string assetName, bool isLasting, Action<GameObject> callback = null) {
        if (Handles.TryGetValue(assetName, out AsyncOperationHandle<GameObject> handle)) {
            if (handle.Status == AsyncOperationStatus.Succeeded)
                callback?.Invoke(handle.Result);
            else
                handle.Completed += obj => callback?.Invoke(obj.Result);
            return;
        }
        AsyncOperationHandle<GameObject> async = Addressables.LoadAssetAsync<GameObject>(assetName);
        async.Completed += obj => callback?.Invoke(obj.Result);
        Handles.Add(assetName, async);
        Reference.Add(assetName, 0);
        if (isLasting)
            LastingAssetNames.Add(assetName);
    }

    public static void LoadGameObjectAssetsAsync(string[] assetNames, bool isLasting, Action<GameObject> assetCallback = null, Action callback = null) {
        int count = assetNames.Length;
        Action<GameObject> action = (obj) =>
        {
            assetCallback?.Invoke(obj);
            if (--count <= 0)
                callback?.Invoke();
        };
        foreach (string item in assetNames)
        {
            if (Handles.TryGetValue(item, out AsyncOperationHandle<GameObject> _obj)) {
                AsyncOperationHandle<GameObject> handle = _obj;
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    action(_obj.Result);
                else
                    handle.Completed += obj => action(obj.Result);
                continue;
            }
            AsyncOperationHandle<GameObject> async = Addressables.LoadAssetAsync<GameObject>(item);
            async.Completed += obj => action(obj.Result);
            Handles.Add(item, async);
            Reference.Add(item, 0);
            if (isLasting)
                LastingAssetNames.Add(item);
        }
    }

    public static bool Contains(string assetName) {
        return Handles.ContainsKey(assetName);
    }

    public static GameObject Instantiate(string assetName) {
        GameObject obj = Object.Instantiate(Handles[assetName].Result);
        InstanceObj.Add(obj, assetName);
        AddReference(assetName);
        return obj;
    }

    public static GameObject Instantiate(string assetName, Transform parent)
    {
        GameObject obj = Object.Instantiate(Handles[assetName].Result, parent);
        InstanceObj.Add(obj, assetName);
        AddReference(assetName);
        return obj;
    }

    public static GameObject Instantiate(string assetName, Vector3 pos, Quaternion quaternion)
    {
        GameObject obj = Object.Instantiate(Handles[assetName].Result, pos, quaternion);
        InstanceObj.Add(obj, assetName);
        AddReference(assetName);
        return obj;
    }

    public static GameObject Instantiate(string assetName, Vector3 pos, Quaternion quaternion, Transform parent)
    {
        GameObject obj = Object.Instantiate(Handles[assetName].Result, pos, quaternion, parent);
        InstanceObj.Add(obj, assetName);
        AddReference(assetName);
        return obj;
    }

    public static GameObject Instantiate(GameObject prefab)
    {
        GameObject obj = Object.Instantiate(prefab);
        string assetName = Handles.Where(x => x.Value.Result == prefab).FirstOrDefault().Key;
        if (string.IsNullOrEmpty(assetName))
        {
            Debug.LogWarning($"GameObjectReference: Instantiate an other prefab with name \"{prefab.name}\"");
            return obj;
        }
        InstanceObj.Add(obj, assetName);
        AddReference(assetName);
        return obj;
    }

    public static GameObject Instantiate(GameObject prefab, Transform parent)
    {
        GameObject obj = Object.Instantiate(prefab, parent);
        string assetName = Handles.Where(x => x.Value.Result == prefab).FirstOrDefault().Key;
        if (string.IsNullOrEmpty(assetName))
        {
            Debug.LogWarning($"GameObjectReference: Instantiate an other prefab with name \"{prefab.name}\"");
            return obj;
        }
        InstanceObj.Add(obj, assetName);
        AddReference(assetName);
        return obj;
    }

    public static GameObject Instantiate(GameObject prefab, Vector3 pos, Quaternion quaternion)
    {
        GameObject obj = Object.Instantiate(prefab, pos, quaternion);
        string assetName = Handles.Where(x => x.Value.Result == prefab).FirstOrDefault().Key;
        if (string.IsNullOrEmpty(assetName))
        {
            Debug.LogWarning($"GameObjectReference: Instantiate an other prefab with name \"{prefab.name}\"");
            return obj;
        }
        InstanceObj.Add(obj, assetName);
        AddReference(assetName);
        return obj;
    }

    public static GameObject Instantiate(GameObject prefab, Vector3 pos, Quaternion quaternion, Transform parent)
    {
        GameObject obj = Object.Instantiate(prefab, pos, quaternion, parent);
        string assetName = Handles.Where(x => x.Value.Result == prefab).FirstOrDefault().Key;
        if (string.IsNullOrEmpty(assetName))
        {
            Debug.LogWarning($"GameObjectReference: Instantiate an other prefab with name \"{prefab.name}\"");
            return obj;
        }
        InstanceObj.Add(obj, assetName);
        AddReference(assetName);
        return obj;
    }

    public static void InstantiateAsync(string assetName, Action<GameObject> callbcak)
    {
        LoadGameObjectAssetAsync(assetName, false, obj => { GameObject _obj = Instantiate(assetName); callbcak?.Invoke(_obj); });
    }

    public static void InstantiateAsync(string assetName, Transform parent, Action<GameObject> callbcak)
    {
        LoadGameObjectAssetAsync(assetName, false, obj => { GameObject _obj = Instantiate(assetName, parent); callbcak?.Invoke(_obj); });
    }

    public static void InstantiateAsync(string assetName, Vector3 pos, Quaternion quaternion, Action<GameObject> callbcak = null)
    {
        LoadGameObjectAssetAsync(assetName, false, obj => { GameObject _obj = Instantiate(assetName, pos, quaternion); callbcak?.Invoke(_obj); });
    }

    public static void InstantiateAsync(string assetName, Vector3 pos, Quaternion quaternion, Transform parent, Action<GameObject> callbcak = null)
    {
        LoadGameObjectAssetAsync(assetName, false, obj => { GameObject _obj = Instantiate(assetName, pos, quaternion, parent); callbcak?.Invoke(_obj); });
    }

    public static void InstantiateAsync(int count, string assetName, Action<GameObject[]> callbcak)
    {
        LoadGameObjectAssetAsync(assetName, false, obj => 
        {
            GameObject[] objs = new GameObject[count];
            for (int i = 0; i < count; i++)
                objs[i] = Instantiate(assetName);
            callbcak?.Invoke(objs);
        });
    }

    public static void InstantiateAsync(int count, string assetName, Transform parent, Action<GameObject[]> callbcak)
    {
        LoadGameObjectAssetAsync(assetName, false, obj => 
        {
            GameObject[] objs = new GameObject[count];
            for (int i = 0; i < count; i++)
                objs[i] = Instantiate(assetName, parent);
            callbcak?.Invoke(objs);
        });
    }

    public static void InstantiateAsync(int count, string assetName, Vector3 pos, Quaternion quaternion, Action<GameObject[]> callbcak = null)
    {
        LoadGameObjectAssetAsync(assetName, false, obj => 
        {
            GameObject[] objs = new GameObject[count];
            for (int i = 0; i < count; i++)
                objs[i] = Instantiate(assetName, pos, quaternion);
            callbcak?.Invoke(objs);
        });
    }

    public static void InstantiateAsync(int count, string assetName, Vector3 pos, Quaternion quaternion, Transform parent, Action<GameObject[]> callbcak = null)
    {
        LoadGameObjectAssetAsync(assetName, false, obj => 
        {
            GameObject[] objs = new GameObject[count];
            for (int i = 0; i < count; i++)
                objs[i] = Instantiate(assetName, pos, quaternion, parent);
            callbcak?.Invoke(objs);
        });
    }


    public static void Destroy(GameObject obj) {
        if (obj == null)
            return;
        if (InstanceObj.TryGetValue(obj, out string str)) {
            SubReference(str);
            InstanceObj.Remove(obj);
        }
        Object.Destroy(obj);
    }

    public static void Release(string assetName) {
        if (Handles.TryGetValue(assetName, out AsyncOperationHandle<GameObject> _obj)) {
            Addressables.Release(_obj);
            Handles.Remove(assetName);
            Reference.Remove(assetName);
            if (LastingAssetNames.Contains(assetName))
                LastingAssetNames.Remove(assetName);
            GameObject[] removeKeys = InstanceObj.Where(x => x.Value == assetName).Select(x => x.Key).ToArray();
            for (int i = 0; i < removeKeys.Length; i++)
                InstanceObj.Remove(removeKeys[i]);
        }
    }

    public static void AddReference(string assetName) {
        Reference[assetName]++;
    }

    public static void SubReference(string assetName)
    {
        if (!LastingAssetNames.Contains(assetName) && --Reference[assetName] <= 0)
        {
            Addressables.Release(Handles[assetName]);
            Handles.Remove(assetName);
            Reference.Remove(assetName);
            if (ReleaseAssetCallback.TryGetValue(assetName, out Action callbcak))
                callbcak?.Invoke();
        }
    }

    public static void AddReleaseAssetCallback(string assetName, Action callback) {
        if (ReleaseAssetCallback.TryGetValue(assetName, out Action action))
            action += callback;
        else
            ReleaseAssetCallback[assetName] = callback;
    }

    public static void SubReleaseAssetCallback(string assetName, Action callback) {
        if (ReleaseAssetCallback.TryGetValue(assetName, out Action action))
            action -= callback;
    }
}
#endif