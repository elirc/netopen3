# 20. An isolated source lab and an integration ladder

This chapter provides a small executable boundary between reading the helper and building the full Management API project. The [lab generator](lab/generate_selection_lab.py) reads the current OrderByRequestedIds method from the actual management base, validates its expected structural anchors, and writes the exact extracted method into a new external directory. It never edits the application. The generated project uses a minimal IEntity contract containing only Key, because that is the member this method reads.

This is an extracted-method test, not a compiled copy of the whole controller. That distinction is essential. It establishes behavior of the current method text under synthetic entities. It does not establish that the original class compiles with every upstream dependency, that controller orchestration is correct, or that the HTTP pipeline behaves as intended. The existing focused project remains the appropriate next layer for real controller types.

## Why extraction can be useful and limited

The helper is self-contained around a dictionary, a requested-key array, and entity keys. Testing its exact body can expose ordering, omission, duplicate, and enumeration mistakes without requiring a database or framework host. A handwritten reimplementation would be weaker evidence about the actual helper because it could reproduce the intended algorithm while the application differs. Extraction keeps the body tied to current source.

The generator still supplies a surrounding class and a minimal interface. Those substitutions limit the claim. It does not reproduce every IEntity member or framework base-class behavior. If the helper evolves to depend on another member or external service, generation or compilation should fail rather than silently inventing a replacement. A narrow lab is useful only when its boundary remains visible.

The generator records both the source-file hash and the extracted-method hash in SOURCE-MAP.json. The hashes identify what was read at generation time. They do not prove provenance by themselves, but they allow a reviewer to compare the generated method with the source and recognize that an old lab may no longer match a changed checkout. Regenerate into a new directory when source changes; do not overwrite an existing experiment.

## Run in a disposable directory

Follow the concrete commands in the [lab README](lab/README.md). Supply a new absolute output path outside netopen3. The generator refuses an existing path and a path inside the source workspace. These refusals protect both application files and an earlier learner experiment. They also make it clear which generated files belong to each run.

The generated project has no external PackageReference and uses the installed .NET SDK. It may create ordinary SDK build outputs in its disposable directory. The generator does not install an SDK, launch a server, or restore the full Umbraco dependency graph. If a suitable SDK is absent, report that setup limitation rather than changing the application's toolchain policy.

The reference mode preserves the extracted method exactly. The starter mode applies one checked transformation in the disposable copy: dictionary removal becomes lookup without removal. The method signature remains unchanged and the starter still compiles. That defect causes repeated requested identities to be emitted repeatedly. The source map labels the deliberate mutation so nobody mistakes the starter for an observation of current application behavior.

## Predict the checks before running

The ordinary-order check requests B then A while hydration arrives A then B. The result must follow B then A. The missing-and-extra check includes an unavailable requested key and an unrequested hydrated entity; neither belongs in the selected output. These checks separate reordering from filtering, because sorting all hydrated entities would still include the extra.

The duplicate-request check requests the same available key more than once and expects one emission. The duplicate-payload check hydrates two distinct objects with the same key and expects the first object reference. Testing reference identity is useful because both objects have the same key and a key-only assertion cannot distinguish first from last payload selection.

The single-enumeration check uses a source that throws if enumerated twice. It protects the current one-pass consumption of hydrated entities. The empty-request check expects an empty selected list. Notice that the helper still builds its hydration dictionary before traversing the empty requested array; this is different from the controller's empty-search early return, which skips hydration entirely. The lab must not blur those two boundaries.

## Diagnose the intentional starter failure

In starter mode, lookup succeeds for every repeated requested occurrence because the dictionary entry remains present. A request B, A, B emits B twice. The failure is behavioral, not a compiler mismatch or a stale interface signature. Repair the copied method by restoring consumption semantics, then rerun the generated project. Keep the application source unchanged.

Do not repair the test by deleting repeated requested identities from its fixture. The repeated identity is the counterexample that reveals the defect. Nor should you claim that deduplicating the request at an unrelated layer is the only correct repair. Within this isolated exercise, the intended method contract includes one emission per requested identity and the existing removal operation enforces that rule directly.

After repair, compare the copied method with the source-mapped reference. If you choose a different implementation, prove the same output and payload policy with the checks and an independent trace. A passing test set is evidence for its cases, not a proof over every possible input. Explain why your algorithm handles missing keys, extras, and duplicates generally, and identify any assumptions about nulls or entity mutation that are outside this exercise.

## Climb the integration ladder deliberately

The first layer is a paper trace. It makes the intended sequence and payload winner explicit. The second is this extracted-method lab. It executes current selection text with synthetic entities. The third is the existing focused controller harness, which links actual upstream test source and references actual production projects. The fourth is a hosted API test with routing, policies, serialization, and configured services. A database-aware layer adds query and storage evidence.

Each layer should answer a question the earlier one cannot. The extracted method cannot verify Total because it does not receive Total. A controller test can verify Total preservation but not database collation when search is mocked. A hosted test with a mocked search service still cannot establish the actual database's equal-name ordering. Stating those limits helps choose the next test instead of treating every passing result as equivalent.

Cost should follow uncertainty. If the defect is repeated emission in a pure helper, an isolated test is fast and diagnostic. If the concern is a policy attribute on the wrong inheritance branch, use metadata or hosted evidence. If the concern is offset movement during concurrent database updates, design a storage-aware schedule. Running a large suite without connecting it to the claim can consume time while leaving the important question unanswered.

## Exercise UM-20A: repair the compatible starter

Generate reference and starter labs into separate new external directories. Predict which check fails in starter mode before running it. Repair only the copied starter source and record the before-and-after result. Include source-map hashes and explain why the intentional mutation does not represent a defect left in the application.

## Exercise UM-20B: identify the evidence ceiling

For each claim, choose the minimum useful layer: first hydrated object wins; response Total stays nineteen; unauthenticated requests cannot reach search; equal names page deterministically in the configured database; a mapper exposes the expected JSON field names. Explain why the isolated lab cannot establish the latter four by itself.

## Exercise UM-20C: add an independent counterexample

Design one additional fixture that would reject a plausible wrong selector not already isolated by the starter mutation. Write the expected payload sequence by hand before implementing the assertion. Keep the fixture small, avoid external services, and explain which broader claims remain untested even if the new check passes.

## Review checkpoint

A complete submission preserves source, uses a new explicit output directory, and distinguishes reference observations from deliberate starter defects. It records actual execution instead of copying an expected result into the report. Most importantly, it can say exactly where the isolated lab's evidence ends and which next layer would answer a different question.
