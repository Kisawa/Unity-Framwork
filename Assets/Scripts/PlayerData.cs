using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framwork;

public class PlayerData : LocalSaveUtility
{
    [WaitingFreeToSave("mask")]
    LayerMask mask;

    [WaitingFreeToSave("label")]
    string label = "null";

    [WaitingFreeToSave("message")]
    Message[] friends;

    [WaitingFreeToSave("Family")]
    Dictionary<string, Person> Family;
}

[Unsafe]
public class Message
{
    public string Note;
    System.DateTime time;
}

public class Person
{
    public string name;
    int age;
    OtherMessage message;
}

[Unsafe]
public class OtherMessage
{
    public float Height;
    float Weight;
}