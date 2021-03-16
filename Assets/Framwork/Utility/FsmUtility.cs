using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

namespace Framwork
{
    public abstract class FsmUtility : MonoBehaviour
    {
        protected abstract IFsm StartFsm { get; }

        public IFsm CurrentFsm { get; private set; }
        public Type CurrentFsmType { get => CurrentFsm.GetType(); }

        public IFsm PreFsm { get; private set; }
        public Type PreFsmType { get => PreFsm.GetType(); }

        Dictionary<Type, IFsm> fsms;

        protected virtual void Awake()
        {
            fsms = new Dictionary<Type, IFsm>();
            FieldInfo[] fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            for (int i = 0; i < fields.Length; i++)
            {
                Type type = fields[i].FieldType;
                if (type.GetInterface("IFsm") != null)
                {
                    if (type.GetConstructor(new Type[] { }) == null)
                        Debug.LogError($"{GetType().Name}: FsmType of \"{type.Name}\" dont have default constructors.");
                    else
                    {
                        if (fsms.ContainsKey(type))
                        {
                            Debug.LogError($"{GetType().Name}: FsmType of \"{type.Name}\" has the same one.");
                            continue;
                        }
                        object fsm = Activator.CreateInstance(type);
                        fields[i].SetValue(this, fsm);
                        fsms.Add(type, fsm as IFsm);
                    }
                }
            }
        }

        protected virtual void Start()
        {
            if (fsms.Count > 0)
            {
                foreach (IFsm item in fsms.Values)
                    item.Init(this);
                if (StartFsm != null)
                {
                    StartFsm.Enter();
                    CurrentFsm = StartFsm;
                }
            }
        }

        protected virtual void Update()
        {
            CurrentFsm?.Update();
        }

        protected virtual void FixedUpdate()
        {
            CurrentFsm?.FixedUpdate();
        }

        public void ChangeState<T>(object sender = null) where T : IFsm
        {
            Type type = typeof(T);
            if (fsms.TryGetValue(type, out IFsm fsm))
            {
                if (fsm == CurrentFsm)
                {
                    fsm.Refresh(sender);
                    return;
                }
                CurrentFsm?.Leave();
                PreFsm = CurrentFsm;
                CurrentFsm = fsm;
                CurrentFsm.Enter(sender);
            }
            else
            {
                Debug.LogWarning($"{GetType().Name}: FsmType of \"{type.Name}\" dont inject.");
            }
        }

        public void ChangeState(IFsm fsm, object sender = null)
        {
            if (fsm == null)
            {
                Debug.LogWarning($"{GetType().Name}: FsmType of \"{fsm.GetType().Name}\" dont inject.");
            }
            else
            {
                if (fsm == CurrentFsm)
                {
                    fsm.Refresh(sender);
                    return;
                }
                CurrentFsm?.Leave();
                PreFsm = CurrentFsm;
                CurrentFsm = fsm;
                CurrentFsm.Enter(sender);
            }
        }
    }
}