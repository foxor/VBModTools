using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public static class FileSerialization {
    public static readonly string Extension = ".tbes";
    public static void Write(ISerializable toWrite, string path) {
        byte[] contents = toWrite.ToStream().data;
        var fStream = new FileStream(path, FileMode.Create);
        var bWrite = new BinaryWriter(fStream);
        bWrite.Write(DataVersionController.CURRENT_DATA_VERSION);
        bWrite.Write(contents.Length);
        bWrite.Write(contents);
        bWrite.Close();
        fStream.Close();
    }
    public static T Read<T>(string path) where T : ISerializable {
        var fStream = new FileStream(path, FileMode.Open);
        var bRead = new BinaryReader(fStream);
        var version = bRead.ReadUInt16();
        var length = bRead.ReadInt32();
        var contents = bRead.ReadBytes(length);
        bRead.Close();
        fStream.Close();
        return new Stream(contents, version).Consume<T>();
    }
    public static bool Exists(string path) {
        return File.Exists(path);
    }
}