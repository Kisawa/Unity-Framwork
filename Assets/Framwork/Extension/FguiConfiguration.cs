using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Framwork/Create FguiConfiguration")]
public class FguiConfiguration : ScriptableObject
{
    public Vector2Int FguiDesignScreenSize;
    public string FguiFontAssetName;
    public AssetType FguiAssetType;
    public string CommonPackName;
    public string LanguageAssetName;
}