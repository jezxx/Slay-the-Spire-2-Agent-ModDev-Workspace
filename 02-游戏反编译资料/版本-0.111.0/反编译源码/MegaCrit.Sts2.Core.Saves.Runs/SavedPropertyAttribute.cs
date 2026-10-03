using System;

namespace MegaCrit.Sts2.Core.Saves.Runs;

[AttributeUsage(AttributeTargets.Property)]
public class SavedPropertyAttribute : Attribute
{
	public readonly SerializationCondition defaultBehaviour;

	/// <summary>
	/// This controls the order in which properties are serialized and deserialized.
	/// It is very rarely necessary, but sometimes properties depend on each other.
	/// </summary>
	public readonly int order;

	public SavedPropertyAttribute()
	{
		defaultBehaviour = SerializationCondition.AlwaysSave;
	}

	public SavedPropertyAttribute(SerializationCondition defaultBehaviour)
	{
		this.defaultBehaviour = defaultBehaviour;
	}

	public SavedPropertyAttribute(SerializationCondition defaultBehaviour, int order)
	{
		this.defaultBehaviour = defaultBehaviour;
		this.order = order;
	}
}
