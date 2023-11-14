using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Resources;

namespace REngine.Assets
{
	internal class AudioImpl : IAudio
	{
		private readonly SFML.Audio.Music pMusic;
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
					value = pMusic.Duration.ToTimeSpan();
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
					value = pMusic.PlayingOffset.ToTimeSpan();
				}
				return value;
			}
			set
			{
				lock (pSync)
				{
					AssertDispose();
					var duration = pMusic.Duration.ToTimeSpan();
					var loop = pMusic.Loop;
					if (value >= duration)
					{
						pMusic.PlayingOffset = duration;
						if (!loop)
							pState = AudioState.Stopped;
					}
					else
					{
						pMusic.PlayingOffset = value;
					}
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
					value = pState;
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
					value = pMusic.Pitch;
				}
				return value;
			}
			set
			{
				lock (pSync)
				{
					AssertDispose();
					pMusic.Pitch = value;
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
					value = pMusic.Volume;
				}
				return value;
			}
			set
			{
				lock (pSync)
				{
					AssertDispose();
					pMusic.Volume = Math.Clamp(value, 0, 100);
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
					value = pMusic.Loop;
				}
				return value;
			}
			set
			{
				lock (pSync)
				{
					AssertDispose();
					pMusic.Loop = value;
				}
			}
		}

		public AudioImpl(SFML.Audio.Music music)
		{
			pMusic = music;
		}
		public void Dispose()
		{
			lock (pSync)
			{
				if (pDisposed) 
					return;
				pState = AudioState.Stopped;
				pMusic.Stop();
				pMusic.Dispose();
				pDisposed = true;
			}
		}

		public IAudio Play()
		{
			lock (pSync) 
			{
				AssertDispose();
				if (pState == AudioState.Playing)
					return this;
				pMusic.Play();
				pState = AudioState.Playing;
			}
			return this;
		}

		public IAudio Stop()
		{
			lock (pSync)
			{
				AssertDispose();
				if (pState == AudioState.Stopped)
					return this;
				pMusic.Stop();
				pState = AudioState.Stopped;
			}
			return this;
		}

		public IAudio Pause()
		{
			lock (pSync)
			{
				AssertDispose();
				if (pState is AudioState.Paused or AudioState.Stopped)
					return this;
				pMusic.Pause();
				pState = AudioState.Paused;
			}
			return this;
		}
		private void AssertDispose()
		{
			if (pDisposed)
				throw new ObjectDisposedException(nameof(IAudio));
		}
	}
}
