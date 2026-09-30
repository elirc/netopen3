# Prediction lab

Predict each outcome before reading the answer key. These questions isolate the conditions in their prompts. Real requests may be rejected earlier when those assumptions do not hold.

Use Python 3. Copy `answers.template.json` to a new filename in a location you choose, replace each null with your answer, then pass its absolute path to the checker. Copying is a learner action; the checker only reads files and never changes your answers.

From the project root:

```powershell
python astraupskill/advanced/labs/check-answers.py C:/path/to/my-answers.json
```

Use JSON booleans (`true`/`false`), numbers, arrays or objects exactly as requested. Array order matters. Keys in an object may appear in any order. Exit 0 means every prediction matches; exit 1 means one or more answers need review; exit 2 means malformed/missing input.

## Cases

### UMB-P1

Requested [B,M,A]; hydrated A:alpha, X:extra, B:beta. Return output IDs in order.

### UMB-P2

Requested [B,A,B,A]; hydrated A:a-first, B:b-first, B:b-later, A:a-later. Return output aliases in order.

### UMB-P3

Search Items is empty and Total is 17. Return {items: [], total: number, hydrationCalls: number, mapperCalls: number}.

### UMB-P4

Requested [A]; hydration is empty; search Total is 9. What Total remains in the response?

### UMB-P5

Requested [A]; hydrated A:first, A:later. Does reversing hydration necessarily preserve selected alias?

### UMB-P6

Requested [B,A]; unique hydrated identities A and B arrive in either order. Does output always stay [B,A] under this contract?

### UMB-P7

Does sorting an unrequested hydrated identity last satisfy the requirement to exclude extras?

### UMB-P8

Does a passing direct controller mock test establish search-index freshness or back-office HTTP authorization?

## After checking

For each mismatch, name the source function or stated contract that changes your prediction. The checker reports case IDs without printing the answer, so you can make another independent attempt.

Compare with [the answer key](answer-key.json) only after your attempt. The separate chapter answer guide explains the reasoning. This lab grades a mental model and does not compile, start, test or modify the application.

[Return to the advanced course](../README.md)
