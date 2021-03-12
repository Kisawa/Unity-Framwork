using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framwork
{
    public interface IObjectPool
    {
        bool IsEnable { get; }

        bool IsDestroy { get; }

        void Init(object sender = null);

        void Enable(object sender = null);

        void Disable();

        void Destroy();
    }
}