using System.Collections.Generic;
using System.Linq;

namespace EasierDresser;

/// <summary>
/// Editörün üst araç çubuğunda görünen Outfit Preset Manager penceresi.
/// </summary>
[EditorApp( "Outfit Presets", "checkroom", "Kıyafet şablonlarını kaydet ve uygula" )]
public class OutfitPresetTool : DockWidget
{
	List<OutfitPreset> _presets = new();
	List<string> _availableClothing = new();

	// Sol panel seçimi
	OutfitPreset _selectedPreset;

	// Yeni şablon için geçici veri
	string _newPresetName = "";
	List<string> _newPresetClothing = new();

	// UI elemanları
	ListView _presetList;
	ListView _clothingList;
	LineEdit _nameEdit;
	Button _saveButton;
	Button _applyButton;
	Button _deleteButton;

	public OutfitPresetTool( Widget parent = null ) : base( parent )
	{
		Title = "Outfit Preset Manager";
		MinimumSize = new Vector2( 600, 400 );

		BuildUI();
		Refresh();
	}

	void BuildUI()
	{
		var root = new Widget( this );
		root.Layout = Layout.Row();
		root.LayoutMargins = new Sandbox.UI.Margin( 8 );

		// Sol panel — kayıtlı şablonlar
		var leftPanel = new Widget( root );
		leftPanel.Layout = Layout.Column();
		leftPanel.MinimumWidth = 180;

		var leftLabel = new Label( "Şablonlar", leftPanel );
		leftLabel.SetStyles( "font-weight: bold; margin-bottom: 4px;" );

		_presetList = new ListView( leftPanel );
		_presetList.ItemActivated += OnPresetSelected;
		leftPanel.Layout.Add( _presetList, 1 );

		var leftButtons = new Widget( leftPanel );
		leftButtons.Layout = Layout.Row();

		_applyButton = new Button( "Uygula", leftButtons );
		_applyButton.Clicked += OnApplyClicked;
		_applyButton.Enabled = false;
		leftButtons.Layout.Add( _applyButton, 1 );

		_deleteButton = new Button( "Sil", leftButtons );
		_deleteButton.Clicked += OnDeleteClicked;
		_deleteButton.Enabled = false;
		leftButtons.Layout.Add( _deleteButton, 1 );

		leftPanel.Layout.Add( leftButtons );
		leftPanel.Layout.AddStretchCell();

		root.Layout.Add( leftPanel );

		// Ayırıcı
		var separator = new Widget( root );
		separator.MinimumWidth = 1;
		separator.MaximumWidth = 1;
		separator.SetStyles( "background: #333; margin: 0 8px;" );
		root.Layout.Add( separator );

		// Sağ panel — yeni şablon
		var rightPanel = new Widget( root );
		rightPanel.Layout = Layout.Column();

		var rightLabel = new Label( "Yeni Şablon", rightPanel );
		rightLabel.SetStyles( "font-weight: bold; margin-bottom: 4px;" );

		var nameRow = new Widget( rightPanel );
		nameRow.Layout = Layout.Row();
		var nameLabel = new Label( "Ad:", nameRow );
		nameLabel.MinimumWidth = 40;
		nameRow.Layout.Add( nameLabel );

		_nameEdit = new LineEdit( nameRow );
		_nameEdit.PlaceholderText = "Şablon adı...";
		_nameEdit.TextEdited += v => _newPresetName = v;
		nameRow.Layout.Add( _nameEdit, 1 );
		rightPanel.Layout.Add( nameRow );

		var clothingLabel = new Label( "Kıyafetler (seç):", rightPanel );
		clothingLabel.SetStyles( "margin-top: 8px; margin-bottom: 2px;" );
		rightPanel.Layout.Add( clothingLabel );

		_clothingList = new ListView( rightPanel );
		_clothingList.SelectionMode = SelectionMode.Multiple;
		rightPanel.Layout.Add( _clothingList, 1 );

		_saveButton = new Button( "Şablon Olarak Kaydet", rightPanel );
		_saveButton.Clicked += OnSaveClicked;
		rightPanel.Layout.Add( _saveButton );

		root.Layout.Add( rightPanel, 1 );

		Layout = Layout.Column();
		Layout.Add( root, 1 );
	}

	void Refresh()
	{
		_presets = OutfitPresetLibrary.LoadAll();
		_availableClothing = OutfitPresetLibrary.FindClothingAssets();

		_presetList.SetItems( _presets.Select( p => p.Name ).ToList() );
		_clothingList.SetItems( _availableClothing );
	}

	void OnPresetSelected( object item )
	{
		var name = item as string;
		_selectedPreset = _presets.FirstOrDefault( p => p.Name == name );
		_applyButton.Enabled = _selectedPreset != null;
		_deleteButton.Enabled = _selectedPreset != null;
	}

	void OnApplyClicked()
	{
		if ( _selectedPreset == null ) return;

		// Sahnede seçili Dresser'ı bul ve uygula
		var dresser = EditorScene.Selection
			.OfType<GameObject>()
			.SelectMany( go => go.Components.GetAll<Dresser>() )
			.FirstOrDefault();

		if ( dresser == null )
		{
			EditorUtility.DisplayDialog( "Hata", "Önce sahnede bir Dresser component'i seçin.", "Tamam" );
			return;
		}

		ApplyPresetToDresser( dresser, _selectedPreset );
	}

	void OnDeleteClicked()
	{
		if ( _selectedPreset == null ) return;
		OutfitPresetLibrary.Delete( _selectedPreset );
		_selectedPreset = null;
		Refresh();
	}

	void OnSaveClicked()
	{
		if ( string.IsNullOrWhiteSpace( _newPresetName ) )
		{
			EditorUtility.DisplayDialog( "Hata", "Şablon adı boş olamaz.", "Tamam" );
			return;
		}

		var selectedIndices = _clothingList.SelectedItems.Cast<int>().ToList();
		var clothing = selectedIndices.Select( i => _availableClothing[i] ).ToList();

		if ( clothing.Count == 0 )
		{
			EditorUtility.DisplayDialog( "Hata", "En az bir kıyafet seçin.", "Tamam" );
			return;
		}

		var preset = new OutfitPreset
		{
			Name = _newPresetName,
			ClothingPaths = clothing
		};

		OutfitPresetLibrary.Save( preset );
		_nameEdit.Text = "";
		_newPresetName = "";
		Refresh();
	}

	public static void ApplyPresetToDresser( Dresser dresser, OutfitPreset preset )
	{
		dresser.Clothing.Clear();
		foreach ( var path in preset.ClothingPaths )
		{
			var asset = ResourceLibrary.Get<Clothing>( path );
			if ( asset != null )
				dresser.Clothing.Add( asset );
		}
	}
}