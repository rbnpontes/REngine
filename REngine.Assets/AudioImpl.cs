using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.IO;
using REngine.Core.Resources;
using SFML.Audio;

namespace REngine.Assets
{
	internal abstract class BaseAudio : IAudio
	{
#if PROFILER
		private const string PlaySignature = $"{nameof(IAudio)}.{nameof(Play)}";
		private const string StopSignature = $"{nameof(IAudio)}.{nameof(Stop)}";
		private const string PauseSignature = $"{nameof(IAudio)}.{nameof(Pause)}";
#endif
		private readonly object pSync = new();

		private bool pDisposed;
		public bool IsDisposed
		{
			get
			{
				bool value;
				lock (pSync)
					value = pDisposed;
				return value;
			}
		}
		public TimeSpan Duration
		{
			get
			{
				TimeSpan value;
				lock (pSync)
				{
					AssertDispose();
					value = OnGetDuration();
				}
				return value;
			}
		}
		public TimeSpan Offset
		{
			get
			{
				TimeSpan value;
				lock (pSync)
				{
					AssertDispose();
					value = OnGetOffset();
				}
				return value;
			}
			set
			{
				lock (pSync)
				{
					AssertDispose();
					var duration = OnGetDuration();
					var loop = OnGetLoop();
					if (value >= duration)
					{
						OnSetOffset(duration);
						if (!loop)
							pState = AudioState.Stopped;
					}
					else
						OnSetOffset(value);
				}
			}
		}

		private AudioState pState = AudioState.Stopped;

		public AudioState State
		{
			get
			{
				AudioState value;
				lock (pSync)
				{
					AssertDispose();
					value = OnGetState();
				}
				return value;
			}
		}
		public float Pitch
		{
			get
			{
				float value;
				lock (pSync)
				{
					AssertDispose();
					value = OnGetPitch();
				}
				return value;
			}
			set
			{
				lock (pSync)
				{
					AssertDispose();
					OnSetPitch(value);
				}
			}
		}
		public float Volume
		{
			get
			{
				float value;
				lock (pSync)
				{
					AssertDispose();
					value = OnGetVolume();
				}
				return value;
			}
			set
			{
				lock (pSync)
				{
					AssertDispose();
					OnSetVolume(Math.Clamp(value, 0, 100));
				}
			}
		}
		public bool Loop
		{
			get
			{
				bool value;
				lock (pSync)
				{
					AssertDispose();
					value = OnGetLoop();
				}
				return value;
			}
			set
			{
				lock (pSync)
				{
					AssertDispose();
					OnSetLoop(value);
				}
			}
		}

		public void Dispose()
		{
			lock (pSync)
			{
				if (pDisposed)
					return;
				pState = AudioState.Stopped;
				OnStop();
				OnDispose();
				pDisposed = true;
			}
		}

		public IAudio Play(bool force = false)
		{
#if PROFILER
			using (Profiler.Instance.Begin(PlaySignature))
			{
#endif
				var state = State;
				lock (pSync)
				{
					AssertDispose();
					if (force)
					{
						if (state != AudioState.Stopped)
							OnStop();
						OnPlay();
						pState = AudioState.Playing;
						return this;
					}

					if (pState == AudioState.Playing)
						return this;
					OnPlay();
					pState = AudioState.Playing;
				}
#if PROFILER
			}
#endif
			return this;
		}

		public IAudio Stop()
		{
#if PROFILER
			using (Profiler.Instance.Begin(StopSignature))
			{
#endif
				lock (pSync)
				{
					AssertDispose();
					var state = OnGetState();
					if (state == AudioState.Stopped)
						return this;
					OnStop();
					pState = AudioState.Stopped;
				}
#if PROFILER
			}
#endif
			return this;
		}

		public IAudio Pause()
		{
#if PROFILER
			using (Profiler.Instance.Begin(PauseSignature))
			{
#endif
				lock (pSync)
				{
					AssertDispose();
					var state = OnGetState();
					if (state is AudioState.Paused or AudioState.Stopped)
						return this;
					OnPause();
					pState = AudioState.Paused;
				}
#if PROFILER
			}
#endif
			return this;
		}
		protected void AssertDispose()
		{
			if (!pDisposed) return;
			throw new ObjectDisposedException(nameof(IAudio));
		}

		protected virtual AudioState OnGetState()
		{
			return pState;
		}

		protected abstract void OnDispose();
		protected abstract void OnPlay();
		protected abstract void OnStop();
		protected abstract void OnPause();
		protected abstract TimeSpan OnGetDuration();
		protected abstract TimeSpan OnGetOffset();
		protected abstract void OnSetOffset(TimeSpan offset);
		protected abstract float OnGetVolume();
		protected abstract void OnSetVolume(float volume);
		protected abstract float OnGetPitch();
		protected abstract void OnSetPitch(float pitch);
		protected abstract bool OnGetLoop();
		protected abstract void OnSetLoop(bool loop);
	}

	internal class MusicWrapperAudio(SFML.Audio.Music music) : BaseAudio
	{
		protected override AudioState OnGetState()
		{
			var state = base.OnGetState();
			if (state == AudioState.Playing && music.Status == SoundStatus.Stopped)
				return AudioState.Stopped;
			return state;
		}

		protected override void OnDispose()
		{
			music.Dispose();
		}

		protected override void OnPlay()
		{
			music.Play();
		}

		protected override void OnStop()
		{
			music.Stop();
		}

		protected override void OnPause()
		{
			music.Pause();
		}

		protected override TimeSpan OnGetDuration()
		{
			return music.Duration.ToTimeSpan();
		}

		protected override TimeSpan OnGetOffset()
		{
			return music.PlayingOffset.ToTimeSpan();
		}

		protected override void OnSetOffset(TimeSpan offset)
		{
			music.PlayingOffset = offset;
		}

		protected override float OnGetVolume()
		{
			return music.Volume;
		}

		protected override void OnSetVolume(float volume)
		{
			music.Volume = volume;
		}

		protected override float OnGetPitch()
		{
			return music.Pitch;
		}

		protected override void OnSetPitch(float pitch)
		{
			music.Pitch = pitch;
		}

		protected override bool OnGetLoop()
		{
			return music.Loop;
		}

		protected override void OnSetLoop(bool loop)
		{
			music.Loop = loop;
		}
	}

	internal class SoundWrapperAudio(Sound sound) : BaseAudio
	{
		protected override AudioState OnGetState()
		{
			var state = base.OnGetState();
			if(state == AudioState.Playing && sound.Status == SoundStatus.Stopped)
				return AudioState.Stopped;
			return state;
		}

		protected override void OnDispose()
		{
			sound.Dispose();
		}

		protected override void OnPlay()
		{
			sound.Play();
		}

		protected override void OnStop()
		{
			sound.Stop();
		}

		protected override void OnPause()
		{
			sound.Pause();
		}

		protected override TimeSpan OnGetDuration()
		{
			return sound.SoundBuffer.Duration.ToTimeSpan();
		}

		protected override TimeSpan OnGetOffset()
		{
			return sound.PlayingOffset.ToTimeSpan();
		}

		protected override void OnSetOffset(TimeSpan offset)
		{
			sound.PlayingOffset = offset;
		}

		protected override float OnGetVolume()
		{
			return sound.Volume;
		}

		protected override void OnSetVolume(float volume)
		{
			sound.Volume = volume;
		}

		protected override float OnGetPitch()
		{
			return sound.Pitch;
		}

		protected override void OnSetPitch(float pitch)
		{
			sound.Pitch = pitch;
		}

		protected override bool OnGetLoop()
		{
			return sound.Loop;
		}

		protected override void OnSetLoop(bool loop)
		{
			sound.Loop = loop;
		}
	}
}
