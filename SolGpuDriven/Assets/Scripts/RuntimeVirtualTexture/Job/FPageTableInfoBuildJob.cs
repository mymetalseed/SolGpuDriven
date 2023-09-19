using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace RVT.Job
{
    public struct FPageTableInfoBuildJob : IJobParallelFor
    {
        internal int pageSize;
        
        [ReadOnly]
        internal NativeList<FPageDrawInfo> drawInfos;
        
        [WriteOnly]
        internal NativeArray<FPageTableInfo> pageTableInfos;

        public void Execute(int i)
        {
            FPageTableInfo pageInfo;
            pageInfo.pageData = new float4(drawInfos[i].drawPos.x, drawInfos[i].drawPos.y, drawInfos[i].mip / 255f, 0);
            pageInfo.matrix_M = float4x4.TRS(new float3(drawInfos[i].rect.x / pageSize, drawInfos[i].rect.y / pageSize, 0), quaternion.identity, drawInfos[i].rect.width / pageSize);
            pageTableInfos[i] = pageInfo;
        }
    }

    public struct FPageRequestInfoSortJob : IJob
    {
        internal NativeList<FPageLoadInfo> loadRequests;

        public void Execute()
        {
            loadRequests.Sort();
        }
    }
    
}