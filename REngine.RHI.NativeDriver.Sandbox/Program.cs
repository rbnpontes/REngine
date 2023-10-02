using REngine.RHI;
using REngine.RHI.DiligentDriver;
using REngine.RHI.NativeDriver;
using System.Windows.Forms;
using NativeWindow = REngine.RHI.NativeDriver.NativeWindow;

DriverFactory.OnDriverMessage += (s, e) =>
{
	Console.WriteLine($"[{e.Severity}]({e.File}:{e.Line}): {e.Message}");
};

GraphicsBackend backend = GraphicsBackend.D3D11;
var adapters = DriverFactory.GetAdapters(backend);
Form form = new Form
{
	Text = "REngine - NativeDriver",
	Name = "REngine",
	StartPosition = FormStartPosition.CenterScreen,
	Width = 500,
	Height = 500
};

ISwapChain? swapChain;
DriverFactory.Build(
	new DriverSettings { 
		Backend = backend, 
		AdapterId = adapters[0].Id,
	}, 
	new NativeWindow { Hwnd = form.Handle }, 
	new SwapChainDesc 
	{
		Size = new SwapChainSize(form.Size)
	}, 
	out swapChain
);

Application.Run(form);