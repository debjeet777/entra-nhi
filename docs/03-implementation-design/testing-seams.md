# Testing Seams

> **Phase:** 0.3.8
> **Package:** Implementation Design
> **Status:** Proposed — design only, no tests or test projects created
> **Date:** 2026-09-18

---

## 1. Status and scope

- **Status:** Proposed testability contracts. This document is design
  output only. It authorizes no implementation and creates no tests,
  fixtures, or test projects.
- **Scope:** This document owns the testability contract catalog: the
  required replaceable/controllable seams per architectural boundary, the
  seam-by-seam test scenarios derived from the testing architecture
  (including false-PASS/false-FAIL, tenant-isolation, injection, and
  failure-injection coverage), synthetic-fixture policy, golden/snapshot
  policy, and the requirement-to-test traceability hook.
- **What this document is not:** It is not a test suite, fixture set,
  framework selection, CI workflow, package selection, or numeric coverage
  claim. Detailed type, schema, and seam-placement definitions belong to
  their owning documents.
- **Ownership (per `README.md` §§5–6):**
  - This document owns: seam-by-seam testability requirements, scenario
    catalogs, fixture policy, snapshot policy, and traceability
    expectations.
  - This document references without redefining: the five-state semantics
    (`core-contracts.md` §4), rule pipeline and evaluation surface
    (`rule-engine-contracts.md`), capability states and transitions
    (`capability-model.md`), collector envelopes and completeness
    (`collector-contracts.md`), graph integrity rules
    (`identity-graph-contracts.md`), evidence/provenance schemas
    (`findings-evidence-schema.md`), error/result layers and failure
    taxonomy (`error-result-model.md`), authentication boundaries
    (`authentication-design.md`), output projection rules
    (`output-renderer-contracts.md`), project/dependency direction
    (`solution-structure.md` §§4–10), and per-seam interface placement
    (`dependency-boundaries.md`).
- **Language discipline:** Per `README.md` §1, this document distinguishes
  (a) **approved architectural requirements** traced to requirement/
  invariant IDs, (b) **proposed testability contracts** owned by this
  document, and (c) **unresolved TBDs** naming the owning later concern
  (§17). Nothing herein claims a test is implemented, passing, or
  enforced. Where architecture states `DESIGNED / REQUIRED`, this document
  preserves that classification.

---

## 2. Testing-seam philosophy

Testing is an architectural requirement, not only a later test-project
concern (testing-architecture §§1–2; INV-02–INV-16 coverage).

1. Every architectural boundary is expressed as an **explicit seam**: a
   dependency-inversion point where a fake, mock, stub, or synthetic
   fixture can substitute for a real collaborator without changing the
   unit under test.
2. Seams MUST permit Core/Application verification with **synthetic
   fixtures only** — no live tenant, no production credentials, no network
   access for core tests.
3. Security properties (false-PASS resistance, tenant isolation, secret
   exclusion, evidence integrity, failure transparency, renderer
   non-authority, AI non-authority) REQUIRE explicit verification. Absence
   of a test is not evidence of security.
4. Failure scenarios are first-class: collection, normalization, graph,
   rule, authentication/authorization, output, cancellation, and resource
   exhaustion behaviors each have dedicated seams and scenarios.
5. Negative and adversarial coverage is mandatory: tests MUST attempt to
   provoke incorrect PASS, incorrect FAIL, cross-tenant contamination,
   credential leakage, and output injection — and prove the invariants
   hold.
6. No seam, fixture, mock, or test configuration MUST weaken or bypass
   production security boundaries, introduce insecure fallback paths, or
   leak into production behavior (INV-16).

---

## 3. Core deterministic seams

Each seam below MUST be replaceable/controllable independently, so tests
can vary one dimension while holding the others fixed. Seam interface
placement belongs to `dependency-boundaries.md`; this document fixes the
testability obligations.

| # | Seam | Control obligation |
| --- | --- | --- |
| S-01 | Assessment-time reference | Tests MUST be able to supply a fixed deterministic time reference with the assessment scope and vary it explicitly. Rule logic MUST NOT read wall-clock; time-dependent policy is exercised only through this seam. Representation owned by `domain-types.md`; mechanics owned with `rule-engine-contracts.md`. |
| S-02 | Normalized identity inputs | Tests MUST be able to substitute fully synthetic normalized identity records (all documented kinds) including edge cases: present values, confirmed-empty, missing optional fields, malformed values, duplicates, conflicts, unknown kinds. Shapes owned by `domain-types.md`. |
| S-03 | Capability observations | Tests MUST be able to set per-category, per-subject capability/observation conditions covering every conceptual condition (observed, not-requested, authorization-limited, licensing/service-limited, unsupported, failed, indeterminate) with reason/provenance. Vocabulary owned by `capability-model.md` §§5–6. |
| S-04 | Collection completeness | Tests MUST be able to declare complete, confirmed-empty, partial, unconfirmed/indeterminate, and failed completeness per category/subject, with the missing subset and its cause identified. Semantics owned by `collector-contracts.md` §§7–8 and `capability-model.md` §§9–10. |
| S-05 | Identity graph | Tests MUST be able to substitute synthetic graph topologies: observed vs derived edges, cycles, self-references, disconnected subgraphs, high-degree nodes, deep structures, duplicates, unknown relationship types, incomplete construction. Surface owned by `identity-graph-contracts.md`. |
| S-06 | Rule registration / rule set | Tests MUST be able to register a controlled rule set (fixed IDs/versions/definitions, disabled-rule markers, incompatible-version cases) isolated from production rule content. Contracts owned by `rule-engine-contracts.md` §11. |
| S-07 | Deterministic configuration | Tests MUST be able to supply explicit deterministic configuration and rule-set/version selections, including invalid-configuration and unknown-option cases that MUST fail visibly. Surfaces owned by `configuration-design.md`; consumption owned by `rule-engine-contracts.md` §3.8. |
| S-08 | Evidence / provenance | Tests MUST be able to supply, withhold, corrupt (adversarially), and inspect evidence/provenance references end to end, verifying linkage, preservation, structured reasons, and fabrication defense. Schemas owned by `findings-evidence-schema.md`. |
| S-09 | Cancellation | Tests MUST be able to inject cancellation at each long-running boundary (collection, graph construction, rule-set evaluation, rendering) and observe explicit cancellation recording with affected scope — never silent success and never a new assessment state (`error-result-model.md` §7). No framework mechanism is selected here. |
| S-10 | Resource-limit behavior | Tests MUST be able to exercise bound exhaustion (large synthetic sets, pathological graphs, oversized strings, duplicate-heavy input) and observe visible failure with the bound and scope identified — never PASS by truncation. Numeric bounds are TBD (§17, T-04); the seam obligation (injectable limits, observable exhaustion) is fixed here. |

---

## 4. Provider isolation seam

1. Tests MUST be able to exercise Core/Application logic with **no
   provider access**: no network, no live service, no provider SDK
   presence on the Core evaluation path.
2. Provider adapters (the single provider-coupled project per
   `solution-structure.md` §11) MUST be independently testable using
   **synthetic/sanitized provider doubles**: synthetic provider responses,
   pagination sequences, throttle/timeout signals, malformed responses,
   and duplicate/conflicting observations — without inventing provider
   endpoint, permission, SDK, or payload-schema claims.
3. Contract tests MUST verify that provider-specific types cannot cross
   the normalization boundary into deterministic rule contracts except as
   explicitly normalized capability/provenance data (INV-03; INV-04).
4. No production tenant data is required for normal automated tests. Any
   future captured provider fixture MUST undergo explicit security review
   and sanitization before repository inclusion; the default baseline is
   fully synthetic (§15).

---

## 5. Authentication seam

Authentication and provider-authorization behavior MUST be testable
without embedding real credentials, tokens, secrets, keys, or production
tenant data. Without inventing concrete authentication implementation
(no flows, SDKs, permission names, token-cache/storage mechanisms, or
scope strings are chosen here — all TBD per §17):

1. **Success boundary:** a synthetic authorized access context sufficient
   for collection to proceed, carrying tenant scope and execution-mode
   context but no secret material. Shape owned by
   `authentication-design.md`.
2. **Authentication failure:** no usable authorized context can be
   established (systemic cause). Tests MUST prove the run fails safely
   and visibly assessment-wide and MUST NOT produce per-rule PASS or
   tenant-security FAIL (`error-result-model.md` F-02 mapping).
3. **Authorization limitation:** an authorized context exists but lacks
   access for scoped categories. Tests MUST prove dependent rules yield
   NOT_EVALUATED with reason (never PASS, never automatic FAIL) and that
   unaffected categories still evaluate.
4. **Cancellation during authentication/authorization establishment:**
   tests MUST prove early termination is recorded explicitly with scope
   and is never rendered as a completed assessment.
5. Tests MUST verify: authentication success does not equal authorization
   for every capability; tokens/credentials do not enter normalized
   state, graph, findings, evidence, reports, or logs; no privilege
   escalation or broader-scope requests occur silently; and no insecure
   fallback is introduced by test doubles.

---

## 6. Collector test seams

Collector boundaries MUST enable deterministic simulation of, at minimum
(semantics owned by `collector-contracts.md`; failure categories owned by
`error-result-model.md` §5):

1. **Complete collection** — observations with correct provenance and an
   observed channel; downstream rules may evaluate normally.
2. **Confirmed-empty** — zero observations where collection semantics
   justify a completeness claim; tests MUST prove the distinction from
   case 3 is preserved.
3. **Unconfirmed / unknown completeness** — zero or partial observations
   where completeness cannot be established; tests MUST prove downstream
   rules yield NOT_EVALUATED (or ERROR per mapping), never PASS.
4. **Partial pagination** — multi-page sequences interrupted mid-stream
   (page failure after earlier pages, repeated/invalid continuation
   signals, dropped pages); tests MUST prove partial state is preserved
   with scope and cause, never reported as complete.
5. **Authorization limitation** — scoped denial; tests MUST prove the
   capability condition is preserved with reason and downstream rules
   yield NOT_EVALUATED.
6. **Provider failure** — service-unavailable, transport, timeout, and
   malformed-response doubles; tests MUST prove structured failure with
   sanitized cause, never silent empty success.
7. **Throttling / retry outcomes where applicable** — bounded-retry
   doubles including exhaustion; tests MUST prove the bound and original
   cause chain are recorded and flow into NOT_EVALUATED or ERROR, never
   PASS. No retry counts, delays, or thresholds are chosen here.
8. **Malformed provider data** — type-confusion, missing properties,
   conflicting observations, unexpected extensions; tests MUST prove
   validation failure surfaces as ERROR or NOT_EVALUATED downstream with
   absent semantics preserved (never silently filled or inferred).
9. **Cancellation** — mid-collection termination; tests MUST prove the
   cancelled scope is recorded explicitly and downstream scope is not
   evaluated or rendered as complete.
10. **Cross-tenant contamination attempt** — observations carrying a
    foreign tenant context; tests MUST prove rejection as an integrity
    failure, never silent mixing.
11. **Resource exhaustion** — oversized pages, excessive record counts,
    anti-loop guard trips; tests MUST prove visible failure with bound
    and scope, never truncated PASS.

Collector tests MUST additionally verify: failures never silently become
successful empty collections; collectors never generate findings and
never determine PASS/FAIL; output carries required provenance/context;
no secret material enters observations; and provider data remains outside
the deterministic rule layer.

---

## 7. Rule-engine test seams

Isolated rule evaluation MUST be exercisable with fully controlled:

- Normalized subjects (synthetic identities across documented kinds,
  including malformed and conflicting cases).
- Relationships and graph projections (synthetic topologies per S-05,
  including pathological structures).
- Capability state (every conceptual condition per S-03 on required
  data).
- Provenance (present, missing, and adversarially inconsistent cases).
- Deterministic configuration and rule-set/version selection (including
  invalid and unknown-option cases).
- Assessment-time reference (fixed values varied explicitly).

Scenarios MUST cover the full rule-state matrix (testing-architecture
§8.1): PASS only with affirmative conditions plus proven completeness;
FAIL only with affirmative deterministic failure evidence; NOT_EVALUATED
for unavailable/insufficient capability or data with reason;
NOT_APPLICABLE for genuine non-applicability with reason; ERROR for
untrustworthy execution with cause. Coverage MUST include per-rule
exception isolation, shared-state integrity-failure behavior, malformed
rule input, capability-state combinations, missing provenance/evidence
handling, deterministic ordering where semantically required, and proof
that rule logic performs no provider calls and admits no AI/model
influence.

---

## 8. False-PASS adversarial tests

The following negative/adversarial cases are **release-gate material**.
Each MUST prove that the degraded input cannot incorrectly yield PASS
(and, per testing-architecture §9.2, the symmetric false-FAIL cases
MUST prove degraded inputs cannot incorrectly yield tenant FAIL):

| Attack path | Required outcome |
| --- | --- |
| Incomplete collection on required data | NOT_EVALUATED or ERROR, never PASS |
| Authorization limitation on required data | NOT_EVALUATED, never PASS |
| Unsupported capability on required data | NOT_EVALUATED (or NOT_APPLICABLE only where applicability genuinely excludes the subject), never PASS |
| Collection error on required data | ERROR or NOT_EVALUATED, never PASS |
| Normalization failure affecting required input | ERROR, never PASS |
| Graph failure affecting required input | ERROR, never PASS |
| Resource exhaustion / bound truncation | Visible failure, never a completed successful assessment and never PASS by truncation |
| Cancellation of required scope | Visible cancellation consequence (NOT_EVALUATED or ERROR with reason), never a completed successful assessment |
| Missing evidence for a PASS claim | PASS MUST NOT be emitted; completeness unproven means no PASS |
| Missing provenance / fabricated-evidence attempt | Represented explicitly / rejected; evidence MUST NOT be fabricated |
| Unknown / indeterminate completeness | NOT_EVALUATED (or ERROR where trust is broken), never PASS |

Verification rules:

- Every false-PASS path MUST be executed against the **full pipeline**,
  not only isolated components.
- ERROR MUST remain distinct from FAIL at every boundary;
  NOT_EVALUATED MUST remain distinct from PASS.
- Authentication/system failure MUST produce ERROR or NOT_EVALUATED,
  never tenant FAIL; missing data alone MUST never produce FAIL.

---

## 9. Tenant-isolation tests

Tenant isolation is a release-critical test property (INV-10). Tests MUST
demonstrate that identities, relationships, evidence, and findings from
different tenants cannot be combined, at minimum:

1. Tenant A observations cannot normalize into tenant B scope.
2. Tenant A nodes cannot join tenant B nodes (no cross-tenant graph
   edges).
3. Tenant A evidence cannot support tenant B findings.
4. Tenant A capability state cannot influence tenant B evaluation.
5. Tenant A diagnostics/artifacts cannot mix with tenant B
   diagnostics/artifacts.
6. Runtime tenant mismatch fails safely and visibly as an integrity
   failure (never a verdict over mixed scope).
7. Sequential assessments do not leak tenant state; caches, where later
   introduced, are tenant-safe.
8. Output artifacts are scoped to a single tenant per run.

Each scenario MUST use synthetic tenant contexts only and MUST assert
explicit rejection or safe visible failure — never silent mixing.

---

## 10. Evidence / provenance tests

Tests MUST verify:

1. `RuleEvaluation` remains authoritative: findings cannot change
   evaluation state and evidence cannot upgrade or downgrade state.
2. Provenance-chain preservation: every finding/evidence item traces to
   source observations through normalization with source, object,
   operation/context, and assessment references intact.
3. Unknown provenance remains explicit and is never invented; evidence
   cannot be created without corresponding source data.
4. No cross-tenant evidence correlation.
5. PASS carries evidence sufficient to establish required completeness;
   FAIL carries affirmative deterministic evidence.
6. NOT_EVALUATED, NOT_APPLICABLE, and ERROR each carry the structured
   reason/context required by `rule-engine-contracts.md` §§3.10, 8 and
   `findings-evidence-schema.md`.
7. Diagnostics remain distinct from tenant security findings; redaction
   does not alter evaluation state or fabricate evidence.
8. **Secret exclusion:** synthetic secret-like strings (token/key/
   password patterns containing no real credential material) injected
   into fixtures are rejected or redacted per future implementation
   contracts and never appear in normalized state, graph, findings,
   evidence, reports, or logs. No real secrets are used.

---

## 11. Determinism / reproducibility tests

Same controlled inputs, deterministic configuration (including rule
versions), capability state, provenance, and assessment-time reference
MUST produce semantically equivalent results across runs.

1. **Semantic equivalence** means: identical per-rule assessment states,
   identical subject attribution, equivalent evidence/provenance linkage,
   and equivalent reason/cause content for non-verdict states. It does
   NOT require byte-identical presentation output (timestamps and other
   evaluation-neutral presentation metadata MAY differ) unless another
   contract explicitly requires byte stability for a specific artifact.
2. Tests MUST prove: no hidden wall-clock dependency in rule evaluation;
   no randomness affecting verdicts; no AI/model influence; no provider
   calls from rule logic; per-rule failure isolation is deterministic;
   shared-state integrity behavior is deterministic; malformed inputs,
   capability combinations, and missing provenance/evidence cases behave
   deterministically; and ordering is stable where semantically required.
3. Property-style determinism checks SHOULD use seeded generation where
   input sets are generated, so failures are reproducible.

---

## 12. Output-authority tests

Renderers may project results but MUST NOT change verdict semantics or
invent assessment authority (INV-12):

1. Across every renderer (CLI, JSON, SARIF, HTML, and any future
   projection): all five assessment states remain distinguishable;
   ERROR/NOT_EVALUATED cannot be hidden, suppressed, or restyled into a
   success impression; severity MUST NOT be reinterpreted into state.
2. No renderer may rerun rules, call provider APIs, request privilege,
   receive authentication credentials, fabricate evidence, or emit secret
   material.
3. Tenant-controlled strings MUST be treated as untrusted data in every
   renderer: safe serialization/encoding against JSON/SARIF injection,
   HTML/script injection, ANSI/control-character and newline
   manipulation, bidirectional-control misleading rendering, malformed
   Unicode breakage, dangerous URI schemes, external-resource leakage,
   and filesystem path-traversal/overwrite/partial-artifact hazards
   (detailed renderer threat coverage owned by
   `output-renderer-contracts.md`; seam obligations fixed here).
4. No tenant/provider string is treated as executable control content.

---

## 13. Failure and cancellation tests

Tests MUST preserve the distinction among assessment states,
operational failures, cancellation, and resource exhaustion — and MUST
NOT introduce a new assessment state:

1. **Assessment states** (layer A per `error-result-model.md` §3): PASS,
   FAIL, NOT_EVALUATED, NOT_APPLICABLE, ERROR — exactly five, no sixth.
   Cancellation, timeout, throttling-exhaustion, and partial-collection
   conditions are operational causes mapped onto these states per
   `capability-model.md` §9 and `error-result-model.md` §4; they are
   never states themselves.
2. **Operational failures** (layer C): each failure category is injected
   per boundary and MUST surface with sanitized diagnostics and causal
   chain, flowing into NOT_EVALUATED (scoped gaps) or ERROR (trust-
   breaking failures) per mapping — never silent PASS and never tenant
   FAIL.
3. **Cancellation** (F-13 concept): injected per S-09; affected scope
   records NOT_EVALUATED or ERROR with cancellation reason; orchestration
   propagates without converting into PASS/FAIL; aggregation preserves
   the incomplete scope visibly.
4. **Resource exhaustion:** injected per S-10; affected scope fails
   visibly with bound and scope identified; partial processing never
   silently becomes PASS.
5. **Integrity violations** (layer D): tenant mismatch, cross-tenant
   edge/evidence attempts, corrupted shared state, invalid rule
   definitions — MUST fail closed and visibly, potentially halting the
   affected scope rather than producing verdicts.

---

## 14. Architecture dependency tests

Tests and checks MUST be able to verify the dependency constraints
established in `solution-structure.md` §§8–10 (per-seam interface
placement owned by `dependency-boundaries.md`; enforcement intent
executed by the structural test project). No specific testing package,
framework, or enforcement library is selected here (§17, T-01).

1. **Core remains provider/I-O/presentation independent:** Core has no
   project reference outside itself and no behavioral dependency on
   infrastructure, output, host, AI/model, authentication implementation,
   or concrete logging/telemetry backends.
2. **Provider SDK types do not leak inward:** no provider SDK, raw
   payload, HTTP-client, renderer-library, CLI-framework, web-framework,
   AI/model-SDK, or concrete-logging-backend type appears in Core
   contracts, rule/graph/findings namespaces, or assessment-result
   surfaces.
3. **Composition remains at the approved composition root:** only the
   outer host wires concrete adapters to ports; Application receives
   abstractions and never instantiates adapters; Core contains no
   composition logic.
4. **Dependency graph remains acyclic:** the project reference graph is a
   DAG rooted at Core with the host at the outer edge referenced by
   nothing; namespace rules inside Core (Rules, Graph, Findings, Results,
   Domain, Capabilities) and the adapter/renderer namespace separations
   hold.
5. Checks MUST be structural (reference/namespace/type-presence rules),
   not behavioral, and MUST NOT require live services, secrets, or
   production data.

---

## 15. Security-oriented tests

In addition to §§8–9, the adversarial suite MUST include (threat-model
mapping per testing-architecture §15; each test uses synthetic inputs
only):

1. Secret-bearing data rejection/exclusion (§10 item 8).
2. Malicious/malformed normalized data: type-confusion payloads,
   conflicting observations, unexpected extensions, invalid Unicode,
   control characters, oversized strings — graceful structured failure,
   no crash, no invariant violation, no fabricated facts.
3. Oversized input / resource bounds (S-10; §13 item 4).
4. Cross-tenant data attempts (§9).
5. Evidence tampering / inconsistent provenance attempts: fabricated
   provenance, modified evidence references, missing-provenance claims —
   rejected or represented explicitly, never silently accepted.
6. Provider/raw-payload leakage attempts: provider-specific types pushed
   at the normalization boundary — rejected; raw payloads never reach
   rule contracts, findings, evidence, or output.
7. False-PASS and false-FAIL scenarios (§8).
8. Renderer/terminal/filesystem injection payloads (§12).
9. Configuration and rule-tampering attempts: modified rule
   definitions, disabled-rule masking, unknown-option injection —
   detected with safe visible failure, never silent weakening.
10. Dependency/supply-chain anticipation: fixture content is never
    executed as code; future SBOM/signing verification hooks (when
    designed) are testable without production secrets.

---

## 16. Synthetic fixtures

1. Synthetic deterministic fixtures are the **normal automated-test
   baseline** for every layer: unit, component, contract, integration,
   end-to-end, regression, adversarial, property-style, fuzz-style,
   golden/snapshot, and resource-bound coverage.
2. Fixture taxonomy (concepts; exact schemas owned elsewhere):
   synthetic identities across documented kinds; synthetic application /
   service-principal / managed-identity / agent-identity representations
   per documented concepts only; fake accountability, permission/access,
   and graph-topology relationships; non-secret credential metadata
   (synthetic key IDs, thumbprints, timestamps — never real material);
   fake capability states; fake collection/provider failures; adversarial
   strings; pathological graph structures.
3. **Forbidden fixture content:** live access/refresh tokens, client
   secrets, passwords, private keys, authentication cookies/session
   tokens, production certificates/signing material, recovery codes,
   real tenant identifiers or data, employer/customer confidential
   information, production tenant exports, or real directory objects.
4. Each fixture SHOULD carry metadata identifying its synthetic nature,
   purpose, scope, and classification. Any future captured provider
   fixture MUST undergo explicit security review and sanitization before
   repository inclusion.
5. Real tenant data MUST NOT be required for repository tests. Test
   artifacts (logs, outputs, temporary fixtures) MUST NOT contain
   credential material or production data.

---

## 17. CI compatibility

Tests MUST be suitable for future non-interactive CI execution without
requiring secrets for Core-level testing. The CI workflow itself is NOT
designed here.

1. Ordinary automated tests (all Core/Application/Output/host unit,
   component, contract, adversarial, and golden coverage) run
   deterministically on synthetic fixtures with no live tenant, no
   network dependency, and no real secrets.
2. Test execution MUST terminate within bounded time and resource limits
   and MUST NOT depend on external services that could cause indefinite
   blocking.
3. Test execution MUST NOT transmit fixture or synthetic tenant data to
   external services beyond what the specific test purpose requires.
4. Untrusted-change contexts MUST NOT gain access to privileged secrets
   or live tenant contexts through test configuration; test doubles MUST
   NOT introduce insecure fallback paths usable outside tests.
5. Security/adversarial suites MUST be runnable as gates (fail visibly
   and block on violation); failures MUST be diagnosable without
   production access.
6. Exact CI workflows, permissions, scanners, SBOM/signing tooling,
   action pins, and release pipelines are TBD (§18, T-05) and MUST NOT
   be invented here.

---

## 18. Golden / snapshot and traceability policy

1. **Golden/snapshot usage:** golden/snapshot comparisons are permitted
   where useful for deterministic canonical-output stability (JSON
   semantic structure, CLI rendering, SARIF structure once mapping is
   defined, HTML safe rendering). Snapshots assert semantic stability,
   not incidental byte stability, except where another contract
   explicitly requires byte stability for a specific artifact.
2. **Golden discipline:** golden tests MUST NOT become an excuse to
   approve incorrect output because a snapshot changed.
   Security-relevant snapshot changes (any evaluation-state change, any
   evidence/reason alteration, any error-visibility reduction) require
   explicit review and approval before the snapshot is updated.
3. **Traceability hook:** each test SHOULD be traceable to one or more
   requirements, invariants, or threat-model entries through the chain
   requirement → invariant → threat/trust boundary → test property →
   test case → result/evidence. Security-critical invariants SHOULD have
   explicit verification evidence. No concrete traceability tool is
   selected here.

---

## 19. Explicit non-goals

- No test, fixture, mock, fake, harness, or test project is created here.
- No test framework, assertion library, mocking library, fuzzing
  framework, snapshot library, or enforcement-tooling package is
  selected.
- No Microsoft Graph endpoint, permission/scope name, SDK API, licensing
  behavior, Entra property, Agent Identity mapping, numeric retry/timeout/
  concurrency/resource bound, schema field name, CLI syntax/exit code,
  SARIF mapping, HTML templating strategy, OAuth flow, or storage
  mechanism is invented.
- No CI workflow, GitHub Actions configuration, scanner selection, SBOM
  tooling, signing implementation, or release pipeline is designed.
- No SaaS/multi-tenant execution, remediation, monitoring, dashboard,
  plugin marketplace, AI verdict, or other NG-series non-goal is
  introduced.

---

## 20. Implementation TBDs

Explicitly unresolved test-implementation choices. None is resolved here
by speculation.

| ID | Unknown | Why unresolved at this stage | Owner |
| --- | --- | --- | --- |
| T-01 | Concrete test framework, assertion/mocking/snapshot/property/fuzz libraries, structural-test enforcement tooling, and test-project mechanics. | Requires a dedicated tooling decision with supply-chain review; no framework is approved yet and none may be assumed. | Future test-implementation decision; seam catalog owned by `dependency-boundaries.md`; scenario catalog owned here. |
| T-02 | Exact fixture schemas, factory/harness shapes, seeded-generation strategy, and fixture-metadata format. | Fixture schemas follow the domain, graph, capability, findings, and error contracts, which finalize their own schemas first. | This document (fixture policy) with `domain-types.md`, `identity-graph-contracts.md`, `capability-model.md`, `findings-evidence-schema.md`, `error-result-model.md` (schemas). |
| T-03 | Per-category operational-failure-to-state mapping table detail, fatal-vs-isolated failure policy, and cancellation mechanism (token/task/threading choice). | Requires the failure-taxonomy mapping owned jointly with the error model; mechanism choices require implementation review. | `error-result-model.md` with `rule-engine-contracts.md` and `collector-contracts.md` / `identity-graph-contracts.md`. |
| T-04 | Numeric resource/timeout/concurrency/pagination bounds, retry/backoff constants, and performance tolerances for resource-bound tests. | Numeric constants require validation, not guessing; bounds must be enforceable before tests assert them. | Owning contract documents (`collector-contracts.md`, `identity-graph-contracts.md`, `rule-engine-contracts.md`, `error-result-model.md`); asserted here once defined. |
| T-05 | CI workflow, permissions, scanners, SBOM/signing tooling, action pins, coverage gates, and release-pipeline mechanics. | Requires a dedicated build/release design phase; inventing workflow detail here would pre-empt it. | Future CI/release design (not this document). |
| T-06 | Golden artifact set, snapshot storage/review mechanics, and traceability tooling. | Requires schema finalization and review-process decisions. | This document (policy §18) with `output-renderer-contracts.md` (artifact semantics). |
| T-07 | Any Microsoft Graph endpoint, permission/scope name, SDK method, property/relationship mapping, licensing-behavior claim, or Agent Identity mapping referenced by future adapter tests. | Requires validation against published Microsoft documentation; invention is prohibited (INV-15). | `collector-contracts.md` / `capability-model.md` / `authentication-design.md` after documentation validation. |

---

## 21. Acceptance criteria

This document is complete for Phase 0.3.8 when:

1. The seam philosophy (§2) and every core deterministic seam
   (S-01–S-10 in §3) are explicit with control obligations.
2. Provider isolation (§4), authentication (§5), collector (§6),
   rule-engine (§7), and false-PASS adversarial (§8) seams and scenarios
   are explicit, including cancellation and resource-exhaustion behavior
   that never yields PASS.
3. Tenant-isolation (§9), evidence/provenance (§10), determinism (§11),
   output-authority (§12), and failure/cancellation (§13) requirements
   are explicit with no sixth assessment state introduced.
4. Architecture dependency checks (§14), security-oriented coverage
   (§15), synthetic-fixture policy (§16), CI compatibility without
   workflow design (§17), and golden/traceability policy (§18) are
   explicit.
5. Every implementation-sensitive unknown is listed in §20 as a TBD with
   a named owner; no test framework, fixture schema, numeric bound, CI
   workflow, or provider detail is invented.
6. `git diff --check` is clean and `git status --short` shows only the
   two authorized files modified.

---

## 22. Invariant and requirement traceability

| Concern | Traces to |
| --- | --- |
| Deterministic repeatable tests, same inputs to same results | INV-02; VERD-006; testing-architecture §§1.1, 8.2 |
| Provider-independent core testing, normalized boundary | INV-03; INV-04; testing-architecture §§1.6, 4–7 |
| Five states, no silent coercion, failure transparency | INV-05; INV-14; testing-architecture §§8–9 |
| Evidence traceability and provenance preservation | INV-06; INV-11; testing-architecture §10 |
| Capability-aware testing, no false PASS/FAIL | INV-07; CAP-001–CAP-004; testing-architecture §§1.7, 8–9 |
| Least privilege, no insecure test fallback | INV-08; INV-16; testing-architecture §§1.14, 5 |
| Secret exclusion in fixtures and artifacts | INV-09; SEC-001; SEC-002; SEC-009; testing-architecture §§1.2, 1.4, 1.12 |
| Tenant isolation as release-critical property | INV-10; testing-architecture §§1.5, 13 |
| Renderer non-authority, AI non-authority | INV-12; INV-13; testing-architecture §§8.2, 11 |
| No undocumented provider dependency in tests | INV-15; testing-architecture §§4–5 |
| Synthetic data, no production tenant, bounded CI-suitable execution | SEC-009; NFR-002; CON-005; testing-architecture §§1.2–1.3, 1.11, 1.13, 18 |

---

## Self-verification (Phase 0.3.8 authoring note)

- Only `docs/03-implementation-design/rule-engine-contracts.md` and
  `docs/03-implementation-design/testing-seams.md` were modified in this
  phase task.
- No requirements or architecture document was modified.
- No application/source/test code and no `.sln`/`.csproj` files were
  created.
- Exactly five canonical assessment states are used: PASS, FAIL,
  NOT_EVALUATED, NOT_APPLICABLE, ERROR. No sixth state or alias was
  introduced.
- No Microsoft Graph endpoint, permission, SDK API, licensing behavior,
  Entra property, Agent Identity mapping, or undocumented provider
  behavior was invented.
- No secrets, credential material, or production tenant data were
  introduced into contracts or fixture policy.
- No `git add/commit/push/reset/clean/checkout/switch` was performed.

(End of file)
