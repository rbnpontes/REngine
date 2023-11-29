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

	/// <summary>
	/// Simple Audio Asset
	/// This object loads audio data on memory
	/// Is fast but can increase memory usage
	/// </summary>
	public sealed class AudioAsset : BaseAudioAsset
	{
		private SoundWrapperAudio? pAudio;
		protected override IAudio OnBuildAudio(Stream stream)
		{
			using var memStream = new MemoryStream();
			stream.CopyTo(memStream);
			var buffer = new SFML.Audio.SoundBuffer(memStream.ToArray());
			var sound = new SFML.Audio.Sound(buffer);
			pAudio = new SoundWrapperAudio(sound);
			return pAudio;
		}

		protected override void OnDispose()
		{
			pAudio?.Dispose();
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
		private MusicWrapperAudio? pAudio;
		protected override IAudio OnBuildAudio(Stream stream)
		{
			pStream = stream;
			SFML.Audio.Music music = new(stream);
			pAudio = new MusicWrapperAudio(music);
			return pAudio;
		}

		protected override void OnDispose()
		{
			pAudio?.Dispose();
			pStream?.Dispose();
		}
	}
}
