# Code review and hardening — 1.0.0.15

Reviewed 22 October 2026. This review covers the supplied local V2 solution and the subsequent OAuth/export updates, including API access, credential storage, ribbon commands, forms, Excel writes, configuration, build references and tests. The attached historical REVIEW.md was treated as background evidence, not as a new instruction or an assurance that the code was correct.

## Findings and implemented changes

| Priority | Finding | Resolution |
|---|---|---|
| High | A crash, cancelled refresh or lost HTTP response could leave a previously consumed refresh token on disk. A subsequent Excel process could replay it. | `OuraTokenManager` persists a `RefreshPending` marker under the account mutex **before** posting. Successful rotation replaces the token pair and clears the marker atomically. An uncertain outcome requires a fresh sign-in. This deliberately favours avoiding replay over automatic recovery. |
| High | Export paths duplicated download, table construction, error handling and COM operations. Older commands did not share the newer cancellation/paging safeguards. | Every ribbon export now uses `RunExport`, the shared async collection reader, `OuraProgressDialog`, `OuraExportTable` and `ExcelExportTarget`. Legacy synchronous API entry points remain for compatibility; UI commands use async operations. |
| High | Dataset size, JSON shape and response bodies were insufficiently bounded. Large exports could exhaust memory or exceed the Excel destination. | Validate range/grid/cell limits before allocation, restrict streamed data responses to 16 MiB and token responses to 64 KiB, reject malformed/deep/duplicate/trailing JSON, enforce row/page limits, require valid row identities/date boundaries and bound continuation tokens. Limits produce actionable failures rather than partial success. |
| High | The page counter advanced only when a progress callback was present. | Increment page count independently of progress notification. A new test demonstrates that the limit stops the next request even without a callback. |
| High | A protected sheet, merged block, unsuitable active sheet or changed destination could fail after downloading. Formatting failures could be reported as complete export failures even after data was written. | Capture the original worksheet and anchor on the UI thread; preflight protection, anchor and capacity. Recheck protection/merged destination before the single bulk assignment. Report heading/note failures as warnings after successful assignment. Release acquired COM references without forcibly final-releasing Excel's shared Application object. |
| Medium | A progress window could close while its worker was still running. | Closing/cancelling waits for worker completion before the modal operation returns. Cancellation propagates through token-lock acquisition, HTTP, paging and table construction. No Excel COM objects enter the worker. |
| Medium | Saved settings deserialized directly to an overloaded-constructor model and silently lost custom preferences on deserialization failure; persisted accessor names were also an unnecessary trust surface. | Deserialize a dedicated presentation-only preferences DTO. Merge labels/order/selection into current definitions, preserve new fields, ignore malformed preferences, normalize order and never accept persisted accessor definitions. |
| Medium | Static default field lists exposed mutable shared objects. Reset only cleared one dataset. | Return independent cloned definitions, synchronize default initialization and reset all four legacy field catalogs. Reset in the form uses fresh defaults. |
| Medium | HTTP recovery, cancellation and diagnostics differed by path. | One request transport uses per-request bearer headers, cancellation/deadlines for headers and bodies, one 401 recovery using the rejected token, bounded GET retries and Retry-After. Token POSTs are never automatically retried. A newer token saved by another process is reused instead of refreshing twice. |
| Medium | Callback handling accepted the first local request as the whole sign-in attempt; an unrelated request or closed browser response could interrupt sign-in. | Accept GET callbacks only, validate redirect/state/issuer and continue waiting after unrelated invalid requests. A rejected consent response ends the attempt. Browser response write failures do not discard a usable code. Closing during sign-in cancels and waits for completion. Keep the public Oura token gateway that worked in 1.0.0.13. |
| Medium | Arbitrary exceptions and API payloads could reach user-facing error text. | `OuraException` carries expected safe guidance; UI boundaries provide a generic message for unexpected failures. Trace only exception type/HResult. Never display/log credentials, authorization codes, callback queries, API bodies or health values. The console diagnostic follows the same restriction. |
| Medium | Large numeric identifiers could lose precision in Excel; formula escaping could itself exceed the text limit; non-finite numeric values were accepted. | Preserve long identifiers as text; escape formula-like strings; check length after escaping; reject NaN/infinity. Preserve numeric zero, boolean values, missing values and timestamp offsets. |
| Medium | Raw records retained their whole page through parent references; consumers could mutate input objects after record construction. | Detach collection rows from their page; public record construction takes a deep snapshot. Field traversal accepts simple dotted property paths instead of JSONPath expressions. |
| Medium | Sleep wall-time helpers used unchecked substrings. | Parse timestamps safely, preserve the recorded location's clock time, handle missing values and reject malformed values. Correct respiratory-rate units to breaths per minute. |
| Low | Drag/drop accepted foreign ListViews and used fragile clone/index logic. Old unused workbook-save sample code inserted a row into a workbook. | Restrict reordering to its originating control, preserve selected item order and remove the unused workbook mutation sample. |
| Low | Unused packages and obsolete binding redirects complicated restore/runtime behaviour. | Retain Newtonsoft.Json 13.0.4 as the sole third-party production package; remove unused Buffers/Memory/Vectors/Unsafe/Serialization.Primitives references and stale redirects/unused web authentication configuration. Retain .NET Framework 4.7.2 and VSTO compatibility. |
| Low | Coding conventions and repeatable checks were not explicit. | Add `.editorconfig`, explicit C# 7.3/deterministic builds, an xUnit project and a GitHub Actions Windows/Linux validation workflow. Format the refactored manual code; preserve generated designer files and public DTO/API names. |

## Design boundaries

- `OuraOAuthClient`: protocol validation and a single token request, no persistence or Excel dependency.
- `OuraTokenManager`: testable rotation state machine with injected persistence, refresh and time. Its caller must hold the cross-process account mutex.
- `SecureTokenStorage` / `OuraConnection`: Windows DPAPI, atomic file replacement, lock ownership and account wiring. The same thread acquires and releases the mutex; no await occurs inside its ownership scope.
- `OuraApiTransport`: authenticated GETs, bounded recovery, timeouts and safe HTTP errors.
- `OuraCollectionClient`: date windows, pagination, identity checks, deduplication and bounded result collection. All exports use this reader, including legacy typed datasets.
- `OuraExportTable`: managed-only field resolution and validated cell values.
- `ExcelExportTarget`: Excel UI-thread operations only, original destination capture, bounds/protection checks and bulk assignment.
- Ribbon/forms: user interaction and orchestration. Broad catches are restricted to UI/worker boundaries so failures cannot escape into Excel; expected failures are handled close to their source.

Large operations stop at 5,000,000 output cells, Excel's row/column limits, 32,767 characters per cell, the remaining destination capacity or 20,000 fetched pages. These are safety limits, not guarantees that every machine can comfortably handle the maximum. Use smaller date ranges for long histories. No data is written until download and table validation succeed. Excel assignment is not transactional: an Excel write failure can still leave changes, and its message explicitly asks the user to inspect the destination.

## Validation performed

- **69 xUnit cases pass**, including pending-marker ordering, refresh interruption/replay prevention, failed saves before/after rotation, concurrent 401 recovery, proactive refresh, malformed token responses, invalid redirects, JSON depth/duplicates/trailing data, response size/encoding, cancellation, HTTP retry policy, URL validation, row/page/grid/cell limits, formula escaping, long identifiers, immutable defaults/records, preference migration and daily combination.
- **318 existing API/export regression checks pass**, covering all field mappings, OAuth/PKCE/state, dates/DST, pagination, duplicate boundaries, optional values, official schema mappings and synthetic sandbox fixtures.
- The Framework compile-check project builds the API, credential handling, forms, ribbon orchestration and writer against **.NET Framework 4.7.2 and the real Excel interop assembly**, with zero warnings/errors. It substitutes VSTO host/designer scaffolding; it is not a full VSTO deployment build.
- All 38 production C# files parse successfully as C# 7.3. A NuGet audit of the unit-test dependency graph (including the production Newtonsoft.Json dependency) reports no known vulnerable packages against the configured feed on the review date.
- Tests use fake HTTP handlers and synthetic fixtures, never the user's account or credentials. The new GitHub Actions workflow has been added, but has not been run on GitHub from this workspace.

Run from the solution directory with .NET SDK 8 installed:

```powershell
dotnet test tests/OuraUnitTests.csproj --configuration Release
dotnet run --project tests/OuraRegressionTests.csproj --configuration Release
dotnet build tests/FrameworkCompileCheck.csproj --configuration Release --warnaserror
```

The tests restore their own pinned dependencies. The production VSTO solution still uses packages.config: restore its packages through Visual Studio/NuGet before building.

## Remaining platform/release checks

These cannot be established by managed tests in this Linux workspace:

1. Build the **real solution** using Visual Studio 2022's Office development workload and the .NET Framework targeting pack. Configure manifest signing with your own certificate. This source archive is not an installer.
2. On Windows Excel, confirm version **1.0.0.15**, existing-account load, sign-in, normal expiry refresh, revoked consent, cancellation/timeout and disconnect. Exercise two Excel processes attempting recovery together and recovery after terminating Excel during refresh. Existing DPAPI credentials remain at the same stable path; this release alone does not add scopes or require reauthorization.
3. Test each button on a real account, all field preferences/reset/custom labels/reordering, cancellation before/after a page, and a multi-year heart-rate history. Validate Windows locales/DST, missing measurements and account-specific permissions.
4. Test exports into protected/merged destinations, the last row/column, old-format `.xls` sheets, chart sheets, multiple workbooks and formatting/note failures. Confirm Excel remains usable and does not stay running after closing it.
5. Publish/install/update/uninstall using your supported Office bitness and certificate trust configuration. The existing historical private-key files remain excluded from source archives. Review/replace any signing key that was publicly distributed; this review does not prove whether a particular key has been exposed.

A desktop application cannot keep a shared developer secret confidential. The current design uses each user's own registration and DPAPI-protected local credentials. If distributing a single shared registration, build a hosted token service or use an Oura-supported native/public-client flow first. Requesting only the scopes needed by selected datasets is a possible future consent-UX improvement; it needs explicit consent-upgrade behaviour and is not required for the existing buttons to work. Generated VSTO artifacts and the legacy public DTO naming remain intentionally compatible; wholesale framework/UI migration would need separate product and deployment decisions.

## References

The approach follows [OAuth 2.0 Security Best Current Practice (RFC 9700)](https://www.rfc-editor.org/rfc/rfc9700.html), especially PKCE and refresh-token replay prevention, and [Microsoft's Office threading guidance](https://learn.microsoft.com/en-us/visualstudio/vsto/threading-support-in-office?view=visualstudio). API contracts are checked against the [Oura V2 documentation](https://cloud.ouraring.com/v2/docs) and the checked-in synthetic fixtures described in `tests/fixtures/README.md`.
