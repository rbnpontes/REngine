﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.Mathematics;
using REngine.Core.Resources;
using REngine.Core.WorldManagement;

namespace REngine.RPI.Components
{
	public sealed class TextComponent(IServiceProvider provider) : BaseSpriteComponent<TextComponent>(provider)
	{
		private readonly ITextRenderer pTextRenderer = provider.Get<ITextRenderer>();
		private TextRendererBatch? pBatch;

		private bool pDirtyProps;
		private string pText = string.Empty;
		// Text has is used to compare changes
		private ulong pTextHash = 0;
		
		private uint pFontSize;
		private float pHorizontalSpacing;
		private float pVerticalSpacing;
		private Color pColor = Color.White;
		private Font? pFont;

		public string Text
		{
			get => pText;
			set
			{
				var hash = Hash.Digest(value);
				pDirtyProps |= hash != pTextHash;

				pTextHash = hash;
				pText = value;
			}
		}
		public float HorizontalSpacing
		{
			get => pHorizontalSpacing;
			set
			{
				pDirtyProps |= Math.Abs(pHorizontalSpacing - value) > float.Epsilon;
				pHorizontalSpacing = value;
			}
		}
		public float VerticalSpacing
		{
			get => pVerticalSpacing;
			set
			{
				pDirtyProps |= Math.Abs(pVerticalSpacing - value) > float.Epsilon;
				pVerticalSpacing = value;
			}
		}
		public Color Color
		{
			get => pColor;
			set
			{
				pDirtyProps |= pColor != value;
				pColor = value;
			}
		}
		public uint FontSize
		{
			get => pFontSize;
			set
			{
				pDirtyProps |= pFontSize != value;
				pFontSize = value;
			}
		}
		public Font? Font
		{
			get => pFont;
			set
			{
				pDirtyProps |= pFont != value;
				pFont = value;
				pTextRenderer.SetFont(value);
			}
		}
		public RectangleF Bounds { get; private set; } = new();

		private void BuildBatch(Font font)
		{
			if (pFont is null)
				return;
			
			pBatch = pTextRenderer.CreateBatch(font.Name);
			UpdateBatchProps(pBatch);
		}

		private void UpdateBatchProps(TextRendererBatch batch)
		{
			batch.Lock();
			batch.Enabled = Enabled;
			batch.Position = Transform.WorldPosition;
			batch.Text = pText;
			batch.Color = pColor;
			batch.HorizontalSpacing = pHorizontalSpacing;
			batch.VerticalSpacing = pVerticalSpacing;
			batch.Size = pFontSize;
			batch.ZIndex = Transform.WorldZIndex;
			Bounds = batch.Bounds;
			batch.Unlock();

			pDirtyProps = false;
		}
		
		protected override void OnUpdate()
		{
			if (pFont is null)
				return;
			if (pBatch is null)
			{
				BuildBatch(pFont);
				return;
			}

			if (!pDirtyProps)
			{
				pBatch.Lock();
				Bounds = pBatch.Bounds;
				pBatch.Unlock();
				return;
			}
			
			UpdateBatchProps(pBatch);
		}

		protected override void OnChangeVisibility(bool value)
		{
			base.OnChangeVisibility(value);
			pDirtyProps = true;
			if(pBatch is not null)
				UpdateBatchProps(pBatch);
		}

		protected override void OnChangeTransform()
		{
			pDirtyProps = true;
		}

		protected override void OnDispose()
		{
			base.OnDispose();
			DisposableQueue.Enqueue(pBatch);
			pBatch = null;
		}
	}
}
