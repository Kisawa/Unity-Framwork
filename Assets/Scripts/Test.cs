using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framwork;

public class Test : MonoBehaviour
{
    public void singleDataTableLoad(DataTableUtility utility)
    {
        //DataTable1 dataTable1 = utility as DataTable1;
        //Debug.LogError(dataTable1.Data[0].label);
    }

    public void callback()
    {
        //AssetGroup assetGroup = new AssetGroup("box", AssetType.Addressables, ("circle", AssetType.Resources));
        //assetGroup.Load(() =>
        //{
        //    box = GameObjectReference.Instantiate("box");
        //    ObjectPool pool = new ObjectPool("circle", 0, default, AssetType.Resources);
        //    GameObjectReference.Instantiate("circle", AssetType.Resources);
        //});
    }

    GameObject box;

    private void OnGUI()
    {
        if (GUILayout.Button("Remove"))
        {
            AssetUtility.GetPrefabUtilitySelf<PrefabManager>().Unload();
        }
        if (GUILayout.Button("Remove"))
        {
            GameObjectReference.Destroy(box);
        }

        //if (GUILayout.Button("Info"))
        //    ReferenceManagment.DebugInfo();
    }
}
