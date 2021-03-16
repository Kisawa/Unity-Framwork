using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framwork;

public class PrefabManager : AssetUtility
{
    //[Resource("circle")]
    //public GameObject Circle;

    //[Addressable("box")]
    //ObjectPool box;

    [Resources("DataTable1")]
    static TextAsset dataTable1;

    protected override void EndInject()
    {
        base.EndInject();
        //string path = CheckPath(Circle, out AssetType assetType);
        //Debug.LogError(path + "   " + assetType);
        //path = CheckPath(box, out assetType);
        //Debug.LogError(path + "   " + assetType);
        //path = CheckPath(dataTable1, out assetType);
        //Debug.LogError(path + "   " + assetType);

        //Debug.LogError(Circle);
        //Debug.LogError(box);
        //Debug.LogError(dataTable1);
        //Unload();
        //Debug.LogError(Circle);
        //Debug.LogError(box);
        //Debug.LogError(dataTable1);
    }
}