# Unified ribbon — 1.0.0.16

**Updated in 1.0.0.17:** Get Oura Data and Connect Oura now share the outlined Oura mark. Heart Rate is compact; Workouts, Sessions and Tags are large. OK is the default button on the export/date forms, preserving validation; the label editor already had this behaviour. See [current preview](_images/RibbonUpdates-1.0.0.17.png). The sections below describe the original 1.0.0.16 ribbon consolidation.

The add-in now creates its own custom **Oura** tab, rather than adding controls to a shared Office tab. This separates it from the other add-in's Functions group (Split Cells/Merge Down in the supplied screenshot); those commands do not exist in this solution.

Groups appear in this order:

| Group | Buttons, in order |
|---|---|
| Oura Commands | Get Oura Data, Get Heart Rate, Daily SpO2, Sessions, Workouts, Tags, Sleep Periods |
| Wellbeing | Daily Stress, Resilience, Bedtime Guidance, Rest Mode |
| Heart Health | Cardiovascular Age, VO2 Max |
| Ring and Account | Ring Details, Battery History, Account Profile, **Connect Oura** |

**Get Oura Data** retains its original OuraDarkSolid image, label and large button. Heart Rate and Connect Oura also use large buttons; other commands have compact icons and labels. Ring and Account is the rightmost group, and Connect Oura is its last button. Excel may compress groups when the window is narrow.

## One configuration approach

- `OuraRibbonDefinition.cs` is the single ordered source for tab/group/button labels, IDs, command routing, tooltips, image keys, sizes and connection requirements. Collection labels come from the existing API catalog.
- `OuraRibbonBar.Layout.cs` constructs every control uniformly from that definition, including legacy commands and connection management. It also owns the loaded image cache and command dispatch.
- `OuraRibbonBar.Designer.cs` is a minimal VSTO host shell: no tab/group/button definitions or separate legacy controls remain there. The previous additional-export builder has been removed; `OuraAdditionalExports.cs` retains only export behaviour and saved selections.
- Every export participates in the same connection/busy-state list. Connect Oura remains available while disconnected.
- All controls are built in the ribbon constructor before Office loads it, as required by [Microsoft's ribbon object model](https://learn.microsoft.com/en-us/visualstudio/vsto/ribbon-object-model-overview?view=visualstudio). The tab has a custom control identity so its Oura label does not rename or merge with a built-in shared tab.

## Icons

Sixteen original geometric icons cover the remaining commands: pulse heart, oxygen drop, meditation lotus, dumbbell, tag, crescent moon, stress trend, resilience shield, bedtime clock, rest pause, heart/clock, lungs, ring, battery, profile and connection links. Labels and distinct shapes identify commands independently of colour.

Editable SVG masters and transparent 16/32-pixel PNG assets are in `Resources/Ribbon`. PNGs are embedded directly into the assembly and cached per ribbon instance; building/installing needs no external image files or graphics tooling. Only the chosen size is loaded. Owned images are disposed when the ribbon is disposed; the original shared Oura resource is left to its resource owner.

[Icon contact sheet](_images/RibbonIcons-1.0.0.16.png) — a design preview, not an actual Excel screenshot.

## Validation

- 71 unit tests pass (the previous 69 plus coverage that every existing export is configured exactly once, connection requirements are correct, and every configured custom icon has embedded RGBA PNGs in both sizes).
- All 318 existing regression checks pass.
- The Framework compile check now includes the actual minimal ribbon shell, full runtime builder and definition, instead of substituting the ribbon's old designer fields. It builds against .NET Framework 4.7.2/Excel interop with no warnings or errors. VSTO runtime types are still test scaffolding.
- All production C# source parses as C# 7.3. Archive checks verify the new UI source and embedded assets are included.

The actual Office renderer is unavailable in this Linux workspace. Rebuild with Visual Studio/VSTO on Windows, close all Excel windows and reload/install to verify the custom tab, responsive grouping, icon appearance under your Office theme/scaling and all button clicks. Confirm 1.0.0.16 in Connect Oura. No authentication/API behaviour or scopes changed, and existing encrypted account storage is retained.
