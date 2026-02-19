namespace Raijin.SatSolver.Application.Events;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class EventMetadataAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}