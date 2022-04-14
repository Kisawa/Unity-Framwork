using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framwork;

public class PrefabManager : AssetUtility
{
    [Resources("circle")]
    public GameObject Circle;

    [Addressables("box")]
    ObjectPool box;

    [Resources("DataTable1")]
    static TextAsset dataTable1;

    protected override void EndInject()
    {
        base.EndInject();
        Debug.Log("Cutom resource load end");
    }
}