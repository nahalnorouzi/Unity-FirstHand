using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;


using PluginManagerHandle = System.IntPtr;
using ADClientHandle = System.IntPtr;
using ADDeviceHandle = System.UInt64;
using ADFrameID = System.UInt64;
using VtfxRuntimeContextHandle = System.IntPtr;


[Osiris.PluginAttr("ADClientPlugin", editorPath: "../Packages/com.frl.rocktenn.adclient/Plugins")]
public class ADClientAPI {
    // Constants
    public const ADDeviceHandle kInvalidDeviceHandle = System.UInt64.MaxValue;


    // Enums
    public enum DeviceHandedness { NotApplicable = -1, Left, Right };
    // Must match InputDataStreamType in ADCommon.h
    public enum InputStreams
    {
        Rotation = 0,
        IMU = 1,
        TouchPosition = 3,
        Button = 4,
        Accelerometer = 5,
        Azalea = 6,
        CapacitancePixels = 7,
        Text = 8,
        TouchCapacitance = 9,
        Magnolia = 10,
        EMG = 11,
        IMU_Accelerometer = 12,
        IMU_Gyroscope = 13,
        IMU_Magnetometer = 14,
        IMU_Temperature = 15,
        BatteryStatus = 16,
        WirelessStatus = 17,
        BendSensor = 18, //not implemented in C# yet
        ImpSensor = 19, //not implemented in C# yet
        Raw = 20,
    }

    // Structs
    public class DeviceInfo {
        ADDeviceHandle _deviceHandle;
        string _name;
        string _model;
        DeviceHandedness _handedness;
        VtfxRuntimeContextHandle _vtfxRuntimeContextHandle;

        public DeviceInfo() {
            this._deviceHandle = ADClientAPI.kInvalidDeviceHandle;
            this._name = "Unknown";
            this._model = "Unknown";
            this._handedness = DeviceHandedness.NotApplicable;
            this._vtfxRuntimeContextHandle = System.IntPtr.Zero;
        }

        public DeviceInfo(ADDeviceHandle handle, string name, string model, DeviceHandedness handedness, VtfxRuntimeContextHandle vtfx) {
            this._deviceHandle = handle;
            this._name = name;
            this._model = model;
            this._handedness = handedness;
            this._vtfxRuntimeContextHandle = vtfx;
        }

        public ADDeviceHandle deviceHandle { get { return _deviceHandle; } }
        public string name { get { return _name; } }
        public string model { get { return _model; } }
        public DeviceHandedness handedness { get { return _handedness; } }
        public VtfxRuntimeContextHandle vtfxHandle { get { return _vtfxRuntimeContextHandle; } }
    }



    // C-API
    [Osiris.PluginFunctionAttr]
    public static ADClient_Initialize Initialize = null;
    public delegate ADClientHandle ADClient_Initialize(PluginManagerHandle pluginManager);

    [Osiris.PluginFunctionAttr]
    public static ADClient_Connect Connect = null;
    public delegate void ADClient_Connect(ADClientHandle client, [MarshalAs(UnmanagedType.LPStr)] string address);

    [Osiris.PluginFunctionAttr]
    public static ADClient_Disconnect Disconnect = null;
    public delegate void ADClient_Disconnect(ADClientHandle client);

    [Osiris.PluginFunctionAttr]
    public static ADClient_FindDeviceByName FindDeviceByName = null;
    public delegate ADDeviceHandle ADClient_FindDeviceByName(ADClientHandle client, [MarshalAs(UnmanagedType.LPStr)] string deviceName);

    [Osiris.PluginFunctionAttr]
    public static ADClient_GetNumDeviceInfos GetNumDeviceInfos = null;
    public delegate int ADClient_GetNumDeviceInfos(ADClientHandle client);

    [Osiris.PluginFunctionAttr(funcName = "ADClient_GetDeviceInfoByIndex")]
    public static ADClient_GetDeviceInfoByIndex GetDeviceInfoByIndex_C = null;
    public delegate bool ADClient_GetDeviceInfoByIndex(ADClientHandle client, int infoIndex, out ADDeviceHandle deviceHandle, byte[] name, int nameLen, byte[] model, int modelLen, out int handedness, out VtfxRuntimeContextHandle vtfx);
    public static DeviceInfo GetDeviceInfoByIndex(ADClientHandle client, int infoIndex) {
        const int BUFFER_SIZE = 256;

        ADDeviceHandle handle;
        byte[] name = new byte[BUFFER_SIZE];
        byte[] model = new byte[BUFFER_SIZE];
        int handedness = 0;
        VtfxRuntimeContextHandle vtfx = System.IntPtr.Zero;

        for (int i = 0; i < BUFFER_SIZE; ++i) {
            name[i] = (byte)'\0';
            model[i] = (byte)'\0';
        }

        bool success = GetDeviceInfoByIndex_C(client, infoIndex, out handle, name, name.Length, model, model.Length, out handedness, out vtfx);
        if (!success)
            return null;

        string nameStr = System.Text.Encoding.UTF8.GetString(name).Trim('\0');
        string modelStr = System.Text.Encoding.UTF8.GetString(model).Trim('\0');

        return new DeviceInfo(handle, nameStr, modelStr, (DeviceHandedness)handedness, vtfx);
    }

    [Osiris.PluginFunctionAttr]
    public static ADClient_Now Now = null;
    public delegate ADFrameID ADClient_Now(ADClientHandle client);

    [Osiris.PluginFunctionAttr]
    public static ADClient_GetLatestFrameID GetLatestFrameID = null;
    public delegate ADFrameID ADClient_GetLatestFrameID(ADClientHandle client, ADDeviceHandle device, int streamId);

    [Osiris.PluginFunctionAttr]
    public static ADClient_GetLatest GetLatest = null;
    public delegate int ADClient_GetLatest(ADClientHandle client, ADDeviceHandle device, int streamId, System.IntPtr buffer, int bufferSizeInBytes);

    [Osiris.PluginFunctionAttr]
    public static ADClient_GetAt GetAt = null;
    public delegate int ADClient_GetAt(ADClientHandle client, ADDeviceHandle device, ADFrameID pos, int streamId, System.IntPtr buffer, int bufferSizeInBytes);

    [Osiris.PluginFunctionAttr]
    public static ADClient_GetBetweenExcludingBegin GetBetweenExcludingBegin = null;
    public delegate int ADClient_GetBetweenExcludingBegin(ADClientHandle client, ADDeviceHandle device, int streamId, ADFrameID begin, ADFrameID end, System.IntPtr buffer, int bufferSizeInBytes);

    [Osiris.PluginFunctionAttr]
    public static ADClient_GetBetweenIncludingBegin GetBetweenIncludingBegin;
    public delegate int ADClient_GetBetweenIncludingBegin(ADClientHandle client, ADDeviceHandle device, int streamId, ADFrameID begin, ADFrameID end, System.IntPtr buffer, int bufferSizeInBytes);

    [Osiris.PluginFunctionAttr]
    public static ADClient_CountButtonPresses CountButtonPresses = null;
    public delegate int ADClient_CountButtonPresses(ADClientHandle client, ADDeviceHandle device, ADFrameID begin, ADFrameID end, byte[] buffer, int bufferSizeInBytes);

    [Osiris.PluginFunctionAttr]
    public static ADClient_CountButtonReleases CountButtonReleases = null;
    public delegate int ADClient_CountButtonReleases(ADClientHandle client, ADDeviceHandle device, ADFrameID begin, ADFrameID end, byte[] buffer, int bufferSizeInBytes);

    [Osiris.PluginFunctionAttr]
    public static ADClient_SetJammerStates SetJammerStates = null;
    public delegate void ADClient_SetJammerStates(ADClientHandle client, ADDeviceHandle device, int[] buffer, int bufferSize);

    [Osiris.PluginFunctionAttr]
    public static ADClient_SetLEDStates SetLEDStates = null;
    public delegate void ADClient_SetLEDStates(ADClientHandle client, ADDeviceHandle device, uint[] buffer, int bufferSize);

    [Osiris.PluginFunctionAttr]
    public static ADClient_SetAzaleaModes SetAzaleaModes = null;
    public delegate void ADClient_SetAzaleaModes(ADClientHandle client, ADDeviceHandle device, int[] buffer, int bufferSize);

    [Osiris.PluginFunctionAttr]
    public static ADClient_SendCommand SendCommand = null;
    public delegate void ADClient_SendCommand(ADClientHandle client, ADDeviceHandle device, byte[] buffer, int bufferSize);

    // DEPRECATED - Old C API
    [Osiris.PluginFunctionAttr]
    public static ADClient_UpdateDeviceState UpdateDeviceState = null;
    public delegate void ADClient_UpdateDeviceState(ADClientHandle client, ADDeviceHandle device);

    [Osiris.PluginFunctionAttr(funcName = "ADClient_GetTouchIDs")]
    static ADClient_GetTouchIDs ADClient_GetTouchIDs_C = null;
    delegate int ADClient_GetTouchIDs(ADClientHandle client, ADDeviceHandle device, uint[] outIDs, int numIDs);
    public static uint[] GetTouchIDs(ADClientHandle client, ADDeviceHandle device) {
        uint[] ids = new uint[5];

        int numResults = ADClient_GetTouchIDs_C(client, device, ids, ids.Length);

        uint[] result = new uint[numResults];
        for (int i = 0; i < numResults; ++i)
            result[i] = ids[i];

        return result;
    }

    [Osiris.PluginFunctionAttr(funcName = "ADClient_GetTouchPosition")]
    static ADClient_GetTouchPosition ADClient_GetTouchPosition_C = null;
    delegate bool ADClient_GetTouchPosition(ADClientHandle client, ADDeviceHandle device, uint touchID, out float x, out float y, out float z);
    public static bool GetTouchPosition(ADClientHandle client, ADDeviceHandle device, uint touchID, out Vector3 position) {
        float x, y, z = 0.0f;

        bool success = ADClient_GetTouchPosition_C(client, device, touchID, out x, out y, out z);
        position = success ? new Vector3(x, y, z) : Vector3.zero;

        return success;
    }

    [Osiris.PluginFunctionAttr]
    public static ADClient_IsButtonDown IsButtonDown = null;
    public delegate bool ADClient_IsButtonDown(ADClientHandle client, ADDeviceHandle device, int buttonIndex);

    [Osiris.PluginFunctionAttr]
    public static ADClient_WasButtonPressed WasButtonPressed = null;
    public delegate bool ADClient_WasButtonPressed(ADClientHandle client, ADDeviceHandle device, int buttonIndex);

    [Osiris.PluginFunctionAttr]
    public static ADClient_WasButtonReleased WasButtonReleased = null;
    public delegate bool ADClient_WasButtonReleased(ADClientHandle client, ADDeviceHandle device, int buttonIndex);

    [Osiris.PluginFunctionAttr(funcName = "ADClient_LatestRotation")]
    static ADClient_LatestRotation ADClient_LatestRotation_C = null;
    delegate bool ADClient_LatestRotation(ADClientHandle client, ADDeviceHandle device, int index, out float x, out float y, out float z, out float w);
    public static bool LatestRotation(ADClientHandle client, ADDeviceHandle device, int index, out Quaternion q) {
        float x, y, z, w = 0.0f;

        bool success = ADClient_LatestRotation_C(client, device, index, out x, out y, out z, out w);
        q = success ? new Quaternion(x, y, z, w) : Quaternion.identity;

        return success;
    }

    [Osiris.PluginFunctionAttr]
    public static ADClient_SetLEDState SetLEDState = null;
    public delegate bool ADClient_SetLEDState(ADClientHandle client, ADDeviceHandle device, int ledIndex, byte r, byte g, byte b, byte a);

    [Osiris.PluginFunctionAttr]
    public static ADClient_RequestPollingRate RequestPollingRate = null;
    public delegate bool ADClient_RequestPollingRate(ADClientHandle client, float expectedUpdateRateHz);
}
