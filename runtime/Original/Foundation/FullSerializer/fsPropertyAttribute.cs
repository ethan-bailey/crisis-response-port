using System;

namespace FullSerializer
{
    // Opt-in member marker used by the recovered original settings models.
    // This is serializer metadata, not a replacement for game settings logic.
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class fsPropertyAttribute : Attribute
    {
        public string Name { get; }

        public fsPropertyAttribute() { }

        public fsPropertyAttribute(string name)
        {
            Name = name;
        }
    }
}
