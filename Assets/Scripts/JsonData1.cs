using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonData1 : JsonDataUntility
{
    public override string JsonAssetName => "JsonData1";

    [JsonField]
    string label;

    [JsonFieldGroup]
    Item message;

    [JsonField]
    Item[] persons;
}

public struct Item
{
    public string name;
    int age;
}