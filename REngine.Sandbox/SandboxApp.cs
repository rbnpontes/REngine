using Diligent;
using Microsoft.VisualBasic.Devices;
using REngine.Assets;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.Mathematics;
using REngine.RHI;
using REngine.RPI;
using REngine.RPI.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Sandbox
{
	internal class SandboxApp : App
	{
		private SampleForm? pSampleForm;
		private ICubeRenderFeature? pCubeFeature;
		private GraphicsBackend pBackend;
		private IWindow? pWindow;
		private IEngine? pEngine;
#if RENGINE_SPRITEBATCH
		private ISpriteBatch? pSpriteBatch;
#endif
		private SpriteInstancedBatchInfo[] pSpriteInstances = new SpriteInstancedBatchInfo[20 * 20];
		public SandboxApp() : base(typeof(SandboxApp))
		{
		}

		protected override IWindow OnSetupWindow(IWindowManager windowManager)
		{
			pSampleForm = new SampleForm();
			pSampleForm.Show();
			var wnd = windowManager.Create(new WindowCreationInfo
			{
				Control = pSampleForm.GameContent
			});
			pSampleForm.GameWindow = wnd;
			return wnd;
		}
		public override void OnStart(IServiceProvider provider)
		{
			base.OnStart(provider);
			pSampleForm?.EngineStart(provider);

//#if RENGINE_SPRITEBATCH
//			pSpriteBatch = provider.Get<ISpriteBatch>();
//#endif
//			pCubeFeature = provider.Get<BasicFeaturesFactory>().CreateCubeFeature();
//			provider.Get<IRenderer>()
//				.AddFeature(pCubeFeature)
//#if RENGINE_SPRITEBATCH
//				.AddFeature(pSpriteBatch.Feature)
//#endif
//				;
//			pBackend = provider.Get<IGraphicsDriver>().Backend;
//			pWindow = provider.Get<IWindow>();
//			pEngine = provider.Get<IEngine>();
//			ImageAsset textureAsset = new ImageAsset("doge.png");
//			using(FileStream stream = new FileStream(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Assets/Textures/doge.jpg"), FileMode.Open)){
//				textureAsset.Load(stream).Wait();
//			}

//			// Setup Texture on Spritebatch
//			pSpriteBatch.SetTexture(0, textureAsset.Image);
		}

		public override void OnUpdate(IServiceProvider provider)
		{
			pSampleForm?.CurrentSample?.Update(provider);
			//// Lets draw Doge Kaleidoscope 
			//float elapsedTime = (float)(pEngine?.ElapsedTime ?? 0.0) / 1000.0f;
			//var wndSize = pWindow?.Size ?? new Size();
			//Vector2 halfSize = new Vector2(wndSize.Width / 2.0f, wndSize.Height / 2.0f);

			//var worldMatrix = Matrix4x4.CreateRotationY(elapsedTime) * Matrix4x4.CreateRotationX(-MathF.PI * 0.1f);
			//var viewMatrix = Matrix4x4.CreateTranslation(0.0f, 0.0f, 5.0f);
			//var projMatrix = CreateFoV(MathF.PI / 4.0f, wndSize.Width / (float)wndSize.Height, 0.01f, 100.0f, pBackend == GraphicsBackend.OpenGL);
			//var wvpMatrix = Matrix4x4.Transpose(worldMatrix * viewMatrix * projMatrix);

			//if(pCubeFeature != null)
			//	pCubeFeature.Transform = wvpMatrix;


			//float stagger = AnalogicTime(elapsedTime + 0.5f, 2.5f, 3);
			//float sineT = stagger * (float)Math.Sin(elapsedTime);
			//float cosT = stagger * (float)Math.Cos(elapsedTime);

			//// Draw Flickering Doge
			//pSpriteBatch?.Draw(new SpriteBatchInfo
			//{
			//	/*Batch texture slot*/
			//	TextureSlot = 0,
			//	Size = new Vector2(300) * AnalogicTime(elapsedTime, 1f, 2),
			//	Angle = elapsedTime,
			//	Anchor = new Vector2(0.5f, 0.5f),
			//	Position = halfSize + (new Vector2(cosT, sineT) * 150)
			//});
			//// Draw Colored Doge
			//pSpriteBatch?.Draw(new SpriteBatchInfo
			//{
			//	TextureSlot = 0,
			//	Angle = elapsedTime,
			//	Anchor = new Vector2(0.5f, 0.5f),
			//	Position = halfSize,
			//	Size = new Vector2(150),
			//	Color = ColorUtils.FromHSL(elapsedTime, 1, 1)
			//});
			//// Draw Instanced Doges
			//pSpriteBatch?.Draw(0, UpdateSpriteInstances(elapsedTime, wndSize));
		}
		
		//private float AnalogicTime(float t, float freq, float amplitude)
		//{
		//	t = (float)(Math.Sin(t * freq) * amplitude);
		//	t = Math.Clamp(t, -(float)Math.Round(freq), (float)Math.Round(freq));
		//	return (float)Math.Floor(t);
		//}

		//private Matrix4x4 CreateFoV(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance, bool isOpenGL)
		//{
		//	if (fieldOfView <= 0.0f || fieldOfView >= MathF.PI)
		//		throw new ArgumentOutOfRangeException(nameof(fieldOfView));

		//	if (nearPlaneDistance <= 0.0f)
		//		throw new ArgumentOutOfRangeException(nameof(nearPlaneDistance));

		//	if (farPlaneDistance <= 0.0f)
		//		throw new ArgumentOutOfRangeException(nameof(farPlaneDistance));

		//	if (nearPlaneDistance >= farPlaneDistance)
		//		throw new ArgumentOutOfRangeException(nameof(nearPlaneDistance));

		//	float yScale = 1.0f / MathF.Tan(fieldOfView * 0.5f);
		//	float xScale = yScale / aspectRatio;

		//	Matrix4x4 result = new()
		//	{
		//		M11 = xScale,
		//		M22 = yScale
		//	};

		//	if (isOpenGL)
		//	{
		//		result.M33 = (farPlaneDistance + nearPlaneDistance) / (farPlaneDistance - nearPlaneDistance);
		//		result.M43 = -2 * nearPlaneDistance * farPlaneDistance / (farPlaneDistance - nearPlaneDistance);
		//		result.M34 = 1.0f;
		//	}
		//	else
		//	{
		//		result.M33 = farPlaneDistance / (farPlaneDistance - nearPlaneDistance);
		//		result.M43 = -nearPlaneDistance * farPlaneDistance / (farPlaneDistance - nearPlaneDistance);
		//		result.M34 = 1.0f;
		//	}

		//	return result;
		//}

	}
}
