# Search ordering as a complete engineering lesson

This pack uses Umbraco's template search to teach a common integration problem: one system supplies an ordered list of identities, while another supplies the corresponding entities. The lists can differ. Correct behavior requires an explicit contract for missing rows, extra rows, repeated identities, duplicate payloads and pagination metadata.

Start after the existing [worked change](../03-WORKED-CHANGE.md). You should be comfortable with C# collections, a controller action, an async service call and a unit-test assertion. Plan five or six focused sessions. Finish each with a prediction table or a small reviewable change, rather than reading the entire CMS before testing one boundary.

1. [Route and setup](01-route-and-setup.md): source map, focused command and troubleshooting.
2. [Worked selection traces](02-selection-traces.md): follow identities and payloads separately.
3. [Debugging labs](03-debugging-labs.md): turn apparently correct responses into counterexamples.
4. [Independent exercises](04-independent-exercises.md): test contract properties and extensions.
5. [Solutions and rubric](05-solutions-and-rubric.md): expected reasoning and review standards.
6. [Capstone](06-capstone.md): defend an ordering contract with reproducible evidence.

The [prediction worksheet](labs/README.md) checks small selection examples without building Umbraco. It is a learning aid, not a replacement for tests of the actual helper and controller. The focused .NET project links the real upstream regression source and references the real Management API project.

Treat historical [verification](../VERIFICATION.md) as a dated record. New setup failures, skipped checks or warnings belong in your own notebook. Building referenced projects does not imply that all upstream tests or a deployed back office were exercised.

Continue with the [extended search-integration workbook](extended-course/README.md) for the longer source-grounded route. The completed route contains 23 chapters, 69 exercises, six separate review guides, two capstone briefs, and an isolated actual-method lab with a compatible broken starter.
