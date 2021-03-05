using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framwork
{
    public enum AssetType
    {
#if ADDRESSABLE
        Resources,
        Addressable
#else
    Resources
#endif
    }
}