Shader "Hidden/BlackBarShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AmountOfScreen ("Amount of screen", Range(0, 1)) = 0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _AmountOfScreen;

            fixed4 frag (v2f_img i) : SV_Target
            {
                fixed4 col;
                
                if(i.uv.y < _AmountOfScreen * 0.5f || i.uv.y > 1 - _AmountOfScreen * 0.5f)
                {
                    col = fixed4(0,0,0,1);
                }
                else
                {
                    col = tex2D(_MainTex, i.uv);
                }
                
                return col;
            }
            ENDCG
        }
    }
}
