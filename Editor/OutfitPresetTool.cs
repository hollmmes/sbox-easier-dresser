using System.Collections.Generic;
using System.Linq;

namespace EasierDresser;

[Dock( "Editor", "Outfit Presets", "checkroom" )]
public class OutfitPresetTool : Widget
{
	public static OutfitPresetTool Instance { get; private set; }

	// Ana gruplar — kategorileri bunlara bağlıyoruz
	static readonly (string Label, Clothing.ClothingCategory[] Cats)[] Groups = new[]
	{
		("Saç", new[] { Clothing.ClothingCategory.Hair, Clothing.ClothingCategory.HairShort, Clothing.ClothingCategory.HairMedium, Clothing.ClothingCategory.HairLong, Clothing.ClothingCategory.HairUpdo, Clothing.ClothingCategory.HairSpecial }),
		("Şapka", new[] { Clothing.ClothingCategory.Hat, Clothing.ClothingCategory.HatCap, Clothing.ClothingCategory.Headwear }),
		("Üst", new[] { Clothing.ClothingCategory.Tops, Clothing.ClothingCategory.TShirt, Clothing.ClothingCategory.Shirt, Clothing.ClothingCategory.Sweatshirt, Clothing.ClothingCategory.Hoodie, Clothing.ClothingCategory.Vest, Clothing.ClothingCategory.Knitwear, Clothing.ClothingCategory.Jacket, Clothing.ClothingCategory.Cardigan, Clothing.ClothingCategory.Coat, Clothing.ClothingCategory.Gilet }),
		("Alt", new[] { Clothing.ClothingCategory.Bottoms, Clothing.ClothingCategory.Trousers, Clothing.ClothingCategory.Jeans, Clothing.ClothingCategory.Shorts, Clothing.ClothingCategory.Skirt }),
		("Ayakkabı", new[] { Clothing.ClothingCategory.Footwear, Clothing.ClothingCategory.Shoes, Clothing.ClothingCategory.Boots, Clothing.ClothingCategory.Trainers, Clothing.ClothingCategory.Heels, Clothing.ClothingCategory.Sandals, Clothing.ClothingCategory.Slippers, Clothing.ClothingCategory.Socks }),
		("Aksesuar", new[] { Clothing.ClothingCategory.Gloves, Clothing.ClothingCategory.Eyewear, Clothing.ClothingCategory.Facial, Clothing.ClothingCategory.NecklaceChain, Clothing.ClothingCategory.EarringStud, Clothing.ClothingCategory.Wristwear, Clothing.ClothingCategory.Ring, Clothing.ClothingCategory.Piercing }),
		("Tam Vücut", new[] { Clothing.ClothingCategory.Fullbody, Clothing.ClothingCategory.Dress, Clothing.ClothingCategory.Suit, Clothing.ClothingCategory.Costume, Clothing.ClothingCategory.Uniform }),
		("İç Çamaşır", new[] { Clothing.ClothingCategory.Underwear, Clothing.ClothingCategory.Bra, Clothing.ClothingCategory.Underpants }),
		("Cilt / Göz", new[] { Clothing.ClothingCategory.Skin, Clothing.ClothingCategory.Eyes }),
	};

	Dresser _dresser;
	List<Clothing> _allClothing = new();

	// Sol panel
	Label _statusLabel;
	Widget _presetList;
	LineEdit _presetNameEdit;

	// Sağ panel
	Widget _categoryPanel;

	public OutfitPresetTool( Widget parent ) : base( parent )
	{
		Instance = this;
		Layout = Layout.Row();
		Layout.Spacing = 0;
		BuildUI();
		RefreshDresser();
		RefreshClothing();
	}

	// ─── UI ────────────────────────────────────────────────────────────────

	void BuildUI()
	{
		// ── Sol panel ──────────────────────────────────────────────────────
		var left = new Widget( this );
		left.Layout = Layout.Column();
		left.Layout.Spacing = 6;
		left.Layout.Margin = 10;
		left.MinimumWidth = 200;
		left.MaximumWidth = 220;

		// Durum
		_statusLabel = new Label( "Dresser bulunamadı", left );
		_statusLabel.SetStyles( "color: #f88; font-size: 11px;" );
		left.Layout.Add( _statusLabel );

		left.Layout.Add( new Label( "Kayıtlı Şablonlar", left ) );
		_presetList = new Widget( left );
		_presetList.Layout = Layout.Column();
		_presetList.Layout.Spacing = 2;
		left.Layout.Add( _presetList, 1 );

		left.Layout.AddSeparator();

		left.Layout.Add( new Label( "Şablon Adı:", left ) );
		_presetNameEdit = new LineEdit( left );
		_presetNameEdit.PlaceholderText = "Yeni şablon adı...";
		left.Layout.Add( _presetNameEdit );

		var saveBtn = new Button( "Mevcut Kıyafetleri Kaydet", left );
		saveBtn.Clicked += OnSavePreset;
		left.Layout.Add( saveBtn );

		var refreshBtn = new Button( "Yenile", left );
		refreshBtn.Clicked += () => { RefreshDresser(); RefreshClothing(); };
		left.Layout.Add( refreshBtn );

		Layout.Add( left );
		Layout.AddSeparator();

		// ── Sağ panel ──────────────────────────────────────────────────────
		var rightScroll = new ScrollArea( this );
		rightScroll.Canvas = new Widget();
		rightScroll.Canvas.Layout = Layout.Column();
		rightScroll.Canvas.Layout.Spacing = 4;
		rightScroll.Canvas.Layout.Margin = 10;

		_categoryPanel = rightScroll.Canvas;
		Layout.Add( rightScroll, 1 );
	}

	// ─── Veri ──────────────────────────────────────────────────────────────

	void RefreshDresser()
	{
		_dresser = EditorScene.Selection
			.OfType<GameObject>()
			.SelectMany( go => go.Components.GetAll<Dresser>() )
			.FirstOrDefault();

		if ( _dresser != null )
		{
			_statusLabel.Text = $"✓ {_dresser.GameObject.Name}";
			_statusLabel.SetStyles( "color: #8f8; font-size: 11px;" );
		}
		else
		{
			_statusLabel.Text = "Sahnede Dresser seçin";
			_statusLabel.SetStyles( "color: #f88; font-size: 11px;" );
		}
	}

	void RefreshClothing()
	{
		// Proje içindeki tüm .clothing assetlerini yükle
		_allClothing = ResourceLibrary.GetAll<Clothing>().ToList();

		RebuildCategoryUI();
		RebuildPresetUI();
	}

	void RebuildCategoryUI()
	{
		_categoryPanel.DestroyChildren();

		foreach ( var (label, cats) in Groups )
		{
			var items = _allClothing
				.Where( c => cats.Contains( c.Category ) )
				.ToList();

			if ( items.Count == 0 ) continue;

			// Grup başlığı
			var groupLabel = new Label( label.ToUpper(), _categoryPanel );
			groupLabel.SetStyles( "font-weight: bold; color: #aaa; font-size: 11px; margin-top: 6px;" );
			_categoryPanel.Layout.Add( groupLabel );

			foreach ( var item in items )
			{
				var row = new Widget( _categoryPanel );
				row.Layout = Layout.Row();
				row.Layout.Spacing = 6;

				var cb = new Checkbox( item.Title ?? item.ResourceName, row );
				cb.Value = IsEquipped( item );

				var captured = item;
				cb.StateChanged += state =>
				{
					if ( _dresser == null )
					{
						cb.Value = false;
						EditorUtility.DisplayDialog( "Hata", "Önce sahnede bir Dresser seçin.", "Tamam" );
						return;
					}
					ToggleClothing( captured, state == CheckState.On );
				};

				row.Layout.Add( cb, 1 );
				_categoryPanel.Layout.Add( row );
			}
		}

		// Eşleşmeyen kategoriler
		var mapped = Groups.SelectMany( g => g.Cats ).ToHashSet();
		var others = _allClothing.Where( c => !mapped.Contains( c.Category ) ).ToList();
		if ( others.Count > 0 )
		{
			var groupLabel = new Label( "DİĞER", _categoryPanel );
			groupLabel.SetStyles( "font-weight: bold; color: #aaa; font-size: 11px; margin-top: 6px;" );
			_categoryPanel.Layout.Add( groupLabel );

			foreach ( var item in others )
			{
				var cb = new Checkbox( item.Title ?? item.ResourceName, _categoryPanel );
				cb.Value = IsEquipped( item );
				var captured = item;
				cb.StateChanged += state => ToggleClothing( captured, state == CheckState.On );
				_categoryPanel.Layout.Add( cb );
			}
		}
	}

	void RebuildPresetUI()
	{
		_presetList.DestroyChildren();

		var presets = OutfitPresetLibrary.LoadAll();
		foreach ( var preset in presets )
		{
			var row = new Widget( _presetList );
			row.Layout = Layout.Row();
			row.Layout.Spacing = 4;

			var btn = new Button( preset.Name, row );
			var captured = preset;
			btn.Clicked += () =>
			{
				RefreshDresser();
				if ( _dresser == null )
				{
					EditorUtility.DisplayDialog( "Hata", "Önce sahnede bir Dresser seçin.", "Tamam" );
					return;
				}
				ApplyPreset( captured );
				RebuildCategoryUI();
			};
			row.Layout.Add( btn, 1 );

			var del = new Button( "✕", row );
			del.Clicked += () =>
			{
				OutfitPresetLibrary.Delete( captured );
				RebuildPresetUI();
			};
			del.FixedWidth = 28;
			row.Layout.Add( del );

			_presetList.Layout.Add( row );
		}
	}

	// ─── Mantık ────────────────────────────────────────────────────────────

	bool IsEquipped( Clothing item )
	{
		if ( _dresser == null ) return false;
		return _dresser.Clothing.Any( e => e.Clothing == item );
	}

	void ToggleClothing( Clothing item, bool equip )
	{
		if ( _dresser == null ) return;

		if ( equip )
		{
			if ( !IsEquipped( item ) )
				_dresser.Clothing.Add( new ClothingContainer.ClothingEntry { Clothing = item } );
		}
		else
		{
			_dresser.Clothing.RemoveAll( e => e.Clothing == item );
		}

		_ = _dresser.Apply();
	}

	void ApplyPreset( OutfitPreset preset )
	{
		_dresser.Clothing.Clear();
		foreach ( var path in preset.ClothingPaths )
		{
			var asset = ResourceLibrary.Get<Clothing>( path );
			if ( asset != null )
				_dresser.Clothing.Add( new ClothingContainer.ClothingEntry { Clothing = asset } );
		}
		_ = _dresser.Apply();
	}

	void OnSavePreset()
	{
		var name = _presetNameEdit.Text.Trim();
		if ( string.IsNullOrEmpty( name ) )
		{
			EditorUtility.DisplayDialog( "Hata", "Şablon adı boş olamaz.", "Tamam" );
			return;
		}

		if ( _dresser == null )
		{
			EditorUtility.DisplayDialog( "Hata", "Önce sahnede bir Dresser seçin.", "Tamam" );
			return;
		}

		var paths = _dresser.Clothing
			.Where( e => e.Clothing != null )
			.Select( e => e.Clothing.ResourcePath )
			.ToList();

		OutfitPresetLibrary.Save( new OutfitPreset { Name = name, ClothingPaths = paths } );
		_presetNameEdit.Text = "";
		RebuildPresetUI();
	}

	// Statik yardımcı — DresserInspector da kullanır
	public static void ApplyPresetToDresser( Dresser dresser, OutfitPreset preset )
	{
		dresser.Clothing.Clear();
		foreach ( var path in preset.ClothingPaths )
		{
			var asset = ResourceLibrary.Get<Clothing>( path );
			if ( asset != null )
				dresser.Clothing.Add( new ClothingContainer.ClothingEntry { Clothing = asset } );
		}
		_ = dresser.Apply();
	}
}

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