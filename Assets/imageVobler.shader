Shader "Hidden/imageVobler"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "White" {}
        _Speed ("Distortion Damper", Float) = 1
        _DistortionDist ("Distortion cracks", Float) = 1
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float _Speed;
            float _DistortionDist;

            fixed4 frag (v2f i) : SV_Target
            {
                float2 offset = float2(
                    tex2D(_NoiseTex, float2(_Time[1],(i.uv.y+(_Time[1])/_Speed))/_DistortionDist).r/10 ,
                    tex2D(_NoiseTex, float2((i.uv.x+(_Time[1])/_Speed),_Time[1])/_DistortionDist).r/10 
                );
                offset-=0.05;
                fixed4 col = tex2D(_MainTex, i.uv + offset);

                return col;
            }
            ENDCG
        }
    }
}
