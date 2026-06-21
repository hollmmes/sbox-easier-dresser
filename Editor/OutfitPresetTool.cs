using System.Collections.Generic;
using System.Linq;

namespace EasierDresser;

/// <summary>
/// Editörde dock olarak açılan Outfit Preset Manager.
/// </summary>
[Dock( "Editor", "Outfit Presets", "checkroom" )]
public class OutfitPresetTool : Widget
{
	public static OutfitPresetTool Instance { get; private set; }

	List<OutfitPreset> _presets = new();
	List<string> _availableClothing = new();
	OutfitPreset _selectedPreset;

	ListView _presetListView;
	ListView _clothingListView;
	LineEdit _nameEdit;
	Button _applyButton;
	Button _deleteButton;

	string _newPresetName = "";

	public OutfitPresetTool( Widget parent ) : base( parent )
	{
		Instance = this;
		BuildUI();
		Refresh();
	}

	void BuildUI()
	{
		Layout = Layout.Row();
		Layout.Margin = 8;
		Layout.Spacing = 8;

		// Sol panel
		var left = new Widget( this );
		left.Layout = Layout.Column();
		left.Layout.Spacing = 4;
		left.MinimumWidth = 180;

		left.Layout.Add( new Label( "Kayıtlı Şablonlar", left ) );

		_presetListView = new ListView( left );
		_presetListView.ItemActivated += item => OnPresetSelected( item );
		left.Layout.Add( _presetListView, 1 );

		var btnRow = new Widget( left );
		btnRow.Layout = Layout.Row();
		btnRow.Layout.Spacing = 4;

		_applyButton = new Button( "Uygula", btnRow );
		_applyButton.Clicked += OnApplyClicked;
		_applyButton.Enabled = false;
		btnRow.Layout.Add( _applyButton, 1 );

		_deleteButton = new Button( "Sil", btnRow );
		_deleteButton.Clicked += OnDeleteClicked;
		_deleteButton.Enabled = false;
		btnRow.Layout.Add( _deleteButton, 1 );

		left.Layout.Add( btnRow );
		Layout.Add( left );

		// Sağ panel
		var right = new Widget( this );
		right.Layout = Layout.Column();
		right.Layout.Spacing = 4;

		right.Layout.Add( new Label( "Yeni Şablon", right ) );

		var nameRow = new Widget( right );
		nameRow.Layout = Layout.Row();
		nameRow.Layout.Spacing = 4;
		nameRow.Layout.Add( new Label( "Ad:", nameRow ) );

		_nameEdit = new LineEdit( nameRow );
		_nameEdit.PlaceholderText = "Şablon adı...";
		_nameEdit.TextEdited += v => _newPresetName = v;
		nameRow.Layout.Add( _nameEdit, 1 );
		right.Layout.Add( nameRow );

		right.Layout.Add( new Label( "Kıyafetler (seç):", right ) );

		_clothingListView = new ListView( right );
		right.Layout.Add( _clothingListView, 1 );

		var saveBtn = new Button( "Şablon Olarak Kaydet", right );
		saveBtn.Clicked += OnSaveClicked;
		right.Layout.Add( saveBtn );

		Layout.Add( right, 1 );
	}

	void Refresh()
	{
		_presets = OutfitPresetLibrary.LoadAll();
		_availableClothing = OutfitPresetLibrary.FindClothingAssets();

		_presetListView.SetItems( _presets.Select( p => (object)p.Name ).ToList() );
		_clothingListView.SetItems( _availableClothing.Select( p => (object)p ).ToList() );
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

		var dresser = EditorScene.Selection
			.OfType<GameObject>()
			.SelectMany( go => go.Components.GetAll<Dresser>() )
			.FirstOrDefault();

		if ( dresser == null )
		{
			EditorUtility.DisplayDialog( "Hata", "Önce sahnede bir Dresser component'i olan objeyi seçin.", "Tamam" );
			return;
		}

		ApplyPresetToDresser( dresser, _selectedPreset );
	}

	void OnDeleteClicked()
	{
		if ( _selectedPreset == null ) return;
		OutfitPresetLibrary.Delete( _selectedPreset );
		_selectedPreset = null;
		_applyButton.Enabled = false;
		_deleteButton.Enabled = false;
		Refresh();
	}

	void OnSaveClicked()
	{
		if ( string.IsNullOrWhiteSpace( _newPresetName ) )
		{
			EditorUtility.DisplayDialog( "Hata", "Şablon adı boş olamaz.", "Tamam" );
			return;
		}

		var selected = _clothingListView.SelectedItems.Cast<string>().ToList();
		if ( selected.Count == 0 )
		{
			EditorUtility.DisplayDialog( "Hata", "En az bir kıyafet seçin.", "Tamam" );
			return;
		}

		OutfitPresetLibrary.Save( new OutfitPreset
		{
			Name = _newPresetName,
			ClothingPaths = selected
		} );

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
			if ( asset == null ) continue;
			dresser.Clothing.Add( new ClothingContainer.ClothingEntry { Clothing = asset } );
		}

		_ = dresser.Apply();
	}
}

/// <summary>
/// Üst menüde "Editor > Outfit Presets" girişini açar.
/// </summary>
public static class OutfitPresetMenu
{
	[Menu( "Editor", "Outfit Presets/Open" )]
	public static void Open()
	{
		var existing = OutfitPresetTool.Instance;
		if ( existing != null && existing.IsValid )
		{
			existing.Show();
			EditorWindow.DockManager.RaiseDock( existing );
		}
		else
		{
			EditorWindow.DockManager.Create<OutfitPresetTool>();
		}
	}
}