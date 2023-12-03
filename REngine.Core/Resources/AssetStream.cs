using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Resources
{
	public sealed class AssetStream(string assetName, Stream stream) : Stream
	{
		public event EventHandler? OnDispose;
		public string Name { get; } = assetName;

		public override void Flush()
		{
			stream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return stream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException($"Is not possible to change {nameof(AssetStream)} length.");
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException($"Is not possible to write on {nameof(AssetStream)}");
		}

		public override bool CanRead => stream.CanRead;
		public override bool CanSeek => stream.CanSeek;
		public override bool CanWrite => stream.CanWrite;
		public override long Length => stream.Length;

		public override long Position
		{
			get => stream.Position;
			set => stream.Position = value;
		}

		protected override void Dispose(bool disposing)
		{
			stream.Dispose();
			OnDispose?.Invoke(this, EventArgs.Empty);
		}
	}
}
