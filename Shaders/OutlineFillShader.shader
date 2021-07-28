// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/OutlineShader"
{
    Properties
    {
        _OutlineColor1("Outline Color", Color) = (1,1,1,1)
        _OutlineColor2("Outline Color", Color) = (1,1,1,1)
        _ColorLerpSpeed("Color Lerp Speed",Range(0,10)) = 2.5
        _PulseSpeed("Pulse Speed",Range(0,200)) = 2.5
        _OutlineWidth("Outline Width",Range(0,15)) = 10
        _OutlineWidth2("Outline Width 2",Range(0,55)) = 10
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent+110"
            "RenderType"="Transparent"
            "DisableBatching" = "True"
        }

        Pass
        {
            Name"Mask"
            Cull off
            ZTest Always
            Zwrite off
            colorMask 0

            Stencil
            {
                ref 1
                pass Replace
            }
        }
        Pass
        {
            Name "Fill"
            Cull Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask RGB

            Stencil
            {
                ref 1
                Comp NotEqual
            }


            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float3 smoothNormal : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                fixed4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            uniform fixed4 _OutlineColor1;
            uniform fixed4 _OutlineColor2;
            float _PulseSpeed;
            float _ColorLerpSpeed;
            uniform float _OutlineWidth;
            uniform float _OutlineWidth2;

            v2f vert(appdata v)
            {
                v2f o;

                float3 viewPosition = UnityObjectToViewPos(v.vertex);
                float3 viewNormal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));

                fixed4 lerpColor = lerp(_OutlineColor1, _OutlineColor2, sin(_Time * _ColorLerpSpeed) + 1);
                float lerpedOutlineWidth = lerp(_OutlineWidth, _OutlineWidth2, sin(_Time * _PulseSpeed) + 1);

                float4 newPosition = UnityViewToClipPos(
                    viewPosition + viewNormal * -viewPosition.z * lerpedOutlineWidth / 1000.0);

                o.position = newPosition;
                o.color = lerpColor;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
    Fallback "Diffuse"

}