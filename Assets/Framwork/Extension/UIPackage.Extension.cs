using FairyGUI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Framwork;
#if ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace FairyGUI {
    public partial class UIPackage
    {
#if ADDRESSABLES
        static Dictionary<string, AsyncOperationHandle> AddressableHandles = new Dictionary<string, AsyncOperationHandle>();
#endif
        static Dictionary<string, ResourceRequest> ResourceHandles = new Dictionary<string, ResourceRequest>();
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

        public static void AddPackageWithType(AssetType assetType, string assetName, Action<UIPackage> callback = null)
        {
            if (PackageCallbackList.ContainsKey(assetName)) {
                Debug.LogWarning($"UIPackage.Extension: Dont call repeatedly while {assetName} has inited");
                return;
            }
            switch (assetType)
            {
                case AssetType.Resources:
                    PackageCallbackList.Add(assetName, callback);
                    ResourceRequest request = Resources.LoadAsync<TextAsset>($"FguiAssets/{assetName}_fui");
                    request.completed += obj =>
                    {
                        ResourceHandles.Add(assetName, request);
                        TextAsset asset = request.asset as TextAsset;
                        addPackageWithType(assetType, asset, assetName);
                    };
                    break;
#if ADDRESSABLES
                case AssetType.Addressables:
                    PackageCallbackList.Add(assetName, callback);
                    AsyncOperationHandle<TextAsset> asyncPackageLoad = Addressables.LoadAssetAsync<TextAsset>($"{assetName}_fui");
                    asyncPackageLoad.Completed += obj =>
                    {
                        AddressableHandles.Add(assetName, asyncPackageLoad);
                        TextAsset asset = asyncPackageLoad.Result;
                        addPackageWithType(assetType, asset, assetName);
                    };
                    break;
#endif
            }
        }

        static void addPackageWithType(AssetType assetType, TextAsset asset, string assetName)
        {
            if (asset == null)
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

        void LoadAtlasWithType(AssetType assetType, PackageItem item)
        {
            string assetReferenceName = item.file.Substring(0, item.file.LastIndexOf('.'));
            switch (assetType)
            {
                case AssetType.Resources:
                    ResourceRequest request = Resources.LoadAsync<Texture>($"FguiAssets/{assetReferenceName}");
                    request.completed += obj =>
                    {
                        ResourceHandles.Add(item.file, request);
                        Texture tex = request.asset as Texture;
                        loadAtlasWithType(tex, item);
                    };
                    break;
#if ADDRESSABLES
                case AssetType.Addressables:
                    AsyncOperationHandle<Texture> asyncTextureLoad = Addressables.LoadAssetAsync<Texture>(assetReferenceName);
                    asyncTextureLoad.Completed += obj =>
                    {
                        AddressableHandles.Add(item.file, asyncTextureLoad);
                        Texture tex = asyncTextureLoad.Result;
                        loadAtlasWithType(tex, item);
                    };
                    break;
#endif
            }
        }

        void loadAtlasWithType(Texture tex, PackageItem item)
        {
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

        void LoadSoundWithType(AssetType assetType, PackageItem item)
        {
            string assetReferenceName = item.file.Substring(0, item.file.LastIndexOf('.'));
            switch (assetType)
            {
                case AssetType.Resources:
                    ResourceRequest request = Resources.LoadAsync<AudioClip>($"FguiAssets/{assetReferenceName}");
                    request.completed += obj =>
                    {
                        ResourceHandles.Add(item.file, request);
                        AudioClip audioClip = request.asset as AudioClip;
                        loadSoundWithType(audioClip, item);
                    };
                    break;
#if ADDRESSABLES
                case AssetType.Addressables:
                    AsyncOperationHandle<AudioClip> asyncAudioLoad = Addressables.LoadAssetAsync<AudioClip>(assetReferenceName);
                    asyncAudioLoad.Completed += obj =>
                    {
                        AddressableHandles.Add(item.file, asyncAudioLoad);
                        AudioClip audioClip = asyncAudioLoad.Result;
                        loadSoundWithType(audioClip, item);
                    };
                    break;
#endif
            }
        }

        void loadSoundWithType(AudioClip audioClip, PackageItem item)
        {
            if (audioClip == null)
                Debug.LogWarning("UIPackage.Extension: AudioClip '" + item.file + "' not found in " + name);
            else
            {
                if (item.audioClip == null)
                    item.audioClip = new NAudioClip(audioClip);
                else
                    item.audioClip.Reload(audioClip);
                item.audioClip.destroyMethod = DestroyMethod.Unload;
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
                var list = pkg._items.Where(x => x.file != null);
                if (ResourceHandles.TryGetValue(key, out ResourceRequest request))
                {
                    Resources.UnloadAsset(request.asset);
                    ResourceHandles.Remove(key);
                }
#if ADDRESSABLES
                if (AddressableHandles.TryGetValue(key, out AsyncOperationHandle handle))
                {
                    Addressables.Release(handle);
                    AddressableHandles.Remove(key);
                }
#endif
                foreach (var item in list)
                {
                    if (ResourceHandles.TryGetValue(item.file, out ResourceRequest _request))
                    {
                        Resources.UnloadAsset(_request.asset);
                        ResourceHandles.Remove(item.file);
                    }
#if ADDRESSABLES
                    if (AddressableHandles.TryGetValue(item.file, out AsyncOperationHandle _handle))
                    {
                        Addressables.Release(_handle);
                        AddressableHandles.Remove(item.file);
                    }
#endif
                }
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
                    var list = pkg._items.Where(x => x.file != null);
                    if (pkg != null)
                    {
                        if (ResourceHandles.TryGetValue(packageName, out ResourceRequest request))
                        {
                            Resources.UnloadAsset(request.asset);
                            ResourceHandles.Remove(packageName);
                        }
#if ADDRESSABLES
                        if (AddressableHandles.TryGetValue(packageName, out AsyncOperationHandle handle))
                        {
                            Addressables.Release(handle);
                            AddressableHandles.Remove(packageName);
                        }
#endif
                        foreach (var item in list)
                        {
                            if (ResourceHandles.TryGetValue(item.file, out ResourceRequest _request))
                            {
                                Resources.UnloadAsset(_request.asset);
                                ResourceHandles.Remove(item.file);
                            }
#if ADDRESSABLES
                            if (AddressableHandles.TryGetValue(item.file, out AsyncOperationHandle _handle))
                            {
                                Addressables.Release(_handle);
                                AddressableHandles.Remove(item.file);
                            }
#endif
                        }
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
                foreach (var item in ResourceHandles.Values)
                    Resources.UnloadAsset(item.asset);
                ResourceHandles.Clear();
#if ADDRESSABLES
                foreach (var item in AddressableHandles.Values)
                    Addressables.Release(item);
                AddressableHandles.Clear();
#endif
                RemoveAllPackages();
                PackageCallbackList.Clear();
                ReferenceDictionary.Clear();
            }
        }
    }
}