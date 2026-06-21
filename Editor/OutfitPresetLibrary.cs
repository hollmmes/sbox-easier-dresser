using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasierDresser;

/// <summary>
/// Proje içindeki şablonları yönetir. Assets/presets/ klasörüne JSON kaydeder/okur.
/// </summary>
public static class OutfitPresetLibrary
{
	static string PresetsFolder => Path.Combine( Project.Current?.GetRootPath() ?? "", "Assets", "presets" );

	public static List<OutfitPreset> LoadAll()
	{
		var result = new List<OutfitPreset>();

		if ( !Directory.Exists( PresetsFolder ) )
			return result;

		foreach ( var file in Directory.GetFiles( PresetsFolder, "*.json" ) )
		{
			try
			{
				var json = File.ReadAllText( file );
				var preset = JsonSerializer.Deserialize<OutfitPreset>( json, OutfitPreset.JsonOptions );
				if ( preset != null )
					result.Add( preset );
			}
			catch ( Exception e )
			{
				Log.Warning( $"[EasierDresser] Şablon okunamadı: {file} — {e.Message}" );
			}
		}

		return result;
	}

	public static void Save( OutfitPreset preset )
	{
		Directory.CreateDirectory( PresetsFolder );

		var safeName = string.Concat( preset.Name.Split( Path.GetInvalidFileNameChars() ) );
		var path = Path.Combine( PresetsFolder, safeName + ".json" );
		var json = JsonSerializer.Serialize( preset, OutfitPreset.JsonOptions );
		File.WriteAllText( path, json );
	}

	public static void Delete( OutfitPreset preset )
	{
		var safeName = string.Concat( preset.Name.Split( Path.GetInvalidFileNameChars() ) );
		var path = Path.Combine( PresetsFolder, safeName + ".json" );
		if ( File.Exists( path ) )
			File.Delete( path );
	}

	public static List<string> FindClothingAssets()
	{
		var result = new List<string>();
		var rootPath = Project.Current?.GetRootPath() ?? "";

		if ( string.IsNullOrEmpty( rootPath ) )
			return result;

		foreach ( var file in Directory.GetFiles( rootPath, "*.clothing", SearchOption.AllDirectories ) )
		{
			var relative = Path.GetRelativePath( rootPath, file ).Replace( '\\', '/' );
			result.Add( relative );
		}

		return result;
	}
}