
HEADER
{
	Description = "";
}

FEATURES
{
	#include "common/features.hlsl"
}

MODES
{
	Forward();
	Depth( S_MODE_DEPTH );
	ToolsShadingComplexity( "tools_shading_complexity.shader" );
}

COMMON
{
	#ifndef S_ALPHA_TEST
	#define S_ALPHA_TEST 0
	#endif
	#ifndef S_TRANSLUCENT
	#define S_TRANSLUCENT 0
	#endif
	
	#include "common/shared.hlsl"
	#include "procedural.hlsl"

	#define S_UV2 1
	#define CUSTOM_MATERIAL_INPUTS
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
	float4 vColor : COLOR0 < Semantic( Color ); >;
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
	float3 vPositionOs : TEXCOORD14;
	float3 vNormalOs : TEXCOORD15;
	float4 vTangentUOs_flTangentVSign : TANGENT	< Semantic( TangentU_SignV ); >;
	float4 vColor : COLOR0;
	float4 vTintColor : COLOR1;
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput v )
	{
		
		PixelInput i = ProcessVertex( v );
		i.vPositionOs = v.vPositionOs.xyz;
		i.vColor = v.vColor;
		
		ExtraShaderData_t extraShaderData = GetExtraPerInstanceShaderData( v );
		i.vTintColor = extraShaderData.vTint;
		
		VS_DecodeObjectSpaceNormalAndTangent( v, i.vNormalOs, i.vTangentUOs_flTangentVSign );
		return FinalizeVertex( i );
		
	}
}

PS
{
	#include "common/pixel.hlsl"
	
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( TopColor, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( BottomColor, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( BlendHeight, Srgb, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( BlendHeight_0, Srgb, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( TopNormal, Srgb, 8, "None", "_normal", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( BottomNormal, Srgb, 8, "None", "_normal", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( TopARM, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( BottomARM, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tTopColor < Channel( RGBA, Box( TopColor ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tBottomColor < Channel( RGBA, Box( BottomColor ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tBlendHeight < Channel( RGBA, Box( BlendHeight ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tBlendHeight_0 < Channel( RGBA, Box( BlendHeight_0 ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tTopNormal < Channel( RGBA, Box( TopNormal ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tBottomNormal < Channel( RGBA, Box( BottomNormal ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tTopARM < Channel( RGBA, Box( TopARM ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tBottomARM < Channel( RGBA, Box( BottomARM ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	TextureAttribute( LightSim_DiffuseAlbedoTexture, g_tBlendHeight_0 )
	TextureAttribute( RepresentativeTexture, g_tBlendHeight_0 )
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		
		Material m = Material::Init();
		m.Albedo = float3( 1, 1, 1 );
		m.Normal = float3( 0, 0, 1 );
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Opacity = 1;
		m.Emission = float3( 0, 0, 0 );
		m.Transmission = 0;
		
		float2 l_0 = i.vTextureCoords.xy * float2( 2, 2 );
		float4 l_1 = Tex2DS( g_tTopColor, g_sSampler0, l_0 );
		float4 l_2 = Tex2DS( g_tBottomColor, g_sSampler0, l_0 );
		float4 l_3 = Tex2DS( g_tBlendHeight, g_sSampler0, i.vTextureCoords.xy );
		float l_4 = 1 - l_3.r;
		float l_5 = l_4 * 1;
		float2 l_6 = i.vTextureCoords.xy * float2( 0.5, 0.5 );
		float4 l_7 = Tex2DS( g_tBlendHeight_0, g_sSampler0, l_6 );
		float l_8 = l_7.r > 0.5727221 ? 1 : 0;
		float l_9 = lerp( l_5, 0, l_8 );
		float2 l_10 = i.vTextureCoords.xy * float2( 1, 1 );
		float l_11 = 1.0f - VoronoiNoise( l_10, 3.1415925, 0.35670826 );
		float l_12 = lerp( 0, l_9, l_11 );
		float l_13 = saturate( l_12 );
		float4 l_14 = lerp( l_1, l_2, l_13 );
		float4 l_15 = Tex2DS( g_tTopNormal, g_sSampler0, l_0 );
		float4 l_16 = Tex2DS( g_tBottomNormal, g_sSampler0, l_0 );
		float4 l_17 = lerp( l_15, l_16, l_13 );
		float4 l_18 = Tex2DS( g_tTopARM, g_sSampler0, l_0 );
		float4 l_19 = Tex2DS( g_tBottomARM, g_sSampler0, l_0 );
		float l_20 = lerp( l_18.g, l_19.g, l_13 );
		
		m.Albedo = l_14.xyz;
		m.Opacity = 1;
		m.Normal = l_17.xyz;
		m.Roughness = l_20;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		
		
		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );
		
		// Result node takes normal as tangent space, convert it to world space now
		m.Normal = TransformNormal( m.Normal, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );
		
		// for some toolvis shit
		m.WorldTangentU = i.vTangentUWs;
		m.WorldTangentV = i.vTangentVWs;
		m.TextureCoords = i.vTextureCoords.xy;
				
		return ShadingModelStandard::Shade( i, m );
	}
}
