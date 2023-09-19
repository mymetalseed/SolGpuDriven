using System;
using Unity.Mathematics;

namespace RVT
{
    public struct FPagePayload
    {
        internal int activeFrame;
        internal bool notLoading;
        internal int2 pageCoord;
        private static readonly int2 s_InvalidTileIndex = new int2(-1, -1);
        internal bool isReady
        {
            get { return (!pageCoord.Equals(s_InvalidTileIndex)); }
        }

        public void ResetTileIndex()
        {
            pageCoord = s_InvalidTileIndex;
        }
        
        public bool Equals(in FPagePayload target)
        {
            return isReady.Equals(target.isReady) && activeFrame.Equals(target.activeFrame) && pageCoord.Equals(target.pageCoord) && notLoading.Equals(target.notLoading);
        }
        
        public override bool Equals(object target)
        {
            return Equals((FPagePayload)target);
        }
        
        public override int GetHashCode()
        {
            int hash = NativeExtensions.Combine(pageCoord.GetHashCode(), activeFrame.GetHashCode());
            hash = NativeExtensions.Combine(hash, notLoading.GetHashCode());
            hash += (isReady ? 0 : 1);
            return hash;
        }
    }
    
    //Page加载信息
    public struct FPageLoadInfo : IComparable<FPageLoadInfo>
    {
        internal int x;
        internal int y;
        internal int mipLevel;

        public FPageLoadInfo(in int x, in int y, in int mipLevel)
        {
            this.x = x;
            this.y = y;
            this.mipLevel = mipLevel;
        }

        public bool Equals(in FPageLoadInfo target)
        {
            return target.x == x && target.y == y && target.mipLevel == mipLevel;
        }

        public bool NotEquals(FPageLoadInfo target)
        {
            return target.x != x || target.y != y || target.mipLevel != mipLevel;
        }

        public override bool Equals(object target)
        {
            return Equals((FPageLoadInfo)target);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() + y.GetHashCode() + mipLevel.GetHashCode();
        }
        
        public int CompareTo(FPageLoadInfo target)
        {
            return mipLevel.CompareTo(target.mipLevel);
        }
    }
}