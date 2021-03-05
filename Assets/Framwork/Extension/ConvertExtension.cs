using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;
using UnityEngine;
using System.Numerics;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public static class ConverExtension
{
    static Dictionary<int, string> unitOfNumber;
    static int MaxUnitOfNumber;

    /// <summary>
    /// 加载数字单位字典
    /// </summary>
    /// <param name="data">数字单位字典，key: 阶段位数（例如满1000进‘K’，key为4），value: 单位（K，M，B...）</param>
    public static void LoadUnitOfNumberData(Dictionary<int, string> data)
    {
        if (data != null && data.Count > 0) {
            unitOfNumber = data;
            MaxUnitOfNumber = unitOfNumber.Keys.OrderByDescending(x => x).FirstOrDefault();
        }
    }

    public static string NumberUnitConver(this int number, int maxSignificantFigure, bool keepDecimal = true)
    {
        return numberUnitConver(number.ToString(), maxSignificantFigure, keepDecimal);
    }
    public static string NumberUnitConver(this long number, int maxSignificantFigure, bool keepDecimal = true)
    {
        return numberUnitConver(number.ToString(), maxSignificantFigure, keepDecimal);
    }
    public static string NumberUnitConver(this BigInteger number, int maxSignificantFigure, bool keepDecimal = true)
    {
        return numberUnitConver(number.ToString(), maxSignificantFigure, keepDecimal);
    }

    /// <summary>
    /// 大数字转换单位制
    /// </summary>
    /// <param name="numStr"></param>
    /// <param name="maxSignificantFigure">保留有效数字最大位数，小于等于0时返回所有有效数字</param>
    /// <returns></returns>
    static string numberUnitConver(string numStr, int maxSignificantFigure, bool keepDecimal)
    {
        if (unitOfNumber == null) {
            Debug.LogWarning("ConverUtility: Unit of number is null.");
            maxSignificantFigure = 0;
        }
        StringBuilder str = new StringBuilder(numStr);
        int length = numStr.Length;
        int minUnitKey = length - maxSignificantFigure;
        if (minUnitKey <= 0 || maxSignificantFigure <= 0)
        {
            for (int i = 1; i < length; i++)
            {
                if (unitOfNumber == null || unitOfNumber.Count <= 0) {
                    if (i % 3 == 0)
                        str.Insert(length - i, ",");
                }
                else if (unitOfNumber.ContainsKey(i))
                    str.Insert(length - i + 1, ",");
            }
            return str.ToString();
        }
        string minUnitVal = null;
        int decimalLength = 0;
        int refer = 0;
        for (int i = 0; i < maxSignificantFigure; i++)
        {
            minUnitKey++;
            if (string.IsNullOrEmpty(minUnitVal))
            {
                if (unitOfNumber.TryGetValue(minUnitKey, out string obj))
                    minUnitVal = obj;
                else
                    decimalLength++;
            }
            else
            {
                if (unitOfNumber.ContainsKey(minUnitKey))
                {
                    str.Insert(maxSignificantFigure - i, ",");
                    refer++;
                }
            }
        }
        str.Remove(maxSignificantFigure + refer, str.Length - maxSignificantFigure - refer);
        if (decimalLength > 0)
        {
            if (maxSignificantFigure == decimalLength)
            {
                bool res = false;
                int _refer = 0;
                while (!res)
                {
                    if (++minUnitKey > MaxUnitOfNumber)
                    {
                        Debug.LogWarning("ConverUtility: Unit of number has no greater unit.");
                        return numberUnitConver(numStr, 0, keepDecimal);
                    }
                    _refer++;
                    if (_refer > 1)
                        str.Insert(0, "0");
                    if (unitOfNumber.TryGetValue(minUnitKey, out string obj))
                    {
                        minUnitVal = obj;
                        res = true;
                    }
                }
                str.Insert(0, "0.");
            }
            else
            {
                if (keepDecimal)
                    str.Insert(maxSignificantFigure - decimalLength + refer, ".");
                else {
                    int decimalCount = str.Length - maxSignificantFigure + decimalLength - refer;
                    str.Remove(maxSignificantFigure - decimalLength + refer, decimalCount);
                }
            }
        }
        str.Append(minUnitVal);
        return str.ToString();
    }

    public static Stack<int> SplitNumber(this int number, params int[] refer)
    {
        return number.splitNumber(refer);
    }

    public static string SplitToString(this int number, string spaceMark, params int[] refer)
    {
        return number.splitToString(spaceMark, "", false, refer);
    }

    public static string SplitToString(this int number, string spaceMark, bool whole, params int[] refer)
    {
        return number.splitToString(spaceMark, "", whole, refer);
    }

    public static string SplitToString(this int number, string spaceMark, string format, params int[] refer) 
    {
        return number.splitToString(spaceMark, format, false, refer);
    }

    public static string SplitToString(this int number, string spaceMark, string format, bool whole, params int[] refer)
    {
        return number.splitToString(spaceMark, format, whole, refer);
    }

    /// <summary>
    /// 数值插值分解并返回
    /// </summary>
    /// <param name="number">数值</param>
    /// <param name="refer">低位到高位的插值数</param>
    /// <returns></returns>
    /// Example: (10000, 60, 60, 24)  =>  Stack<int>() { 0, 2, 46, 40 }
    static Stack<int> splitNumber(this int number, params int[] refer)
    {
        Stack<int> nums = new Stack<int>();
        for (int i = 0; i < refer.Length; i++)
        {
            nums.Push(number % refer[i]);
            number = number / refer[i];
        }
        nums.Push(number);
        return nums;
    }

    /// <summary>
    /// 数值插值转换为string
    /// </summary>
    /// <param name="number">数值</param>
    /// <param name="spaceMark">间隔符</param>
    /// <param name="format">ToString("...")</param>
    /// <param name="whole">高位为0时是否保留</param>
    /// <param name="refer">低位到高位的插值数</param>
    /// <returns></returns>
    /// Example: (10000, ":", "D2", true, 60, 60, 24)  =>  00:02:46:40
    static string splitToString(this int number, string spaceMark, string format, bool whole, params int[] refer)
    {
        StringBuilder str = new StringBuilder();
        for (int i = 0; i < refer.Length; i++)
        {
            str.Insert(0, (number % refer[i]).ToString(format));
            number = number / refer[i];
            if (number == 0 && !whole)
                break;
            if (i < refer.Length - 1)
                str.Insert(0, spaceMark);
        }
        if (whole || number != 0) {
            str.Insert(0, spaceMark);
            str.Insert(0, number.ToString(format));
        }
        return str.ToString();
    }

    public static BigInteger Lerp(this BigInteger bigInteger, BigInteger targetBigInteger, float refer)
    {
        if (refer > 1)
            return targetBigInteger;
        else {
            BigInteger _refer = Mathf.CeilToInt(refer * 100);
            return bigInteger - bigInteger * _refer / 100 + targetBigInteger * _refer / 100;
        }
    }

    public static Vector3 ToVector3(this Vector2 vector2, float z = 0)
    {
        return new Vector3(vector2.x, vector2.y, z);
    }
}