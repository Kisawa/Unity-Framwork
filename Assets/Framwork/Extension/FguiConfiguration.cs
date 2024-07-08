using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framwork
{
    [CreateAssetMenu(menuName = "Framwork/Create FguiConfiguration")]
    public class FguiConfiguration : ScriptableObject
    {
        public string AssetsResourcesPath = "FguiAssets";
        public Vector2Int FguiDesignScreenSize;
        public string FguiFontAssetName;
        public string CommonPackName;
        public string LanguageAssetName;
    }
}