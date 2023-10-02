namespace REngine.RHI.NativeDriver.Tests
{
	[TestFixture]
	public class DriverTests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test, Sequential]
		public void MustGetAdapters(
			[Values(
#if WINDOWS
				GraphicsBackend.D3D11,
				GraphicsBackend.D3D12,
#endif
				GraphicsBackend.Vulkan,
				GraphicsBackend.OpenGL
			)] GraphicsBackend backend)
		{
			var adapters = DriverFactory.GetAdapters(GraphicsBackend.D3D11);
			Assert.Greater(adapters.Length, 0);

			Console.WriteLine($"Adapters({backend}):");
			Console.WriteLine("- Id, Name, VendorId");
			foreach (var adapter in adapters)
			{
				Assert.IsNotEmpty(adapter.Name);
				Console.WriteLine($"- {adapter.Id}, {adapter.Name}, {adapter.VendorId}");
			}

			Assert.Pass();
		}
	}
}