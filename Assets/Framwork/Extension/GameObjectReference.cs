using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framwork
{
    public sealed class GameObjectReference : ReferenceManagment
    {
        static Dictionary<GameObject, (string, AssetType)> pathDic = new Dictionary<GameObject, (string, AssetType)>();

#if ADDRESSABLE
        public static void LoadGameObjectAsync(string path, Action<GameObject> callback, AssetType assetType = AssetType.Addressables)
#else
        public static void LoadGameObjectAsync(string path, Action<GameObject> callback, AssetType assetType = AssetType.Resources)
#endif
        {
            switch (assetType)
            {
                case AssetType.Resources:
                    ResourcesLoad(path, callback);
                    break;
#if ADDRESSABLE
                case AssetType.Addressables:
                    AddressablesLoad(path, callback);
                    break;
#endif
            }
        }

#if ADDRESSABLE
        public static GameObject Instantiate(string path, AssetType assetType = AssetType.Addressables)
#else
        public static GameObject Instantiate(string path, AssetType assetType = AssetType.Resources)
#endif
        {
            GameObject gameObject = GetAeest<GameObject>(path, assetType);
            if (gameObject == null)
                throw new NullReferenceException($"GameObjectReference: {assetType} \"{path}\" dont load");
            GameObject instance = Object.Instantiate(gameObject);
            pathDic[instance] = (path, assetType);
            AddReference(path, assetType);
            return instance;
        }

#if ADDRESSABLE
        public static GameObject Instantiate(string path, Transform parent, AssetType assetType = AssetType.Addressables)
#else
        public static GameObject Instantiate(string path, Transform parent, AssetType assetType = AssetType.Resources)
#endif
        {
            GameObject gameObject = GetAeest<GameObject>(path, assetType);
            if (gameObject == null)
                throw new NullReferenceException($"GameObjectReference: {assetType} \"{path}\" dont load");
            GameObject instance = Object.Instantiate(gameObject, parent);
            pathDic[instance] = (path, assetType);
            AddReference(path, assetType);
            return instance;
        }

#if ADDRESSABLE
        public static GameObject Instantiate(string path, Vector3 pos, Quaternion rotation, AssetType assetType = AssetType.Addressables)
#else
        public static GameObject Instantiate(string path, Vector3 pos, Quaternion rotation, AssetType assetType = AssetType.Resources)
#endif
        {
            GameObject gameObject = GetAeest<GameObject>(path, assetType);
            if (gameObject == null)
                throw new NullReferenceException($"GameObjectReference: {assetType} \"{path}\" dont load");
            GameObject instance = Object.Instantiate(gameObject, pos, rotation);
            pathDic[instance] = (path, assetType);
            AddReference(path, assetType);
            return instance;
        }

#if ADDRESSABLE
        public static GameObject Instantiate(string path, Vector3 pos, Quaternion rotation, Transform parent, AssetType assetType = AssetType.Addressables)
#else
        public static GameObject Instantiate(string path, Vector3 pos, Quaternion rotation, Transform parent, AssetType assetType = AssetType.Resources)
#endif
        {
            GameObject gameObject = GetAeest<GameObject>(path, assetType);
            if (gameObject == null)
                throw new NullReferenceException($"GameObjectReference: {assetType} \"{path}\" dont load");
            GameObject instance = Object.Instantiate(gameObject, pos, rotation, parent);
            pathDic[instance] = (path, assetType);
            AddReference(path, assetType);
            return instance;
        }

        public static GameObject Instantiate(GameObject prefab)
        {
            GameObject instance = Object.Instantiate(prefab);
            string path = CheckPath(prefab, out AssetType assetType);
            pathDic[instance] = (path, assetType);
            AddReference(path, assetType);
            return instance;
        }

        public static GameObject Instantiate(GameObject prefab, Transform parent)
        {
            GameObject instance = Object.Instantiate(prefab, parent);
            string path = CheckPath(prefab, out AssetType assetType);
            pathDic[instance] = (path, assetType);
            AddReference(path, assetType);
            return instance;
        }

        public static GameObject Instantiate(GameObject prefab, Vector3 pos, Quaternion rotation)
        {
            GameObject instance = Object.Instantiate(prefab, pos, rotation);
            string path = CheckPath(prefab, out AssetType assetType);
            pathDic[instance] = (path, assetType);
            AddReference(path, assetType);
            return instance;
        }

        public static GameObject Instantiate(GameObject prefab, Vector3 pos, Quaternion rotation, Transform parent)
        {
            GameObject instance = Object.Instantiate(prefab, pos, rotation, parent);
            string path = CheckPath(prefab, out AssetType assetType);
            pathDic[instance] = (path, assetType);
            AddReference(path, assetType);
            return instance;
        }

#if ADDRESSABLE
        public static void InstantiateAsync(string path, Action<GameObject> callbcak, AssetType assetType = AssetType.Addressables)
#else
        public static void InstantiateAsync(string path, Action<GameObject> callbcak, AssetType assetType = AssetType.Resources)
#endif
        {
            LoadGameObjectAsync(path, obj =>
            {
                GameObject instance = Object.Instantiate(obj);
                pathDic[instance] = (path, assetType);
                AddReference(path, assetType);
                callbcak?.Invoke(instance);
            }, assetType);
        }

#if ADDRESSABLE
        public static void InstantiateAsync(string path, Transform parent, Action<GameObject> callbcak, AssetType assetType = AssetType.Addressables)
#else
        public static void InstantiateAsync(string path, Transform parent, Action<GameObject> callbcak, AssetType assetType = AssetType.Resources)
#endif
        {
            LoadGameObjectAsync(path, obj =>
            {
                GameObject instance = Object.Instantiate(obj, parent);
                pathDic[instance] = (path, assetType);
                AddReference(path, assetType);
                callbcak?.Invoke(instance);
            }, assetType);
        }

#if ADDRESSABLE
        public static void InstantiateAsync(string path, Vector3 pos, Quaternion rotation, Action<GameObject> callbcak = null, AssetType assetType = AssetType.Addressables)
#else
        public static void InstantiateAsync(string path, Vector3 pos, Quaternion rotation, Action<GameObject> callbcak = null, AssetType assetType = AssetType.Resources)
#endif
        {
            LoadGameObjectAsync(path, obj =>
            {
                GameObject instance = Object.Instantiate(obj, pos, rotation);
                pathDic[instance] = (path, assetType);
                AddReference(path, assetType);
                callbcak?.Invoke(instance);
            }, assetType);
        }

#if ADDRESSABLE
        public static void InstantiateAsync(string path, Vector3 pos, Quaternion rotation, Transform parent, Action<GameObject> callbcak = null, AssetType assetType = AssetType.Addressables)
#else
        public static void InstantiateAsync(string path, Vector3 pos, Quaternion rotation, Transform parent, Action<GameObject> callbcak = null, AssetType assetType = AssetType.Resources)
#endif
        {
            LoadGameObjectAsync(path, obj =>
            {
                GameObject instance = Object.Instantiate(obj, pos, rotation, parent);
                pathDic[instance] = (path, assetType);
                AddReference(path, assetType);
                callbcak?.Invoke(instance);
            }, assetType);
        }


        public static void Destroy(GameObject obj)
        {
            if (obj == null)
                return;
            if (pathDic.TryGetValue(obj, out (string, AssetType) item))
            {
                if (linked.TryGetValue(item, out List<(string, AssetType)> link))
                {
                    for (int i = 0; i < link.Count; i++)
                    {
                        (string, AssetType) _item = link[i];
                        for (int j = 0; j < pathDic.Count; j++)
                        {
                            var linkInstance = pathDic.ElementAt(j);
                            if (linkInstance.Value == _item)
                            {
                                Destroy(linkInstance.Key);
                                j--;
                            }
                        }
                    }
                }
                SubReference(item.Item1, item.Item2);
                pathDic.Remove(obj);
            }
            Object.Destroy(obj);
        }

        public static void LinkAsset(GameObject root, params GameObject[] element)
        {
            string rootPath = CheckPath(root, out AssetType rootAssetType);
            (string, AssetType)[] elementItem = new (string, AssetType)[element.Length];
            for (int i = 0; i < element.Length; i++)
            {
                string elementPath = CheckPath(element[i], out AssetType elementAssetType);
                elementItem[i] = (elementPath, elementAssetType);
            }
            LinkAsset(rootPath, rootAssetType, elementItem);
        }
    }
}