
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
	CreateInputTexture2D( Parallax, Srgb, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Normal, Srgb, 8, "None", "_normal", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( TopNormal, Srgb, 8, "None", "_normal", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Height, Srgb, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Colour, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( TopARM, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( ARM, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tTopColor < Channel( RGBA, Box( TopColor ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tParallax < Channel( RGBA, Box( Parallax ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tNormal < Channel( RGBA, Box( Normal ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tTopNormal < Channel( RGBA, Box( TopNormal ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tHeight < Channel( RGBA, Box( Height ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tColour < Channel( RGBA, Box( Colour ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tTopARM < Channel( RGBA, Box( TopARM ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tARM < Channel( RGBA, Box( ARM ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	TextureAttribute( LightSim_DiffuseAlbedoTexture, g_tColour )
	TextureAttribute( RepresentativeTexture, g_tColour )
	float2 g_vBlendTiling < UiGroup( ",0/,0/0" ); Default2( 1,1 ); Range2( 0,0, 2,2 ); >;
	float g_flTopMaskDensity < UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 0, 1 ); >;
	float g_flParallaxHeight < UiGroup( ",0/,0/0" ); Default1( 0.6 ); Range1( 0, 1 ); >;
	float4 g_vWaterColour < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 0.14, 0.14, 0.09, 1.00 ); >;
	float g_flWaterHeightThreshold < UiGroup( ",0/,0/0" ); Default1( 0.4379501 ); Range1( 0, 1 ); >;
	float g_flWaterEdgeHardness < UiGroup( ",0/,0/0" ); Default1( 0.7589459 ); Range1( 0, 1 ); >;
	float g_flWaterBlendClamp < UiGroup( ",0/,0/0" ); Default1( 0.3977271 ); Range1( 0, 1 ); >;
	
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
		float2 l_1 = g_vBlendTiling;
		float2 l_2 = TileAndOffsetUv( l_0, l_1, float2( 0, 0 ) );
		float2 l_3 = TileAndOffsetUv( l_0, l_1, float2( 0, 0 ) );
		float4 l_4 = Tex2DS( g_tParallax, g_sSampler0, l_3 );
		float l_5 = g_flTopMaskDensity;
		float l_6 = 1.0f - VoronoiNoise( i.vTextureCoords.xy, 3.1415925, l_5 );
		float l_7 = lerp( l_4.r, 0.5, l_6 );
		float l_8 = l_7 / 2;
		float l_9 = g_flParallaxHeight;
		float l_10 = l_7 * l_9;
		float l_11 = l_8 - l_10;
		float3 l_12 = CalculatePositionToCameraDirWs( i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz );
		float3 l_13 = Vec3WsToTs( l_12, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );
		float l_14 = l_13.x;
		float l_15 = l_13.y;
		float2 l_16 = float2( l_14, l_15);
		float l_17 = l_13.z;
		float l_18 = l_17 + 0.42;
		float2 l_19 = l_16 / float2( l_18, l_18 );
		float2 l_20 = float2( l_11, l_11 ) * l_19;
		float2 l_21 = l_2 + l_20;
		float4 l_22 = Tex2DS( g_tTopColor, g_sSampler0, l_21 );
		float4 l_23 = g_vWaterColour;
		float4 l_24 = l_23 * float4( 2, 2, 2, 2 );
		float4 l_25 = Tex2DS( g_tNormal, g_sSampler0, l_21 );
		float4 l_26 = Tex2DS( g_tTopNormal, g_sSampler0, l_21 );
		float4 l_27 = Tex2DS( g_tHeight, g_sSampler0, l_3 );
		float l_28 = g_flWaterHeightThreshold;
		float l_29 = l_27.r / l_28;
		float l_30 = g_flWaterEdgeHardness;
		float l_31 = pow( l_29, l_30 );
		float l_32 = saturate( l_31 );
		float l_33 = g_flWaterBlendClamp;
		float l_34 = l_32 < l_33 ? 0 : l_32;
		float4 l_35 = lerp( l_25, l_26, l_34 );
		float3 l_36 = pow( 1.0 - dot( normalize( l_35.xyz ), normalize( CalculatePositionToCameraDirWs( i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz ) ) ), 10 );
		float4 l_37 = lerp( l_23, l_24, float4( l_36, 0 ) );
		float4 l_38 = Tex2DS( g_tColour, g_sSampler0, l_21 );
		float4 l_39 = l_37 + l_38;
		float4 l_40 = lerp( l_39, l_38, l_32 );
		float4 l_41 = lerp( l_22, l_40, l_6 );
		float4 l_42 = Tex2DS( g_tTopARM, g_sSampler0, l_21 );
		float4 l_43 = Tex2DS( g_tARM, g_sSampler0, l_21 );
		float l_44 = l_43.g * 2;
		float l_45 = lerp( l_42.g, l_44, l_34 );
		float l_46 = lerp( l_42.b, l_43.b, l_34 );
		
		m.Albedo = l_41.xyz;
		m.Opacity = 1;
		m.Normal = l_35.xyz;
		m.Roughness = l_45;
		m.Metalness = l_46;
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
