// Battle Aces ClassicDuel 데모 무대 — 경량 풀스크린 톤(비네팅 + 살짝 대비)
// Built-in 파이프라인 Camera.OnRenderImage 용
Shader "Hidden/BattleAcesDemoStageTone"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);
                float2 d = i.uv - 0.5;
                float vig = 1.0 - dot(d, d) * 0.72;
                vig = saturate(vig);
                c.rgb = lerp(c.rgb * 0.86, c.rgb, vig);
                c.rgb = c.rgb * 1.035 - 0.012;
                return saturate(c);
            }
            ENDCG
        }
    }
    FallBack Off
}
