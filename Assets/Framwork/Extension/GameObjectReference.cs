using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framwork
{
    public sealed class GameObjectReference : ReferenceManagment
    {
#if ADDRESSABLES
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
#if ADDRESSABLES
                case AssetType.Addressables:
                    AddressablesLoad(path, callback);
                    break;
#endif
            }
        }

#if ADDRESSABLES
        public static GameObject Instantiate(string path, AssetType assetType = AssetType.Addressables)
#else
        public static GameObject Instantiate(string path, AssetType assetType = AssetType.Resources)
#endif
        {
            GameObject gameObject = GetAeest<GameObject>(path, assetType);
            if (gameObject == null)
                throw new NullReferenceException($"GameObjectReference: {assetType} \"{path}\" dont load");
            GameObject instance = Object.Instantiate(gameObject);
            InstanceDic[instance] = (path, assetType);
            AddReference(path, assetType);
            return instance;
        }

#if ADDRESSABLES
        public static GameObject Instantiate(string path, Transform parent, AssetType assetType = AssetType.Addressables)
#else
        public static GameObject Instantiate(string path, Transform parent, AssetType assetType = AssetType.Resources)
#endif
        {
            GameObject gameObject = GetAeest<GameObject>(path, assetType);
            if (gameObject == null)
                throw new NullReferenceException($"GameObjectReference: {assetType} \"{path}\" dont load");
            GameObject instance = Object.Instantiate(gameObject, parent);
            InstanceDic[instance] = (path, assetType);
            AddReference(path, assetType);
            return instance;
        }

#if ADDRESSABLES
        public static GameObject Instantiate(string path, Vector3 pos, Quaternion rotation, AssetType assetType = AssetType.Addressables)
#else
        public static GameObject Instantiate(string path, Vector3 pos, Quaternion rotation, AssetType assetType = AssetType.Resources)
#endif
        {
            GameObject gameObject = GetAeest<GameObject>(path, assetType);
            if (gameObject == null)
                throw new NullReferenceException($"GameObjectReference: {assetType} \"{path}\" dont load");
            GameObject instance = Object.Instantiate(gameObject, pos, rotation);
            InstanceDic[instance] = (path, assetType);
            AddReference(path, assetType);
            return instance;
        }

#if ADDRESSABLES
        public static GameObject Instantiate(string path, Vector3 pos, Quaternion rotation, Transform parent, AssetType assetType = AssetType.Addressables)
#else
        public static GameObject Instantiate(string path, Vector3 pos, Quaternion rotation, Transform parent, AssetType assetType = AssetType.Resources)
#endif
        {
            GameObject gameObject = GetAeest<GameObject>(path, assetType);
            if (gameObject == null)
                throw new NullReferenceException($"GameObjectReference: {assetType} \"{path}\" dont load");
            GameObject instance = Object.Instantiate(gameObject, pos, rotation, parent);
            InstanceDic[instance] = (path, assetType);
            AddReference(path, assetType);
            return instance;
        }

        public static GameObject Instantiate(GameObject prefab)
        {
            GameObject instance = Object.Instantiate(prefab);
            string path = CheckPath(prefab, out AssetType assetType);
            InstanceDic[instance] = (path, assetType);
            AddReference(path, assetType);
            return instance;
        }

        public static GameObject Instantiate(GameObject prefab, Transform parent)
        {
            GameObject instance = Object.Instantiate(prefab, parent);
            string path = CheckPath(prefab, out AssetType assetType);
            InstanceDic[instance] = (path, assetType);
            AddReference(path, assetType);
            return instance;
        }

        public static GameObject Instantiate(GameObject prefab, Vector3 pos, Quaternion rotation)
        {
            GameObject instance = Object.Instantiate(prefab, pos, rotation);
            string path = CheckPath(prefab, out AssetType assetType);
            InstanceDic[instance] = (path, assetType);
            AddReference(path, assetType);
            return instance;
        }

        public static GameObject Instantiate(GameObject prefab, Vector3 pos, Quaternion rotation, Transform parent)
        {
            GameObject instance = Object.Instantiate(prefab, pos, rotation, parent);
            string path = CheckPath(prefab, out AssetType assetType);
            InstanceDic[instance] = (path, assetType);
            AddReference(path, assetType);
            return instance;
        }

#if ADDRESSABLES
        public static void InstantiateAsync(string path, Action<GameObject> callbcak, AssetType assetType = AssetType.Addressables)
#else
        public static void InstantiateAsync(string path, Action<GameObject> callbcak, AssetType assetType = AssetType.Resources)
#endif
        {
            LoadGameObjectAsync(path, obj =>
            {
                GameObject instance = Object.Instantiate(obj);
                InstanceDic[instance] = (path, assetType);
                AddReference(path, assetType);
                callbcak?.Invoke(instance);
            }, assetType);
        }

#if ADDRESSABLES
        public static void InstantiateAsync(string path, Transform parent, Action<GameObject> callbcak, AssetType assetType = AssetType.Addressables)
#else
        public static void InstantiateAsync(string path, Transform parent, Action<GameObject> callbcak, AssetType assetType = AssetType.Resources)
#endif
        {
            LoadGameObjectAsync(path, obj =>
            {
                GameObject instance = Object.Instantiate(obj, parent);
                InstanceDic[instance] = (path, assetType);
                AddReference(path, assetType);
                callbcak?.Invoke(instance);
            }, assetType);
        }

#if ADDRESSABLES
        public static void InstantiateAsync(string path, Vector3 pos, Quaternion rotation, Action<GameObject> callbcak = null, AssetType assetType = AssetType.Addressables)
#else
        public static void InstantiateAsync(string path, Vector3 pos, Quaternion rotation, Action<GameObject> callbcak = null, AssetType assetType = AssetType.Resources)
#endif
        {
            LoadGameObjectAsync(path, obj =>
            {
                GameObject instance = Object.Instantiate(obj, pos, rotation);
                InstanceDic[instance] = (path, assetType);
                AddReference(path, assetType);
                callbcak?.Invoke(instance);
            }, assetType);
        }

#if ADDRESSABLES
        public static void InstantiateAsync(string path, Vector3 pos, Quaternion rotation, Transform parent, Action<GameObject> callbcak = null, AssetType assetType = AssetType.Addressables)
#else
        public static void InstantiateAsync(string path, Vector3 pos, Quaternion rotation, Transform parent, Action<GameObject> callbcak = null, AssetType assetType = AssetType.Resources)
#endif
        {
            LoadGameObjectAsync(path, obj =>
            {
                GameObject instance = Object.Instantiate(obj, pos, rotation, parent);
                InstanceDic[instance] = (path, assetType);
                AddReference(path, assetType);
                callbcak?.Invoke(instance);
            }, assetType);
        }


        public static void Destroy(GameObject obj)
        {
            if (obj == null)
                return;
            UnlinkInstance(obj);
            if (InstanceDic.TryGetValue(obj, out (string, AssetType) item))
            {
                if (Linked.TryGetValue(item, out List<(string, AssetType)> link))
                {
                    if (SubReference(item.Item1, item.Item2))
                    {
                        for (int i = 0; i < link.Count; i++)
                        {
                            (string, AssetType) _item = link[i];
                            for (int j = 0; j < InstanceDic.Count; j++)
                            {
                                var linkInstance = InstanceDic.ElementAt(j);
                                if (linkInstance.Value == _item)
                                {
                                    if (CheckReference(linkInstance.Value.Item1, linkInstance.Value.Item2) < 1)
                                    {
                                        Destroy(linkInstance.Key);
                                        j--;
                                    }
                                }
                            }
                            for (int j = 0; j < PoolDic.Count; j++)
                            {
                                var linkPool = PoolDic.ElementAt(j);
                                if (linkPool.Value == _item)
                                {
                                    if (CheckReference(_item.Item1, _item.Item2) < 2)
                                    {
                                        linkPool.Key.Clear();
                                        j--;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                    SubReference(item.Item1, item.Item2);
                InstanceDic.Remove(obj);
            }
            Object.Destroy(obj);
        }

        static Dictionary<GameObject, int> g_referenceCount = new Dictionary<GameObject, int>();
        static Dictionary<GameObject, List<GameObject>> g_linked = new Dictionary<GameObject, List<GameObject>>();
        static Dictionary<ObjectPool, int> p_referenceCount = new Dictionary<ObjectPool, int>();
        static Dictionary<GameObject, List<ObjectPool>> p_linked = new Dictionary<GameObject, List<ObjectPool>>();

        public static void LinkInstance(GameObject root, params GameObject[] element)
        {
            if (root == null)
                return;
            if (!g_linked.TryGetValue(root, out List<GameObject> linkInstance))
            {
                linkInstance = new List<GameObject>();
                g_linked.Add(root, linkInstance);
            }
            for (int i = 0; i < element.Length; i++)
            {
                GameObject el = element[i];
                if (!linkInstance.Contains(el))
                {
                    linkInstance.Add(el);
                    if (g_referenceCount.TryGetValue(el, out int reference))
                        g_referenceCount[el] = reference + 1;
                    else
                        g_referenceCount.Add(el, 1);
                }
            }
        }

        public static void LinkInstance(GameObject root, params ObjectPool[] element)
        {
            if (root == null)
                return;
            if (!p_linked.TryGetValue(root, out List<ObjectPool> linkPool))
            {
                linkPool = new List<ObjectPool>();
                p_linked.Add(root, linkPool);
            }
            for (int i = 0; i < element.Length; i++)
            {
                ObjectPool el = element[i];
                if (!linkPool.Contains(el))
                {
                    linkPool.Add(el);
                    if (p_referenceCount.TryGetValue(el, out int reference))
                        p_referenceCount[el] = reference + 1;
                    else
                        p_referenceCount.Add(el, 1);
                }
            }
        }

        public static void UnlinkInstance(GameObject root)
        {
            if (root == null)
                return;
            if (g_linked.TryGetValue(root, out List<GameObject> linkInstance))
            {
                for (int i = 0; i < linkInstance.Count; i++)
                {
                    GameObject instance = linkInstance[i];
                    int reference = --g_referenceCount[instance];
                    if (reference < 1)
                    {
                        g_referenceCount.Remove(instance);
                        Destroy(instance);
                    }
                }
                g_linked.Remove(root);
            }
            if (p_linked.TryGetValue(root, out List<ObjectPool> linkPool))
            {
                for (int i = 0; i < linkPool.Count; i++)
                {
                    ObjectPool pool = linkPool[i];
                    int reference = --p_referenceCount[pool];
                    if (reference < 1)
                    {
                        p_referenceCount.Remove(pool);
                        pool.Clear();
                    }
                }
                p_linked.Remove(root);
            }
        }
    }
}