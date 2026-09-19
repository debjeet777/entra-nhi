# EntraNHI Phase 0.3 — Implementation Design Package

> **Phase:** 0.3
> **Package:** Implementation Design
> **Status:** Complete — reviewed and locked; design phase only
> **Date:** 2026-09-18

---

## 1. Status

- This package is **Phase 0.3, Implementation Design**.
- This is a **design phase only**. It produces implementation-ready contracts,
  interface boundaries, and structural decisions. It does not produce
  implementation.
- **Product implementation has NOT started.** No application, source, test,
  project (`.sln`/`.csproj`), dependency, or build code is introduced by this
  package.
- No statement in this package claims that a security control, collector,
  rule, renderer, authentication mechanism, or test is implemented, verified,
  or enforced. Where architecture documents state `DESIGNED / REQUIRED`, this
  package preserves that classification.

Normative language follows `docs/02-architecture/README.md`:

- **MUST / MUST NOT** for genuine constraints carried forward from
  requirements or architecture.
- **SHOULD** where implementation flexibility is intended.
- **TBD** for an implementation-sensitive unknown that this package does not
  resolve by speculation.

---

## 2. Purpose

This package translates the approved requirements, architecture invariants,
threat model, trust boundaries, and testing architecture into
implementation-ready contracts that a later implementation phase can build
against without reinterpreting architectural intent.

Concretely, this package exists to:

1. Convert the normalized domain model (`docs/02-architecture/domain-model.md`)
   into explicit implementation contracts (records, states, semantics) without
   inventing undocumented provider properties.
2. Convert the collector, capability, normalization, graph, rule-engine,
   findings/evidence, authentication, output, configuration, and testing
   architectures into explicit interface and schema contracts.
3. Define the proposed .NET solution/project structure and dependency
   direction without creating project files or code.
4. Define explicit seam boundaries (dependency inversion points) so each
   architectural boundary is testable with synthetic fixtures and without a
   live tenant.
5. Define the implementation sequence so later work proceeds in an order that
   preserves provider isolation, core/output separation, and failure
   transparency.
6. Record every implementation-sensitive unknown as an explicit **TBD** bound
   to the later design document responsible for resolving it, instead of
   guessing.

What this package does not do:

- It does not modify requirements or architecture.
- It does not invent Microsoft Graph endpoints, permissions, scopes,
  properties, Agent ID mappings, or undocumented relationships.
- It does not select package versions, SDKs, libraries, or frameworks beyond
  what is already approved elsewhere.
- It does not introduce SaaS, dashboard, remediation, monitoring, AI-verdict,
  or other out-of-scope capabilities.

---

## 3. Sources of Engineering Truth

### 3.1 Authoritative inputs

| Source | Role |
| --- | --- |
| `docs/01-requirements/product-requirements.md` | Authoritative functional, security, output, capability, verdict, constraint, and acceptance requirements (FR, SEC, NFR, OUT, CAP, VERD, ASM, CON, NG, AC series). |
| `docs/02-architecture/` (all 12 documents) | Authoritative architecture and security design: system context, invariants (INV-01–INV-16, INV-G1–INV-G8), domain model, collection, identity graph, rule engine, findings/evidence, authentication/authorization, output, trust boundaries (15 boundaries), threat model (including adversarial analysis A–L), testing architecture. |
| `docs/00-product-research/` | Product context only (problem, vision, landscape, differentiation, methodology). Used only where product context is necessary to interpret intent. Never authoritative over requirements or architecture. |
| `AGENTS.md`, `SECURITY.md` (repository root) | Repository-level security and agent-operating policy (read-only assessment default, least privilege, secret exclusion, non-destructive change, authorization boundaries). Preserved as constraints on this package. |

### 3.2 Precedence

1. **Requirements + architecture/security design** are engineering truth and
   take precedence over everything else in this package.
2. **Product research** is subordinate context. Where product research and
   requirements/architecture disagree, requirements/architecture govern.
3. **This package (Phase 0.3)** is subordinate to both. It MUST NOT relax,
   reinterpret, or silently override any requirement or invariant. An apparent
   conflict between an implementation preference and an approved requirement
   or invariant is resolved in favor of the requirement or invariant, and the
   conflict is escalated through change control (see §8) rather than resolved
   by editing meaning in place.

### 3.3 Traceability expectation

Each Phase 0.3 contract document traces its constraints back to specific
requirement IDs and invariant IDs. A design statement that cannot be traced
to an approved requirement, invariant, trust boundary, or explicitly marked
implementation choice is either a **proposed** structural decision owned by
that document or a **TBD** — it is never silent architecture.

---

## 4. Design Principles

The following principles are carried forward from requirements and
architecture and MUST be preserved by every document in this package. They
are constraints on implementation contracts, not claims of implemented
behavior.

1. **Read-only V1.** No contract may define a write, delete, update,
   remediation, credential-rotation, permission-modification, or other tenant-
   mutating operation. (INV-01; FR-040; SEC-003–SEC-007; CON-001.)
2. **Deterministic assessment.** Identical normalized inputs, capability
   state, rule version/configuration, and relevant deterministic execution
   context produce the same evaluation outcome. Rule contracts MUST NOT admit
   randomness, wall-clock reads inside evaluation logic, probabilistic
   branching, or LLM inference on the evaluation path. (INV-02; VERD-006.)
3. **Explicit five-state semantics.** Every rule evaluation resolves to
   exactly one of **PASS**, **FAIL**, **NOT_EVALUATED**, **NOT_APPLICABLE**,
   or **ERROR**. No additional terminal state may be created in V1. Missing
   data MUST NOT be silently coerced to PASS or FAIL; ERROR MUST NOT be
   converted to FAIL. (INV-05; VERD-001–VERD-005.)
4. **Capability/licensing awareness.** Licensing, API, permission, and support
   limitations are represented explicitly and flow into evaluation through
   capability state. Unavailable capability MUST NOT automatically resolve to
   FAIL. No tenant is assumed identical to another. (INV-07; CAP-001–CAP-004.)
5. **False-PASS resistance.** PASS requires affirmative satisfaction of the
   rule's defined conditions plus the rule-defined input completeness for
   that rule. "Nothing found" is PASS only when the rule explicitly
   establishes that collection was complete enough for absence to be
   meaningful. Incomplete collection, authorization denial, unsupported
   capability, collection error, normalization failure, graph failure, and
   resource exhaustion MUST NOT produce PASS. (Rule-engine §4.1, §5; threat-
   model paths A, C, J; testing-architecture §9.1.)
6. **Tenant isolation.** Each execution context targets a single tenant. No
   contract may permit silent cross-tenant normalization, graph edges,
   evidence correlation, or artifact mixing. Tenant-context mismatch is an
   integrity failure and fails safely and visibly. (INV-10; INV-G1.)
7. **Evidence/provenance.** PASS and FAIL require traceable evidence
   referencing collected normalized facts. NOT_EVALUATED, NOT_APPLICABLE, and
   ERROR require structured reason/context. Provenance (source system, object
   reference, collection operation/context, assessment context) is preserved
   across every transformation boundary and MUST NOT be fabricated.
   (INV-06; INV-11.)
8. **Secret exclusion.** Secret values, private-key material, client secrets,
   passwords, bearer/access/refresh tokens, and recovery codes are not
   assessment-domain data. They MUST NOT enter domain records, graph,
   findings, evidence, reports, logs, or persisted artifacts. Transient
   authentication material is runtime-only. (INV-09; SEC-001; SEC-002; OUT-006.)
9. **Least privilege.** Authentication and collection contracts request only
   the minimum source access required for enabled capabilities. Optional
   capabilities MUST NOT silently broaden privilege requirements.
   (INV-08; SEC-008; FR-030.)
10. **Renderer/output non-authority.** Assessment semantics are independent of
    CLI, JSON, SARIF, HTML, or future presentation. Output contracts are
    projections of authoritative `RuleEvaluation`/finding data. Renderers
    MUST NOT rerun rules, call providers, request privilege, alter evaluation
    state, reinterpret severity into state, fabricate evidence, or suppress
    ERROR/NOT_EVALUATED into a false-secure impression. (INV-12.)
11. **AI non-authority.** AI/LLM output MUST NEVER determine, modify, or
    override PASS, FAIL, NOT_EVALUATED, NOT_APPLICABLE, or ERROR; MUST NEVER
    supply or fabricate evidence, capability state, or severity; and MUST
    NEVER sit on the authentication, authorization, collection, or evaluation
    path. Any future AI use is downstream explanation only. (INV-13; SEC-010;
    VERD-007; CON-004.)
12. **Failure transparency.** Collection, normalization, graph, rule,
    evidence, and serialization failures are represented explicitly (ERROR,
    NOT_EVALUATED, or explicit diagnostics) and MUST NOT silently produce
    successful or security-compliant results. (INV-14.)
13. **Provider normalization.** Provider-specific source objects are translated
    into stable normalized contracts before evaluation. Provider SDK types,
    raw payloads, HTTP semantics, and undocumented properties MUST NOT leak
    into deterministic rule contracts except through explicitly normalized
    capability/provenance data. (INV-03; INV-04.)
14. **Testability.** Every architectural boundary is expressed as an explicit
    seam testable with synthetic fixtures, fakes, and mocks, without a live
    tenant, production credentials, or network access for core tests.
    (Testing-architecture §§1–2; INV-03/INV-04 consequences.)
15. **Dependency control.** No unnecessary SDKs or dependencies. Core MUST NOT
    depend on infrastructure, output, host, AI/LLM, authentication
    implementation, or concrete logging/telemetry backends. Dependencies are
    minimized, maintained, pinned/reviewed through future build controls, and
    aligned with supply-chain architecture. No package versions are chosen in
    this package unless already explicitly approved elsewhere. (CON-003.)

---

## 5. Document Map

`docs/03-implementation-design/` contains 16 documents. Each row states the
single responsibility of that document. Shared terms (e.g., `RuleEvaluation`,
capability states) are defined once in their owning document and referenced —
not redefined — elsewhere (see §6).

| # | Document | Responsibility |
| --- | --- | --- |
| 1 | `README.md` (this document) | Owns package governance: status, purpose, sources/precedence, principles, document map, decision ownership, TBD policy, change control, implementation gate, and maturity. Owns no type, interface, or schema definition. |
| 2 | `solution-structure.md` | Owns the proposed .NET solution/project structure, assembly-boundary criteria, dependency direction, forbidden dependencies, composition-root ownership, host extensibility, and build/release implications. Defines no C# types, Graph bindings, or package versions. |
| 3 | `core-contracts.md` | TBD — owns the stable core contract surface shared across normalization, graph, rules, findings, and output (assessment context, normalized identity/record references, capability-state references, provenance references, evaluation/finding references) as abstracted from provider types. |
| 4 | `domain-types.md` | TBD — owns the normalized domain type contracts (`IdentityRecord`, `IdentityKind`, credential-metadata, permission/access, accountability, tenant context, absent/unknown semantics) derived from `domain-model.md` without inventing undocumented provider fields. |
| 5 | `collector-contracts.md` | TBD — owns collector interfaces, the source-observation envelope contract, pagination/retry/cancellation surfaces, and the collector-to-normalization handoff, derived from `collection-architecture.md`. Owns no endpoint, permission-name, or SDK-method choice. |
| 6 | `capability-model.md` | TBD — owns the implementable capability-state contract (states, granularity, per-category scoping, propagation shape) derived from the domain, collection, and rule-engine capability semantics. Owns no licensing-behavior claims about Microsoft tenants. |
| 7 | `identity-graph-contracts.md` | TBD — owns the graph contract surface (node/edge shapes, observed-vs-derived classification, traversal/projection operations, integrity rules INV-G1–INV-G8) as a deterministic projection of normalized contracts. Owns no graph-database or storage choice. |
| 8 | `rule-engine-contracts.md` | TBD — owns the deterministic rule-engine contracts (`RuleDefinition`, evaluation pipeline stages, `RuleEvaluation` shape, applicability/capability/input validation order, isolation and configuration surfaces) with exactly five terminal states. Owns no concrete security rule content. |
| 9 | `findings-evidence-schema.md` | TBD — owns the findings/evidence/provenance/diagnostic schema contracts (`RuleEvaluation` vs `Finding` distinction, evidence references, provenance chain, emission-policy hook, redaction hook) with secret-exclusion and fabrication-defense constraints. Owns no signing/hashing implementation. |
| 10 | `error-result-model.md` | TBD — owns the shared error/result contract (`Result`/`Error` shape, failure-category taxonomy, mapping of failures to NOT_EVALUATED vs ERROR, diagnostic vs finding separation, sanitization rules) used consistently across collectors, normalization, graph, rules, evidence, and output. |
| 11 | `output-renderer-contracts.md` | TBD — owns the canonical output model contract and per-renderer contracts (CLI, JSON, SARIF, HTML), semantic-preservation rules, injection-defense obligations, file/path-safety obligations, and renderer-failure semantics. Owns no templating-library, schema-version, SARIF-mapping, or CLI-syntax choice beyond approved architecture. |
| 12 | `authentication-design.md` | TBD — owns the authentication/authorization implementation boundaries (execution contexts, `AuthorizedAccessContext` abstraction shape, per-capability authorization surface, secret/token handling, tenant-context establishment, failure categories) without prescribing OAuth flows, SDKs, permission names, or storage mechanisms. |
| 13 | `configuration-design.md` | TBD — owns configuration surfaces (assessment options, rule-set/version selection, capability selection, output options) with secure-default, explicit-opt-in, unknown-option, and invalid-configuration semantics. Owns no configuration-file format choice beyond approved constraints. |
| 14 | `dependency-boundaries.md` | TBD — owns the dependency-inversion seam catalog: which interface lives in which assembly, which direction each dependency flows, which references are forbidden (including automated-enforcement intent for `EntraNHI.Architecture.Tests`), and the logging/telemetry-abstraction boundary. Owns no package selection. |
| 15 | `testing-seams.md` | TBD — owns the testability contract catalog: required fakes/mocks/fixtures per boundary, seam-by-seam test scenarios derived from testing architecture (including false-PASS/false-FAIL, tenant-isolation, injection, and failure-injection coverage), golden/snapshot policy, and the requirement-to-test traceability hook. Owns no test-framework choice and creates no tests. |
| 16 | `implementation-sequence.md` | TBD — owns the ordered build plan (which contracts and seams are established first, which collectors/rules/renderers follow, and which gate criteria must hold before the next stage), sequenced to protect provider isolation and core/output separation. Authorizes no implementation start until the §9 gate is met. |

All 16 documents in this package are authored and non-empty. Their
detailed contracts remain governed by each document's stated ownership in
the table above and in §6. Package-level review and final lock-gate review
are complete; Phase 0.3 is locked and this package is the approved
implementation-design baseline. Authored, reviewed, and locked does NOT mean
implemented, verified in product code, or released; product implementation
remains NOT started (see §9–§10).

---

## 6. Decision Ownership

To avoid duplicate or conflicting definitions, each implementation decision
has exactly one owning document. Other documents reference the owner's
definition by name; they MUST NOT restate it normatively.

| Decision | Owner | Non-owners MUST |
| --- | --- | --- |
| Package governance, precedence, gate | `README.md` | Reference; not restate governance. |
| Assembly/project boundaries, dependency direction, composition root, host extensibility | `solution-structure.md` | Reference the tree/direction; not redefine projects. |
| Seam placement and allowed/forbidden references | `dependency-boundaries.md` (with structural ownership by `solution-structure.md`) | `solution-structure.md` owns the tree; `dependency-boundaries.md` owns the per-seam interface catalog. In case of apparent conflict, the forbidden-dependency rule (deny) governs until both documents are reconciled through change control. |
| Shared core contract surface | `core-contracts.md` | Reference; not duplicate field lists. |
| Normalized domain types and absent/unknown semantics | `domain-types.md` | Reference; not re-derive domain semantics. |
| Collector interfaces and normalization handoff | `collector-contracts.md` | Reference; not redefine envelopes. |
| Capability states and propagation | `capability-model.md` | Reference state names/semantics; not redefine them. |
| Graph nodes/edges/operations and INV-G rules | `identity-graph-contracts.md` | Reference; not redefine edge taxonomy. |
| Rule definitions, pipeline, `RuleEvaluation` evaluation surface | `rule-engine-contracts.md` | Reference; not redefine states or pipeline order. |
| `Finding` vs `RuleEvaluation`, evidence/provenance/diagnostic schemas, emission hook | `findings-evidence-schema.md` | Reference; not redefine evidence semantics. |
| Shared error/result shape and failure taxonomy | `error-result-model.md` | Reference; not redefine failure categories. |
| Canonical output model and renderer obligations | `output-renderer-contracts.md` | Reference; not redefine renderer rules. |
| AuthN/AuthZ boundaries and access-context abstraction | `authentication-design.md` | Reference; not prescribe flows or permissions. |
| Configuration surfaces and secure defaults | `configuration-design.md` | Reference; not redefine option semantics. |
| Test seams, fixtures, and scenario catalog | `testing-seams.md` | Reference; not redefine test taxonomy. |
| Build order and stage gates | `implementation-sequence.md` | Reference; not resequence stages. |

Terminology rule: the first normative definition of `RuleEvaluation`,
`Finding`, evidence reference, capability state, `SourceObservation`,
`AuthorizedAccessContext`, canonical output model, and error/result shape
lives in its owning document. All other documents use the term referentially
(e.g., "the `RuleEvaluation` defined by `rule-engine-contracts.md`") and
MUST NOT assign it a competing shape.

---

## 7. TBD Policy

1. **Implementation-sensitive unknowns remain explicit TBDs.** Where
   authoritative Microsoft documentation validation, a library choice, a
   schema finalization, or an operational constant is required, the document
   marks it **TBD** and names the later design document responsible for
   resolving it. No guessing.
2. **Non-exhaustive TBD classes** include: exact Graph endpoints, permission/
   scope names, SDK methods, Agent ID property/relationship mappings,
   licensing-behavior claims, retry/timeout/concurrency constants, schema
   field names, serialization formats, CLI syntax/exit codes, SARIF mappings,
   HTML templating/CSP strategy, OAuth flows, token-cache/storage mechanisms,
   tenant-validation mechanics, identifier formats, signing/hashing, package
   selections/versions, target-framework/monikers, and numeric resource
   limits.
3. **TBD format.** Each TBD states (a) what is unknown, (b) why it cannot be
   resolved at this stage (e.g., "requires validation against published
   Microsoft documentation"), and (c) which later document owns its
   resolution.
4. **No silent resolution.** A TBD resolved without its owning document, or
   resolved by inventing undocumented Microsoft behavior, is a defect in this
   package. Reviewers SHOULD reject it under the §9 gate.
5. **TBDs do not block package structure.** The solution tree, dependency
   direction, and seam catalog are designed so TBD details (e.g., which Graph
   SDK call backs a collector) can be filled in later without redesigning the
   normalized core.

---

## 8. Change Control

1. **Architecture changes require returning to architecture.** If
   implementation design reveals that an invariant, trust boundary, threat-
   model mitigation, testing-architecture requirement, or product requirement
   is unworkable, the change is made in the relevant document under
   `docs/01-requirements/` or `docs/02-architecture/` — with its own review —
   and this package is then updated to match. This package MUST NOT silently
   override architecture in place.
2. **Additive and reversible preference.** Consistent with repository policy,
   prefer additive contract extensions over breaking redefinitions. A breaking
   contract change requires explicit justification, impact statement, and
   recovery path before approval.
3. **No scope expansion through design.** SaaS/multi-tenancy, dashboard,
   remediation, monitoring/alerting, AI verdicts, plugin marketplaces, and
   other NG-series non-goals MUST NOT enter this package as assumed
   capabilities. Proposing any of them requires an approved requirements/
   architecture change first.
4. **Dependency discipline.** Introducing a dependency beyond Graph
   communication, output formatting, and CLI operation requires explicit
   approval (CON-003). This package records dependency *principles* only; any
   concrete addition is a later, separately reviewed decision.
5. **No bypass of controls.** Nothing in this package bypasses secret
   scanning, signing, branch protection, or required checks, and nothing in
   this package authorizes committing secrets, production data, or
   credential-bearing configuration.

---

## 9. Implementation Gate

**No production implementation begins until this package is reviewed and
locked.** Concretely:

> **Phase 0.3 lock state:** package-level review and final lock-gate review
> are complete; Phase 0.3 is locked. This satisfies the review/lock
> prerequisite stated above. Product implementation remains NOT started
> (see §10).

1. All 16 documents are authored, reviewed, and internally consistent; no
   conflicting definitions remain across owners (§6).
2. Every implementation-sensitive unknown is either resolved with traced
   authority or marked as an explicit TBD with a named owning document (§7).
3. No Microsoft Graph endpoint, permission, property, Agent ID mapping, or
   undocumented relationship is invented anywhere in the package.
4. Dependency direction and forbidden dependencies are stated and assigned an
   enforcement path (see `solution-structure.md` §8–§9 and the future
   `dependency-boundaries.md` enforcement intent).
5. The false-PASS/false-FAIL, tenant-isolation, secret-exclusion, renderer-
   non-authority, and AI-non-authority properties have explicit contract
   support and explicit test seams (future `testing-seams.md`).
6. Validation for this package phase (file scope, emptiness of uninvolved
   files, `git diff --check`, `git status`) is reported and clean.

Phase 0.3 has satisfied this gate through review and lock. Authorized work
remains limited to design documents in
`docs/03-implementation-design/`. Creating `.sln`/`.csproj` files, running
`dotnet` scaffolding, writing application/source/test code, or staging,
committing, or pushing implementation artifacts remains out of scope;
product implementation has NOT started.

---

## 10. Current Maturity

| Track | State |
| --- | --- |
| Requirements (`docs/01-requirements/`) | **Complete** (approved engineering truth for V1 scope). |
| Architecture/security design (`docs/02-architecture/`) | **Complete** (approved engineering truth for structure, invariants, boundaries, threat model, testing architecture). |
| Product research (`docs/00-product-research/`) | **Complete** (subordinate product context). |
| Implementation design (`docs/03-implementation-design/`) | **Complete — reviewed and locked** — this package. All 16 documents are authored and non-empty; package-level review and final lock-gate review are complete; Phase 0.3 is locked and this package is the approved implementation-design baseline. Reviewed/locked does NOT mean implemented, verified in product code, or released; the §9 review/lock prerequisite is satisfied and product implementation remains NOT started. |
| Product implementation (`src/`, `tests/`, packages, CI release) | **NOT started.** No code, projects, dependencies, or builds are introduced or claimed. |

---

## Self-verification (package authoring note)

This README was authored under the Phase 0.3.3 authorization:

- Only `docs/03-implementation-design/README.md` and
  `docs/03-implementation-design/solution-structure.md` were modified.
- No other file was created, deleted, renamed, moved, or modified.
- No requirements or architecture document was modified.
- No application/source/test code and no `.sln`/`.csproj` files were created.
- No Microsoft Graph endpoints, permissions, properties, Agent ID mappings,
  or undocumented relationships were invented.
- No secrets or credential material were introduced.
- No `git add/commit/push/reset/clean/checkout/switch` was performed.

(End of file)
