#pragma warning disable 0649
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Reflection;

namespace Framwork
{
    public class FramworkEntry : MonoBehaviour
    {
        public static FramworkEntry Self;

        public FguiConfiguration FguiConfiguration;
        [SerializeField] bool useFgui;
        [SerializeField] string enterUITypeFullName;
        [SerializeField] int selectSingleFguiMask;
        [SerializeField] int selectNoSingleFguiMask;
        [SerializeField] UnityEvent fguiConfigLoadCallback;
        [SerializeField] SingleFguiEvent enterUIShowCallback;

        [SerializeField] bool loadDataTable;
        [SerializeField] bool dataTableIsNull;
        [SerializeField] string[] ignoreDataTableTypeNames;
        [SerializeField] SingleDataTableEvent loadSingleDataTableCallback;
        [SerializeField] UnityEvent loadAllDataTableCallback;

        [SerializeField] bool loadJsonData;
        [SerializeField] bool jsonDataIsNull;
        [SerializeField] string[] ignoreJsonDataTypeNames;
        [SerializeField] SingleJsonDataEvent loadSingleJsonDataCallback;
        [SerializeField] UnityEvent loadAllJsonDataCallback;

        [SerializeField] bool injectLocalData;
        [SerializeField] bool localDataIsNull;
        [SerializeField] string[] ignoreLocalDataTypeNames;
        [SerializeField] SingleLocalDataEvent injectSingleLocalDataCallback;
        [SerializeField] UnityEvent injectAllLocalDataCallback;

        [SerializeField] bool loadAsset;
        [SerializeField] bool assetUtilityIsNull;
        [SerializeField] string[] ignoreAssetUtilityTypeNames;
        [SerializeField] PrefabUtilityEvent loadSingleAssetUtilityCallback;
        [SerializeField] UnityEvent loadAllAssetUtilityCallback;

        [SerializeField] UnityEvent framworkReadyCallback;

        [SerializeField] DataType[] runTimeSequence;

        private void Awake()
        {
            Self = this;
            sequenceLoad();
        }

        void sequenceLoad(int index = 0)
        {
            if (index < runTimeSequence.Length)
            {
                DataType type = runTimeSequence[index];
                switch (type)
                {
                    case DataType.Other:
                        StartCoroutine(loadOther(() => sequenceLoad(index + 1)));
                        break;
                    case DataType.Fgui:
                        fguiConfigLoad(() => sequenceLoad(index + 1));
                        break;
                    case DataType.DataTable:
                        dataTableLoad(() => sequenceLoad(index + 1));
                        break;
                    case DataType.JsonData:
                        jsonDataLoad(() => sequenceLoad(index + 1));
                        break;
                    case DataType.LocalData:
                        loaclSaveDataLoad(() => sequenceLoad(index + 1));
                        break;
                    case DataType.Asset:
                        injectPrefabUtility(() => sequenceLoad(index + 1));
                        break;
                    default:
                        sequenceLoad(index + 1);
                        break;
                }
            }
            else
                framworkReadyCallback?.Invoke();
        }

        IEnumerator loadOther(Action end)
        {
            int count = 1;
            Action callback = () =>
            {
                if (--count <= 0)
                    end?.Invoke();
            };
            if (loadAsset && !assetUtilityIsNull && Array.IndexOf(runTimeSequence, DataType.Asset) == -1)
            {
                count++;
                AssetUtility.LoadAll(prefabUtility => loadSingleAssetUtilityCallback?.Invoke(prefabUtility), () => { loadAllAssetUtilityCallback?.Invoke(); callback.Invoke(); }, ignoreAssetUtilityTypeNames);
                yield return 0;
            }
            if (loadDataTable && !dataTableIsNull && Array.IndexOf(runTimeSequence, DataType.DataTable) == -1)
            {
                count++;
                DataTableUtility.LoadAllDataTable(dataTable => loadSingleDataTableCallback?.Invoke(dataTable), () => { loadAllDataTableCallback?.Invoke(); callback.Invoke(); }, ignoreDataTableTypeNames);
                yield return 0;
            }
            if (loadJsonData && !jsonDataIsNull && Array.IndexOf(runTimeSequence, DataType.JsonData) == -1)
            {
                count++;
                JsonDataUntility.LoadAllJsonData(jsonData => loadSingleJsonDataCallback?.Invoke(jsonData), () => { loadAllJsonDataCallback?.Invoke(); callback.Invoke(); }, ignoreJsonDataTypeNames);
                yield return 0;
            }
            if (useFgui && FguiConfiguration != null && Array.IndexOf(runTimeSequence, DataType.Fgui) == -1)
            {
                count++;
                int fguiCount = 2;
                Action fguiAction = () =>
                {
                    if (--fguiCount <= 0)
                    {
                        fguiConfigLoadCallback?.Invoke();
                        callback.Invoke();
                    }
                };
                FguiUtility.LoadFguiConfig(() =>
                {
                    if (!string.IsNullOrEmpty(enterUITypeFullName))
                    {
                        Type mainUIType = Type.GetType(enterUITypeFullName);
                        MethodInfo showFguiMethodInfo = typeof(FguiUtility).GetMethod("ShowFgui", BindingFlags.Static | BindingFlags.Public);
                        Action<SingleFgui> action = fgui => { enterUIShowCallback?.Invoke(fgui); fguiAction.Invoke(); };
                        showFguiMethodInfo.MakeGenericMethod(new Type[] { mainUIType }).Invoke(null, new object[] { null, action });
                    }
                    else
                        fguiAction.Invoke();
                });
                if (selectSingleFguiMask != 0 || selectNoSingleFguiMask != 0)
                {
                    fguiCount++;
                    FguiUtility.InjectAllPackage(FguiType.All, fguiAction, selectSingleFguiMask, selectNoSingleFguiMask);
                }
                fguiAction.Invoke();
                yield return 0;
            }
            if (injectLocalData && !localDataIsNull && Array.IndexOf(runTimeSequence, DataType.LocalData) == -1)
            {
                LocalSaveUtility.InjectAll(localData => injectSingleLocalDataCallback?.Invoke(localData), ignoreLocalDataTypeNames);
                count++;
                injectAllLocalDataCallback?.Invoke();
                callback.Invoke();
            }
            callback.Invoke();
        }

        void dataTableLoad(Action callback)
        {
            if (loadDataTable && !dataTableIsNull)
                DataTableUtility.LoadAllDataTable(dataTable => loadSingleDataTableCallback?.Invoke(dataTable), () => { loadAllDataTableCallback?.Invoke(); callback?.Invoke(); }, ignoreDataTableTypeNames);
            else
                callback?.Invoke();
        }

        void jsonDataLoad(Action callback)
        {
            if (loadJsonData && !jsonDataIsNull)
                JsonDataUntility.LoadAllJsonData(jsonData => loadSingleJsonDataCallback?.Invoke(jsonData), () => { loadAllJsonDataCallback?.Invoke(); callback?.Invoke(); }, ignoreJsonDataTypeNames);
            else
                callback?.Invoke();
        }

        void loaclSaveDataLoad(Action callback)
        {
            if (injectLocalData && !localDataIsNull)
            {
                LocalSaveUtility.InjectAll(localData => injectSingleLocalDataCallback?.Invoke(localData), ignoreLocalDataTypeNames);
                injectAllLocalDataCallback?.Invoke();
                callback?.Invoke();
            }
            else
                callback?.Invoke();
        }

        void injectPrefabUtility(Action callback)
        {
            if (loadAsset && !assetUtilityIsNull)
                AssetUtility.LoadAll(prefabUtility => loadSingleAssetUtilityCallback?.Invoke(prefabUtility), () => { loadAllAssetUtilityCallback?.Invoke(); callback?.Invoke(); }, ignoreAssetUtilityTypeNames);
            else
                callback?.Invoke();
        }

        void fguiConfigLoad(Action callback)
        {
            if (useFgui && FguiConfiguration != null)
            {
                int fguiCount = 2;
                Action fguiAction = () =>
                {
                    if (--fguiCount <= 0)
                    {
                        fguiConfigLoadCallback?.Invoke();
                        callback?.Invoke();
                    }
                };
                FguiUtility.LoadFguiConfig(() =>
                {
                    if (!string.IsNullOrEmpty(enterUITypeFullName))
                    {
                        Type mainUIType = Type.GetType(enterUITypeFullName);
                        MethodInfo showFguiMethodInfo = typeof(FguiUtility).GetMethod("ShowFgui", BindingFlags.Static | BindingFlags.Public);
                        Action<SingleFgui> action = fgui => { enterUIShowCallback?.Invoke(fgui); fguiAction.Invoke(); };
                        showFguiMethodInfo.MakeGenericMethod(new Type[] { mainUIType }).Invoke(null, new object[] { null, action });
                    }
                    else
                        fguiAction.Invoke();
                });
                if (selectSingleFguiMask != 0 || selectNoSingleFguiMask != 0)
                {
                    fguiCount++;
                    FguiUtility.InjectAllPackage(FguiType.All, fguiAction, selectSingleFguiMask, selectNoSingleFguiMask);
                }
                fguiAction.Invoke();
            }
            else
                callback?.Invoke();
        }

        void OnApplicationFocus(bool focus)
        {
            if (!focus)
                LocalSaveUtility.SaveAll();
        }
    }

    [Serializable]
    public class SingleDataTableEvent : UnityEvent<DataTableUtility> { }

    [Serializable]
    public class SingleJsonDataEvent : UnityEvent<JsonDataUntility> { }

    [Serializable]
    public class SingleLocalDataEvent : UnityEvent<LocalSaveUtility> { }

    [Serializable]
    public class SingleFguiEvent : UnityEvent<SingleFgui> { }

    [Serializable]
    public class PrefabUtilityEvent : UnityEvent<AssetUtility> { }

    public enum DataType
    {
        Other,
        Fgui,
        DataTable,
        JsonData,
        LocalData,
        Asset
    }
}