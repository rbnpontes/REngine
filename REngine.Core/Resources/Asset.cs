using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Mathematics;

namespace REngine.Core.Resources
{
	public abstract class Asset : IDisposable, IHashable
	{
		private bool pDisposed;
		protected readonly object mSync = new();

		protected long mSize;
		protected string mName = string.Empty;

		/// <summary>
		/// Asset Data Size
		/// </summary>
		public long Size
		{
			get
			{
				lock (mSync)
					return mSize;
			}
		}

		/// <summary>
		/// Asset Name
		/// </summary>
		public string Name
		{
			get
			{
				lock(mSync)
					return mName;
			}
		}

		public void Dispose()
		{
			if(pDisposed) return;

			OnDispose();
			pDisposed = true;
		}

		/// <summary>
		/// Load IAsset data from Stream
		/// This method does not close Stream, you must close yourself
		/// </summary>
		/// <param name="stream"></param>
		/// <returns>A readable task</returns>
		public Task Load(AssetStream stream)
		{
			return Task.Run(() =>
			{
				lock(mSync)
					TryLoadAsset(stream);
			});
		}

		private void TryLoadAsset(AssetStream stream)
		{
			mSize = stream.Length;
			mName = stream.Name;

			OnLoad(stream);
		}

		protected abstract void OnLoad(AssetStream stream);
		protected abstract void OnDispose();
		public virtual ulong ToHash()
		{
			return Hash.Digest(mName);
		}
	}
}
