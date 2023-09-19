
using Unity.Mathematics;

namespace RVT
{
    public struct FPage
    {
        public bool isNull;

        //Mip等级
        public int mipLevel;
        public FRectInt rect;
        public FPagePayload payload;

        public FPage(int x, int y, int width, int height, int mipLevel, bool isNull = false)
        {
            this.rect = new FRectInt(x, y, width, height);
            this.isNull = isNull;
            this.mipLevel = mipLevel;
            this.payload = new FPagePayload();
            this.payload.pageCoord = new int2(-1, -1);
            this.payload.notLoading = true;
        }
        
        public bool Equals(in FPage Target)
        {
            return rect.Equals(Target.rect) && payload.Equals(Target.payload) && mipLevel.Equals(Target.mipLevel) && isNull.Equals(Target.isNull);
        }

        public override bool Equals(object obj)
        {
            return Equals((FPage)obj);
        }

        public override int GetHashCode()
        {
            int hash = NativeExtensions.Combine(rect.GetHashCode(), payload.GetHashCode());
            hash = NativeExtensions.Combine(hash, mipLevel.GetHashCode());
            hash += (isNull ? 0 : 1);
            return hash;
        }
    }
}