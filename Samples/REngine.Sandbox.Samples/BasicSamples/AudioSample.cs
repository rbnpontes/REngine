using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using REngine.Assets;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.Resources;
using REngine.RPI;
using REngine.Sandbox.BaseSample;

namespace REngine.Sandbox.Samples.BasicSamples
{
	[Sample("Audio Sample")]
	internal class AudioSample(
		IImGuiSystem imGuiSystem,
		IAssetManager assetManager) : ISample
	{
		private IAudio? pAudio;
		private IAudio? pMemAudio;

		public void Dispose()
		{
			imGuiSystem.OnGui -= OnGui;
		}

		public IWindow? Window { get; set; }
		public void Load(IServiceProvider provider)
		{
			// Streamed Audio Asset owns FileStream
			// When Audio Asset goes to dispose
			// File stream will dispose too.
			pAudio = assetManager.GetAsset<AudioAsset>("Sounds/silent_wood_by_purrplecat.ogg").Audio;
			imGuiSystem.OnGui += OnGui;
		}

		private void OnGui(object? sender, EventArgs e)
		{
			if (pAudio is null || pAudio.IsDisposed)
				return;

			ImGui.Begin("Audio Sample");

			switch (pAudio.State)
			{
				case AudioState.Playing:
				{
					RenderLoopCheckbox();
					RenderPauseButton();
					RenderStopButton();
					RenderVolume();
					RenderPitch();
					RenderDuration();
				}
					break;
				case AudioState.Paused:
				{
					RenderLoopCheckbox(); 
					RenderPlayButton();
					RenderStopButton();
					RenderVolume();
					RenderPitch();
					RenderDuration();
				}
					break;
				case AudioState.Stopped:
				{
					RenderLoopCheckbox();
					RenderPlayButton();
					RenderDuration();
				}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			ImGui.End();
			return;

			void RenderStopButton()
			{
				if (ImGui.Button("Stop")) pAudio.Stop();
			}

			void RenderPlayButton()
			{
				if (ImGui.Button("Play")) pAudio.Play();
			}

			void RenderPauseButton()
			{
				if (ImGui.Button("Pause")) pAudio.Pause();
			}

			void RenderLoopCheckbox()
			{
				bool loop = pAudio.Loop;
				ImGui.Checkbox("Loop", ref loop);
				pAudio.Loop = loop;
			}

			void RenderVolume()
			{
				var volume = pAudio.Volume;
				ImGui.SliderFloat("Volume", ref volume, 0, 100);
				pAudio.Volume = volume;
			}

			void RenderPitch()
			{
				var pitch = pAudio.Pitch;
				ImGui.DragFloat("Pitch", ref pitch, 0.001f, 0);
				pAudio.Pitch = pitch;
			}

			void RenderDuration()
			{
				var duration = (float)pAudio.Duration.TotalSeconds;
				var offset = (float)pAudio.Offset.TotalSeconds;
				if (ImGui.SliderFloat("Time", ref offset, 0, duration))
				{
					pAudio.Offset = TimeSpan.FromSeconds(offset);
				}
			}
		}

		public void Update(IServiceProvider provider)
		{
		}
	}
}
