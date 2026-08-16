sampler baseTexture : register(s0);

float4 sourcePalette[16];
float4 targetPalette[16];
float matchThreshold = 0.01;
int paletteSize;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR
{
    float4 color = tex2D(baseTexture, coords) * sampleColor;
    
    // Check each color in the source palette
    for (int i = 0; i < paletteSize; i++)
    {
        float3 diff = color.rgb - sourcePalette[i].rgb;
        float distance = sqrt(dot(diff, diff));
        
        // If color is close enough to this palette color, swap it
        if (distance <= matchThreshold)
        {
            color.rgb = targetPalette[i].rgb;
            break; // Stop checking once we find a match
        }
    }
    
    return color;
}

technique Technique1
{
    pass PaletteSwapPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}