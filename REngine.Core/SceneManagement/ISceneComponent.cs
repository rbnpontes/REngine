using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.SceneManagement
{
	public interface ISceneComponent : IDisposable
	{
		public int Id { get; }
		public string Name { get; }
		public bool Enabled { get; set; }
		public SceneObject? Owner { get; }

		public void Attach(SceneObject target);
		public void Detach();

		public void OnOwnerChangeVisibility(bool value);
	}
}
