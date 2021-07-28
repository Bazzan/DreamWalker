Shader "Custom/FresnelShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        [MaterialToggle] _isToggled("isToggle", Float) = 0

        [HDR] _Emission ("Emission", color) = (0,0,0)

        _FresnelColor ("Fresnel Color", Color) = (1,1,1,1)
        [PowerSlider(10)] _FresnelExponent ("Fresnel Exponent", Range(0.25, 10)) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "Queue"="Geometry"
        }

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        // #pragma multi_compile FEATURE_ON FEATURE_OFF


        sampler2D _MainTex;


        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        half3 _Emission;
        float3 _FresnelColor;
        float _FresnelExponent;
        float _isToggeled;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
            float3 viewDir;
            // INTERNAL_DATA
        };


        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 col = tex2D(_MainTex, IN.uv_MainTex);
            col *= _Color;

            o.Albedo = col.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;

            float fresnel = dot(IN.worldNormal, IN.viewDir);
            fresnel = saturate(1 - fresnel);
            fresnel = pow(fresnel, _FresnelExponent);
            float3 fresnelColor = fresnel * _FresnelColor;
                o.Emission = _Emission + fresnelColor;

            // o.Albedo = col.rgb * fresnel;
        }
        ENDCG
    }
    FallBack "Diffuse"
}