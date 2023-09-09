using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.DependencyInjection
{
	internal enum ServiceConstructorType
	{
		Reflection,
		Lambda
	}
	internal class ServiceConstructor
	{
		public ServiceConstructorType ConstructorType { get; set; } = ServiceConstructorType.Reflection;
		public Type InterfaceType { get; set; }
		public Type TargetType { get; set; }
		public ActivationCall<object>? ActivationCall { get; set; }
		public IEnumerable<Type> Dependencies { get; set; } = new List<Type>();

		public ServiceConstructor(Type interfaceType,Type targetType)
		{
			InterfaceType = interfaceType;
			TargetType = targetType;
		}

		public ServiceConstructor(Type targetType, ServiceConstructorType type = ServiceConstructorType.Reflection)
		{
			InterfaceType = TargetType = targetType;
			ConstructorType = type;
		}
	}
}
