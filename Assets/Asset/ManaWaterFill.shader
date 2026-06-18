Shader "CardGame/UI/ManaWaterFill"
{
    Properties
    {
        _MainTex     ("Sprite Texture", 2D) = "white" {}
        _Color       ("Water Color", Color) = (0.11, 0.62, 0.46, 1)
        _BgColor     ("Empty/Background Color", Color) = (0.1, 0.1, 0.1, 0.5)
        _FillAmount  ("Fill Amount", Range(0,1)) = 0.5
        _WaveHeight  ("Wave Height", Range(0, 0.1)) = 0.025
        _WaveSpeed   ("Wave Speed", Range(0, 10)) = 2
        _WaveFreq    ("Wave Frequency", Range(0, 50)) = 12
        _CircleEdge  ("Circle Edge Softness", Range(0.001, 0.05)) = 0.01
    }

    SubShader
    {
        Tags
        {
            "Queue"            = "Transparent"
            "IgnoreProjector"  = "True"
            "RenderType"       = "Transparent"
            "PreviewType"      = "Plane"
            "CanUseSpriteAtlas"= "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv     : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            fixed4    _Color;
            fixed4    _BgColor;
            float     _FillAmount;
            float     _WaveHeight;
            float     _WaveSpeed;
            float     _WaveFreq;
            float     _CircleEdge;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 centered       = i.uv - float2(0.5, 0.5);
                float  distFromCenter = length(centered);

                float circleMask = 1.0 - smoothstep(0.5 - _CircleEdge, 0.5, distFromCenter);

                if (circleMask <= 0.001)
                    return fixed4(0, 0, 0, 0);

                float wave       = sin((i.uv.x * _WaveFreq) + (_Time.y * _WaveSpeed)) * _WaveHeight;
                float waterLevel = _FillAmount + wave;

                fixed4 col = (i.uv.y > waterLevel) ? _BgColor : _Color;
                col.a *= circleMask;

                return col;
            }
            ENDCG
        }
    }
}
