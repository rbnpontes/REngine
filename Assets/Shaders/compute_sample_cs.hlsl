#ifndef GRID_SIZE
#define GRID_SIZE 100
#endif

RWBuffer<float3> g_input;

#ifndef THREAD_GROUP_SIZE
#define THREAD_GROUP_SIZE 32
#endif

[numthreads(THREAD_GROUP_SIZE, THREAD_GROUP_SIZE, 1)]
void main(uint3 threadId : SV_DispatchThreadID)
{

    // float3x3 transform;
    //
    // // position
    // transform[0][0] = 1;
    // transform[0][1] = 1;
    // transform[0][2] = 0;
    // // rotation and anchor x
    // transform[1][0] = 1;
    // transform[1][1] = 1;
    // transform[1][2] = 0;
    // // scale and anchor y
    // transform[2][0] = 100;
    // transform[2][1] = 100;
    // transform[2][2] = 0;
    g_input[0] = float3(0, 0, 0);
}