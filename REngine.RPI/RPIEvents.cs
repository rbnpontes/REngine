using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	public sealed class RPIEvents
	{
		public event EventHandler? OnUpdateSettings;

		public RPIEvents ExecuteUpdateSettings(RenderSettings settings)
		{
			OnUpdateSettings?.Invoke(settings,  EventArgs.Empty);
			return this;
		}
	}
}
