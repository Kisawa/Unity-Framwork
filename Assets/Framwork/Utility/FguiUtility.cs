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

        protected virtual bool MakeFullScreen => true;

        public GComponent EnterUI { get; private set; }

        public bool IsActive { get; private set; }

        static Dictionary<string, FguiUtility> FguiDictionary = new Dictionary<string, FguiUtility>();

        protected virtual void Init(object sender = null)
        {
            if (MakeFullScreen)
                EnterUI.MakeFullScreen();
            EnterUI.fairyBatching = IsBatching;
            EnterUI.GetChild("btnClose")?.asButton.onClick.Add(() => Hide());
        }

        public virtual void Show(object sender = null)
        {
            GRoot.inst.AddChild(EnterUI);
            EnterUI.visible = true;
            IsActive = true;
        }

        public virtual void Hide(object sender = null)
        {
            EnterUI.visible = false;
            IsActive = false;
        }

        public virtual void Destroy()
        {
            GRoot.inst.RemoveChild(EnterUI);
            UnityEngine.Object.Destroy(EnterUI.displayObject.gameObject);
            FguiDictionary.Remove($"{PackName}.{EnterUIName}");
        }

        void Created(GObject obj, object sender, bool justShow = true)
        {
            EnterUI = obj.asCom;
            Init(sender);
            if (justShow)
                Show(sender);
        }

        public static void InitFgui<T>(object sender = null) where T : SingleFgui, new()
        {
            if (FramworkEntry.Self == null && FramworkEntry.Self.FguiConfiguration == null)
                throw new NullReferenceException("FguiUtility: No fgui configuration.");

            T t = new T();
            if (FguiDictionary.ContainsKey($"{t.PackName}.{t.EnterUIName}"))
            {
                Debug.LogWarning($"FguiUtility: {typeof(T).Name} has Inited");
            }
            else
            {
                AddPackage(t.PackName);
                GObject obj = UIPackage.CreateObject(t.PackName, t.EnterUIName);
                t.Created(obj, sender, false);
                FguiDictionary.Add($"{t.PackName}.{t.EnterUIName}", t);
            }
        }

        public static T ShowFgui<T>(object sender = null) where T : SingleFgui, new()
        {
            if (FramworkEntry.Self == null && FramworkEntry.Self.FguiConfiguration == null)
                throw new NullReferenceException("FguiUtility: No fgui configuration.");
            T t = new T();
            if (FguiDictionary.TryGetValue($"{t.PackName}.{t.EnterUIName}", out FguiUtility _t))
            {
                _t.EnterUI.visible = true;
                _t.Show(sender);
                return _t as T;
            }
            else
            {
                AddPackage(t.PackName);
                GObject obj = UIPackage.CreateObject(t.PackName, t.EnterUIName);
                t.Created(obj, sender);
                FguiDictionary.Add($"{t.PackName}.{t.EnterUIName}", t);
                return t;
            }
        }

        public static void HideFgui<T>(object sender = null) where T : SingleFgui, new()
        {
            T t = new T();
            if (FguiDictionary.TryGetValue($"{t.PackName}.{t.EnterUIName}", out FguiUtility _t))
            {
                _t.Hide(sender);
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
            t.Created(UIPackage.CreateObject(t.PackName, t.EnterUIName), sender, justShow);
            return t;
        }

        public static void DestroyAllSingleFgui()
        {
            foreach (FguiUtility item in FguiDictionary.Values)
                item.Destroy();
            FguiDictionary.Clear();
        }

        /// <summary>
        /// 加载Fgui公共资源包和语言文件
        /// </summary>
        public static void LoadFguiConfig()
        {
            if (FramworkEntry.Self == null && FramworkEntry.Self.FguiConfiguration == null)
                throw new NullReferenceException("FguiUtility: No fgui configuration.");
            FguiConfiguration configuration = FramworkEntry.Self.FguiConfiguration;

            if (!string.IsNullOrEmpty(configuration.FguiFontAssetName))
                UIConfig.defaultFont = configuration.FguiFontAssetName;
            GRoot.inst.SetContentScaleFactor(configuration.FguiDesignScreenSize.x, configuration.FguiDesignScreenSize.y, ScreenMatchMode.MatchWidthOrHeight);

            if (!string.IsNullOrEmpty(configuration.CommonPackName))
                AddPackage(configuration.CommonPackName);

            if (!string.IsNullOrEmpty(configuration.LanguageAssetName))
            {
                ResourcesLoad<TextAsset>(configuration.LanguageAssetName, obj =>
                {
                    AddReference(configuration.LanguageAssetName, AssetType.Resources);
                    UIPackage.SetStringsSource(new FairyGUI.Utils.XML(obj.text));
                    SubReference(configuration.LanguageAssetName, AssetType.Resources);
                });
            }
        }

        /// <summary>
        /// 加载所有继承自FguiUtility并需要使用的Fgui包文件
        /// </summary>
        public static void InjectAllPackage(FguiType fguiType, int singleMask = -1, int noSingleMask = -1)
        {
            if (FramworkEntry.Self == null && FramworkEntry.Self.FguiConfiguration == null)
                throw new NullReferenceException("FguiUtility: No fgui configuration.");
            FguiConfiguration configuration = FramworkEntry.Self.FguiConfiguration;
            Type[] singleTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes().Where(y => typeof(SingleFgui).IsAssignableFrom(y) && y.IsClass && !y.IsAbstract)).ToArray();
            Type[] noSingleTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes().Where(y => typeof(NoSingleFgui).IsAssignableFrom(y) && y.IsClass && !y.IsAbstract)).ToArray();

            if (fguiType == FguiType.All || fguiType == FguiType.Single)
            {
                for (int i = 0; i < singleTypes.Length; i++)
                {
                    int j = 1 << i;
                    if ((singleMask & j) != j)
                        continue;
                    Type item = singleTypes[i];
                    string packageName = (Activator.CreateInstance(item) as FguiUtility).PackName;
                    AddPackage(packageName);
                }
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
                    AddPackage(packageName);
                }
            }
        }

        public static void AddPackage(string packageName)
        {
            if (FramworkEntry.Self == null && FramworkEntry.Self.FguiConfiguration == null)
                throw new NullReferenceException("FguiUtility: No fgui configuration.");
            FguiConfiguration configuration = FramworkEntry.Self.FguiConfiguration;
            UIPackage.AddPackage($"{configuration.AssetsResourcesPath}/{packageName}");
        }
    }

    public enum FguiType { All, Single, NoSingle }
}