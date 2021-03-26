using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framwork
{
    public sealed class AssetManagment : ReferenceManagment
    {
#if ADDRESSABLES
        public static void LoadAssetAsync<T>(string path, Action<T> callback, AssetType assetType = AssetType.Addressables) where T : Object
#else
        public static void LoadAssetAsync<T>(string path, Action<T> callback, AssetType assetType = AssetType.Resources) where T : Object
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
        public static void UnloadAsset(string path, AssetType assetType = AssetType.Addressables)
#else
        public static void UnloadAsset(string path, AssetType assetType = AssetType.Resources)
#endif
        {
            SubReference(path, assetType);
        }

        public static void UnloadAsset<T>(T asset) where T : Object
        {
            string path = CheckPath(asset, out AssetType assetType);
            SubReference(path, assetType);
        }

        public static void LinkAsset(Object root, params Object[] element)
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

        public static class Use
        {
#if ADDRESSABLES
            public static void AssetAsync<T>(string path, Action<T> callback, AssetType assetType = AssetType.Addressables) where T : Object
#else
            public static void AssetAsync<T>(string path, Action<T> callback, AssetType assetType = AssetType.Resources) where T : Object
#endif
            {
                LoadAssetAsync<T>(path, obj =>
                {
                    if (obj == null)
                        throw new NullReferenceException($"AssetManagment - {assetType} \"{path}\" is null");
                    AddReference(path, assetType);
                    callback?.Invoke(obj);
                }, assetType);
            }

#if ADDRESSABLES
            public static T Asset<T>(string path, AssetType assetType = AssetType.Addressables) where T : Object
#else
            public static T Asset<T>(string path, AssetType assetType = AssetType.Resources) where T : Object
#endif
            {
                T asset = GetAeest<T>(path, assetType);
                if (asset == null)
                    throw new NullReferenceException($"AssetManagment: {assetType} \"{path}\" dont load");
                AddReference(path, assetType);
                return asset;
            }
        }
    }
}