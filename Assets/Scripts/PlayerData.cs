using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framwork;

public class PlayerData : LocalSaveUtility
{
    [WaitingFreeToSave("mask")]
    LayerMask mask;

    [WaitingFreeToSave("label")]
    string label;

    [Unsafe]
    [WaitingFreeToSave("message")]
    Message[] friends;

    [DepthUnsafe]
    [WaitingFreeToSave("Family")]
    Dictionary<string, Person> Family;

    protected override void Init()
    {
        base.Init();
        Debug.LogError("PlayerData");
    }
}

public class Message
{
    public string Note;
    System.DateTime time;
}

public class Person
{
    public string name;
    int age;
    [Unsafe]
    OtherMessage message;
}

public class OtherMessage
{
    public float Height;
    float Weight;
}