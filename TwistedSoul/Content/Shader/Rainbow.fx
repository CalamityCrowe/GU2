#if OPENGL
#define PS_SHADERMODEL ps_3_0
#else
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif
// Get the colours in the current texture
sampler colourMap;
float threshold;

float4 Rainbow(float4 pos : SV_POSITION, float4 color1 : COLOR0,float2 coords : TEXCOORD0) : COLOR
{


	// Get the colour and stores it in a float4 (r,g,b,a)
	float4 colour = tex2D(colourMap, float2(coords.x, coords.y));

	colour.g = colour.g * (cos(threshold * 4 + coords.x - coords.y) + 1) / 2;
	colour.b = colour.b * (sin(threshold * 2 - coords.x - coords.y) + 1) / 2;

	
	return colour;

}
technique
{
	pass
	{
		PixelShader = compile PS_SHADERMODEL Rainbow();
	}
}