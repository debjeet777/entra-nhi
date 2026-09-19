# Core Contracts

> **Phase:** 0.3.4
> **Package:** Implementation Design
> **Status:** Proposed — design only, no code or project files created
> **Date:** 2026-09-18

---

## 1. Status and scope

- **Status:** Proposed conceptual contracts. This document is design output
  only. It authorizes no implementation.
- **Scope:** Defines the stable inward-facing conceptual contract boundary of
  `EntraNHI.Core` and the ports consumed by `EntraNHI.Application`, as
  structured by `solution-structure.md` §§4–8 and governed by `README.md`
  §§4–6.
- **What this document is:** The owner of the shared core contract surface:
  assessment identity/context, normalized identity input references,
  identity-graph input/query references, rule definition/evaluation
  references, finding-production references, evidence/provenance references,
  capability-observation references, assessment/result aggregation references,
  output-neutral result references, identifier/value concepts, and the clock/
  time abstraction position. Each detailed shape is owned by its respective
  later document; this document defines the boundary taxonomy, the
  cross-cutting design principles, the five-state evaluation contract, and
  the dependency prohibitions that keep the core provider-independent.
- **What this document is not:** It is not a C# type catalog, interface
  specification with members, Graph binding, permission list, package
  selection, or implementation sequence. Detailed schemas belong to:
  `domain-types.md` (normalized domain types), `capability-model.md`
  (capability states), `identity-graph-contracts.md` (graph surface),
  `rule-engine-contracts.md` (`RuleDefinition`, pipeline, `RuleEvaluation`
  evaluation surface), `findings-evidence-schema.md` (`Finding` vs
  `RuleEvaluation`, evidence/provenance/diagnostic schemas),
  `error-result-model.md` (error/result shape and failure taxonomy),
  `collector-contracts.md` (collector interfaces and normalization handoff),
  `output-renderer-contracts.md` (canonical output model and renderers),
  `authentication-design.md` (auth boundaries), `configuration-design.md`
  (configuration surfaces), `dependency-boundaries.md` (seam catalog and
  enforcement), `testing-seams.md` (test scenario catalog), and
  `implementation-sequence.md` (build order).
- **Language discipline:** Per `README.md` §1, this document distinguishes
  (a) **approved architectural requirements** traced to requirement/
  invariant IDs, (b) **proposed conceptual contracts** owned by this
  document, and (c) **unresolved TBDs** naming the owning later document.
  Nothing herein claims a control is implemented, verified, or enforced.
  Where architecture states `DESIGNED / REQUIRED`, this document preserves
  that classification.
- **Illustrative syntax discipline:** This document MAY use C#-style
  signatures or pseudotypes where that materially improves precision. Any
  such syntax is **illustrative and non-normative** — it is not final
  production code, not a namespace decision, and not a member list. Final
  shapes belong to the owning documents listed above. No framework, package,
  SDK, or library implementation is selected here (CON-003).

---

## 2. Contract design principles

The following principles constrain every contract family in §3. Each is
traced; none is a claim of implemented behavior.

1. **Deterministic behavior (INV-02; VERD-006).** Identical normalized
   inputs, capability state, rule version/configuration, and relevant
   deterministic execution context produce the same semantic evaluation
   outcome. Contracts MUST NOT admit randomness, wall-clock reads inside
   evaluation logic, probabilistic branching, or model inference on the
   evaluation path. Presentation metadata that does not affect evaluation
   (e.g., report timestamps) MAY differ; evaluation semantics MUST NOT.
2. **Immutable/value-oriented inputs where practical.** Normalized inputs
   crossing into `Core` SHOULD be immutable values (records) rather than
   mutable entities or live handles. Rationale: immutability preserves the
   snapshot semantics the findings architecture requires (evidence refers to
   what was observed for a particular assessment), prevents evaluation from
   mutating the graph or domain model during rule execution, and makes
   synthetic-fixture testing trivially substitutable. Exact C# record/class
   mechanics are a TBD owned jointly by `domain-types.md` (domain shapes)
   and `dependency-boundaries.md` (construction mechanics); this document
   only requires the orientation.
3. **Explicit outcomes (INV-05; INV-14).** Every evaluation resolves to
   exactly one canonical state (§4). Failure, incompleteness, and
   inapplicability are represented explicitly — never as silent PASS/FAIL,
   never as absent output. Contracts MUST provide a place for structured
   reason/context on every non-PASS/FAIL path.
4. **Explicit tenant context (INV-10).** Every contract that carries
   tenant-derived data carries the tenant assessment context (§5). No
   contract permits ambient or implied tenancy.
5. **Explicit capability context (INV-07; CAP-001–CAP-004).** Every contract
   that consumes collected data also consumes the capability/collection
   state for that data, at a granularity sufficient for per-rule PASS gating
   (see `capability-model.md`). Capability absence MUST NOT be
   indistinguishable from confirmed-empty observation.
6. **Evidence/provenance preservation (INV-06; INV-11).** PASS/FAIL require
   traceable evidence references; other states require structured reason/
   context. Provenance (source system, object reference, collection
   operation/context, assessment context) is preserved across every
   transformation boundary and MUST NOT be fabricated. Contracts MUST NOT
   provide a path that drops provenance silently.
7. **No ambient provider state.** Contracts MUST NOT read provider state
   through ambient singletons, static clients, implicit HTTP contexts, or
   hidden caches. All provider-derived content arrives as explicit,
   normalized, provenance-carrying parameters.
8. **No hidden network access (CON-005).** No `Core` contract performs
   network I/O, initiates provider queries, requests privilege, or depends
   on live-service reachability. Collection-window observations are data;
   the core never treats them as a live query surface.
9. **No renderer authority (INV-12).** Contracts expose the authoritative
   result in output-neutral form (§11). Renderers project; they do not
   evaluate, re-derive, suppress, or reinterpret state.
10. **No AI authority (INV-13; SEC-010; VERD-007; CON-004).** No contract
    admits model output onto the authentication, authorization, collection,
    evaluation, evidence, capability, or severity path. Any future
    explanatory AI attaches downstream of the canonical result as a
    non-authoritative consumer.
11. **Testability.** Every contract family MUST be expressible with synthetic
    fixtures, fakes, and mocks, without a live tenant, production
    credentials, or network access for core tests (testing-architecture
    §§1–2). Contracts SHOULD prefer pure functions over normalized inputs
    plus explicit context objects, so `testing-seams.md` can target each
    seam independently.
12. **Cancellation/operation context without premature mechanisms.**
    Long-running conceptual operations (graph construction over large
    normalized sets, rule-set evaluation, aggregation) SHOULD accept an
    explicit operation context carrying cancellation intent, bounding/time-
    reference policy, and diagnostic capture — without choosing the
    implementation mechanism here. No reactive framework, task library,
    threading model, or timeout constant is selected. Exact
    cancellation/pagination/retry mechanics belong to
    `collector-contracts.md` (collection side) and `rule-engine-contracts.md`
    / `identity-graph-contracts.md` (evaluation/graph side); fatal-vs-
    isolated failure policy belongs to `error-result-model.md`. This
    document only requires that *if* an operation can be long-running or
    bounded, its contract MUST make the operation context explicit rather
    than ambient, and MUST define how early termination surfaces (explicit
    ERROR/NOT_EVALUATED/diagnostic, never silent PASS).

---

## 3. Core boundary taxonomy

The core boundary is organized into conceptual contract families. A family
is a responsibility area, not necessarily one interface. The kind of
contract chosen per family follows the guidance at the end of this section.

### 3.1 Assessment/evaluation orchestration boundary

- **Responsibility:** Coordinate the deterministic pipeline stages
  (discovery → compatibility → applicability → capability validation →
  input validation → evaluation → state assignment → evidence handoff →
  diagnostics → aggregation, per rule-engine architecture §10) without
  holding rule authority itself.
- **Kind:** Service contract(s) consumed by `EntraNHI.Application`
  orchestration and implemented inward by `Core` logic. Detailed pipeline
  stage contracts belong to `rule-engine-contracts.md`; orchestration port
  placement belongs to `dependency-boundaries.md`.
- **Illustrative (non-normative) sketch:**
  `AssessmentResult Evaluate(AssessmentScope scope, OperationContext op)`
  — shown only to fix the direction (scope in, result out, context
  explicit). No member list is normative here.

### 3.2 Normalized identity input

- **Responsibility:** The canonical unit of assessment: the normalized
  identity record and its associated metadata references.
- **Kind:** Immutable record / value concepts, owned in detail by
  `domain-types.md`. This document references them (e.g., "the
  `IdentityRecord` defined by `domain-types.md`") and MUST NOT restate
  field lists.

### 3.3 Identity graph input/query boundary

- **Responsibility:** Deterministic relational projection of normalized
  records: node/edge lookup, edge traversal by relationship kind,
  subgraph extraction, aggregation — all deterministic and idempotent.
- **Kind:** Module boundary plus query-operation contracts, owned in detail
  by `identity-graph-contracts.md` (node/edge shapes, observed-vs-derived
  classification, INV-G1–INV-G8). Rules consume graph projections; they
  MUST NOT mutate the graph during evaluation.

### 3.4 Rule definition

- **Responsibility:** Stable identity/version metadata, applicability
  semantics, required capability/data declarations, deterministic
  evaluation predicate, PASS/FAIL semantics, unavailable-input behavior,
  evidence requirements, severity metadata (non-authoritative for logic),
  references, and deprecation/supersession metadata — per rule-engine
  architecture §7.
- **Kind:** Immutable record concept (`RuleDefinition`), owned in detail by
  `rule-engine-contracts.md`. This document establishes the boundary (§6)
  and defers the schema.

### 3.5 Rule evaluation

- **Responsibility:** The authoritative per-rule, per-target, per-context
  outcome record with exactly one canonical state plus evidence/provenance/
  capability/configuration references.
- **Kind:** Immutable record concept (`RuleEvaluation`), owned in detail by
  `rule-engine-contracts.md`. This document defines the five-state
  semantics (§4) and the output-neutrality requirement (§11); it MUST NOT
  define the record shape.

### 3.6 Finding production

- **Responsibility:** Transform authoritative `RuleEvaluation` records into
  reportable `Finding` representations per an explicit emission policy,
  without changing evaluation state.
- **Kind:** Module boundary plus transformation contract, owned in detail
  by `findings-evidence-schema.md` (`Finding` vs `RuleEvaluation`
  distinction, emission policy, fingerprint strategy, redaction hook).

### 3.7 Evidence/provenance

- **Responsibility:** What counts as evidence, how evidence references chain
  back to normalized facts and source observations, and what provenance
  each reference must carry.
- **Kind:** Value concepts plus reference contracts, owned in detail by
  `findings-evidence-schema.md`. This document establishes the boundary
  (§7): what `Core` must require, not the schema.

### 3.8 Capability observation/query

- **Responsibility:** How rules and the engine learn what was collectible:
  per-category capability/collection state, queryable at rule-evaluation
  time, propagated from collectors through normalization.
- **Kind:** Value concepts plus query surface, owned in detail by
  `capability-model.md`. This document establishes the boundary (§8):
  consumption without discovery mechanics.

### 3.9 Assessment/result aggregation

- **Responsibility:** Collect per-rule evaluations into an assessment-level
  result preserving per-rule detail, evidence references, diagnostics,
  capability context, configuration/version references, and explicit
  incompleteness (per-rule isolation; shared-state integrity failures fail
  closed and visibly).
- **Kind:** Immutable record concept plus aggregation-operation contract,
  owned in detail by `rule-engine-contracts.md` (aggregation stage) with
  failure mapping owned by `error-result-model.md`. This document requires
  that aggregation MUST preserve — not summarize away — ERROR and
  NOT_EVALUATED conditions.

### 3.10 Output-neutral assessment result

- **Responsibility:** The canonical, format-agnostic assessment result
  handed to output ports: evaluations, findings (where emitted),
  evidence/provenance references, capability states, diagnostics,
  assessment/tenant/configuration identity.
- **Kind:** Immutable record concept (canonical output model input),
  owned in detail by `output-renderer-contracts.md` as the projection
  source. This document requires output-neutrality (§11).

### 3.11 Clock/time abstraction — position

- **Justification:** Deterministic testing requires that time-dependent
  policy (e.g., credential-expiry evaluation against "now") MUST NOT read
  wall-clock inside rule logic (rule-engine architecture §9.3). *If* any
  current or future rule requires a time reference, that reference MUST
  arrive as explicit deterministic input, recorded in assessment context
  and evidence.
- **Position:** A clock/time abstraction is therefore justified **only as
  an explicit assessment-time reference** supplied with the assessment
  scope (§5), not as a general ambient clock service and not as a rule-
  callable "now". Representation, resolution, and UTC/offset handling
  principles belong to `domain-types.md` §12; assessment-time mechanics
  (ordering, parallelism interaction, staleness) belong to
  `rule-engine-contracts.md`. If no V1 rule requires time-dependent policy,
  the reference SHOULD still exist as an explicitly-marked-unused context
  field so a later rule can adopt it without redesigning the scope
  contract — this is a structural reservation, not a behavior claim.
- **Prohibition:** No contract exposes a mutable, ambient, or
  provider-coupled clock to rule logic. No timestamp constant or
  formatting choice is made here.

### 3.12 Identifiers/value concepts required across contracts

- **Responsibility:** Stable machine-readable references binding the whole
  chain: assessment, tenant assessment context, normalized identity key,
  source reference, rule ID/version, evaluation, finding, evidence,
  provenance, diagnostic, configuration/rule-set reference, and
  assessment-time reference where relevant.
- **Kind:** Value concepts (opaque identifiers), with formats deferred.
  Identifier formats, fingerprint/dedup strategy, and hashing/signing
  posture belong to `findings-evidence-schema.md` (assessment/evaluation/
  finding/evidence/provenance identifiers) with identity-key specifics
  owned by `domain-types.md`. This document requires: identifiers MUST NOT
  depend solely on display names; MUST be tenant-scoped where identity-
  bearing; MUST be stable within an assessment; and MUST NOT claim global
  uniqueness until an identifier strategy is designed.

### When to use which contract kind

- **Value type / immutable record:** Use for data crossing a boundary that
  must be snapshotted, compared, evidenced, or fixture-constructed
  (identity records, capability states, evaluation records, findings,
  evidence references, provenance records, assessment scope/result).
  Prefer records where auditability or equality semantics matter.
- **Service contract (interface):** Use for behavior with substitutable
  implementations or side-effect-adjacent responsibilities (evaluation
  orchestration, graph query/projection operations, finding-production
  transformation, output ports, collection ports). Interfaces live inward
  (defined by `Core`/`Application` needs) and are implemented outward —
  per `solution-structure.md` §8 and the seam catalog in
  `dependency-boundaries.md`.
- **Module boundary (namespace-level organization):** Use for grouping
  related pure logic that needs authority separation without an assembly
  split (e.g., `Rules` vs `Findings` vs `Graph` vs `Domain` vs `Results`
  inside `Core`, per `solution-structure.md` §7). Module boundaries carry
  namespace reference rules enforced by future structural tests; they MUST
  NOT become per-rule or per-renderer assemblies (counter-criterion,
  `solution-structure.md` §3).
- **Do not force every concept into an interface.** Identifier concepts,
  state enumerations (in the conceptual sense), provenance records, and
  reason/context payloads are values, not services. Creating an interface
  for each would obscure the seam catalog and invite mock-everything test
  brittleness. `dependency-boundaries.md` owns the final seam-by-seam
  interface catalog; this document only classifies each family as above.

---

## 4. Five-state evaluation contract

The canonical assessment states are EXACTLY the following five. No sixth
authoritative state exists in V1. Terminology, casing, and semantics below
are carried forward from requirements (VERD-001–VERD-005) and architecture
(INV-02, INV-05; rule-engine architecture §§3–4):

- **PASS** — The rule was applicable, required observations and
  capabilities were sufficiently available, evaluation completed
  successfully, and the defined failure condition was not satisfied.
  PASS is a security-relevant assertion. It requires affirmative
  satisfaction of the rule's defined conditions **plus** the rule-defined
  input completeness for that rule. Each rule defines its own PASS
  semantics, including what constitutes sufficient completeness.
  PASS requires traceable evidence establishing that required observations
  were complete enough and the failure condition was not satisfied
  (INV-06; findings-evidence architecture §7).
- **FAIL** — The rule was applicable, required observations and
  capabilities were sufficiently available, evaluation completed
  successfully, and the rule's explicitly defined failure condition was
  satisfied. FAIL requires an applicable rule, sufficient capability/input
  state, a deterministic failure predicate satisfied, and evidence plus
  provenance references sufficient to explain the failure. A collection,
  authentication, or system failure is not itself a tenant security FAIL
  unless a separate rule explicitly evaluates that condition as assessment
  data under a documented requirement.
- **NOT_EVALUATED** — The rule could not legitimately determine PASS or
  FAIL because required assessment capability or data was unavailable,
  unsupported, insufficient, or otherwise not collected as required by that
  rule. Carries structured reason/context identifying which required
  capability or data was missing and the capability state that caused the
  gap. This is the correct outcome for authorization denials, licensing/
  service gaps, unsupported capabilities, collection failures, and
  incomplete inputs affecting required data — per the rule's declared
  requirements.
- **NOT_APPLICABLE** — The rule does not apply to the evaluated identity
  or context according to the rule's deterministic applicability criteria.
  Carries structured reason/context identifying the identity kind, context,
  and the applicability criterion that determined non-applicability.
- **ERROR** — The rule should have been evaluable or evaluation was
  attempted, but an unexpected engine, data, or runtime failure prevented
  trustworthy completion (e.g., invalid normalized input violating a
  required invariant, rule execution exception, inconsistent graph state,
  invalid rule definition/configuration, internal deterministic engine
  failure). Carries structured reason/context sufficient to explain the
  failure without exposing credential material.

Critical rules (INV-05; INV-14; rule-engine architecture §4; CAP-002):

1. Missing capability, licensing, permission, or unsupported data MUST
   NEVER silently become PASS. Authorization denial, unsupported
   capability, collection failure, normalization failure, graph-
   construction failure affecting required input, rule execution exception,
   unknown state, and resource exhaustion MUST NOT produce PASS.
2. Missing data MUST NOT automatically become FAIL either. FAIL requires
   affirmative deterministic evidence that the documented failure condition
   is satisfied. Missing data is absence of evidence, not evidence of
   failure. Whether absence under verified complete collection constitutes
   PASS or NOT_EVALUATED is determined by the rule's explicit completeness
   semantics — never by a universal absence rule.
3. ERROR MUST NOT silently become PASS or FAIL. ERROR represents failure of
   trustworthy execution, not a tenant security verdict. It MUST NOT be
   converted to FAIL to simplify reporting, nor to PASS to simplify output.
4. NOT_EVALUATED and NOT_APPLICABLE MUST remain distinguishable end to end
   (evaluation → finding handling → output projection → diagnostics).
   NOT_APPLICABLE MUST NOT be used to hide unsupported functionality;
   NOT_EVALUATED MUST NOT be used for a rule that deterministically does
   not apply.
5. Renderer/CLI cannot reinterpret state. Output projections MUST preserve
   all five states exactly; severity MUST NOT be reinterpreted into state;
   ERROR/NOT_EVALUATED MUST NOT be suppressed into a false-secure
   impression (INV-12).
6. AI cannot create or change authoritative state. Model output MUST NEVER
   determine, modify, override, or supply any of the five states, nor
   fabricate the evidence/capability/severity behind them (INV-13).
7. Do not invent a sixth authoritative state. Data/observation conditions
   (unavailable, unsupported, not collected, partial, conflicting) are
   capability/domain semantics consumed *before* state assignment — they
   are not verdicts. Severity, confidence, risk scores, or finding-
   emission decisions are downstream metadata/policy, not states.
   `error-result-model.md` owns the failure-category taxonomy that maps
   operational failures onto NOT_EVALUATED vs ERROR; it MUST NOT create a
   new terminal state in doing so.

---

## 5. Assessment identity/context

The conceptual information needed to keep an assessment scoped to the
correct tenant and execution (INV-10; collection architecture §§6, 9;
rule-engine architecture §20; findings-evidence architecture §12):

- **Tenant assessment context reference.** Binds every normalized record,
  graph node/edge, evaluation, evidence reference, finding, diagnostic,
  and output artifact to one explicit single-tenant execution scope.
  Tenant-context mismatch or contamination is an integrity failure and
  MUST fail safely and visibly (never silent mixing, never cross-tenant
  edges, never cross-tenant evidence correlation in V1).
- **Assessment identifier.** Stable machine-readable reference for the run
  (collection window, not a transactional-snapshot claim unless the source
  provides it). Supports auditability, evidence snapshot semantics
  (later changes MUST NOT silently alter an already-produced evaluation),
  and requirement-to-result traceability.
- **Execution-mode context (non-credential).** Whether the run was
  initiated interactively or as workload/automation, and which validated
  authorization boundary established the context — without carrying token
  mechanics, secret values, or credential material. Token mechanics are
  explicitly out of scope here; the `AuthorizedAccessContext` abstraction
  shape belongs to `authentication-design.md`.
- **Deterministic configuration/rule-set reference.** Active rule set and
  versions, deterministic configuration values, and any assessment-time
  reference (§3.11) — sufficient to reproduce the semantic outcome from
  preserved normalized inputs (VERD-006; INV-02).
- **Collection-window context.** Observation timestamps and collection-time
  context sufficient to express that the normalized assessment represents
  a collection window over mutable external state (collection architecture
  §9), including diagnosable inconsistency markers where conflicting
  observations were preserved rather than silently overwritten.

Non-requirements for this section:

- Do NOT specify authentication token mechanics, credential mechanisms,
  token-cache/storage policy, OAuth flows, consent UX, or permission names.
  All belong to `authentication-design.md` (with permission rationale
  validated against published documentation).
- Do NOT assume SaaS or multi-tenant hosting. V1 executes one tenant per
  run under an operator-controlled trusted local runtime (system-context
  §9; NG-003). **Tenant isolation** (data from separate tenant contexts
  MUST never be silently mixed within or across runs) is distinct from
  **SaaS multi-tenancy** (simultaneous multi-tenant service operation,
  which is a V1 non-goal). Contracts MUST enforce the former and MUST NOT
  assume, require, or design the latter.

---

## 6. Rule contract

What a deterministic rule conceptually receives and returns (INV-02–INV-05;
rule-engine architecture §§2, 7–11):

- **Receives (only):** the target normalized identity record(s) (by
  reference to `domain-types.md` shapes); relevant normalized
  relationships / graph projection (subgraphs, traversals, aggregations
  per `identity-graph-contracts.md`); capability states for required data
  (per `capability-model.md`); provenance references; the assessment
  context (§5); deterministic configuration values; and its own
  `RuleDefinition`/version metadata. Explicitly defined derived values
  produced by deterministic logic from the above MAY additionally be
  consumed, provided their derivation is traceable.
- **Must not receive or reach:** provider SDK objects, raw provider
  payloads, live provider responses, tokens/secrets/credential material,
  undocumented external state, or model-generated facts. Rules MUST NOT
  call providers, request additional privilege, or perform I/O.
- **Returns:** exactly one canonical state (§4) with evidence references
  (for PASS/FAIL) or structured reason/context (for NOT_EVALUATED /
  NOT_APPLICABLE / ERROR), plus diagnostic capture sufficient for audit.
- **Carries as metadata:** stable rule ID (stable across versions),
  rule version (behavior changes produce a new version), title,
  description, security rationale, target identity/context kinds,
  deterministic applicability predicate, required capabilities, required
  normalized inputs, deterministic evaluation predicate, PASS/FAIL
  semantics including completeness requirements, unavailable-input
  behavior, evidence requirements, severity metadata (flows into findings/
  output, never into evaluation logic), references/documentation metadata,
  and deprecation/supersession metadata — per rule-engine architecture §7.
- **Behavioral obligations:** applicability evaluated before capability/
  input validation before predicate execution (pipeline order owned by
  `rule-engine-contracts.md`); per-rule failure isolation (one rule's
  exception MUST NOT corrupt unrelated evaluations where safe continuation
  is possible); shared-state integrity failures fail closed and visibly;
  configuration weakening is explicit, validated, and visible (INV-16);
  severity MUST NOT alter evaluation logic; extension MUST NOT admit
  arbitrary untrusted code execution by default (no dynamic plugin
  marketplace in V1).

**Boundary and deferral.** The complete rule schema — concrete
`RuleDefinition`/`RuleEvaluation` shapes, registration/compatibility
mechanics, version-format choice, pipeline stage contracts, isolation vs
fatal policy, ordering/parallelism, assessment-time representation,
severity taxonomy, configuration format, finding-emission interaction,
graph-query abstraction, and resource limits — belongs to
`rule-engine-contracts.md`. This document establishes only the boundary
above: what crosses into and out of a rule, and under which invariants.
`rule-engine-contracts.md` MUST NOT relax any §4 critical rule and MUST
NOT admit a provider, AI, renderer, or ambient-clock dependency.

---

## 7. Evidence/provenance contract boundary

What `Core` must require from evidence/provenance, without duplicating the
detailed schema (INV-06; INV-11; findings-evidence architecture §§3–8):

- **Required of every PASS/FAIL:** traceable evidence references
  sufficient to explain the outcome — referencing normalized, non-secret
  assessment facts (identity attributes, relationship observations, graph
  relationships, capability states, non-secret credential metadata,
  accountability/permission observations, deterministic derived values,
  relevant assessment context) with enough provenance to trace back toward
  the collected source observation without requiring reports to expose
  raw provider payloads. Presentation text is not authoritative evidence.
- **Required of every NOT_EVALUATED / NOT_APPLICABLE / ERROR:** structured
  reason/context (which capability/data/condition, which rule/criterion,
  which failure) sufficient for audit and for distinguishing capability
  gaps from security failures — never fabricated PASS/FAIL evidence.
- **Required of every provenance reference:** source system (API family),
  source object reference, collection operation/context, assessment
  context, observation-time context where appropriate, and
  transformation/derivation lineage for derived facts. Unknown or
  unavailable provenance is represented explicitly, never invented.
- **Required end to end:** the conceptual chain provider observation →
  source observation → normalized domain fact → graph/derived fact where
  applicable → evaluation evidence reference → finding where emitted →
  output representation MUST preserve traceability at each transformation;
  later tenant changes MUST NOT silently alter an already-produced
  evaluation (snapshot semantics); data minimization applies (references
  over raw duplication; no unnecessary raw payloads).
- **Forbidden content:** secrets or raw authentication material of any
  kind — including token values, secret values, private-key material,
  passwords, recovery codes, authorization headers, and secret-bearing
  configuration. Credential metadata is evidence only when explicitly
  non-secret. Diagnostics MUST NOT expose secrets either.

**Deferral.** Evidence/provenance/diagnostic record shapes, provenance
schema, emission policy, fingerprint strategy, identifier formats,
canonical serialization, persistence/retention posture, redaction policy,
evidence-integrity failure policy, hashing/signing/timestamping posture,
and artifact-permission posture belong to `findings-evidence-schema.md`.
This document requires only the boundary above.

---

## 8. Capability boundary

How rules and `Core` consume capability knowledge without knowing
provider-specific discovery mechanics (INV-07; CAP-001–CAP-004;
collection architecture §5; domain-model capability semantics):

- **Consumption shape:** Rules declare required capabilities; the engine
  validates declared requirements against current capability state before
  executing security predicates. Capability state MUST be sufficiently
  granular that one category's unavailability does not invalidate
  unrelated successfully collected categories (e.g., credential-metadata
  gaps do not erase identity enumeration; agent-identity gaps do not block
  application-registration assessment; per-identity failures do not affect
  other identities).
- **No universal capability-to-state mapping:** The rule definition
  controls deterministic state semantics within §4 constraints. Different
  rules MAY handle the same capability gap differently (NOT_EVALUATED vs
  NOT_APPLICABLE) based on documented applicability and evaluation logic —
  but no rule MAY resolve a required-capability gap to PASS or to FAIL
  without affirmative failure evidence.
- **Isolation from discovery:** `Core` contracts MUST NOT encode how
  capability was detected (no endpoint probing logic, no licensing-tier
  inference, no permission-name checks, no SDK calls). Detection lives in
  collectors/capability detection; `Core` sees only the resulting explicit
  state plus provenance. No licensing-behavior claims about tenant
  environments are made here (CAP-005).
- **Deferral.** Capability-state names, serialization/schema, hierarchy,
  per-category granularity, propagation shape, and per-rule requirement
  declaration mechanics belong to `capability-model.md`. This document
  MUST NOT redefine state names and MUST NOT invent licensing behavior.

---

## 9. Error/result boundary

Separation of four conceptually distinct outcome classes. Detailed
taxonomy, shapes, and mappings belong to `error-result-model.md`; this
section fixes only the separation (INV-14; rule-engine architecture
§§4, 13; collection architecture §10; findings-evidence architecture
§§15, 24):

1. **Expected domain/evaluation outcomes.** PASS, FAIL, NOT_APPLICABLE,
   and capability-driven NOT_EVALUATED produced through the normal
   pipeline from normalized inputs. These are assessment answers, not
   system failures. They aggregate into the assessment result with
   evidence/reason preserved.
2. **Capability/data incompleteness.** Authorization-denied, licensing/
   service-absent, unsupported, not-collected, partial, or conflicting
   observations that prevent meaningful evaluation of an otherwise
   applicable rule. These flow into NOT_EVALUATED (or NOT_APPLICABLE where
   the rule deterministically does not apply) with structured reason —
   never silent PASS, never automatic FAIL.
3. **Operational failures.** Authentication-establishment failures,
   source/service unavailability, exhausted transient failures,
   malformed/unexpected source responses, normalization-input validation
   failures, graph-construction failures, evidence-reference failures,
   output serialization failures, cancellation, and resource exhaustion.
   These flow into ERROR (or NOT_EVALUATED where the rule semantics and
   `error-result-model.md` mapping so determine for a scoped gap) with
   sanitized structured diagnostics — never silent success, never tenant
   FAIL, never secret-bearing detail.
4. **Programmer/invariant violations.** Corrupted shared normalized state,
   tenant-context mismatch/contamination, cross-tenant edge/evidence
   attempts, contract-invariant breaches, invalid rule
   definitions/configurations, fabricated-evidence attempts. These are
   integrity failures: fail closed and visibly, potentially halting
   evaluation rather than producing potentially incorrect results, with
   explicit diagnostics. Tenant-context mismatch is always in this class.

`error-result-model.md` owns: the shared `Result`/`Error` shape,
failure-category taxonomy, the mapping of each failure to NOT_EVALUATED
vs ERROR, diagnostic-vs-finding separation, and sanitization rules. It
MUST preserve §4 (no sixth state; ERROR ≠ FAIL; incompleteness ≠ PASS).

---

## 10. Collection port boundary

How `Application`/`Core` receive normalized collected data without
provider SDK types (INV-03; INV-04; collection architecture §§2, 4, 11):

- **Direction:** Collectors (outer adapters) implement ports defined
  inward; `Core` never reaches outward. Collectors produce source
  observations plus per-category capability results; normalization
  validates, translates into normalized domain contracts, preserves
  provenance, distinguishes observed from derived, preserves
  unavailable/unknown/error states, and prevents provider SDK objects or
  resource references from entering rule contracts.
- **Port-carried content (conceptual):** source family identifier (not an
  SDK type); source object/reference; tenant assessment context; collection
  operation/context; observation time context; capability state per
  observation category; normalization-ready source data (non-secret);
  provenance; structured error information where collection failed.
  Authentication tokens and secret values MUST NOT appear in port content.
- **Collector prohibitions at the port:** no PASS/FAIL evaluation, no
  findings construction, no remediation/tenant mutation, no dynamic
  privilege requests, no silent scope expansion, no hidden partial
  collection, no manufactured observations, no undocumented-semantics
  inference.
- **Deferral.** Concrete collector interfaces, the source-observation
  envelope schema/serialization, pagination/retry/cancellation surfaces,
  and the collector-to-normalization handoff mechanics belong to
  `collector-contracts.md` (with seam placement in
  `dependency-boundaries.md`). This document requires only the boundary:
  normalized data in, SDK types never.

---

## 11. Output-neutral result boundary

The authoritative assessment result MUST be independent of JSON, SARIF,
HTML, terminal formatting, web/API presentation, or AI explanation
(INV-12; INV-13; findings-evidence architecture §16; output-architecture
decisions):

- **Authoritative content:** rule evaluations for every evaluated
  rule/target (all five states representable and auditable — not only
  FAILs), findings where the emission policy requires them, evidence and
  provenance references, capability states, diagnostics, assessment/
  tenant/configuration identity, and assessment-time reference where
  relevant. Structured reason/context accompanies every NOT_EVALUATED,
  NOT_APPLICABLE, and ERROR.
- **Projection rule:** Output contracts are projections of this
  authoritative data. Renderers MUST NOT rerun rules, call providers,
  request privilege, alter evaluation state, reinterpret severity into
  state, fabricate evidence/provenance, suppress ERROR/NOT_EVALUATED into
  a false-secure impression, or execute remediation. Renderer failure is a
  system/output diagnostic — never a tenant finding and never silent
  success.
- **Injection posture at the boundary:** The canonical result treats all
  tenant/provider-originated strings as untrusted data carried opaquely;
  encoding/escaping obligations belong to renderers per
  `output-renderer-contracts.md`. The canonical model MUST NOT pre-encode
  for any single format.
- **Deferral.** The canonical output projection shape, per-renderer
  contracts (CLI, JSON, SARIF, HTML), semantic-preservation rules,
  injection-defense obligations, file/path-safety obligations, exit-code
  mapping policy, and renderer-failure semantics belong to
  `output-renderer-contracts.md`. This document requires only
  output-neutrality and non-authority.

---

## 12. Dependency prohibitions

`Core` contracts (and therefore `EntraNHI.Core` compilation, per
`solution-structure.md` §9) MUST NOT depend on — by reference, by type,
by namespace, or by behavior — any of the following. Each prohibition
traces to an invariant; the automated-enforcement rule set belongs to
`dependency-boundaries.md` and is executed by future structural tests.

1. **Provider communication types:** Microsoft Graph SDK types, provider
   HTTP clients, raw payload types, pagination/SDK helpers — anything
   that would let rule logic reach a provider (INV-03; INV-04).
2. **Azure/Entra provider SDK types** beyond the same exclusion: no second
   provider-coupled surface inside `Core`, including any optional-
   enrichment API types (collection architecture §1; INV-03).
3. **CLI framework** types or parsing behavior (host concern;
   `solution-structure.md` §§5.5, 13; INV-12).
4. **HTML/SARIF renderer implementation** types or libraries, and no other
   renderer-implementation library (INV-12).
5. **Web framework** types (no V1 web host exists; future host attaches
   outside `Core`; NG-004).
6. **AI/LLM SDK** types or model-output shapes anywhere on the
   authentication, authorization, collection, evaluation, evidence,
   capability, or severity path (INV-13; SEC-010; VERD-007; CON-004).
7. **Concrete authentication implementation:** token types, credential
   mechanisms, flows, caches, stores, or permission-name constants.
   `Core` consumes only the non-credential assessment/execution context
   (§5); the access-context abstraction shape belongs to
   `authentication-design.md` (INV-08).
8. **Concrete logging/telemetry backend:** `Core` MAY define (via
   `dependency-boundaries.md`) a minimal logging/telemetry abstraction
   boundary if required; it MUST NOT reference a concrete backend, and
   MUST NOT transmit tenant data to undisclosed destinations (CON-005;
   NFR-004).
9. **Environment variables / configuration source directly:** `Core`
   receives validated deterministic configuration values as explicit
   parameters; it MUST NOT read process environment, configuration files,
   or registries itself. Configuration surfaces, secure defaults,
   unknown/invalid-option semantics, and format choices belong to
   `configuration-design.md` (INV-16).
10. **Persistence/database technology:** No database, ORM, migration,
    SaaS storage, or archival-mechanism dependency unless later explicitly
    approved through architecture change control. V1 operates on local/CI
    artifacts; persistence posture (if any) is designed separately
    (findings-evidence architecture §23).
11. **Any other `src/` project:** `Core` takes no project reference to
    `Application`, `Infrastructure.Graph`, `Output`, or `Cli`
    (`solution-structure.md` §8). Filesystem-write helpers beyond
    in-memory model needs, ambient credential access, and undisclosed
    network transmission are likewise forbidden.

---

## 13. Contract versioning/evolution principles

Compatibility and stability principles — without promising a public stable
API before V1 (no such promise is made here):

1. **Additive and reversible preference.** Consistent with repository
   policy: prefer additive contract extensions (new optional fields,
   new capability categories, new rule registrations, new renderer
   projections) over breaking redefinitions. A breaking contract change
   requires explicit justification, impact statement, and recovery path
   before approval (`README.md` §8).
2. **Identity stability.** Rule IDs are stable across versions; behavior
   changes produce a new rule version; deprecated rules retain their ID
   with supersession metadata; removal is deprecation, not ID reassignment
   (rule-engine architecture §8). Identifier concepts in §3.12 follow the
   same discipline once formats are designed.
3. **Version awareness in evidence.** Every evaluation MUST remain
   attributable to the rule definition/version and deterministic
   configuration that produced it, so historical records keep their meaning
   (rule-engine architecture §§7–8; findings-evidence snapshot semantics).
4. **Capability-gated evolution.** New identity categories, relationship
   kinds, or enrichment sources enter as explicitly capability-gated,
   provenance-preserving additions that unsupported environments report as
   NOT_EVALUATED/NOT_APPLICABLE — never as silent reinterpretations of
   existing states (INV-07; INV-15).
5. **No silent semantic change.** A contract change that alters the meaning
   of PASS/FAIL, the completeness bar for PASS, or the mapping of failures
   to NOT_EVALUATED vs ERROR is a versioned behavior change with change
   traceability — never an editorial clarification.
6. **Deferral.** Exact version-format choices (rule versions, schema
   versions, output versions), compatibility test policy, and
   golden/snapshot policy belong to `rule-engine-contracts.md`,
   `findings-evidence-schema.md`, `output-renderer-contracts.md`, and
   `testing-seams.md` respectively.

---

## 14. Security implications

Stated as structural support, not as implemented controls:

- **Read-only posture (INV-01; FR-040; SEC-003–SEC-007; CON-001).** No
  contract defines a write, delete, update, remediation,
  credential-rotation, permission-modification, or other tenant-mutating
  operation. The only network-capable project is the outer collection
  adapter, constrained by collector contracts to documented read-only
  behavior.
- **Determinism + five states (INV-02; INV-05).** Co-locating the
  evaluation contract, state semantics, and aggregation requirements in
  one dependency-free boundary keeps the deterministic pipeline auditable
  as a unit.
- **Provider isolation / normalized boundary (INV-03; INV-04).** Port
  direction (§10) plus §12 prohibitions keep provider types out of rule,
  graph, finding, and output-model contracts; structural-test intent makes
  bypass visible.
- **Capability awareness / failure transparency / false-PASS resistance
  (INV-07; INV-14; CAP-002).** Capability/failure state travels the same
  inward path as observations, so evaluation always sees the completeness
  metadata its PASS gating requires; §4 critical rules are established
  at the contract level.
- **Evidence/provenance (INV-06; INV-11).** §7 requires traceability at
  every transformation without inventing integrity controls (no hashing/
  signing claims here; those belong to `findings-evidence-schema.md` as
  future hardening).
- **Secret exclusion / least privilege (INV-09; INV-08; SEC-001; SEC-002;
  OUT-006).** §§5, 7, 10, 12 leave secrets no type, field, or path into
  domain, graph, findings, evidence, diagnostics, or output-model
  contracts; privilege rationale belongs to `authentication-design.md`.
- **Core/output separation / renderer non-authority (INV-12).** §11 plus
  §12 make renderer overreach a contract violation, not a judgment call.
- **AI non-authority (INV-13).** §12 prohibition plus §4 rule 6 and §11
  leave no contract path for model output to reach verdicts, evidence,
  capability, or severity.
- **Tenant isolation (INV-10).** §5 plus integrity-failure handling in §9
  reject cross-tenant edges, evidence, and aggregation visibly.
- **Undocumented-behavior resistance (INV-15; CAP-005).** No endpoint,
  permission, property, mapping, or licensing claim is made here; every
  such detail is a named TBD (see §17).
- **Secure defaults (INV-16).** Configuration weakening is explicit and
  visible (§6); optional capabilities MUST NOT silently broaden privilege
  (§8).

---

## 15. Testability implications

Contract support for testing (scenario catalog itself belongs to
`testing-seams.md`; no tests are created here):

- **Provider-free core tests.** §12 prohibitions plus value-oriented
  inputs (§2) let core tests exercise normalization references → graph
  projections → rule evaluation → findings construction purely from
  synthetic normalized fixtures: determinism matrix, five-state coverage,
  false-PASS/false-FAIL defense, provenance integrity, error taxonomy.
- **Fakeable edges.** Service contracts (§3.1, §3.3, §3.6, §10, §11 ports)
  give `Application` tests injectable fakes for collectors, capability
  sources, auth contexts, clocks/time references, and output ports —
  proving propagation (capability gap → NOT_EVALUATED, failure → ERROR,
  tenant mismatch → visible integrity failure, cancellation → visible
  state) without real I/O.
- **Adversarial and isolation coverage.** Explicit tenant context (§5),
  untrusted-string opaqueness (§11), and integrity-failure class (§9)
  give seams for tenant-isolation, injection-payload, malformed-input,
  and failure-injection scenarios.
- **Non-authority verification.** Renderer/AI non-authority (§§4, 11, 12)
  is testable as pass-through preservation: identical canonical results
  in, identical semantics out, across all formats.
- **Structure tests.** §12 prohibitions plus §3 module-boundary rules are
  encodable as structural assertions once `dependency-boundaries.md`
  defines the rule set (e.g., rule namespaces reference no provider/
  renderer/auth namespaces).

---

## 16. Explicit non-goals

The following are explicitly NOT part of these contracts (each traces to
requirements non-goals or architecture out-of-scope):

1. No SaaS hosting, multi-tenant service, or cross-tenant aggregation
   (NG-003).
2. No production web dashboard or user-facing web interface (NG-004).
3. No remediation, credential rotation/lifecycle operations, permission or
   policy modification, or identity create/delete/disable (NG-001; NG-002;
   NG-007; SEC-003–SEC-007).
4. No autonomous or AI-driven security decisions or AI-generated verdicts
   (NG-005; SEC-010; VERD-007; CON-004).
5. No real-time monitoring, alerting, or event-driven analysis (NG-008).
6. No replacement of Entra administration/governance products (NG-006).
7. No Graph endpoint/permission/property selection, no Agent ID mapping, no
   licensing-behavior claims, no SDK/method choices, no package versions,
   no schema finalization, no CLI syntax/exit-code finalization, no SARIF
   mapping, no HTML templating strategy, no OAuth-flow choice — all
   deferred as TBDs (§17).
8. No persistence/database design, no signing/hashing implementation, no
   numeric resource limits, no target-framework choice.

---

## 17. Open TBDs and owner documents

Each TBD states what is unknown, why it cannot be resolved here, and which
later document owns its resolution. None is resolved by speculation.

| # | TBD | Why unresolved here | Owned by |
| --- | --- | --- | --- |
| T-01 | Concrete C# interface/record shapes and member lists for every family in §3. | Requires coordination with each owning schema document; choosing members here would preempt owners and risk inventing undocumented semantics. | Respective owner (`domain-types.md`, `capability-model.md`, `identity-graph-contracts.md`, `rule-engine-contracts.md`, `findings-evidence-schema.md`, `error-result-model.md`, `collector-contracts.md`, `output-renderer-contracts.md`), coordinated by this document |
| T-02 | Exact C# namespace names and folder layout within each project. | Logical module boundaries (§3) are sufficient at this stage; physical names require the seam catalog. | This document with `dependency-boundaries.md` |
| T-03 | Capability-state names, serialization, hierarchy, and per-category granularity. | Requires collection + rule-engine capability semantics finalization; inventing names here would duplicate the owner. | `capability-model.md` |
| T-04 | Graph node/edge shapes, query/traversal API, observed-vs-derived taxonomy detail, INV-G enforcement mechanics. | Requires graph-contract design; this document only references the projection boundary. | `identity-graph-contracts.md` |
| T-05 | Complete `RuleDefinition`/`RuleEvaluation` schemas, registration/compatibility mechanics, version format, pipeline stage contracts, ordering/parallelism, severity taxonomy. | Requires rule-engine contract design; rule content itself is out of scope. | `rule-engine-contracts.md` |
| T-06 | `Finding` vs `RuleEvaluation` schemas, evidence/provenance/diagnostic schemas, emission policy, fingerprint strategy, identifier formats, redaction schema, hashing/signing/timestamping posture. | Requires findings/evidence schema design; integrity mechanisms need separate security review. | `findings-evidence-schema.md` |
| T-07 | Shared `Result`/`Error` shape, failure-category taxonomy, NOT_EVALUATED-vs-ERROR mapping, diagnostic-vs-finding separation, sanitization rules. | Requires error-model design across all pipeline stages. | `error-result-model.md` |
| T-08 | Collector interfaces, source-observation envelope schema, pagination/retry/cancellation surfaces, normalization handoff mechanics, provider-communication decision. | Requires collector-contract design against published documentation; no endpoint/permission/SDK choice is made here. | `collector-contracts.md` with `dependency-boundaries.md` |
| T-09 | Canonical output projection shape, per-renderer contracts, SARIF mapping, HTML strategy, CLI syntax/exit codes, injection/file-safety policies. | Requires output-contract design; no presentation choice is made here. | `output-renderer-contracts.md` |
| T-10 | `AuthorizedAccessContext` shape, per-capability authorization surface, tenant-validation mechanics, token/secret handling, interactive vs workload contexts. | Requires authentication-design against validated flows; token mechanics are explicitly excluded here. | `authentication-design.md` |
| T-11 | Assessment options, rule-set/version selection, capability selection, output options, secure defaults, unknown/invalid-configuration semantics. | Requires configuration-surface design. | `configuration-design.md` |
| T-12 | Per-seam interface placement, dependency-injection mechanics, logging/telemetry abstraction surface, structural-test rule set and tooling. | Requires seam-catalog design. | `dependency-boundaries.md` |
| T-13 | Assessment-time representation detail, clock/time-test mechanics, resource/bounding limits. | Requires rule-engine + testing-seam coordination; no constant is chosen here. | `rule-engine-contracts.md` with `testing-seams.md` (time principles from `domain-types.md` §12) |
| T-14 | Identifier formats (assessment, evaluation, finding, evidence, provenance, identity keys), generation strategy, dedup/conflict policy detail. | Requires findings-schema + domain-type coordination; formats need separate design. | `findings-evidence-schema.md` with `domain-types.md` |
| T-15 | Agent identity endpoint/property/relationship mappings; managed-identity classification mappings; optional enrichment scope. All require validation against published documentation and MUST NOT be invented. | Undocumented provider semantics cannot be resolved by design speculation (INV-15; CAP-005). | `collector-contracts.md` (mappings), `capability-model.md` (granularity), `domain-types.md` (classification vocabulary) |

---

## 18. Acceptance criteria

These contracts are accepted when:

1. **Family coverage.** Every family in §3 has an explicit responsibility,
   a classified contract kind (value/record, service contract, or module
   boundary with rationale), and a named owning document for its detailed
   shape — with no orphan responsibility and no forced-interface modeling.
2. **Five-state fidelity.** §4 defines exactly PASS, FAIL, NOT_EVALUATED,
   NOT_APPLICABLE, ERROR with the §4 critical rules intact; no sixth
   authoritative state exists; data/observation semantics are not states.
3. **Determinism and isolation.** §§2, 5–6, 10, 12 preserve INV-02/INV-03/
   INV-04: pure evaluation over normalized inputs, no ambient provider
   state, no hidden network access, explicit operation context only.
4. **Tenant and capability explicitness.** §§5, 8–9 carry single-tenant
   scoping, capability granularity, and the four-class error/result
   separation without redefining owner schemas.
5. **Authority placement.** Rule authority in evaluation logic, evidence
   authority in findings construction, projection-only rendering (§11),
   and no authority in orchestration routing, adapters, hosts, or any
   future AI consumer — satisfying INV-06/INV-12/INV-13 structurally.
6. **Secret exclusion.** §§5, 7, 10, 12 admit no secret-bearing type,
   field, or path; diagnostics and provenance requirements exclude
   credential material.
7. **Dependency soundness.** §12 prohibitions hold without exception; no
   Graph/SDK, CLI, renderer, web, AI/LLM, auth-implementation, logging-
   backend, environment/configuration-source, or persistence dependency is
   admitted into `Core` contracts.
8. **Versioning discipline.** §13 requires additive evolution, identity
   stability, and version-aware evidence without promising a public stable
   API before V1.
9. **TBD explicitness.** Every implementation-sensitive unknown is listed
   in §17 with its owning document; no endpoint, permission, property,
   mapping, package, or numeric limit is invented.
10. **Non-goal containment.** Nothing in §16 appears as an assumed
    capability.
11. **Cross-document agreement.** The consistency contract with
    `domain-types.md` holds: identical five states, tenant scoping,
    normalized vocabulary by reference, provider isolation, capability
    awareness, evidence/provenance by reference, secret exclusion,
    determinism, failure transparency, renderer non-authority, AI
    non-authority — with no duplicated normative schema.

---

*(End of file)*
