using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Mathematics
{
	public static class Mathf
	{
		public const double Degrees2Radians = Math.PI / 180.0;
		public const double Radians2Degrees = 1.0 / Degrees2Radians;

		public static double Lerp(double from, double to, double time) {
			return (1.0 - time) * from + time * to;
		}
		public static float Lerp(float from, float to, float time)
		{
			return (float)Lerp((double)from, (double)to, (double)time);
		}
		public static int Lerp(int from, int to, double time)
		{
			return (int)Lerp((double)from, (double)to, (double)time);
		}
		public static byte Lerp(byte from, byte to, double time)
		{
			// For a better lerp values, i convert byte values to be range in 0 ~ 1024
			// and then round byte again to be in the range of 0 ~ 255
			double a = (from / 255.0f) * 1024.0;
			double b = (to / 255.0f) * 1024.0;
			double result = (Lerp(a, b, time) / 1024.0) * 255.0;
			return (byte)result;
		}

		public static Color Lerp(Color from, Color to, double time)
		{
			return Color.FromArgb(
				Lerp(from.A, to.A, time),
				Lerp(from.R, to.R, time),
				Lerp(from.G, to.G, time),
				Lerp(from.B, to.B, time)
			);
		}

		public static Vector2 Lerp(Vector2 from, Vector2 to, double time)
		{
			return new Vector2(
				(float)Lerp(from.X, to.X, (float)time),
				(float)Lerp(from.Y, to.Y, (float)time)
			);
		}
		public static Vector3 Lerp(Vector3 from, Vector3 to, double time)
		{
			return new Vector3(
				(float)Lerp(from.X, to.X, (float)time),
				(float)Lerp(from.Y, to.Y, (float)time),
				(float)Lerp(from.Z, to.Z, (float)time)
			);
		}

		public static double InverseLerp(double from, double to, double time)
		{
			return (time - from) / (to - from);
		}
		// https://github.com/rbfx/rbfx/blob/f0096f6bef330efa89e76c343e359cea2910ce7c/Source/Urho3D/Math/MathDefs.h#L152
		public static double SmoothStep(double from, double to, double time)
		{
			time = Math.Clamp(InverseLerp(from, to, time), 0.0, 1.0);
			return time * time * (3.0 - 2.0 * time);
		}

		/// <summary>
		/// Return fractional part of value
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static double Fract(double value)
		{
			return value - Math.Floor(value);
		}
		public static float Fract(float value)
		{
			return (float)Fract((double)value);
		}
	}
}
