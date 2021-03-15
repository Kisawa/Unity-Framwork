using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framwork
{
    public enum AssetType
    {
#if ADDRESSABLES
        Resources,
        Addressables
#else
    Resources
#endif
    }
}