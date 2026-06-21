using System.Linq;

namespace EasierDresser;

[CustomEditor( typeof(Dresser) )]
public class DresserInspector : Widget
{
	SerializedObject _so;

	public DresserInspector( SerializedObject so ) : base( null )
	{
		_so = so;
		Layout = Layout.Column();
		Layout.Spacing = 4;
		BuildUI();
	}

	void BuildUI()
	{
		// Varsayılan inspector alanlarını çiz
		var sheet = new ControlSheet();
		foreach ( var prop in _so )
			sheet.AddRow( prop );
		Layout.Add( sheet );

		var presets = OutfitPresetLibrary.LoadAll();
		if ( presets.Count == 0 ) return;

		var lbl = new Label( "── Outfit Presets ──", this );
		lbl.SetStyles( "color: #aaa; font-size: 11px; margin-top: 8px;" );
		Layout.Add( lbl );

		foreach ( var preset in presets )
		{
			var btn = new Button( preset.Name, this );
			var captured = preset;
			btn.Clicked += () =>
			{
				var dresser = _so.Targets.OfType<Dresser>().FirstOrDefault();
				if ( dresser != null )
					OutfitPresetTool.ApplyPresetToDresser( dresser, captured );
			};
			Layout.Add( btn );
		}
	}
}