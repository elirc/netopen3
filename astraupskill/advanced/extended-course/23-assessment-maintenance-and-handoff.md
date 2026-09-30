# 23. Assessment, maintenance, and handoff

The final assessment asks whether you can defend a change to this search path without overstating what the code or tests establish. You have followed identities through search, hydration, selection, mapping, response construction, and proposed client behavior. The next skill is maintaining that chain of evidence when source changes, tests are added, or a feature proposal moves toward implementation.

Use the workbook as a set of reasoning tools rather than a list of patterns to apply everywhere. Dictionary removal is appropriate to this one-time-emission contract, but another caller might require multiplicity. A cursor can improve navigation, but it does not automatically establish snapshot consistency. A direct controller test is valuable, but it does not execute authorization middleware. The assessment rewards choosing a boundary deliberately.

## Prepare an evidence portfolio

Collect one source map, one selection trace, one executed disposable lab result, one proposed integration test, and one capstone design. Each artifact should name its claim and limitation. A source map should identify the Guid hydration overload, not merely the method name shared with the alias overload. A trace should identify both keys and winning payloads. A lab report should distinguish reference mode from deliberate starter mutation.

The portfolio does not need production data, credentials, or a running back-office installation. Synthetic identities are enough for collection and orchestration reasoning. If you choose to execute a hosted or database integration test, record the environment and configuration needed to interpret it. Avoid adding unrelated machine information just to make the report look comprehensive.

For each artifact, ask what plausible wrong implementation it would reject. An ordinary order trace rejects returning hydration order. A first-payload fixture rejects last-write-wins dictionary insertion. A duplicate-request fixture rejects lookup without consumption. A test that would pass both correct and plausible wrong behavior is not useless, but its claim should be narrower than a full regression guarantee.

## A structured oral review

Start with the current contract in plain language. Search supplies an ordered key sequence and a total. Hydration supplies full templates without a guarantee that its enumeration matches that sequence. The helper selects first hydrated payloads, emits requested keys once in requested order, and excludes missing and unrequested entities. The controller maps selected templates and preserves the search total.

Then explain the empty branch. An empty search item collection returns a response with the search total without hydration or mapping. A nonempty search page whose keys all fail to hydrate takes a different path: hydration and selection occur, then mapping is called with an empty selected sequence. Both can produce no items, but their collaborator behavior differs. A mature explanation includes this distinction without needing to inspect the final item count alone.

Next identify one evidence boundary you have not crossed. Perhaps you ran the extracted helper but not the controller harness. Perhaps you inspected policy attributes but did not host the API. State the missing evidence directly and propose the smallest useful next test. The assessment is not improved by claiming every layer has been verified when only one has executed.

## Review a hypothetical patch

A patch replaces the helper with entities.OrderBy using each entity's index in requestedIds. At first glance it appears to restore search order. Ask what happens to unrequested entities, duplicate hydrated keys, repeated requested identities, and missing keys. Sorting alone does not implement the same membership and payload policy. A counterexample with an extra entity and two equal-key payloads can reject the patch quickly.

Another patch changes TryAdd to assignment. Ordinary key-order tests may still pass, but duplicate payload precedence changes. A third patch preserves selection but sets response Total to selected.Count. That breaks the distinction between global matching count and page-level hydration. A fourth patch adds a CancellationToken parameter without observing it or forwarding it to any supporting dependency. Its signature changes, but the claimed cancellation behavior remains unproven.

For each patch, write the smallest fixture that exposes the relevant difference. Avoid one enormous test that combines every defect and then reports an ambiguous mismatch. Small counterexamples localize causality. A mixed fixture is still useful after focused cases exist, because it checks interactions among rules, but it should not replace all of them.

## Maintain the course alongside source

Source links identify files, but file existence alone does not guarantee that an explanation remains correct. If the Guid service overload changes, the async and enumeration chapters may need review. If the helper changes duplicate policy, selection traces and lab expectations may need review. If controller inheritance changes, authorization evidence must be revisited. Keep a small dependency map from claims to source boundaries.

The extraction generator intentionally checks recognizable method structure. A refusal after a source change is a prompt to inspect the new method, not an instruction to weaken the check until generation succeeds. If the method gains external dependencies, the isolated lab may need a different boundary or may no longer be the right tool. Preserving a green lab by stubbing away meaningful new behavior would reduce its value.

When updating the workbook, preserve learner artifacts separately from generated reference material. Do not overwrite an existing disposable directory, because it may contain a learner's repair and evidence. Create a new lab instance and compare manifests. This makes it possible to discuss how the source evolved without destroying the earlier learning record.

## A migration note for real changes

If a capstone becomes an application change, rewrite the proposal as a concrete behavior description. State the trigger, previous behavior, resulting behavior, and compatibility impact. Include tests that correspond to the final implementation rather than a history of abandoned designs. A reviewer should be able to understand the change without reading the entire workbook.

For diagnostics, describe the counter definitions, sink boundary, and unchanged response semantics. For continuation, describe ordering, work budgets, token validation, query binding, and expiration. For client state, describe the generation guard and selection identity. Do not bury the central behavior under a list of implementation files. The source diff can show those details; the review note should explain their consequence.

Validation should be proportional and explicit. Record the isolated checks, controller tests, hosted tests, or database tests that actually ran. If a layer remains untested, identify the resulting uncertainty. A known limitation is useful information for a reviewer deciding whether the change is ready for its intended environment.

## Exercise UM-23A: defend the current path

Present the source-to-response contract in five minutes using a mixed fixture with duplicates, missing hydration, and an extra entity. Explain first-payload selection and total preservation. Include the empty-search versus all-missing-hydration distinction. End with one claim that your evidence does not establish and the next test required to establish it.

## Exercise UM-23B: review four plausible regressions

Create a review table for sorting-only selection, last-payload insertion, selected-count Total, and an unobserved cancellation parameter. Give a minimal counterexample or evidence gap for each. State whether the failure belongs to helper behavior, controller orchestration, metadata semantics, or execution control. Avoid claiming an HTTP outcome from a direct method exception.

## Exercise UM-23C: hand off a capstone honestly

Choose one capstone and write a one-page handoff with implemented prototype behavior, proposed application work, executed checks, unexecuted checks, and a compatibility concern. Include a source or generated-artifact identifier sufficient to reproduce the work. Another learner should be able to continue without guessing which features already exist.

## Final assessment rubric

A passing portfolio explains the current implementation accurately, includes a reproducing counterexample, and labels execution scope. A strong portfolio also compares alternatives, identifies a minimal next test, and preserves source and learner work. An exceptional portfolio makes its assumptions easy to challenge and updates them when evidence changes. Word count, diagram count, and a green console line are not substitutes for that reasoning.

The course is complete when its instructional route, exercises, separate reviews, capstone briefs, and bounded verification artifacts are available. Completing the course does not mean that every proposed feature has been implemented in Umbraco. That distinction is the final lesson: useful engineering work includes both concrete changes and a clear account of what remains a design, a hypothesis, or an untested boundary.
