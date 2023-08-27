using Sparser.ComputeBufferEx;
using UnityEngine;

namespace Sparser.Sort
{
    public class BitonicSort : IBufferSort<int>
    {
        private static ComputeShader shader;
        private static int sortKernel;
        private static int sortSingleLevelKernel;
        private static int copyAndFillKernel;
        private static int copyBackKernel;

        static BitonicSort()
        {
            shader = Resources.Load<ComputeShader>("BitonicSort");
            sortKernel = shader.FindKernel("Sort");
            sortSingleLevelKernel = shader.FindKernel("SortSingleLevel");
            copyAndFillKernel = shader.FindKernel("CopyAndFill");
            copyBackKernel = shader.FindKernel("CopyBack");
        }
        
        public void Sort(ComputeBufferBase<int> buffer, int count = 0)
        {
            if (Mathf.IsPowerOfTwo(count))
            {
                SortPow2(buffer, count);
            }
            else
            {
                using (var copy = CopyToPow2Buffer(buffer, count)) {
                    SortPow2(copy, copy.Count);
                    CopyToOriginalBuffer(copy, buffer, count);
                }
            }
        }

        public void SortIndirect(ComputeBufferBase<int> buffer, ComputeBufferBase<uint> count)
        {
            throw new System.NotImplementedException();
        }

        private void SortPow2(ComputeBufferBase<int> buffer, int count)
        {
            uint gx, gy, gz;
            shader.GetKernelThreadGroupSizes(sortKernel,out gx,out gy,out gz);
            int numGroupsX = Mathf.CeilToInt((count * 0.5f) / gx);
            
            shader.SetInt("count",count);
            shader.SetBuffer(sortKernel,"data",buffer.Buffer);
            shader.SetBuffer(sortSingleLevelKernel,"data",buffer.Buffer);

            int n = (int)Mathf.Pow(2, Mathf.Ceil(Mathf.Log(count, 2)));
            for (int i = 2; i <= n; i *= 2)
            {
                shader.SetInt("level",i);

                for (int j = i / 2; j > gx; j /= 2)
                {
                    shader.SetInt("single_level",j);
                    shader.Dispatch(sortSingleLevelKernel,numGroupsX,1,1);
                }
                shader.Dispatch(sortKernel,numGroupsX,1,1);
            }
        }

        private StructuredBuffer<int> CopyToPow2Buffer(ComputeBufferBase<int> buffer, int count)
        {
            int n = (int)Mathf.Pow(2, Mathf.Ceil(Mathf.Log(count, 2)));
            StructuredBuffer<int> copy = new StructuredBuffer<int>(n);

            uint gx, gy, gz;
            shader.GetKernelThreadGroupSizes(copyAndFillKernel,out gx,out gy,out gz);
            int numGroupsX = Mathf.CeilToInt((float)n/gx);

            shader.SetInt("buffer_length",n);
            shader.SetInt("count",count);
            shader.SetBuffer(copyAndFillKernel,"data",buffer.Buffer);
            shader.SetBuffer(copyAndFillKernel,"copy",copy.Buffer);
            shader.Dispatch(copyAndFillKernel,numGroupsX,1,1);

            return copy;
        }

        private void CopyToOriginalBuffer(StructuredBuffer<int> copy, ComputeBufferBase<int> original, int count)
        {
            uint gx, gy, gz;
            shader.GetKernelThreadGroupSizes(copyBackKernel,out gx,out gy,out gz);
            int numGroupsX = Mathf.CeilToInt((float)count / gx);
            
            shader.SetInt("count",count);
            shader.SetBuffer(copyBackKernel, "data", original.Buffer);
            shader.SetBuffer(copyBackKernel, "copy", copy.Buffer);
            shader.Dispatch(copyBackKernel, numGroupsX, 1, 1);
        }
    }
}