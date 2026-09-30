# Find the narrow path through a large repository

## Placement and source map

Before reading the helper, answer: if search requests B then A and hydration returns A then B, which order should the response use? If hydration also returns X, is sorting X last sufficient? If the search reports total 17 but the current page has no surviving entities, should total become zero?

Locate these four sources:

| Boundary | Source |
|---|---|
| Search, hydration and response metadata | [SearchTemplateItemController](../../Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/Template/Item/SearchTemplateItemController.cs) |
| Identity ordering helper | [ManagementApiControllerBase](../../Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/ManagementApiControllerBase.cs) |
| Actual controller regression cases | [SearchTemplateItemControllerTests](../../Umbraco-CMS/tests/Umbraco.Tests.UnitTests/Umbraco.Cms.Api.Management/Controllers/Template/Item/SearchTemplateItemControllerTests.cs) |
| Focused test project | [TemplateOrder.Tests.csproj](../../scripts/template-order-tests/TemplateOrder.Tests.csproj) |

Session 1 ends with a three-arrow map: search identities, hydrated entities, ordered mapped response. Session 2 predicts the traces. Session 3 runs the focused checks. Session 4 adds one independent contract case. Session 5 presents the capstone and a limitation.

## Reproduce the focused command

Run from the outer `netopen3` workspace in PowerShell. The nested repository's [global.json](../../Umbraco-CMS/global.json) defines its SDK policy. Entering that directory before invoking dotnet makes the intended SDK selection visible. Confirm the SDK is installed; a missing SDK is a setup failure, not a failing ordering test.

```powershell
Push-Location Umbraco-CMS
try {
    dotnet --version
    $upskillGitRoot = (Get-Location).Path
    dotnet test ../scripts/template-order-tests/TemplateOrder.Tests.csproj `
      /p:UmbracoBuild=true `
      "/p:GitVersionBaseDirectory=$upskillGitRoot" `
      "/p:GitRepoRoot=$upskillGitRoot" `
      --nologo --verbosity minimal /m:1 --logger trx
} finally {
    Pop-Location
}
```

This is a reproduction command to run in your learning environment, not a claim of a new .NET run during this documentation expansion. It can restore packages and compile referenced projects. Use `--no-restore` only after the relevant assets have successfully restored.

## Troubleshooting by stage

If SDK selection fails, inspect the requested SDK policy and installed SDKs. If restore fails, inspect package sources and [Directory.Packages.props](../../scripts/template-order-tests/Directory.Packages.props), which imports upstream test versions. Do not remove version pins merely to obtain a green command.

If GitVersion fails, verify that the two metadata paths point at the actual nested checkout. The focused project lives outside it, so a path guessed from the project file can be wrong. If frontend or packaging work starts unexpectedly, verify the `UmbracoBuild` property and that you selected the focused project. Do not count an interrupted packaging operation as an executed test.

Finally inspect the test summary and TRX. A successful restore/build without discovered tests is not ordering evidence. Record the number of distinct cases and any warning separately. The [earlier verification record](../VERIFICATION.md) explains why this harness avoids unrelated packaging tasks; it does not justify suppressing a new failure you observe.
