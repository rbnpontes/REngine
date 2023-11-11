using Standart.Hash.xxHash;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
		public static ulong Digest(byte[] bytes)
		{
			return xxHash64.ComputeHash(bytes);
		}

		public static ulong Combine(ulong a, ulong b)
		{
			const ulong prime1 = 11400714785074694791UL;
			const ulong prime2 = 14029467366897019727UL;

			return a * prime1 + b * prime2;
		}

		public static ulong Combine(ulong a, uint x0, uint x1 = 0)
		{
			return Combine(a, Combine(x0, x1));
		}

		public static ulong Combine(uint x0, uint x1 = 0)
		{
			return ((ulong)x0 << 32) | x1;
		}

		public static ulong Combine(ulong a, byte x0, byte x1 = 0, byte x2 = 0, byte x3 = 0)
		{
			return Combine(a, Combine(x0, x1, x2, x3));	
		}

		public static ulong Combine(byte x0, byte x1 = 0, byte x2 = 0, byte x3 = 0)
		{
			return ((ulong)x0 << 24) | ((ulong)x1 << 16) | ((ulong)x2 << 8) | x3;
		}
	}
}
