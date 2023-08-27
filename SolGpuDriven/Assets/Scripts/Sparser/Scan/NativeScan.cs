using Sparser.ComputeBufferEx;
using UnityEngine;

namespace Sparser.Scan
{
    public class NativeScan : IBufferScan<int>
    {
        private static ComputeShader shader;
        private static int scanKernel;
        private static int scanGroupResultsKernel;
        private static int addGroupResultsKernel;
        private static int inclusiveToExclusiveKernel;

        private const int LANE_STRIDE = 4;

        static NativeScan()
        {
            shader = Resources.Load<ComputeShader>("NaiveScan");
            scanKernel = shader.FindKernel("Scan");
            scanGroupResultsKernel = shader.FindKernel("ScanGroupResults");
            addGroupResultsKernel = shader.FindKernel("AddGroupResults");
            inclusiveToExclusiveKernel = shader.FindKernel("InclusiveToExclusive");
        }

        public void Scan(ComputeBufferBase<int> buffer, int count)
        {
            uint gx, gy, gz;
            shader.GetKernelThreadGroupSizes(scanKernel, out gx, out gy, out gz);
            int numGroupsX = Mathf.CeilToInt((float)count / gx);

            shader.SetInt("count", count);
            shader.SetInt("lane_stride", LANE_STRIDE);
            shader.SetBuffer(scanKernel, "data", buffer.Buffer);
            shader.Dispatch(scanKernel, numGroupsX, 1, 1);

            // If we can't complete the scan within a single group.
            if (count > LANE_STRIDE * gx) {
                using (var groupData = new StructuredBuffer<int>((int)gx)) {
                    shader.SetBuffer(scanGroupResultsKernel, "data", buffer.Buffer);
                    shader.SetBuffer(scanGroupResultsKernel, "group_data", groupData.Buffer);
                    shader.Dispatch(scanGroupResultsKernel, 1, 1, 1);

                    shader.SetBuffer(addGroupResultsKernel, "data", buffer.Buffer);
                    shader.SetBuffer(addGroupResultsKernel, "group_data", groupData.Buffer);
                    shader.Dispatch(addGroupResultsKernel, numGroupsX, 1, 1);
                }
            }

            shader.SetBuffer(inclusiveToExclusiveKernel, "data", buffer.Buffer);
            shader.Dispatch(inclusiveToExclusiveKernel, 1, 1, 1);
        }
        

        public void ScanIndirect(ComputeBufferBase<int> buffer, ComputeBufferBase<uint> count)
        {
            throw new System.NotImplementedException();
        }
    }
}