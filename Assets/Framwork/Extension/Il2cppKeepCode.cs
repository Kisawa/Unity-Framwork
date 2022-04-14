namespace Framwork
{
    public static class Il2cppKeepCode
    {
        public static void LocalSaveUtilityKeepCode()
        {
            ES3Reader reader = ES3.StartLoad();
            #region Reader
            reader.TryToLoad("int", out int refer1);
            reader.TryToLoad("uint", out uint refer2);
            reader.TryToLoad("short", out short refer3);
            reader.TryToLoad("ushort", out ushort refer4);
            reader.TryToLoad("long", out long refer5);
            reader.TryToLoad("ulong", out ulong refer6);
            reader.TryToLoad("float", out float refer7);
            reader.TryToLoad("double", out double refer8);
            reader.TryToLoad("decimal", out decimal refer9);
            reader.TryToLoad("byte", out byte refer10);
            reader.TryToLoad("sbyte", out sbyte refer11);
            reader.TryToLoad("char", out char refer12);
            reader.TryToLoad("bool", out bool refer13);
            reader.TryToLoad("BigInteger", out System.Numerics.BigInteger refer14);
            reader.TryToLoad("UnityVector2", out UnityEngine.Vector2 refer15);
            reader.TryToLoad("UnityVector3", out UnityEngine.Vector3 refer16);
            reader.TryToLoad("UnityVector4", out UnityEngine.Vector4 refer17);
            reader.TryToLoad("UnityVector2Int", out UnityEngine.Vector2Int refer18);
            reader.TryToLoad("UnityVector3Int", out UnityEngine.Vector3Int refer19);
            reader.TryToLoad("UnityQuaternion", out UnityEngine.Quaternion refer20);
            reader.TryToLoad("UnityColor", out UnityEngine.Color refer21);
            reader.TryToLoad("UnityBounds", out UnityEngine.Bounds refer22);
            reader.TryToLoad("UnityBoundsInt", out UnityEngine.BoundsInt refer23);
            reader.TryToLoad("UnityRect", out UnityEngine.Rect refer24);
            reader.TryToLoad("UnityRectInt", out UnityEngine.RectInt refer25);
            reader.TryToLoad("UnityLayerMask", out UnityEngine.LayerMask refer26);
            #endregion
            ES3Writer writer = ES3.StartSave();
            #region Writer
            int _refer1 = 0;
            writer.ToSave<int>("int", _refer1);
            uint _refer2 = 0;
            writer.ToSave<uint>("uint", _refer2);
            short _refer3 = 0;
            writer.ToSave<short>("short", _refer3);
            ushort _refer4 = 0;
            writer.ToSave<ushort>("ushort", _refer4);
            long _refer5 = 0;
            writer.ToSave<long>("long", _refer5);
            ulong _refer6 = 0;
            writer.ToSave<ulong>("ulong", _refer6);
            float _refer7 = 0;
            writer.ToSave<float>("float", _refer7);
            double _refer8 = 0;
            writer.ToSave<double>("double", _refer8);
            decimal _refer9 = 0;
            writer.ToSave<decimal>("decimal", _refer9);
            byte _refer10 = 0;
            writer.ToSave<byte>("byte", _refer10);
            sbyte _refer11 = 0;
            writer.ToSave<sbyte>("sbyte", _refer11);
            char _refer12 = 'a';
            writer.ToSave<char>("char", _refer12);
            bool _refer13 = true;
            writer.ToSave<bool>("bool", _refer13);
            System.Numerics.BigInteger _refer14 = 0;
            writer.ToSave<System.Numerics.BigInteger>("BigInteger", _refer14);
            UnityEngine.Vector2 _refer15 = UnityEngine.Vector2.zero;
            writer.ToSave<UnityEngine.Vector2>("UnityVector2", _refer15);
            UnityEngine.Vector3 _refer16 = UnityEngine.Vector3.zero;
            writer.ToSave<UnityEngine.Vector3>("UnityVector3", _refer16);
            UnityEngine.Vector4 _refer17 = UnityEngine.Vector4.zero;
            writer.ToSave<UnityEngine.Vector4>("UnityVector4", _refer17);
            UnityEngine.Vector2Int _refer18 = UnityEngine.Vector2Int.zero;
            writer.ToSave<UnityEngine.Vector2Int>("UnityVector2Int", _refer18);
            UnityEngine.Vector3Int _refer19 = UnityEngine.Vector3Int.zero;
            writer.ToSave<UnityEngine.Vector3Int>("UnityVector3Int", _refer19);
            UnityEngine.Quaternion _refer20 = UnityEngine.Quaternion.identity;
            writer.ToSave<UnityEngine.Quaternion>("UnityQuaternion", _refer20);
            UnityEngine.Color _refer21 = UnityEngine.Color.white;
            writer.ToSave<UnityEngine.Color>("UnityColor", _refer21);
            UnityEngine.Bounds _refer22 = default;
            writer.ToSave<UnityEngine.Bounds>("UnityBounds", _refer22);
            UnityEngine.BoundsInt _refer23 = default;
            writer.ToSave<UnityEngine.BoundsInt>("UnityBoundsInt", _refer23);
            UnityEngine.Rect _refer24 = default;
            writer.ToSave<UnityEngine.Rect>("UnityRect", _refer24);
            UnityEngine.RectInt _refer25 = default;
            writer.ToSave<UnityEngine.RectInt>("UnityRectInt", _refer25);
            UnityEngine.LayerMask _refer26 = default;
            writer.ToSave<UnityEngine.LayerMask>("UnityLayerMask", _refer26);
            #endregion
        }
    }
}