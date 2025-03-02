
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
	CreateInputTexture2D( Texture_ps_0, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Parallax, Srgb, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Colour, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Height, Srgb, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Normal, Srgb, 8, "None", "_normal", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Roughness, Srgb, 8, "None", "_rough", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( AO, Srgb, 8, "None", "_ao", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tTexture_ps_0 < Channel( RGBA, Box( Texture_ps_0 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tParallax < Channel( RGBA, Box( Parallax ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tColour < Channel( RGBA, Box( Colour ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tHeight < Channel( RGBA, Box( Height ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tNormal < Channel( RGBA, Box( Normal ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tRoughness < Channel( RGBA, Box( Roughness ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tAO < Channel( RGBA, Box( AO ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	TextureAttribute( LightSim_DiffuseAlbedoTexture, g_tHeight )
	TextureAttribute( RepresentativeTexture, g_tHeight )
	float4 g_vWaterColour < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 0.14, 0.14, 0.09, 1.00 ); >;
	float g_flWaterHeightThreshold < UiGroup( ",0/,0/0" ); Default1( 0.25 ); Range1( 0, 1 ); >;
	float g_flWaterEdgeHardness < UiGroup( ",0/,0/0" ); Default1( 2 ); Range1( 0, 1 ); >;
	float g_flWaterBlendClamp < UiGroup( ",0/,0/0" ); Default1( 0.23017989 ); Range1( 0, 1 ); >;
	
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
		
		float2 l_0 = i.vTextureCoords.xy * float2( 1, 1 );
		float2 l_1 = i.vTextureCoords.xy * float2( 1, 1 );
		float4 l_2 = Tex2DS( g_tParallax, g_sSampler0, l_1 );
		float l_3 = l_2.r / 2;
		float l_4 = l_2.r * 0.575;
		float l_5 = l_3 - l_4;
		float3 l_6 = CalculatePositionToCameraDirWs( i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz );
		float3 l_7 = Vec3WsToTs( l_6, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );
		float l_8 = l_7.x;
		float l_9 = l_7.y;
		float2 l_10 = float2( l_8, l_9);
		float l_11 = l_7.z;
		float l_12 = l_11 + 0.42;
		float2 l_13 = l_10 / float2( l_12, l_12 );
		float2 l_14 = float2( l_5, l_5 ) * l_13;
		float2 l_15 = l_0 + l_14;
		float4 l_16 = Tex2DS( g_tTexture_ps_0, g_sSampler0, l_15 );
		float4 l_17 = g_vWaterColour;
		float4 l_18 = Tex2DS( g_tColour, g_sSampler0, l_15 );
		float4 l_19 = Tex2DS( g_tHeight, g_sSampler0, l_1 );
		float l_20 = g_flWaterHeightThreshold;
		float l_21 = l_19.r / l_20;
		float l_22 = g_flWaterEdgeHardness;
		float l_23 = pow( l_21, l_22 );
		float l_24 = saturate( l_23 );
		float4 l_25 = lerp( l_17, l_18, l_24 );
		float l_26 = 1.0f - VoronoiNoise( i.vTextureCoords.xy, 3.1415925, 0.2 );
		float4 l_27 = lerp( l_16, l_25, l_26 );
		float4 l_28 = float4( 0, 0, 1, 1 );
		float4 l_29 = Tex2DS( g_tNormal, g_sSampler0, l_15 );
		float l_30 = g_flWaterBlendClamp;
		float l_31 = l_24 < l_30 ? 0 : l_24;
		float4 l_32 = lerp( l_28, l_29, l_31 );
		float4 l_33 = Tex2DS( g_tRoughness, g_sSampler0, l_15 );
		float l_34 = 1 - l_33.r;
		float l_35 = lerp( 0, l_34, l_31 );
		float4 l_36 = Tex2DS( g_tAO, g_sSampler0, l_1 );
		float l_37 = lerp( 1, l_36.r, l_31 );
		
		m.Albedo = l_27.xyz;
		m.Opacity = 1;
		m.Normal = l_32.xyz;
		m.Roughness = l_35;
		m.Metalness = 0;
		m.AmbientOcclusion = l_37;
		
		
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
