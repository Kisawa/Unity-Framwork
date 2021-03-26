using System;
using System.Collections.Generic;
using UnityEngine;
#if ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif
using Object = UnityEngine.Object;

namespace Framwork
{
    public abstract class ReferenceManagment
    {
        static Dictionary<string, int> R_ReferenceCount = new Dictionary<string, int>();
        static Dictionary<string, ResourceRequest> R_Handle = new Dictionary<string, ResourceRequest>();
#if ADDRESSABLES
        static Dictionary<string, int> A_ReferenceCount = new Dictionary<string, int>();
        static Dictionary<string, AsyncOperationHandle> A_Handles = new Dictionary<string, AsyncOperationHandle>();
#endif
        protected static Dictionary<(string, AssetType), List<(string, AssetType)>> Linked = new Dictionary<(string, AssetType), List<(string, AssetType)>>();
        protected static Dictionary<GameObject, (string, AssetType)> InstanceDic = new Dictionary<GameObject, (string, AssetType)>();
        protected static Dictionary<ObjectPool, (string, AssetType)> PoolDic = new Dictionary<ObjectPool, (string, AssetType)>();

        protected static int CheckReference(string path, AssetType assetType)
        {
            int reference = 0;
            switch (assetType)
            {
                case AssetType.Resources:
                    if (R_ReferenceCount.TryGetValue(path, out reference))
                        return reference;
                    break;
#if ADDRESSABLES
                case AssetType.Addressables:
                    if (A_ReferenceCount.TryGetValue(path, out reference))
                        return reference;
                    break;
#endif
            }
            return reference;
        }

        protected static void AddReference(string path, AssetType assetType)
        {
            switch (assetType)
            {
                case AssetType.Resources:
                    if (R_ReferenceCount.ContainsKey(path))
                        R_ReferenceCount[path]++;
                    else
                        R_ReferenceCount.Add(path, 1);
                    break;
#if ADDRESSABLES
                case AssetType.Addressables:
                    if (A_ReferenceCount.ContainsKey(path))
                        A_ReferenceCount[path]++;
                    else
                        A_ReferenceCount.Add(path, 1);
                    break;
#endif
            }
        }

        protected static bool SubReference(string path, AssetType assetType)
        {
            switch (assetType)
            {
                case AssetType.Resources:
                    if (R_ReferenceCount.ContainsKey(path) && --R_ReferenceCount[path] <= 0)
                    {
                        if (R_Handle.TryGetValue(path, out ResourceRequest request))
                        {
                            if ((request.asset as GameObject) == null)
                                Resources.UnloadAsset(request.asset);
                            R_Handle.Remove(path);
                        }
                        R_ReferenceCount.Remove(path);
                        UnlinkAsset(path, assetType);
                        return true;
                    }
                    break;
#if ADDRESSABLES
                case AssetType.Addressables:
                    if (A_ReferenceCount.ContainsKey(path) && --A_ReferenceCount[path] <= 0)
                    {
                        if (A_Handles.TryGetValue(path, out AsyncOperationHandle handle))
                        {
                            Addressables.Release(handle);
                            A_Handles.Remove(path);
                        }
                        A_ReferenceCount.Remove(path);
                        UnlinkAsset(path, assetType);
                        return true;
                    }
                    break;
#endif
            }
            return false;
        }

        protected static string CheckPath(Object obj, out AssetType assetType)
        {
            if (obj == null)
            {
                assetType = AssetType.Resources;
                return "";
            }
            foreach (var item in R_Handle)
            {
                if (item.Value.asset == obj)
                {
                    assetType = AssetType.Resources;
                    return item.Key;
                }
            }
#if ADDRESSABLES
            foreach (var item in A_Handles)
            {
                if ((item.Value.Result as Object) == obj)
                {
                    assetType = AssetType.Addressables;
                    return item.Key;
                }
            }
#endif
            assetType = AssetType.Resources;
            return "";
        }

        protected static T GetAeest<T>(string path, AssetType assetType) where T : Object
        {
            switch (assetType)
            {
                case AssetType.Resources:
                    if (R_Handle.TryGetValue(path, out ResourceRequest request) && request.isDone)
                        return request.asset as T;
                    break;
#if ADDRESSABLES
                case AssetType.Addressables:
                    if (A_Handles.TryGetValue(path, out AsyncOperationHandle handle) && handle.IsDone)
                        return handle.Result as T;
                    break;
#endif
            }
            return null;
        }

        protected static void ResourcesLoad<T>(string path, Action<T> callback) where T : Object
        {
            if (R_Handle.TryGetValue(path, out ResourceRequest request))
            {
                if (request.isDone)
                    callback?.Invoke(request.asset as T);
                else
                    request.completed += obj => callback?.Invoke(request.asset as T);
            }
            else
            {
                request = Resources.LoadAsync<T>(path);
                R_Handle.Add(path, request);
                R_ReferenceCount[path] = 0;
                request.completed += obj => callback?.Invoke(request.asset as T);
            }
        }

#if ADDRESSABLES
        protected static void AddressablesLoad<T>(string path, Action<T> callback) where T : Object
        {
            if (A_Handles.TryGetValue(path, out AsyncOperationHandle handle))
            {
                if (handle.IsDone)
                    callback?.Invoke(handle.Result as T);
                else
                    handle.Completed += obj => callback?.Invoke(handle.Result as T);
            }
            else
            {
                AsyncOperationHandle<T> _handle = Addressables.LoadAssetAsync<T>(path);
                A_Handles.Add(path, _handle);
                A_ReferenceCount[path] = 0;
                _handle.Completed += obj => callback?.Invoke(obj.Result);
            }
        }
#endif

        public static void LinkAsset(string rootPath, AssetType rootAssetType, params (string, AssetType)[] element)
        {
            if (element == null || element.Length == 0)
                return;
            (string, AssetType) item = (rootPath, rootAssetType);
            if (!Linked.TryGetValue((rootPath, rootAssetType), out List<(string, AssetType)> link))
            {
                link = new List<(string, AssetType)>();
                Linked.Add((rootPath, rootAssetType), link);
            }
            for (int i = 0; i < element.Length; i++)
            {
                (string, AssetType) _item = element[i];
                if (link.Contains(_item) || _item == item)
                    continue;
                link.Add(_item);
                AddReference(_item.Item1, _item.Item2);
            }
        }

        protected static void UnlinkAsset(string path, AssetType assetType)
        {
            (string, AssetType) root = (path, assetType);
            if (Linked.TryGetValue(root, out List<(string, AssetType)> link))
            {
                for (int i = 0; i < link.Count; i++)
                {
                    (string, AssetType) item = link[i];
                    SubReference(item.Item1, item.Item2);
                }
                Linked.Remove(root);
            }
        }
    }
}