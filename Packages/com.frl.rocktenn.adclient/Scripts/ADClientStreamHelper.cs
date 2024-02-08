using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

using ADDeviceHandle = System.UInt64;
using ADFrameID = System.UInt64;

public static class MarshalUtilities
{
    // Interface to cast a structure into another type
    //   MarshalUtilities.Vector3f -> UnityEngine.Vector3
    public interface Into<T>
    {
        T Into();
    }

    // Interface to deserialize a structure from an IntPtr
    public interface FromPtr
    {
        void FromPtr(ref IntPtr ptr);
    }

    // Reads a primitive out of an IntPtr
    //
    // !!Warning!!
    //    Marshal.SizeOf<bool> == 4.
    //    You probably don't want to call ReadPrimite<bool>
    //    Instead use ReadAndConvert<MarshalUtilities.Byte, bool>() 
    public static T ReadPrimitive<T>(ref IntPtr ptr)
        where T : new()
    {
        var t = new T();
        t = (T)Marshal.PtrToStructure<T>(ptr);
        ptr += Marshal.SizeOf<T>();
        return t;
    }

    // Reads a struct/class out of an IntPtr
    public static T Read<T>(ref IntPtr ptr)
        where T : FromPtr, new()
    {
        var temp = new T();
        temp.FromPtr(ref ptr);
        return temp;
    }

    // Reads a struct/class of type T then converts into type U via Into
    public static U ReadAndConvert<T, U>(ref IntPtr ptr)
        where T : FromPtr, Into<U>, new()
    {
        return Read<T>(ref ptr).Into();
    }

    public struct Byte : MarshalUtilities.FromPtr, Into<byte>, Into<bool>
    {
        public byte b;

        public void FromPtr(ref IntPtr ptr)
        {
            b = ReadPrimitive<byte>(ref ptr);
        }

        byte Into<byte>.Into()
        {
            return b;
        }

        bool Into<bool>.Into()
        {
            return b != 0;
        }
    }

    public struct float1 : MarshalUtilities.FromPtr, Into<float>
    {
        public float x;

        public void FromPtr(ref IntPtr ptr)
        {
            x = ReadPrimitive<float>(ref ptr);
        }

        float Into<float>.Into()
        {
            return x;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct float2 : MarshalUtilities.FromPtr, Into<Vector2>
    {
        public float x, y;

        public void FromPtr(ref IntPtr ptr)
        {
            x = ReadPrimitive<float>(ref ptr);
            y = ReadPrimitive<float>(ref ptr);
        }

        Vector2 Into<Vector2>.Into()
        {
            return new Vector2(x, y);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct float3 : MarshalUtilities.FromPtr, Into<Vector3>
    {
        public float x, y, z;

        public void FromPtr(ref IntPtr ptr)
        {
            x = ReadPrimitive<float>(ref ptr);
            y = ReadPrimitive<float>(ref ptr);
            z = ReadPrimitive<float>(ref ptr);
        }

        Vector3 Into<Vector3>.Into()
        {
            return new Vector3(x, y, z);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct float4 : MarshalUtilities.FromPtr, Into<Vector4>, Into<Quaternion>
    {
        public float x, y, z, w;

        public void FromPtr(ref IntPtr ptr)
        {
            x = ReadPrimitive<float>(ref ptr);
            y = ReadPrimitive<float>(ref ptr);
            z = ReadPrimitive<float>(ref ptr);
            w = ReadPrimitive<float>(ref ptr);
        }

        Vector4 Into<Vector4>.Into()
        {
            return new Vector4(x, y, z, w);
        }

        Quaternion Into<Quaternion>.Into()
        {
            return new Quaternion(x, y, z, w);
        }
    }

    [System.Serializable]
    public struct RawData : MarshalUtilities.FromPtr, MarshalUtilities.Into<RawData>
    {
        public byte[] data;

        public void FromPtr(ref IntPtr ptr)
        {
            int numBytesInPayload = MarshalUtilities.ReadPrimitive<int>(ref ptr);
            data = new byte[numBytesInPayload];
            int startIndex = 0;
            Marshal.Copy(ptr, data, startIndex, numBytesInPayload);
            ptr += numBytesInPayload;
        }

        public RawData Into()
        {
            return this;
        }
    }
}

[System.Serializable]
public class ADFrame<T>
{
    public ADFrameID frameId { get; private set; }
    public UInt64 deviceTimestamp { get; private set; }
    public T[] data { get; private set; }

    public void ReadAndConvert<From>(ref IntPtr ptr)
        where From : MarshalUtilities.FromPtr, MarshalUtilities.Into<T>, new()
    {
        frameId = MarshalUtilities.ReadPrimitive<ADFrameID>(ref ptr);
        deviceTimestamp = MarshalUtilities.ReadPrimitive<UInt64>(ref ptr);
        var numEntries = MarshalUtilities.ReadPrimitive<UInt16>(ref ptr);

        if (data == null || data.Length != numEntries)
            data = new T[numEntries];

        for (int i = 0; i < numEntries; ++i)
        {
            data[i] = MarshalUtilities.ReadAndConvert<From, T>(ref ptr);
        }
    }
}

// Utility to access a time series data; (ADFrameID, data)
//
// Example: ADClientStream<MarshalUtilities.Vector3f, UnityEngine.Vector3>
//
// T = source C++ type; MarshalUtilities.Vector3
// U = output Unity type; UnityEngine.Vector3
public class ADClientStream<T, U>
    where T : MarshalUtilities.FromPtr, MarshalUtilities.Into<U>, new()
    where U : new()
{
    const int BUFFER_SIZE = 1048576; // 1 megabyte
    static IntPtr _buffer = Marshal.AllocHGlobal(BUFFER_SIZE);

    // Private fields
    ADDeviceHandle _device;
    int _streamId;
    ADFrameID _lastSeen;

    // Methods
    public void Init(ADDeviceHandle device, int streamId)
    {
        _device = device;
        _streamId = streamId;
    }

    public ADFrame<U> GetLatest()
    {
        var adc = ADClientSingleton.c_handle;

        // Get single frame (raw)
        int numFrames = ADClientAPI.GetLatest(adc, _device, _streamId, _buffer, BUFFER_SIZE);
        if (numFrames == 0)
            return null;

        // Parse buffer
        IntPtr ptr = _buffer;
        ADFrame<U> frame = new ADFrame<U>();
        frame.ReadAndConvert<T>(ref ptr);

        return frame;
    }

    public ADFrame<U> GetAt(ADFrameID pos)
    {
        var adc = ADClientSingleton.c_handle;

        // Get single frame (raw)
        int numFrames = ADClientAPI.GetAt(adc, _device, pos, _streamId, _buffer, BUFFER_SIZE);
        if (numFrames == 0)
            return null;

        // Parse buffer
        IntPtr ptr = _buffer;
        ADFrame<U> frame = new ADFrame<U>();
        frame.ReadAndConvert<T>(ref ptr);

        return frame;
    }

    public void GetBetweenExcludingBegin(ADFrameID begin, ADFrameID end, ref List<ADFrame<U>> result)
    {
        var adc = ADClientSingleton.c_handle;

        // Get all new frames
        int numFrames = ADClientAPI.GetBetweenExcludingBegin(adc, _device, _streamId, begin, end, _buffer, BUFFER_SIZE);
        ResizeList(ref result, numFrames);

        // No frames, return
        if (numFrames <= 0)
            return;

        // Read each frame
        IntPtr ptr = _buffer;
        for (int i = 0; i < numFrames; ++i)
        {
            result[i].ReadAndConvert<T>(ref ptr);
        }

        // Update last seen
        _lastSeen = result[result.Count - 1].frameId;
    }

    public void GetBetweenIncludingBegin(ADFrameID begin, ADFrameID end, ref List<ADFrame<U>> result)
    {
        GetBetweenExcludingBegin(begin - 1, end, ref result);
    }

    public void GetUnseen(ref List<ADFrame<U>> result)
    {
        var adc = ADClientSingleton.c_handle;

        // Get frames
        var latest = ADClientAPI.GetLatestFrameID(adc, _device, _streamId);
        GetBetweenExcludingBegin(_lastSeen, latest, ref result);

        // Update lastSeen timestamp
        if (result.Count > 0)
        {
            _lastSeen = result[result.Count - 1].frameId;
        }
    }

    void ResizeList(ref List<ADFrame<U>> list, int desired_count)
    {
        int current_count = list.Count;
        if (desired_count < current_count)
        {
            list.RemoveRange(desired_count, current_count - desired_count);
        }
        else if (desired_count > current_count)
        {
            for (int i = 0; i < (desired_count - current_count); ++i)
                list.Add(new ADFrame<U>());
        }
    }
}
