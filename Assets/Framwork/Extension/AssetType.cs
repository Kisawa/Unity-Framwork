using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framwork
{
    public enum AssetType
    {
#if ADDRESSABLE
        Resources,
        Addressables
#else
    Resources
#endif
    }
}