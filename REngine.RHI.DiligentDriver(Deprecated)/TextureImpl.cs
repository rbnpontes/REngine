﻿using REngine.RHI.DiligentDriver.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver
{
	internal class TextureImpl : GPUObjectImpl, ITexture
	{
		private readonly ITextureView?[] pTextureViews = new ITextureView[(int)TextureViewType.ShadingRate + 1];
		public TextureDesc Desc
		{
			get
			{
				var texture = GetHandle<Diligent.ITexture>();
				var adapter = new TextureAdapter();

				adapter.Fill(texture.GetDesc(), out TextureDesc desc);
				return desc;
			}
		}

		public override string Name => GetHandle<Diligent.ITexture>().GetDesc().Name;

		public TextureImpl(Diligent.ITexture texture) : base(texture)
		{
			texture.SetUserData(new ObjectWrapper(this));
		}

		public ITextureView GetDefaultView(TextureViewType view)
		{
			var result = pTextureViews[(int)view];
			if (result == null)
			{
				pTextureViews[(int)view] = result = new TextureViewImpl(this, GetHandle<Diligent.ITexture>().GetDefaultView((Diligent.TextureViewType)view));
				result.OnDispose += HandleTextureViewDispose;
			}

			return result;
		}

		private void HandleTextureViewDispose(object sender, EventArgs e)
		{
			ITextureView? tex = sender as ITextureView;
			if (tex == null)
				return;

			tex.OnDispose -= HandleTextureViewDispose;
			pTextureViews[(int)tex.ViewType] = null;
		}
	}
}