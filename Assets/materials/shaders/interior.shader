
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
	CreateInputTextureCube( Cubemap, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Overlay, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( OverlayHeight, Srgb, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( OverlayNormal, Srgb, 8, "None", "_normal", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( OverlapARM, Srgb, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	TextureCube g_tCubemap < Channel( RGBA, Box( Cubemap ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tOverlay < Channel( RGBA, Box( Overlay ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tOverlayHeight < Channel( RGBA, Box( OverlayHeight ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tOverlayNormal < Channel( RGBA, Box( OverlayNormal ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tOverlapARM < Channel( RGBA, Box( OverlapARM ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	TextureAttribute( LightSim_DiffuseAlbedoTexture, g_tOverlayHeight )
	TextureAttribute( RepresentativeTexture, g_tOverlayHeight )
	float g_flOverlayOffset < UiGroup( ",0/,0/0" ); Default1( 0.59875226 ); Range1( 0, 1 ); >;
	
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
		float l_6 = round( 1 );
		float2 l_7 = TileAndOffsetUv( l_5, float2( l_6, l_6 ), float2( 0, 0 ) );
		float2 l_8 = frac( l_7 );
		float2 l_9 = l_8 * float2( 2, 2 );
		float2 l_10 = l_9 - float2( 1, 1 );
		float l_11 = l_10.x;
		float l_12 = l_10.y;
		float3 l_13 = float3( l_11, l_12, 1 );
		float3 l_14 = l_3 * l_13;
		float3 l_15 = l_4 - l_14;
		float l_16 = l_15.x;
		float l_17 = l_15.y;
		float l_18 = min( l_16, l_17 );
		float l_19 = l_15.z;
		float l_20 = min( l_18, l_19 );
		float3 l_21 = l_2 * float3( l_20, l_20, l_20 );
		float3 l_22 = l_21 + l_13;
		float3 l_23 = float3( -1, -1, -1 );
		float3 l_24 = l_22 * l_23;
		float4 l_25 = float4( l_24, 0 ).xzyw;
		float4 l_26 = TexCubeS( g_tCubemap, g_sSampler0, l_25.xyz );
		float4 l_27 = l_26 * float4( 0.2, 0.2, 0.2, 0.2 );
		float3 l_28 = pow( 1.0 - dot( normalize( i.vNormalWs ), normalize( CalculatePositionToCameraDirWs( i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz ) ) ), 2 );
		float4 l_29 = lerp( l_26, l_27, float4( l_28, 0 ) );
		float2 l_30 = i.vTextureCoords.xy * float2( 0.85, 0.85 );
		float2 l_31 = i.vTextureCoords.xy * float2( 0.85, 0.85 );
		float4 l_32 = Tex2DS( g_tOverlayHeight, g_sSampler0, l_31 );
		float l_33 = l_32.r / 2;
		float l_34 = l_32.r * 0.525;
		float l_35 = l_33 - l_34;
		float3 l_36 = CalculatePositionToCameraDirWs( i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz );
		float3 l_37 = Vec3WsToTs( l_36, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );
		float l_38 = l_37.x;
		float l_39 = l_37.y;
		float2 l_40 = float2( l_38, l_39);
		float l_41 = l_37.z;
		float l_42 = l_41 + 0.42;
		float2 l_43 = l_40 / float2( l_42, l_42 );
		float2 l_44 = float2( l_35, l_35 ) * l_43;
		float2 l_45 = l_30 + l_44;
		float4 l_46 = Tex2DS( g_tOverlay, g_sSampler0, l_45 );
		float l_47 = l_31.y;
		float l_48 = pow( l_47, 0.3 );
		float l_49 = g_flOverlayOffset;
		float l_50 = l_48 > l_49 ? 1 : 0;
		float4 l_51 = lerp( l_29, l_46, l_50 );
		float3 l_52 = float3( 0, 0, 1 );
		float4 l_53 = Tex2DS( g_tOverlayNormal, g_sSampler0, l_45 );
		float4 l_54 = lerp( float4( l_52, 0 ), l_53, l_50 );
		float3 l_55 = 1 - l_28;
		float l_56 = lerp( 0, 0.05, l_55.x );
		float4 l_57 = Tex2DS( g_tOverlapARM, g_sSampler0, l_45 );
		float l_58 = lerp( l_56, l_57.g, l_50 );
		float l_59 = lerp( 0, l_57.b, l_50 );
		float l_60 = lerp( 1, l_57.r, l_50 );
		
		m.Albedo = l_51.xyz;
		m.Opacity = 1;
		m.Normal = l_54.xyz;
		m.Roughness = l_58;
		m.Metalness = l_59;
		m.AmbientOcclusion = l_60;
		
		
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
