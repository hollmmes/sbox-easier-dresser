# SBox Easier Dresser

A category-based outfit editor for s&box. Browse clothing assets organized by category, toggle items with checkboxes, and apply them to any Dresser component in real time. Save combinations as presets for quick reuse.

## Features

- Clothing listed by category (Hair, Hat, Top, Bottom, Shoes, Accessories, etc.)
- Real-time toggle — checking an item immediately applies it to the character
- Save the current outfit as a named preset
- Apply saved presets with one click
- Preset buttons also appear in the Dresser component inspector

## Usage

1. Open the panel: **Editor → Outfit Presets → Open**
2. Select the GameObject with a **Dresser** component in the Hierarchy
3. Click **Yenile** (Refresh) — the status label turns green showing the active Dresser
4. Check/uncheck clothing items on the right panel — changes apply instantly
5. To save an outfit: enter a name in **Şablon Adı** and click **Mevcut Kıyafetleri Kaydet**
6. Saved presets appear in the left panel and inside the Dresser inspector

## Requirements

- s&box with a scene containing a character using `SkinnedModelRenderer` + `Dresser`
- `.clothing` asset files in your project (from s&box asset library or custom)

## Notes

Preset files are saved as JSON under `Assets/presets/` in your project folder and can be version-controlled alongside your project.