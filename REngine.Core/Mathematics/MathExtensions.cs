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

		public static Vector2 ToVector2(this Size size)
		{
			return new Vector2(size.Width, size.Height);
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

		public static Vector4 ToVector4(this Vector3 vec3)
		{
			return new Vector4(vec3, 0);
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

		/// <summary>
		/// Convert Quaternion to Euler Angles
		/// </summary>
		/// <param name="rot"></param>
		/// <returns></returns>
		public static Vector3 ToEulerAngles(this Quaternion rot)
		{
			// https://github.com/rbfx/rbfx/blob/dfe7bcc39ea7af92fc03471ecf2d30b3e044eb86/Source/Urho3D/Math/Quaternion.cpp#L190
			
			float check = 2.0f * (-rot.Y * rot.Z + rot.W * rot.X);
			float singularityThreshold = 0.999999f;

			float yy = rot.Y * rot.Y;
			float zz = rot.Z * rot.Z;
			float xx = rot.X * rot.X;
			float unit = rot.W * rot.W + xx + yy + zz;

			if(Math.Abs(check) > singularityThreshold * unit)
			{
				return new Vector3(
					(float)(90.0 * Math.Sign(check)),
					(float)(-Math.Atan2(2.0 * (rot.X * rot.Z - rot.W * rot.Y), 1.0 - 2.0 * (yy + zz)) * Mathf.Radians2Degrees),
					.0f
				);
			}

			return new Vector3(
				(float)(Math.Asin(check) * Mathf.Radians2Degrees),
				(float)(Math.Atan2(2.0 * (rot.X * rot.Z + rot.W * rot.Y), 1.0 - 2.0 * (rot.X * rot.X + yy)) * Mathf.Radians2Degrees),
				(float)(Math.Atan2(2.0 * (rot.X * rot.Y + rot.W * rot.Z), 1.0 - 2.0 * (rot.X * rot.X + zz)) * Mathf.Radians2Degrees) 
			);
		}
		/// <summary>
		/// Convert Vector3 Euler Angles to Quaternion
		/// </summary>
		/// <param name="rot"></param>
		/// <returns></returns>
		public static Quaternion FromEulerAngles(this Vector3 rot)
		{
			// https://github.com/rbfx/rbfx/blob/dfe7bcc39ea7af92fc03471ecf2d30b3e044eb86/Source/Urho3D/Math/Quaternion.cpp#L51
			var (sinX, cosX) = Math.SinCos(rot.X * 0.5);
			var (sinY, cosY) = Math.SinCos(rot.Y * 0.5);
			var (sinZ, cosZ) = Math.SinCos(rot.Z * 0.5);

			return new Quaternion(
				(float)(cosY * sinX * cosZ + sinY * cosX * sinZ),
				(float)(sinY * cosX * cosZ - cosY * sinX * sinZ),
				(float)(cosY * cosX * sinZ - sinY * sinX * cosZ),
				(float)(cosY * cosX * cosZ + sinY * sinX * sinZ)
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
		public static Vector2 GetPosition(this RectangleF @this)
		{
			return new Vector2(@this.X, @this.Y);
		}
		public static SizeF GetSize(this RectangleF @this)
		{
			return new SizeF(@this.Width, @this.Height);
		}
	}
}
