# 10. Independent oracles and properties

An expected result should be derived independently enough to catch the implementation's likely mistakes. For the ordering helper, a simple slow oracle can be easier to trust than a second dictionary implementation. Walk requested identities in order, skip identities already emitted, and find the first hydrated object with that identity by a straightforward scan. This expresses the contract differently from the production build-and-remove algorithm.

## Independence is about failure modes

If both implementation and expected-value code use dictionary assignment, both can accidentally choose the last duplicate payload. A green comparison then proves agreement, not the intended first-wins rule. A hand-written table and a differently structured oracle reduce that shared mistake. Review the oracle against small examples before using it to assess generated cases.

A slow oracle is acceptable for small finite tests. It does not need production performance because its purpose is clarity. Use it over a bounded identity universe and short sequences, then compare the actual helper or a clearly labeled practice implementation. Report exactly which implementation ran. A model-only comparison is not proof that the repository helper compiled and executed.

## Useful properties

Every output key belongs to both requested and hydrated key sets. No output key repeats. Output order follows the first requested occurrence of each surviving identity. The selected payload is the first hydrated occurrence for that key. Output length is no greater than either the number of distinct requested keys or the number of distinct hydrated keys. Adding an unrequested hydrated entity should not change output under stable existing payload order.

Properties can overlap without being redundant. Membership and uniqueness alone do not establish order. Order and count alone do not establish the payload winner. A suite should cover all dimensions rather than assume one broad equality check explains its own failures. When a generated case fails, print the compact input and expected/actual identities and payload labels.

## Metamorphic relations

A metamorphic relation describes a controlled input change and the expected output relationship. Appending a later duplicate payload for an already present key should not change the selected payload. Appending a duplicate requested identity after its first occurrence should not add another output. Removing an unrequested hydrated entity should not change output. Moving the first requested occurrence can change output position without changing payload choice.

These relations are stronger than cosmetic renaming. They probe the exact policy boundaries. Avoid claiming that arbitrary permutation of hydration leaves output unchanged, because moving duplicate payloads can change which one is first. A property must preserve the assumptions under which it is true.

## Exercise UM-10A: build a finite universe

Use three identities A, B and C and requested sequences of length zero through three. Generate hydrated sequences over the same identities, labeling each occurrence with its position so duplicates remain distinguishable. Add one outside identity X for membership tests. State the number of generated cases and the maximum size, and keep the run deterministic.

Compare a slow oracle with the actual helper through an appropriate harness, or label a practice implementation honestly if the full project cannot run. Preserve failing cases in a readable form. A single minimal counterexample is often more educational than thousands of unexplained failures.

## Exercise UM-10B: design mutations

Evaluate four deliberate wrong implementations in a disposable copy: last payload wins, repeated requested keys emit repeatedly, extras remain after sorting, and output sorted by alias. Identify the smallest fixture that catches each. Then show whether your property set catches it without relying on a manually chosen example.

If a mutation survives, inspect whether the property set is incomplete or the mutation is equivalent under the tested input domain. Do not automatically label every surviving mutation a production defect. The analysis should explain the missing distinction and improve the experiment accordingly.

## Exercise UM-10C: shrink a counterexample

Suppose a failing generated case has many keys. Remove requested occurrences and hydrated payloads while preserving the failure until the example is small enough to explain. Record why each remaining element is necessary. This manual shrinking exercise teaches you to turn a noisy test failure into an actionable regression case.

## Review standard

A strong property investigation uses an independently reviewed oracle, a finite declared domain, meaningful metamorphic relations and readable counterexamples. It distinguishes model checks from execution of repository code and explains the plausible wrong implementation each property can reject.
