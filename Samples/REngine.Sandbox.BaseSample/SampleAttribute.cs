namespace REngine.Sandbox.BaseSample
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public sealed class SampleAttribute(string sampleName) : Attribute
	{
		public string SampleName { get; private set; } = sampleName;
	}
}
