# Excel Oura VSTO Add-in — 1.0.0.18

Imports Oura API V2 sleep, activity, readiness, heart rate, sessions, SpO2, workouts, tags, stress, resilience, heart-health measurements and ring information into Windows desktop Excel. Field selections, order and custom labels are retained. This source updates the supplied local V2 project, including its prior fixes; it does not replace it with the older GitHub release.

Personal access tokens are retired. Use **Connect Oura** on the ribbon to authorize with OAuth2. Never put tokens or a shared application secret in `app.config` or source control.

## Build on Windows

Use Visual Studio 2022 with **Office/SharePoint development**, the .NET Framework 4.7.2 targeting pack, Windows desktop Excel and the VSTO runtime. Restore NuGet packages for `ExcelOuraVSTOAddIn.sln`, set the VSTO project as startup, then build it. The regression tools additionally require .NET SDK 8.

Manifest signing is off for compilation checks. Before loading/debugging the add-in in Excel, configure a certificate in the project's Signing settings and enable manifest signing (a new temporary certificate is appropriate for local development). For an installable ClickOnce release, use a suitable new publishing certificate and publish through Visual Studio. Existing PFX files and old compiled outputs are excluded from the source download. The source download is **not an installer**; rebuild before installing. Do not reuse a signing private key that has been distributed publicly.

## Connect your Oura account

1. Register your own application through the [Oura developer portal](https://developer.ouraring.com/applications).
2. Add this exact redirect address to the registration: `http://localhost:48179/oura/callback/`. The port and trailing slash must match. If that port is occupied, use another port above 1023 in both places.
3. In Excel, select **Oura → Ring and Account → Connect Oura**. Enter your registration's client ID, client secret and redirect address.
4. Select **Sign in with Oura**. Sign in and approve the data you want to export in your usual browser. The browser returns to the local callback; check Excel for the result. Sign-in times out after three minutes; closing during sign-in cancels it; wait for cancellation to finish, then close the dialog.
5. Select an export command, date range and fields. The export starts at the active worksheet cell and overwrites the destination block. Missing data produces blank cells, and numeric values remain numeric.

Oura requires a client secret for the documented authorization-code exchange even when PKCE is used. This build therefore uses **your own app registration**. It is suitable for personal/local use. Before distributing one shared registration to other users, put code exchange and refresh behind a hosted authentication service; a secret embedded in desktop binaries cannot remain confidential. Oura currently limits unapproved applications to ten users. Gen3 and later API access requires an active membership. Users may decline individual data permissions; a 403 explains the consent/membership checks.

If Windows cannot start the callback listener, check for a port conflict. Where HTTP URL permissions are required, an administrator can reserve only the callback URL for your Windows account, using an elevated PowerShell window:

```powershell
netsh http add urlacl url=http://localhost:48179/oura/callback/ "user=$env:USERDOMAIN\$env:USERNAME"
```

Use the configured URL if you changed it. Excel itself should run as your ordinary user. A real Oura registration/browser callback still needs to be tested on Windows before release.

## Ribbon ordering — 1.0.0.18

Oura Commands now places the large Get Oura Data, Sessions, Workouts and Tags buttons first. The rightmost compact column contains Get Heart Rate, Daily SpO2 and Sleep Periods, from top to bottom. Close all Excel windows before rebuilding/installing this version.

## Keyboard and button updates — 1.0.0.17

Enter now submits the export-selection and date-selection forms through their OK validation handlers; the label editor already uses OK as its default. Standard Windows behaviour lets a focused alternative button handle Enter instead. Invalid dates or an empty field selection keep the export form open.

Get Oura Data and Connect Oura share a new outlined Oura mark matching the other ribbon icons. Heart Rate is compact; Workouts, Sessions and Tags are large. The Oura tab and far-right Ring and Account group remain as configured. [Preview the updated icons and sizes](_images/RibbonUpdates-1.0.0.17.png). Close all Excel windows before rebuilding/installing, and confirm **1.0.0.18** in Connect Oura.

## Unified Oura ribbon — 1.0.0.16

The add-in now has its own **Oura** tab. **Ring and Account** is the last group, with **Connect Oura** at the far right. All 17 buttons use one ordered definition and one builder; each has an icon, and **Get Oura Data** keeps its original Oura image and large size. See [RIBBON_CHANGE.md](RIBBON_CHANGE.md) for the layout and icon sources, or [preview the new icons](_images/RibbonIcons-1.0.0.16.png).

All existing exports and connection behaviour are retained. Close every Excel window, rebuild/install this source with your signing setup, then confirm **1.0.0.18** in the connection dialog. Ribbon layout is cached while Excel runs. Existing saved credentials do not need to be re-entered for this UI change.

## Code review and hardening — 1.0.0.15

All exports now share cancellable downloads, strict response validation, worksheet/memory limits and one Excel writer. Saved labels/order/selection are merged safely into current field definitions. An interrupted token refresh requires reconnecting instead of replaying an uncertain refresh token. See [CODE_REVIEW.md](CODE_REVIEW.md) for findings, implementation details, test evidence and the Windows release checklist.

This release keeps the working OAuth gateway and existing export buttons. No new scopes were added after 1.0.0.14, so an already authorized account can continue using its saved connection. Close Excel, restore/build this source and reload it with your signing configuration. Confirm **1.0.0.18** in the connection-window title.

## Heart rate and additional exports — 1.0.0.14

Authentication was confirmed working on Windows with 1.0.0.13. This update keeps that public token route and fixes the heart-rate range error.

**Heart Rate** is now available outside the debugger. Oura time-series requests have a 30-day ceiling; selected histories are split into windows of at most 29 days, with pagination inside each window and duplicate boundary samples removed. Dates represent this computer's local calendar days, including daylight-saving changes. All data is fetched before headers or cells are changed. A later request failure therefore leaves the worksheet untouched. The new export progress window shows downloaded pages and allows cancellation.

The ribbon now has these exports in addition to Get Oura Data, SpO2, Workouts and Tags:

| Group | Buttons | Notes |
|---|---|---|
| Oura Commands | Heart Rate, Sessions, Sleep Periods | Sessions now works with HR/HRV sample arrays. Sleep Periods includes individual sleeps/naps rather than one representative sleep per day. |
| Wellbeing | Daily Stress, Resilience, Bedtime Guidance, Rest Mode | Missing measurements stay blank. Stress/recovery durations and bedtime offsets are explicitly labeled in seconds. |
| Heart Health | Cardiovascular Age, VO2 Max | Requires appropriate permissions and available ring measurements. |
| Ring and Account | Ring Details, Battery History, Account Profile | Ring Details/Profile return current/all records without date filters. Battery History uses timestamp windows. |

Every export supports field selection, ordering, custom labels and optional header descriptions. New export selections are saved independently by endpoint. Nested session samples are exported as delimited values with sample interval/start columns; timestamp strings retain UTC offsets. Numbers and booleans remain usable in Excel. The destination block is overwritten as before; headers/data are written together only after a successful fetch/table build. Excel formatting/comment failures can still occur after the data block is assigned.

**After upgrading, open Connect Oura and sign in again once to approve the additional permissions** (stress, heart health, ring information and optional account profile/email). Keep your existing app registration and redirect address. Also enable the relevant data permissions in the developer portal if required. An existing authorization remains saved until a new sign-in succeeds. A dataset can be empty because the ring/account has no measurements, or forbidden because the user/application has not granted it; the dialog explains permission failures.

Close all Excel windows, restore/build the new source and reload/install it with your signing setup. Confirm **1.0.0.18** in the connection-window title. The source archive is not a rebuilt installer.

## Storage and disconnection

The client secret and OAuth token pair are encrypted with Windows DPAPI (`CurrentUser`) in `%LOCALAPPDATA%\ExcelOuraVSTOAddIn\oauth.dat`. The stable path survives add-in version updates. Do not copy it to another user or machine. The add-in refreshes tokens shortly before expiry and once on a 401. Refresh tokens are single-use: a Windows named mutex coordinates Excel processes, and rotated tokens are saved together through an atomic replacement. A persisted pending marker prevents replay after an interrupted refresh; reconnect if asked.

**Disconnect** deletes the local OAuth credentials and clears the old encrypted PAT setting. To revoke the grant at Oura too, remove the application from your Oura account. Old PAT settings are never used as a fallback. Original archives/build folders may still contain historical credentials; the updated source package excludes those folders.

## Validation

After restoring the solution's NuGet packages:

```powershell
dotnet test tests/OuraUnitTests.csproj --configuration Release
dotnet run --project tests/OuraRegressionTests.csproj
dotnet build tests/FrameworkCompileCheck.csproj
```

The xUnit project tests validation, failure handling and the refresh state machine. The regression executable tests PKCE/state/callbacks, token forms, token rotation responses, HTTP recovery/retries, every field mapping, nullable values, date-window splitting, cancellation, all new catalog fields against the official schema, synthetic sandbox fixtures and bulk table generation without contacting an Oura account. The framework check compiles the API, connection, storage, forms and ribbon source against .NET Framework 4.7.2 and the real Excel interop library. It uses test-only VSTO host/designer scaffolding and does not load Excel, exercise Windows DPAPI or build a ClickOnce installer.

`OuraAPITestLine` is a diagnostic console tool: supply a current OAuth access token via the `OURA_ACCESS_TOKEN` environment variable. It does not perform interactive sign-in or persist/refresh tokens; normal users should connect through Excel. It prints row counts rather than health data.

See [UPGRADE_REVIEW.md](UPGRADE_REVIEW.md) for verified changes, remaining issues, validation limits and release checks. [REVIEW.md](REVIEW.md) is the historical review supplied with the project.

## API references

- [Current V2 API documentation](https://cloud.ouraring.com/v2/docs)
- [OAuth2 authorization, PKCE and refresh requirements](https://cloud.ouraring.com/docs/authentication)
- [Oura membership/API access](https://ouraringhelp.zendesk.com/hc/en-us/articles/4415266939155-The-Oura-API)
