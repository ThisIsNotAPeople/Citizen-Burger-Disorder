Shader "Custom/Bumped Diffuse UV X Float No Seam"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _BumpMap ("Normalmap", 2D) = "bump" {}

        _UVShiftX ("UV Shift X", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Lambert
        #pragma target 2.0

        sampler2D _MainTex;
        sampler2D _BumpMap;

        fixed4 _Color;
        float _UVShiftX;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
        };

        void surf(Input IN, inout SurfaceOutput o)
        {
            float2 mainUV = IN.uv_MainTex;
            float2 bumpUV = IN.uv_BumpMap;

            // Сдвиг UV по X / U.
            // Без frac(), чтобы не было полосы на стыке.
            mainUV.x += _UVShiftX;
            bumpUV.x += _UVShiftX;

            fixed4 c = tex2D(_MainTex, mainUV) * _Color;

            o.Albedo = c.rgb;
            o.Alpha = c.a;
            o.Normal = UnpackNormal(tex2D(_BumpMap, bumpUV));
        }
        ENDCG
    }

    FallBack "Bumped Diffuse"
}