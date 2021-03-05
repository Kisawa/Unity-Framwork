using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : LocalSaveUtility
{
    [WaitingFreeToSave("label")]
    string label;

    [Unsafe]
    [WaitingFreeToSave("message")]
    Message[] friends;

    [DepthUnsafe]
    [WaitingFreeToSave("Family")]
    Dictionary<string, Person> Family;
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