using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Reflection;

public class FramworkEntry : MonoBehaviour
{
    public static FramworkEntry Self;

    public FguiConfiguration FguiConfiguration;
    [SerializeField] bool useFgui;
    [SerializeField] string mainUITypeFullName;
    [SerializeField] UnityEvent fguiConfigLoadCallback;
    [SerializeField] SingleFguiEvent mainUIShowCallback;

    [SerializeField] bool loadDataTable;
    [SerializeField] bool dataTableIsNull;
    [SerializeField] string[] ignoreDataTableTypeName;
    [SerializeField] SingleDataTableEvent loadSingleDataTableCallback;
    [SerializeField] UnityEvent loadAllDataTableCallback;

    [SerializeField] bool loadJsonData;
    [SerializeField] bool jsonDataIsNull;
    [SerializeField] string[] ignoreJsonDataTypeName;
    [SerializeField] SingleJsonDataEvent loadSingleJsonDataCallback;
    [SerializeField] UnityEvent loadAllJsonDataCallback;

    [SerializeField] bool injectLocalData;
    [SerializeField] bool localDataIsNull;
    [SerializeField] string[] ignoreLocalDataTypeName;
    [SerializeField] SingleLocalDataEvent injectSingleLocalDataCallback;
    [SerializeField] UnityEvent injectAllLocalDataCallback;

    [SerializeField] UnityEvent framworkReadyCallback;

    [SerializeField] DataType[] runTimeSequence;

    int ready = 0;

    private void Awake()
    {
        Self = this;
        StartCoroutine(load());
        sequenceLoad();
    }

    IEnumerator load()
    {
        int count = 1;
        Action callback = () =>
        {
            if (--count <= 0)
            {
                ready++;
                if(ready == 2)
                    framworkReadyCallback?.Invoke();
            }
        };
        if (loadDataTable && !dataTableIsNull && Array.IndexOf(runTimeSequence, DataType.DataTable) == -1)
        {
            count++;
            DataTableUtility.LoadAllDataTable(dataTable => loadSingleDataTableCallback?.Invoke(dataTable), () => { loadAllDataTableCallback?.Invoke(); callback.Invoke(); }, ignoreDataTableTypeName);
            yield return 0;
        }
        if (loadJsonData && !jsonDataIsNull && Array.IndexOf(runTimeSequence, DataType.JsonData) == -1)
        {
            count++;
            JsonDataUntility.LoadAllJsonData(jsonData => loadSingleJsonDataCallback?.Invoke(jsonData), () => { loadAllJsonDataCallback?.Invoke(); callback.Invoke(); }, ignoreJsonDataTypeName);
            yield return 0;
        }
        if (useFgui && FguiConfiguration != null && Array.IndexOf(runTimeSequence, DataType.Fgui) == -1)
        {
            count++;
            FguiUtility.LoadFguiConfig(() => 
            { 
                fguiConfigLoadCallback?.Invoke();
                if (!string.IsNullOrEmpty(mainUITypeFullName))
                {
                    Type mainUIType = Type.GetType(mainUITypeFullName);
                    MethodInfo showFguiMethodInfo = typeof(FguiUtility).GetMethod("ShowFgui", BindingFlags.Static | BindingFlags.Public);
                    Action<SingleFgui> action = fgui => { mainUIShowCallback?.Invoke(fgui); callback?.Invoke(); } ;
                    showFguiMethodInfo.MakeGenericMethod(new Type[] { mainUIType }).Invoke(null, new object[] { null, action });
                }
                else
                    callback?.Invoke();
            });
            yield return 0;
        }
        if (injectLocalData && !localDataIsNull && Array.IndexOf(runTimeSequence, DataType.LocalData) == -1)
        {
            LocalSaveUtility.InjectAll(localData => injectSingleLocalDataCallback?.Invoke(localData), ignoreLocalDataTypeName);
            count++;
            injectAllLocalDataCallback?.Invoke();
            callback.Invoke();
        }
        callback.Invoke();
    }

    void sequenceLoad(int index = 0)
    {
        if (index < runTimeSequence.Length)
        {
            DataType type = runTimeSequence[index];
            switch (type)
            {
                case DataType.DataTable:
                    dataTableLoad(() => sequenceLoad(index + 1));
                    break;
                case DataType.JsonData:
                    jsonDataLoad(() => sequenceLoad(index + 1));
                    break;
                case DataType.LocalData:
                    loaclSaveDataLoad(() => sequenceLoad(index + 1));
                    break;
                case DataType.Fgui:
                    fguiConfigLoad(() => sequenceLoad(index + 1));
                    break;
                default:
                    sequenceLoad(index + 1);
                    break;
            }
        }
        else
        {
            ready++;
            if(ready == 2)
                framworkReadyCallback?.Invoke();
        }
    }

    void dataTableLoad(Action callback)
    {
        if (loadDataTable && !dataTableIsNull)
            DataTableUtility.LoadAllDataTable(dataTable => loadSingleDataTableCallback?.Invoke(dataTable), () => { loadAllDataTableCallback?.Invoke(); callback?.Invoke(); }, ignoreDataTableTypeName);
        else
            callback?.Invoke();
    }

    void jsonDataLoad(Action callback)
    {
        if (loadJsonData && !jsonDataIsNull)
            JsonDataUntility.LoadAllJsonData(jsonData => loadSingleJsonDataCallback?.Invoke(jsonData), () => { loadAllJsonDataCallback?.Invoke(); callback.Invoke(); }, ignoreJsonDataTypeName);
        else
            callback?.Invoke();
    }

    void loaclSaveDataLoad(Action callback)
    {
        if (injectLocalData && !localDataIsNull)
        {
            LocalSaveUtility.InjectAll(localData => injectSingleLocalDataCallback?.Invoke(localData), ignoreLocalDataTypeName);
            injectAllLocalDataCallback?.Invoke();
            callback.Invoke();
        }
        else
            callback?.Invoke();
    }

    void fguiConfigLoad(Action callback)
    {
        if (useFgui && FguiConfiguration != null)
        {
            FguiUtility.LoadFguiConfig(() =>
            {
                fguiConfigLoadCallback?.Invoke();
                if (!string.IsNullOrEmpty(mainUITypeFullName))
                {
                    Type mainUIType = Type.GetType(mainUITypeFullName);
                    MethodInfo showFguiMethodInfo = typeof(FguiUtility).GetMethod("ShowFgui", BindingFlags.Static | BindingFlags.Public);
                    Action<SingleFgui> action = fgui => { mainUIShowCallback?.Invoke(fgui); callback?.Invoke(); };
                    showFguiMethodInfo.MakeGenericMethod(new Type[] { mainUIType }).Invoke(null, new object[] { null, action });
                }
                else
                    callback?.Invoke();
            });
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

public enum DataType
{ 
    DataTable,
    JsonData,
    LocalData,
    Fgui
}