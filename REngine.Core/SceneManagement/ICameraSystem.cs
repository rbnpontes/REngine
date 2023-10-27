using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.SceneManagement
{
	public interface ICameraSystem
	{
		public IEnumerable<ICamera> GetAllCameras();
		public ICamera GetCameraById(uint id);
		public ICamera Build();
	}
}
