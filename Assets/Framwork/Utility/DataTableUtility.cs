using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;

namespace Framwork
{
    public abstract class DataTableUtility : ReferenceManagment
    {
        static Dictionary<string, DataTableUtility> DataTableDictionary = new Dictionary<string, DataTableUtility>();

        public abstract string TableAssetName { get; }

#if ADDRESSABLES
        public virtual AssetType AssetType { get => AssetType.Addressables; }
#else
        public virtual AssetType AssetType { get => AssetType.Resources; }
#endif

        public DataTableUtility() { }

        protected int currentRowIndex;

        protected virtual void StartInject() { }

        protected abstract void InjectLine(params string[] currentLineTextList);

        protected virtual void EndInject() { }

        public void Load(Action<DataTableUtility> callback = null)
        {
            switch (AssetType)
            {
                case AssetType.Resources:
                    {
                        ResourcesLoad<TextAsset>(TableAssetName, obj =>
                        {
                            if (obj == null)
                                throw new NullReferenceException($"DataTableUtility - Asset {TableAssetName} in {GetType().Name} is null");
                            AddReference(TableAssetName, AssetType.Resources);
                            StartInject();
                            InjectData(obj.text);
                            EndInject();
                            SubReference(TableAssetName, AssetType.Resources);
                            callback?.Invoke(this);
                        });
                        break;
                    }
#if ADDRESSABLES
                case AssetType.Addressables:
                    {
                        AddressablesLoad<TextAsset>(TableAssetName, obj =>
                        {
                            if (obj == null)
                                throw new NullReferenceException($"DataTableUtility - Asset {TableAssetName} in {GetType().Name} is null");
                            AddReference(TableAssetName, AssetType.Addressables);
                            StartInject();
                            InjectData(obj.text);
                            EndInject();
                            SubReference(TableAssetName, AssetType.Addressables);
                            callback?.Invoke(this);
                        });
                        break;
                    }
#endif
            }
        }

        public static T GetDataTable<T>() where T : DataTableUtility
        {
            DataTableDictionary.TryGetValue(typeof(T).Name, out DataTableUtility utility);
            return utility as T;
        }

        public static void LoadDataTable<T>(Action<T> callback = null) where T : DataTableUtility, new()
        {
            string key = typeof(T).Name;
            if (DataTableDictionary.TryGetValue(key, out DataTableUtility utility))
            {
                callback?.Invoke(utility as T);
                Debug.LogWarning($"DataTableUtility: {key} has loaded.");
            }
            else
            {
                T t = new T();
                t.Load(callback as Action<DataTableUtility>);
                DataTableDictionary.Add(key, t);
            }
        }

        public static void LoadAllDataTable(Action<DataTableUtility> dataTableCallback = null, Action endLoadCallback = null, string[] ignoreTypeNames = null)
        {
            Type[] types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes().Where(y => typeof(DataTableUtility).IsAssignableFrom(y) && y.IsClass && !y.IsAbstract)).ToArray();
            MethodInfo method = typeof(DataTableUtility).GetMethod("LoadDataTable", BindingFlags.Public | BindingFlags.Static);
            int typeLength = types.Length + 1;
            Action<DataTableUtility> callback = (obj) =>
            {
                if (obj != null)
                    dataTableCallback?.Invoke(obj);
                if (--typeLength <= 0)
                    endLoadCallback?.Invoke();
            };
            for (int i = 0; i < types.Length; i++)
            {
                if (ignoreTypeNames == null || !ignoreTypeNames.Contains(types[i].Name))
                    method.MakeGenericMethod(types[i]).Invoke(null, new object[] { callback });
                else
                    typeLength--;
            }
            callback.Invoke(null);
        }

        void InjectData(string text)
        {
            string[] lineText = text.Trim().Split('\n');
            currentRowIndex = -1;
            for (int i = 0; i < lineText.Length; i++)
            {
                string item = lineText[i];
                if (!item.StartsWith("#"))
                {
                    currentRowIndex++;
                    InjectLine(item.Trim().Split('\t'));
                }
            }
        }
    }
}