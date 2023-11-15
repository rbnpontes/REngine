using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.IO
{
	public abstract class ShaderStream : Stream
	{
		// ReSharper disable once InconsistentNaming
		protected Stream? mTargetStream;
		public override bool CanRead
		{
			get
			{
				AssertDispose();
				return mTargetStream?.CanRead ?? false;
			}
		}

		public override bool CanSeek
		{
			get
			{
				AssertDispose();
				return mTargetStream?.CanSeek ?? false;
			}
		}

		public override bool CanWrite
		{
			get
			{
				AssertDispose();
				return mTargetStream?.CanWrite ?? false;
			}
		}

		public override long Length
		{
			get
			{
				AssertDispose();
				return mTargetStream?.Length ?? 0;
			}
		}

		public override long Position
		{
			get
			{
				AssertDispose();
				return mTargetStream?.Position ?? 0L;
			}
			set
			{
				AssertDispose();
				if(mTargetStream != null)
					mTargetStream.Position = value;
			}
		}

		public override void Flush()
		{
			AssertDispose();
			mTargetStream?.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			AssertDispose();
			return mTargetStream?.Read(buffer, offset, count) ?? 0;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			AssertDispose();
			return mTargetStream?.Seek(offset, origin) ?? 0;
		}

		public override void SetLength(long value)
		{
			AssertDispose();
			mTargetStream?.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			AssertDispose();
			mTargetStream?.Write(buffer, offset, count);
		}

		protected override void Dispose(bool disposing)
		{
			mTargetStream?.Dispose();
			mTargetStream = null;
		}

		private void AssertDispose()
		{
			if (mTargetStream is null)
				throw new ObjectDisposedException(nameof(ShaderStream));
		}

		public abstract string GetShaderCode();
	}

	public class StreamedShaderStream : ShaderStream
	{
		public StreamedShaderStream(Stream stream)
		{
			mTargetStream = stream;
		}
		public override string GetShaderCode()
		{
			using TextReader reader = new StreamReader(mTargetStream);
			return reader.ReadToEnd();
		}
	}

	public sealed class FileShaderStream : ShaderStream
	{
		public FileShaderStream(string filePath)
		{
			mTargetStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
		}
		public override string GetShaderCode()
		{
			using TextReader reader = new StreamReader(mTargetStream);
			return reader.ReadToEnd();
		}
	}
}
