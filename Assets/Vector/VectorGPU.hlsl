#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	StructuredBuffer<float3x4> _Vectors;
#endif

float _Step;

void ConfigureProcedural () {
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		float3 position = _Vectors[unity_InstanceID]._m03_m13_m23;

		unity_ObjectToWorld = 0.0;
		unity_ObjectToWorld._m03_m13_m23_m33 = float4(position, 1.0);
		
		unity_ObjectToWorld._m00_m01_m02 = _Vectors[unity_InstanceID]._m00_m01_m02;
		unity_ObjectToWorld._m10_m11_m12 = _Vectors[unity_InstanceID]._m10_m11_m12;
		unity_ObjectToWorld._m20_m21_m22 = _Vectors[unity_InstanceID]._m20_m21_m22;
	#endif
}

void ShaderGraphFunction_float (float3 In, out float3 Out){
    Out = In;
}

void ShaderGraphFunction_half (half3 In, out half3 Out){
    Out = In;
}



