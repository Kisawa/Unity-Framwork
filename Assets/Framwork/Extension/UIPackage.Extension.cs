using FairyGUI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Framwork;
#if ADDRESSABLE
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace FairyGUI {
    public partial class UIPackage
    {
#if ADDRESSABLE
        static Dictionary<string, AsyncOperationHandle> AsyncHandleDictionary = new Dictionary<string, AsyncOperationHandle>();
#endif
        static Dictionary<string, int> ReferenceDictionary = new Dictionary<string, int>();
        static Dictionary<string, Action<UIPackage>> PackageCallbackList = new Dictionary<string, Action<UIPackage>>();
        int relyFileCount;
        int RelyFileCount
        {
            get => relyFileCount;
            set
            {
                relyFileCount = value;
                if (relyFileCount == -1) {
                    PackageCallbackList[name]?.Invoke(this);
                }
            }
        }

        public static void CreateFguiObjectWithType(AssetType assetType, string pkgName, string resName, Action<GObject> callback)
        {
            UIPackage pkg = GetByName(pkgName);
            if (pkg == null)
            {
                if (PackageCallbackList.TryGetValue(pkgName, out Action<UIPackage> action))
                {
                    Action<UIPackage> newPackageCallback = (_pkg) => { callback?.Invoke(_pkg.CreateObject(resName)); AddReference(pkgName); };
                    action += newPackageCallback;
                }
                else {
                    AddPackageWithType(assetType, pkgName, (_pkg) =>
                    {
                        callback?.Invoke(_pkg.CreateObject(resName));
                        AddReference(pkgName);
                    });
                }
            }
            else {
                if (pkg.RelyFileCount != -1)
                    PackageCallbackList[pkg.name] += (_pkg) => { callback?.Invoke(_pkg.CreateObject(resName)); AddReference(pkgName); };
                else {
                    callback?.Invoke(pkg.CreateObject(resName));
                    AddReference(pkgName);
                }
            }
        }

        public static GObject CreateFguiObject(string pkgName, string resName) 
        {
            UIPackage pkg = GetByName(pkgName);
            if (pkg == null)
                throw new Exception($"UIPackage.Extension: {pkgName} dont load.");
            else {
                AddReference(pkgName);
                return pkg.CreateObject(resName);
            }
        }

        public async static void AddPackageWithType(AssetType assetType, string assetName, Action<UIPackage> callback = null)
        {
            if (PackageCallbackList.ContainsKey(assetName)) {
                Debug.LogWarning($"UIPackage.Extension: Dont call repeatedly while {assetName} has inited");
                return;
            }
            TextAsset asset = null;
            switch (assetType)
            {
                case AssetType.Resources:
                    PackageCallbackList.Add(assetName, callback);
                    asset = Resources.Load<TextAsset>($"FguiAssets/{assetName}_fui");
                    break;
#if ADDRESSABLE
                case AssetType.Addressable:
                    PackageCallbackList.Add(assetName, callback);
                    AsyncOperationHandle<TextAsset> asyncPackageLoad = Addressables.LoadAssetAsync<TextAsset>($"{assetName}_fui");
                    await asyncPackageLoad.Task;
                    AsyncHandleDictionary.Add(assetName, asyncPackageLoad);
                    asset = asyncPackageLoad.Result as TextAsset;
                    break;
#endif
            }
            if(asset == null)
                throw new Exception("UIPackage.Extension: Cannot load ui package in '" + assetName + "'");
            ByteBuffer buffer = new ByteBuffer(asset.bytes);
            UIPackage pkg = new UIPackage();
            pkg._loadFunc = _loadFromResourcesPath;
            pkg._assetPath = assetName;
            if (pkg.LoadPackage(buffer, assetName, assetName))
            {
                _packageInstById[pkg.id] = pkg;
                _packageInstByName[pkg.name] = pkg;
                _packageInstById[assetName] = pkg;
                _packageList.Add(pkg);
            }
            pkg.LoadAllAssetsOfPackageWithType(assetType);
        }

        public static bool CheckPackageIsLoaded(string pkgName)
        {
            return PackageCallbackList.ContainsKey(pkgName);
        }

        public static bool CheckPackageIsLoading(string pkgName) {
            if (CheckPackageIsLoaded(pkgName))
            {
                UIPackage pkg = GetByName(pkgName);
                if (pkg == null)
                {
                    return true;
                }
                else if (pkg.RelyFileCount > -1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else {
                return false;
            }
        }

        public static void AddPackageLoadedCallback(string pkgName, Action<UIPackage> callback) {
            if (CheckPackageIsLoaded(pkgName))
            {
                if (CheckPackageIsLoading(pkgName))
                {
                    if (PackageCallbackList[pkgName] == null)
                        PackageCallbackList[pkgName] = callback;
                    else
                        PackageCallbackList[pkgName] += callback;
                }
                else {
                    Debug.LogWarning($"UIPackage.Extension: {pkgName} is not loading");
                }
            }
            else {
                Debug.LogWarning($"UIPackage.Extension: {pkgName} is not loading");
            }
        }

        public void LoadAllAssetsOfPackageWithType(AssetType assetType)
        {
            var items = _items.Where(x => x.file != null);
            RelyFileCount = items.Count() - 1;
            foreach (PackageItem item in items)
                GetItemAssetWithType(assetType, item);
        }

        public void GetItemAssetWithType(AssetType assetType, PackageItem item)
        {
            switch (item.type)
            {
                case PackageItemType.Atlas:
                    if (item.texture == null)
                        LoadAtlasWithType(assetType, item);
                    break;
                case PackageItemType.Sound:
                    if (item.audioClip == null)
                        LoadSoundWithType(assetType, item);
                    break;
                default:
                    --RelyFileCount;
                    Debug.LogError($"UIPackage.Extension: No code of {item.type}");
                    break;
            }
        }

        async void LoadAtlasWithType(AssetType assetType, PackageItem item)
        {
            string assetReferenceName = item.file.Substring(0, item.file.LastIndexOf('.'));
            Texture tex = null;
            switch (assetType)
            {
                case AssetType.Resources:
                    tex = Resources.Load<Texture>($"FguiAssets/{assetReferenceName}");
                    break;
#if ADDRESSABLE
                case AssetType.Addressable:
                    AsyncOperationHandle<Texture> asyncTextureLoad = Addressables.LoadAssetAsync<Texture>(assetReferenceName);
                    await asyncTextureLoad.Task;
                    AsyncHandleDictionary.Add(item.file, asyncTextureLoad);
                    tex = asyncTextureLoad.Result as Texture;
                    break;
#endif
            }
            Texture alphaTex = null;
            DestroyMethod dm = DestroyMethod.Unload;
            if (tex == null)
            {
                Debug.LogWarning("UIPackage.Extension: Texture '" + item.file + "' not found in " + name);
            }
            else if (!(tex is Texture2D))
            {
                Debug.LogWarning("UIPackage.Extension: Settings for '" + item.file + "' is wrong! Correct values are: (Texture Type=Default, Texture Shape=2D)");
                tex = null;
            }
            else
            {
                if (((Texture2D)tex).mipmapCount > 1)
                    Debug.LogWarning("UIPackage.Extension: Settings for '" + item.file + "' is wrong! Correct values are: (Generate Mip Maps=unchecked)");
            }

            if (tex == null)
            {
                tex = NTexture.CreateEmptyTexture();
                dm = DestroyMethod.Destroy;
            }

            if (item.texture == null)
            {
                item.texture = new NTexture(tex, alphaTex, (float)tex.width / item.width, (float)tex.height / item.height);
                item.texture.destroyMethod = dm;
            }
            else
            {
                item.texture.Reload(tex, alphaTex);
                item.texture.destroyMethod = dm;
            }
            --RelyFileCount;
        }

        async void LoadSoundWithType(AssetType assetType, PackageItem item)
        {
            DestroyMethod dm = DestroyMethod.Unload;
            if (_resBundle != null)
                dm = DestroyMethod.None;
            AudioClip audioClip = null;
            string assetReferenceName = item.file.Substring(0, item.file.LastIndexOf('.'));
            switch (assetType)
            {
                case AssetType.Resources:
                    audioClip = Resources.Load<AudioClip>($"FguiAssets/{assetReferenceName}");
                    break;
#if ADDRESSABLE
                case AssetType.Addressable:
                    AsyncOperationHandle<AudioClip> asyncAudioLoad = Addressables.LoadAssetAsync<AudioClip>(assetReferenceName);
                    await asyncAudioLoad.Task;
                    AsyncHandleDictionary.Add(item.file, asyncAudioLoad);
                    audioClip = asyncAudioLoad.Result as AudioClip;
                    break;
#endif
            }
            if (audioClip == null)
                Debug.LogWarning("UIPackage.Extension: AudioClip '" + item.file + "' not found in " + name);
            else
            {
                if (item.audioClip == null)
                    item.audioClip = new NAudioClip(audioClip);
                else
                    item.audioClip.Reload(audioClip);
                item.audioClip.destroyMethod = dm;
            }
            --RelyFileCount;
        }

        static void AddReference(string key)
        {
            if (ReferenceDictionary.ContainsKey(key))
                ReferenceDictionary[key]++;
            else
                ReferenceDictionary.Add(key, 1);
        }

        static void SubReference(string key)
        {
            ReferenceDictionary[key]--;
            CheckReferenceUse(key);
        }

        static void CheckReferenceUse(string key)
        {
            if (ReferenceDictionary[key] <= 0)
            {
                UIPackage pkg = GetByName(key);
#if ADDRESSABLE
                if (AsyncHandleDictionary.TryGetValue(key, out AsyncOperationHandle handle))
                {
                    Addressables.Release(AsyncHandleDictionary[key]);
                    AsyncHandleDictionary.Remove(key);
                }
                var list = pkg._items.Where(x => x.file != null);
                foreach (var item in list)
                {
                    if (AsyncHandleDictionary.TryGetValue(item.file, out AsyncOperationHandle _handle))
                    {
                        Addressables.Release(AsyncHandleDictionary[item.file]);
                        AsyncHandleDictionary.Remove(item.file);
                    }
                }
#endif
                RemovePackage(key);
                PackageCallbackList.Remove(key);
                ReferenceDictionary.Remove(key);
            }
        }

        public static void SubFguiPackageReference(string packageName)
        {
            if (!string.IsNullOrEmpty(packageName))
            {
                if (ReferenceDictionary.ContainsKey(packageName))
                {
                    SubReference(packageName);
                }
                else
                {
                    Debug.LogError($"UIPackage.Extension: SubReference error with null key. Key: {packageName}");
                }
            }
        }

        public static void ReleaseAsyncHandle(string packageName = null)
        {
            if (!string.IsNullOrEmpty(packageName))
            {
                if (ReferenceDictionary.ContainsKey(packageName))
                {
                    ReferenceDictionary[packageName] = 0;
                    CheckReferenceUse(packageName);
                }
                else
                {
                    UIPackage pkg = GetByName(packageName);
                    if (pkg != null)
                    {
#if ADDRESSABLE
                        if (AsyncHandleDictionary.TryGetValue(packageName, out AsyncOperationHandle handle))
                        {
                            Addressables.Release(AsyncHandleDictionary[packageName]);
                            AsyncHandleDictionary.Remove(packageName);
                        }
                        var list = pkg._items.Where(x => x.file != null);
                        foreach (var item in list)
                        {
                            if (AsyncHandleDictionary.TryGetValue(item.file, out AsyncOperationHandle _handle))
                            {
                                Addressables.Release(AsyncHandleDictionary[item.file]);
                                AsyncHandleDictionary.Remove(item.file);
                            }
                        }
#endif
                        RemovePackage(packageName);
                        PackageCallbackList.Remove(packageName);
                    }
                    else {
                        Debug.LogWarning($"UIPackage.Extension: Package \"{packageName}\" dont load.");
                    }
                }
            }
            else
            {
#if ADDRESSABLE
                foreach (var item in AsyncHandleDictionary.Values)
                    Addressables.Release(item);
                AsyncHandleDictionary.Clear();
#endif
                RemoveAllPackages();
                PackageCallbackList.Clear();
                ReferenceDictionary.Clear();
            }
        }
    }
}