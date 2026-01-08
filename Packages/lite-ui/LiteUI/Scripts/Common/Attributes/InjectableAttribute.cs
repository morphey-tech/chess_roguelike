using System;
using JetBrains.Annotations;

namespace LiteUI.Common.Attributes
{
    [PublicAPI, MeansImplicitUse(ImplicitUseKindFlags.Assign)]
    public sealed class InjectableAttribute : Attribute
    {
    }
}
