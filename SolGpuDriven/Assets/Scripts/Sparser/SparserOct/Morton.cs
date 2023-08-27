
using UnityEngine;

namespace Sparser
{
    public static class Morton
    {
        private static int Interleave(int x) {
            x = (x | (x << 16)) & 0x030000FF;
            x = (x | (x <<  8)) & 0x0300F00F;
            x = (x | (x <<  4)) & 0x030C30C3;
            x = (x | (x <<  2)) & 0x09249249;
            return x;
        }

        private static int Deinterleave(int x) {
            x &= 0x9249249;
            x = (x ^ (x >> 2)) & 0x30c30c3;
            x = (x ^ (x >> 4)) & 0x0300f00f;
            x = (x ^ (x >> 8)) & 0x30000ff;
            x = (x ^ (x >> 16)) & 0x000003ff;
            return x;
        }

        public static int Encode(Vector3 coords) {
            int x = Interleave((int)coords.x);
            int y = Interleave((int)coords.y);
            int z = Interleave((int)coords.z);
            return x | (y << 1) | (z << 2);
        }

        public static Vector3 Decode(int x) {
            return new Vector3(
                Deinterleave(x),
                Deinterleave(x >> 1),
                Deinterleave(x >> 2)
            );
        }
    }

}