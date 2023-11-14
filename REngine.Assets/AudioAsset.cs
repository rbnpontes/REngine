using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Resources;

namespace REngine.Assets
{
	public abstract class BaseAudioAsset : IAsset
	{
		public int Size { get; private set; }
		public string Checksum { get; private set; } = string.Empty;
		public string Name { get; set; } = string.Empty;

		public IAudio? Audio { get; private set; }

		public void Dispose()
		{
			Audio?.Dispose();
			Audio = null;
			OnDispose();
			GC.SuppressFinalize(this);
		}

		public Task Save(Stream stream)
		{
			throw new NotSupportedException();
		}

		public Task Load(Stream stream)
		{
			return Task.Run(() =>
			{
				Size = (int)stream.Length;
				Audio = OnBuildAudio(stream);
			});
		}

		protected abstract IAudio OnBuildAudio(Stream stream);
		protected virtual void OnDispose(){}
	}
	public sealed class AudioAsset : BaseAudioAsset
	{
		protected override IAudio OnBuildAudio(Stream stream)
		{
			using MemoryStream memStream = new();
			stream.CopyTo(memStream);
			SFML.Audio.Music music = new(memStream.GetBuffer());
			return new AudioImpl(music);
		}
	}
	/// <summary>
	/// Streamed Audio Asset works in the same way
	/// of AudioState, but this class owns Stream
	/// object and dispose when this class dispose.
	/// This can be usefully if you don't want to load
	/// the whole file in memory.
	/// </summary>
	public sealed class StreamedAudioAsset : BaseAudioAsset
	{
		private Stream? pStream;
		protected override IAudio OnBuildAudio(Stream stream)
		{
			pStream = stream;
			SFML.Audio.Music music = new(stream);
			return new AudioImpl(music);
		}

		protected override void OnDispose()
		{
			pStream?.Dispose();
		}
	}
}
