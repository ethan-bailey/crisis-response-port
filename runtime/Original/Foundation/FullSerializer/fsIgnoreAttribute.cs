using System;

namespace FullSerializer
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class fsIgnoreAttribute : Attribute
	{
	}
}
