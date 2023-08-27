using Sparser.ComputeBufferEx;

namespace Sparser.Scan
{
	public interface IBufferScan<T> where T : struct
	{
		void Scan(ComputeBufferBase<T> buffer, int count);

		void ScanIndirect(ComputeBufferBase<T> buffer, ComputeBufferBase<uint> count);
	}
}