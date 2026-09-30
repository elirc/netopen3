# Build counterexamples for plausible implementations

Use the focused source and fixtures. Make experimental mutations only in a practice branch/copy and keep your learner work separate from the reference. The exercises below are deliberate bug designs, not claims that these bugs remain in the current helper.

## UMB-L1 — sort extras to the end

A proposed fix says: “If an ID was not requested, use a very large sort index.” Construct a two-request fixture with one extra hydrated identity. Predict the complete response. Explain why a test that checks only the first two IDs could pass even though the API still returns an unauthorized-by-contract extra item. Here “requested” is a selection rule; it does not by itself establish a security authorization policy.

Use an exact ordered sequence assertion. Record the difference between ordering and membership, then identify the requested-ID loop that enforces both in the current helper.

## UMB-L2 — last payload silently wins

Replace dictionary `TryAdd` with indexed assignment in a disposable copy. Hydrate two entities with the same identity and different aliases. Predict which alias survives. A test that asserts only IDs misses this regression. Add an alias assertion and explain whether the product contract wants first or last data, rather than assuming all duplicates are byte-identical.

## UMB-L3 — duplicate requests leak through

Imagine lookup without removal. Requested IDs are B, A, B. Hydrated identities are unique. Predict the output length and aliases. Then compare lookup-only behavior with the current remove-after-emission behavior. The business rule is “each available requested identity once, in its first requested position”; it is not simply “one output per request array element.”

## UMB-L4 — a correct-looking empty response

Use an empty search page whose Total is 17. An implementation replaces Total with Items.Count and returns an empty array. The body looks harmless in a screenshot, but a pagination consumer now believes there are no results. Assert Total separately and verify hydration/mapping were not called. The latter checks the short-circuit behavior, not only response shape.

## UMB-L5 — tests never ran

Take a log from a failed restore or version-calculation attempt in your practice environment. Mark the last completed stage: SDK selection, restore, build, discovery or execution. Do not invent a passing test count from exit status alone. Find a real assertion result or TRX counter before describing a case as exercised.

## Investigation record

For each lab, submit the minimal fixture, expected exact output, observed output, first differing rule and one meaningful assertion. Keep fixtures small enough to trace by hand. A thousand randomly generated rows can be useful later, but a four-row counterexample is easier for a reviewer to understand and preserve as a regression.

Before repairing a failure, state which contract would change if you chose the alternative behavior. That habit separates an implementation bug from a product-policy decision.
