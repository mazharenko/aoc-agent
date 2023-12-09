#pragma warning disable CA1018
using System.ComponentModel;
using JetBrains.Annotations;

namespace System.Runtime.CompilerServices
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	[UsedImplicitly]
	internal class RequiredMemberAttribute : Attribute
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[UsedImplicitly]
	internal class CompilerFeatureRequiredAttribute : Attribute
	{
		public CompilerFeatureRequiredAttribute(string name)
		{
		}
	}
	
	[EditorBrowsable(EditorBrowsableState.Never)]
	[UsedImplicitly]
	internal class IsExternalInit{}
}

namespace System.Diagnostics.CodeAnalysis
{
	[AttributeUsage(System.AttributeTargets.Constructor)]
	[UsedImplicitly]
	public sealed class SetsRequiredMembersAttribute : Attribute
	{
	}
}
#pragma warning restore CA1018
