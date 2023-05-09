using System.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

[ProtectStatics]
public class Stream {
    public static Encoding StringEncoding = System.Text.Encoding.UTF8;
    public static Dictionary<ushort, Type> AvailableAssemblyTypes;

    public byte[] data;
    public int index;
    public int version;
    public Stream(int size) {
        data = new byte[size];
        index = 0;
        version = DataVersionController.CURRENT_DATA_VERSION;
    }
    public Stream(byte[] data, int version) {
        this.data = data;
        index = 0;
        if (version != DataVersionController.CURRENT_DATA_VERSION) {
            Debug.LogWarning("Deserializing old version!");
        }
        this.version = version;
    }

    #region Static setup

    public static string CleanTypeName(string typeName) {
        if (typeName.Contains("`")) {
            return typeName.Substring(0, typeName.IndexOf('`'));
        }
        return typeName;
    }

    public static bool IsAssemblyType(Type t) {
        return typeof(IAssemblyType).IsAssignableFrom(t) ||
            t.GetCustomAttributeCached(typeof(SerializableEnumAttribute)) != null;
    }

    public static void UpdateAvailableAssemblyTypes() {
        IEnumerable<Type> AllTypes = Assembly.GetExecutingAssembly().GetTypes().Where(IsAssemblyType).Where(x => x.HasIndex());
        AvailableAssemblyTypes = AllTypes.ToDictionary((x) => {
            return x.GetIndex();
        });
    }

    public static IEnumerable<Type> AssemblyTypes {
        get {
            if (AvailableAssemblyTypes == null) {
                UpdateAvailableAssemblyTypes();
            }
            return AvailableAssemblyTypes.Values;
        }
    }
    public static Type TypeFromIndex(ushort typeIndex) {
        return AvailableAssemblyTypes[typeIndex];
    }

    static Stream() {
        UpdateAvailableAssemblyTypes();
    }
    #endregion

    #region Generic serialization
    // This function is very performance sensitive.
    public Type FullyResolveType() {
        if (index >= data.Length) {
            // If we're told to interpert an empty stream, it becomes SNull
            return typeof(SNull);
        }
        Type t;
        if (this.version < 8) {
            var typeName = ConsumeString();
            t = AssemblyTypes.Where(x => string.Equals(x.GetNameCached(), typeName)).Single();
        }
        else {
            var typeIndex = ConsumeShort();
            if (!AvailableAssemblyTypes.TryGetValue(typeIndex, out t)) {
                throw new Exception($"Couldn't find type index {typeIndex}");
            }
        }
        if (t.ContainsGenericParametersCached()) {
            byte numParameters = ConsumeByte();
            var typeParameters = ArrayPool<Type>.Get(numParameters);
            for (byte i = 0; i < numParameters; i++) {
                typeParameters[i] = FullyResolveType();
            }
            t = t.MakeGenericTypeCached(typeParameters);
            ArrayPool<Type>.Return(typeParameters);
        }
        return t;
    }

    public void RecursiveTypeToStream(Type type) {
        WriteShort(type.GetIndex());
        if (type.IsGenericType) {
            WriteByte((byte)type.GenericTypeArguments.Count());
            foreach (var parameterType in type.GenericTypeArguments) {
                RecursiveTypeToStream(parameterType);
            }
        }
    }

    public static int RecursiveTypeToSize(Type type) {
        int totalSize = ShortSize();
        if (type.IsGenericType) {
            totalSize += 1; // type argument count
            foreach (var parameterType in type.GenericTypeArguments) {
                totalSize += RecursiveTypeToSize(parameterType);
            }
        }
        return totalSize;
    }

    protected static bool IsBoxingConstructor(ConstructorInfo constructor) {
        var parameters = constructor.GetParameters();
        if (parameters.Count() != 1) {
            return false;
        }
        // We use the boxing constructor to convert lists to SLists, and SList has a param constructor for convenience
        if (parameters[0].GetCustomAttributes<ParamArrayAttribute>().Any()) {
            return false;
        }
        return true;
    }

    public static ISerializable ToSerializable(object property) {
        if (property is ISerializable) {
            return property as ISerializable;
        }
        // Despite the fact that you aren't allowed to put object nulls into the property list
        // they can get in there if, for example, the item with key 40 gets filled in and the object gets serialized
        if (property == null) {
            return new SNull();
        }
        var typeIndex = property.GetType().GetCustomAttributesCached().OfType<TypeIndexAttribute>().Single().Index;
        if (AvailableAssemblyTypes.ContainsKey(typeIndex)) {
            ConstructorInfo constructor = AvailableAssemblyTypes[typeIndex].GetConstructors().Where(IsBoxingConstructor).Single();
            return (ISerializable)constructor.Invoke(new object[] { property });
        }
        throw new ArgumentException("Object of type " + property.GetType().GetNameCached() + " is not supported for serialization.  Serializing " + property.ToString());
    }
    public ISerializable Consume() {
        Type serializedType = FullyResolveType();
        Type originalType = serializedType;
        if (serializedType == null) {
            Debug.Log("Found an empty serialization type.  Assuming this is an empty object.");
            return null;
        }
        ISerializable serializable = (ISerializable)Constructor.Construct(serializedType);
        serializable.DeSerialize(this);
        return serializable;
    }

    public T Consume<T>() where T : ISerializable {
        return (T)Consume();
    }

    public void Write(object property) {
        ISerializable sProp = ToSerializable(property);
        RecursiveTypeToStream(sProp.GetType());
        sProp.Serialize(this);
    }

    public static int SerializationSize(object property) {
        ISerializable sProp = ToSerializable(property);
        int typeSize = RecursiveTypeToSize(sProp.GetType());
        return typeSize + sProp.SerializationSize();
    }
    #endregion

    #region Basic type serialization
    public string ConsumeString() {
        var endIndex = index;
        for (; data[endIndex] != 0; endIndex++) ;
        var byteCount = endIndex - index;
        var s = StringEncoding.GetString(data, index, byteCount);
        index = endIndex + 1;
        return s;
    }

    public byte ConsumeByte() {
        return data[index++];
    }
    public ushort ConsumeShort() {
        ushort value = (ushort)(
            data[index + 0] << 8 |
            data[index + 1]);
        index += 2;
        return value;
    }
    public int ConsumeInt() {
        var value =
            data[index + 0] << 24 |
            data[index + 1] << 16 |
            data[index + 2] << 8 |
            data[index + 3];
        index += 4;
        return value;
    }
    public ulong ConsumeLong() {
        ulong value =
            ((ulong)data[index + 0]) << 56 |
            ((ulong)data[index + 1]) << 48 |
            ((ulong)data[index + 2]) << 40 |
            ((ulong)data[index + 3]) << 32 |
            ((ulong)data[index + 4]) << 24 |
            ((ulong)data[index + 5]) << 16 |
            ((ulong)data[index + 6]) << 8 |
            ((ulong)data[index + 7]);
        index += 8;
        return value;
    }
    public float ConsumeFloat() {
        float value = BitConverter.ToSingle(data, index);
        index += 4;
        return value;
    }

    public void WriteString(string str) {
        var count = StringEncoding.GetBytes(str, 0, str.Length, data, index);
        index += count;
        WriteByte(0);
    }
    public void WriteByte(byte b) {
        data[index++] = b;
    }
    public void WriteShort(ushort s) {
        data[index++] = (byte)((s >> 8) & 0xFF);
        data[index++] = (byte)((s >> 0) & 0xFF);
    }
    public void WriteInt(int i) {
        data[index++] = (byte)((i >> 24) & 0xFF);
        data[index++] = (byte)((i >> 16) & 0xFF);
        data[index++] = (byte)((i >>  8) & 0xFF);
        data[index++] = (byte)((i >>  0) & 0xFF);
    }
    public void WriteLong(ulong i) {
        data[index++] = (byte)((i >> 56) & 0xFF);
        data[index++] = (byte)((i >> 48) & 0xFF);
        data[index++] = (byte)((i >> 40) & 0xFF);
        data[index++] = (byte)((i >> 32) & 0xFF);
        data[index++] = (byte)((i >> 24) & 0xFF);
        data[index++] = (byte)((i >> 16) & 0xFF);
        data[index++] = (byte)((i >>  8) & 0xFF);
        data[index++] = (byte)((i >>  0) & 0xFF);
    }
    public void WriteFloat(float f) {
        BitConverter.GetBytes(f).CopyTo(data, index);
        index += 4;
    }

    // This function is the answer to the question "how much space does this string take up in the stream", which includes the termination byte
    public static int StringSize(string str) {
        int terminatorByteSize = 1;
        return StringEncoding.GetByteCount(str) + terminatorByteSize;
    }
    public static int ShortSize() {
        return 2;
    }
    public static int IntSize() {
        return 4;
    }
    public static int LongSize() {
        return 8;
    }
    public static int FloatSize() {
        return 4;
    }
    #endregion
}