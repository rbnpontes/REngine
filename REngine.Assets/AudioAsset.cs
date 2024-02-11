using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Resources;

namespace REngine.Assets
{
	public abstract class BaseAudioAsset : Asset
	{
		public IAudio? Audio { get; private set; }

		protected override void OnDispose()
		{
			Audio?.Dispose();
			Audio = null;
		}
		
		protected override void OnLoad(AssetStream stream)
		{
			Audio = OnBuildAudio(stream);
		}

		protected abstract IAudio OnBuildAudio(Stream stream);
	}

	/// <summary>
	/// Simple Audio Asset
	/// This object loads audio data on memory
	/// Is fast but can increase memory usage
	/// </summary>
	public sealed class AudioAsset : BaseAudioAsset
	{
#if !WEB
		private SoundWrapperAudio? pAudio;
#endif
		protected override IAudio OnBuildAudio(Stream stream)
		{
#if WEB
			throw new NotImplementedException();
#else
			var buffer = new SFML.Audio.SoundBuffer(stream);
			var sound = new SFML.Audio.Sound(buffer);
			pAudio = new SoundWrapperAudio(sound);
			return pAudio;
#endif
		}

		protected override void OnDispose()
		{
			base.OnDispose();
#if !WEB
			pAudio?.Dispose();
#endif
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
#if !WEB
		private MusicWrapperAudio? pAudio;
#endif
		protected override IAudio OnBuildAudio(Stream stream)
		{
#if WEB
			throw new NotImplementedException();
#else
			pStream = stream;
			SFML.Audio.Music music = new(stream);
			pAudio = new MusicWrapperAudio(music);
			return pAudio;
#endif
		}

		protected override void OnDispose()
		{
			base.OnDispose();
#if !WEB
			pAudio?.Dispose();
			pStream?.Dispose();
#endif
		}
	}
}
