Shader "TextureChange" {
Properties {
 _Blend ("Blend", Range(0,1)) = 0.5
 _Color ("Main Color", Color) = (1,1,1,1)
 _MainTex ("Uncooked", 2D) = "white" {}
 _Texture2 ("Cooked", 2D) = "white" {}
 _BumpMap ("Normalmap", 2D) = "bump" {}
}
SubShader {
 Tags { "RenderType"="Opaque" }
 LOD 200
 CGPROGRAM
 #pragma surface surf Lambert
 sampler2D _MainTex;
 sampler2D _Texture2;
 fixed _Blend;
 fixed4 _Color;

 struct Input {
     float2 uv_MainTex;
     float2 uv_Texture2;
 };

 void surf (Input IN, inout SurfaceOutput o) {
     fixed4 a = tex2D(_MainTex, IN.uv_MainTex);
     fixed4 b = tex2D(_Texture2, IN.uv_Texture2);
     fixed4 c = lerp(a, b, _Blend) * _Color;
     o.Albedo = c.rgb;
     o.Alpha = c.a;
 }
 ENDCG
}
Fallback "Diffuse"
}
