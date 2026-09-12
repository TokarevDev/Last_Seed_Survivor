using System;

namespace Game.Presentation.Validation
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class RequiredViewReferenceAttribute : Attribute
    {
    }
}
