using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framwork;

public class DataTable1 : DataTableUtility
{
    public Dictionary<int, _DataTable1> Data;

    public override string TableAssetName => "DataTable1";

    public override AssetType AssetType => AssetType.Resources;

    protected override void StartInject()
    {
        base.StartInject();
        Data = new Dictionary<int, _DataTable1>();
    }

    protected override void InjectLine(params string[] currentLineTextList)
    {
        _DataTable1 _data = new _DataTable1();
        _data.label = currentLineTextList[0];
        _data.num = int.Parse(currentLineTextList[1]);
        Data.Add(currentRowIndex, _data);
    }

    protected override void EndInject()
    {
        base.EndInject();
        Debug.LogError("DataTable1" + "  " + Data[0].label);
    }
}

public struct _DataTable1
{
    public string label;
    public int num;
}