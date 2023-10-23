cbuffer ObjectConstants
{
    float2 g_atlasSize;
    int g_reverse;
};

Texture2D<unorm float> g_input;
RWTexture2D<unorm float> g_output;

#ifndef THREAD_GROUP_SIZE
#define THREAD_GROUP_SIZE 32
#endif
#define SDF_SAMPLES 8
#define THRESHOLD 0.8f

void setValue(int2 position, float value)
{
    position.x = clamp(position.x, 0.0, g_atlasSize.x);
    position.y = clamp(position.y, 0.0, g_atlasSize.y);
    g_output[position] = clamp(value, 0.0, 1.0);
}
float getValue(int2 position)
{
    return g_input[position];
}

void doLeftToRight(uint3 threadId)
{
    int i = 0;
    
    float currValue = getValue(threadId.xy);
    
    [unroll(SDF_SAMPLES)]
    for (i = 0; i < SDF_SAMPLES; ++i)
    {
        int2 coords = int2(threadId.x + i, threadId.y);
        float value = getValue(coords);
        
        if (value >= THRESHOLD)
        {
            float d = 1 - (float(i) / float(SDF_SAMPLES));
            currValue += d;
            break;
        }
    }
        
    setValue(threadId.xy, currValue);
}

void doRightToLeft(uint3 threadId)
{
    int2 inverseCoords = int2((g_atlasSize.x - threadId.x), threadId.y);
    float inverseCurrValue = getValue(inverseCoords);
    
    int i = 0;
    
    [unroll(SDF_SAMPLES)]
    for (i = 0; i < SDF_SAMPLES; ++i)
    {
        int2 coords = int2(inverseCoords.x - i, threadId.y);
        float value = getValue(coords);
        
        if (value >= THRESHOLD)
        {
            float d = 1 - (float(i) / float(SDF_SAMPLES));
            inverseCurrValue += d;
            break;
        }
    }
    
    setValue(inverseCoords, inverseCurrValue);
}

[numthreads(1, 1, 1)]
void main(uint3 threadId : SV_DispatchThreadID)
{
    if (g_reverse == 0)
        doLeftToRight(threadId);
    else
        doRightToLeft(threadId);
}