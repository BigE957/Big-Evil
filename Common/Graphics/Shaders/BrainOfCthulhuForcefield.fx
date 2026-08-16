sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
texture voronoi;
sampler2D voronoiSampler = sampler_state
{
    texture = <voronoi>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

texture depthNoise;
sampler2D noiseSampler = sampler_state
{
    texture = <depthNoise>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{       
    float resolutionRatio = uScreenResolution.x / uScreenResolution.y;

    float screenScale = 1080.0 / min(uScreenResolution.x, uScreenResolution.y);

    float pixelSize = 0.002 * uZoom;
    float2 aspectCorrectedCoords = coords;
    aspectCorrectedCoords.x *= resolutionRatio;
    float2 pixelatedCoords = floor(aspectCorrectedCoords / pixelSize) * pixelSize + pixelSize * 0.5;
    pixelatedCoords.x /= resolutionRatio;

    float radius = uProgress * screenScale * resolutionRatio;

    float4 baseColor = (1, 1, 1, 1);
    baseColor.rgb = uColor;
    
    float4 edgeColor = (1, 1, 1, 1);
    edgeColor.rgb = uSecondaryColor;
    
    float2 center = (uDirection - (0.5, 0.5)) * 2;
    center.x *= resolutionRatio;
    
    float2 correctedCoords = (pixelatedCoords - (0.5, 0.5)) * 2;
    correctedCoords.x *= resolutionRatio;
    float2 correctedTrueCoords = (coords - (0.5, 0.5)) * 2;
    correctedTrueCoords.x *= resolutionRatio;
    
    float2 centerOffset = center - correctedCoords;
    float angle = atan2(centerOffset.y, centerOffset.x);
    float dist = length(centerOffset);
    float2 polarUV = float2(angle, dist);
    
    float2 voronoiUV = float2(angle / (2 * 3.14159), dist - (uTime / 4));
    float voronoiValue = tex2D(voronoiSampler, voronoiUV).r;
    
    float sin1 = sin((uTime * 9) + (angle * 8)) / 132 + 0.5;
    float sin2 = sin((-uTime * 3) + (angle * 16)) / 124 + 0.5;
    float sin3 = sin((uTime * 6) + (angle * 6)) / 116 + 0.5;
    
    float voronoiStrength = 0.025;
    
    radius += sin1 + sin2 + sin3;
    radius += voronoiValue * voronoiStrength;
    radius -= 1.5;
    
    if (dist < radius)
    {
        float normalizedDist = dist / radius;
        float distortionStrength = 0.15 * uOpacity;
        float distortionFalloff = pow(normalizedDist, 3);
        float sphericalOffset = (1.0 - normalizedDist) * distortionStrength * distortionFalloff;
        float2 distortionDirection = normalize(center - correctedCoords);
        float2 distortedCoords = coords + (distortionDirection * sphericalOffset * normalizedDist);
    
        float distDiff = (radius - dist) / radius;
        if (distDiff > 0.5)
            distDiff = 1;
        else
            distDiff *= 2;
        float l = lerp(0, pow(1 - distDiff, 5), uOpacity);
    
        float baseWave = dist / uZoom * 50 - uTime * 7;
        float angleWave = sin(angle * 12 - uTime * 4) * 0.3;
        float wave = pow(max(0, sin(baseWave + angleWave)), 3) * 0.8;
        
        float waveFade = pow(normalizedDist, 3);
        float waveIntensity = wave * waveFade * 0.7;
        
        float perspectiveScale = 1.0 + (normalizedDist * 3.0);
        float2 noiseUV = float2(
            polarUV.x / (2 * 3.14159),
            polarUV.y * (perspectiveScale / uZoom.x) / 5 - (uTime * 0.2)
        );
        float noiseValue = tex2D(noiseSampler, noiseUV).r;
    
        float noiseFade = pow(normalizedDist, 2) * 0.333; //controls overall intensity
        float depthNoise = noiseValue * noiseFade;

        float4 baseTexture = tex2D(uImage0, distortedCoords);
        float4 shieldColor = lerp(baseTexture, edgeColor, l);
        float4 depthColor = float4(depthNoise, depthNoise, depthNoise, 0);
        depthColor.rgb = lerp(edgeColor, (1, 1, 1, 1), 0.33);
        depthColor *= depthNoise;

        return shieldColor 
            + (baseColor * waveIntensity * uOpacity) 
            + depthColor * uOpacity;
    }
    else if (dist < radius + 0.01)
    {
        float4 brightEdgeColor = lerp(edgeColor, (1, 1, 1, 1), 0.4);
        return brightEdgeColor;
    }
    
    return tex2D(uImage0, coords);
}

technique Technique1
{
    pass BoCShieldPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}