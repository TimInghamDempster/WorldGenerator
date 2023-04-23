#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection;
float3 CameraPos;

Texture GlobeTexture;
samplerCUBE GlobeSampler = sampler_state
{
	texture = <GlobeTexture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Mirror;
	AddressV = Mirror;
};
Texture NormalTexture;
samplerCUBE NormalSampler = sampler_state
{
	texture = <NormalTexture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Mirror;
	AddressV = Mirror;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float3 CentrePos : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float3 WorldPos : TEXCOORD0;
	float3 CentrePos : TEXCOORD1;
};

struct PixelShaderOutput
{
	float4 Color : COLOR0;
	float Depth : DEPTH0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(input.Position, WorldViewProjection);
	output.Color = input.Color;
	output.WorldPos = input.Position.xyz;
	output.CentrePos = input.CentrePos;

	return output;
}

PixelShaderOutput MainPS(VertexShaderOutput input)
{
	PixelShaderOutput output = (PixelShaderOutput)0;

	output.Color = input.Color;
	float3 eyeVec = normalize(input.WorldPos - CameraPos);
	
	float a = 1.0f;
	float b = 2.0f * dot(eyeVec, CameraPos);
	float c = dot(CameraPos, CameraPos) - 1.0f;

	float discriminant = (b* b) - (4.0f * a * c);
	clip(discriminant);

	float dist = (-b - sqrt(discriminant)) / 2.0f * a;
	float3 onSpherePoint = normalize(CameraPos + eyeVec * dist);

	float3 colour = texCUBE(GlobeSampler, onSpherePoint);
	float3 normal = texCUBE(NormalSampler, onSpherePoint);
	float3 lightdir = normalize(float3(1.0f, -0.3f, -1.0f));
	float light = 1.0f;// max(dot(normal, lightdir), 0.1f);
	output.Color.xyz = light* colour;
	output.Color.w = 1.0f;

	output.Depth = length(onSpherePoint - input.CentrePos);

	return output;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};