# 03. Dictionaries and consumption

The helper in [ManagementApiControllerBase](../../../Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/ManagementApiControllerBase.cs) uses a dictionary in two phases. First it indexes hydrated entities by key with TryAdd. Then it walks requested identities and removes each matching entry while appending the removed entity to the output. Understanding the changing dictionary state is more useful than memorizing the final three lines.

## Trace the build phase

Use hydrated sequence A1, C1, A2, B1, where the letter is identity and the number distinguishes payload occurrence. After A1, the dictionary contains A to A1. C1 adds C. A2 does not replace A1 because TryAdd fails for an existing key. B1 adds B. The dictionary contains the first payload for each unique hydrated identity, regardless of request membership at this stage.

That last point matters: the current helper indexes extras too and later omits them because no requested key removes them. A future optimization could filter before indexing, but it must preserve first-payload and request-order semantics and should be justified by workload. Do not describe the existing dictionary as containing only requested entities unless the source actually filters them first.

## Trace the consume phase

Now request B, A, B, M. Removing B returns B1 and appends it. Removing A returns A1 and appends it. The second B removal fails because B is already absent. M is missing and also fails. C remains in the dictionary but is never emitted. Output is B1, A1.

Removal therefore performs two jobs: lookup and recording that the identity has been consumed. A lookup without removal would allow repeated requested keys to emit duplicates unless a separate seen set were added. The current design keeps that state in one structure. Explain this as a behavioral mechanism rather than calling Remove a micro-optimization without context.

## Complexity with meaningful variables

Let n be the number of hydrated entities and m the number of requested identities. Under ordinary dictionary operation assumptions, the two traversals give expected linear work in n plus m, with storage proportional to unique hydrated keys and output size. State those assumptions rather than promising a strict constant-time hash operation for every possible implementation and adversarial input.

A nested scan that searches the hydrated sequence for every requested key can perform work proportional to n times m and may repeatedly enumerate a deferred source. Sorting hydrated entities by repeated Array.IndexOf calls can also add substantial lookup work while still failing the membership or duplicate contract. Complexity and correctness should be reviewed together; a fast wrong policy is not an improvement.

## Exercise UM-03A: write a state ledger

For hydration C1, A1, C2, B1, X1 and requests B, C, B, Z, A, record the dictionary after every insertion and removal and the output after every request. Include failed TryAdd and failed Remove events. The final output should be derived from the ledger rather than guessed from visual sorting.

Then explain which entries remain unused and why their presence does not leak into the result. This tests whether you understand the difference between indexing all available data and selecting requested data. A dictionary's enumeration order is irrelevant to the final order because output follows the requested loop.

## Exercise UM-03B: replace consumption with a seen set

In a disposable practice implementation, keep the dictionary intact and use a separate emitted-key set. Specify the order of checking membership and recording emission so missing requested keys do not produce incorrect behavior. Compare outputs with the current helper over your contract table. This exercise teaches equivalence reasoning, not a required production refactor.

Discuss tradeoffs: additional state, readability, potential reuse of the full lookup and ease of diagnostics. The current consuming dictionary is compact, while a retained lookup may help a future report of missing or unused identities. A design choice should follow an actual requirement rather than a preference for one collection operation.

## Exercise UM-03C: investigate enumeration counts

Wrap a synthetic hydrated enumerable with a counter and observe how many times the helper enumerates it. Compare with a naive per-request scan. Use a sequence whose enumeration is observable but deterministic. Do not infer database query counts from this wrapper unless the actual repository sequence is known to execute a query per enumeration.

This distinction is important in a large codebase: IEnumerable describes an iteration capability, not whether the source is a list, deferred query, generator or remote operation. Performance claims need the concrete source and boundary. The helper's explicit loop gives you one fact, while repository materialization behavior requires separate inspection.

## Review standard

A strong answer shows first-payload retention during insertion and uniqueness through consumption during requested traversal. It uses n and m correctly, avoids relying on dictionary enumeration order, and distinguishes enumerable operations from database effects. The final ledger should make every emitted and omitted payload explainable.
