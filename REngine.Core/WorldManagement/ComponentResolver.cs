using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.WorldManagement
{
	public abstract class ComponentResolver<T> : BaseComponentResolver where T : Component
	{
		public ComponentResolver(IServiceProvider serviceProvider) : base(serviceProvider) { }

		protected override Type GetComponentType()
		{
			return typeof(T);
		}

		public override Component OnDeserialize(object componentData)
		{
			return (Component)componentData;
		}
		public override object OnSerialize(Component component)
		{
			return component;
		}
	}
}
