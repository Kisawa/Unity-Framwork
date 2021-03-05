using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public class ObjectsPool
{
    GameObject prefab;
    public Transform Parent;
    List<GameObject> objects = new List<GameObject>();

    public ObjectsPool(GameObject obj, Transform parent = null, Vector3 parentScale = default, int initCount = 0, string sortingLayerName = null) {
        prefab = obj;
        if (parent == null) {
            GameObject parentObj = new GameObject(obj.name + "Group");
            if (parentScale != default)
                parentObj.transform.localScale = parentScale;
            if (!string.IsNullOrEmpty(sortingLayerName))
                parentObj.AddComponent<SortingGroup>().sortingLayerName = sortingLayerName;
            parent = parentObj.transform;
        }
        Parent = parent;
        for (int i = 0; i < initCount; i++)
        {
#if ADDRESSABLE
            GameObject _obj = GameObjectReference.Instantiate(prefab, parent);
#else
            GameObject _obj = Object.Instantiate(prefab, parent);
#endif
            _obj.SetActive(false);
            objects.Add(_obj);
        }
    }

    public ObjectsPool(string assetName, Transform parent = null, Vector3 parentScale = default, int initCount = 0, string sortingLayerName = null)
    {
#if ADDRESSABLE
        GameObjectReference.LoadGameObjectAssetAsync(assetName, false, obj => {
            prefab = obj;
            if (parent == null)
            {
                GameObject parentObj = new GameObject(obj.name + "Group");
                if (parentScale != default)
                    parentObj.transform.localScale = parentScale;
                if (!string.IsNullOrEmpty(sortingLayerName))
                    parentObj.AddComponent<SortingGroup>().sortingLayerName = sortingLayerName;
                parent = parentObj.transform;
            }
            Parent = parent;
            for (int i = 0; i < initCount; i++)
            {
                GameObject _obj = GameObjectReference.Instantiate(prefab, parent);
                _obj.SetActive(false);
                objects.Add(_obj);
            }
        });
#else
        GameObject obj = Resources.Load<GameObject>(assetName);
        prefab = obj;
        if (parent == null)
        {
            GameObject parentObj = new GameObject(obj.name + "Group");
            if (parentScale != default)
                parentObj.transform.localScale = parentScale;
            if (!string.IsNullOrEmpty(sortingLayerName))
                parentObj.AddComponent<SortingGroup>().sortingLayerName = sortingLayerName;
            parent = parentObj.transform;
        }
        Parent = parent;
        for (int i = 0; i < initCount; i++)
        {
            GameObject _obj = Object.Instantiate(prefab, parent);
            _obj.SetActive(false);
            objects.Add(_obj);
        }
#endif
    }

    public GameObject GetOne(Func<GameObject, bool> condition = null) {
        GameObject obj = GetNoActiveGameObject(condition);
        if (obj == null)
        {
#if ADDRESSABLE
            obj = GameObjectReference.Instantiate(prefab, Parent);
#else
            obj = Object.Instantiate(prefab, Parent);
#endif
            objects.Add(obj);
        }
        else
            obj.SetActive(true);
        return obj;
    }

    public GameObject GetOne(Vector3 position, Quaternion quaternion, Func<GameObject, bool> condition = null) {
        GameObject obj = GetNoActiveGameObject(condition);
        if (obj == null)
        {
#if ADDRESSABLE
            obj = GameObjectReference.Instantiate(prefab, position, quaternion, Parent);
#else
            obj = Object.Instantiate(prefab, position, quaternion, Parent);
#endif
            objects.Add(obj);
        }
        else
        {
            obj.transform.position = position;
            obj.transform.rotation = quaternion;
            obj.SetActive(true);
        }
        return obj;
    }

    public GameObject GetOne(Action<GameObject> UntilEnableCall, Func<GameObject, bool> condition = null) {
        GameObject obj = GetNoActiveGameObject(condition);
        if (obj == null)
        {
#if ADDRESSABLE
            obj = GameObjectReference.Instantiate(prefab, Parent);
#else
            obj = Object.Instantiate(prefab, Parent);
#endif
            objects.Add(obj);
            UntilEnableCall?.Invoke(obj);
        }
        else
        {
            UntilEnableCall?.Invoke(obj);
            obj.SetActive(true);
        }
        return obj;
    }

    public GameObject GetOne(Action<GameObject> UntilEnableCall, Vector3 position, Quaternion quaternion, Func<GameObject, bool> condition = null)
    {
        GameObject obj = GetNoActiveGameObject(condition);
        if (obj == null)
        {
#if ADDRESSABLE
            obj = GameObjectReference.Instantiate(prefab, position, quaternion, Parent);
#else
            obj = Object.Instantiate(prefab, position, quaternion, Parent);
#endif
            objects.Add(obj);
            UntilEnableCall?.Invoke(obj);
        }
        else
        {
            obj.transform.position = position;
            obj.transform.rotation = quaternion;
            UntilEnableCall?.Invoke(obj);
            obj.SetActive(true);
        }
        return obj;
    }

    public void HideAll() {
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i] != null)
            {
                objects[i].SetActive(false);
            }
        }
    }

    public void ClearAll() {
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i] != null)
            {
#if ADDRESSABLE
                GameObjectReference.Destroy(objects[i]);
#else
                Object.Destroy(objects[i]);
#endif
            }
        }
        objects.Clear();
    }

    public bool Check(Func<GameObject, bool> compare, PoolCheck poolCheck)
    {
        bool res = false;
        Func<GameObject, bool> func = GetCheckFunc(poolCheck);
        for (int i = 0; i < objects.Count; i++)
        {
            GameObject obj = objects[i];
            if (obj == null)
                continue;
            bool _res = false;
            if (func != null)
            {
                _res = func(objects[i]);
                if (_res)
                    _res = compare(obj);
                else
                    continue;
            }
            else
            {
                _res = compare(obj);
            }
            if (_res) {
                res = true;
                break;
            }
        }
        return res;
    }

    public bool CheckIndex(Func<GameObject, bool> compare, out int index, PoolCheck poolCheck)
    {
        bool res = false;
        Func<GameObject, bool> func = GetCheckFunc(poolCheck);
        index = -1;
        for (int i = 0; i < objects.Count; i++)
        {
            GameObject obj = objects[i];
            if (obj == null)
                continue;
            bool _res = false;
            if (func != null)
            {
                _res = func(objects[i]);
                if (_res)
                    _res = compare(obj);
                else
                    continue;
            }
            else
            {
                _res = compare(obj);
            }
            if (_res) 
            {
                res = true;
                index = i;
                break;
            }
        }
        return res;
    }

    public bool CheckOne(Func<GameObject, bool> compare, out GameObject obj, PoolCheck poolCheck)
    {
        bool res = false;
        Func<GameObject, bool> func = GetCheckFunc(poolCheck);
        obj = null;
        for (int i = 0; i < objects.Count; i++)
        {
            GameObject _obj = objects[i];
            if (_obj == null)
                continue;
            bool _res = false;
            if (func != null)
            {
                _res = func(objects[i]);
                if (_res)
                    _res = compare(_obj);
                else
                    continue;
            }
            else
            {
                _res = compare(_obj);
            }
            if (_res)
            {
                res = true;
                obj = _obj;
                break;
            }
        }
        return res;
    }

    Func<GameObject, bool> GetCheckFunc(PoolCheck poolCheck)
    {
        Func<GameObject, bool> func = null;
        switch (poolCheck)
        {
            case PoolCheck.Enable:
                func = x => x.activeSelf;
                break;
            case PoolCheck.Disable:
                func = x => !x.activeSelf;
                break;
            default:
                break;
        }
        return func;
    }

    GameObject GetNoActiveGameObject(Func<GameObject, bool> condition = null) 
    {
        GameObject obj = null;
        List<int> removeList = new List<int>();
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i] == null)
            {
                removeList.Add(i);
                break;
            }
            bool res = false;
            if (!objects[i].activeSelf)
                res = true;
            if (res && condition != null)
                res = condition(objects[i]);
            if (res)
                obj = objects[i];
        }
        foreach (int item in removeList)
            objects.RemoveAt(item);
        return obj;
    }
}

public class ObjectsPool<T> where T : class, IObjectPool, new()
{
    List<T> objects = new List<T>();

    public ObjectsPool() { }

    public ObjectsPool(int initCount)
    {
        for (int i = 0; i < initCount; i++)
        {
            T obj = new T();
            obj.InitObjectPool();
            objects.Add(obj);
        }
    }

    public T InitOne(object sender) {
        T obj = new T();
        obj.InitObjectPool(sender);
        objects.Add(obj);
        return obj;
    }

    public T GetOne(object sender = null) {
        T obj = GetNoEnableObject();
        if (obj == null)
        {
            obj = new T();
            obj.InitObjectPool(sender);
            obj.Pool_Enable(sender);
            objects.Add(obj);
        }
        else
        {
            obj.Pool_Enable(sender);
        }
        return obj;
    }

    public T GetOne(Func<T, bool> condition, object sender = null)
    {
        T obj = GetNoEnableObject(condition);
        if (obj == null)
        {
            obj = new T();
            obj.InitObjectPool(sender);
            obj.Pool_Enable(sender);
            objects.Add(obj);
        }
        else
        {
            obj.Pool_Enable(sender);
        }
        return obj;
    }

    public T GetOne(Action<T> UntilEnableCall, Func<T, bool> condition = null, object sender = null)
    {
        T obj = GetNoEnableObject(condition);
        if (obj == null)
        {
            obj = new T();
            obj.InitObjectPool(sender);
            UntilEnableCall?.Invoke(obj);
            obj.Pool_Enable(sender);
            objects.Add(obj);
        }
        else
        {
            UntilEnableCall?.Invoke(obj);
            obj.Pool_Enable(sender);
        }
        return obj;
    }

    public void Remove(T obj)
    {
        if (objects.Contains(obj))
            objects.Remove(obj);
    }

    public void DisableAll()
    {
        foreach (T item in objects)
        {
            if (item == null || item.IsDestroyFromPool)
            {
                objects.Remove(item);
                break;
            }
            item.Pool_Disable();
        }
    }

    public void ClearAll()
    {
        foreach (T item in objects)
        {
            if (item == null || item.IsDestroyFromPool)
                break;
            item.Pool_Destroy();
        }
        objects.Clear();
    }

    public bool Check(Func<T, bool> compare, PoolCheck poolCheck)
    {
        bool res = false;
        Func<T, bool> func = GetCheckFunc(poolCheck);
        for (int i = 0; i < objects.Count; i++)
        {
            T obj = objects[i];
            if (obj == null || obj.IsDestroyFromPool)
                continue;
            bool _res = false;
            if (func != null)
            {
                _res = func(objects[i]);
                if (_res)
                {
                    if (compare != null)
                        _res = compare(obj);
                }
                else
                    continue;
            }
            else {
                _res = compare(obj);
            }
            if (_res) {
                res = true;
                break;
            }
        }
        return res;
    }

    public bool CheckIndex(Func<T, bool> compare, out int index, PoolCheck poolCheck)
    {
        bool res = false;
        Func<T, bool> func = GetCheckFunc(poolCheck);
        index = -1;
        for (int i = 0; i < objects.Count; i++)
        {
            T obj = objects[i];
            if (obj == null || obj.IsDestroyFromPool)
                continue;
            bool _res = false;
            if (func != null)
            {
                _res = func(objects[i]);
                if (_res) {
                    if (compare != null)
                        _res = compare(obj);
                }
                else
                    continue;
            }
            else
            {
                _res = compare(obj);
            }
            if (_res)
            {
                res = true;
                index = i;
                break;
            }
        }
        return res;
    }

    public bool CheckOne(Func<T, bool> compare, out T obj, PoolCheck poolCheck)
    {
        bool res = false;
        Func<T, bool> func = GetCheckFunc(poolCheck);
        obj = null;
        for (int i = 0; i < objects.Count; i++)
        {
            T _obj = objects[i];
            if (_obj == null || _obj.IsDestroyFromPool)
                continue;
            bool _res = false;
            if (func != null)
            {
                _res = func(objects[i]);
                if (_res) { 
                    if(compare != null)
                        _res = compare(_obj);
                }
                else
                    continue;
            }
            else
            {
                _res = compare(_obj);
            }
            if (_res)
            {
                res = true;
                obj = _obj;
                break;
            }
        }
        return res;
    }

    public List<T> CheckAny(Func<T, bool> compare, PoolCheck poolCheck) {
        Func<T, bool> func = GetCheckFunc(poolCheck);
        List<T> objs = new List<T>();
        for (int i = 0; i < objects.Count; i++)
        {
            T _obj = objects[i];
            if (_obj == null || _obj.IsDestroyFromPool)
                continue;
            bool _res = false;
            if (func != null)
            {
                _res = func(objects[i]);
                if (_res) { 
                    if(compare != null)
                        _res = compare(_obj);
                }
                else
                    continue;
            }
            else
                _res = compare(_obj);
            if (_res)
                objs.Add(_obj);
        }
        return objs;
    }

    public void CheckAny(Queue<T> queue, Func<T, bool> compare, PoolCheck poolCheck) {
        Func<T, bool> func = GetCheckFunc(poolCheck);
        if (queue == null)
            throw NullReferenceException($"ObjectPool: CheckAny func with a null Queue");
        for (int i = 0; i < objects.Count; i++)
        {
            T _obj = objects[i];
            if (_obj == null || _obj.IsDestroyFromPool)
                continue;
            bool _res = false;
            if (func != null)
            {
                _res = func(objects[i]);
                if (_res)
                { 
                    if(compare != null)
                        _res = compare(_obj);
                }
                else
                    continue;
            }
            else
                _res = compare(_obj);
            if (_res)
                queue.Enqueue(_obj);
        }
    }

    private Exception NullReferenceException(string v)
    {
        throw new NotImplementedException();
    }

    public T CheckTop(Func<T, float> orderRefer, PoolCheck poolCheck, Func<T, bool> compare = null) {
        Func<T, bool> func = GetCheckFunc(poolCheck);
        T obj = null;
        for (int i = 0; i < objects.Count; i++)
        {
            T _obj = objects[i];
            if (_obj == null || _obj.IsDestroyFromPool)
                continue;
            bool _res = false;
            if (func != null)
            {
                _res = func(objects[i]);
                if (_res)
                {
                    if (compare != null)
                        _res = compare(_obj);
                }
                else
                    continue;
            }
            else if (compare != null)
                _res = compare(_obj);
            else
                _res = true;
            if (_res) {
                if (obj == null)
                    obj = _obj;
                else {
                    if (orderRefer(obj) > orderRefer(_obj))
                        obj = _obj;
                }
            }
        }
        return obj;
    }

    public T CheckTopByDescending(Func<T, float> orderRefer, PoolCheck poolCheck, Func<T, bool> compare = null) {
        Func<T, bool> func = GetCheckFunc(poolCheck);
        T obj = null;
        for (int i = 0; i < objects.Count; i++)
        {
            T _obj = objects[i];
            if (_obj == null || _obj.IsDestroyFromPool)
                continue;
            bool _res = false;
            if (func != null)
            {
                _res = func(objects[i]);
                if (_res)
                {
                    if (compare != null)
                        _res = compare(_obj);
                }
                else
                    continue;
            }
            else if (compare != null)
                _res = compare(_obj);
            else
                _res = true;
            if (_res)
            {
                if (obj == null)
                    obj = _obj;
                else
                {
                    if (orderRefer(obj) < orderRefer(_obj))
                        obj = _obj;
                }
            }
        }
        return obj;
    }

    public void CallAny(Action<T> call, PoolCheck poolCheck)
    {
        Func<T, bool> func = GetCheckFunc(poolCheck);
        for (int i = 0; i < objects.Count; i++)
        {
            T _obj = objects[i];
            if (_obj == null || _obj.IsDestroyFromPool)
                continue;
            if (func == null)
                call(_obj);
            else if (func(_obj))
                call(_obj);
        }
    }

    public void CallAny(Action<T> call, Func<T, bool> checkFunc)
    {
        for (int i = 0; i < objects.Count; i++)
        {
            T _obj = objects[i];
            if (_obj == null || _obj.IsDestroyFromPool)
                continue;
            if (checkFunc == null || checkFunc(_obj))
                call(_obj);
        }
    }

    public T CheckRandomOne(PoolCheck poolCheck, Func<T, bool> compare = null) {
        List<T> list = null;
        switch (poolCheck)
        {
            case PoolCheck.Enable:
                list = objects.Where(x => x != null && !x.IsDestroyFromPool && x.IsEnableFromPool && (compare == null || (compare != null && compare(x)))).ToList();
                break;
            case PoolCheck.Disable:
                list = objects.Where(x => x != null && !x.IsDestroyFromPool && !x.IsEnableFromPool && (compare == null || (compare != null && compare(x)))).ToList();
                break;
            default:
                list = objects.Where(x => x != null && !x.IsDestroyFromPool && (compare == null || (compare != null && compare(x)))).ToList();
                break;
        }
        T obj = null;
        if (list.Count > 0) {
            int random = UnityEngine.Random.Range(0, list.Count);
            obj = list[random];
        }
        return obj;
    }

    public int Count(PoolCheck poolCheck = PoolCheck.All, Func<T, bool> compare = null) {
        Func<T, bool> func = GetCheckFunc(poolCheck);
        int count = 0;
        for (int i = 0; i < objects.Count; i++)
        {
            T _obj = objects[i];
            if (_obj == null || _obj.IsDestroyFromPool)
                continue;
            bool _res = true;
            if (func != null)
            {
                _res = func(objects[i]);
                if (_res) { 
                    if(compare != null)
                        _res = compare(_obj);
                }
                else
                    continue;
            }
            else if(compare != null)
                _res = compare(_obj);
            if (_res)
                count++;
        }
        return count;
    }

    Func<T, bool> GetCheckFunc(PoolCheck poolCheck) {
        Func<T, bool> func = null;
        switch (poolCheck)
        {
            case PoolCheck.Enable:
                func = x => x.IsEnableFromPool;
                break;
            case PoolCheck.Disable:
                func = x => !x.IsEnableFromPool;
                break;
            default:
                break;
        }
        return func;
    }

    T GetNoEnableObject(Func<T, bool> condition = null)
    {
        T obj = null;
        List<int> removeList = new List<int>();
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i] == null || objects[i].IsDestroyFromPool)
            {
                removeList.Add(i);
                continue;
            }
            bool res = false;
            if (!objects[i].IsEnableFromPool)
                res = true;
            if (res && condition != null)
                res = condition(objects[i]);
            if (res)
                obj = objects[i];
        }
        foreach (int item in removeList)
            objects.RemoveAt(item);
        return obj;
    }
}

public enum PoolCheck { All, Enable, Disable }