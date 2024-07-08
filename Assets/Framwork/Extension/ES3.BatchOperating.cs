using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace Framwork
{
    public class ES3Saver
    {
        public ES3Settings setting;

        ES3Writer writer;
        ES3File file;

        public ES3Saver()
        {
            setting = new ES3Settings();
            Init();
        }

        public ES3Saver(ES3Settings setting)
        {
            this.setting = setting;
            Init();
        }

        void Init()
        {
            if (setting.location == global::ES3.Location.Cache)
                file = ES3File.GetOrCreateCachedFile(setting);
            else
                writer = ES3Writer.Create(setting);
        }

        public void Save<T>(string key, T value)
        {
            if (file != null)
                file.Save(key, value);
            else if (writer != null)
                writer.Write<T>(key, value);
        }

        public void EndSave()
        {
            if (writer != null)
            {
                writer.Save();
                writer.Dispose();
            }
        }
    }

    public class ES3Loader
    {
        public ES3Settings setting;

        ES3File file;

        public ES3Loader()
        {
            setting = new ES3Settings();
            Init();
        }

        public ES3Loader(ES3Settings setting)
        {
            this.setting = setting;
            Init();
        }

        void Init()
        {
            if (setting.location == global::ES3.Location.Cache)
                file = ES3File.GetOrCreateCachedFile(setting);
        }

        public bool TryToLoad<T>(string key, out T output)
        {
            bool res;
            output = default;
            if (file != null)
            {
                res = file.KeyExists(key);
                if (res)
                    output = file.Load<T>(key);
            }
            else
            {
                using (ES3Reader reader = ES3Reader.Create(setting))
                    res = reader == null ? false : reader.Goto(key);
                if (res)
                {
                    using (ES3Reader reader = ES3Reader.Create(setting))
                        output = reader.Read<T>(key);
                }
            }
            return res;
        }

        public void EndLoad() { }
    }

    public static partial class ES3
    {
        public static ES3Saver StartSave(ES3Settings setting = null)
        {
            if (setting == null)
                setting = new ES3Settings();
            return new ES3Saver(setting);
        }

        public static ES3Loader StartLoad(ES3Settings setting = null)
        {
            if (setting == null)
                setting = new ES3Settings();
            ES3Loader loader = new ES3Loader(setting);
            return loader;
        }
    }
}