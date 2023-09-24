using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core
{
	public class EngineSettings : IMergeable<EngineSettings>
	{
		public string AppDataPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "REngine");

		public void Merge(EngineSettings value)
		{
			AppDataPath = value.AppDataPath;
		}
	}
}
