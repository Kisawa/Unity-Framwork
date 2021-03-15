using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framwork
{
    public class AssetGroup : ReferenceManagment
    {
        (string, AssetType) root;
        List<(string, AssetType)> paths;

        public AssetGroup()
        {
            paths = new List<(string, AssetType)>();
        }

        public AssetGroup(AssetType assetType, params string[] path)
        {
            paths = new List<(string, AssetType)>();
            for (int i = 0; i < path.Length; i++)
                paths.Add((path[i], assetType));
        }

        public AssetGroup(params (string, AssetType)[] path)
        {
            paths = new List<(string, AssetType)>();
            paths.AddRange(path);
        }

        public AssetGroup(AssetType assetType, string rootPath, params string[] element)
        {
            root = (rootPath, assetType);
            paths = new List<(string, AssetType)>();
            for (int i = 0; i < element.Length; i++)
                paths.Add((element[i], assetType));
        }

        public AssetGroup(string rootPath, AssetType rootAssetType, params (string, AssetType)[] element)
        {
            root = (rootPath, rootAssetType);
            paths = new List<(string, AssetType)>();
            paths.AddRange(element);
        }

        public void Load(Action callback)
        {
            int count = 1 + paths.Count;
            Action<Object> action = obj =>
            {
                if (--count <= 0)
                {
                    if (root != default)
                        LinkAsset(root.Item1, root.Item2, paths.ToArray());
                    callback?.Invoke();
                }
            };
            if (root != default)
            {
                count++;
                load(root.Item1, root.Item2, action);
            }
            for (int i = 0; i < paths.Count; i++)
            {
                (string, AssetType) item = paths[i];
                load(item.Item1, item.Item2, action);
            }
            action?.Invoke(null);
        }

        void load(string path, AssetType assetType, Action<Object> callback)
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
    }
}