﻿using REngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{
	public interface IShaderResourceBinding : INativeObject
	{
		public void Set(ShaderTypeFlags flags, string resourceName, IGPUObject resource);
	}
}
