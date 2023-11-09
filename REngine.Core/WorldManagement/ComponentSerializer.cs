using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.WorldManagement
{
	public abstract class ComponentSerializer<T> : BaseComponentSerializer where T : Component
	{
		public ComponentSerializer(IServiceProvider serviceProvider) : base(serviceProvider) { }

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
