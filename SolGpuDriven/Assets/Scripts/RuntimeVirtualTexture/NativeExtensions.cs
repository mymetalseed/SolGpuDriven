﻿using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace RVT
{
  static class NativeExtensions
    {
        //Color To rgba32 8位一个颜色
        public static int ToInt(this Color32 color)
        {
            return (int)color.r + ((int)color.g << 8) + ((int)color.b << 16) + ((int)color.a << 24);
        }
        
        internal struct DefaultComparer<T> : IEqualityComparer<T> where T : IEquatable<T>
        {
            public bool Equals(T x, T y) => x.Equals(y);

            public int GetHashCode(T obj)
            {
                return obj.GetHashCode();
            }
        }
        
        public static unsafe int Unique<T, U>(T* array, int size, U comp) where T : unmanaged where U : IEqualityComparer<T>
        {
            int length = 0, current = 1;
            while (current < size)
            {
                if (!comp.Equals(array[current], array[length]))
                {
                    length += 1;
                    if (current != length)
                        array[length] = array[current];
                }
                current += 1;
            }
            return length;
        }

        public static unsafe int Unique<T>(T* array, int size) where T : unmanaged, IEquatable<T>
        {
            return Unique(array, size, new DefaultComparer<T>());
        }

        public static unsafe NativeArray<T> Unique<T>(NativeArray<T> array) where T : unmanaged, IEquatable<T>
        {
            return array.GetSubArray(0, Unique((T*)array.GetUnsafePtr(), array.Length, new DefaultComparer<T>()));
        }

        public static int Combine(int newKey, int currentKey)
        {
            return unchecked((currentKey * (int)0xA5555529) + newKey);
        }
    }
    
    struct ColorComparer : IComparer<Color32>
    {
        public int Compare(Color32 x, Color32 y) => x.ToInt().CompareTo(y.ToInt());
    }
    
    struct ColorEqualComparer : IEqualityComparer<Color32>
    {
        public bool Equals(Color32 x, Color32 y) => x.ToInt() == y.ToInt();
        public int GetHashCode(Color32 obj) => obj.GetHashCode();
    }
}