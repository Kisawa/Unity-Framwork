using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framwork;

public class JsonData1 : JsonDataUntility
{
    public override string JsonAssetName => "JsonData1";

    [JsonField]
    string label;

    [JsonFieldGroup]
    Item message;

    [JsonField]
    Item[] persons;

    protected override void EndInject()
    {
        base.EndInject();
        Debug.LogError("JsonData1" + "  " + label);
    }
}

public struct Item
{
    public string name;
    int age;
}