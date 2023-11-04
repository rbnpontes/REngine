using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.WorldManagement
{
	public class EntityException : Exception 
	{ 
		public EntityException(string message) : base(message) { }
	}
	public class InvalidEntityIdException : Exception 
	{ 
		public InvalidEntityIdException(string message) : base($"Invalid Id. {message}") { }
	}
}
