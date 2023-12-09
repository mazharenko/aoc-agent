using System;
using JetBrains.Annotations;

namespace mazharenko.AoCAgent.Generator;

[AttributeUsage(AttributeTargets.Class)]
[PublicAPI]
public class ManualInterpretationAttribute : Attribute
{
	
}