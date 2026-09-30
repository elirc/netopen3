# Disposable actual-method selection lab

This generator extracts the current OrderByRequestedIds method from the real management base. It adds a minimal IEntity interface with Key and a synthetic test harness. It does not compile the full controller, test HTTP, or exercise database queries. Reference mode preserves the extracted method; starter mode deliberately replaces removal with lookup only in the new disposable copy.

Run from the outer netopen3 workspace with Python and an installed .NET 10 SDK. These commands use new temporary directories and do not modify application files. No external NuGet package references are added; package sources are cleared in the generated project. SDK reference packs must already be available.

```powershell
$selectionLab = Join-Path $env:TEMP ("template-selection-reference-" + [guid]::NewGuid().ToString('N'))
python astraupskill/advanced/extended-course/lab/generate_selection_lab.py --mode reference --output $selectionLab
dotnet run --project (Join-Path $selectionLab 'Lab.csproj')

$selectionStarter = Join-Path $env:TEMP ("template-selection-starter-" + [guid]::NewGuid().ToString('N'))
python astraupskill/advanced/extended-course/lab/generate_selection_lab.py --mode starter --output $selectionStarter
dotnet run --project (Join-Path $selectionStarter 'Lab.csproj')
```

Expected reference result: six passed, zero failed, exit zero. Expected starter result: five passed, one failed, exit one. The starter failure concerns repeated requested identities; compilation should succeed. Repair Selection.cs only inside the disposable starter directory, then rerun it. Preserve SOURCE-MAP.json so the original extraction and deliberate mutation remain reviewable.

An existing output directory, a relative output path, or a path inside netopen3 is refused. A changed helper signature or unsupported extraction structure is also refused. Do not bypass these checks by overwriting a previous experiment. Generate a fresh directory after reviewing the changed source.

The six checks cover order, missing and extra entities, repeated requested identities, first hydrated payload reference, single enumeration, and empty selection. They do not verify response Total, controller short-circuiting, mapper behavior, authorization, serialization, or database ordering. Use the focused controller project and appropriately configured integration tests for those claims.

## Recorded completion check

The completion pass executed both generated modes: reference six passed with exit zero; starter five passed and the intentional duplicate-request check failed with exit one. Repairing only the disposable starter restored six passes. Existing-output, relative-output, and in-source-output refusal checks passed. This evidence covers the extracted method and generator boundary, not the full application.
