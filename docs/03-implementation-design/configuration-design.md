# Configuration Design

> **Phase:** 0.3.9
> **Package:** Implementation Design
> **Status:** Proposed — design only, no code or project files created
> **Date:** 2026-09-18

---

## 0. Status, scope, and precedence

- **Status:** Proposed implementation-level configuration architecture. This document is design output only. It authorizes no implementation.
- **Scope:** Defines what configuration may influence, configuration categories, ownership by architectural layer, loading/validation boundary, deterministic snapshot semantics, validation, defaults, rule/output/provider configuration, secret exclusion, tenant isolation, provenance, versioning, resource limits, testing implications, and explicit TBDs. It creates no configuration files, no code, no dependencies, and no concrete framework choices.
- **What this document is:** The owning document for configuration surfaces (assessment options, rule-set/version selection, capability selection, output options) per `docs/03-implementation-design/README.md` §5–§6. Other Phase 0.3 documents reference option semantics defined here; they MUST NOT redefine them.
- **What this document is not:** It is not a configuration-file format specification, a command-line syntax specification, a type/interface catalog (shapes belong to the respective contract documents coordinated by `core-contracts.md`), a Graph/permission binding, or a test catalog (scenarios belong to `testing-seams.md`).
- **Precedence:** Requirements (`docs/01-requirements/product-requirements.md`) and architecture (`docs/02-architecture/`, especially `architecture-invariants.md`, `rule-engine.md`, `collection-architecture.md`, `authentication-authorization.md`, `output-architecture.md`, `trust-boundaries.md`, `threat-model.md`, `testing-architecture.md`) are engineering truth. Where this document and an approved requirement or invariant appear to conflict, the requirement/invariant governs and the conflict is escalated through change control per `README.md` §8. This document MUST NOT relax any invariant.
- **Language discipline:** **MUST / MUST NOT** carry forward genuine constraints from requirements or architecture. **SHOULD** marks intended implementation flexibility. **TBD** marks an implementation-sensitive unknown resolved only by its named owning document, never by speculation.
- **Canonical assessment states:** Exactly five — **PASS**, **FAIL**, **NOT_EVALUATED**, **NOT_APPLICABLE**, **ERROR**. This document creates no additional state, alias state, or presentation state. Any reference to operational outcomes (e.g., configuration accepted/rejected, resource exhausted) is NOT an assessment state.

---

## 1. Purpose and configuration authority boundary

### 1.1 Purpose

Configuration parameterizes explicitly configurable assessment behavior within fixed architectural authority. It selects among supported behaviors, supplies deterministic rule parameters where a rule explicitly supports them, controls presentation preferences, and bounds resource usage. It never creates assessment authority.

### 1.2 What configuration may influence

Configuration MAY, only where an owning contract explicitly supports it:

- select the active rule set and rule versions under evaluation;
- select enabled capabilities/collection categories for the run;
- supply deterministic thresholds or parameters consumed by a rule that explicitly declares them;
- supply output/rendering preferences (projection choices, destinations requested, verbosity within semantic-preserving bounds);
- supply execution and resource bounds (cancellation, size/count limits) within validated ranges;
- supply provider-facing non-secret operational settings consumed exclusively outside Core (see §10);
- supply feature/capability toggles where architecture explicitly permits optional behavior.

Each influence MUST be traceable to an explicitly configurable surface defined by its owning contract (`rule-engine-contracts.md` for rule configuration, `capability-model.md` for capability selection, `collector-contracts.md` for collection operational settings, `output-renderer-contracts.md` for output options, `authentication-design.md` for auth-boundary settings).

### 1.3 What configuration MUST NEVER control

Configuration MUST NOT:

1. redefine the canonical assessment states (**PASS**, **FAIL**, **NOT_EVALUATED**, **NOT_APPLICABLE**, **ERROR**) or introduce any sixth state, alias, or renderer-specific verdict (INV-05);
2. bypass capability requirements — unavailable capability MUST still flow through capability validation into **NOT_EVALUATED** (or **NOT_APPLICABLE**/**ERROR** per rule semantics), never into **PASS** (INV-07);
3. bypass collection-completeness requirements — configuration MUST NOT declare incomplete collection complete, and MUST NOT permit "nothing found" to mean **PASS** unless the rule explicitly establishes completeness (INV-02; rule-engine §4–§5);
4. suppress systemic failures into **PASS** — collection, normalization, graph, rule-execution, serialization, or resource-exhaustion failures MUST remain visible as **ERROR**, **NOT_EVALUATED**, or explicit diagnostics (INV-14);
5. convert **NOT_EVALUATED** / **ERROR** into **PASS** (or into **FAIL** merely for reporting convenience) (INV-05; INV-14);
6. disable tenant isolation or permit cross-tenant influence (INV-10);
7. weaken evidence/provenance requirements — **PASS**/**FAIL** still require traceable evidence references; non-verdict states still require structured reason/context (INV-06; INV-11);
8. grant provider authority to Core — configuration MUST NOT give rule logic provider-query capability, ambient network access, or SDK references (INV-03; INV-04);
9. grant renderers assessment authority — output configuration MUST NOT permit renderers to re-evaluate rules, alter states, or fabricate evidence (INV-12);
10. grant AI/LLM components assessment authority — configuration MUST NOT place model output on the evaluation, authorization, collection, or capability path, and MUST NOT permit model output to determine or override any of the five states (INV-13).

A configuration surface that purports to authorize any of the above is invalid and MUST be rejected visibly (see §6). Where an existing contract does not explicitly permit a configuration influence, the influence is NOT permitted.

### 1.4 Authority preservation rule

Assessment authority remains where architecture places it: deterministic rule evaluation in Core (`rule-engine-contracts.md`), evidence/provenance construction per `findings-evidence-schema.md`, orchestration without verdict authority in Application, collection in the provider adapter, projection-only rendering in Output. Configuration is input to authority; it is never authority itself.

---

## 2. Configuration categories

Categories are conceptual groupings. They do not imply separate files, stores, or concrete mechanisms (see §4 and §17). Security-critical categories and presentation-only categories MUST remain clearly separated so a presentation preference can never weaken a security invariant.

| # | Category | Character | Examples of intent (non-exhaustive, conceptual) |
| --- | --- | --- | --- |
| C-01 | Assessment configuration | Security-critical | Active rule-set/version selection; assessment-time reference where deterministic time-dependent policy is supported; severity handling posture (metadata only, never verdict-altering); assessment-context scoping including tenant scope reference. |
| C-02 | Rule configuration | Security-critical | Per-rule deterministic parameters where the rule definition explicitly declares them; rule enablement/disablement within supported policy; rule-version pinning. |
| C-03 | Deterministic thresholds / parameters | Security-critical, conditional | Numeric/boolean/enumerated inputs consumed only by rules that explicitly declare them (e.g., a rule-declared bound). No implicit global thresholds. No invented values in this document. |
| C-04 | Output / rendering preferences | Presentation-only | Format selection among V1 outputs (CLI/text, JSON, SARIF, HTML); projection verbosity within semantic-preserving bounds; destination requests; redaction-level requests within the redaction policy owned by output contracts. MUST NOT alter verdicts, evidence, or completeness (see §9). |
| C-05 | Execution / resource limits | Security-critical (fail-safe) | Cancellation behavior; collection paging/window bounds; evaluation/graph traversal bounds; rendering size bounds. MUST be explicit, validated, bounded, and fail-visible (see §15). |
| C-06 | Provider-facing non-secret operational configuration | Security-critical, outside Core | Non-secret adapter behavior such as bounded retry/backoff posture, paging behavior, timeout posture, and collection-window semantics — consumed exclusively by the provider adapter through ports owned by `collector-contracts.md`. MUST NOT enter Core rule signatures (see §10). |
| C-07 | Feature / capability configuration | Security-critical, conditional | Enablement of explicitly optional capabilities where architecture permits opt-in. MUST NOT silently broaden privilege requirements, assume uniform tenant capabilities, or enable unsupported capabilities (see INV-07; INV-08). |

Rules:

- C-01, C-02, C-03, C-05, C-06, C-07 are NEVER presentation-only. Their invalid/conflicting/missing handling MUST follow §6 fail-visible semantics.
- C-04 is NEVER security-critical. It MUST NOT be able to change C-01–C-03 semantics, suppress C-05 enforcement, or reinterpret capability/completeness.
- No category carries secret-bearing values (see §11).
- No category carries cross-tenant directives (see §12).

---

## 3. Configuration ownership

Ownership follows the dependency direction in `solution-structure.md` §§7–§9: Core depends on nothing outward; adapters and host depend inward.

| Category | Owning layer (conceptual) | Boundary notes |
| --- | --- | --- |
| Assessment configuration (C-01) | Application orchestration consumes; Core defines the inward-facing contract it will accept; CLI composes and supplies. | Core owns acceptance/validation semantics for what reaches deterministic evaluation (via `rule-engine-contracts.md` + `core-contracts.md`). Application owns pipeline coordination (which stages receive which configuration). CLI as V1 composition root owns sourcing/loading (see §4). |
| Rule configuration (C-02, C-03) | Rule definitions (Core) declare supported parameters; Core validates; Application routes; CLI supplies. | Rules MUST NOT read ambient configuration. All rule configuration arrives as explicit deterministic input through the rule-engine contract (see §8). Provider-specific rule settings do not exist inside Core. |
| Output / rendering preferences (C-04) | Output layer owns renderer-option semantics (`output-renderer-contracts.md`); Application routes options to renderers; CLI supplies operator preferences. | Renderer options MUST NOT be visible to rule evaluation. Assessment contracts MUST NOT depend on output options. |
| Execution / resource limits (C-05) | Each layer owns enforcement of bounds applicable to its stage (collection bounds in adapter, evaluation bounds in Core engine contract, rendering bounds in Output); Application propagates cancellation; CLI supplies operator-requested bounds within validated ranges. | Bounds are enforced where the resource is consumed, not only where configuration is loaded. |
| Provider-facing operational configuration (C-06) | Provider adapter owns consumption (`collector-contracts.md` + `authentication-design.md` boundary); Application supplies through ports; Core MUST NOT observe it. | Core independence from provider-specific configuration is structural: Core contracts MUST NOT reference provider settings, SDK settings, or auth flows. |
| Feature / capability configuration (C-07) | Capability model (`capability-model.md`) owns state vocabulary and granularity; Application orchestrates detection/selection; adapter reports actual capability; CLI requests desired scope. | Requested capabilities MUST NOT be confused with achieved capabilities. Only detected capability state enters evaluation. |

Preserved boundaries:

- **Core independence:** No provider-specific, renderer-specific, host-specific, or authentication-implementation configuration type appears in Core contracts.
- **Application orchestration boundary:** Application routes validated configuration; it MUST NOT reinterpret rule semantics, fabricate capability state, or suppress validation failures.
- **Provider-specific configuration outside Core:** C-06 never crosses the normalization boundary into rule signatures.
- **Renderer-specific configuration outside assessment authority:** C-04 never reaches rule evaluation or findings construction.
- **CLI as V1 composition root:** Only `EntraNHI.Cli` wires configuration sources to ports (per `solution-structure.md` §10). No other project composes the object graph. A future host would provide its own composition root under the same constraints.

---

## 4. Configuration loading boundary

Conceptual flow only. No concrete configuration framework, store, or library is selected in this document (see §17). The flow MUST be implementable without giving Core I/O, ambient-state, or provider access.

Conceptual stages:

1. **Source acquisition (outside Core).** The composition root acquires raw configuration from operator-supplied sources. This document does not enumerate which source kinds are supported.
2. **Parsing into candidate structure.** Raw source material is parsed into a candidate configuration structure without applying assessment semantics. Parse failures are validation failures (see §6).
3. **Shape validation.** Candidate structure is checked against the configuration contract shape owned by this document in coordination with `core-contracts.md` (required/optional surfaces, types of values at the contract level, no concrete field list invented here beyond categories in §2).
4. **Semantic validation.** Values are checked for meaning: supported rule-set/version references, supported capability requests, rule-declared parameter applicability, output-option support, bound coherence (see §6).
5. **Normalization into inward-facing contracts.** Validated configuration is normalized into the inward-facing configuration contracts consumed by Application/Core/Output/adapter ports. Normalization MUST NOT silently substitute a less restrictive interpretation for an invalid value.
6. **Snapshot capture.** The normalized configuration is captured as the immutable/effectively immutable configuration snapshot for the run (see §5) and its non-secret effective content recorded for provenance (see §13).
7. **Distribution.** Application routes each snapshot slice only to the layer contract that owns it (rule slices to Core engine, output slices to renderers, provider slices to adapter). No layer receives another layer's configuration beyond what its contract requires.
8. **Visible rejection.** Any failure in stages 2–5 MUST fail visibly before assessment proceeds (or before the affected scope proceeds, per fatal-vs-isolated policy owned by `rule-engine-contracts.md` / `error-result-model.md`). Silent fallback to defaults that weaken protections is prohibited (see §6–§7).

Constraints:

- Configuration MUST be loaded and validated outside Core. Core receives only the validated snapshot slice through its engine contract; Core performs no source I/O and reads no ambient state for configuration (see §8).
- Authentication secret/token handling is NOT part of this flow. It remains exclusively within the authentication/provider boundary owned by `authentication-design.md` (see §11).
- This document MUST NOT be read as selecting environment variables, files, command-line parsing libraries, `IConfiguration`, YAML, registry, secrets managers, or any other concrete mechanism. Those selections, if ever made, belong to later detailed design constrained by this document.

---

## 5. Deterministic configuration snapshot

### 5.1 Concept

The **effective configuration snapshot** is the normalized, validated, immutable (or effectively immutable) configuration actually consumed during an assessment run. It is captured once per run before the stages it governs begin, and it MUST NOT mutate mid-assessment in a way that silently changes verdict behavior.

### 5.2 Determinism requirement

The same **normalized inputs + capability state + provenance + assessment-time reference + effective configuration snapshot + rule definitions/versions** MUST produce deterministic assessment semantics (INV-02; rule-engine §9). Two runs that differ only in presentation metadata or in C-04 output preferences MUST produce the same authoritative `RuleEvaluation` outcomes.

Consequences:

- Rule evaluation MUST observe only the snapshot, never a live or re-read configuration source.
- No mid-assessment configuration mutation, reload, or ambient override may silently change verdict behavior. If a later phase ever supports reconfiguration, it MUST define a new assessment run with a new snapshot and new provenance, not a silent in-place change.
- Assessment-time reference, where supported, is part of the deterministic input set alongside the snapshot; wall-clock reads inside rule logic remain prohibited (rule-engine §9.3).
- Ordering effects MUST be deterministic: rule execution order, collection ordering where observable, and aggregation order MUST NOT depend on configuration-source iteration order, filesystem enumeration order, or other unstable ambient ordering unless normalized into a stable order first.

### 5.3 Snapshot scope

- The snapshot covers all of C-01–C-03 and the security-relevant portions of C-05/C-07 actually consumed by assessment semantics, plus the C-04/C-06 slices necessary for provenance (see §13).
- Raw source dumps (e.g., verbatim file contents, ambient process state) are NOT part of the snapshot and MUST NOT be recorded where they could leak secrets or unstable state.
- Per-rule slices of the snapshot SHOULD be attributable to the rule(s) they affected, so a finding can be traced to the configuration that parameterized it without exposing unrelated configuration.

### 5.4 Mid-assessment mutation prohibition

- Configuration objects handed to Core evaluation MUST be immutable or treated as effectively immutable for the duration of the run (no public mutators on the evaluation path; no global/static mutable configuration).
- Application orchestration MUST NOT refresh, merge, or overlay configuration between pipeline stages (collect → normalize → graph → evaluate) unless the overlay produces a new run identity with new provenance.
- Cancellation/resource-bound enforcement (C-05) observes runtime signals (cancellation requests, exhaustion events) but MUST NOT reinterpret those signals as configuration changes that alter rule predicates. Exhaustion maps to visible failure states per §15, never to silent predicate changes.

---

## 6. Validation semantics

Validation is fail-visible. Invalid security-relevant configuration MUST fail visibly and MUST NEVER silently fall back to a less restrictive interpretation that could produce false **PASS**.

### 6.1 Schema / shape validation

- Candidate configuration MUST conform to the contract shape: known surfaces, expected value kinds, required vs optional presence, nesting/ownership (which slice belongs to which layer).
- Malformed structure (unparseable material, wrong value kinds at the contract level, missing required enclosing surfaces) MUST be rejected with a structured diagnostic identifying the offending surface. The diagnostic MUST NOT include secret material (see §11).
- Shape validation occurs before any assessment stage consumes the configuration.

### 6.2 Semantic validation

- References MUST resolve: rule-set/version references MUST name supported, compatible rule definitions; capability requests MUST name supported capability categories per `capability-model.md`; rule parameters MUST correspond to parameters the target rule explicitly declares.
- Values MUST be meaningful in context: a parameter for a disabled or inapplicable rule, a capability request outside V1 scope, or an output option for an unsupported renderer MUST be handled per §§6.4–6.5, never by silent reinterpretation.
- Semantic validation MUST NOT consult provider state, network state, or AI judgment. It validates the request; capability detection later establishes what was actually achieved.

### 6.3 Range / bound validation

- Bounded values (counts, sizes, durations/timeouts postures, retry postures where supported) MUST be checked against explicit minima/maxima owned by the enforcing layer's contract. Unbounded, negative-where-nonsensical, or self-contradictory bounds (e.g., a limit that guarantees immediate exhaustion) MUST be rejected.
- No numeric bound is invented in this document. Concrete minima/maxima are TBDs owned by the enforcing contract (`collector-contracts.md` for collection bounds, `rule-engine-contracts.md` for evaluation bounds, `output-renderer-contracts.md` for rendering bounds) — see §17.
- Range validation MUST consider composition: individually valid bounds that are jointly incoherent (e.g., a per-item bound exceeding the total bound in a way the contract forbids) MUST be rejected or normalized only into a strictly more restrictive, explicitly documented posture — never into a permissive one.

### 6.4 Unsupported configuration handling

- A request for an unsupported rule, rule version, capability, output format/option, or resource posture MUST NOT silently enable a substitute, silently broaden scope, or silently proceed as if supported.
- Required-scope unsupported requests (e.g., assessment requires a capability the tool does not support) MUST fail visibly before evaluation claims completeness.
- Optional-scope unsupported requests MUST either fail visibly or be explicitly reported as not applied with a diagnostic, per the owning contract's stated policy — and MUST NEVER be reported as applied. Silently enabling an unsupported capability is prohibited (INV-07; INV-16).

### 6.5 Conflicting configuration handling

- Direct conflicts (e.g., rule enabled and its required capability disabled; output requests that contradict redaction policy; bounds that contradict each other) MUST be rejected visibly or resolved only toward the strictly more restrictive, explicitly documented interpretation, with the resolution recorded in effective-configuration provenance (see §13).
- Priority/override rules between configuration contributors, if ever supported, MUST be explicit, deterministic, and documented. Implicit "last writer wins" from unstable source ordering is prohibited.
- Any conflict whose safe resolution is ambiguous MUST fail visibly rather than guess.

### 6.6 Missing required configuration

- Absence of configuration required for a deterministic outcome (e.g., active rule-set/version reference where the engine contract requires it; tenant scope where single-tenant execution requires it) MUST fail visibly. The run MUST NOT proceed as if a secure default existed where none is defined.
- Absence of optional configuration MUST resolve only to the explicit default defined for that surface (see §7), or remain absent where the contract permits absence. Absence MUST NOT be treated as enablement of optional capabilities.

### 6.7 Unknown configuration where relevant

- Unknown settings/keys/options MUST NOT silently change security semantics (rule-engine §16.1).
- Security-critical surfaces MUST reject unknown settings visibly or explicitly report them as ignored with a diagnostic while proving the ignore cannot weaken protections — per the owning contract's stated unknown-handling policy. Silent ignore on a security-critical surface is prohibited.
- Presentation-only surfaces (C-04) MAY ignore unknown options with an explicit diagnostic where the output contract permits it, provided the ignore is recorded and cannot affect assessment semantics. The output contract MUST state which behavior applies; this document does not choose it.

---

## 7. Defaults

### 7.1 Safe-default principles (INV-16)

Defaults MUST:

1. be **explicit** — each default is stated by the owning contract, recorded in effective-configuration provenance, and distinguishable from operator-supplied values;
2. be **deterministic** — the same absent input always yields the same default; defaults MUST NOT depend on ambient state, tenant-discovered state, wall-clock values, or unstable ordering;
3. **not weaken false-PASS protections** — no default may declare completeness, assume capability, suppress a failure, or convert **NOT_EVALUATED**/**ERROR** toward **PASS**;
4. **not silently enable unsupported capabilities** — optional capabilities default to disabled/not-requested unless architecture explicitly states otherwise with rationale;
5. **not fabricate completeness** — "collect everything by default" MUST NOT be claimed where the adapter cannot establish completeness; completeness is established per-rule/per-collection by evidence, not by default posture;
6. **not bypass evidence requirements** — defaults MUST NOT reduce **PASS**/**FAIL** evidence obligations or non-verdict reason obligations.

### 7.2 What this document does not do

- This document invents NO concrete default numeric values, NO default rule enablement list, NO default capability set, NO default output selection, and NO default bound. Each concrete default is a TBD owned by its enforcing contract (see §17).
- In particular, this document does NOT state which rules are enabled by default, which output format is emitted by default, or what any resource bound defaults to. Stating those here would be invention.

### 7.3 Default vs fallback distinction

- A **default** is the documented value/behavior for absent optional configuration, applied openly and recorded as defaulted in provenance.
- A **fallback** is a runtime substitution for invalid, conflicting, or failed configuration. Fallbacks that move toward a less restrictive posture (broader scope, weaker validation, suppressed failure, assumed completeness) are prohibited. The only permissible fallback direction is toward explicit visible failure or toward a strictly more restrictive posture explicitly permitted by the owning contract and recorded as such.

---

## 8. Rule configuration

### 8.1 Delivery path

Rule-specific configuration reaches deterministic rule evaluation ONLY through the inward-facing rule-engine contract owned by `rule-engine-contracts.md` (in coordination with `core-contracts.md`):

- The CLI composition root supplies candidate rule configuration.
- Application validates, normalizes, and routes it as part of the snapshot (see §§4–5).
- The rule engine validates rule-definition compatibility/version (rule-engine §10.2) and required-capability/input preconditions (§§10.4–10.5) before any evaluation predicate observes the configuration.
- The evaluation predicate observes ONLY the validated per-rule snapshot slice plus normalized inputs, graph projection, capability states, provenance references, assessment context, and rule definition/version metadata. No other channel exists.

### 8.2 Rule prohibitions

Rules MUST NOT:

1. read ambient process configuration directly (no process-wide lookups, no static/global configuration access on the evaluation path);
2. read environment state directly (no wall-clock, no locale/ordering-sensitive ambient reads except through the deterministic assessment-time reference where supported);
3. perform provider or network lookups for configuration (no Graph/ARM calls, no privilege requests, no live capability probing from inside rule logic — capability state arrives as input);
4. mutate global configuration (no static mutation, no snapshot mutation, no cross-rule side channels through configuration);
5. reinterpret invalid configuration as **PASS** (invalid rule configuration MUST surface per `error-result-model.md` mapping as **ERROR** or **NOT_EVALUATED** with structured reason, never as **PASS**; see rule-engine §§4, 10.2).

### 8.3 Rule-declared parameters only

- A rule consumes ONLY parameters its `RuleDefinition` explicitly declares (rule-engine §7). Undeclared parameters addressed to a rule MUST be rejected or explicitly ignored with a diagnostic per §6.7 — never injected into evaluation.
- Parameter interpretation is owned by the declaring rule's documented semantics. Two rules MUST NOT share mutable parameter state. A parameter for one rule MUST NOT alter another rule's predicate.
- Severity metadata MUST NOT be configurable into verdict logic. Severity flows to findings/output only (rule-engine §17).

### 8.4 Invalid rule-definition/configuration mapping

- Incompatible rule version, malformed rule definition, or invalid per-rule configuration MUST surface as engine-level failure mapped per `error-result-model.md` (normally **ERROR** for attempted-but-untrustworthy evaluation, or **NOT_EVALUATED** where the rule-engine contract specifies that unavailable-input behavior applies). The mapping MUST be explicit in `rule-engine-contracts.md`; this document does not choose it.
- Disabled rules MUST remain diagnosable in assessment metadata where the rule-engine contract requires it (rule-engine §16.1), so omission is auditable rather than silent.

---

## 9. Output configuration

### 9.1 Separation principle (INV-12)

Presentation preferences (C-04) control **representation** of authoritative results. They MUST NOT control **assessment semantics**. The authoritative `RuleEvaluation`/finding data defined by `rule-engine-contracts.md` and `findings-evidence-schema.md` is complete before rendering begins; output configuration only parameterizes its projection via `output-renderer-contracts.md`.

### 9.2 What output configuration MAY control

Only where `output-renderer-contracts.md` explicitly supports it:

- which V1 formats to emit (CLI/text, JSON, SARIF, HTML);
- projection verbosity/detail depth within semantic-preserving bounds (e.g., whether optional evidence-detail levels are included, never whether required evidence is omitted);
- destination requests (console, operator-configured file destinations) subject to path-safety and safe-write policy;
- redaction-level requests within the redaction policy (redaction MAY withhold display detail but MUST indicate withholding and MUST NOT alter states — output-architecture §14).

### 9.3 What output configuration MUST NOT do

Output configuration MUST NOT:

1. change verdicts (**PASS**/**FAIL**/**NOT_EVALUATED**/**NOT_APPLICABLE**/**ERROR** survive rendering exactly);
2. remove required semantic information (rule identity/version, states, findings, required evidence/provenance references, capability/completeness context, errors/failures required by contracts);
3. fabricate evidence or provenance;
4. reinterpret completeness (a partial/failed assessment MUST NOT be presentable as complete through configuration);
5. convert failure states (no collapsing **ERROR**/**NOT_EVALUATED**/**NOT_APPLICABLE** into success/failure appearances, no severity-into-state reinterpretation);
6. alter rule applicability (output options MUST NOT add, remove, or re-scope evaluated rules).

A renderer option that purports to do any of the above is invalid. Verbosity options that would hide **NOT_EVALUATED**/**ERROR**/partial conditions for visual convenience are prohibited (output-architecture §§3–5).

---

## 10. Provider configuration

### 10.1 Boundary

Provider-facing configuration (C-06) remains outside Core. It is consumed exclusively by the provider adapter (`EntraNHI.Infrastructure.Graph`) through collector/capability ports owned by `collector-contracts.md`, with authorization-boundary aspects owned by `authentication-design.md`. Core rule signatures, graph contracts, findings schemas, and output-model contracts MUST NOT reference provider settings.

### 10.2 Non-invention rule (INV-15)

This document invents NO exact Graph endpoints, permissions, scopes, SDK settings, authentication flows, tenant properties, licensing mappings, or Agent Identity mappings. It also invents no property names, relationship names, endpoint behaviors, or support/limitation claims about Microsoft tenants. All such content is TBD owned by `collector-contracts.md` (endpoints/properties/mappings after documentation validation), `capability-model.md` (capability granularity), and `authentication-design.md` (permission rationale) per `solution-structure.md` §20/T-04.

Accordingly, this document describes provider-facing configuration ONLY as abstract operational posture (bounded retry/backoff posture, paging behavior, collection-window semantics, cancellation posture) without stating mechanisms, constants, or mappings. Concrete settings, if ever defined, MUST be validated against published Microsoft documentation and MUST NOT assume undocumented behavior; where documentation is absent, affected assessment MUST yield **NOT_EVALUATED** per INV-15, never **PASS**.

### 10.3 Non-secret constraint

C-06 MUST NOT contain secret-bearing values. Authentication secret/token handling (including any cache/storage posture) belongs exclusively to the authentication/provider boundary and MUST NOT be designed into general product configuration (see §11).

### 10.4 No privilege broadening through configuration

Optional-capability configuration MUST NOT silently broaden requested privilege (INV-08). Enabling an optional collection category MUST either stay within already-authorized scope or fail visibly with an authorization-denial path that yields **NOT_EVALUATED**/**ERROR** rather than **PASS**. The permission rationale for any capability-gated collection belongs to `authentication-design.md`.

---

## 11. Secret exclusion

### 11.1 Excluded values (INV-09)

Configuration contracts used by Core/Application assessment semantics MUST NOT contain:

- passwords;
- client secrets;
- access tokens;
- refresh tokens;
- private keys;
- certificate private material;
- recovery codes/secrets;
- authorization headers/cookies carrying authentication material;
- any other secret-bearing value.

This prohibition covers configuration snapshots, effective-configuration provenance records, evidence, findings, outputs, logs, diagnostics, and synthetic test fixtures. Credential **metadata** explicitly classified as non-secret by the owning contract (e.g., a non-sensitive identifier type name, never the value) MAY appear only where explicitly permitted — this document permits none by itself.

### 11.2 Authentication-boundary exclusivity

Authentication secret/token handling — acquisition, transient in-memory use, cache/storage posture, redaction in diagnostics — remains exclusively within the authentication/provider boundary owned by `authentication-design.md` and consumed by the provider adapter. General product configuration MUST NOT duplicate, mirror, persist, log, or serialize that material.

Transient handling required by the validated authentication implementation is NOT prohibited by this document, but such transients MUST NEVER enter the configuration snapshot, normalized domain records, graph, findings, evidence, provenance, or renderer input.

### 11.3 Snapshot / evidence / log hygiene

- Effective-configuration provenance (see §13) records WHICH non-secret settings affected the run, never secret values. There is no "redacted secret" field in configuration provenance — secrets are absent, not masked.
- Diagnostics for configuration validation failures MUST identify the offending surface without echoing secret-bearing content. A validator that echoes raw authentication material into an error is defective.
- Sample, example, and test configuration MUST use sanitized placeholders and synthetic non-secret values. Real production credentials, tenant exports, and employer/customer confidential values MUST NOT appear in documentation, fixtures, or tests (per `AGENTS.md` / `SECURITY.md`).

---

## 12. Tenant isolation

### 12.1 Single-tenant scoping (INV-10)

V1 executes one tenant assessment context per run (NG-003; `solution-structure.md` §2). Configuration MUST be explicitly tenant-scoped where tenant-specific behavior is supported: the effective snapshot is bound to exactly one tenant assessment context, and that binding is recorded in provenance (see §13).

### 12.2 Contamination prohibition

- Configuration from one tenant's run MUST NEVER influence another tenant's assessment. No shared mutable configuration, no cross-run caching of tenant-derived settings, no ambient configuration channel that leaks tenant-specific choices across runs.
- Tenant-context mismatch or contamination — including configuration bound to a different tenant scope than the collected data — is an integrity failure. It MUST fail safely and visibly (no **PASS**, no silent continuation), with the mismatch surfaced through the shared error/result vocabulary owned by `error-result-model.md`.
- Cross-tenant aggregation, comparison, or combined output is outside V1. Configuration MUST NOT provide a multi-tenant or cross-tenant correlation mode. Any future aggregation contract would require separate architecture; it MUST NOT be assumable through configuration.

### 12.3 Isolation through the configuration flow

- The composition root MUST establish the tenant scope before snapshot capture; stages after capture MUST verify scope carriage rather than re-derive it from ambient state.
- Per-tenant configuration overlays, if ever supported, MUST be explicit, validated per §6, captured in the snapshot, and proven to not leak across scopes. Implicit per-tenant file discovery (e.g., silently merging tenant-named sources) is prohibited.
- Test fixtures MUST prove isolation with synthetic tenant identifiers (see §16); real tenant identifiers MUST NOT be used.

---

## 13. Configuration provenance / effective configuration

### 13.1 Purpose

Sufficient non-secret provenance MUST exist to explain which configuration affected an assessment, so an auditor can reproduce deterministic semantics and understand why a rule was enabled, parameterized, or reported in a given way — without exposing secrets or unstable ambient state.

### 13.2 Required effective-configuration content (conceptual, non-secret only)

The assessment record SHOULD reference or contain:

- active rule-set / rule-version selection actually consumed (IDs + versions, not just requested names);
- per-rule parameter values actually consumed, attributed to the declaring rule;
- capability/collection scope requested vs capability state actually achieved (requested scope does not imply achieved state);
- output/redaction posture actually applied, where it affects interpretation (e.g., that detail was withheld under redaction policy);
- resource bounds actually enforced where exhaustion or truncation is possible;
- tenant-scope binding of the snapshot;
- assessment-time reference where deterministic time-dependent policy is supported;
- tool/version reference sufficient to interpret the above.

Provenance MUST distinguish operator-supplied values from applied defaults (see §7): a defaulted value is recorded as defaulted, not presented as an operator choice.

### 13.3 Prohibited content

- NO secret-bearing values (see §11). Provenance contains no secrets, masked or otherwise.
- NO raw configuration-source dumps (verbatim file contents, ambient process listings, unvalidated source text). Provenance records the normalized effective snapshot, not the raw sources.
- NO unstable ambient state (load-time timestamps that affect meaning, source-iteration order, machine-specific paths) except where explicitly classified as non-semantic presentation metadata separated from assessment meaning.

### 13.4 Preservation

Effective-configuration provenance travels with the canonical assessment result into output projection (consumed by `output-renderer-contracts.md`) so CLI/JSON/SARIF/HTML can represent it within their semantic-preservation obligations. Renderers project it; they MUST NOT alter it (see §9 and `output-renderer-contracts.md` §16).

---

## 14. Versioning and compatibility

Contract-level expectations only. No concrete versioning format, numbering scheme, or migration tooling is chosen in this document (see §17).

### 14.1 Configuration schema evolution

- Configuration contract evolution SHOULD be additive and reversible (per `README.md` §8): new optional surfaces are preferred over breaking redefinitions. A breaking change requires explicit justification, impact statement, and recovery path before approval.
- Each assessment record MUST be interpretable against the configuration contract revision that produced it, via the effective-configuration provenance (see §13) plus tool/version references. Reinterpreting a historical record under a newer contract without explicit migration semantics is prohibited.

### 14.2 Unknown future settings

- Future settings unknown to the current tool MUST be handled per §6.7: security-critical surfaces fail visibly or explicitly report non-application; presentation surfaces follow the output contract's stated policy. In no case may an unknown future setting silently weaken protections or change the meaning of the five states.
- Forward-compatibility posture (whether newer configuration is accepted, rejected, or partially applied by older tool versions) MUST be explicit in the owning contract. Silent reinterpretation of unknown fields is prohibited.

### 14.3 Backward compatibility

- Older configuration SHOULD remain interpretable where the evolution is purely additive, with absent new surfaces resolving to their explicit defaults (see §7).
- Where older configuration requests behavior removed or redefined by a newer contract, the tool MUST fail visibly or map to the explicitly documented successor behavior — never silently substitute a permissive interpretation.

### 14.4 Rule configuration evolution

- Rule parameter declarations evolve with rule versions: a parameter added, removed, or redefined is a rule-version concern owned by `rule-engine-contracts.md` (§§7–8). Configuration referencing a retired parameter or a retired rule version MUST be handled per §§6.4–6.5 (visible failure or explicitly reported non-application), never by silent remapping to a different rule's semantics.
- Rule IDs remain stable across versions; behavior changes produce new versions. Configuration MUST reference both ID and version where the engine contract requires it, so historical records remain attributable.

### 14.5 Deterministic interpretation

- The same effective snapshot interpreted under the same contract revision MUST yield the same assessment semantics regardless of when or where interpretation occurs. Version-dependent interpretation differences MUST be explicit in provenance, never ambient.

---

## 15. Resource limits

### 15.1 Explicit, validated, bounded, fail-visible

Resource-limit configuration (C-05) MUST be explicit (stated by the owning contract, recorded in provenance), validated (see §6.3), bounded (both upper and lower bounds where meaningful, owned by the enforcing layer), and fail-visible (exhaustion surfaces as an explicit condition, never as silent success).

### 15.2 Exhaustion semantics

Resource exhaustion or truncation — collection paging exhaustion, graph traversal bounds reached, evaluation time/memory bounds reached, rendering size bounds reached, cancellation requested — MUST NEVER silently yield **PASS**. Depending on the stage and the owning contract's fatal-vs-isolated policy:

- evaluation-affecting exhaustion MUST surface as **ERROR** or **NOT_EVALUATED** with structured reason (per `rule-engine-contracts.md` + `error-result-model.md`);
- collection-affecting exhaustion MUST surface as partial-collection/capability state that downstream stages treat as incompleteness, not completeness;
- rendering-affecting exhaustion MUST surface as explicit renderer/output failure that does not mutate the authoritative result (per `output-renderer-contracts.md`).

Truncated output MUST NOT be presentable as a complete report (see output-architecture §§11, 20 and `output-renderer-contracts.md` §§13, 17).

### 15.3 No invented bounds

This document sets NO concrete numeric bounds (counts, sizes, durations, retry counts, concurrency levels). Concrete limits are TBDs owned by the enforcing contracts (`collector-contracts.md`, `rule-engine-contracts.md`, `identity-graph-contracts.md`, `output-renderer-contracts.md`) — see §17. Stating numbers here would be invention.

---

## 16. Testing implications

Configuration design MUST support deterministic synthetic tests for at least the following. No test framework is chosen here; no tests are created in this phase. The scenario catalog belongs to `testing-seams.md` (derived from `testing-architecture.md`); this section states only what the configuration contracts MUST make testable.

| # | Testable property | Implication for contracts |
| --- | --- | --- |
| T-CFG-01 | Valid configuration accepted deterministically | Contracts MUST define at least one valid shape per surface so synthetic valid fixtures produce byte-stable snapshot semantics. |
| T-CFG-02 | Invalid configuration rejected visibly | Malformed shape, wrong value kinds, unresolvable references, and out-of-range bounds MUST each have a defined visible-failure path exercised without live tenant or network. |
| T-CFG-03 | Conflicting settings handled per §6.5 | Fixtures combining mutually exclusive options MUST exercise either visible rejection or the documented restrictive resolution plus provenance recording. |
| T-CFG-04 | Missing required settings fail visibly | Omission of each required surface MUST have a defined failure path; tests MUST prove absence is never treated as enablement or completeness. |
| T-CFG-05 | Unsafe fallback prevention | Tests MUST prove no invalid/conflicting/missing input silently becomes a permissive posture (no fallback-to-**PASS**, no silent capability enablement, no assumed completeness). |
| T-CFG-06 | Tenant isolation | Synthetic multi-tenant fixtures MUST prove configuration bound to tenant A cannot influence tenant B's assessment; mismatch fixtures MUST yield visible integrity failure. Real tenant data MUST NOT be used. |
| T-CFG-07 | Deterministic snapshot behavior | Same candidate configuration MUST always yield the same snapshot; snapshot MUST be immutable/effectively immutable mid-run; reordered-but-equivalent source iteration MUST NOT change verdict semantics. |
| T-CFG-08 | Resource bounds | Bound fixtures (at-limit, over-limit, zero/negative/nonsensical, jointly incoherent) MUST exercise validation rejection and exhaustion fail-visible paths without asserting any invented numeric bound from this document. |
| T-CFG-09 | Secret exclusion | Configuration fixtures MUST prove secret-bearing values have no accepted path into snapshots, provenance, evidence, findings, outputs, or logs. Tests use placeholders/mocks, never real secrets. |
| T-CFG-10 | Unknown-setting handling | Unknown-key fixtures MUST exercise the §6.7 policy per surface (reject vs explicitly reported ignore) and prove security semantics unchanged. |
| T-CFG-11 | Default explicitness | Absent-optional fixtures MUST prove resolution to the stated default, provenance recording of defaulted-ness, and no weakening of false-**PASS** protections. |
| T-CFG-12 | Layer separation | Tests MUST prove rule evaluation observes only its snapshot slice (no ambient reads), Core observes no provider/renderer settings, and output options do not reach evaluation. |

All configuration tests MUST run without a live tenant, production credentials, network access for core paths, or real customer/employer data.

---

## 17. Implementation TBDs

Unresolved implementation choices are preserved explicitly rather than guessed. Each TBD states what is unknown, why it cannot be resolved here, and which document owns its resolution. None is resolved in this document.

| # | TBD | Why unresolved here | Owned by |
| --- | --- | --- | --- |
| CFG-T-01 | Concrete configuration source mechanism(s) and file/store format(s), if any. | Selecting a mechanism (files, stores, parsing libraries) is an implementation choice requiring later detailed design; architecture does not mandate one. | Later detailed design constrained by this document; format semantics (if files chosen) validated against §§4–7. This document MUST NOT be read as having selected one. |
| CFG-T-02 | Concrete command-line syntax, option spelling, and exit-code mapping. | CLI syntax is presentation/host detail requiring later design; choosing it here would preempt host and output contracts. | `output-renderer-contracts.md` (exit behavior policy) with configuration semantics from this document; composition mechanics in `EntraNHI.Cli` per `solution-structure.md` §13. |
| CFG-T-03 | Concrete inward-facing configuration contract shapes (field lists, member names, serialization forms). | Field-level shapes belong to the coordinated contract surface, not to this category-level document (per `README.md` §6 decision ownership). | `core-contracts.md` coordinating `rule-engine-contracts.md`, `capability-model.md`, `collector-contracts.md`, `output-renderer-contracts.md`, `error-result-model.md`. |
| CFG-T-04 | Concrete default values: default rule enablement, default capability scope, default output selection, default numeric bounds. | Stating values here would invent behavior without rule/capability/output authority. | Enforcing contract per surface: `rule-engine-contracts.md` (rule defaults), `capability-model.md` (capability defaults), `output-renderer-contracts.md` (output defaults), `collector-contracts.md` / `rule-engine-contracts.md` / `output-renderer-contracts.md` (respective bounds). |
| CFG-T-05 | Concrete numeric resource bounds (counts, sizes, durations, retry/backoff postures, concurrency). | Bounds require later performance, API-behavior, and serialization analysis; inventing numbers would be speculation. | Enforcing layer contract (see CFG-T-04 owners) with `dependency-boundaries.md` for enforcement mechanics. |
| CFG-T-06 | Exact Graph endpoints, permission/scope rationale, property mappings, Agent Identity mappings, licensing-behavior claims, SDK-vs-HTTP decision, pagination/retry constants. | Require validation against published Microsoft documentation; MUST NOT be invented (INV-15). | `collector-contracts.md` (endpoints/properties/mappings), `authentication-design.md` (permission rationale), `capability-model.md` (granularity), per `solution-structure.md` §20/T-04. |
| CFG-T-07 | Authentication mechanisms, flows, token-cache/storage posture, tenant-validation mechanics, consent UX, sovereign-cloud handling. | Authentication implementation detail outside this document's authority. | `authentication-design.md` per `solution-structure.md` §20/T-05. |
| CFG-T-08 | Configuration schema revision identifier and compatibility/versioning representation. | Versioning representation is a cross-contract decision (configuration + output + rule versions) requiring later coordination. | This document owns the requirement (see §14); representation TBD coordinated with `core-contracts.md` and `output-renderer-contracts.md`; no format chosen here. |
| CFG-T-09 | Fatal-vs-isolated failure policy for configuration failures scoped to a subset of rules/stages. | Requires coordination with engine error taxonomy and aggregation semantics. | `rule-engine-contracts.md` (evaluation-scope policy) with `error-result-model.md` (failure-category mapping). |
| CFG-T-10 | Assessment-time representation and time-dependent policy mechanics. | Time-dependent evaluation requires explicit deterministic design not yet specified. | `rule-engine-contracts.md` (with `core-contracts.md`) per `solution-structure.md` §20/T-10. |
| CFG-T-11 | Dependency-injection mechanics, logging/telemetry abstraction surface, and structural-enforcement tooling for configuration boundaries. | Implementation mechanics deferred to seam catalog and build controls. | `dependency-boundaries.md` per `solution-structure.md` §20/T-12. |
| CFG-T-12 | Per-surface test fixtures, adversarial configuration payloads, golden/snapshot policy for configuration, and test-framework choice. | Test authoring belongs to the testability catalog, not to this contract document. | `testing-seams.md` per `solution-structure.md` §20/T-13. |

---

## 18. Traceability and acceptance

### 18.1 Invariant traceability

| Invariant | Preserved by |
| --- | --- |
| INV-02 deterministic assessment | §§5, 8, 13, 14 |
| INV-03 provider isolation / INV-04 normalized boundary | §§1.3, 3, 8, 10 |
| INV-05 explicit five states | §§1, 6, 8, 9 |
| INV-06 evidence traceability / INV-11 provenance | §§1.3, 7, 8, 9, 13 |
| INV-07 capability awareness | §§1.3, 2, 6, 8, 10 |
| INV-08 least privilege | §§10, 11 |
| INV-09 secret exclusion | §11 (+ §§4, 6, 13, 16) |
| INV-10 tenant isolation | §12 (+ §§4–6, 16) |
| INV-12 core/output separation | §§3, 4, 9 |
| INV-13 AI non-authority | §1.3 (+ §§4, 6) |
| INV-14 failure transparency | §§1.3, 6, 8, 9, 15 |
| INV-15 no undocumented dependency | §10 (+ §§6, 8) |
| INV-16 security-sensitive defaults | §7 (+ §§6, 14) |

### 18.2 Acceptance criteria

This document is accepted when:

1. Every §1.3 prohibition is stated without exception paths that would permit silent weakening.
2. Categories (§2) separate security-critical from presentation-only configuration with no cross-influence path.
3. Ownership (§3) preserves Core independence, Application orchestration, provider-outside-Core, renderer-outside-authority, and CLI-as-composition-root.
4. Loading (§4), snapshot (§5), validation (§6), and defaults (§7) jointly guarantee deterministic, fail-visible behavior with no silent permissive fallback.
5. Rule (§8), output (§9), provider (§10), secret (§11), tenant (§12), provenance (§13), versioning (§14), and resource (§15) sections each preserve their cited invariants without inventing endpoints, permissions, properties, mappings, numeric values, formats, frameworks, or libraries.
6. Exactly the five states **PASS**, **FAIL**, **NOT_EVALUATED**, **NOT_APPLICABLE**, **ERROR** appear as assessment states; no sixth state, alias, or renderer-specific verdict is introduced.
7. Every implementation-sensitive unknown is listed in §17 with its owning document; no TBD is resolved by speculation.

---

*(End of file)*
