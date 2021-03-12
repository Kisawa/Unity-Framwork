using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framwork
{
    public static class ReferencePool
    {
        static Dictionary<Type, Queue<IReferencePool>> referencePool = new Dictionary<Type, Queue<IReferencePool>>();

        public static T Pop<T>() where T : class, IReferencePool, new()
        {
            Type type = typeof(T);
            if (referencePool.ContainsKey(type))
            {
                if (referencePool[type].Count > 0)
                    return referencePool[type].Dequeue() as T;
            }
            T t = new T();
            return t;
        }

        public static void Recircle<T>(T refer) where T : class, IReferencePool
        {
            Type type = typeof(T);
            refer.Recircle();
            if (!referencePool.ContainsKey(type))
                referencePool[type] = new Queue<IReferencePool>();
            referencePool[type].Enqueue(refer);
        }
    }
}