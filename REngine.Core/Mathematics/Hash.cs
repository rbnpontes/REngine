using Standart.Hash.xxHash;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Mathematics
{
	public static class Hash
	{
		public static ulong Digest(string input)
		{
			return xxHash64.ComputeHash(input);
		}
	}
}
