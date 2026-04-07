Shader "BattleAces/FogOverlay"
{
    Properties
    {
        _FogTex("Fog Mask", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent+120"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }
        LOD 100
        ZWrite Off
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _FogTex;
            float4 _FogTex_ST;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return tex2D(_FogTex, i.uv);
            }
            ENDCG
        }
    }
    FallBack Off
}
