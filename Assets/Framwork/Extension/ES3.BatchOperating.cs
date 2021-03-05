using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public static partial class ES3
{
    public static List<Type> UnsafeTypeList = new List<Type>();

    public static ES3Writer StartSave(ES3Settings setting = null)
    {
        if (setting == null)
            setting = new ES3Settings();
        return ES3Writer.Create(setting);
    }

    public static void ToSave<T>(this ES3Writer writer, string key, object value) 
    {
        writer.settings.safeReflection = false;
        writer.Write<T>(key, value);
    }

    public static void EndSave(this ES3Writer writer)
    {
        writer.Save();
        writer.Dispose();
    }

    public static ES3Reader StartLoad(ES3Settings setting = null)
    {
        if(setting == null)
            setting = new ES3Settings();
        ES3Reader reader = ES3Reader.Create(setting);
        return reader;
    }

    public static bool TryToLoad<T>(this ES3Reader reader, string key, out T output)
    {
        if (reader == null) {
            output = default;
            return false;
        }
        if (reader.ContainsKey(key)) {
            try
            {
                output = reader.Read<T>(key);
            }
            catch (Exception)
            {
                output = default;
                return false;
            }
            return true;
        }
        output = default;
        return false;
    }

    public static void EndLoad(this ES3Reader reader)
    {
        if(reader != null)
            reader.Dispose();
    }

    public static bool ContainsKey(this ES3Reader reader, string key) 
    {
        if (reader == null)
            return false;
        return reader.Goto(key);
    }
}