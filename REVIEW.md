> Historical review supplied with the project. For the 22 October 2026 OAuth2 update, current findings and validation limits, see [UPGRADE_REVIEW.md](UPGRADE_REVIEW.md). Statements below about continued PAT availability are obsolete.

# Code Review & Roadmap — ExcelOuraVSTOAddIn

Reviewed: the full VSTO add-in solution (`ExcelOuraVSTOAddIn`, `OuraAPIInterface`, `OuraAPITestLine`). This
document covers what was found, what was fixed in this pass, what's left, and what it would take to run
this from Excel on the web. The fixes described below are already applied in this copy of the source.

## 1. The reported bug: field/config selection "forgotten after a while"

Root cause, confirmed: in `OuraRibbonBar.ConfigureExportConditions`, the line that writes the selected
fields back to settings was commented out:

```csharp
//OuraExcelSettings.Default.Fields = JsonConvert.SerializeObject(OuraFields.CurrentFields());
```

Every other setting (date range, headers, form size/position) was saved correctly; the field
selection/order/custom-labels were not. That state only ever lived in a `static` in-memory list
(`OuraFields.CurrentFields()`) for the lifetime of the Excel/VSTO process. Everything looked fine "for a
while" because repeated use within the same Excel session kept reading that in-memory state; the moment
Excel restarted — or Office recycled the add-in's AppDomain after a period of inactivity, which VSTO does
— the state was gone and the add-in fell back to compiled-in defaults. That matches the symptom exactly.

**Fixed.** `ConfigureExportConditions` now takes a save callback and actually persists the field list.
While in there, the same gap was found and fixed for the **Daily Spo2, Workouts, and Tags** exports — those
three never attempted to save their field choices at all (only the main "Get Oura Data" button tried, and
even that was disabled). Three new settings (`Spo2Fields`, `WorkoutFields`, `TagFields`) were added so each
export type now remembers its own configuration independently.

## 2. A more serious bug this masked: most Sleep/Activity fields never returned real data

While tracing the fields bug, a second, more serious defect turned up in `OuraAPIInterface/OuraFields.cs`.
The field list is a lookup table mapping a display name (e.g. "Bedtime Start") to a property/method name to
read via reflection off `OuraCombinedObjectV2` (e.g. `BedtimeStart`). Roughly 30 of ~40 default Sleep and
Activity fields referenced **pre-V2 API property names that don't exist** on `OuraCombinedObjectV2`
(`BedtimeStart`, `SleepScore`, `Duration`, `Awake`, `Light`, `REM`, `Deep`, `CalActive`, `CalTotal`,
`Steps`, `NonWear`, and more) — the class was actually renamed to V2-style names (`s_bedtime_start`,
`ds_score`, `act_steps`, ...) at some point, but `OuraFields.cs` was never updated to match.

Because the reflection helpers in `OuraRibbonBar.cs` swallow a missing property/method and just return the
literal string `"FAILURE"`, this failed silently: selecting "Bedtime Start", "Sleep Score", "Steps",
"Calorie Active" (all checked-by-default fields) filled Excel with the text `FAILURE` instead of your data,
with no error dialog, exception, or log entry — indistinguishable from a working export unless you looked
closely at the values. Only the Readiness fields (`rr_*`) happened to still match, so the add-in appeared
to "half work."

There's a second file, `OuraFields-MobileCreation.cs`, that looks like an abandoned attempt to fix exactly
this — it correctly maps most Sleep fields to `s_*` names — but it was **not included in the `.csproj`**
(confirmed against `OuraAPIInterface.csproj`'s `<Compile Include>` list), it never compiled, and its
Activity section was never finished (still references the same broken names). It's been removed in this
pass; its useful ideas were folded into the fix below.

**Fixed.** `OuraFields.SetupDefaultFields()` was rewritten so every entry's `MethodName` is verified against
the real public members of `OuraCombinedObjectV2` (cross-checked against `OuraResponsesV2.cs`). All Sleep,
Daily Sleep, and Activity fields (including the daily-sleep and activity score *contributors*, which
weren't exposed before) are included and correctly mapped. Field descriptions were also filled in
consistently.

As defense in depth, `dynamicExecuteProperty`/`dynamicExecuteMethod` no longer return the ambiguous string
`"FAILURE"` on a lookup miss — they now write `#FIELD_ERROR: <name>` (so a mismatch is visually obvious
and easy to search for) and also log to the debug output. They also now catch exceptions from the
property getter/method call itself (previously unguarded — one bad value could abort the entire export
mid-loop, discarding rows already written) and format list-valued fields (e.g. the raw 5-minute HR/HRV
samples) as a delimited string instead of `System.Collections.Generic.List\`1[...]`.

## 3. Token storage — a real deployment blocker for "download and run" use

`ConfigureOuraToken.cs` saved the Personal Access Token via:

```csharp
var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
config.AppSettings.Settings["OuraApiKey"].Value = txtOuraAPIKey.Text;
config.Save(ConfigurationSaveMode.Modified);
```

`ConfigurationUserLevel.None` opens the **application-level** config file — i.e. the file next to the
installed binaries. For anyone who just downloads a release and runs it (rather than building from source
in `bin\Debug`), that's typically under `Program Files` or a VSTO ClickOnce deployment folder, neither of
which a standard Windows user can write to. Saving a token in a normal install throws
`UnauthorizedAccessException` and the feature fails — for exactly the audience this task is meant to
support ("other people to download and use locally"). It was also stored in plain text.

**Fixed.** Added `SecureTokenStorage.cs`, which stores the token in the add-in's existing per-user
settings file (`OuraExcelSettings`, `User` scope — the same file field selections are now correctly saved
to, under `%LocalAppData%\...\ExcelOuraVSTOAddIn...\user.config`, always writable by the signed-in user),
encrypted at rest with Windows DPAPI (`ProtectedData`, `CurrentUser` scope). It transparently migrates an
existing plain-text token from `app.config` on first run. `ConfigureOuraToken.cs` and `OuraRibbonBar.cs`
were updated to use it instead of `ConfigurationManager`.

## 4. Other correctness bugs fixed in this pass

- **Pagination was never handled.** Oura's v2 `usercollection` endpoints page results and return a
  `next_token` when more data is available. None of the `Perform*V2` methods checked for it, so a date
  range large enough to span multiple pages (the Readme explicitly advertises "you can go for years")
  would silently return only the first page with no indication data was missing. Added a generic
  `FetchAllPages` helper in `OuraAPI2Wrapper.cs` and wired every V2 endpoint through it. Also normalized
  the inconsistent `next_token` field typing (`object` on some response classes, `string` on others) to
  `string` throughout.
- **`MissingMemberHandling` was inverted.** `OuraAPI2Wrapper.MakeRequest<T>` set strict member checking
  (`MissingMemberHandling.Error` — throw if the API returns a field the C# model doesn't know about) for
  everyone *except* when a debugger was attached. That's backwards: real users got hard failures the
  moment Oura added a field to a response, while a developer stepping through in Visual Studio saw it
  silently ignored. Flipped so strict checking only applies under the debugger; end users now get a
  forward-compatible, best-effort parse.
- **`UserInfoResponse.Gender` vs. Oura v2's `biological_sex`.** The v1 API used `gender`; v2's
  `personal_info` endpoint uses `biological_sex`. The DataContract mapping still targeted `gender`, so
  under strict deserialization this could throw, and under lenient deserialization it silently left the
  field blank. Added a `biological_sex`-mapped `BiologicalSex` property and kept `Gender` as a
  backwards-compatible computed alias so the existing "Configure Oura Token" form needs no changes.
- **Dead/orphaned files removed**, since they were either never compiled or actively misleading:
  `OuraFields-MobileCreation.cs` (unused, half-finished, superseded by the fix in §2),
  `OuraDateSelectionForm2.*` (an unused duplicate of `OuraSetupTransferForm`, never referenced in the
  `.csproj`), and the V1 API files `OurAPIWrapper.cs`/`OuraResponse.cs` (superseded entirely by the V2
  wrapper/response classes; nothing active referenced them — `UserInfoResponse`, which the V2 code did
  depend on, was moved into `OuraResponsesV2.cs` rather than deleted).
- A ~55KB single-line JSON debug dump left in a code comment in `OuraRibbonBar.cs` (harmless, but made the
  file difficult for tools/editors to open) was removed.

## 5. Remaining issues (not fixed in this pass — flagged for a follow-up)

Roughly in priority order:

- **Excel COM objects aren't released.** `Marshal.ReleaseComObject` is never called on `Range`/`Worksheet`
  interop objects. For a single export this is unlikely to matter, but repeated large exports over a long
  Excel session can leave an orphaned `EXCEL.EXE` process running after Excel is closed. Worth wrapping
  the export loop's COM usage and releasing ranges as you move off them, or batching cell writes (next
  point) to reduce the number of COM calls dramatically.
- **Cell-by-cell writes are slow and chatty over COM.** Every field, every row, is a separate
  `Range.Value` COM call (`WriteCellToExcel`). For a year of daily data across ~30 fields that's tens of
  thousands of individual COM round-trips. Building a 2D `object[,]` array in .NET and writing it in one
  `Range.Value2 = array` call would be dramatically faster and is the standard pattern for bulk Excel
  writes.
- **No async / no progress indication.** All API calls and the write loop run synchronously on the UI
  thread; Excel visibly hangs ("Not Responding") during a large pull with no progress bar or way to
  cancel. Worth moving the HTTP calls to `async`/`await` (or a background thread with progress reported
  back to the ribbon) and showing a simple progress dialog.
- **No retry/backoff for rate limiting.** A `429 Too Many Requests` from Oura is treated like any other
  error (message box, abort) rather than being retried with backoff — more likely to matter now that
  pagination means multi-page pulls make several requests per export.
- **Blocking `MessageBox.Show` calls inside library code.** `OuraAPIInterface` (a supposedly reusable
  class library) shows Windows Forms message boxes directly on HTTP failures. This makes the library
  unusable from a non-UI context (e.g. a console tool, a test suite, or a future web backend) and is a
  layering violation. Prefer throwing/returning a result object and letting the caller (the ribbon code)
  decide how to present it.
- **No automated tests.** Given how long the field-mapping bug in §2 went unnoticed, a small unit test
  suite asserting that every `OuraFields.MethodName` resolves via reflection against its target type would
  have caught this immediately and would prevent regressions going forward. This is a natural, cheap first
  test to add.
- **Casing inconsistency**: the app-settings key is referenced as both `OuraAPIKey` and `OuraApiKey` in
  different places. `ConfigurationManager.AppSettings` happens to be case-insensitive so this "works," but
  it's fragile and confusing — worth standardizing on one constant.
- **`OuraAPI2Wrapper.PerformTagV2`** calls the `enhanced_tag` endpoint but deserializes into the older,
  mismatched `Tags.TagResponseV2` shape (documented in code now, but the method is effectively unused
  dead weight since the "Tags" ribbon button correctly uses `PerformEnhancedTagV2` instead) — candidate for
  removal once confirmed nothing external depends on it.
- **`.csproj.user` is checked into source control.** This file holds machine/developer-specific settings
  (e.g. debug start options) and shouldn't be versioned — add it to `.gitignore`.
- **Packages via `packages.config`** (old-style NuGet) rather than `PackageReference` — fine functionally,
  but migrating would simplify the `.csproj` and remove the need to check in a `packages\` restore
  folder.
- **No versioning/changelog, no CI, no code-signed installer.** For a "download and run" audience, an
  Authenticode-signed ClickOnce/MSI installer avoids SmartScreen warnings, and a GitHub Actions build
  (`msbuild` + a VSTO-capable Windows runner) that produces a versioned release artifact would remove the
  current fully-manual release process.

## 6. Additional Oura data not currently exposed

The add-in covers Sleep, Daily Sleep, Activity, Readiness, Heart Rate, Sessions, Workouts, Tags, and Daily
SpO2. Oura's v2 API also exposes, none of which this add-in surfaces yet:

- **`/v2/usercollection/daily_stress`** — daily stress summary (stress high/recovery high minutes, day
  summary) - added fairly recently to the API, likely of direct interest alongside readiness/activity.
- **`/v2/usercollection/daily_resilience`** — Oura's "resilience" contributor breakdown.
- **`/v2/usercollection/daily_cardiovascular_age`** — cardiovascular age estimate.
- **`/v2/usercollection/vO2_max`** — VO2 max estimate (for users with supported activities/rings).
- **`/v2/usercollection/rest_mode_period`** — rest mode periods (illness/injury tracking).
- **`/v2/usercollection/ring_configuration`** — which ring(s) are paired, model/firmware/size — useful
  metadata for anyone tracking multiple rings or a ring replacement.
- **Sleep Time recommendation** (`/v2/usercollection/sleep_time`) — Oura's recommended bedtime window.
- **Webhook subscriptions** — out of scope for a pull-based Excel add-in, but worth knowing they exist if a
  "keep this workbook always current" feature is ever wanted.

All of these fit the existing `OuraFields`/reflection pattern in `OuraAPIInterface` cleanly and would be a
natural next increment — a new response type in `OuraResponsesV2.cs`, a `Perform...V2` method in
`OuraAPI2Wrapper.cs`, an `OuraFields.CurrentXxxFields()` list, and a ribbon button, following the same
shape as the existing Workouts/Tags/Spo2 exports.

## 7. Running this from Excel on the web

The current add-in is a **VSTO (COM) add-in**: a .NET Framework DLL loaded in-process by the desktop
Excel.exe via the VSTO runtime, using `Microsoft.Office.Interop.Excel`. That entire model is Windows
desktop-only — it cannot run in Excel on the web, Excel on Mac, or Excel on iPad, and Microsoft has been
steering all new Office extensibility toward a different model for this exact reason. Getting this
functionality onto Excel for the web means **rebuilding it as an Office Add-in** (a.k.a. "Office.js"
web add-in) rather than porting the existing project — different language, different API, different
hosting model. Roughly, in order:

1. **Rewrite in TypeScript/JavaScript against Office.js.** Office Add-ins are a web app (HTML/CSS/JS)
   hosted by Microsoft's platform and rendered in a task pane inside Excel, on any platform (desktop, web,
   Mac). Excel access goes through `Excel.run(context => ...)` and the `Excel.Range` JS API
   (`range.values = [[...]]`) instead of COM interop — this also directly fixes the "cell-by-cell is slow"
   issue above, since batched array writes are the *only* way the JS API works, not just an optimization.
2. **Host it somewhere with HTTPS.** Office Add-ins require the task pane and its manifest to be served
   over HTTPS from a real domain (Azure Static Web Apps, a small App Service, GitHub Pages behind a CDN
   with a custom domain, etc. — `localhost` only works for local dev via the Office dev certs). This is a
   real, ongoing hosting dependency the current downloadable EXE model doesn't have.
3. **Solve authentication properly.** A Personal Access Token *can* still work (the user pastes it into
   the task pane, and it's stored via `OfficeRuntime.storage` or `Office.context.roamingSettings`, which
   roam with the Office document/account rather than a local Windows profile). But since this now becomes
   a publicly distributable web app rather than an EXE only the user runs locally, OAuth2 Authorization
   Code + PKCE (which Oura's API supports, and which the current Readme explicitly leaves as optional
   future work) becomes the safer default — it avoids ever asking users to generate and paste a
   long-lived token into a web page, and Oura's tokens are scoped per app for a proper OAuth flow.
4. **Plan for a thin backend, even though Excel-web add-ins are "static."** Two things push toward
   needing one: (a) if the Oura API doesn't set permissive CORS headers for direct browser calls (would
   need to verify against current Oura API docs — the desktop add-in never had to care, since server-to-
   server HTTP calls aren't subject to CORS), a small serverless proxy (Azure Function, Cloudflare Worker)
   is needed to forward requests; (b) a proper OAuth2 Authorization Code flow needs a confidential client
   to exchange the code for tokens without exposing a client secret in browser JS — that's naturally the
   same backend. This can be quite small (a couple of HTTP-triggered functions) and doesn't need a
   database if tokens are kept client-side.
5. **Re-manifest the field-selection UX.** The current `ListView` with drag-drop reordering
   (`OuraSetupTransferForm`, `ListViewWithReordering.cs`) is WinForms and doesn't carry over; the
   task-pane UI would need an HTML/CSS equivalent (a simple checkbox list with up/down controls, or a
   drag-and-drop library, is a straightforward rebuild — the `OuraFields` data model itself transfers
   over conceptually unchanged).
6. **Packaging/distribution.** An Office Add-in is described by an XML (or newer JSON, "unified") manifest
   and can be sideloaded for testing, distributed via a shared file/network share for an org, pushed via
   Microsoft 365 Centralized Deployment for an enterprise, or published to AppSource for public
   distribution — a different distribution story than "download a ClickOnce installer from GitHub
   Releases," worth deciding deliberately rather than defaulting into.

Net effect: this is a genuine rewrite of the UI and integration layers (weeks, not a refactor), though the
*data model* (`OuraFields`, the field list, the combined-object shape) and the *understanding of the Oura
API surface* built into this codebase carry over directly and are the hard-won part. A pragmatic path is
to keep the VSTO add-in as the desktop/offline option (now that it's fixed) and build the Office.js
version as a separate, additive project sharing the same conceptual field catalog, rather than trying to
make one codebase serve both — the two hosting models are different enough that a shared codebase would
fight both platforms.

## 8. Summary of code changes in this pass

| File | Change |
|---|---|
| `OuraAPIInterface/OuraFields.cs` | Rewrote `SetupDefaultFields()` so Sleep/Activity field mappings match real `OuraCombinedObjectV2` members (was the root cause of silent `"FAILURE"` values). |
| `OuraAPIInterface/OuraFields-MobileCreation.cs` | Removed (unused, half-finished, superseded). |
| `OuraDateSelectionForm2.*` | Removed (unused duplicate, not in `.csproj`). |
| `OuraAPIInterface/OurAPIWrapper.cs`, `OuraResponse.cs` | Removed (unused V1 API code); `UserInfoResponse` moved into `OuraResponsesV2.cs`. |
| `OuraRibbonBar.cs` | Fixed the settings-save bug; generalized field load/save to cover Spo2/Workout/Tag exports too; hardened `dynamicExecuteProperty`/`dynamicExecuteMethod`; removed a giant embedded debug comment; wired up `SecureTokenStorage`. |
| `OuraExcelSettings.settings` / `.Designer.cs`, `app.config` | Added `Spo2Fields`, `WorkoutFields`, `TagFields`, `OuraApiKeyProtected` settings. |
| `SecureTokenStorage.cs` (new) | DPAPI-encrypted, per-user token storage with legacy migration. |
| `ConfigureOuraToken.cs` | Uses `SecureTokenStorage` instead of writing to `app.config`. |
| `OuraAPIInterface/OuraAPI2Wrapper.cs` | Added pagination (`next_token`) handling across all V2 endpoints; fixed inverted `MissingMemberHandling` logic. |
| `OuraAPIInterface/OuraResponsesV2.cs` | Normalized `next_token` typing to `string`; added `UserInfoResponse` with correct v2 `biological_sex` mapping. |
| `OuraAPITestLine/Program.cs` | Removed dead code referencing the deleted V1 types. |
| `ExcelOuraVSTOAddIn.csproj`, `OuraAPIInterface.csproj` | Updated compile lists for the above; added `System.Security` reference for DPAPI. |

None of this could be compiled/tested in this environment (no Windows/Office/VSTO toolchain available
here) — please do a build + smoke test in Visual Studio before relying on it, particularly the
reflection-based field mappings in §2 and the DPAPI token storage in §3.
