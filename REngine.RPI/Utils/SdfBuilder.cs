using REngine.Core.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.Utils
{
	public class SdfBuilder
	{
		const float SdfInfinity = 1e20f;
		public float Cutoff { get; set; } = 0.25f;
		public float Radius { get; set; } = 8;

		private Image pImg;
		public SdfBuilder(Image image) 
		{
			if (image.Components != 1)
				throw new ArgumentException("Image must contains only 1 channel or components");
			pImg = image;
		}

		// https://github.com/dy/bitmap-sdf/blob/master/index.js
		public Image Build()
		{
			uint size = Math.Max(pImg.Size.Width, pImg.Size.Height);
			float[] data = new float[pImg.Data.Length];

			int i, l;
			// Convert Byte to Float
			for(i =0; i < pImg.Data.Length; ++i)
				data[i] = pImg.Data[i] / 255.0f;

			float[] gridOuter = new float[data.Length];
			float[] gridInner = new float[data.Length];

			float[] f = new float[size];
			float[] d = new float[size];
			float[] z = new float[size + 1];
			int[] v = new int[size];

			// initialize Grid
			for(i =0, l = gridOuter.Length; i < l; ++i)
			{
				float alpha = data[i];
				if(alpha == 1.0f)
				{
					gridOuter[i] = 0;
					gridInner[i] = SdfInfinity;
				} 
				else
				{
					gridOuter[i] = alpha == 0.0f ? SdfInfinity : (float)Math.Pow(Math.Max(0, 0.5 - alpha), 2);
					gridInner[i] = alpha == 0.0f ? 0 : (float)Math.Pow(Math.Max(0, alpha - 0.5), 2);
				}
			}

			EuclideanDistance2D(
				gridOuter,
				pImg.Size.Width,
				pImg.Size.Height,
				f, d, v, z
			);
			EuclideanDistance2D(
				gridInner,
				pImg.Size.Width,
				pImg.Size.Height,
				f, d, v, z
			);

			byte[] output = new byte[pImg.Size.Width * pImg.Size.Height];
			for(i = 0, l = output.Length; i < l; ++i)
			{
				double value = Math.Min(Math.Max(1 - ((gridOuter[i] - gridInner[i]) / Radius + Cutoff), 0), 1);
				output[i] = (byte)(value * 255.0);
			}

			Image result = new();
			result.SetData(new ImageDataInfo
			{
				Components = 1,
				Size = pImg.Size,
				Data = output
			});
			return result;
		}

		private void EuclideanDistance2D(
			float[] data, 
			int width, 
			int height,
			float[] f,
			float[] d,
			int[] v,
			float[] z
		)
		{
			int y;
			int x;

			for(x = 0; x < width; ++x)
			{
				for (y = 0; y < height; ++y)
					f[y] = data[y * width + x];
				EuclideanDistance(f, d, v, z, height);
				for(y = 0; y < height; ++y)
					data[y * width + x] = d[y];
			}

			for(y =0; y < height; ++y)
			{
				for (x = 0; x < width; ++x)
					f[x] = data[y * width + x];
				EuclideanDistance(f, d, v, z, width);
				for (x = 0; x < width; ++x)
					data[y * width + x] = (float)Math.Sqrt(d[x]);
			}
		}

		private void EuclideanDistance(
			float[] f,
			float[] d,
			int[] v,
			float[] z,
			int n)
		{
			v[0] = 0;
			z[0] = -SdfInfinity;
			z[1] = SdfInfinity;

			int q, k;
			for(q = 1, k = 0; q < n; ++q)
			{
				float s = ((f[q] + q * q) - (f[v[k]] + v[k] * v[k])) / (2.0f * q - 2.0f * v[k]);
			
				while(s <= z[k])
				{
					--k;
					s = ((f[q] + q * q) - (f[v[k]] + v[k] * v[k])) / (2.0f * q - 2.0f * v[k]);
				}
				++k;
				v[k] = q;
				z[k] = s;
				z[k + 1] = SdfInfinity;
			}

			for(q = 0, k = 0; q < n; ++q)
			{
				while (z[k + 1] < q)
					++k;
				d[q] = (q - v[k]) * (q - v[k]) + f[v[k]];
			}
		}
	}
}
