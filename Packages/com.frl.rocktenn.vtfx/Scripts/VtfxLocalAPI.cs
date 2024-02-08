using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using DeviceHandle = System.IntPtr;
using VtfxRuntimeContextHandle = System.IntPtr;
using VtfxPluginHandle = System.IntPtr;
using EffectHandle = System.IntPtr;
using NodeHandle = System.IntPtr;
using ResourceLoaderHandle = System.IntPtr;
using TunableHandle = System.IntPtr;
using PluginManagerHandle = System.IntPtr;
using CStr = System.IntPtr;

[Osiris.PluginAttr("VtfxRuntime", editorPath: "../Packages/com.frl.rocktenn.vtfx/Plugins")]
public class VtfxLocalPluginAPI
{
    // Constants
    static public VtfxRuntimeContextHandle kInvalidHandle = System.IntPtr.Zero;

    // Enums
    public enum TunableType
    {
        Float,
        Int,
        X_Count,
    };

    public enum OutputType
    {
        XAudio2,
        GalbiLite_Sundial,
        Test,
    }

    public enum BufferType : int
    {
        PCM,
        Intensity,
    };

    public enum DeviceHandedness : int
    {
        NotApplicable = -1,
        Left,
        Right
    }

    public enum LogLevel : int
    {
        Verbose,
        Info,
        Warning,
        Error
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct FillResult
    {
        public int _numActiveEffects;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct VtfxSpanFillResult
    {
        public int num_filled;
        public int num_needed;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct VtfxDeviceInfo
    {
        public int _numChannels;
        [MarshalAs(UnmanagedType.U1)] public bool _signedSample;
        [MarshalAs(UnmanagedType.U1)] public bool _floatSample;
        public BufferType _bufferType;
        public int _bytesPerSample;
        public int _samplesPerChannelPerSec;
        public int _samplesPerChannelPerBuffer;
        public DeviceHandedness _handedness;
        public DeviceHandle _parentDeviceHandle;
        public int _parentDeviceChannelOffset;
    }

    // DParams
    public static string DPARAM_ABS_INPUT = "Input";

    public static string DPARAM_ADSR_INITIAL_DELAY = "InitialDelay";
    public static string DPARAM_ADSR_ATTACK_DURATION = "AttackDuration";
    public static string DPARAM_ADSR_ATTACK_AMPLITUDE = "AttackAmplitude";
    public static string DPARAM_ADSR_DECAY_DURATION = "DecayDuration";
    public static string DPARAM_ADSR_SUSTAIN_DURATION = "SustainDuration";
    public static string DPARAM_ADSR_SUSTAIN_AMPLITUDE = "SustainAmplitude";
    public static string DPARAM_ADSR_RELEASE_DURATION = "ReleaseDuration";
    public static string DPARAM_ADSR_FINAL_DELAY = "FinalDelay";

    public static string DPARAM_BIQUAD_FILTER_INPUT = "Input";
    public static string DPARAM_BIQUAD_FILTER_CENTER_FREQUENCY = "CenterFrequency";
    public static string DPARAM_BIQUAD_FILTER_Q_FACTOR = "QFactor";
    public static string DPARAM_BIQUAD_FILTER_GAIN = "Gain";

    public static string DPARAM_CHANNEL_BREAKOUT_INPUT = "Input";
    public static string DPARAM_CLAMP_INPUT = "Input";
    public static string DPARAM_CLAMP_MAX = "Max";
    public static string DPARAM_CLAMP_MIN = "Min";

    public static string DPARAM_CROSSFADE_POSITION = "Position";
    public static string DPARAM_CROSSFADE_INPUT_ARRAY = "Inputs";

    public static string DPARAM_CURVE_T_VALUE_OVERRIDE = "TValueOverride";

    public static string DPARAM_DELAY_INPUT = "Input";
    public static string DPARAM_DELAY_DURATION = "Duration";

    public static string DPARAM_EXPONENTIAL_DECAY_BASE = "Base";
    public static string DPARAM_EXPONENTIAL_DECAY_RATE = "Rate";
    public static string DPARAM_EXPONENTIAL_DECAY_MIN_AMPLITUDE = "MinAmplitude";
    public static string DPARAM_EXPONENTIAL_DECAY_INITIAL_DECAY = "InitialDelay";
    public static string DPARAM_EXPONENTIAL_DECAY_DECAY_DURATION = "DecayDuration";

    public static string DPARAM_EXTRACT_CHANNEL_INPUT = "Input";
    public static string DPARAM_EXTRACT_CHANNEL_CHANNEL_INDEX = "ChannelIndex";

    public static string DPARAM_FILTER_INPUT = "Input";
    public static string DPARAM_FILTER_CUTOFF = "Cutoff";

    public static string DPARAM_IMPACT_FORCE_AMPLITUDE = "Amplitude";
    public static string DPARAM_IMPACT_FORCE_DURATION = "Duration";

    public static string DPARAM_KEEP_EFFECT_ALIVE_INPUT = "Input";
    public static string DPARAM_KEEP_EFFECT_ALIVE_DURATION = "Duration";

    public static string DPARAM_MAKE_CONST_INPUT = "Input";

    public static string DPARAM_MATH_OP_INPUT_ARRAY = "Inputs";

    public static string DPARAM_MATRIX_INPUT = "Input";
    public static string DPARAM_MATRIX_ELEMENTS_ARRAY = "Elements";

    public static string DPARAM_MODAL_RESONATOR_BANK_INPUT = "Input";
    public static string DPARAM_MODAL_RESONATOR_BANK_ACTIVEMODES = "ActiveModes";
    public static string DPARAM_MODAL_RESONATOR_BANK_FIRST_FREQUENCY = "FirstFrequency";
    public static string DPARAM_MODAL_RESONATOR_BANK_FIRST_DECAY = "FirstDecay";
    public static string DPARAM_MODAL_RESONATOR_BANK_FIRST_AMPLITUDE = "FirstAmplitude";
    public static string DPARAM_MODAL_RESONATOR_BANK_MODEDATA_ARRAY = "ModeData";

    public static string DPARAM_OUTPUT_INPUT = "Input";

    public static string DPARAM_PITCH_ADJUST_INPUT = "Input";
    public static string DPARAM_PITCH_ADJUST_PITCH_MULT = "PitchMult";

    public static string DPARAM_POLYNOMIAL_DECAY_DECAY_DURATION = "DecayDuration";
    public static string DPARAM_POLYNOMIAL_DECAY_POWER = "Power";
    public static string DPARAM_POLYNOMIAL_DECAY_INITIAL_DELAY = "InitialDelay";

    public static string DPARAM_PYTHON_INPUT_ARRAY = "Inputs";

    public static string DPARAM_RANDOM_TYPE = "Type";
    public static string DPARAM_RANDOM_PICK_RATE = "PickRate";
    public static string DPARAM_RANDOM_INTEGER_VALUES = "IntegerValues";
    public static string DPARAM_RANDOM_MIN = "Min";
    public static string DPARAM_RANDOM_MAX = "Max";
    public static string DPARAM_RANDOM_MEAN = "Mean";
    public static string DPARAM_RANDOM_STD_DEVIATION = "StdDeviation";

    public static string DPARAM_RAW_DATA_INDEX = "Index";

    public static string DPARAM_REMAP_INPUT = "Input";
    public static string DPARAM_REMAP_SOURCE_MIN = "SourceMin";
    public static string DPARAM_REMAP_SOURCE_MAX = "SourceMax";
    public static string DPARAM_REMAP_TARGET_MIN = "TargetMin";
    public static string DPARAM_REMAP_TARGET_MAX = "TargetMax";

    public static string DPARAM_REPLICATE_CHANNEL_INPUT = "Input";
    public static string DPARAM_REPLICATE_CHANNEL_OUT_CHANNEL_COUNT = "OutChannelCount";

    public static string DPARAM_SET_CHANNEL_INPUT = "Input";
    public static string DPARAM_SET_CHANNEL_OUTPUT_INDEX = "OutputIndex";

    public static string DPARAM_SOFT_TRAPEZOID_INITIAL_DELAY = "InitialDelay";
    public static string DPARAM_SOFT_TRAPEZOID_RAMP_UP_DURATION = "RampUpDuration";
    public static string DPARAM_SOFT_TRAPEZOID_AMPLITUDE = "Amplitude";
    public static string DPARAM_SOFT_TRAPEZOID_HOLD_DURATION = "HoldDuration";
    public static string DPARAM_SOFT_TRAPEZOID_RAMP_DOWN_DURATION = "RampDownDuration";
    public static string DPARAM_SOFT_TRAPEZOID_FINAL_DELAY = "FinalDelay";

    public static string DPARAM_SPATIALIZE_1D_INPUT = "Input";
    public static string DPARAM_SPATIALIZE_1D_FALLOFF_START = "FalloffStart";
    public static string DPARAM_SPATIALIZE_1D_FALLOFF_SIZE = "FalloffSize";
    public static string DPARAM_SPATIALIZE_1D_POSITION = "Position";

    public static string DPARAM_SPATIALIZE_2D_INPUT = "Input";
    public static string DPARAM_SPATIALIZE_2D_FALLOFF_START = "FalloffStart";
    public static string DPARAM_SPATIALIZE_2D_FALLOFF_SIZE = "FalloffSize";
    public static string DPARAM_SPATIALIZE_2D_POSITION_X = "PositionX";
    public static string DPARAM_SPATIALIZE_2D_POSITION_Y = "PositionY";

    public static string DPARAM_SPATIALIZE_3D_INPUT = "Input";
    public static string DPARAM_SPATIALIZE_3D_FALLOFF_START = "FalloffStart";
    public static string DPARAM_SPATIALIZE_3D_FALLOFF_SIZE = "FalloffSize";
    public static string DPARAM_SPATIALIZE_3D_POSITION_X = "PositionX";
    public static string DPARAM_SPATIALIZE_3D_POSITION_Y = "PositionY";
    public static string DPARAM_SPATIALIZE_3D_POSITION_Z = "PositionZ";

    public static string DPARAM_SQUARE_PULSE_INITIAL_DELAY = "InitialDelay";
    public static string DPARAM_SQUARE_PULSE_FINAL_DELAY = "FinalDelay";
    public static string DPARAM_SQUARE_PULSE_AMPLITUDE = "Amplitude";
    public static string DPARAM_SQUARE_PULSE_HOLD_DURATION = "HoldDuration";

    public static string DPARAM_STACK_CHANNELS_INPUTS_ARRAY = "Inputs";

    public static string DPARAM_STOP_IF_QUIET_INPUT = "Input";
    public static string DPARAM_STOP_IF_QUIET_THRESHOLD = "Threshold";
    public static string DPARAM_STOP_IF_QUIET_DURATION = "Duration";

    public static string DPARAM_TRAPEZOID_INITAL_DELAY = "InitialDelay";
    public static string DPARAM_TRAPEZOID_RAMP_UP_DURATION = "RampUpDuration";
    public static string DPARAM_TRAPEZOID_AMPLITUDE = "Amplitude";
    public static string DPARAM_TRAPEZOID_HOLD_DURATION = "HoldDuration";
    public static string DPARAM_TRAPEZOID_RAMP_DOWN_DURATION = "RampDownDuration";
    public static string DPARAM_TRAPEZOID_FINAL_DELAY = "FinalDelay";

    public static string DPARAM_VOLUME_ADJUST_INPUT = "Input";
    public static string DPARAM_VOLUME_ADJUST_CHANNELS_ARRAY = "Channels";

    public static string DPARAM_WAVE_GEN_FREQUENCY = "Frequency";
    public static string DPARAM_WAVE_GEN_AMPLITUDE = "Amplitude";
    public static string DPARAM_WAVE_GEN_BIAS = "Bias";
    public static string DPARAM_WAVE_GEN_PHASE = "Phase";

    // CParams
    public static string VTFX_CPARAM_ADSR_STOP_EFFECT_ON_FINISHED = "StopEffectOnFinished";
    public static string VTFX_CPARAM_ADSR_LOOP = "Loop";

    public static string VTFX_CPARAM_BIQUAD_FILTER_TYPE_ENUM = "FilterType";

    public static string VTFX_CPARAM_CLAMP_ENABLE_MIN = "EnableMin";
    public static string VTFX_CPARAM_CLAMP_ENABLE_MAX = "EnableMax";

    public static string VTFX_CPARAM_COMMENT_TEXT = "Text";

    public static string VTFX_CPARAM_CROSSFADE_NUM_INPUTS = "NumInputs";
    public static string VTFX_CPARAM_CROSSFADE_IS_POSITION_NORMALIZED = "IsPositionNormalized";

    public static string VTFX_CPARAM_CURVE_BEGIN_AT_FIRST_VALUE_WHEN_STARTING = "BeginAtFirstValueWhenStarting";
    public static string VTFX_CPARAM_CURVE_RETAIN_FINAL_VALUE_ON_FINISHED = "RetainFinalValueOnFinished";
    public static string VTFX_CPARAM_CURVE_STOP_EFFECT_ON_FINISHED = "StopEffectOnFinished";
    public static string VTFX_CPARAM_CURVE_LOOP = "Loop";
    public static string VTFX_CPARAM_CURVE_ENABLE_T_VALUE_OVERRIDE = "EnableTValueOverride";
    public static string VTFX_CPARAM_CURVE_SNAP_TO_T = "SnapToT";
    public static string VTFX_CPARAM_CURVE_SNAP_TO_V = "SnapToV";

    public static string VTFX_CPARAM_EXPONENTIAL_DECAY_STOP_EFFECT_ON_FINISHED = "StopEffectOnFinished";
    public static string VTFX_CPARAM_EXPONENTIAL_DECAY_LOOP = "Loop";
    public static string VTFX_CPARAM_EXPONENTIAL_DECAY_DURATION_TYPE_ENUM = "DurationType";

    public static string VTFX_CPARAM_IMPACT_FORCE_STOP_EFFECT_ON_FINISHED = "StopEffectOnFinished";
    public static string VTFX_CPARAM_IMPACT_FORCE_LOOP = "Loop";

    public static string VTFX_CPARAM_MAKE_CONST_TRUNCATE_TO_INT = "TruncateToInt";

    public static string VTFX_CPARAM_MATH_OP_NUM_INPUTS = "NumInputs";

    public static string VTFX_CPARAM_MATRIX_NUM_ROWS = "NumRows";
    public static string VTFX_CPARAM_MATRIX_NUM_COLS = "NumCols";
    public static string VTFX_CPARAM_MATRIX_SHOW_MATRIX_CELL_INPUTS = "ShowMatrixCellInputs";

    public static string VTFX_CPARAM_MUPARSER_EQUATION = "Equation";

    public static string VTFX_CPARAM_NOISE_TYPE_ENUM = "NoiseType";

    public static string VTFX_CPARAM_OUTPUT_NAME_MUST_CONTAIN_STRINGS = "NameMustContainStrings";
    public static string VTFX_CPARAM_OUTPUT_HANDEDNESS_ENUM = "Handedness";
    public static string VTFX_CPARAM_OUTPUT_MUST_BE_AUDIO = "MustBeAudio";
    public static string VTFX_CPARAM_OUTPUT_MUST_NOT_BE_AUDIO = "MustNotBeAudio";

    public static string VTFX_CPARAM_POLYNOMIAL_DECAY_STOP_EFFECT_ON_FINISHED = "StopEffectOnFinished";
    public static string VTFX_CPARAM_POLYNOMIAL_DECAY_LOOP = "Loop";

    public static string VTFX_CPARAM_PYTHON_COMMENT = "Comment";
    public static string VTFX_CPARAM_PYTHON_NUM_INPUTS = "NumInputs";

    public static string VTFX_CPARAM_STREAMING_SRC_BUFFER = "SrcBuffer";
    public static string VTFX_CPARAM_STREAMING_SAMPLES_PER_SEC = "SamplesPerSec";
    public static string VTFX_CPARAM_STREAMING_WARMUP_SAMPLES = "WarmupSamples";

    public static string VTFX_CPARAM_REPLICATE_CHANNEL_USE_NUM_DEVICE_CHANNELS = "UseNumDeviceChannels";

    public static string VTFX_CPARAM_SOFT_TRAPEZOID_STOP_EFFECT_ON_FINISHED = "StopEffectOnFinished";
    public static string VTFX_CPARAM_SOFT_TRAPEZOID_LOOP = "Loop";

    public static string VTFX_CPARAM_SPATIALIZE_1D_LISTENER_MODE_ENUM = "ListenerMode";
    public static string VTFX_CPARAM_SPATIALIZE_1D_USE_NUM_DEVICE_CHANNELS = "UseNumDeviceChannels";
    public static string VTFX_CPARAM_SPATIALIZE_1D_EXPLICIT_NUM_OUTPUT_CHANNELS = "ExplicitNumOutputChannels";

    public static string VTFX_CPARAM_SPATIALIZE_3D_USE_OVR_AUDIO = "UseOvrAudio";
    public static string VTFX_CPARAM_SPATIALIZE_3D_NARROW_BAND = "NarrowBand";
    public static string VTFX_CPARAM_SPATIALIZE_3D_WIDE_BAND = "WideBand";
    public static string VTFX_CPARAM_SPATIALIZE_3D_DIRECT_TIME_OF_ARRIVAL = "DirectTimeOfArrival";
    public static string VTFX_CPARAM_SPATIALIZE_3D_DISABLE_REFLECTIONS = "DisableReflections";
    public static string VTFX_CPARAM_SPATIALIZE_3D_RANGE_MIN = "RangeMin";
    public static string VTFX_CPARAM_SPATIALIZE_3D_RANGE_MAX = "RangeMax";

    public static string VTFX_CPARAM_SQUARE_PULSE_STOP_EFFECT_ON_FINISHED = "StopEffectOnFinished";
    public static string VTFX_CPARAM_SQUARE_PULSE_LOOP = "Loop";

    public static string VTFX_CPARAM_STACK_CHANNELS_NUM_INPUTS = "NumInputs";

    public static string VTFX_CPARAM_TRAPEZOID_STOP_EFFECT_ON_FINISHED = "StopEffectOnFinished";
    public static string VTFX_CPARAM_TRAPEZOID_LOOP = "Loop";

    public static string VTFX_CPARAM_VOLUME_ADJUST_ENABLE_PER_CHANNEL_VOLUMES = "EnablePerChannelVolumes";
    public static string VTFX_CPARAM_VOLUME_ADJUST_NUM_CHANNELS = "NumChannels";

    public static string VTFX_CPARAM_WAVE_GEN_WAVE_TYPE_ENUM = "WaveType";
    public static string VTFX_CPARAM_WAVE_GEN_NOTE = "Note";

    // Device filter tags
    public static string VTFX_TAG_HAPTICS = "Haptics";
    public static string VTFX_TAG_AUDIO = "Audio";
    public static string VTFX_TAG_LEFT = "Left";
    public static string VTFX_TAG_RIGHT = "Right";

    // Utilities
    static string StringFromCStr(CStr cstr) { return Marshal.PtrToStringAnsi(cstr); }

    [Osiris.PluginFunctionAttr]
    public static VTFX_CreateRuntimeContext CreateRuntimeContext = null;
    public delegate VtfxRuntimeContextHandle VTFX_CreateRuntimeContext(PluginManagerHandle pluginManager);

    [Osiris.PluginFunctionAttr]
    public static VTFX_DestroyRuntimeContext DestroyRuntimeContext = null;
    public delegate void VTFX_DestroyRuntimeContext(VtfxRuntimeContextHandle vtfxRCH);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Initialize Initialize = null;
    public delegate void VTFX_Initialize(VtfxRuntimeContextHandle vtfxRCH, bool useDebugOptions);

    [Osiris.PluginFunctionAttr]
    public static VTFX_PreShutDown PreShutDown = null;
    public delegate void VTFX_PreShutDown(VtfxRuntimeContextHandle vtfxRCH);

    [Osiris.PluginFunctionAttr]
    public static VTFX_ShutDown ShutDown = null;
    public delegate void VTFX_ShutDown(VtfxRuntimeContextHandle vtfxRCH);

    [Osiris.PluginFunctionAttr]
    public static VTFX_IsConnected IsConnected = null;
    public delegate bool VTFX_IsConnected(VtfxRuntimeContextHandle vtfxRCH);

    [Osiris.PluginFunctionAttr(funcName = "VTFX_FindSystemAudioDeviceIdentifierByName")]
    static VTFX_FindSystemAudioDeviceIdentifierByName FindSystemAudioDeviceIdentifierByName_C = null;
    delegate IntPtr VTFX_FindSystemAudioDeviceIdentifierByName(VtfxRuntimeContextHandle vtfxRCH, [MarshalAs(UnmanagedType.LPStr)] string outputName, [MarshalAs(UnmanagedType.LPStr)] string audioDeviceName);
    public static string FindSystemAudioDeviceIdentifierByName(VtfxRuntimeContextHandle vtfxRCH, string outputName, string audioDeviceName)
    {
        IntPtr result = FindSystemAudioDeviceIdentifierByName_C(vtfxRCH, outputName, audioDeviceName);
        if (result == IntPtr.Zero) {
            return null;
        }

        return Marshal.PtrToStringAnsi(result);
    }

    // Device API
    [Osiris.PluginFunctionAttr]
    public static VTFX_EnsureEarsDeviceExists EnsureEarsDeviceExists = null;
    public delegate void VTFX_EnsureEarsDeviceExists(VtfxRuntimeContextHandle vtfxRCH);

    [Osiris.PluginFunctionAttr]
    public static VTFX_TryLoadingAllVTFXOutputPlugins TryLoadingAllVtfxOutputPlugins = null;
    public delegate void VTFX_TryLoadingAllVTFXOutputPlugins(VtfxRuntimeContextHandle vtfxRCH);

    [Osiris.PluginFunctionAttr]
    public static VTFX_CreateDevice CreateDevice = null;
    public delegate DeviceHandle VTFX_CreateDevice(
        VtfxRuntimeContextHandle vtfxRCH,
        VtfxDeviceInfo deviceInfo,
        [MarshalAs(UnmanagedType.LPStr)] string deviceName,
        [MarshalAs(UnmanagedType.LPStr)] string channelName,
        [MarshalAs(UnmanagedType.LPStr)] string driverIdentifier,
        [MarshalAs(UnmanagedType.LPStr)] string driverPort);

    [Osiris.PluginFunctionAttr]
    public static VTFX_CreateOutput CreateOutput = null;
    public delegate void VTFX_CreateOutput(VtfxRuntimeContextHandle vtfxRCH, [MarshalAs(UnmanagedType.LPStr)] string name, DeviceHandle deviceHandle);

    [Osiris.PluginFunctionAttr]
    public static VTFX_FindDeviceByName FindDeviceByName = null;
    public delegate DeviceHandle VTFX_FindDeviceByName(VtfxRuntimeContextHandle vtfxRCH, [MarshalAs(UnmanagedType.LPStr)] string name);

    [Osiris.PluginFunctionAttr]
    public static VTFX_GetNumDevices GetNumDevices = null;
    public delegate int VTFX_GetNumDevices(VtfxRuntimeContextHandle vtfxRCH);

    [Osiris.PluginFunctionAttr]
    public static VTFX_GetDevices GetDevices = null;
    public delegate bool VTFX_GetDevices(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle[] handles, int numHandles, out int numDevices);

    [Osiris.PluginFunctionAttr(funcName = "VTFX_Device_GetName")]
    static VTFX_Device_GetName Device_GetName_C = null;
    delegate VtfxSpanFillResult VTFX_Device_GetName(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle device, byte[] name, int nameLen);
    public static string Device_GetName(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle device)
    {
        byte[] name = new byte[256];

        VtfxSpanFillResult result = Device_GetName_C(vtfxRCH, device, name, name.Length);
        if (result.num_filled == 0)
            return "";

        string nameStr = System.Text.Encoding.UTF8.GetString(name).Trim('\0');
        return nameStr;
    }

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_GetDriverIdentifier Device_GetDriverIdentifier = null;
    public delegate VtfxSpanFillResult VTFX_Device_GetDriverIdentifier(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle device, byte[] identifier, int identifierLen);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_GetDriverPort Device_GetDriverPort = null;
    public delegate VtfxSpanFillResult VTFX_Device_GetDriverPort(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle device, byte[] driverPort, int driverPortLen);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_GetChannelName Device_GetChannelName = null;
    public delegate VtfxSpanFillResult VTFX_Device_GetChannelName(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle device, byte[] channelName, int channelNameLen);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_GetNumChannels Device_GetNumChannels = null;
    public delegate int VTFX_Device_GetNumChannels(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle device);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_GetSupportsSignedSample Device_GetSupportsSignedSample = null;
    public delegate bool VTFX_Device_GetSupportsSignedSample(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle device);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_GetSupportsFloatSample Device_GetSupportsFloatSample = null;
    public delegate bool VTFX_Device_GetSupportsFloatSample(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle device);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_GetBufferType Device_GetBufferType = null;
    public delegate int VTFX_Device_GetBufferType(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle device);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_GetBytesPerSample Device_GetBytesPerSample = null;
    public delegate int VTFX_Device_GetBytesPerSample(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle device);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_GetSamplesPerChannelPerSec Device_GetSamplesPerChannelPerSec = null;
    public delegate int VTFX_Device_GetSamplesPerChannelPerSec(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle device);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_GetSamplesPerChannelPerBuffer Device_GetSamplesPerChannelPerBuffer = null;
    public delegate int VTFX_Device_GetSamplesPerChannelPerBuffer(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle device);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_GetHandedness Device_GetHandedness = null;
    public delegate int VTFX_Device_GetHandedness(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle device);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_FillNextOutputBuffer Device_FillNextOutputBuffer = null;
    public delegate FillResult VTFX_Device_FillNextOutputBuffer(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle device, byte[] buffer, int bufferSize);

    // IntPtr buffers is treated as a uint8_t**
    // Calling this function will require some manual marshalling on the C# side
    // https://stackoverflow.com/questions/2527014/how-do-i-marshal-a-pointer-to-an-array-of-pointers-to-structures
    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_FillNextOutputBuffers Device_FillNextOutputBuffers = null;
    public delegate FillResult VTFX_Device_FillNextOutputBuffers(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle device, IntPtr buffers, int numBuffers, int bufferSize);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_SetMasterVolume Device_SetMasterVolume = null;
    public delegate void VTFX_Device_SetMasterVolume(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle handle, float volume);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_GetMasterVolume Device_GetMasterVolume = null;
    public delegate float VTFX_Device_GetMasterVolume(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle handle);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_SetDelay Device_SetDelay = null;
    public delegate void VTFX_Device_SetDelay(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle handle, float delay);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_GetDelay Device_GetDelay = null;
    public delegate float VTFX_Device_GetDelay(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle handle);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_SetChannelVolumes Device_SetChannelVolumes = null;
    public delegate void VTFX_Device_SetChannelVolumes(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle handle, float[] volumes, int numVolumes);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_SetCalibrationChannelVolumes Device_SetCalibrationChannelVolumes = null;
    public delegate void VTFX_Device_SetCalibrationChannelVolumes(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle handle, float[] volumes, int numVolumes);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_GetChannelVolumes Device_GetChannelVolumes = null;
    public delegate int VTFX_Device_GetChannelVolumes(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle handle, float[] volumes, int numVolumes);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_GetCurrentChannelOutputVolumes Device_GetCurrentChannelOutputVolumes = null;
    public delegate int VTFX_Device_GetCurrentChannelOutputVolumes(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle handle, float[] volumes, int numVolumes);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_Release Device_Release = null;
    public delegate void VTFX_Device_Release(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle handle);

    // Device Tunable API
    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_CreateTunable Device_CreateTunable = null;
    public delegate float VTFX_Device_CreateTunable(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle deviceHandle, [MarshalAs(UnmanagedType.LPStr)] string name);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_GetNumTunables Device_GetNumTunables = null;
    public delegate int VTFX_Device_GetNumTunables(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle deviceHandle);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_FindTunableByName Device_FindTunableByName = null;
    public delegate TunableHandle VTFX_Device_FindTunableByName(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle device, [MarshalAs(UnmanagedType.LPStr)] string name);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_GetTunableAtIndex Device_GetTunableAtIndex = null;
    public delegate TunableHandle VTFX_Device_GetTunableAtIndex(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle device, int index);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_SetListenerPose Device_SetListenerPose = null;
    public delegate void VTFX_Device_SetListenerPose(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle device,
      float posX, float posY, float posZ,
      float fwdX, float fwdY, float fwdZ,
      float upX, float upY, float upZ);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_AddTag Device_AddTag = null;
    public delegate void VTFX_Device_AddTag(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle handle, [MarshalAs(UnmanagedType.LPStr)] string tag);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_HasTag Device_HasTag = null;
    public delegate bool VTFX_Device_HasTag(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle handle, [MarshalAs(UnmanagedType.LPStr)] string tag);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Device_AddChannelLabel Device_AddChannelLabel = null;
    public delegate void VTFX_Device_AddChannelLabel(VtfxRuntimeContextHandle vtfxRCH, DeviceHandle handle, [MarshalAs(UnmanagedType.LPStr)] string label, int index);

    // Effect API

    [Osiris.PluginFunctionAttr]
    public static VTFX_CreateEffect CreateEffect = null;
    public delegate EffectHandle VTFX_CreateEffect(VtfxRuntimeContextHandle vtfxRCH, [MarshalAs(UnmanagedType.LPStr)] string name);

    [Osiris.PluginFunctionAttr]
    public static VTFX_CreateEffectJson CreateEffectJson = null;
    public delegate EffectHandle VTFX_CreateEffectJson(VtfxRuntimeContextHandle vtfxRCH, [MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string json);

    [Osiris.PluginFunctionAttr]
    public static VTFX_ReleaseEffect ReleaseEffect = null;
    public delegate void VTFX_ReleaseEffect(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect);

    [Osiris.PluginFunctionAttr]
    public static VTFX_ClearEffects ClearEffects = null;
    public delegate void VTFX_ClearEffects(VtfxRuntimeContextHandle vtfxRCH);

    [Osiris.PluginFunctionAttr]
    public static VTFX_LoadEffects LoadEffects = null;
    public delegate bool VTFX_LoadEffects(VtfxRuntimeContextHandle vtfxRCH, [MarshalAs(UnmanagedType.LPStr)] string fileName, bool append = false);

    [Osiris.PluginFunctionAttr]
    public static VTFX_LoadEffectsAsync LoadEffectsAsync = null;
    public delegate bool VTFX_LoadEffectsAsync(VtfxRuntimeContextHandle vtfxRCH, [MarshalAs(UnmanagedType.LPStr)] string fileName);

    [Osiris.PluginFunctionAttr]
    public static VTFX_LoadEffectsJson LoadEffectsJson = null;
    public delegate bool VTFX_LoadEffectsJson(VtfxRuntimeContextHandle vtfxRCH, [MarshalAs(UnmanagedType.LPStr)] string json, [MarshalAs(UnmanagedType.LPStr)] string dataPath, bool append = false);

    [Osiris.PluginFunctionAttr]
    public static VTFX_SaveEffects SaveEffects = null;
    public delegate bool VTFX_SaveEffects(VtfxRuntimeContextHandle vtfxRCH);

    [Osiris.PluginFunctionAttr]
    public static VTFX_FindEffectByName FindEffectByName = null;
    public delegate EffectHandle VTFX_FindEffectByName(VtfxRuntimeContextHandle vtfxRCH, [MarshalAs(UnmanagedType.LPStr)] string name);

    [Osiris.PluginFunctionAttr]
    public static VTFX_GetNumEffects GetNumEffects = null;
    public delegate int VTFX_GetNumEffects(VtfxRuntimeContextHandle vtfxRCH);

    [Osiris.PluginFunctionAttr]
    public static VTFX_GetEffectByIndex GetEffectByIndex = null;
    public delegate EffectHandle VTFX_GetEffectByIndex(VtfxRuntimeContextHandle vtfxRCH, int index);

    [Osiris.PluginFunctionAttr(funcName = "VTFX_Effect_GetName")]
    public static VTFX_Effect_GetName Effect_GetName_C = null;
    public delegate VtfxSpanFillResult VTFX_Effect_GetName(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect, byte[] name, int nameLen);
    public static string Effect_GetName(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect)
    {
        byte[] name = new byte[256];

        VtfxSpanFillResult result = Effect_GetName_C(vtfxRCH, effect, name, name.Length);
        if (result.num_filled == 0)
            return "";

        string nameStr = System.Text.Encoding.UTF8.GetString(name).Trim('\0');
        return nameStr;
    }

    [Osiris.PluginFunctionAttr]
    public static VTFX_Effect_GetDescription Effect_GetDescription_C = null;
    public delegate VtfxSpanFillResult VTFX_Effect_GetDescription(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect, byte[] description, int descriptionLen);
    public static string Effect_GetDescription(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect)
    {
        byte[] description = new byte[1024];

        VtfxSpanFillResult result = Effect_GetDescription_C(vtfxRCH, effect, description, description.Length);
        if (result.num_filled == 0)
            return "";

        string descriptionStr = System.Text.Encoding.UTF8.GetString(description).Trim('\0');
        return descriptionStr;
    }

    [Osiris.PluginFunctionAttr]
    public static VTFX_Effect_SetDescription Effect_SetDescription = null;
    public delegate void VTFX_Effect_SetDescription(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect, [MarshalAs(UnmanagedType.LPStr)] string description);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Effect_SetDuration Effect_SetDuration = null;
    public delegate void VTFX_Effect_SetDuration(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect, float duration);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Effect_SetHandedness Effect_SetHandedness = null;
    public delegate void VTFX_Effect_SetHandedness(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect, [MarshalAs(UnmanagedType.SysInt)] DeviceHandedness handedness);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Effect_CreateNode Effect_CreateNode = null;
    public delegate NodeHandle VTFX_Effect_CreateNode(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect, [MarshalAs(UnmanagedType.LPStr)] string nodeName, [MarshalAs(UnmanagedType.LPStr)] string nodeType);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Effect_FindNodeByName Effect_FindNodeByName = null;
    public delegate NodeHandle VTFX_Effect_FindNodeByName(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect, [MarshalAs(UnmanagedType.LPStr)] string name);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Effect_FindNodeByType Effect_FindNodeByType = null;
    public delegate NodeHandle VTFX_Effect_FindNodeByType(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect, [MarshalAs(UnmanagedType.LPStr)] string typeName, int index);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Effect_IsValid Effect_IsValid = null;
    public delegate bool VTFX_Effect_IsValid(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Effect_IsPlaying Effect_IsPlaying = null;
    public delegate bool VTFX_Effect_IsPlaying(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Effect_SetPlaying Effect_SetPlaying = null;
    public delegate void VTFX_Effect_SetPlaying(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect, [MarshalAs(UnmanagedType.U1)] bool playing);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Effect_SetNextPlayTime Effect_SetNextPlayTime = null;
    public delegate void VTFX_Effect_SetNextPlayTime(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect, float playTime);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Effect_GetLooping Effect_GetLooping = null;
    public delegate bool VTFX_Effect_GetLooping(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Effect_SetLooping Effect_SetLooping = null;
    public delegate void VTFX_Effect_SetLooping(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect, [MarshalAs(UnmanagedType.U1)] bool looping);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Effect_GetMasterVolume Effect_GetMasterVolume = null;
    public delegate float VTFX_Effect_GetMasterVolume(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Effect_SetMasterVolume Effect_SetMasterVolume = null;
    public delegate void VTFX_Effect_SetMasterVolume(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect, float volume);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Effect_Clone Effect_Clone = null;
    public delegate EffectHandle VTFX_Effect_Clone(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect, [MarshalAs(UnmanagedType.LPStr)] string cloneName);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Effect_SetTransient Effect_SetTransient = null;
    public delegate void VTFX_Effect_SetTransient(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect, [MarshalAs(UnmanagedType.U1)] bool transient);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Effect_SetDestroyOnComplete Effect_SetDestroyOnComplete = null;
    public delegate void VTFX_Effect_SetDestroyOnComplete(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect, [MarshalAs(UnmanagedType.U1)] bool shouldDestroyOnComplete);

    // Effect Tunable API
    [Osiris.PluginFunctionAttr]
    public static VTFX_Effect_CreateTunable Effect_CreateTunable = null;
    public delegate TunableHandle VTFX_Effect_CreateTunable(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect, [MarshalAs(UnmanagedType.LPStr)] string name);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Effect_GetNumTunables Effect_GetNumTunables = null;
    public delegate int VTFX_Effect_GetNumTunables(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Effect_FindTunableByName Effect_FindTunableByName = null;
    public delegate TunableHandle VTFX_Effect_FindTunableByName(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect, [MarshalAs(UnmanagedType.LPStr)] string name);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Effect_GetTunableAtIndex Effect_GetTunableAtIndex = null;
    public delegate TunableHandle VTFX_Effect_GetTunableAtIndex(VtfxRuntimeContextHandle vtfxRCH, EffectHandle effect, int index);

    // Node API

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_SetDParamValue Node_SetDParamValue = null;
    public delegate void VTFX_Node_SetDParamValue(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string paramName, float value);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_SetDParamArrayValue Node_SetDParamArrayValue = null;
    public delegate void VTFX_Node_SetDParamArrayValue(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string paramName, int offset, float value);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_SetDParamValues Node_SetDParamValues = null;
    public delegate void VTFX_Node_SetDParamValues(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string paramName, float[] values);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_GetDParamValue Node_GetDParamValue = null;
    public delegate float VTFX_Node_GetDParamValue(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string paramName);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_GetDParamArrayValue Node_GetDParamArrayValue = null;
    public delegate float VTFX_Node_GetDParamArrayValue(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string paramName, int offset);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_SetDParamInput Node_SetDParamInput = null;
    public delegate void VTFX_Node_SetDParamInput(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string paramName, NodeHandle nodeWithOutput, int outputIndex);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_SetDParamArrayInput Node_SetDParamArrayInput = null;
    public delegate void VTFX_Node_SetDParamArrayInput(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string paramName, int offset, NodeHandle nodeWithOutput, int outputIndex);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_GetDParamInput Node_GetDParamInput = null;
    public delegate NodeHandle VTFX_Node_GetDParamInput(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string paramName);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_GetDParamArrayInput Node_GetDParamArrayInput = null;
    public delegate NodeHandle VTFX_Node_GetDParamArrayInput(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string paramName, int offset);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_GetNumElementsInDParamArray Node_GetNumElementsInDParamArray = null;
    public delegate NodeHandle VTFX_Node_GetNumElementsInDParamArray(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string paramName);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_SetCParamFloat Node_SetCParamFloat = null;
    public delegate void VTFX_Node_SetCParamFloat(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string paramName, float value);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_SetCParamInt Node_SetCParamInt = null;
    public delegate void VTFX_Node_SetCParamInt(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string paramName, int value);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_SetCParamBool Node_SetCParamBool = null;
    public delegate void VTFX_Node_SetCParamBool(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string paramName, bool value);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_SetCParamString Node_SetCParamString = null;
    public delegate void VTFX_Node_SetCParamString(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string paramName, [MarshalAs(UnmanagedType.LPStr)] string value);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_SetCParamFloatArray Node_SetCParamFloatArray = null;
    public delegate void VTFX_Node_SetCParamFloatArray(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string paramName, float[] values, int valuesLength);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_SetCParamStringArray Node_SetCParamStringArray = null;
    public delegate void VTFX_Node_SetCParamStringArray(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string paramName, [MarshalAs(UnmanagedType.LPStr)] string[] values, int valuesLength);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_SetCParamEnum Node_SetCParamEnum = null;
    public delegate void VTFX_Node_SetCParamEnum(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string paramName, int enumValue);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_GetCParamFloat Node_GetCParamFloat = null;
    public delegate float VTFX_Node_GetCParamFloat(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string paramName);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_GetCParamInt Node_GetCParamInt = null;
    public delegate int VTFX_Node_GetCParamInt(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string paramName);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_GetCParamBool Node_GetCParamBool = null;
    public delegate bool VTFX_Node_GetCParamBool(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string paramName);

    [Osiris.PluginFunctionAttr(funcName = "VTFX_Node_GetCParamString")]
    public static VTFX_Node_GetCParamString Node_GetCParamString_C = null;
    public delegate VtfxSpanFillResult VTFX_Node_GetCParamString(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string paramName, byte[] outName, int nameLen);
    public static string Node_GetCParamString(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, string paramName)
    {
        byte[] name = new byte[256];

        VtfxSpanFillResult result = Node_GetCParamString_C(vtfxRCH, node, paramName, name, name.Length);
        if (result.num_filled == 0)
            return "";

        string nameStr = System.Text.Encoding.UTF8.GetString(name).Trim('\0');
        return nameStr;
    }

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_GetCParamFloatArray Node_GetCParamFloatArray = null;
    public delegate float[] VTFX_Node_GetCParamFloatArray(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string paramName, float[] outArray, int arrayLength);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_GetCParamEnum Node_GetCParamEnum = null;
    public delegate int VTFX_Node_GetCParamEnum(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string paramName);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_SetDisableSelfOnly Node_SetDisableSelfOnly = null;
    public delegate void VTFX_Node_SetDisableSelfOnly(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.Bool)] bool disableSelfOnly);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_GetDisableSelfOnly Node_GetDisableSelfOnly = null;
    public delegate bool VTFX_Node_GetDisableSelfOnly(VtfxRuntimeContextHandle vtfxRCH, NodeHandle effect);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_SetDisableSelfAndChildren Node_SetDisableSelfAndChildren = null;
    public delegate void VTFX_Node_SetDisableSelfAndChildren(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.Bool)] bool disableSelfAndChildren);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_GetDisableSelfAndChildren Node_GetDisableSelfAndChildren = null;
    public delegate bool VTFX_Node_GetDisableSelfAndChildren(VtfxRuntimeContextHandle vtfxRCH, NodeHandle effect);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_SetWriteDebugOutput Node_SetWriteDebugOutput = null;
    public delegate void VTFX_Node_SetWriteDebugOutput(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.Bool)] bool writeDebugOutput);

    // Node API

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_RawData_Load Node_RawData_Load = null;
    public delegate void VTFX_Node_RawData_Load(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string filename);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_RawData_GetDurationSeconds Node_RawData_GetDurationSeconds = null;
    public delegate float VTFX_Node_RawData_GetDurationSeconds(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_Streaming_Init Node_Streaming_Init = null;
    public delegate void VTFX_Node_Streaming_Init(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, int samplesPerSec, int numSamples, int warmupSamples);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_Streaming_AddData Node_Streaming_AddData = null;
    public delegate void VTFX_Node_Streaming_AddData(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, float[] samples, int numSamples);

    /*
    // p/invoke requires array to be flattened. disabled for now.
    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_Streaming_AddMultichannelData Node_Streaming_AddMultichannelData = null;
    public delegate void VTFX_Node_Streaming_AddMultichannelData(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, float[][] channelBuffers, int numChannels, int samplesPerChannel);
    */

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_Curve_AddPoint Node_Curve_AddPoint = null;
    public delegate void VTFX_Node_Curve_AddPoint(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, float t, float v);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_Output_AddDeviceTag Node_Output_AddDeviceTag = null;
    public delegate void VTFX_Node_Output_AddDeviceTag(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.LPStr)] string str);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_Output_SetHandedness Node_Output_SetHandedness = null;
    public delegate void VTFX_Node_Output_SetHandedness(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, [MarshalAs(UnmanagedType.SysInt)] DeviceHandedness handedness);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_Output_AddExplicitDevice Node_Output_AddExplicitDevice = null;
    public delegate void VTFX_Node_Output_AddExplicitDevice(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, DeviceHandle device);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Node_Tunable_SetTunable Node_Tunable_SetTunable = null;
    public delegate void VTFX_Node_Tunable_SetTunable(VtfxRuntimeContextHandle vtfxRCH, NodeHandle node, TunableHandle tunable);

    [Osiris.PluginFunctionAttr]
    public static VTFX_CreateGlobalTunable CreateGlobalTunable = null;
    public delegate TunableHandle VTFX_CreateGlobalTunable(VtfxRuntimeContextHandle vtfxRCH, [MarshalAs(UnmanagedType.LPStr)] string name);

    [Osiris.PluginFunctionAttr]
    public static VTFX_GetNumGlobalTunables GetNumGlobalTunables = null;
    public delegate int VTFX_GetNumGlobalTunables(VtfxRuntimeContextHandle vtfxRCH);

    [Osiris.PluginFunctionAttr]
    public static VTFX_GetTunableValue GetTunableValue = null;
    public delegate float VTFX_GetTunableValue(VtfxRuntimeContextHandle vtfxRCH, TunableHandle tunable);

    [Osiris.PluginFunctionAttr]
    public static VTFX_SetTunableValue SetTunableValue = null;
    public delegate void VTFX_SetTunableValue(VtfxRuntimeContextHandle vtfxRCH, TunableHandle tunable, float value);

    [Osiris.PluginFunctionAttr]
    public static VTFX_GetTunableRangeStart GetTunableRangeStart = null;
    public delegate float VTFX_GetTunableRangeStart(VtfxRuntimeContextHandle vtfxRCH, TunableHandle tunable);

    [Osiris.PluginFunctionAttr]
    public static VTFX_GetTunableRangeEnd GetTunableRangeEnd = null;
    public delegate float VTFX_GetTunableRangeEnd(VtfxRuntimeContextHandle vtfxRCH, TunableHandle tunable);

    [Osiris.PluginFunctionAttr]
    public static VTFX_SetTunableRange SetTunableRange = null;
    public delegate void VTFX_SetTunableRange(VtfxRuntimeContextHandle vtfxRCH, TunableHandle tunable, float rangeStart, float rangeEnd);

    [Osiris.PluginFunctionAttr(funcName = "VTFX_GetTunableName")]
    public static VTFX_GetTunableName GetTunableName_C = null;
    public delegate VtfxSpanFillResult VTFX_GetTunableName(VtfxRuntimeContextHandle vtfxRCH, TunableHandle tunable, byte[] name, int nameLen);
    public static string GetTunableName(VtfxRuntimeContextHandle vtfxRCH, TunableHandle tunable)
    {
        byte[] name = new byte[256];

        VtfxSpanFillResult result = GetTunableName_C(vtfxRCH, tunable, name, name.Length);
        if (result.num_filled == 0)
            return "";

        string nameStr = System.Text.Encoding.UTF8.GetString(name).Trim('\0');
        return nameStr;
    }

    [Osiris.PluginFunctionAttr]
    public static VTFX_SetTunableName SetTunableName = null;
    public delegate void VTFX_SetTunableName(VtfxRuntimeContextHandle vtfxRCH, TunableHandle tunable, [MarshalAs(UnmanagedType.LPStr)] string name);

    [Osiris.PluginFunctionAttr]
    public static VTFX_GetTunableDefault GetTunableDefault = null;
    public delegate float VTFX_GetTunableDefault(VtfxRuntimeContextHandle vtfxRCH, TunableHandle tunable);

    [Osiris.PluginFunctionAttr]
    public static VTFX_SetTunableDefault SetTunableDefault = null;
    public delegate void VTFX_SetTunableDefault(VtfxRuntimeContextHandle vtfxRCH, TunableHandle tunable, float newDefault);

    [Osiris.PluginFunctionAttr]
    public static VTFX_GetTunableType GetTunableType = null;
    public delegate int VTFX_GetTunableType(VtfxRuntimeContextHandle vtfxRCH, TunableHandle tunable);

    [Osiris.PluginFunctionAttr]
    public static VTFX_SetTunableType SetTunableType = null;
    public delegate void VTFX_SetTunableType(VtfxRuntimeContextHandle vtfxRCH, TunableHandle tunable, int type);

    [Osiris.PluginFunctionAttr]
    public static VTFX_FindGlobalTunableByName FindGlobalTunableByName = null;
    public delegate TunableHandle VTFX_FindGlobalTunableByName(VtfxRuntimeContextHandle vtfxRCH, [MarshalAs(UnmanagedType.LPStr)] string name);

    [Osiris.PluginFunctionAttr]
    public static VTFX_GetGlobalTunableAtIndex GetGlobalTunableAtIndex = null;
    public delegate TunableHandle VTFX_GetGlobalTunableAtIndex(VtfxRuntimeContextHandle vtfxRCH, int index);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Studio_Create Studio_Create = null;
    public delegate void VTFX_Studio_Create(VtfxRuntimeContextHandle vtfxRCH, int mode);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Studio_Destroy Studio_Destroy = null;
    public delegate void VTFX_Studio_Destroy(VtfxRuntimeContextHandle vtfxRCH);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Studio_Run Studio_Run = null;
    public delegate void VTFX_Studio_Run(VtfxRuntimeContextHandle vtfxRCH);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Studio_Stop Studio_Stop = null;
    public delegate void VTFX_Studio_Stop(VtfxRuntimeContextHandle vtfxRCH);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Studio_CreateAndRunAsync Studio_CreateAndRunAsync = null;
    public delegate void VTFX_Studio_CreateAndRunAsync(VtfxRuntimeContextHandle vtfxRCH, int mode);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Studio_IsRunning Studio_IsRunning = null;
    public delegate void VTFX_Studio_IsRunning(VtfxRuntimeContextHandle vtfxRCH);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Studio_FocusWindow Studio_FocusWindow = null;
    public delegate void VTFX_Studio_FocusWindow(VtfxRuntimeContextHandle vtfxRCH);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Studio_ShowWindow Studio_ShowWindow = null;
    public delegate void VTFX_Studio_ShowWindow(VtfxRuntimeContextHandle vtfxRCH);

    [Osiris.PluginFunctionAttr]
    public static VTFX_Studio_HideWindow Studio_HideWindow = null;
    public delegate void VTFX_Studio_HideWindow(VtfxRuntimeContextHandle vtfxRCH);

    [Osiris.PluginFunctionAttr]
    public static VTFX_SetLogLevel SetLogLevel = null;
    public delegate void VTFX_SetLogLevel(VtfxRuntimeContextHandle vtfxRCH, LogLevel level);

    [Osiris.PluginFunctionAttr]
    public static VTFX_SetRemoteControlBroadcastEnabled SetRemoteControlBroadcastEnabled = null;
    public delegate void VTFX_SetRemoteControlBroadcastEnabled(VtfxRuntimeContextHandle vtfxRCH, bool enabled);

    [Osiris.PluginFunctionAttr]
    public static VTFX_RegisterDeviceTag RegisterDeviceTag = null;
    public delegate void VTFX_RegisterDeviceTag(string tag);

    public delegate void LoadResourceOnSuccessCb(
        [MarshalAs(UnmanagedType.LPStr)] string resourceName,
        byte[] resourceBytes,
        UInt64 numBytes,
        IntPtr requestUserData);

    public delegate void LoadResourceOnFailCb(
        [MarshalAs(UnmanagedType.LPStr)] string resourceName,
        [MarshalAs(UnmanagedType.LPStr)] string err,
        IntPtr requestUserData);

    public delegate void ReleaseUserDataFn(
        IntPtr userData);

    public delegate void LoadResourceFn(
        [MarshalAs(UnmanagedType.LPStr)] string resourceName,
        IntPtr loaderUserData,
        IntPtr requestUserData,
        LoadResourceOnSuccessCb onSuccess,
        LoadResourceOnFailCb onFail);

    [Osiris.PluginFunctionAttr]
    public static VTFX_RegisterResourceLoader RegisterResourceLoader = null;
    public delegate ResourceLoaderHandle VTFX_RegisterResourceLoader(
        VtfxRuntimeContextHandle vtfxRCH,
        [MarshalAs(UnmanagedType.LPStr)] string loaderName,
        LoadResourceFn loadResourceFn,
        IntPtr loaderUserData,
        ReleaseUserDataFn releaseLoaderUserDataFn);
}
