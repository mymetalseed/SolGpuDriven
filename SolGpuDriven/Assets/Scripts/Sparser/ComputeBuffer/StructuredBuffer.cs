using UnityEngine;

namespace Sparser.ComputeBufferEx
{
    public class StructuredBuffer<T> : ComputeBufferBase<T> where T : struct
    {
        public StructuredBuffer(int count, int counterValue = 0) : base(count, ComputeBufferType.Default) {
        }
    }

}