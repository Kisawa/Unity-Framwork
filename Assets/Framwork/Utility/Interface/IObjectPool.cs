using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framwork
{
    public interface IObjectPool
    {
        bool IsEnableFromPool { get; }

        bool IsDestroyFromPool { get; }

        void InitObjectPool(object sender = null);

        void Pool_Enable(object sender = null);

        void Pool_Disable();

        void Pool_Destroy();
    }
}