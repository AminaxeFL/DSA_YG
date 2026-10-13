#ifndef DIESEL_SKY_SUN_INCLUDED
#define DIESEL_SKY_SUN_INCLUDED

float3 SkySunDir(float azimuth, float elevation)
{
    float ca = cos(elevation);

    return normalize(float3(
        ca * cos(azimuth),
        sin(elevation),
        ca * sin(azimuth)
    ));
}

float2 SkySunScreenDir(float azimuth, float elevation)
{
    float3 d = SkySunDir(azimuth, elevation);
    return float2(d.x, d.y);
}

#endif