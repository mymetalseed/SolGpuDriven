#ifndef MORTON_ENCODING_COMMON

int interleave(int x) {
    x = (x | (x << 16)) & 0x030000FF;
    x = (x | (x <<  8)) & 0x0300F00F;
    x = (x | (x <<  4)) & 0x030C30C3;
    x = (x | (x <<  2)) & 0x09249249;
    return x;
}

int deinterleave(int x) {
    x &= 0x9249249;
    x = (x ^ (x >> 2)) & 0x30c30c3;
    x = (x ^ (x >> 4)) & 0x0300f00f;
    x = (x ^ (x >> 8)) & 0x30000ff;
    x = (x ^ (x >> 16)) & 0x000003ff;
    return x;
}

int morton_encode(int3 coords) {
    int x = interleave(coords.x);
    int y = interleave(coords.y);
    int z = interleave(coords.z);
    return x | (y << 1) | (z << 2);
}

int3 morton_decode(int x) {
    return int3(
        deinterleave(x),
        deinterleave(x >> 1),
        deinterleave(x >> 2)
    );
}

#endif
