using System;
using System.Collections.Generic;
using UnityEngine;
#if ADDRESSABLE
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif
using Object = UnityEngine.Object;

namespace Framwork
{
    public abstract class ReferenceManagment
    {
        static Dictionary<string, int> R_ReferenceCount = new Dictionary<string, int>();
        static Dictionary<string, Type> R_TypeDic = new Dictionary<string, Type>();
        static Dictionary<string, ResourceRequest> R_Handle = new Dictionary<string, ResourceRequest>();
#if ADDRESSABLE
        static Dictionary<string, int> A_ReferenceCount = new Dictionary<string, int>();
        static Dictionary<string, AsyncOperationHandle> A_Handles = new Dictionary<string, AsyncOperationHandle>();
#endif
        protected static Dictionary<(string, AssetType), List<(string, AssetType)>> linked = new Dictionary<(string, AssetType), List<(string, AssetType)>>();

        protected static void AddReference(string path, AssetType assetType)
        {
            switch (assetType)
            {
                case AssetType.Resources:
                    if (R_ReferenceCount.ContainsKey(path))
                        R_ReferenceCount[path]++;
                    break;
#if ADDRESSABLE
                case AssetType.Addressables:
                    if (A_ReferenceCount.ContainsKey(path))
                        A_ReferenceCount[path]++;
                    break;
#endif
            }
        }

        public static void DebugInfo()
        {
            int i = 1;
        }

        protected static void SubReference(string path, AssetType assetType)
        {
            bool unload = false;
            switch (assetType)
            {
                case AssetType.Resources:
                    if (R_ReferenceCount.ContainsKey(path) && --R_ReferenceCount[path] <= 0)
                    {
                        if (R_Handle.TryGetValue(path, out ResourceRequest request))
                        {
                            if (R_TypeDic[path] != typeof(GameObject))
                                Resources.UnloadAsset(request.asset);
                            R_Handle.Remove(path);
                        }
                        R_ReferenceCount.Remove(path);
                        R_TypeDic.Remove(path);
                        unload = true;
                    }
                    break;
#if ADDRESSABLE
                case AssetType.Addressables:
                    if (A_ReferenceCount.ContainsKey(path) && --A_ReferenceCount[path] <= 0)
                    {
                        if (A_Handles.TryGetValue(path, out AsyncOperationHandle handle))
                        {
                            Addressables.Release(handle);
                            A_Handles.Remove(path);
                        }
                        A_ReferenceCount.Remove(path);
                        unload = true;
                    }
                    break;
#endif
            }
            if (!unload)
                return;
            (string, AssetType) root = (path, assetType);
            if (linked.TryGetValue(root, out List<(string, AssetType)> link))
            {
                for (int i = 0; i < link.Count; i++)
                {
                    (string, AssetType) item = link[i];
                    SubReference(item.Item1, item.Item2);
                }
                linked.Remove(root);
            }
        }

        protected static string CheckPath(Object obj, out AssetType assetType)
        {
            foreach (var item in R_Handle)
            {
                if (item.Value.asset == obj)
                {
                    assetType = AssetType.Resources;
                    return item.Key;
                }
            }
#if ADDRESSABLE
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
#if ADDRESSABLE
                case AssetType.Addressables:
                    if (A_Handles.TryGetValue(path, out AsyncOperationHandle handle))
                        return handle.Result as T;
                    break;
#endif
            }
            return null;
        }

        protected static void ResourcesLoad(string path, Action<Object> callback, Type type)
        {
            if (R_Handle.TryGetValue(path, out ResourceRequest request))
            {
                if (request.isDone)
                    callback?.Invoke(request.asset);
                else
                    request.completed += obj => callback?.Invoke(request.asset);
            }
            else
            {
                request = Resources.LoadAsync(path);
                R_Handle.Add(path, request);
                R_ReferenceCount[path] = 0;
                R_TypeDic[path] = type;
                request.completed += obj => callback?.Invoke(request.asset);
            }
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
                R_TypeDic[path] = typeof(T);
                request.completed += obj => callback?.Invoke(request.asset as T);
            }
        }

#if ADDRESSABLE
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

        protected static void LinkAsset(string rootPath, AssetType rootAssetType, params (string, AssetType)[] element)
        {
            if (element == null || element.Length == 0)
                return;
            (string, AssetType) item = (rootPath, rootAssetType);
            if (!linked.TryGetValue((rootPath, rootAssetType), out List<(string, AssetType)> link))
            {
                link = new List<(string, AssetType)>();
                linked.Add((rootPath, rootAssetType), link);
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
    }
}