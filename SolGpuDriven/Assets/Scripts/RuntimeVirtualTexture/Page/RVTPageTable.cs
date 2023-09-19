using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace RVT
{
    public unsafe struct FPageTable : IDisposable
    {
        //mip等级
        internal int mipLevel;

        //每个cell的大小(正方形)
        internal int cellSize;

        //横纵轴的cell个数
        internal int cellCount;

        //Page的缓冲区
        [NativeDisableUnsafePtrRestriction] 
        internal FPage* pageBuffer;

        public FPageTable(in int mipLevel, in int tableSize)
        {
            this.mipLevel = mipLevel;
            //?
            this.cellSize = (int)math.pow(2, mipLevel);
            this.cellCount = tableSize / cellSize;
            this.pageBuffer = (FPage*)UnsafeUtility.Malloc(Marshal.SizeOf(typeof(FPage)) * (cellCount * cellCount), 64,
                Allocator.Persistent);
            //X*X大小Page
            for (int i = 0; i < cellCount; ++i)
            {
                for (int j = 0; j < cellCount; ++j)
                {
                    this.pageBuffer[i * cellCount + j] = new FPage(i * cellSize, j * cellSize, cellSize, cellSize, mipLevel);
                }
            }
        }

        public ref FPage GetPage(in int x, in int y)
        {
            int2 uv = new int2((x / cellSize) % cellCount, (y / cellSize) % cellCount);
            return ref pageBuffer[uv.x * cellCount + uv.y];
        }

        public void Dispose()
        {
            UnsafeUtility.Free((void*)pageBuffer, Allocator.Persistent);
        }
    }
}