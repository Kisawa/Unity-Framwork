namespace Framwork
{
    public static class Il2cppKeepCode
    {
        public static void LocalSaveUtilityKeepCode()
        {
            ES3Loader loader = ES3.StartLoad();
            #region Reader
            loader.TryToLoad("int", out int refer1);
            loader.TryToLoad("uint", out uint refer2);
            loader.TryToLoad("short", out short refer3);
            loader.TryToLoad("ushort", out ushort refer4);
            loader.TryToLoad("long", out long refer5);
            loader.TryToLoad("ulong", out ulong refer6);
            loader.TryToLoad("float", out float refer7);
            loader.TryToLoad("double", out double refer8);
            loader.TryToLoad("decimal", out decimal refer9);
            loader.TryToLoad("byte", out byte refer10);
            loader.TryToLoad("sbyte", out sbyte refer11);
            loader.TryToLoad("char", out char refer12);
            loader.TryToLoad("bool", out bool refer13);
            loader.TryToLoad("BigInteger", out System.Numerics.BigInteger refer14);
            loader.TryToLoad("UnityVector2", out UnityEngine.Vector2 refer15);
            loader.TryToLoad("UnityVector3", out UnityEngine.Vector3 refer16);
            loader.TryToLoad("UnityVector4", out UnityEngine.Vector4 refer17);
            loader.TryToLoad("UnityVector2Int", out UnityEngine.Vector2Int refer18);
            loader.TryToLoad("UnityVector3Int", out UnityEngine.Vector3Int refer19);
            loader.TryToLoad("UnityQuaternion", out UnityEngine.Quaternion refer20);
            loader.TryToLoad("UnityColor", out UnityEngine.Color refer21);
            loader.TryToLoad("UnityBounds", out UnityEngine.Bounds refer22);
            loader.TryToLoad("UnityBoundsInt", out UnityEngine.BoundsInt refer23);
            loader.TryToLoad("UnityRect", out UnityEngine.Rect refer24);
            loader.TryToLoad("UnityRectInt", out UnityEngine.RectInt refer25);
            loader.TryToLoad("UnityLayerMask", out UnityEngine.LayerMask refer26);
            #endregion
            ES3Saver saver = ES3.StartSave();
            #region Writer
            int _refer1 = 0;
            saver.Save<int>("int", _refer1);
            uint _refer2 = 0;
            saver.Save<uint>("uint", _refer2);
            short _refer3 = 0;
            saver.Save<short>("short", _refer3);
            ushort _refer4 = 0;
            saver.Save<ushort>("ushort", _refer4);
            long _refer5 = 0;
            saver.Save<long>("long", _refer5);
            ulong _refer6 = 0;
            saver.Save<ulong>("ulong", _refer6);
            float _refer7 = 0;
            saver.Save<float>("float", _refer7);
            double _refer8 = 0;
            saver.Save<double>("double", _refer8);
            decimal _refer9 = 0;
            saver.Save<decimal>("decimal", _refer9);
            byte _refer10 = 0;
            saver.Save<byte>("byte", _refer10);
            sbyte _refer11 = 0;
            saver.Save<sbyte>("sbyte", _refer11);
            char _refer12 = 'a';
            saver.Save<char>("char", _refer12);
            bool _refer13 = true;
            saver.Save<bool>("bool", _refer13);
            System.Numerics.BigInteger _refer14 = 0;
            saver.Save<System.Numerics.BigInteger>("BigInteger", _refer14);
            UnityEngine.Vector2 _refer15 = UnityEngine.Vector2.zero;
            saver.Save<UnityEngine.Vector2>("UnityVector2", _refer15);
            UnityEngine.Vector3 _refer16 = UnityEngine.Vector3.zero;
            saver.Save<UnityEngine.Vector3>("UnityVector3", _refer16);
            UnityEngine.Vector4 _refer17 = UnityEngine.Vector4.zero;
            saver.Save<UnityEngine.Vector4>("UnityVector4", _refer17);
            UnityEngine.Vector2Int _refer18 = UnityEngine.Vector2Int.zero;
            saver.Save<UnityEngine.Vector2Int>("UnityVector2Int", _refer18);
            UnityEngine.Vector3Int _refer19 = UnityEngine.Vector3Int.zero;
            saver.Save<UnityEngine.Vector3Int>("UnityVector3Int", _refer19);
            UnityEngine.Quaternion _refer20 = UnityEngine.Quaternion.identity;
            saver.Save<UnityEngine.Quaternion>("UnityQuaternion", _refer20);
            UnityEngine.Color _refer21 = UnityEngine.Color.white;
            saver.Save<UnityEngine.Color>("UnityColor", _refer21);
            UnityEngine.Bounds _refer22 = default;
            saver.Save<UnityEngine.Bounds>("UnityBounds", _refer22);
            UnityEngine.BoundsInt _refer23 = default;
            saver.Save<UnityEngine.BoundsInt>("UnityBoundsInt", _refer23);
            UnityEngine.Rect _refer24 = default;
            saver.Save<UnityEngine.Rect>("UnityRect", _refer24);
            UnityEngine.RectInt _refer25 = default;
            saver.Save<UnityEngine.RectInt>("UnityRectInt", _refer25);
            UnityEngine.LayerMask _refer26 = default;
            saver.Save<UnityEngine.LayerMask>("UnityLayerMask", _refer26);
            #endregion
        }
    }
}