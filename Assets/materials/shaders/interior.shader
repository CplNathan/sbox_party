
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
	Depth();
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
	CreateInputTextureCube( Texture_ps_0, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	TextureCube g_tTexture_ps_0 < Channel( RGBA, Box( Texture_ps_0 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	TextureAttribute( LightSim_DiffuseAlbedoTexture, g_tTexture_ps_0 )
	TextureAttribute( RepresentativeTexture, g_tTexture_ps_0 )
	float g_flTiling < UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 0, 1 ); >;
	
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
		
		float3 l_0 = CalculatePositionToCameraDirWs( i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz );
		float3 l_1 = Vec3WsToTs( l_0, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );
		float3 l_2 = l_1 * float3( -1, -1, -1 );
		float3 l_3 = float3( 1, 1, 1 ) / l_2;
		float3 l_4 = abs( l_3 );
		float2 l_5 = i.vTextureCoords.xy * float2( 1, 1 );
		float l_6 = g_flTiling;
		float l_7 = round( l_6 );
		float2 l_8 = TileAndOffsetUv( l_5, float2( l_7, l_7 ), float2( 0, 0 ) );
		float2 l_9 = frac( l_8 );
		float2 l_10 = l_9 * float2( 2, 2 );
		float2 l_11 = l_10 - float2( 1, 1 );
		float l_12 = l_11.x;
		float l_13 = l_11.y;
		float3 l_14 = float3( l_12, l_13, 1 );
		float3 l_15 = l_3 * l_14;
		float3 l_16 = l_4 - l_15;
		float l_17 = l_16.x;
		float l_18 = l_16.y;
		float l_19 = min( l_17, l_18 );
		float l_20 = l_16.z;
		float l_21 = min( l_19, l_20 );
		float3 l_22 = l_2 * float3( l_21, l_21, l_21 );
		float3 l_23 = l_22 + l_14;
		float3 l_24 = float3( -1, -1, -1 );
		float3 l_25 = l_23 * l_24;
		float4 l_26 = float4( l_25, 0 ).xzyw;
		float4 l_27 = TexCubeS( g_tTexture_ps_0, g_sSampler0, l_26.xyz );
		float4 l_28 = l_27 * float4( 0.1, 0.1, 0.1, 0.1 );
		float3 l_29 = pow( 1.0 - dot( normalize( i.vNormalWs ), normalize( CalculatePositionToCameraDirWs( i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz ) ) ), 2 );
		float4 l_30 = lerp( l_27, l_28, float4( l_29, 0 ) );
		float3 l_31 = 1 - l_29;
		float l_32 = lerp( 0, 0.05, l_31.x );
		
		m.Albedo = l_30.xyz;
		m.Opacity = 1;
		m.Roughness = l_32;
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
