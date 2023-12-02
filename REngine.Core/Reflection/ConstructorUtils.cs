using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Reflection
{
	public static class ConstructorUtils
	{
		public static ConstructorInfo? FindSuitableConstructor(Type type, IEnumerable<Type> expectedParamTypes)
		{
			return type.GetConstructors()
				.Where(x =>
				{
					var parameters = x.GetParameters();
					if(parameters.Length != expectedParamTypes.Count()) 
						return false;

					for (var i = 0; i < parameters.Length; ++i)
					{
						var expectedParamType = expectedParamTypes.ElementAt(i);
						var paramType = parameters[i].ParameterType;

						if (paramType == expectedParamType)
							continue;
						if (!expectedParamType.IsAssignableTo(paramType))
							return false;
					}

					return true;
				})
				.FirstOrDefault();
		}
	}
}
