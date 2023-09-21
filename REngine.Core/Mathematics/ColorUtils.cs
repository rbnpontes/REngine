using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Mathematics
{
	public static class ColorUtils
	{
		public static Color FromHSL(float hue, float saturation, float light, float alpha = 1.0f)
		{
			float c;
			if (light < 0.5f)
				c = (1.0f + (2.0f * light - 1.0f)) * saturation;
			else
				c = (1.0f - (2.0f * light - 1.0f)) * saturation;

			float m = light - 0.5f * c;
			return FromHCM(hue, saturation, m, alpha);
		}

		/// <summary>
		/// Calculate and set RGB values. Convenience function used by FromHSV and FromHSL to avoid code duplication.
		/// https://github.com/rbfx/rbfx/blob/78857100c74205e5dd376b623a376f4c0ba2afec/Source/Urho3D/Math/Color.cpp#L305
		/// </summary>
		/// <param name="hue"></param>
		/// <param name=""></param>
		/// <returns></returns>
		private static Color FromHCM(float hue, float c, float m, float a)
		{
			float R;
			float G;
			float B;
			float A = a;
			if (hue < 0.0f || hue > 1.0f)
				hue -= (float)Math.Floor(hue);

			float hs = hue * 6.0f;
			float x = c * (1.0f - (float)Math.Abs(hs % 2.0) - 1.0f);

			if(hs < 2.0f)
			{
				B = 0;
				if(hs < 1.0)
				{
					G = x;
					R = c;
				}
				else
				{
					G = c;
					R = x;
				}
			}
			else if(hs < 4.0f)
			{
				R = 0;
				if(hs < 3.0f)
				{
					G = c;
					B = x;
				}
				else
				{
					G = x;
					B = c;
				}
			}
			else
			{
				G = 0;
				if(hs < 5.0f)
				{
					R = x;
					B = c;
				}
				else
				{
					R = c;
					B = x;
				}
			}

			R += m;
			G += m;
			B += m;

			return Color.FromArgb(
				(byte)(A * 255.0f),
				(byte)(R * 255.0f),
				(byte)(G * 255.0f),
				(byte)(B * 255.0f)
			);
		}
	}
}
