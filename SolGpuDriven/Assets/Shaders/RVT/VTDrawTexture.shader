﻿Shader "Klay/VT/DrawTexture"
{
    Properties
    {
    	_MainTex ("Texture", 2D) = "white" {}
    	
	    _Diffuse1("Diffuse1", 2D) = "white" {}
		_Normal1("Normal1", 2D) = "white" {}
    	
		_Diffuse2("Diffuse2", 2D) = "white" {}
		_Normal2("Normal2", 2D) = "white" {}
    	
		_Diffuse3("Diffuse3", 2D) = "white" {}
		_Normal3("Normal3", 2D) = "white" {}
    	
		_Diffuse4("Diffuse4", 2D) = "white" {}
		_Normal4("Normal4", 2D) = "white" {}

		_TileOffset1("TileOffset1",Vector) = (1,1,0,0)
		_TileOffset2("TileOffset2",Vector) = (1,1,0,0)
		_TileOffset3("TileOffset3",Vector) = (1,1,0,0)
		_TileOffset4("TileOffset4",Vector) = (1,1,0,0)
    	
		_Blend("Blend", 2D) = "white" {}
		_BlendTile("Blend Tile",Vector) = (0,0,100,100)
    	
    	_Decal0("Decal0", 2D) = "white" {}
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
    	
    	// 0
        Pass
        {
            CGPROGRAM
			#pragma target 3.5

            #pragma vertex tileVert
            #pragma fragment frag

            #include "UnityCG.cginc"
			#include "VTDrawTexture.cginc"
            ENDCG
        }
    	
    	// 1
    	Pass
        {
        	Tags { "LightMode" = "VTDecalRenderer" }
        	
            CGPROGRAM
			#pragma target 3.5

            #pragma vertex decalVert01
            #pragma fragment decalFrag01

            #include "UnityCG.cginc"
			#include "VTDrawTexture.cginc"
            ENDCG
        }
    	
    	// 2
    	Pass
        {
        	Tags { "LightMode" = "VTDecalRenderer" }
        	
            CGPROGRAM
			#pragma target 3.5

            #pragma vertex decalVert02
            #pragma fragment decalFrag01

            #include "UnityCG.cginc"
			#include "VTDrawTexture.cginc"
            ENDCG
        }
    	
    	// 3
    	Pass
        {
        	Tags { "LightMode" = "VTDecalRenderer" }
        	Blend SrcAlpha OneMinusSrcAlpha
        	
            CGPROGRAM
			#pragma target 3.5

            #pragma vertex decalVert02
            #pragma fragment decalFrag02

            #include "UnityCG.cginc"
			#include "VTDrawTexture.cginc"
            ENDCG
        }
    	
    	// 4
        Pass
        {
	        Tags { "LightMode" = "CopyRenderer" }
        	
	        CGPROGRAM
			#pragma target 3.5

            #pragma vertex vert
            #pragma fragment copyFrag

            #include "UnityCG.cginc"
			#include "VTDrawTexture.cginc"
            ENDCG
        }
    }
}
