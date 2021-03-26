using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framwork
{
    public class ObjectPool : ReferenceManagment
    {
        public bool Ready => prefab != null;
        public bool Null { get; private set; }
        public Transform Parent { get; private set; }

        GameObject prefab;
        string path;
        AssetType assetType;
        List<GameObject> objects = new List<GameObject>();

        public ObjectPool(GameObject prefab, int initCount = 0, Vector3 groupScale = default)
        {
            this.prefab = prefab;
            path = CheckPath(prefab, out assetType);
            AddReference(path, assetType);
            GameObject parentObj = new GameObject(path + "_Group");
            parentObj.transform.localScale = groupScale == default ? Vector3.one : groupScale;
            Parent = parentObj.transform;
            PoolDic.Add(this, (path, assetType));
            init(initCount);
        }

#if ADDRESSABLES
        public ObjectPool(string path, int initCount = 0, Vector3 groupScale = default, AssetType assetType = AssetType.Addressables)
#else
        public ObjectPool(string path, int initCount = 0, Vector3 groupScale = default, AssetType assetType = AssetType.Resources)
#endif
        {
            this.path = path;
            this.assetType = assetType;
            GameObject parentObj = new GameObject(path + "_Group");
            parentObj.transform.localScale = groupScale == default ? Vector3.one : groupScale;
            Parent = parentObj.transform;
            switch (assetType)
            {
                case AssetType.Resources:
                    ResourcesLoad<GameObject>(path, obj =>
                    {
                        AddReference(path, assetType);
                        prefab = obj;
                        PoolDic.Add(this, (path, assetType));
                        init(initCount);
                    });
                    break;
#if ADDRESSABLES
                case AssetType.Addressables:
                    AddressablesLoad<GameObject>(path, obj =>
                    {
                        AddReference(path, assetType);
                        prefab = obj;
                        PoolDic.Add(this, (path, assetType));
                        init(initCount);
                    });
                    break;
#endif
            }
        }

        void init(int initCount)
        {
            for (int i = 0; i < initCount; i++)
            {
                GameObject instance = Object.Instantiate(prefab, Parent);
                instance.SetActive(false);
                objects.Add(instance);
            }
        }

        public GameObject GetOne(Func<GameObject, bool> condition = null)
        {
            GameObject obj = GetWaittingObject(condition);
            if (obj == null)
            {
                obj = Object.Instantiate(prefab, Parent);
                obj.SetActive(true);
                objects.Add(obj);
            }
            else
            {
                obj.transform.SetParent(Parent);
                obj.SetActive(true);
            }
            return obj;
        }

        public GameObject GetOne(Vector3 position, Quaternion rotation, Func<GameObject, bool> condition = null)
        {
            GameObject obj = GetWaittingObject(condition);
            if (obj == null)
            {
                obj = Object.Instantiate(prefab, position, rotation, Parent);
                obj.SetActive(true);
                objects.Add(obj);
            }
            else
            {
                obj.transform.SetParent(Parent);
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
            }
            return obj;
        }

        public void DisableAll()
        {
            for (int i = 0; i < objects.Count; i++)
            {
                GameObject obj = objects[i];
                if (obj == null)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                obj.SetActive(false);
            }
        }

        public void DestroyAll()
        {
            for (int i = 0; i < objects.Count; i++)
            {
                GameObject obj = objects[i];
                if (obj != null)
                    Object.Destroy(obj);
            }
            objects.Clear();
        }

        public void Clear()
        {
            PoolDic.Remove(this);
            DestroyAll();
            SubReference(path, assetType);
            prefab = null;
            path = "";
            if (Parent != null)
                Object.Destroy(Parent.gameObject);
            Parent = null;
            Null = true;
        }

        public bool Check(PoolCheck poolCheck)
        {
            Func<GameObject, bool> func = GetCheckFunc(poolCheck);
            for (int i = 0; i < objects.Count; i++)
            {
                GameObject obj = objects[i];
                if (obj == null)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (func(obj))
                    return true;
            }
            return false;
        }

        public bool Check(Func<GameObject, bool> condition)
        {
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                GameObject obj = objects[i];
                if (obj == null)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (condition(obj))
                    return true;
            }
            return false;
        }

        public bool Check(Func<GameObject, bool> condition, PoolCheck poolCheck)
        {
            Func<GameObject, bool> func = GetCheckFunc(poolCheck);
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                GameObject obj = objects[i];
                if (obj == null)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (func(obj) && condition(obj))
                    return true;
            }
            return false;
        }

        public bool CheckOne(PoolCheck poolCheck, out GameObject instance)
        {
            Func<GameObject, bool> func = GetCheckFunc(poolCheck);
            for (int i = 0; i < objects.Count; i++)
            {
                GameObject obj = objects[i];
                if (obj == null)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (func(obj))
                {
                    instance = obj;
                    return true;
                }
            }
            instance = null;
            return false;
        }

        public bool CheckOne(Func<GameObject, bool> condition, out GameObject instance)
        {
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                GameObject obj = objects[i];
                if (obj == null)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (condition(obj))
                {
                    instance = obj;
                    return true;
                }
            }
            instance = null;
            return false;
        }

        public bool CheckOne(Func<GameObject, bool> condition, PoolCheck poolCheck, out GameObject instance)
        {
            Func<GameObject, bool> func = GetCheckFunc(poolCheck);
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                GameObject obj = objects[i];
                if (obj == null)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (func(obj) && condition(obj))
                {
                    instance = obj;
                    return true;
                }
            }
            instance = null;
            return false;
        }

        Func<GameObject, bool> GetCheckFunc(PoolCheck poolCheck)
        {
            Func<GameObject, bool> func;
            switch (poolCheck)
            {
                case PoolCheck.Enable:
                    func = x => x.activeSelf;
                    break;
                case PoolCheck.Disable:
                    func = x => !x.activeSelf;
                    break;
                default:
                    func = x => true;
                    break;
            }
            return func;
        }

        GameObject GetWaittingObject(Func<GameObject, bool> condition = null)
        {
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                GameObject obj = objects[i];
                if (obj == null)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (!obj.activeSelf && condition(obj))
                    return obj;
            }
            return null;
        }
    }

    public class ObjectPool<T> where T : class, IObjectPool, new()
    {
        List<T> objects = new List<T>();

        public ObjectPool() { }

        public ObjectPool(int initCount)
        {
            for (int i = 0; i < initCount; i++)
                InitOne();
        }

        public ObjectPool(int initCount, object sender)
        {
            for (int i = 0; i < initCount; i++)
                InitOne(sender);
        }

        public T InitOne(object sender = null)
        {
            T instance = new T();
            instance.Init(sender);
            objects.Add(instance);
            return instance;
        }

        public T GetOne(object sender = null)
        {
            T obj = GetWaittingObject();
            if (obj == null)
            {
                obj = new T();
                obj.Init(sender);
                obj.Enable(sender);
                objects.Add(obj);
            }
            else
                obj.Enable(sender);
            return obj;
        }

        public T GetOne(Func<T, bool> condition, object sender = null)
        {
            T obj = GetWaittingObject(condition);
            if (obj == null)
            {
                obj = new T();
                obj.Init(sender);
                obj.Enable(sender);
                objects.Add(obj);
            }
            else
                obj.Enable(sender);
            return obj;
        }

        public void DisableAll()
        {
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                obj.Disable();
            }
        }

        public void DestroyAll()
        {
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if(obj != null && !obj.IsDestroy)
                    obj.Destroy();
            }
            objects.Clear();
        }

        public bool Check(PoolCheck poolCheck)
        {
            Func<T, bool> func = GetCheckFunc(poolCheck);
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (func(obj))
                    return true;
            }
            return false;
        }

        public bool Check(Func<T, bool> condition)
        {
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (condition(obj))
                    return true;
            }
            return false;
        }

        public bool Check(Func<T, bool> condition, PoolCheck poolCheck)
        {
            Func<T, bool> func = GetCheckFunc(poolCheck);
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (func(obj) && condition(obj))
                    return true;
            }
            return false;
        }

        public int CheckIndex(PoolCheck poolCheck)
        {
            Func<T, bool> func = GetCheckFunc(poolCheck);
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (func(obj))
                    return i;
            }
            return -1;
        }

        public int CheckIndex(Func<T, bool> condition)
        {
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (condition(obj))
                    return i;
            }
            return -1;
        }

        public int CheckIndex(Func<T, bool> condition, PoolCheck poolCheck)
        {
            Func<T, bool> func = GetCheckFunc(poolCheck);
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (func(obj) && condition(obj))
                    return i;
            }
            return -1;
        }

        public bool CheckOne(PoolCheck poolCheck, out T instance)
        {
            Func<T, bool> func = GetCheckFunc(poolCheck);
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (func(obj))
                {
                    instance = obj;
                    return true;
                }
            }
            instance = null;
            return false;
        }

        public bool CheckOne(Func<T, bool> condition, out T instance)
        {
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (condition(obj))
                {
                    instance = obj;
                    return true;
                }
            }
            instance = null;
            return false;
        }

        public bool CheckOne(Func<T, bool> condition, PoolCheck poolCheck, out T instance)
        {
            Func<T, bool> func = GetCheckFunc(poolCheck);
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (func(obj) && condition(obj))
                {
                    instance = obj;
                    return true;
                }
            }
            instance = null;
            return false;
        }

        public List<T> CheckAny(PoolCheck poolCheck)
        {
            List<T> list = new List<T>();
            Func<T, bool> func = GetCheckFunc(poolCheck);
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (func(obj))
                    list.Add(obj);
            }
            return list;
        }

        public List<T> CheckAny(Func<T, bool> condition)
        {
            List<T> list = new List<T>();
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (condition(obj))
                    list.Add(obj);
            }
            return list;
        }

        public List<T> CheckAny(Func<T, bool> condition, PoolCheck poolCheck)
        {
            List<T> list = new List<T>();
            Func<T, bool> func = GetCheckFunc(poolCheck);
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (func(obj) && condition(obj))
                    list.Add(obj);
            }
            return list;
        }

        public T CheckTop(Func<T, float> orderBy)
        {
            T instance = null;
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (instance == null || orderBy(instance) > orderBy(obj))
                    instance = obj;
            }
            return instance;
        }

        public T CheckTop(Func<T, float> orderBy, PoolCheck poolCheck)
        {
            T instance = null;
            Func<T, bool> func = GetCheckFunc(poolCheck);
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (func(obj) && (instance == null || orderBy(instance) > orderBy(obj)))
                    instance = obj;
            }
            return instance;
        }

        public T CheckTop(Func<T, float> orderBy, Func<T, bool> condition)
        {
            T instance = null;
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (condition(obj) && (instance == null || orderBy(instance) > orderBy(obj)))
                    instance = obj;
            }
            return instance;
        }

        public T CheckTop(Func<T, float> orderBy, Func<T, bool> condition, PoolCheck poolCheck)
        {
            T instance = null;
            Func<T, bool> func = GetCheckFunc(poolCheck);
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (func(obj) && condition(obj) && (instance == null || orderBy(instance) > orderBy(obj)))
                    instance = obj;
            }
            return instance;
        }

        public List<T> CheckTop(int top, Func<T, float> orderBy)
        {
            SortedList<float, T> sort = new SortedList<float, T>();
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                sort.Add(orderBy(obj), obj);
            }
            return sort.Take(top).Select(x => x.Value).ToList();
        }

        public List<T> CheckTop(int top, Func<T, float> orderBy, PoolCheck poolCheck)
        {
            SortedList<float, T> sort = new SortedList<float, T>();
            Func<T, bool> func = GetCheckFunc(poolCheck);
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if(func(obj))
                    sort.Add(orderBy(obj), obj);
            }
            return sort.Take(top).Select(x => x.Value).ToList();
        }

        public List<T> CheckTop(int top, Func<T, float> orderBy, Func<T, bool> condition)
        {
            SortedList<float, T> sort = new SortedList<float, T>();
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (condition(obj))
                    sort.Add(orderBy(obj), obj);
            }
            return sort.Take(top).Select(x => x.Value).ToList();
        }

        public List<T> CheckTop(int top, Func<T, float> orderBy, Func<T, bool> condition, PoolCheck poolCheck)
        {
            SortedList<float, T> sort = new SortedList<float, T>();
            Func<T, bool> func = GetCheckFunc(poolCheck);
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (func(obj) && condition(obj))
                    sort.Add(orderBy(obj), obj);
            }
            return sort.Take(top).Select(x => x.Value).ToList();
        }

        public T CheckTopByDescending(Func<T, float> orderBy)
        {
            T instance = null;
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (instance == null || orderBy(instance) < orderBy(obj))
                    instance = obj;
            }
            return instance;
        }

        public T CheckTopByDescending(Func<T, float> orderBy, PoolCheck poolCheck)
        {
            T instance = null;
            Func<T, bool> func = GetCheckFunc(poolCheck);
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (func(obj) && (instance == null || orderBy(instance) < orderBy(obj)))
                    instance = obj;
            }
            return instance;
        }

        public T CheckTopByDescending(Func<T, float> orderBy, Func<T, bool> condition)
        {
            T instance = null;
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (condition(obj) && (instance == null || orderBy(instance) < orderBy(obj)))
                    instance = obj;
            }
            return instance;
        }

        public T CheckTopByDescending(Func<T, float> orderBy, Func<T, bool> condition, PoolCheck poolCheck)
        {
            T instance = null;
            Func<T, bool> func = GetCheckFunc(poolCheck);
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (func(obj) && condition(obj) && (instance == null || orderBy(instance) < orderBy(obj)))
                    instance = obj;
            }
            return instance;
        }

        public List<T> CheckTopByDescending(int top, Func<T, float> orderBy)
        {
            SortedList<float, T> sort = new SortedList<float, T>();
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                sort.Add(-orderBy(obj), obj);
            }
            return sort.Take(top).Select(x => x.Value).ToList();
        }

        public List<T> CheckTopByDescending(int top, Func<T, float> orderBy, PoolCheck poolCheck)
        {
            SortedList<float, T> sort = new SortedList<float, T>();
            Func<T, bool> func = GetCheckFunc(poolCheck);
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (func(obj))
                    sort.Add(-orderBy(obj), obj);
            }
            return sort.Take(top).Select(x => x.Value).ToList();
        }

        public List<T> CheckTopByDescending(int top, Func<T, float> orderBy, Func<T, bool> condition)
        {
            SortedList<float, T> sort = new SortedList<float, T>();
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (condition(obj))
                    sort.Add(-orderBy(obj), obj);
            }
            return sort.Take(top).Select(x => x.Value).ToList();
        }

        public List<T> CheckTopByDescending(int top, Func<T, float> orderBy, Func<T, bool> condition, PoolCheck poolCheck)
        {
            SortedList<float, T> sort = new SortedList<float, T>();
            Func<T, bool> func = GetCheckFunc(poolCheck);
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (func(obj) && condition(obj))
                    sort.Add(-orderBy(obj), obj);
            }
            return sort.Take(top).Select(x => x.Value).ToList();
        }

        public void CallAny(Action<T> call, PoolCheck poolCheck)
        {
            Func<T, bool> func = GetCheckFunc(poolCheck);
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (func(obj))
                    call?.Invoke(obj);
            }
        }

        public void CallAny(Action<T> call, Func<T, bool> condition)
        {
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (condition(obj))
                    call?.Invoke(obj);
            }
        }

        public void CallAny(Action<T> call, Func<T, bool> condition, PoolCheck poolCheck)
        {
            Func<T, bool> func = GetCheckFunc(poolCheck);
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (func(obj) && condition(obj))
                    call?.Invoke(obj);
            }
        }

        public T CheckRandomOne(PoolCheck poolCheck)
        {
            List<T> list = CheckAny(poolCheck);
            if (list.Count > 0)
                return list[UnityEngine.Random.Range(0, list.Count)];
            else
                return null;
        }

        public T CheckRandomOne(Func<T, bool> condition)
        {
            List<T> list = CheckAny(condition);
            if (list.Count > 0)
                return list[UnityEngine.Random.Range(0, list.Count)];
            else
                return null;
        }

        public T CheckRandomOne(Func<T, bool> condition, PoolCheck poolCheck)
        {
            List<T> list = CheckAny(condition, poolCheck);
            if (list.Count > 0)
                return list[UnityEngine.Random.Range(0, list.Count)];
            else
                return null;
        }

        public int Count(PoolCheck poolCheck)
        {
            int count = 0;
            Func<T, bool> func = GetCheckFunc(poolCheck);
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (func(obj))
                    count++;
            }
            return count;
        }

        public int Count(Func<T, bool> condition)
        {
            int count = 0;
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (condition(obj))
                    count++;
            }
            return count;
        }

        public int Count(Func<T, bool> condition, PoolCheck poolCheck)
        {
            int count = 0;
            Func<T, bool> func = GetCheckFunc(poolCheck);
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (func(obj) && condition(obj))
                    count++;
            }
            return count;
        }

        Func<T, bool> GetCheckFunc(PoolCheck poolCheck)
        {
            Func<T, bool> func;
            switch (poolCheck)
            {
                case PoolCheck.Enable:
                    func = x => x.IsEnable;
                    break;
                case PoolCheck.Disable:
                    func = x => !x.IsEnable;
                    break;
                default:
                    func = x => true;
                    break;
            }
            return func;
        }

        T GetWaittingObject(Func<T, bool> condition = null)
        {
            if (condition == null)
                condition = x => true;
            for (int i = 0; i < objects.Count; i++)
            {
                T obj = objects[i];
                if (obj == null || obj.IsDestroy)
                {
                    objects.RemoveAt(i);
                    i--;
                    continue;
                }
                if (!obj.IsEnable && condition(obj))
                    return obj;
            }
            return null;
        }
    }

    public enum PoolCheck { All, Enable, Disable }
}