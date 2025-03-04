
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
	CreateInputTexture2D( Normal, Srgb, 8, "None", "_normal", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Height, Srgb, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Colour, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Roughness, Srgb, 8, "None", "_rough", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tTexture_ps_0 < Channel( RGBA, Box( Texture_ps_0 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tParallax < Channel( RGBA, Box( Parallax ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tNormal < Channel( RGBA, Box( Normal ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tHeight < Channel( RGBA, Box( Height ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tColour < Channel( RGBA, Box( Colour ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tRoughness < Channel( RGBA, Box( Roughness ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	TextureAttribute( LightSim_DiffuseAlbedoTexture, g_tColour )
	TextureAttribute( RepresentativeTexture, g_tColour )
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
		float l_3 = 1.0f - VoronoiNoise( i.vTextureCoords.xy, 3.1415925, 2 );
		float l_4 = lerp( l_2.r, 0.5, l_3 );
		float l_5 = l_4 / 2;
		float l_6 = l_4 * 0.6;
		float l_7 = l_5 - l_6;
		float3 l_8 = CalculatePositionToCameraDirWs( i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz );
		float3 l_9 = Vec3WsToTs( l_8, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );
		float l_10 = l_9.x;
		float l_11 = l_9.y;
		float2 l_12 = float2( l_10, l_11);
		float l_13 = l_9.z;
		float l_14 = l_13 + 0.42;
		float2 l_15 = l_12 / float2( l_14, l_14 );
		float2 l_16 = float2( l_7, l_7 ) * l_15;
		float2 l_17 = l_0 + l_16;
		float4 l_18 = Tex2DS( g_tTexture_ps_0, g_sSampler0, l_17 );
		float4 l_19 = g_vWaterColour;
		float4 l_20 = l_19 * float4( 2, 2, 2, 2 );
		float4 l_21 = float4( 0, 0, 1, 1 );
		float4 l_22 = Tex2DS( g_tNormal, g_sSampler0, l_17 );
		float4 l_23 = Tex2DS( g_tHeight, g_sSampler0, l_1 );
		float l_24 = g_flWaterHeightThreshold;
		float l_25 = l_23.r / l_24;
		float l_26 = g_flWaterEdgeHardness;
		float l_27 = pow( l_25, l_26 );
		float l_28 = saturate( l_27 );
		float l_29 = g_flWaterBlendClamp;
		float l_30 = l_28 < l_29 ? 0 : l_28;
		float4 l_31 = lerp( l_21, l_22, l_30 );
		float3 l_32 = pow( 1.0 - dot( normalize( l_31.xyz ), normalize( CalculatePositionToCameraDirWs( i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz ) ) ), 10 );
		float4 l_33 = lerp( l_19, l_20, float4( l_32, 0 ) );
		float4 l_34 = Tex2DS( g_tColour, g_sSampler0, l_17 );
		float4 l_35 = l_33 + l_34;
		float4 l_36 = lerp( l_35, l_34, l_28 );
		float4 l_37 = lerp( l_18, l_36, l_3 );
		float4 l_38 = Tex2DS( g_tRoughness, g_sSampler0, l_17 );
		float l_39 = 1 - l_38.r;
		float l_40 = lerp( 0, l_39, l_30 );
		
		m.Albedo = l_37.xyz;
		m.Opacity = 1;
		m.Normal = l_31.xyz;
		m.Roughness = l_40;
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
