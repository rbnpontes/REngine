using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core
{
	public class EngineSettings : IMergeable<EngineSettings>
	{
		public static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "REngine");
		public static readonly string LoggerPath = Path.Combine(AppDataPath, "rengine.log");

		public void Merge(EngineSettings value)
		{
		}
	}
}
