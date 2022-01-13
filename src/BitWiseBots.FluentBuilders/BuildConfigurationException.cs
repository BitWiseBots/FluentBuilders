using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace BitWiseBots.FluentBuilders
{	
	/// <summary>
	/// Represents an error in the configuration of the builder.
	/// Inspect the message for details on the issue and potential fixes.
	/// </summary>
	[Serializable]
	[ExcludeFromCodeCoverage]
	public sealed class BuildConfigurationException : Exception
	{
		internal BuildConfigurationException(string message) : base(message)
		{
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		private BuildConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}