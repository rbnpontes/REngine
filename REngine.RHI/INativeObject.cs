﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{
	public interface INativeObject
	{
		public IntPtr Handle { get; }
	}
}