using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Mathematics
{
	public static class MathExtensions
	{
		public static Color Lerp(this Color color, Color to, double time)
		{
			return Mathf.Lerp(color, to, time);
		}
		public static byte[] ToBytes(this Color color)
		{
			return new byte[]
			{
				color.A,
				color.R,
				color.G,
				color.B
			};
		}

		public static Vector3 ToVector3(this Color color)
		{
			return new Vector3(
				color.R / 255.0f,
				color.G / 255.0f,
				color.B / 255.0f
			);
		}
		public static Vector4 ToVector4(this Color color)
		{
			return new Vector4(
				color.R / 255.0f,
				color.G / 255.0f,
				color.B / 255.0f,
				color.A / 255.0f
			);
		}

		public static Vector4 ToVector4(this Rectangle rectangle)
		{
			return new Vector4(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
		}

		public static Color ToColor(this Vector3 color)
		{
			return Color.FromArgb(
				(byte)(color.X * 255.0f),
				(byte)(color.Y * 255.0f),
				(byte)(color.Z * 255.0f)
			);
		}
		public static Color ToColor(this Vector4 color)
		{
			return Color.FromArgb(
				(byte)(color.W * 255.0f),
				(byte)(color.X * 255.0f),
				(byte)(color.Y * 255.0f),
				(byte)(color.Z * 255.0f)
			);
		}

		public static RectangleF Merge(this RectangleF first, Rectangle second)
		{
			float left = first.Left;
			float right = first.Right;
			float top = first.Top;
			float bottom = first.Bottom;

			if (first.Width <= 0 || first.Width <= 0)
				return second;
			else if(second.Width > 0 && second.Height > 0)
			{
				if(second.Left < left)
					left = second.Left;
				if (second.Top < top)
					top = second.Top;
				if(second.Right > right)
					right = second.Right;
				if(second.Bottom > bottom)
					bottom = second.Bottom;
			}

			return RectangleF.FromLTRB(left, top, right, bottom);
		}
		
		public static RectangleF Merge(this RectangleF first, RectangleF second)
		{
			float left = first.Left;
			float right = first.Right;
			float top = first.Top;
			float bottom = first.Bottom;

			if(second.Left < left)
				left = second.Left;
			if(second.Top < top)
				top = second.Top;
			if(second.Right > right)
				right = second.Right;
			if (second.Bottom > bottom)
				bottom = second.Bottom;

			return RectangleF.FromLTRB(left, top, right, bottom);
		}
		public static RectangleF Merge(this RectangleF first, Vector2 second)
		{
			float left = first.Left;
			float right = first.Right;
			float top = first.Top;
			float bottom = first.Bottom;

			if(second.X < left)
				left = second.X;
			if(second.X > right)
				right = second.X;
			if(second.Y < top)
				top = second.Y;
			if(second.Y > bottom)
				bottom = second.Y;

			return RectangleF.FromLTRB(left, top, right, bottom);
		}
		public static RectangleF Merge(this RectangleF first, float x, float y)
		{
			return first.Merge(new Vector2(x, y));
		}
		/// <summary>
		/// Scale a Rectangle by an scale value
		/// </summary>
		/// <param name="scale"></param>
		/// <returns></returns>
		public static RectangleF Scale(this RectangleF @this, float scale)
		{
			return @this.Scale(scale, scale);
		}
		/// <summary>
		/// Scale a Rectnagle by an scale values
		/// </summary>
		/// <param name="scaleX"></param>
		/// <param name="scaleY"></param>
		/// <returns></returns>
		public static RectangleF Scale(this RectangleF @this, float scaleX, float scaleY)
		{
			return new RectangleF(
				@this.X * scaleX,
				@this.Y * scaleY,
				@this.Width * scaleX,
				@this.Height * scaleY
			);
			//return RectangleF.Inflate(@this, scaleX, scaleY);
		}

		/// <summary>
		/// Add vector to rect bounds
		/// </summary>
		/// <param name="this"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		public static RectangleF Add(this RectangleF @this, Vector2 position)
		{
			return @this.Add(position.X, position.Y);
		}
		public static RectangleF Add(this RectangleF @this, float x, float y)
		{
			return RectangleF.FromLTRB(
				@this.Left + x,
				@this.Top + y,
				@this.Right + x,
				@this.Bottom + y
			);
		}
	}
}
