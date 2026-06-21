using System.Linq;

namespace EasierDresser;

/// <summary>
/// Dresser component'i inspector'ında şablon butonları gösterir.
/// </summary>
[CustomEditor( typeof(Dresser) )]
public class DresserInspector : ControlWidget
{
	Dresser _dresser;

	public DresserInspector( SerializedObject so ) : base( so )
	{
		_dresser = so.Targets.OfType<Dresser>().FirstOrDefault();
		BuildUI();
	}

	void BuildUI()
	{
		Layout = Layout.Column();
		Layout.Spacing = 4;

		// Mevcut Dresser özelliklerini göster
		BuildDefaultInspector();

		var presets = OutfitPresetLibrary.LoadAll();
		if ( presets.Count == 0 ) return;

		// Ayırıcı başlık
		var separator = new Label( "── Outfit Presets ──", this );
		separator.SetStyles( "color: #aaa; font-size: 11px; margin-top: 8px; margin-bottom: 4px;" );
		Layout.Add( separator );

		// Her şablon için bir buton
		foreach ( var preset in presets )
		{
			var btn = new Button( preset.Name, this );
			var captured = preset;
			btn.Clicked += () =>
			{
				if ( _dresser == null ) return;
				OutfitPresetTool.ApplyPresetToDresser( _dresser, captured );
				SceneEditorSession.Active?.Scene?.Editor?.UndoRedo?.Push( "Apply Outfit Preset" );
			};
			Layout.Add( btn );
		}
	}
}