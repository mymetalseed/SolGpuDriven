using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RVT.Job
{
    public struct FDecodeFeedbackJob : IJobParallelFor
    {
        [ReadOnly]
        internal int pageSize;

        [ReadOnly]
        internal NativeArray<Color32> encodeDatas;

        [WriteOnly]
        internal NativeArray<int4> decodeDatas;

        public void Execute(int index)
        {
            Color32 x = encodeDatas[index];
            float4 xRaw = new float4((float)x.r / 255.0f, (float)x.g / 255.0f, (float)x.b / 255.0f, x.a);

            float3 x888 = math.floor(xRaw.xyz * 255);
            float High =  math.floor(x888.z / 16);	// x888.z >> 4
            float Low = x888.z - High * 16;		// x888.z & 15
            float2 x1212 = x888.xy + new float2(Low, High) * 256;
            x1212 = math.saturate(x1212 / 4095) * pageSize;

            decodeDatas[index] = new int4((int)x1212.x, (int)x1212.y, x.a, 255);
        }
    }

    
    internal unsafe struct FAnalysisFeedbackJob : IJob
    {
        internal int maxMip;
        internal int pageSize;
        internal int tileNum;
        internal int frameCount;

        [NativeDisableUnsafePtrRestriction]
        internal FLruCache* lruCache;

        internal NativeArray<int4> readbackDatas;
        internal NativeArray<FPageTable> pageTables;
        internal NativeList<FPageLoadInfo> loadRequests;

        public void Execute()
        {
            int4 preValue = -1;
            for (int i = 0; i < readbackDatas.Length; ++i)
            {
                int4 readbackData = readbackDatas[i];

                //跳过相同的处理
                if (readbackData.Equals(preValue)) 
                    continue;

                preValue = readbackData;
                
                if (readbackData.z > maxMip || readbackData.z < 0 || readbackData.x < 0 || readbackData.y < 0 ||
                    readbackData.x >= pageSize || readbackData.y >= pageSize)
                {
                    continue;
                }

                ref FPage page = ref pageTables[readbackData.z].GetPage(readbackData.x, readbackData.y);

                if (page.isNull)
                    continue;

                //page还未加载
                if (!page.payload.isReady && page.payload.notLoading)
                {
                    page.payload.notLoading = false;
                    //添加一个loadRequest
                    loadRequests.AddNoResize(new FPageLoadInfo(readbackData.x, readbackData.y, readbackData.z));
                }
                
                if (page.payload.isReady && page.payload.activeFrame != frameCount)
                {
                    page.payload.activeFrame = frameCount;
                    //已经准备好,但是激活frame不等与当前frame
                    //直接激活(?
                    lruCache[0].SetActive(page.payload.pageCoord.y * tileNum + page.payload.pageCoord.x);
                }
            }
        }

    }
}