using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ADClient
{
    /// <summary>
    /// Utility classes and functions for marshalling.
    /// </summary>
    public static class MarshalUtilities
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MarshalledArray
        {
            public int length;
            public IntPtr data;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Transformf
        {
            public Quaternionf rotation;
            public Vector3f translation;
            public float scale;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Vector3f
        {
            public static implicit operator Vector3(Vector3f v)
            {
                return new Vector3(v.x, v.y, v.z);
            }

            public float x;
            public float y;
            public float z;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Quaternionf
        {
            public static implicit operator Quaternion(Quaternionf q)
            {
                return new Quaternion(q.x, q.y, q.z, q.w);
            }

            public float x;
            public float y;
            public float z;
            public float w;
        }

        public static Vector3 FromUnmanaged(this Vector3f v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static Vector3f ToUnmanaged(this Vector3 v)
        {
            return new Vector3f { x = v.x, y = v.y, z = v.z };
        }

        public static Vector3[] FromUnmanaged(this Vector3f[] arr)
        {
            return arr.Select(v => v.FromUnmanaged()).ToArray();
        }

        public static Quaternion FromUnmanaged(this Quaternionf q)
        {
            return new Quaternion(q.x, q.y, q.z, q.w);
        }

        public static Quaternionf ToUnmanaged(this Quaternion q)
        {
            return new Quaternionf { x = q.x, y = q.y, z = q.z, w = q.w };
        }

        public static Quaternion[] FromUnmanaged(this Quaternionf[] arr)
        {
            return arr.Select(q => q.FromUnmanaged()).ToArray();
        }

        public abstract class JaggedArrayMarshaller<T>
        {
            public T[][] MarshalFromNative(IntPtr ptr, int length = -1, int fixedNumCols = -1)
            {
                int numRows = length;
                IntPtr curPtr = new IntPtr(ptr.ToInt64());
                if (numRows < 0)
                {
                    numRows = Marshal.ReadInt32(ptr);
                    curPtr = new IntPtr(curPtr.ToInt64() + sizeof(int));
                }

                T[][] data = new T[numRows][];
                int elemSize = Marshal.SizeOf(typeof(T));

                // Populate the array
                for (int row = 0; row < numRows; row++)
                {
                    IntPtr innerPtr = curPtr;
                    int numCols = fixedNumCols;

                    if (numCols < 0)
                    {
                        MarshalledArray ma = (MarshalledArray)Marshal.PtrToStructure(innerPtr, typeof(MarshalledArray));
                        numCols = ma.length;
                        innerPtr = ma.data;
                    }

                    // allocate row
                    data[row] = new T[numCols];

                    // copy row data
                    MarshalRowFromNative(innerPtr, data[row], numCols);

                    // increment ptr
                    if (fixedNumCols < 0)
                    {
                        curPtr = new IntPtr(curPtr.ToInt64() + Marshal.SizeOf(typeof(MarshalledArray)));
                    }
                    else
                    {
                        curPtr = new IntPtr(curPtr.ToInt64() + elemSize * numCols);
                    }

                }

                return data;
            }

            protected abstract void MarshalRowFromNative(IntPtr ptr, T[] rowData, int length);
        }

        // because Marshal.Copy is not templatized, and because you can't constrain a templatized class to primitive types,
        // we have to have separate marshallers for each primitive type...

        public class JaggedArrayMarshallerInt : JaggedArrayMarshaller<int>
        {
            protected override void MarshalRowFromNative(IntPtr ptr, int[] rowData, int length)
            {
                Marshal.Copy(ptr, rowData, 0, length);
            }
        }

        public class JaggedArrayMarshallerFloat : JaggedArrayMarshaller<float>
        {
            protected override void MarshalRowFromNative(IntPtr ptr, float[] rowData, int length)
            {
                Marshal.Copy(ptr, rowData, 0, length);
            }
        }

        /// <summary>
        /// Marshals an array of structs from native to managed memory
        /// </summary>
        public static T[] StructArrayFromNative<T>(MarshalledArray ma)
        {
            T[] array = new T[ma.length];
            int elemSize = Marshal.SizeOf(typeof(T));

            // Populate the array, one struct at a time
            for (int i = 0; i < ma.length; i++)
            {
                IntPtr elemPtr = new IntPtr(ma.data.ToInt64() + (elemSize * i));
                array[i] = (T)Marshal.PtrToStructure(elemPtr, typeof(T));
            }

            return array;
        }

        /// <summary>
        /// Marshals an array of bytes from native to managed memory
        /// </summary>
        public static byte[] ByteArrayFromNative(MarshalledArray ma)
        {
            byte[] array = new byte[ma.length];

            Marshal.Copy(ma.data, array, 0, ma.length);
            return array;
        }
    }
}
