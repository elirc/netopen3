# 19. Build boundaries and reproducible evidence

A focused test command can still compile a large dependency graph. Understanding that graph helps distinguish a selection defect from an environment failure and prevents a learning exercise from turning into an uncontrolled repair of unrelated build infrastructure. This chapter studies the existing outer test project, its central package import, and the nested checkout's SDK policy. It does not ask you to install dependencies or rebuild the full CMS merely to understand the selection rule.

Inspect [TemplateOrder.Tests.csproj](../../../scripts/template-order-tests/TemplateOrder.Tests.csproj), its adjacent [Directory.Packages.props](../../../scripts/template-order-tests/Directory.Packages.props), and the nested [global.json](../../../Umbraco-CMS/global.json). The project targets net10.0, references the real Management API project, and links the real upstream controller test file. Its package references name the test SDK, Moq, NUnit, and the adapter. The adjacent package file imports upstream test versions and removes inherited global package references from this outer harness.

## A linked test is not a copied test

The Compile entry points at the upstream SearchTemplateItemControllerTests source. When that file changes, the focused project compiles the changed file on its next build. This is useful evidence because the harness does not maintain a separate implementation of those controller tests. The Link metadata controls how the source appears in project tooling; it does not create an independent snapshot of the file.

The ProjectReference points at the actual Management API project. That provides real production types and implementation, but also brings its build dependencies. A command aimed at four tests can therefore perform more work than four small methods suggest. Keep the distinction between test discovery scope and compilation scope explicit when estimating cost or diagnosing a failure.

By contrast, the new isolated lab in the next chapter extracts only the current selection method into a small program with a minimal entity interface. It has a smaller dependency boundary and a correspondingly smaller claim. Neither harness is a universal replacement for the other. The full-reference harness tests orchestration against real framework types; the extracted-method lab tests a particular method body without proving project integration.

## SDK selection belongs in the evidence record

The inspected nested global.json asks for SDK 10.0.100, permits latestFeature roll-forward, and disallows prerelease SDKs. That policy is more precise than requiring any installed dotnet command. Record the selected SDK version in a reproduction report. A machine can have several SDKs, and invocation location can affect which global.json is found.

The earlier [route and setup guide](../01-route-and-setup.md) enters the nested checkout before running the focused outer project. This makes the intended SDK context visible and supplies paths used by build metadata. Do not casually move that command to an unrelated working directory and assume every property resolves identically. Relative project paths, SDK lookup, imported files, and repository metadata each have their own resolution context.

If SDK selection fails before compilation, no ordering assertion has run. Record a setup failure with the requested policy and the installed environment. Do not write that the selection tests failed, and do not change the repository's SDK policy just to suppress the mismatch without considering compatibility. The learning objective is to identify the failing stage accurately before choosing a repair.

## Central package management is part of the harness

The package import means versions are inherited from the upstream test configuration rather than repeated in each outer PackageReference. Removing a version pin or adding an arbitrary version locally changes the dependency contract. A restore that succeeds after such a change may be useful experimentation, but it is not evidence that the original harness restored unchanged.

The removal of GlobalPackageReference entries is also deliberate. It limits inherited global build or packaging tasks in the outer harness while retaining the package-version import. This is not the same as removing all package references or all upstream build behavior. Read the actual XML before describing the mechanism. An overly broad explanation can lead a learner to delete the wrong ItemGroup and lose necessary test dependencies.

When troubleshooting, preserve the original configuration and make any experiment in a disposable copy with a recorded diff. A package-source outage, missing cache, incompatible SDK, and compile error are different failure categories. Solving one does not establish that the others are absent. A clean report identifies the earliest failing stage and avoids speculative edits to unrelated files.

## Follow the evidence pipeline

Use a sequence of stages: SDK selection, project evaluation, dependency restore, compilation, test discovery, test execution, and result inspection. A successful exit from one stage is not automatically success at the next. In particular, a build can succeed while no tests are discovered. A test command can also select fewer cases than expected because of filtering or adapter configuration.

The inspected upstream file currently contains four test methods for ordinary ordering, missing and extra entities with total preservation, duplicate keys with first-payload selection, and an empty page that skips hydration and mapping. Record the discovered test count when running that harness. If a future revision adds tests, use the actual current count rather than treating four as a permanent magic number.

Keep warnings separate from assertion results. A warning may be harmless for the focused claim or may identify a configuration concern requiring attention. Suppressing all warnings because the test count looks correct weakens the report. Conversely, a warning alone does not imply an assertion failure. Report what happened, its relevance, and the remaining limitation.

## A worked failure classification

Suppose a command restores packages successfully but fails during compilation of a referenced project. The result is no completed controller test evidence. The useful record includes the compile failure category, the affected project, and whether any test assembly was produced. It should not say zero tests failed in a way that implies success; no tests may have run at all.

Now suppose compilation succeeds and the adapter discovers four tests, but one duplicate-payload assertion fails. This is behavioral evidence at the focused controller layer. The next investigation should inspect the selected payload sequence and helper behavior, not begin by changing package versions. A trace with A-first and A-later can reveal whether dictionary insertion policy changed.

Finally, suppose all four tests pass while the HTTP host was never started. The result supports those direct controller scenarios and the actual referenced code compiled in that environment. It does not prove authorization, routing, serialization, database collation, or deployed behavior. A precise success statement remains valuable because it tells a reviewer exactly which uncertainty has been reduced.

## Reproducibility includes the source boundary

Record hashes or revision identifiers for the helper, controller, test source, and harness configuration when creating an evidence bundle. A commit identifier alone can be insufficient if the working tree contains local changes. The goal is to associate results with the actual bytes that were tested, not to expose repository credentials or collect the whole machine state.

For the disposable lab, its generated source map records the selected method and source-file hashes. That makes an extraction reviewable without copying the whole application. For the full-reference harness, a revision plus relevant working-tree differences and a test result artifact can provide a useful record. Choose evidence proportional to the claim and avoid dumping unrelated files into the report.

## Exercise UM-19A: map the build graph

Draw the outer test project, linked upstream test file, referenced Management API project, package-version import, and SDK policy. Label which arrows compile source, reference a project, import configuration, or select a toolchain. Explain why a focused test selection does not guarantee a tiny compilation graph.

## Exercise UM-19B: classify three failures

Write reports for an unavailable SDK, a referenced-project compile error, and one failed duplicate-payload assertion after four tests are discovered. State which stages completed and what evidence is still missing. Include one next action for each case that targets the observed failure rather than changing unrelated configuration.

## Exercise UM-19C: create a reproducibility manifest

Specify the minimum source, toolchain, command, and result information required to reproduce a focused controller run. Compare that manifest with the smaller extracted-method lab. Explain which claims each supports and how you would record local modifications without including secrets or irrelevant machine data.

## Review checkpoint

A strong submission treats the build as a sequence of evidence-producing stages. It understands linked source and imported versions, preserves repository configuration during diagnosis, and refuses to turn no discovered tests into a passing result. It can explain both the value and the limits of the outer focused harness.
