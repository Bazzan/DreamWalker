Shader "Custom/OutlineMaskShader"
{
    Properties
    {
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent+100" "RenderType"="Transparent"
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
    }
}