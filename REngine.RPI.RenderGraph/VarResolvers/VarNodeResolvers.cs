using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.RenderGraph.VarResolvers
{
    public interface IVarValueResolver
    {
        object Resolve(string value);
    }

    class BoolVarResolver : IVarValueResolver
    {
        public object Resolve(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            bool.TryParse(value, out var result);
            return result;
        }
    }

    class IntVarResolver : IVarValueResolver
    {
        public object Resolve(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            int.TryParse(value, out var result);
            return result;
        }
    }

    class StringVarResolver : IVarValueResolver
    {
        public object Resolve(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            return value;
        }
    }

	class FloatVarResolver : IVarValueResolver
	{
		public object Resolve(string value)
		{
            if (string.IsNullOrEmpty(value))
                return 0.0f;
            float.TryParse(value, out var result);
            return result;
		}
	}

	class DoubleVarResolver : IVarValueResolver
	{
		public object Resolve(string value)
		{
            if (string.IsNullOrEmpty(value))
                return 0.0;
            double.TryParse(value, out var result);
            return result;
		}
	}
}
