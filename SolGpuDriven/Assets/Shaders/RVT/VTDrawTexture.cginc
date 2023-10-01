﻿#ifndef VIRTUAL_DRAW_TEXTURE_INCLUDED
#define VIRTUAL_DRAW_TEXTURE_INCLUDED

sampler2D _MainTex;

sampler2D _Diffuse1;
sampler2D _Diffuse2;
sampler2D _Diffuse3;
sampler2D _Diffuse4;

// Texture2D _Diffuse1;
// Texture2D _Diffuse2;
// Texture2D _Diffuse3;
// Texture2D _Diffuse4;

sampler2D _Normal1;
sampler2D _Normal2;
sampler2D _Normal3;
sampler2D _Normal4;

SamplerState sampler_Diffuse1;

float4x4 _ImageMVP;

sampler2D _Blend;
float4 _BlendTile;

float4 _TileOffset1;
float4 _TileOffset2;
float4 _TileOffset3;
float4 _TileOffset4;

sampler2D _Decal0;
float4 _DecalOffset0;

// albedo and normal of the compressed single tile
sampler2D _TileAlbedo;
sampler2D _TileNormal;

struct pixel_output
{
    float4 col0 : COLOR0;
    float4 col1 : COLOR1;
};

struct v2f_drawTex
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
};

///////////////////////////////////////////////////////////////////////////////

v2f_drawTex vert(appdata_img v)
{
    v2f_drawTex o;
    o.pos = mul(_ImageMVP, v.vertex);
    o.uv = v.texcoord;

    return o;
}

pixel_output frag(v2f_drawTex i) : SV_Target
{
    float4 blend = tex2D(_Blend, i.uv * _BlendTile.xy + _BlendTile.zw);

    int mip_level = 0;
    float2 transUv = i.uv * _TileOffset1.xy + _TileOffset1.zw;
    float4 diffuse1 = tex2Dlod(_Diffuse1, float4(transUv, 0, mip_level));
    float4 normal1 = tex2Dlod(_Normal1, float4(transUv, 0, mip_level));

    transUv = i.uv * _TileOffset2.xy + _TileOffset2.zw;
    float4 diffuse2 = tex2Dlod(_Diffuse2, float4(transUv, 0, mip_level));
    float4 normal2 = tex2Dlod(_Normal2, float4(transUv, 0, mip_level));

    transUv = i.uv * _TileOffset3.xy + _TileOffset3.zw;
    float4 diffuse3 = tex2Dlod(_Diffuse3, float4(transUv, 0, mip_level));
    float4 normal3 = tex2Dlod(_Normal3, float4(transUv, 0, mip_level));

    transUv = i.uv * _TileOffset4.xy + _TileOffset4.zw;
    float4 diffuse4 = tex2Dlod(_Diffuse4, float4(transUv, 0, mip_level));
    float4 normal4 = tex2Dlod(_Normal4, float4(transUv, 0, mip_level));

    pixel_output o;
    o.col0 = blend.r * diffuse1 + blend.g * diffuse2 + blend.b * diffuse3 + blend.a * diffuse4;
    o.col1 = blend.r * normal1 + blend.g * normal2 + blend.b * normal3 + blend.a * normal4;
    return o;
}

///////////////////////////////////////////////////////////////////////////////

v2f_drawTex decalVert01(appdata_img v)
{
    v2f_drawTex o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = v.texcoord;
    return o;
}

fixed4 decalFrag01(v2f_drawTex i) : SV_Target
{
    float2 decalUV = i.uv * _DecalOffset0.xy + _DecalOffset0.zw;
    float4 decal0 = tex2D(_Decal0, i.uv);

    float4 color = tex2D(_MainTex, i.uv);
    color.rgb = color.rgb * (1 - decal0.a) + decal0.rgb * decal0.a;
    return color;
}

v2f_drawTex decalVert02(appdata_img v)
{
    v2f_drawTex o;
    o.pos = mul(_ImageMVP, v.vertex);
    o.uv = v.texcoord;
    return o;
}

fixed4 decalFrag02(v2f_drawTex i) : SV_Target
{
    float2 decalUV = i.uv * _DecalOffset0.xy + _DecalOffset0.zw;
    float4 decal0 = tex2D(_Decal0, i.uv);
    clip(decal0.a < 0.1f);
    return decal0;
}

///////////////////////////////////////////////////////////////////////////////

// Copy Tile To VT
v2f_drawTex tileVert(appdata_img v)
{
    v2f_drawTex o;
    o.pos = v.vertex;
    o.uv = v.texcoord;

    return o;
}

pixel_output copyFrag(v2f_drawTex i) : SV_Target
{
    int mip_level = 0;
    pixel_output o;
    // invert y pls
    o.col0 = tex2Dlod(_TileAlbedo, float4(float2(i.uv.x, 1.0f - i.uv.y), 0, mip_level));
    o.col1 = tex2Dlod(_TileNormal, float4(float2(i.uv.x, 1.0f - i.uv.y), 0, mip_level));
    return o;
}

#endif
