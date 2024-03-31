using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

public static class Helper
{
    private static Dictionary<Type, int> sizes = new Dictionary<Type, int>();

    public static int SizeOf(Type type)
    {
        int size;
        if (sizes.TryGetValue(type, out size))
        {
            return size;
        }

        size = SizeOfType(type);
        sizes.Add(type, size);
        return size;
    }

    private static int SizeOfType(Type type)
    {
        var dm = new DynamicMethod("SizeOfType", typeof(int), new Type[] { });
        ILGenerator il = dm.GetILGenerator();
        il.Emit(OpCodes.Sizeof, type);
        il.Emit(OpCodes.Ret);
        return (int)dm.Invoke(null, null);
    }

    public static byte[] ToFlipped<T>(T input)
    {
        var result = (byte[])typeof(BitConverter).GetMethod("GetBytes", new[] { typeof(T) })
            .Invoke(null, new object[] { input });
        Array.Reverse(result);

        return result;
    }

    public static BitArray ShiftRight(this BitArray instance, uint shiftCount)
    {
        return new BitArray(new bool[shiftCount].Concat(instance.Cast<bool>().Take(instance.Length - 1)).ToArray());
    }

    public static BitArray ShiftLeft(this BitArray instance, uint shiftCount)
    {
        return new BitArray((instance.Cast<bool>().Take(instance.Length - 1).ToArray()).Concat(new bool[shiftCount]).ToArray());
    }

    public static uint GetCodeFromString(string inCCState)
    {
        byte[] CC = Encoding.ASCII.GetBytes(inCCState);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(CC);

        return BitConverter.ToUInt32(CC, 0);
    }

    public static string GetCodeFromInt(uint inCCState)
    {
        byte[] CC = BitConverter.GetBytes(inCCState);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(CC);

        return Encoding.ASCII.GetString(CC);
    }

    public static void Resize<T>(this List<T> list, int sz) where T : new()
    {
        int cur = list.Count;
        if (sz < cur)
            list.RemoveRange(sz, cur - sz);
        else if (sz > cur)
        {
            if (sz > list.Capacity)//this bit is purely an optimisation, to avoid multiple automatic capacity changes.
                list.Capacity = sz;
            //list.AddRange(Enumerable.Repeat(new T(), sz - cur));
            list.AddRange(Enumerable.Range(1, sz - cur).Select(i => new T()));
        }
    }
}
