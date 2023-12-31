﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.WorldManagement
{
	public interface IComponentSerializer
	{
		public Component Create();
		public Type GetSerializeType();
		public object OnSerialize(Component component);
		public Component OnDeserialize(object componentData);
		public void OnBeforeSerialize();
		public void OnAfterSerialize();

		public void OnBeforeDeserialize();
		public void OnAfterDeserialize();
	}
}
