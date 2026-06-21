using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasierDresser;

/// <summary>
/// Bir kıyafet şablonunu temsil eder. JSON olarak diske kaydedilir.
/// </summary>
public class OutfitPreset
{
	[JsonPropertyName( "name" )]
	public string Name { get; set; } = "New Preset";

	[JsonPropertyName( "clothing" )]
	public List<string> ClothingPaths { get; set; } = new();

	[JsonPropertyName( "created_at" )]
	public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

	public static JsonSerializerOptions JsonOptions => new()
	{
		WriteIndented = true
	};
}