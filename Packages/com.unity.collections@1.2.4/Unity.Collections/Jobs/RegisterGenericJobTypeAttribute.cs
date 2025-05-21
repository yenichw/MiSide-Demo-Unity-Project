using System;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Jobs
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	[MovedFrom(true, "Unity.Entities", "Unity.Entities", null)]
	public class RegisterGenericJobTypeAttribute : Attribute
	{
		public Type ConcreteType;

		public RegisterGenericJobTypeAttribute(Type type)
		{
			ConcreteType = type;
		}
	}
}
