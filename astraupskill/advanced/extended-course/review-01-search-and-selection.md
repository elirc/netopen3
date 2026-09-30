# Review guide: search, selection and payload identity

Attempt chapters 01–05 before reading this guide. Keep your first answers so that you can explain what changed after source inspection. The goal is not to memorize a particular dictionary implementation. It is to recognize that an endpoint can reconcile two different views of the same identities while preserving a contract owned by only one of them.

## Reviewing UM-01A: a useful source map

Your map should begin with the action's query, skip and take inputs and the synchronous entity-search call. Its result is a paged model of slim entities. The page supplies the identity sequence and Total. Key extraction creates a Guid array from the page items. Template hydration accepts those keys and yields full template models. The shared ordering helper selects hydrated models in requested-key order. Finally, the mapper creates response models and the controller returns them with the original search Total.

The empty branch begins immediately after search. If Items has no elements, the controller constructs an empty paged response with Total preserved and returns without hydration or mapping. Put that branch on the diagram explicitly. A map showing all five stages for every request conceals a real negative-call contract. By contrast, a nonempty search page can become empty after hydration and selection; that path still invokes the mapper with an empty selected sequence.

Do not draw a network boundary simply because the template method returns Task. The local Guid-key overload obtains templates through the repository and uses Task.FromResult. A Task is a representation of completion, not evidence of a remote request. Similarly, do not label entity search as an external ranked index: the injected IEntitySearchService is implemented here through paged database descendants ordered by node text. A different service elsewhere in the tree does not change this action's dependency.

## Reviewing UM-01B: an ordered join by identity

For requested keys B, A and hydrated models A, C, B, the mapper receives B, A. C is omitted because no requested key selects it. The result therefore combines membership filtering with order restoration. A generic sort of every hydrated entity could leave C in the output even if it placed B and A first. The distinction becomes especially useful when diagnosing a response that contains extra items: changing the comparison function alone may not address the selection error.

Give B the alias landing and A the alias article. The expected mapped association is B with landing, then A with article. Checking only the identifier sequence misses a mapper that accidentally swaps aliases. Checking only the set of aliases misses an association error of the same kind. A useful expected result records the pairs, preserving both selection order and the selected object's payload.

Search Total belongs beside, rather than inside, that pair sequence. A page can have two surviving items and Total of nineteen. The controller preserves nineteen. This does not certify that nineteen currently hydrated templates exist; it states what the search stage reported. A reviewer should reject both silently recalculating Total as two and claiming that preserved Total constitutes a cross-service consistency guarantee.

## Reviewing UM-01C: evidence boundaries

Three defensible claims are that the action uses the template object type for search, passes extracted page keys to hydration, and preserves search Total. These can be traced directly in the action. A fourth, narrower implementation observation is that the local search implementation materializes its descendant results into an array. That observation is relevant to repeated enumeration, but it is not a promise that every possible implementation of the injected interface behaves identically.

Questions that remain open include whether a particular HTTP request reaches the action after all policies execute, how model binding handles repeated parameters, and whether search and hydration observe the same database state. Direct action tests do not answer those questions. Record the missing evidence next to each question: a hosted API test, a binding test, or a repository transaction investigation. A precise unanswered question is useful engineering output because it tells the next person what to inspect.

## Reviewing UM-02A: write expectations before mechanism

A compact fixture can carry several independent requirements. Use requested A, B, A, missing and hydrated B-first, extra, A-first, B-later. The expected output is A-first, B-first. Its order follows the first occurrences in the requested sequence. Each key appears once. The extra hydrated object disappears. The missing key creates no placeholder. The earlier hydrated payload wins for duplicate B even though the later B might have a more attractive display name.

Each sentence is a separate requirement and deserves a counterexample. Output B-first, A-first has the right members and wrong order. Output A-first, B-first, A-first repeats a requested duplicate. Output A-first, B-later chooses the wrong payload. Adding extra preserves some order while violating membership. A test suite that checks only the final count cannot distinguish any of the first three failures.

An empty requested array with nonempty hydration should yield no selected entities. Nonempty requested keys with empty hydration should also yield none. Those helper cases are separate from the controller's empty-search short circuit. Do not transfer a helper-level observation about output into a claim about which controller collaborators ran. The method contract and the orchestration contract operate at different levels.

## Reviewing UM-02B and UM-02C: alternatives change policy

A last-wins dictionary assignment selects the last hydrated model for a duplicate key. It can be a valid design in another system, but it changes this helper's current policy. An exception-on-duplicate policy changes successful duplicate reconciliation into failure. Returning every matching hydrated model changes cardinality and can repeat a key. Compare alternatives explicitly instead of treating their data structures as interchangeable optimizations.

A proposed sort by the index of a key in requestedIds often looks close to correct because scrambled unique examples pass. It still needs a membership decision for keys that are absent from requestedIds, a duplicate-request decision and a duplicate-payload decision. If its key lookup is linear inside a comparison, its performance argument also differs from the dictionary approach. The first review question should be whether the outputs satisfy the contract; complexity comes after semantic equivalence.

An implementation may use a seen set, an index map or another structure and still satisfy the contract. Source similarity is not the acceptance criterion. A review that demands the original dictionary line for line merely tests resemblance. A review that checks ordered membership and winning payloads permits safe refactoring while rejecting subtle behavior changes.

## Reviewing UM-03A: the state ledger

Start with hydrated B-first, A-first, B-later and extra. After the first two insertions the dictionary contains B mapped to B-first and A mapped to A-first. TryAdd for B-later fails to replace the stored value. Extra enters the dictionary. The requested sequence A, B, A, missing then drives removal. Removing A emits A-first, removing B emits B-first, the second A finds no entry, and missing finds no entry. Extra remains unused and is never emitted.

The ledger reveals two different jobs. TryAdd establishes the payload winner. Remove establishes single consumption while emitting in requested order. Replacing only Remove with TryGetValue keeps the winner policy but repeats A. Replacing only TryAdd with assignment retains uniqueness but changes B's winner. The ability to isolate those mutations is a sign that you understand the policy rather than just the final example.

Do not explain the order using dictionary enumeration. The helper does not emit dictionary values by iterating the dictionary. It iterates requestedIds and removes corresponding entries. This distinction makes the proof less dependent on incidental dictionary ordering behavior and more closely aligned with the actual output loop.

## Reviewing UM-03B and UM-03C: cost and enumeration

A separate seen set can enforce single output per requested key while a first-wins dictionary retains payload lookup. Under ordinary expected dictionary and set operation costs, building the dictionary visits n hydrated entries and walking the requested sequence visits m keys, for expected work proportional to n plus m. State is proportional to unique hydrated keys, plus any output and seen structures. Specify which quantities your symbols represent instead of using the word linear without context.

The helper enumerates the hydrated input during dictionary construction and walks the requested array during selection. An instrumented enumerable can count the hydrated traversal. Do not include an unrelated earlier enumeration by the controller in the helper's count. Conversely, an end-to-end enumeration experiment must account for every stage it includes. A number without a boundary can be technically accurate and still mislead a reviewer about where work occurs.

Changing from consumption to a seen set is an exercise proposal. It is not a claim that a refactor has already been applied to the shared helper. The course keeps production behavior in place while teaching how to demonstrate equivalence. A useful submission includes its counterexamples and its scope even if the learner decides not to implement the alternative.

## Reviewing UM-04A: count and total

Construct a table with search Total, search-page count, hydration count, selected count and mapped count. These are not five names for one value. Search Total can cover all matching entities across pages. Hydration can contain extras or duplicate keys in a deliberately adversarial fixture. Selection excludes extras and collapses duplicates. Mapping has a separate contract; the controller's regression mock preserves the selected sequence but does not prove every possible mapper implementation's behavior.

Consider search keys A, missing, B with Total nineteen, hydration B, extra, A, and ordinary one-to-one mapping. Counts are three search-page items, three hydrated objects, two selected models and two mapped responses, while Total stays nineteen. Now make hydration empty. Selected and mapped counts become zero, but Total remains nineteen. The action has no refill loop that fetches another search page to replace missing models.

Treat missing hydration as an observation before proposing a cause. Deletion between calls is one plausible scenario; it is not established by the fixture. Different service filters, stale data, or a mock designed to test reconciliation can create the same shape. A strong incident note separates the observed missing identity from the investigated cause.

## Reviewing UM-04B and UM-04C: client and refill proposals

A client can truthfully display the surviving items and a search-reported total while acknowledging that some page items are unavailable. Whether such messaging belongs in this product requires product and API design; the current response does not automatically provide a diagnostic explanation. Avoid inventing a precise reason for missing models in user-facing text. The server has not supplied that reason merely by returning a sparse page.

A refill proposal must bound both additional search requests and total hydrated work. It must also explain how offsets, duplicate keys, changing data and Total behave. Fetching until the desired count appears can fail to terminate when services disagree persistently. A fixed maximum number of attempts makes termination clearer but does not alone solve consistency or pagination semantics. Write an explicit exhausted result before coding the happy path.

Assess compatibility separately from algorithmic correctness. Existing clients may interpret skip and take as a page from the original search sequence. A refill algorithm that silently consumes later pages can change where the next client request starts. A capstone should document that policy and its tradeoff rather than presenting refill as an obvious repair to the existing sparse-page behavior.

## Reviewing UM-05A through UM-05C: identity and mutable inputs

Two model instances with the same Key are duplicate identities for this helper even if their aliases differ and object references are distinct. First means first in the hydrated enumeration. It does not mean earliest creation time, newest version or lexical minimum. A winner fixture needs different payloads so that selecting the wrong instance becomes observable. If both duplicates have identical aliases, the output can hide a policy regression.

Last-wins assignment is easiest to detect with exactly one requested key and two hydrated instances sharing that key. The expected payload comes from the first instance. This minimal case is valuable when a larger generated counterexample fails: removing irrelevant identities makes the cause obvious and the regression easier to maintain.

Mutable keys expose an assumption rather than a documented supported workflow. A dictionary captures the key observed at insertion, while the stored object can later report something else if it is mutated. Do not claim that the helper freezes domain entities or validates key stability. A proposed defensive design could snapshot identity and payload, reject inconsistent models or impose an immutability contract, but each option has cost and compatibility consequences. Record the assumption explicitly before deciding whether a production change is warranted.

## Evidence to retain

Keep a source map, the adversarial fixture table, a state ledger and one minimized counterexample. Together they show that you can follow the action, state the reconciliation contract, explain the implementation and detect a plausible incorrect alternative. A reviewer should be able to reconstruct your expected outputs without executing your candidate implementation. That independence makes the evidence useful when the implementation later changes.
