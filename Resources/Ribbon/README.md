# Ribbon icons

Original geometric icons authored for this project. Each editable SVG has a 32 × 32 view box and is the source for the accompanying transparent 16 × 16 and 32 × 32 PNG files. PNGs were rasterized using resvg 2.6.2, with no font or external image dependencies. Both sizes are committed so building the add-in needs no graphics tools or network access.

Get Oura Data and Connect Oura share the outlined `oura.svg` mark, matching the stroke weight and slate colour of the icon set. Every button selects its icon and size through OuraRibbonDefinition; the runtime builder embeds/loads the corresponding PNG. Any icons added here must be included in both sizes. The Oura mark belongs to its existing rights holder. The original bitmap remains available to the existing forms; the ribbon uses the outlined version.

Palette: blue for oxygen measurements, coral for heart measurements, purple for sleep/sessions, teal for recovery and battery, amber for stress/tags, slate for account/ring actions. Labels and distinct shapes identify commands without relying on colour alone. Standard buttons use 16 px; large buttons use 32 px. Get Oura Data, Connect Oura, Workouts, Sessions and Tags are large; Heart Rate is compact. Windows/Office handles subsequent display scaling.

See `_images/RibbonUpdates-1.0.0.17.png` for the latest button changes and `_images/RibbonIcons-1.0.0.16.png` for a contact sheet (not an Excel screenshot). Changes to SVG sources require regenerating the matching PNGs before building. No rasterization is performed at runtime.
