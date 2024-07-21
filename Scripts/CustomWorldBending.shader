Shader "Custom/WorldBending"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BendDistance ("Bend Distance", Float) = 10
        _BendSlope ("Bend Slope", Float) = 1
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert alpha:fade vertex:vert

        sampler2D _MainTex;
        float _BendDistance;
        float _BendSlope;

        struct Input
        {
            float2 uv_MainTex;
        };

        void vert (inout appdata_full v)
        {
            float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float3 cameraPos = _WorldSpaceCameraPos;
            float3 cameraForward = -UNITY_MATRIX_V[2].xyz;
            
            float3 toVertex = worldPos.xyz - cameraPos;
            float distanceAlongCamera = dot(toVertex, cameraForward);
            
            float bendAmount = max(0, distanceAlongCamera - _BendDistance) * _BendSlope;
            worldPos.y += bendAmount;
            
            v.vertex = mul(unity_WorldToObject, worldPos);
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}