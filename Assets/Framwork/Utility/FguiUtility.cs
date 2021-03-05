using System;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using System.Linq;
using System.Reflection;
#if ADDRESSABLE
using UnityEngine.AddressableAssets;
#endif
using static FairyGUI.UIContentScaler;

public abstract class FguiUtility
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
    }

    void Created(GObject obj, object sender, bool justShow = true)
    {
        EnterUI = obj.asCom;
        Init(sender);
        if(justShow)
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
                else {
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

    protected static void InjectSelf<T>(T data, bool justShow, object sender = null) where T : NoSingleFgui
    {
        data.Created(UIPackage.CreateFguiObject(data.PackName, data.EnterUIName), sender, justShow);
    }

    public static void Destroy(FguiUtility fgui) 
    {
        fgui.Destroy();
        string key = $"{fgui.PackName}.{fgui.EnterUIName}";
        if (FguiDictionary.ContainsKey(key))
            FguiDictionary.Remove(key);
        UIPackage.SubFguiPackageReference(fgui.PackName);
    }

    public static void DestroyAllSingleFgui() {
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
                    TextAsset languageAsset = Resources.Load<TextAsset>(configuration.LanguageAssetName);
                    UIPackage.SetStringsSource(new FairyGUI.Utils.XML(languageAsset.text));
                    action();
                    break;
#if ADDRESSABLE
                case AssetType.Addressable:
                    Addressables.LoadAssetAsync<TextAsset>(configuration.LanguageAssetName).Completed += (obj) =>
                    {
                        UIPackage.SetStringsSource(new FairyGUI.Utils.XML(obj.Result.text));
                        action();
                    };
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
    public static void InjectAllPackage(FguiType fguiType, Action<UIPackage> packageCallback = null, Action endInjectCallback = null) 
    {
        if (FramworkEntry.Self == null && FramworkEntry.Self.FguiConfiguration == null)
            throw new NullReferenceException("FguiUtility: No fgui configuration.");
        FguiConfiguration configuration = FramworkEntry.Self.FguiConfiguration;
        Type[] types = null;
        switch (fguiType)
        {
            case FguiType.All:
                types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes().Where(y => typeof(FguiUtility).IsAssignableFrom(y) && y.IsClass && !y.IsAbstract)).ToArray();
                break;
            case FguiType.Single:
                types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes().Where(y => typeof(SingleFgui).IsAssignableFrom(y) && y.IsClass && !y.IsAbstract)).ToArray();
                break;
            case FguiType.NoSingle:
                types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes().Where(y => typeof(NoSingleFgui).IsAssignableFrom(y) && y.IsClass && !y.IsAbstract)).ToArray();
                break;
            default:
                break;
        }
        if (types != null)
        {
            int packageCount = 1;
            List<string> packageNames = new List<string>();
            Action<UIPackage> action = (pkg) =>
            {
                if(pkg != null)
                    packageCallback?.Invoke(pkg);
                if (--packageCount <= 0)
                    endInjectCallback?.Invoke();
            };
            MethodInfo methodInfo = typeof(UIPackage).GetMethod("AddPackageWithType");
            foreach (Type item in types)
            {
                string packageName = (Activator.CreateInstance(item) as FguiUtility).PackName;
                if (UIPackage.CheckPackageIsLoaded(packageName))
                {
                    if (UIPackage.CheckPackageIsLoading(packageName))
                    {
                        packageCount++;
                        UIPackage.AddPackageLoadedCallback(packageName, action);
                    }
                    else
                    {
                        packageCallback?.Invoke(UIPackage.GetByName(packageName));
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
            if (packageNames.Count == 0)
            {
                if(packageCount <= 0)
                    endInjectCallback?.Invoke();
            }
            else
            {
                foreach (string item in packageNames)
                {
                    methodInfo.Invoke(null, new object[] { configuration.FguiAssetType, item, action });
                }
            }
            action.Invoke(null);
        }
    }
}

public enum FguiType { All, Single, NoSingle }