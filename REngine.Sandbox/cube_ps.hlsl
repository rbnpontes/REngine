struct PSInput
{
	float4 pos		 : SV_POSITION;
	float4 color	 : COLOR0;
};

struct PSOutput
{
	float4 color	 : SV_TARGET;
};

void main(in PSInput input, out PSOutput output)
{
	output.color = input.color;
}