using System;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using System.Linq;
using static FairyGUI.UIContentScaler;

namespace Framwork
{
    public abstract class FguiUtility : ReferenceManagment
    {
        public abstract string PackName { get; }
        public abstract string EnterUIName { get; }

        protected virtual bool IsBatching => true;

        public GComponent EnterUI { get; private set; }

        public bool IsActive { get; private set; }

        static Dictionary<string, FguiUtility> FguiDictionary = new Dictionary<string, FguiUtility>();

        protected virtual void Init(object sender = null)
        {
            EnterUI.MakeFullScreen();
            EnterUI.fairyBatching = IsBatching;
            EnterUI.GetChild("btnClose")?.asButton.onClick.Add(() => Hide());
        }

        protected virtual void Show(object sender = null)
        {
            GRoot.inst.AddChild(EnterUI);
            EnterUI.visible = true;
            IsActive = true;
        }

        protected virtual void Hide()
        {
            EnterUI.visible = false;
            IsActive = false;
        }

        protected virtual void Destroy()
        {
            GRoot.inst.RemoveChild(EnterUI);
            UnityEngine.Object.Destroy(EnterUI.displayObject.gameObject);
            FguiDictionary.Remove($"{PackName}.{EnterUIName}");
            UIPackage.SubFguiPackageReference(PackName);
        }

        void Created(GObject obj, object sender, bool justShow = true)
        {
            EnterUI = obj.asCom;
            Init(sender);
            if (justShow)
                Show(sender);
        }

        public static void InitFgui<T>(object sender = null, Action<T> callback = null) where T : SingleFgui, new()
        {
            if (FramworkEntry.Self == null && FramworkEntry.Self.FguiConfiguration == null)
                throw new NullReferenceException("FguiUtility: No fgui configuration.");
            FguiConfiguration configuration = FramworkEntry.Self.FguiConfiguration;
            T t = new T();
            if (FguiDictionary.ContainsKey($"{t.PackName}.{t.EnterUIName}"))
            {
                Debug.LogWarning($"FguiUtility: {typeof(T).Name} has Inited");
            }
            else
            {
                UIPackage.CreateFguiObjectWithType(configuration.FguiAssetType, t.PackName, t.EnterUIName, (obj) => {
                    t.Created(obj, sender, false);
                    FguiDictionary.Add($"{t.PackName}.{t.EnterUIName}", t);
                    callback?.Invoke(t);
                });
            }
        }

        public static void ShowFgui<T>(object sender = null, Action<T> callback = null) where T : SingleFgui, new()
        {
            if (FramworkEntry.Self == null && FramworkEntry.Self.FguiConfiguration == null)
                throw new NullReferenceException("FguiUtility: No fgui configuration.");
            FguiConfiguration configuration = FramworkEntry.Self.FguiConfiguration;
            T t = new T();
            if (FguiDictionary.TryGetValue($"{t.PackName}.{t.EnterUIName}", out FguiUtility _t))
            {
                _t.EnterUI.visible = true;
                _t.Show(sender);
                callback?.Invoke(_t as T);
            }
            else
            {
                UIPackage.CreateFguiObjectWithType(configuration.FguiAssetType, t.PackName, t.EnterUIName, (obj) => {
                    if (FguiDictionary.TryGetValue($"{t.PackName}.{t.EnterUIName}", out FguiUtility _obj))
                    {
                        ShowFgui(sender, callback);
                    }
                    else
                    {
                        t.Created(obj, sender);
                        FguiDictionary.Add($"{t.PackName}.{t.EnterUIName}", t);
                        callback?.Invoke(t);
                    }
                });
            }
        }

        public static void HideFgui<T>() where T : SingleFgui, new()
        {
            T t = new T();
            if (FguiDictionary.TryGetValue($"{t.PackName}.{t.EnterUIName}", out FguiUtility _t))
            {
                _t.Hide();
            }
        }

        public static void Destroy<T>() where T : SingleFgui, new()
        {
            T t = new T();
            if (FguiDictionary.TryGetValue($"{t.PackName}.{t.EnterUIName}", out FguiUtility utility))
                utility.Destroy();
        }

        public static bool TryGetFgui<T>(out T fgui) where T : SingleFgui, new()
        {
            fgui = new T();
            if (FguiDictionary.TryGetValue($"{fgui.PackName}.{fgui.EnterUIName}", out FguiUtility _fgui))
            {
                fgui = _fgui as T;
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 同步操作，必须提前加载Fgui相应资源包
        /// </summary>
        /// <returns></returns>
        public static T NewFgui<T>(bool justShow, object sender = null) where T : NoSingleFgui, new()
        {
            T t = new T();
            t.Created(UIPackage.CreateFguiObject(t.PackName, t.EnterUIName), sender, justShow);
            return t;
        }

        public static void DestroyAllSingleFgui()
        {
            foreach (FguiUtility item in FguiDictionary.Values)
            {
                item.Destroy();
                UIPackage.SubFguiPackageReference(item.PackName);
            }
            FguiDictionary.Clear();
        }

        /// <summary>
        /// 加载Fgui公共资源包和语言文件
        /// </summary>
        public static void LoadFguiConfig(Action callback = null)
        {
            if (FramworkEntry.Self == null && FramworkEntry.Self.FguiConfiguration == null)
                throw new NullReferenceException("FguiUtility: No fgui configuration.");
            FguiConfiguration configuration = FramworkEntry.Self.FguiConfiguration;
            int waitCount = 1;
            Action action = () =>
            {
                if (--waitCount <= 0)
                    callback?.Invoke();
            };
            if (!string.IsNullOrEmpty(configuration.FguiFontAssetName))
                UIConfig.defaultFont = configuration.FguiFontAssetName;
            GRoot.inst.SetContentScaleFactor(configuration.FguiDesignScreenSize.x, configuration.FguiDesignScreenSize.y, ScreenMatchMode.MatchWidthOrHeight);
            if (!string.IsNullOrEmpty(configuration.CommonPackName))
            {
                waitCount++;
                UIPackage.AddPackageWithType(configuration.FguiAssetType, configuration.CommonPackName, (pkg) => { action(); });
            }
            if (!string.IsNullOrEmpty(configuration.LanguageAssetName))
            {
                waitCount++;
                switch (configuration.FguiAssetType)
                {
                    case AssetType.Resources:
                        ResourcesLoad<TextAsset>(configuration.LanguageAssetName, obj =>
                        {
                            AddReference(configuration.LanguageAssetName, AssetType.Resources);
                            UIPackage.SetStringsSource(new FairyGUI.Utils.XML(obj.text));
                            SubReference(configuration.LanguageAssetName, AssetType.Resources);
                            action();
                        });
                        break;
#if ADDRESSABLES
                    case AssetType.Addressables:
                        AddressablesLoad<TextAsset>(configuration.LanguageAssetName, obj =>
                        {
                            AddReference(configuration.LanguageAssetName, AssetType.Addressables);
                            UIPackage.SetStringsSource(new FairyGUI.Utils.XML(obj.text));
                            SubReference(configuration.LanguageAssetName, AssetType.Addressables);
                            action();
                        });
                        break;
#endif
                }
            }
            action.Invoke();
        }

        public static void InjectPackage<T>(Action<UIPackage> callback = null) where T : FguiUtility, new()
        {
            if (FramworkEntry.Self == null && FramworkEntry.Self.FguiConfiguration == null)
                throw new NullReferenceException("FguiUtility: No fgui configuration.");
            FguiConfiguration configuration = FramworkEntry.Self.FguiConfiguration;
            T t = new T();
            UIPackage.AddPackageWithType(configuration.FguiAssetType, t.PackName, callback);
        }

        /// <summary>
        /// 加载所有继承自FguiUtility并需要使用的Fgui包文件
        /// </summary>
        public static void InjectAllPackage(FguiType fguiType, Action callback = null, int singleMask = -1, int noSingleMask = -1)
        {
            if (FramworkEntry.Self == null && FramworkEntry.Self.FguiConfiguration == null)
                throw new NullReferenceException("FguiUtility: No fgui configuration.");
            FguiConfiguration configuration = FramworkEntry.Self.FguiConfiguration;
            Type[] singleTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes().Where(y => typeof(SingleFgui).IsAssignableFrom(y) && y.IsClass && !y.IsAbstract)).ToArray();
            Type[] noSingleTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes().Where(y => typeof(NoSingleFgui).IsAssignableFrom(y) && y.IsClass && !y.IsAbstract)).ToArray();
            int packageCount = 1;
            List<string> packageNames = new List<string>();
            Action<UIPackage> action = (pkg) =>
            {
                if (--packageCount <= 0)
                    callback?.Invoke();
            };
            if (fguiType == FguiType.All || fguiType == FguiType.Single)
            {
                for (int i = 0; i < singleTypes.Length; i++)
                {
                    int j = 1 << i;
                    if ((singleMask & j) != j)
                        continue;
                    Type item = singleTypes[i];
                    string packageName = (Activator.CreateInstance(item) as FguiUtility).PackName;
                    if (UIPackage.CheckPackageIsLoaded(packageName))
                    {
                        if (UIPackage.CheckPackageIsLoading(packageName))
                        {
                            packageCount++;
                            UIPackage.AddPackageLoadedCallback(packageName, action);
                        }
                    }
                    else
                    {
                        if (!packageNames.Contains(packageName))
                        {
                            packageNames.Add(packageName);
                            packageCount++;
                        }
                    }
                }
                for (int i = 0; i < packageNames.Count; i++)
                    UIPackage.AddPackageWithType(configuration.FguiAssetType, packageNames[i], action);
            }
            if (fguiType == FguiType.All || fguiType == FguiType.NoSingle)
            {
                for (int i = 0; i < noSingleTypes.Length; i++)
                {
                    int j = 1 << i;
                    if ((noSingleMask & j) != j)
                        continue;
                    Type item = noSingleTypes[i];
                    string packageName = (Activator.CreateInstance(item) as FguiUtility).PackName;
                    if (UIPackage.CheckPackageIsLoaded(packageName))
                    {
                        if (UIPackage.CheckPackageIsLoading(packageName))
                        {
                            packageCount++;
                            UIPackage.AddPackageLoadedCallback(packageName, action);
                        }
                    }
                    else
                    {
                        if (!packageNames.Contains(packageName))
                        {
                            packageNames.Add(packageName);
                            packageCount++;
                        }
                    }
                }
                for (int i = 0; i < packageNames.Count; i++)
                    UIPackage.AddPackageWithType(configuration.FguiAssetType, packageNames[i], action);
            }
            action.Invoke(null);
        }
    }

    public enum FguiType { All, Single, NoSingle }
}