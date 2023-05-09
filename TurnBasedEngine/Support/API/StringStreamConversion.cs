using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StringStreamConversion
{
    private static readonly char DELIMITER = '|';

    public static string Serialize(IEnumerable<string> stream)
    {
        return string.Join(DELIMITER + "", stream.ToArray<string>());
    }

    public static IEnumerable<string> Deserialize(string serialized)
    {
        return serialized.Split(new char[] { DELIMITER }, StringSplitOptions.None);
    }
}