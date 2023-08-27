using Sparser.ComputeBufferEx;

namespace Sparser.Compact
{
    public interface IBufferCompact<T> where T : struct
    {
        void Compact(ComputeBufferBase<T> buffer, ComputeBufferBase<int> keys, int count);

        void CompactIndirect(ComputeBufferBase<T> buffer, ComputeBufferBase<int> keys, ComputeBufferBase<uint> count);
    }
}