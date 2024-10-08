﻿using System;
using System.Linq;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Crypto.Common.PQC.Helpers;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class IntermediateValueHelper
{
    public static string Print(byte[] ba)
    {
        return BitConverter.ToString(ba).Replace("-","");
    }
    
    public static string PrintArray(int[] array)
    {
        var str = array.Aggregate("[", (current, t) => current + (t + ", "));

        str = str.Remove(str.Length - 2);
        str += "]";

        return str;
    }
    
    public static string Print2dArray(int[][] array)
    {
        var str = array.Aggregate("[", (current, t) => current + PrintArray(t) + ",\n");

        str = str.Remove(str.Length - 2);
        str += "]";

        return str;
    }

    public static string Print3dArray(int[][][] array)
    {
        var str = array.Aggregate("[", (current, t) => current + Print2dArray(t) + ",\n");

        str = str.Remove(str.Length - 2);
        str += "]";

        return str;
    }
}
