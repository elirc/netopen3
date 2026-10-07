# Work through mismatched result sets

Write each prediction down before checking it. The checks use the focused test project from [VERIFICATION](VERIFICATION.md): `dotnet test scripts/template-order-tests/TemplateOrder.Tests.csproj /p:UmbracoBuild=true`, run from the repository root (about four minutes on the recorded run, almost all of it building the Management API). Make every source edit in a disposable branch and restore it with `git checkout -- Umbraco-CMS`.

## Exercise 1 - Extras and missing keys

**Goal.** Requested identifiers are A, missing, B. Hydrated entities arrive as B, extra, A. Predict the old `Array.IndexOf` sort order and the corrected result. Explain why giving extras a *high* sort index would still fail the requirement to exclude them.

**Check.** The corrected result is asserted by `Search_Template_Item_Excludes_Unrequested_And_Missing_Entities_Without_Changing_Total`. To check your "old" prediction, paste the one-liner from [03](03-WORKED-CHANGE.md#before-and-after) over the helper body and run the focused project. The assertion message prints the actual sequence of ids, so compare it with what you wrote.

## Exercise 2 - Duplicates on both sides

**Goal.** Request B, A, B, A. Hydration returns A-first, B-first, B-later, A-later. Predict both the identifiers and the aliases in the response. Name the dictionary operation that chooses the first payload and the one that prevents repeated output.

**Check.** `Search_Template_Item_Collapses_Duplicate_Keys_And_Keeps_First_Hydrated_Entity` asserts the ids, the aliases and `Total` 4. Change `TryAdd` to the indexer (`entitiesById[entity.Key] = entity;`) and predict which of its three assertions fail before you run it.

## Exercise 3 - The empty page

**Goal.** Return an empty search page with `Total` 17. Predict the response `Total` and the number of hydration and mapper calls. Explain why replacing `Total` with `Items.Count` would change what pagination means.

**Check.** `Search_Template_Item_Empty_Page_Preserves_Total_Without_Hydration_Or_Mapping` asserts both, using `Times.Never` on `GetAllAsync` and `MapEnumerable`. Delete the early `return` in `SearchTemplateItemController.cs` (the `if (searchResult.Items.Any() is false)` block starting at line 51) and predict which `Verify` fails first.

## Exercise 4 - Hydration order is not authority

**Goal.** Keep the requested sequence fixed and reverse the hydration order for distinct identifiers. Predict whether the response order changes. Then repeat with duplicate payloads and explain why reversing hydration *can* change the selected alias under the first-payload policy.

**Check.** No existing test reverses hydration order for duplicates. Write the case: copy the duplicate test, reverse the hydrated array, and assert the aliases you predicted (`b-later`, `a-later`). This is a characterization test. It documents the policy rather than proving the policy is right.

## Exercise 5 - Cost

**Goal.** For a large batch, compare repeated `Array.IndexOf` lookups with one dictionary build and one pass over the requested ids. State the average-case complexity assumption and the memory tradeoff. Keep performance reasoning separate from claims about authorization or search-index consistency, which this helper does not establish.

**Check.** Write the answer as two big-O expressions in terms of *h* hydrated and *r* requested items, then compare with [06](06-SOLUTIONS-AND-REVIEW.md). Do not run a benchmark from a unit-test project and call it evidence.

## Course navigation

[README](README.md) / [01-CODEBASE-MAP](01-CODEBASE-MAP.md) / [02-CONCEPTS](02-CONCEPTS.md) / [03-WORKED-CHANGE](03-WORKED-CHANGE.md) / [04-TESTING-AND-DEBUGGING](04-TESTING-AND-DEBUGGING.md) / [05-PRACTICE](05-PRACTICE.md) / [06-SOLUTIONS-AND-REVIEW](06-SOLUTIONS-AND-REVIEW.md) / [07-TRACE-LAB](07-TRACE-LAB.md) / [VERIFICATION](VERIFICATION.md)
