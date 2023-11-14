using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Resources
{
	public enum AudioState
	{
		Playing,
		Paused,
		Stopped
	}
	public interface IAudio : IDisposable
	{
		public bool IsDisposed { get; }
		public TimeSpan Duration { get; }
		public TimeSpan Offset { get; set; }
		public AudioState State { get; }
		public float Pitch { get; set; }
		public float Volume { get; set; }
		public bool Loop { get; set; }
		public IAudio Play();
		public IAudio Stop();
		public IAudio Pause();
	}
}
