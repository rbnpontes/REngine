using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.WorldManagement
{
	public abstract class BaseComponentSerializer : IComponentSerializer
	{
		protected IServiceProvider mServiceProvider;
		public BaseComponentSerializer(IServiceProvider serviceProvider) 
		{
			mServiceProvider = serviceProvider;
		}

		public abstract Component Create();

		public abstract Type GetSerializeType();

		public virtual void OnAfterSerialize()
		{
		}

		public void OnBeforeSerialize()
		{
		}

		public virtual void OnAfterDeserialize()
		{
		}
		public virtual void OnBeforeDeserialize()
		{
		}

		public abstract Component OnDeserialize(object componentData);

		public abstract object OnSerialize(Component component);

		protected abstract Type GetComponentType();
	}

	public sealed class DefaultComponentSerializer : BaseComponentSerializer
	{
		private Type? pTargetComponent;

		public DefaultComponentSerializer(IServiceProvider provider) : base(provider)
		{
		}

		public override Component Create()
		{
			Type type = GetComponentType();
			object[] paramValues = Array.Empty<object>();
			// Find Suitable Constructor
			var ctor = type.GetConstructors().Where(ctorInfo =>
			{
				var parameters = ctorInfo.GetParameters();
				var currParamValues = new object[parameters.Length];
				for (int i = 0; i < parameters.Length; ++i)
				{
					var targetValue = mServiceProvider.GetService(parameters[i].ParameterType);
					if (targetValue is null)
						return false;
					currParamValues[i] = targetValue;
				}

				paramValues = currParamValues;
				return true;
			}).FirstOrDefault();

			if (ctor is null)
				throw new NullReferenceException($"Cannot found suitable Constructor to create Component '{type.Name}'");

			Component? result = Activator.CreateInstance(type, paramValues) as Component;
			if (result is null)
				throw new NullReferenceException($"Error has occurred while is creating component type '{nameof(type.Name)}'");
			return result;
		}

		protected override Type GetComponentType()
		{
			if (pTargetComponent is null)
				throw new NullReferenceException("Target Component is null, Did you set component type ?");
			return pTargetComponent;
		}

		public void SetComponentType(Type componentType)
		{
			if (!componentType.IsAssignableTo(typeof(Component)))
				throw new Exception($"Invalid Component Type. Type must inherit {nameof(Component)} type.");
			pTargetComponent = componentType;
		}

		public override Type GetSerializeType()
		{
			return GetComponentType();
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
