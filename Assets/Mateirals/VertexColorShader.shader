Shader "Unlit/VertexColorShader"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZWrite On
            ZTest LEqual
            Cull Off
            Lighting Off
            Fog { Mode Off }
            ColorMaterial AmbientAndDiffuse
            BindChannels {
                Bind "Color", color
                Bind "Vertex", vertex
            }
        }
    }
}
