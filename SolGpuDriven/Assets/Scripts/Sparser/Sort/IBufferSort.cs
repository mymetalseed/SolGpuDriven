using Sparser.ComputeBufferEx;

namespace Sparser.Sort
{
    public interface IBufferSort<T> where T : struct
    {
        void Sort(ComputeBufferBase<T> buffer, int count = 0);
        void SortIndirect(ComputeBufferBase<T> buffer, ComputeBufferBase<uint> count);
    }
}