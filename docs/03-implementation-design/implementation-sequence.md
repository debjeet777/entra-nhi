# Implementation Sequence

> **Phase:** 0.3.10
> **Package:** Implementation Design
> **Status:** Proposed — design only, no code or project files created
> **Date:** 2026-09-18

---

## 1. Status and scope

- **Status:** Proposed controlled build order for EntraNHI V1. This
  document is design output only. It authorizes no implementation.
- **Scope:** Defines the engineering sequence — which contracts and seams
  are established first, which adapters/rules/renderers follow, and which
  gate criteria MUST hold before the next stage — sequenced to protect
  provider isolation and core/output separation. Derived from the
  approved requirements (`docs/01-requirements/product-requirements.md`),
  architecture (`docs/02-architecture/`, including invariants
  INV-01–INV-16, trust boundaries, threat model, testing architecture),
  and the completed Phase 0.3 contracts, as structured by
  `solution-structure.md` and constrained by `dependency-boundaries.md`.
- **What this document is:** The owner of build order and stage gates
  (`README.md` §§5–6). First normative definition of stage order, entry/
  exit criteria, false-PASS/tenant-isolation/determinism/evidence gates,
  stop/go conditions, and the release-candidate gate lives here; all
  other documents reference this order rather than resequencing it.
- **What this document is not:** It is not an implementation start
  authorization, a delivery-date promise, a staffing plan, a CI workflow
  design, a package selection, a Graph binding, a rule catalogue, a test
  suite, or a configuration-file format. Those belong to their owning
  documents or to future reviewed decisions. Until the implementation
  gate in `README.md` §9 is met, authorized work remains limited to
  design documents.
- **Language discipline:** Per `README.md` §1, this document distinguishes
  (a) **approved architectural requirements** traced to requirement/
  invariant IDs, (b) **proposed sequencing decisions** owned by this
  document, and (c) **unresolved TBDs** naming the owning later concern
  (§23). Nothing herein claims any stage is implemented, tested, or
  enforced.
- **Canonical assessment states (referenced, not redefined):** The
  authoritative assessment states remain EXACTLY `PASS`, `FAIL`,
  `NOT_EVALUATED`, `NOT_APPLICABLE`, `ERROR`, with semantics owned by
  `core-contracts.md` §4 and the pipeline owned by
  `rule-engine-contracts.md`. No sixth authoritative assessment state is
  created here. Operational terms used below (incomplete collection,
  authorization limitation, provider failure, unknown completeness,
  cancellation, resource exhaustion, confirmed-empty, capability
  unavailable/unknown) are explicitly typed in their correct layer per
  `error-result-model.md` and `capability-model.md` — never as assessment
  states.
- **Provider discipline:** No Microsoft Graph endpoint, permission/scope
  name, SDK type or method, Entra property, Agent Identity mapping,
  licensing-behavior claim, or undocumented provider relationship is
  invented or selected here (INV-15). Provider references below mean "the
  Graph communication mechanism and collection surface (TBD owned by
  `collector-contracts.md` with `capability-model.md` /
  `authentication-design.md`)" and nothing more specific.
- **Package discipline:** No framework, library, SDK, test-framework,
  scanner, workflow action, or version is selected here (CON-003).
  Tooling references below are capabilities with TBD owners (§23).

---

## 2. Purpose and sequencing principles

This is an engineering sequence, not a delivery-date promise. Its
objective is to implement the highest-risk security and semantic
foundations before provider/UI convenience layers.

Implementation order prioritizes, in this order:

1. **Architectural invariants** — the DAG rooted at `Core`, sole CLI
   composition root, provider isolation, renderer non-authority, AI
   non-authority, read-only posture (INV-01–INV-04, INV-08–INV-13).
2. **Canonical result semantics** — exactly five states with explicit
   reason/context on every non-`PASS`/`FAIL` path (INV-05).
3. **False-PASS resistance** — `PASS` only on affirmative satisfaction
   plus rule-defined input completeness; incompleteness never yields
   `PASS` (rule-engine architecture; threat-model paths A, C, J).
4. **Tenant isolation** — single-tenant scoping with visible integrity
   failure on mismatch/contamination (INV-10).
5. **Deterministic behavior** — identical normalized inputs, capability
   state, rule version/configuration, and deterministic context produce
   identical outcomes (INV-02).
6. **Evidence/provenance** — traceable evidence for verdicts, structured
   context otherwise, preserved end to end and never fabricated
   (INV-06; INV-11).
7. **Capability/completeness semantics** — explicit unavailable/unknown/
   limited/failed/partial/indeterminate handling flowing into evaluation
   (INV-07; INV-14).
8. **Testability** — every boundary exercised from synthetic fixtures
   before live-provider or presentation work begins (testing
   architecture).

Provider integration and presentation polish come last because they are
the most volatile and the least load-bearing: they consume the above
foundations and MUST NOT redefine them.

---

## 3. Preconditions before source implementation

No production source implementation begins until:

1. **Phase 0.3 implementation design is reviewed and locked** per the
   implementation gate in `README.md` §9 (all 16 documents authored,
   internally consistent, no conflicting definitions across owners).
2. **Unresolved implementation-critical TBDs are identified** — the
   blocking subset (narrowly: target framework/monikers, build
   strictness, namespace layout, port placement where ambiguous,
   assessment-time representation, failure-taxonomy mapping detail,
   test-harness mechanics) is listed with owners; see §23 and each
   stage's deferred list in §5.
3. **Initial concrete technology/package decisions are reviewed before
   adoption.** Any package, SDK, framework, scanner, or workflow-action
   choice requires explicit review with supply-chain rationale (CON-003);
   this document selects none.
4. **No secret-bearing development configuration is committed.**
   Repository-stored credentials, secret-bearing sample files, and
   production tenant data are prohibited by repository policy
   (SEC-002; SEC-009); development uses synthetic fixtures and, where a
   live context is ever needed, externally supplied non-persisted
   operator credentials under the rules in §17.
5. **Repository security controls remain active.** Nothing in this
   sequence bypasses secret scanning, signing requirements, branch
   protection, or required checks.

TBD posture: this document distinguishes **blocking** decisions (those
without which the next stage cannot be built correctly — e.g., the
error/result shape before orchestration error mapping) from
**deferrable** decisions (those fillable later without redesign — e.g.,
retry constants, SARIF profile detail, HTML asset strategy). Not all
TBDs MUST be resolved before any implementation; each stage in §5 names
its prerequisite contracts and its explicitly deferred decisions.

---

## 4. Implementation stages — overview

The controlled stage sequence below is consistent with the project
topology in `solution-structure.md` §4 and the dependency direction in
`dependency-boundaries.md` §4. Stage names/order are reconciled against
approved documentation; no unapproved capability is introduced.

| Stage | Name | Primary project(s) |
| --- | --- | --- |
| 1 | Solution/build skeleton and dependency enforcement | Solution + `EntraNHI.Architecture.Tests` intent |
| 2 | Core canonical domain/result contracts | `EntraNHI.Core` |
| 3 | Capability/completeness/error semantics | `EntraNHI.Core` |
| 4 | Evidence/provenance contracts | `EntraNHI.Core` |
| 5 | Identity graph | `EntraNHI.Core` |
| 6 | Deterministic rule engine | `EntraNHI.Core` |
| 7 | Application orchestration | `EntraNHI.Application` |
| 8 | Authentication/provider boundary foundation | Outer boundary (`Cli` composition + adapter consumption via `authentication-design.md`) |
| 9 | Microsoft Graph collector adapter | `EntraNHI.Infrastructure.Graph` |
| 10 | Output-neutral assessment assembly | `EntraNHI.Core` + `EntraNHI.Application` handoff |
| 11 | CLI host/composition | `EntraNHI.Cli` |
| 12 | Output renderers | `EntraNHI.Output` |
| 13 | Integrated synthetic assessment flow | All (synthetic only) |
| 14 | Security/adversarial hardening | All |
| 15 | CI/supply-chain/release hardening | Build/CI/release posture |
| 16 | Release-candidate validation | All (gate, not a build stage) |

Build order follows dependency direction: `Core` → `Application` →
`Infrastructure.Graph` + `Output` (either order) → `Cli`, with
`Architecture.Tests` as a structural gate throughout. Detailed stage
contracts follow in §5. Testing progression across stages is mapped in
§16; cross-cutting gates are fixed in §§7–10.

---

## 5. Stage definitions

For every stage: objective, primary project(s), prerequisite
contracts/stages, expected implementation artifacts (conceptual level),
required tests, security gates, exit criteria, and important deferred
decisions/TBDs. No code is written here; artifacts below are
contract/implementation obligations for the later implementation phase,
not created artifacts.

### Stage 1 — Solution/build skeleton and dependency enforcement

- **Objective.** Establish the solution tree and make dependency
  violations structurally visible from the first commit of
  implementation.
- **Primary project(s).** Solution structure; `EntraNHI.Architecture.Tests`
  intent (structural enforcement only, no behavior).
- **Prerequisites.** Locked `solution-structure.md` and
  `dependency-boundaries.md`; reviewed TBD owner list (§23).
- **Expected artifacts (conceptual).** Five production projects and six
  test projects with the allowed-reference matrix
  (`dependency-boundaries.md` §4); namespace organization intent
  (`solution-structure.md` §7); composition-root singularity (CLI only);
  build order `Core` → `Application` → adapters → `Cli`.
- **Required tests.** Architecture/dependency tests asserting the DAG,
  `Core` independence, forbidden-type absence in `Core`, and no second
  composition root (scenario catalog owned by `testing-seams.md` §14).
- **Security gates.** Forbidden-dependency review on the skeleton;
  supply-chain restraint acknowledged (no unapproved package admitted to
  satisfy scaffolding).
- **Exit criteria.** Skeleton builds with zero provider/renderer/host
  references in `Core`; architecture tests execute and pass on the empty
  surface; any skeleton deviation is corrected, not waived.
- **Deferred.** Target-framework/moniker choice, build strictness,
  signing/packaging, enforcement tooling selection (TBD §23, T-01, T-02).

### Stage 2 — Core canonical domain/result contracts

- **Objective.** Establish the stable inward vocabulary every later
  stage builds against: normalized domain references, tenant assessment
  context, identifier/value concepts, and the five-state evaluation
  contract.
- **Primary project(s).** `EntraNHI.Core`.
- **Prerequisites.** Stage 1 exit; `core-contracts.md`,
  `domain-types.md` (absent/unknown semantics), `error-result-model.md`
  (shape, not yet full taxonomy mapping).
- **Expected artifacts (conceptual).** Tenant-context carriage on every
  tenant-data contract; absent-vs-unknown distinction; five-state
  vocabulary with structured reason/context on non-`PASS`/`FAIL` paths;
  no ambient tenancy, no ambient provider state, no secret-bearing
  field.
- **Required tests.** Contract tests for absent/unknown handling,
  tenant-context carriage, and five-state vocabulary integrity —
  synthetic only.
- **Security gates.** Secret-exclusion review of every new field (no
  credential/token-bearing member); provider-type absence verified.
- **Exit criteria.** Core vocabulary is implementable and testable with
  no provider, network, secret, filesystem, renderer, or tenant access;
  reviewers confirm no sixth state and no provider detail invented.
- **Deferred.** Identifier formats, serialization schemas, numeric
  bounds (TBD §23, T-03).

### Stage 3 — Capability/completeness/error semantics

- **Objective.** Make "what could the assessment actually see"
  representable before any rule trusts it: capability states,
  collection-completeness conditions, and the shared failure taxonomy
  with its mapping to `NOT_EVALUATED` vs `ERROR`.
- **Primary project(s).** `EntraNHI.Core`.
- **Prerequisites.** Stage 2 exit; `capability-model.md`,
  `collector-contracts.md` (envelope/completeness handoff concepts),
  `error-result-model.md` (failure categories, sanitization).
- **Expected artifacts (conceptual).** Capability-state vocabulary and
  propagation shape; completeness conditions (including
  confirmed-empty vs indeterminate/partial/failed); failure-category
  taxonomy; diagnostic-vs-finding separation; sanitization rules.
- **Required tests.** Capability/completeness matrix tests proving:
  unavailable/unknown/limited/failed completeness never yields `PASS`;
  confirmed-empty is representable and distinct from indeterminate;
  sanitization holds on diagnostics.
- **Security gates.** False-PASS gate draft (§7) exercised on capability
  paths; authorization-limitation ≠ emptiness verified.
- **Exit criteria.** Every completeness/capability condition maps to an
  explicit handling path; no silent coercion to `PASS`/`FAIL` remains.
- **Deferred.** Per-category granularity detail, numeric retry/timeout
  constants, fatal-vs-isolated policy detail (TBD §23, T-03, T-04).

### Stage 4 — Evidence/provenance contracts

- **Objective.** Make "what proves this conclusion" structural before
  any rule emits a verdict: evaluation-vs-finding distinction, evidence
  references, provenance chain, emission-policy hook, redaction hook.
- **Primary project(s).** `EntraNHI.Core`.
- **Prerequisites.** Stages 2–3 exit; `findings-evidence-schema.md`.
- **Expected artifacts (conceptual).** Evidence linkage (verdict →
  normalized facts); provenance preservation across transformation
  boundaries; structured reason/context for `NOT_EVALUATED` /
  `NOT_APPLICABLE` / `ERROR`; fabrication-defense obligations;
  redaction obligations.
- **Required tests.** Evidence-linkage tests (every `PASS`/`FAIL`
  traceable; missing/fabricated evidence rejected); provenance-
  preservation tests across each transformation; redaction tests.
- **Security gates.** Evidence/provenance gate (§10) draft; secret
  exclusion re-verified on every evidence/provenance field.
- **Exit criteria.** No verdict path exists without an evidence
  obligation; no non-verdict path exists without a structured-reason
  obligation; provenance cannot be silently dropped.
- **Deferred.** Identifier/fingerprint mechanics, signing/hashing
  posture, emission-policy tuning (TBD §23, T-03).

### Stage 5 — Identity graph

- **Objective.** Establish the deterministic identity-graph projection
  over normalized contracts with integrity rules INV-G1–INV-G8.
- **Primary project(s).** `EntraNHI.Core`.
- **Prerequisites.** Stages 2–4 exit; `identity-graph-contracts.md`.
- **Expected artifacts (conceptual).** Node/edge construction from
  normalized inputs only; observed-vs-derived classification;
  traversal/projection operations; cross-tenant edge rejection;
  cycle/self-reference/disconnect tolerance with explicit integrity
  behavior.
- **Required tests.** Integrity-rule tests (INV-G1–INV-G8) from synthetic
  topologies: cycles, self-references, disconnected subgraphs,
  duplicates, unknown relationship types, incomplete construction;
  tenant-isolation tests at graph construction.
- **Security gates.** Tenant-isolation gate draft (§8) at graph layer;
  resource-exhaustion visibility (pathological graphs fail visibly,
  never `PASS` by truncation).
- **Exit criteria.** Graph is a pure projection of normalized contracts;
  no provider type, renderer type, or secret reaches graph construction.
- **Deferred.** Query/traversal API ergonomics, serialization for
  diagnostics, size/memory policy numbers (TBD §23, T-03).

### Stage 6 — Deterministic rule engine

- **Objective.** Establish the deterministic evaluation pipeline with
  exactly five terminal states, per-rule isolation, and configuration-
  validation semantics.
- **Primary project(s).** `EntraNHI.Core`.
- **Prerequisites.** Stages 2–5 exit; `rule-engine-contracts.md`
  (pipeline order: discovery → compatibility → applicability →
  capability validation → input validation → evaluation → state
  assignment → evidence handoff → diagnostics → aggregation).
- **Expected artifacts (conceptual).** Rule-definition surface with
  identity/versioning; staged pipeline with the mandated validation
  order; per-rule isolation; deterministic configuration consumption;
  assessment-time reference seam (no wall-clock reads in evaluation).
- **Required tests.** Five-state matrix tests; ordering tests (capability
  before evaluation; applicability before capability-gated logic);
  isolation tests (one rule's failure cannot corrupt another);
  determinism/repeatability tests (§9); negative/adversarial tests for
  the first rule-shaped fixtures (§13).
- **Security gates.** False-PASS gate (§7) fully exercised at engine
  level; determinism gate (§9) draft; evidence gate (§10) completed for
  engine outputs.
- **Exit criteria.** Engine foundation is declared complete only when
  evidence-backed `PASS`/`FAIL` and structured context for all
  non-verdict states are proven (§10), with determinism proven on
  controlled inputs (§9).
- **Deferred.** Final V1 rule catalogue content (see §13 — only
  contract/rule-identity work here), rule ordering/parallelism choices,
  resource limits, severity taxonomy (TBD §23, T-05).

### Stage 7 — Application orchestration

- **Objective.** Coordinate the approved use case end to end through
  ports: authenticate → capabilities → collect → normalize → graph →
  evaluate → findings → output-neutral results, propagating capability
  state and structured failures at every step.
- **Primary project(s).** `EntraNHI.Application`.
- **Prerequisites.** Stage 6 exit; collector/capability/auth/output port
  concepts (`collector-contracts.md`, `capability-model.md`,
  `authentication-design.md`, `output-renderer-contracts.md` with
  placement per `dependency-boundaries.md`).
- **Expected artifacts (conceptual).** Pipeline coordination with
  per-stage error mapping to `NOT_EVALUATED` vs `ERROR`;
  tenant-scope carriage; cancellation/timeout visibility;
  no-authority routing of authoritative `Core` results.
- **Required tests.** Orchestration tests with faked adapters proving
  propagation (capability → `NOT_EVALUATED`, failure → `ERROR`, tenant
  mismatch → visible integrity failure, cancellation → visible state);
  no-authority verification (orchestrator cannot change verdicts).
- **Security gates.** Tenant-isolation gate (§8) at orchestration layer;
  secret non-inspection verified (orchestrator carries the
  authorized-context reference without inspecting it).
- **Exit criteria.** Full pipeline coordinates correctly against fakes
  with zero provider/renderer/auth implementation referenced.
- **Deferred.** Injection mechanics (container choice), clock/ordering
  implementation, numeric timeout/concurrency bounds (TBD §23, T-01,
  T-04).

### Stage 8 — Authentication/provider boundary foundation

- **Objective.** Establish the consumable, credential-free authorized-
  context abstraction and its failure semantics — without selecting
  flows, mechanisms, or storage.
- **Primary project(s).** Outer boundary (abstraction owned by
  `authentication-design.md`; wired by `Cli` composition; consumed by
  the provider adapter).
- **Prerequisites.** Stage 7 exit (ports exist to consume the
  abstraction); `authentication-design.md` §§4–6, 10–13.
- **Expected artifacts (conceptual).** Execution-mode abstraction
  (interactive/workload, conceptual only); authorized-context reference
  shape (established/not-established, tenant binding, mode, safe context
  reference, sanitized failure, cancellation, correlation — never raw
  credentials); establishment/authorization-failure categories;
  tenant-validation mechanics concepts.
- **Required tests.** Boundary tests proving: successful establishment
  implies none of authorization sufficiency, completeness, capability
  support, or `PASS`; denial/failure surfaces as explicit limitation or
  systemic failure (sanitized); wrong-tenant/incompatible-context
  refused; cancellation distinct from verdicts.
- **Security gates.** Secret-confinement review (no credential/token
  material in domain, graph, rules, findings, evidence, results,
  renderers, logs, fixtures); least-privilege posture acknowledged
  (read-only minimum; exact permission rationale deferred).
- **Exit criteria.** Orchestration and the future adapter can consume the
  abstraction with fakes; no flow, SDK class, permission name, cache, or
  store is selected.
- **Deferred.** Exact flows, mechanisms, token-cache/storage policy,
  consent UX, sovereign-cloud endpoints, permission names (TBD §23,
  T-06).

### Stage 9 — Microsoft Graph collector adapter

- **Objective.** Implement the single provider-coupled adapter behind the
  Stage 7–8 ports: query execution, pagination, bounded resilience, and
  normalization-handoff shaping.
- **Primary project(s).** `EntraNHI.Infrastructure.Graph`.
- **Prerequisites.** Stages 3, 7, 8 exit (inward contracts and test seams
  exist first); `collector-contracts.md` (envelope, pagination/retry/
  cancellation surfaces, handoff).
- **Expected artifacts (conceptual).** Per-concern collection with
  provenance/tenant/capability preservation; partial-failure shaping;
  collection-window semantics (no transactional-snapshot claim unless
  the source provides it).
- **Required tests.** Adapter tests with synthetic/sanitized provider
  doubles: pagination sequences, bounded retry/throttling, partial
  collection, authorization-denial and malformed-response handling,
  provenance/capability preservation, secret exclusion, tenant-context
  carriage. No live tenant; no invented endpoint/permission claims.
- **Security gates.** False-PASS gate (§7) re-exercised provider-side
  (gaps/failures/denials never `PASS`); provider-type leakage check
  (no SDK/payload type in `Core` contracts).
- **Exit criteria.** Adapter delivers observations + capability +
  provenance + tenant context through ports with all failure modes
  explicit; `Core` remains provider-free.
- **Deferred.** Exact endpoints, properties, Agent Identity mappings,
  SDK-vs-HTTP choice, retry/timeout/concurrency constants (TBD §23,
  T-04).

### Stage 10 — Output-neutral assessment assembly

- **Objective.** Assemble the authoritative, renderer-independent result:
  the canonical output-model projection of evaluations, findings,
  evidence, capability, and diagnostics.
- **Primary project(s).** `EntraNHI.Core` + `EntraNHI.Application`
  handoff (model vocabulary owned by `output-renderer-contracts.md`
  canonical-model concept).
- **Expected artifacts (conceptual).** Output-neutral result shape
  preserving all five states with evidence/reason intact; aggregation
  semantics that never suppress `ERROR`/`NOT_EVALUATED`; redaction
  applied before projection.
- **Required tests.** Semantic-preservation tests: every state and its
  payload survives assembly; aggregation cannot manufacture `PASS`;
  injection-neutral carriage (tenant strings carried opaquely).
- **Security gates.** Renderer-non-authority precondition (assembly is
  the authority; renderers are projections); secret-exclusion
  re-verified on the assembled result.
- **Exit criteria.** A stable output-neutral result exists that Stages
  11–12 can consume without reaching back into evaluation internals.
- **Deferred.** Serialization schemas, JSON versioning, SARIF mapping
  detail (see Stage 12; TBD §23, T-07).

### Stage 11 — CLI host/composition

- **Objective.** Establish the thin V1 host and sole composition root:
  argument handling, configuration-source loading shape, pipeline
  composition, console/file wiring, exit-behavior mapping policy.
- **Primary project(s).** `EntraNHI.Cli`.
- **Prerequisites.** Stages 7, 8, 10 exit; `configuration-design.md`
  (loading shape; semantics) and `output-renderer-contracts.md`
  (exit-code mapping policy).
- **Expected artifacts (conceptual).** Command dispatch; validated-
  configuration consumption (secure defaults, explicit opt-in,
  unknown/invalid visible failure); composition wiring; failure
  visibility (incomplete/error surfaced, never silent success).
- **Required tests.** Host tests with faked adapters: argument/config
  handling, exit-code mapping, safe file-destination behavior, failure
  visibility; pass-through verification (no verdict logic beyond
  routing).
- **Security gates.** CLI authentication discipline (business/command
  logic never inspects/persists/logs/serializes/exposes/embeds tokens;
  handling confined to the authentication/provider boundary);
  path-safety obligations for destinations.
- **Exit criteria.** Full synthetic run composes and terminates
  deterministically with correct exit behavior and no secret handling
  outside the approved boundary.
- **Deferred.** Exact CLI syntax, exit-code values, configuration-file
  format (TBD §23, T-07).

### Stage 12 — Output renderers

- **Objective.** Implement the four V1 projections (CLI/text, JSON,
  SARIF, HTML) as isolated adapters consuming only the Stage 10 result.
- **Primary project(s).** `EntraNHI.Output`.
- **Prerequisites.** Stage 10 exit; `output-renderer-contracts.md`
  (per-renderer contracts, encoding/file-safety obligations,
  renderer-failure semantics).
- **Expected artifacts (conceptual).** One stable contract per renderer
  plus internal encoding/file helpers; renderer-failure isolation
  (failure is a system/output diagnostic, never a tenant finding).
- **Required tests.** Five-state preservation per format; renderer-
  non-authority tests; injection-payload tests (HTML/XSS,
  ANSI/control, CR/LF, bidi, malformed Unicode); JSON/SARIF validity;
  path-traversal/overwrite/partial-artifact tests; redaction tests;
  no-network/no-telemetry verification.
- **Security gates.** Injection-defense review; SARIF-mapping-limitation
  explicitness (§14); filesystem-safety review.
- **Exit criteria.** All four renderers project the same authoritative
  result without divergence in state semantics; limitations (notably
  SARIF) remain explicit.
- **Deferred.** Templating strategy, CSP/asset choices, SARIF
  version/profile finalization, schema versioning (TBD §23, T-07).

### Stage 13 — Integrated synthetic assessment flow

- **Objective.** Prove the first true vertical slice: composed pipeline
  (Stages 7+8+9 fakes/doubles + 10+11+12) executing end to end from
  synthetic inputs to all four rendered artifacts.
- **Primary project(s).** All (synthetic only — no real tenant
  credentials/data).
- **Prerequisites.** Stages 1–12 exit.
- **Expected artifacts (conceptual).** Synthetic end-to-end runbook
  (fixture sets + expected five-state outcomes); first golden/snapshot
  candidates (policy owned by `testing-seams.md`).
- **Required tests.** Integrated synthetic scenarios covering
  `PASS`/`FAIL`/`NOT_EVALUATED`/`NOT_APPLICABLE`/`ERROR` paths;
  cancellation and resource-exhaustion paths; tenant-mismatch path;
  output-semantic agreement across all four renderers.
- **Security gates.** All cross-cutting gates §§7–10 exercised
  integrated; secret-scanning of fixtures/artifacts.
- **Exit criteria.** The slice passes without real tenant access and
  without weakening any boundary; gaps found here recycle to their
  owning stage rather than being patched in the slice.
- **Deferred.** Real-tenant validation (see §17 — explicitly later and
  optional for core development).

### Stage 14 — Security/adversarial hardening

- **Objective.** Attempt to break the integrated slice adversarially and
  prove the invariants hold.
- **Primary project(s).** All.
- **Prerequisites.** Stage 13 exit.
- **Expected artifacts (conceptual).** Adversarial findings log with
  dispositions (fixed in owning stage or escalated to stop/go review
  §21); tightened validation/sanitization/encoding/bounding where the
  attempt log justifies it (additive/reversible preferred, §20).
- **Required tests.** False-PASS adversarial suite (missing-functionality
  probes, denial/gap/failure injection, unknown-completeness,
  cancellation, exhaustion, confirmed-empty confusion, capability
  unavailable/unknown); tenant-contamination probes; evidence-
  fabrication probes; injection payloads; failure-injection coverage
  (catalog owned by `testing-seams.md`).
- **Security gates.** False-PASS suite MUST pass; tenant-isolation suite
  MUST pass; secret-exclusion re-verified across logs/telemetry/outputs.
- **Exit criteria.** No adversarial probe produces incorrect `PASS`,
  cross-tenant leakage, fabricated evidence, or suppressed `ERROR`/
  `NOT_EVALUATED`; residual risks are recorded as TBDs with owners, not
  silently accepted.
- **Deferred.** Numeric bound finalization where validation is still
  pending (TBD §23, T-04).

### Stage 15 — CI/supply-chain/release hardening

- **Objective.** Introduce automation early enough to protect
  implementation evolution (without designing the workflow here).
- **Primary project(s).** Build/CI/release posture.
- **Prerequisites.** Stages 1, 13–14 (skeleton + slice + hardening inform
  what the automation MUST guard).
- **Expected capabilities (conceptual; exact tooling TBD §23, T-02).**
  Build/test; architecture checks; secret scanning; dependency
  review/scanning; pinned actions; least-privilege workflow permissions;
  artifact/release integrity controls — each where appropriate and
  already consistent with repository policy.
- **Required tests.** Automation self-verification (gates actually fail
  the pipeline on violation: broken build, failing architecture test,
  detected secret, unreviewed dependency).
- **Security gates.** Least-privilege workflow review; artifact-
  integrity review.
- **Exit criteria.** Every later change is guarded by the automation
  that Stages 1–14 justify; no workflow claims exceed what is
  implemented.
- **Deferred.** Exact workflow files, action pins, scanner selection,
  SBOM/signing mechanics (TBD §23, T-02).

### Stage 16 — Release-candidate validation

- **Objective.** Prove V1 readiness against the release-candidate gate
  (§19). This is a gate, not a build stage.
- **Primary project(s).** All.
- **Prerequisites.** Stages 1–15 exit.
- **Required evidence.** See §19 (architecture/contract match, required
  tests, adversarial suites, isolation/determinism suites, secret/security
  review, provider-isolation verification, output-semantics verification,
  documentation currency, TBD closure, claim restraint).
- **Exit criteria.** All §19 conditions met; otherwise the candidate is
  not a release candidate and returns to its owning stage.
- **Deferred.** Nothing — unresolved release-blocking TBDs MUST be closed
  here, not deferred.

---

## 6. Vertical-slice discipline

Broad infrastructure MUST NOT be implemented before the security
semantics can be exercised. The first synthetic end-to-end vertical
slice MUST exist at Stage 13 — and thin precursor slices SHOULD be
exercised earlier wherever cheap:

- After Stage 6: synthetic normalized input → graph → engine →
  findings, asserting five-state and evidence behavior with zero
  adapters.
- After Stage 7: orchestrated pipeline against faked adapters,
  asserting propagation before any real adapter exists.
- After Stage 10: output-neutral assembly consumed by a stub renderer,
  asserting semantic preservation before renderer polish.

The Stage 13 slice MUST NOT require real tenant credentials/data. It
uses synthetic fixtures and provider doubles only. A slice that needs a
live tenant to demonstrate value is a sequencing failure: it proves the
seams were built in the wrong order.

---

## 7. Core-first implementation

`Core` MUST be implementable and testable without any of the following:

- Microsoft Graph (no SDK presence, no network, no provider doubles on
  the evaluation path — synthetic normalized inputs only);
- network access of any kind;
- authentication secrets (no credential/token material in fixtures,
  factories, or assertions);
- filesystem (in-memory model construction and synthetic fixtures only);
- renderer (no formatting library, no template, no exit-code logic);
- real tenant (no production data, no live context, no captured payload
  unless sanitized through explicit review — default is fully
  synthetic).

Rationale: if `Core` cannot be built and proven provider-free, every
downstream stage inherits an unverifiable foundation. Stage 2–6 exit
criteria enforce this; Stage 13 proves the adapters add observation
without redefining semantics.

---

## 8. False-PASS gates

Before provider integration can be considered trustworthy, automated
tests MUST prove the following false-PASS scenarios (each: the condition
holds → the affected rule/run MUST NOT yield `PASS`):

1. Incomplete collection (partial completeness with the missing subset
   identified).
2. Authorization limitation (denial surfaced as explicit limitation,
   never as emptiness).
3. Provider failure (scoped, explicit failure — never silent success).
4. Unknown completeness (indeterminate observation — never assumed
   complete).
5. Cancellation (injected at collection, graph, evaluation, and
   rendering boundaries — recorded with scope, never a verdict).
6. Resource exhaustion (bound identified with scope — never `PASS` by
   truncation).
7. Confirmed-empty semantics (only an explicitly established complete-
   and-empty observation may support absence-based `PASS`; emptiness is
   never assumed from silence).
8. Capability unavailable/unknown (unsupported or indeterminate
   capability — handled per capability semantics, never auto-`FAIL` and
   never `PASS`).

No stage may treat missing functionality as `PASS`. The gate is enforced
at Stages 3, 6, 9, 13, and 14; Stage 16 requires the full adversarial
suite to pass.

---

## 9. Tenant-isolation gates

Synthetic cross-tenant contamination tests MUST pass before
provider-backed integrated assessment is trusted:

1. Tenant-context mismatch (observation, graph node, evidence, or result
   carrying a foreign tenant context) MUST fail safely and visibly as an
   integrity failure — never silently mixed, never coerced to a verdict.
2. Reused/cached contexts, pooled adapters, and shared fixtures MUST NOT
   leak tenant data across runs (run-to-run isolation proven with
   alternating-tenant synthetic scenarios).
3. Graph construction MUST reject cross-tenant edges; findings
   construction MUST reject cross-tenant evidence correlation; output
   assembly MUST reject cross-tenant artifact mixing.
4. No test in this gate uses production tenant data; all contamination
   probes are synthetic.

The gate is enforced at Stages 5, 7, 13, and 14; Stage 16 requires the
full isolation suite to pass.

---

## 10. Determinism gates

Repeatability tests for controlled normalized input, capability state,
provenance, configuration, and assessment-time reference are required:

1. Identical normalized inputs + capability state + rule
   version/configuration + deterministic context MUST produce identical
   semantic outcomes across repeated runs (presentation metadata such as
   report timestamps MAY differ; evaluation semantics MUST NOT).
2. Assessment-time-dependent policy is exercised only through the
   assessment-time reference seam; rule logic MUST NOT read wall-clock,
   randomness, or model inference.
3. Capability-state variation, provenance variation, and configuration
   variation each produce the explicitly specified outcome change — and
   only that change.
4. Rule-set/version selection is deterministic: the same selection
   resolves the same definitions in the same order with the same
   compatibility behavior.

The gate is enforced at Stages 6, 13, and 14; Stage 16 requires the
deterministic suite to pass.

---

## 11. Evidence/provenance gates

Evidence-backed `PASS`/`FAIL` and structured context for non-verdict
states are required before the rule-engine foundation (Stage 6) may be
declared complete:

1. Every `PASS` and every `FAIL` MUST reference traceable evidence
   (normalized facts observed for that assessment); missing or
   fabricated evidence MUST be rejected, never defaulted.
2. Every `NOT_EVALUATED`, `NOT_APPLICABLE`, and `ERROR` MUST carry
   structured reason/context (what was missing, limited, inapplicable,
   or failed — with scope).
3. Provenance (source system, object reference, collection
   operation/context, assessment context) MUST survive every
   transformation boundary and MUST NOT be fabricated or silently
   dropped.
4. Redaction obligations MUST hold: sensitive-but-non-secret context is
   redacted per schema before projection; secret-bearing values never
   enter the schema at all (§12 concepts; `authentication-design.md`).

The gate is enforced at Stages 4, 6, 10, 13, and 14.

---

## 12. Provider integration sequence

1. Microsoft Graph integration comes after inward contracts and test
   seams exist (Stages 2–8 before Stage 9). The adapter implements
   already-defined ports; it never defines domain semantics outward.
2. Provider implementation MUST be tested with synthetic/sanitized
   fixtures before any real-tenant validation: synthetic provider
   responses, pagination sequences, throttle/timeout signals,
   malformed responses, duplicate/conflicting observations —
   without inventing exact endpoints, permissions, SDK types, or payload
   schemas.
3. Exact endpoints, permissions, SDK types, property mappings, Agent
   Identity mappings, and licensing-behavior claims remain TBD until
   validated against published Microsoft documentation (TBD §23, T-04).
   No stage invents them to unblock itself; a stage blocked on provider
   truth raises a stop/go condition (§21) instead.
4. Provider-type isolation is verified continuously (Stages 9, 13, 14,
   16): no SDK/API/payload type in `Core` contracts, findings/evidence,
   rule evaluation, or output-neutral results
   (`dependency-boundaries.md` §11).

---

## 13. Authentication integration

1. Authentication implementation remains within the approved
   authentication/provider boundary (`authentication-design.md`).
   Orchestration carries the authorized-context reference without
   inspection; the adapter consumes it; the CLI wires it. `Core`,
   domain, graph, rules, findings, evidence, results, renderers, logs,
   telemetry, and fixtures never observe credential/token material.
2. No credentials/tokens in repository/tests/logs/evidence/output —
   without exception. Transient handling required by the future validated
   implementation at the proper boundary is the only permitted handling,
   and it is runtime-only, never persisted, logged, serialized, or
   rendered.
3. Exact flows remain TBD until reviewed (TBD §23, T-06). No stage
   selects OAuth/OIDC flows, mechanisms, caches, stores, or SDK auth
   classes to unblock itself.

---

## 14. Rule implementation sequence

A safe approach to implementing the first rules (contract/rule identity
first; provider-backed validation last):

1. **Contract/rule identity first.** Rule identity, versioning, and
   registration mechanics per `rule-engine-contracts.md` — before any
   rule content.
2. **Synthetic inputs.** Each candidate rule is specified against
   synthetic normalized inputs with explicit field/completeness needs.
3. **Explicit applicability.** The rule states when it applies and when
   it yields `NOT_APPLICABLE` — before evaluation logic.
4. **Explicit capability/completeness requirements.** The rule states the
   capability states and completeness conditions its `PASS` gating
   requires — before provider data exists.
5. **Evidence requirements.** The rule states the evidence its
   `PASS`/`FAIL` requires — before verdicts are trusted.
6. **Negative/adversarial tests.** False-`PASS` and false-`FAIL` probes
   for the rule (gap/denial/failure/unknown/cancellation/exhaustion/
   empty-confusion) — before provider-backed validation.
7. **Provider-backed validation only then.** Synthetic-provider-backed
   exercise first; real-tenant validation only under §17, if ever.

The final V1 rule catalogue is not invented here: no concrete security
rule content is assumed unless already approved in requirements/
architecture. Rule content remains subordinate to the pipeline and its
gates.

---

## 15. Output sequence

1. Presentation work MUST NOT outrun authoritative assessment semantics:
   Stages 10 (output-neutral assembly) before 12 (renderers); no
   renderer work begins until its input contract — the canonical output
   model — is stable.
2. Machine-readable/human-readable outputs consume output-neutral
   results. Renderers project; they never re-derive, mutate, suppress, or
   reinterpret state (§10 gate; `dependency-boundaries.md` §8).
3. SARIF mapping limitations MUST remain explicit until resolved (TBD
   §23, T-07): version/profile choice, field-mapping constraints, and
   any semantic gap between the five states and SARIF constructs are
   documented as limitations, not silently papered over. The same
   explicitness applies to JSON schema versioning and HTML
   templating/CSP strategy.

---

## 16. Configuration sequence

1. Effective deterministic configuration is implemented only after its
   authority/validation contracts are established
   (`configuration-design.md` with `rule-engine-contracts.md`): option
   catalog, secure defaults, explicit opt-in, unknown-option and
   invalid-configuration semantics.
2. `Core` consumes validated, normalized/effective configuration only;
   it never reads configuration sources. Loading shape lives in `Cli`;
   validation authority lives with the configuration contracts.
3. Unsafe fallback behavior MUST be tested: unknown options, invalid
   values, missing required selections, and insecure combinations MUST
   fail visibly — never silently default to broader collection, weaker
   `PASS` gating, suppressed diagnostics, or broadened privilege.

---

## 17. Testing sequence

Testing progression maps across stages as follows (scenario catalog
owned by `testing-seams.md`; no framework selected here):

| Test class | Primary stage(s) | Intent |
| --- | --- | --- |
| Unit tests | 2–6 (Core), 7 (orchestration units), 9 (adapter units), 12 (renderer units) | Isolated behavior of one contract surface from synthetic fixtures |
| Contract tests | 2–10 | Boundary shapes hold: ports, envelopes, capability/provenance carriage, output-neutral preservation |
| Architecture/dependency tests | 1 (skeleton) then every stage | DAG, forbidden-type absence, composition-root singularity, namespace rules |
| Provider adapter tests | 9, 13–14 | Synthetic-double coverage: pagination, resilience, partial/denial/malformed handling, leakage checks |
| Integration tests | 7 (faked), 13 (full synthetic slice) | Staged composition first with fakes, then full synthetic flow to all four artifacts |
| Deterministic/reproducibility tests | 6, 13–14, 16 | Repeatability on controlled inputs/capability/provenance/configuration/time reference |
| Adversarial/security tests | 14 (with drafts in 3, 6, 9) | False-PASS suite, tenant-contamination probes, fabrication probes, injection payloads, failure injection |
| Output semantic tests | 10, 12–13 | Five-state preservation per format, non-authority, injection/file-safety, redaction |
| Release-candidate tests | 16 | Full gate evidence per §19 |

Normal automated tests use synthetic data throughout. Real-tenant
validation, if performed later, follows §18.

---

## 18. Real-tenant validation boundary

Normal automated tests use synthetic data. Real-tenant validation, if
performed later, MUST:

- use authorized non-production/test tenant context only;
- use least privilege (minimum read-only access for enabled
  capabilities; no broadened consent for convenience);
- avoid repository-stored credentials (no committed secrets, sample
  credentials, or recorded tokens);
- avoid employer/customer data (no production tenant, no personal data
  beyond a sanitized test case, no tenant exports);
- follow documented security handling (sanitized failures, secret
  exclusion, provenance preservation, tenant-scope discipline).

Real tenant access is NOT required for core development (Stages 1–14).
A stage that cannot proceed without live-tenant data is either missing
a synthetic seam (fix the seam) or facing a stop/go condition (§21) —
never a justification for committing credentials or production data.

---

## 19. CI/CD and supply-chain sequence

CI/security automation SHOULD be introduced early enough to protect
implementation evolution (skeleton protection from Stage 1; full
hardening at Stage 15), but this document does not design the workflow.

Future controls SHOULD include, where appropriate and already consistent
with repository policy:

- build/test;
- architecture checks;
- secret scanning;
- dependency review/scanning;
- pinned actions;
- least-privilege workflow permissions;
- artifact/release integrity controls.

Exact tooling remains TBD (TBD §23, T-02). No workflow file, action pin,
scanner, SBOM mechanic, or signing implementation is selected here. CI
MUST NOT become a second composition root, a secret store, or a live-
tenant dependency: it runs synthetic suites with least privilege.

---

## 20. Release-candidate gate

V1 release candidate may be considered ready only when all of the
following hold with evidence:

1. Implementation matches locked architecture/contracts (no silent
   reinterpretation; change-controlled deviations only).
2. Required tests pass (unit, contract, architecture/dependency,
   provider-adapter, integration, output-semantic suites per §17).
3. False-PASS adversarial suite passes (§8).
4. Tenant-isolation tests pass (§9).
5. Deterministic tests pass (§10).
6. Secret scanning/security review passes (no credential/token material
   in code, tests, fixtures, logs, evidence, outputs, or history).
7. Provider isolation verified (no SDK/API/payload type in `Core`
   contracts, findings/evidence, rule evaluation, or output-neutral
   results).
8. Output semantics verified (all five states preserved per renderer;
   SARIF limitations explicit; injection/file-safety proven).
9. Documentation reflects actual implementation (contracts, TBD
   resolutions, and limitations current; no stale design claims).
10. Unresolved release-blocking TBDs closed (§23 triage complete).
11. No unsupported security/market claims. In particular, do not call the
    product enterprise-certified or 100% secure; assessment answers are
    per-rule verdicts with explicit capability/completeness bounds, never
    a global compliance or certification claim.

Failure on any item returns the candidate to its owning stage; the gate
is re-entered only with fresh evidence.

---

## 21. Rollback/checkpoint discipline

1. Implementation SHOULD use small reviewable checkpoints: one stage
   increment (or sub-increment) per review, with its tests and gate
   evidence attached.
2. Do not require destructive changes. Prefer additive/reversible
   evolution: additive contract extensions over breaking redefinitions;
   new adapters/renderers as additions behind stable ports; tightened
   validation as additive constraints.
3. If a destructive migration/change becomes necessary (breaking
   contract change, project-tree change, re-sequencing), it requires
   explicit review and recovery planning: exactly what would be removed
   or overwritten, why it is necessary, the expected impact, and how the
   previous state can be recovered — consistent with repository
   non-destructive policy. No force-push, history rewrite, or branch/tag
   deletion without explicit approval.

---

## 22. Stop/go gates

Implementation MUST stop for design review (not silently redesign
around the problem) when any of the following holds:

1. A required contract cannot represent observed provider behavior
   without violating an invariant or a dependency boundary.
2. Implementation would violate an invariant (including the DAG, five
   states, secret exclusion, tenant isolation, determinism, or
   non-authority rules).
3. Provider behavior contradicts an assumption the design relies on
   (capability, completeness, pagination, authorization, or data-shape
   assumption).
4. False-PASS protection cannot be maintained for a planned rule or
   capability path.
5. Tenant isolation cannot be proven for a planned composition or data
   flow.
6. Secret handling would cross an approved boundary.
7. An implementation-critical TBD requires an architectural choice
   (rather than a fill-in detail).
8. New V1 scope would be required (write/remediation, new provider,
   new host, AI verdicts, SaaS/multi-tenancy, or other non-goal).

Disposition of a stop/go trigger: record the conflict, return to the
owning requirements/architecture document for a reviewed change if
warranted (`README.md` §8), then update this package to match. Silent
workarounds in implementation are defects.

---

## 23. Post-V1 boundary

Post-V1 ideas remain separate from V1 sequencing. Do not implement as
part of V1 unless later explicitly approved through requirements/
architecture change:

- write/remediation (including credential rotation, permission/policy
  modification, identity lifecycle operations);
- autonomous AI security decisions (including AI-generated verdicts,
  AI-supplied evidence, or AI on the auth/collection/evaluation path);
- unapproved providers (beyond the single V1 Graph adapter);
- unapproved SaaS/multi-tenant architecture (including cross-tenant
  aggregation, hosted dashboards, or multi-user session posture);
- unrelated enterprise features (monitoring/alerting, governance-product
  replacement, or other NG-series non-goals).

Proposals in this class require an approved requirements/architecture
change first; this sequence reserves no capacity, ports, or projects
for them beyond the host-extensibility seam already established by
`solution-structure.md` §14.

---

## 24. Implementation TBDs

Unresolved implementation sequencing/tooling/package/provider-validation
decisions. None is resolved here by speculation.

| ID | Unknown | Why unresolved at this stage | Owner |
| --- | --- | --- | --- |
| T-01 | Target framework/monikers, build strictness (nullable/analysis), signing/packaging layout, solution/project mechanics. | Requires a dedicated build-control decision with supply-chain review; no version or setting is approved yet. | Future build-control work through `dependency-boundaries.md` (principles) and this document (ordering) |
| T-02 | CI workflow definition, action pins, workflow permissions, secret/dependency scanners, SBOM/signing tooling, coverage/release gates. | Requires a dedicated build/release design phase; inventing workflow detail here would pre-empt it. | Future CI/release design (not this document) |
| T-03 | Identifier formats, serialization schemas, namespace/folder names, snapshot/golden mechanics, hashing/signing/timestamping posture, graph size/memory policy, severity taxonomy, rule ordering/parallelism and registration/version mechanics. | Each needs its owning contract finalized first; numeric/schema choices require validation, not guessing. | Owning contracts (`core-contracts.md`, `domain-types.md`, `identity-graph-contracts.md`, `rule-engine-contracts.md`, `findings-evidence-schema.md`, `output-renderer-contracts.md`) with `testing-seams.md` for snapshot policy |
| T-04 | Numeric resource/timeout/concurrency/pagination bounds, retry/backoff constants, fatal-vs-isolated failure policy detail, cancellation mechanism choice. | Constants require validation and enforceability review; mechanism choices require implementation review. | `collector-contracts.md`, `identity-graph-contracts.md`, `rule-engine-contracts.md`, `error-result-model.md` (policies); asserted in tests once defined |
| T-05 | Final V1 rule content beyond contract/rule-identity scaffolding. | Rule content follows the pipeline and its gates; inventing rules here would pre-empt `rule-engine-contracts.md` and validation. | `rule-engine-contracts.md` with capability/evidence contracts |
| T-06 | Authentication flows, mechanisms, token-cache/storage policy, tenant-validation mechanics, consent UX, sovereign-cloud endpoints, permission rationale and names. | Requires dedicated auth review; flows and permissions MUST NOT be invented. | `authentication-design.md` (with `capability-model.md` for per-capability scoping) |
| T-07 | Canonical output projection shape detail, JSON schema/versioning, SARIF version/profile/mapping, HTML templating/CSP/asset strategy, CLI syntax/exit-code values, configuration-file format and option catalog, terminal/file-safety policies, redaction schema detail. | Each needs its owning contract with format/safety review; mapping limitations stay explicit until resolved. | `output-renderer-contracts.md` (with `configuration-design.md` for options) |
| T-08 | Test-framework/library selection, fixture-harness shapes, per-seam fakes/mocks detail, enforcement tooling for architecture tests. | Requires a dedicated tooling decision with supply-chain review; no framework is approved yet. | Future test-implementation decision; scenario catalog owned by `testing-seams.md`, rule set by `dependency-boundaries.md` |
| T-09 | Any Graph endpoint, permission/scope name, SDK type/method, property/relationship mapping, licensing-behavior claim, or Agent Identity mapping; per-category operational-failure-to-state table detail where provider truth is required. | Requires validation against published Microsoft documentation; invention is prohibited (INV-15). | `collector-contracts.md` / `capability-model.md` / `authentication-design.md` after documentation validation |
| T-10 | Which TBDs block which stages beyond the §5 per-stage lists (blocking vs deferrable triage at lock time). | Triage belongs to the Phase 0.3 lock review with all 16 documents present. | Phase 0.3 lock review (this document records the triage) |

---

## 25. Acceptance criteria

This document is complete for Phase 0.3.10 when:

1. The 16-stage sequence (§§4–5) is consistent with the project topology
   and dependency direction, with `Core`-first ordering and provider/
   presentation work last.
2. Every stage defines objective, primary project(s), prerequisites,
   conceptual artifacts, required tests, security gates, exit criteria,
   and deferred TBDs with no code written.
3. Vertical-slice discipline (§6), `Core`-first implementability (§7),
   false-PASS gates (§8), tenant-isolation gates (§9), determinism gates
   (§10), and evidence/provenance gates (§11) are all explicit with
   exactly five states and no sixth state.
4. Provider (§12), authentication (§13), rule (§14), output (§15),
   configuration (§16), testing (§17), real-tenant (§18), CI/supply-chain
   (§19), release-candidate (§20), rollback (§21), stop/go (§22), and
   post-V1 (§23) boundaries are explicit with no invented provider
   detail and no selected package/tooling.
5. Every implementation-sensitive unknown is listed in §24 as a TBD with
   a named owner.
6. `git diff --check` is clean and `git status --short` shows only the
   two authorized files modified.

---

## 26. Invariant and requirement traceability

| Concern | Traces to |
| --- | --- |
| Read-only V1, single-tenant CLI delivery | INV-01; INV-10; FR-040; NFR-001; NG-003–NG-004; CON-001 |
| Deterministic five-state evaluation, no sixth state | INV-02; INV-05; VERD-001–VERD-006 |
| Provider isolation, normalized boundary | INV-03; INV-04; INV-15 |
| Evidence/provenance, failure transparency | INV-06; INV-11; INV-14 |
| Capability awareness, false-PASS resistance | INV-07; CAP-001–CAP-004; threat-model paths A, C, J |
| Least privilege, secret exclusion | INV-08; INV-09; SEC-001; SEC-002; SEC-008; SEC-009 |
| Renderer/output non-authority | INV-12 |
| AI non-authority | INV-13; SEC-010; VERD-007; CON-004 |
| Dependency control, network restraint | CON-003; CON-005 |
| Testability, synthetic-first, no production data in tests | Testing-architecture §§1–2, 4–7, 18, 20; SEC-009; NFR-002 |

---

## Self-verification (Phase 0.3.10 authoring note)

- Only `docs/03-implementation-design/dependency-boundaries.md` and
  `docs/03-implementation-design/implementation-sequence.md` were
  modified in this phase task.
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
  introduced.
- No `git add/commit/push/reset/clean/checkout/switch` was performed.

(End of file)
