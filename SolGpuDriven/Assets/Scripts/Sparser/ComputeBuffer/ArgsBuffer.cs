using UnityEngine;

namespace Sparser.ComputeBufferEx
{
    public class ArgsBuffer : ComputeBufferBase<uint>
    {
        public ArgsBuffer() : base(3, ComputeBufferType.IndirectArguments)
        {
        }
    }
}