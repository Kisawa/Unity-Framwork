using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framwork;

public class test : MonoBehaviour
{
    private void Start()
    {
        AssetManagment.LoadAssetAsync<Sprite>("", obj => { }, AssetType.Resources);
        AssetManagment.LoadAssetAsync<Sprite>("", obj => { }, AssetType.Addressables);
    }
}