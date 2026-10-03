using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public class ModelIdRunSaveConverter : JsonConverter<ModelId>
{
	public override ModelId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string @string = reader.GetString();
		if (@string != null)
		{
			return ModelId.Deserialize(@string);
		}
		return ModelId.none;
	}

	public override void Write(Utf8JsonWriter writer, ModelId value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
