# Solution Structure

> **Phase:** 0.3.3
> **Package:** Implementation Design
> **Status:** Proposed — design only, no code or project files created
> **Date:** 2026-09-18

---

## 1. Status and scope

- **Status:** Proposed implementation structure. This document is design
  output only. It authorizes no implementation.
- **Scope:** Defines the proposed .NET solution/project structure and
  dependency direction for EntraNHI V1 **without creating code or project
  files**. No `.sln`, `.csproj`, `src/`, or `tests/` content is created by
  this document.
- **What this document is:** A structural proposal derived from the approved
  requirements (`docs/01-requirements/product-requirements.md`) and
  architecture (`docs/02-architecture/`, especially `system-context.md`,
  `architecture-invariants.md`, `domain-model.md`,
  `collection-architecture.md`, `identity-graph.md`, `rule-engine.md`,
  `findings-evidence.md`, `authentication-authorization.md`,
  `output-architecture.md`, `trust-boundaries.md`, `threat-model.md`, and
  `testing-architecture.md`).
- **What this document is not:** It is not a type catalog, interface
  specification, Graph binding, permission list, package selection, build
  script, or implementation sequence. Those belong to the owning Phase 0.3
  documents identified in `README.md` §5–§6 (notably `core-contracts.md`,
  `domain-types.md`, `collector-contracts.md`, `capability-model.md`,
  `identity-graph-contracts.md`, `rule-engine-contracts.md`,
  `findings-evidence-schema.md`, `error-result-model.md`,
  `output-renderer-contracts.md`, `authentication-design.md`,
  `configuration-design.md`, `dependency-boundaries.md`, `testing-seams.md`,
  and `implementation-sequence.md`).
- **Language discipline:** This document distinguishes three statement
  classes: (a) **approved architectural requirement** (traced to a
  requirement/invariant ID), (b) **proposed implementation structure**
  (owned by this document), and (c) **unresolved TBD** (explicitly marked
  with the later document responsible). Nothing herein claims a control is
  implemented, verified, or enforced.

---

## 2. Design drivers

The proposed structure is driven by the following approved requirements
(not by tooling preference):

1. **Read-only, single-tenant CLI/library delivery.** V1 is an operator-run
   assessment tool delivered as a CLI tool or library with no mandatory
   server component (NFR-001), executing against one tenant per run (INV-10;
   NG-003). The structure MUST support a thin CLI host now and a future
   host later without restructuring the core.
2. **Provider isolation + normalized domain boundary.** The rule engine MUST
   NOT query providers; rules consume normalized contracts only
   (INV-03; INV-04). The structure MUST make it physically difficult for
   provider SDK types to reach rule logic.
3. **Deterministic five-state evaluation.** Exactly PASS, FAIL,
   NOT_EVALUATED, NOT_APPLICABLE, ERROR (INV-02; INV-05). The structure MUST
   give the rule engine, identity graph, and findings/evidence stable homes
   whose boundaries survive later rule additions.
4. **Capability awareness and failure transparency.** Capability detection is
   first-class (INV-07); failures are explicit, never silent PASS/FAIL
   (INV-14). The structure MUST let collectors report capability/failure
   state that survives normalization into evaluation.
5. **Evidence traceability and provenance.** PASS/FAIL require evidence;
   other states require structured reason; provenance is preserved end to
   end (INV-06; INV-11). Findings/evidence MUST be structurally separated
   from both evaluation logic and presentation.
6. **Core/output separation.** Assessment semantics are independent of CLI,
   JSON, SARIF, HTML, or future presentation (INV-12). Renderers are
   downstream projections and MUST NOT hold rule authority.
7. **Secret exclusion and least privilege.** Secrets never enter domain
   artifacts (INV-09); only minimum permissions are requested (INV-08). The
   structure MUST isolate authentication material and provider clients at
   the outer boundary.
8. **AI non-authority.** AI/LLM is never on the verdict, authorization, or
   collection path (INV-13). The structure MUST NOT create an AI-shaped hole
   in the core; any future explanatory AI attaches outside authoritative
   projects.
9. **Testability without a live tenant.** Core evaluation is testable from
   synthetic normalized fixtures with mocked providers (testing-architecture
   §§1–2; INV-03/INV-04 consequences). The structure MUST yield seams that
   `testing-seams.md` can target per boundary, including adversarial,
   tenant-isolation, injection, and failure-injection coverage.
10. **Dependency control and supply-chain restraint.** No unapproved
    dependencies (CON-003); network limited to approved endpoints (CON-005).
    The structure MUST minimize the dependency surface of the core and
    concentrate third-party surface in isolated adapters.

---

## 3. Assembly/project boundary criteria

A separate assembly (project) is proposed **only if** at least one of the
following holds; otherwise the responsibility is a namespace/module inside
an existing project (see §7):

1. **Divergent dependency risk.** The responsibility requires a third-party
   or platform dependency that the core MUST NOT take (Graph SDK/HTTP,
   HTML/SARIF libraries, CLI framework, web framework, AI/LLM SDK,
   authentication implementation, concrete logging backend). Separation
   makes the forbidden reference a project-reference violation rather than
   a code-review observation.
2. **Independent security authority.** The responsibility owns an
   invariant-enforcing role (deterministic verdicts; evidence authority;
   renderer non-authority; auth boundary) whose compromise mode differs
   from its neighbors. Separation makes authority visible in the solution
   explorer, not just in comments.
3. **Independent change cadence or replacement.** The responsibility is
   expected to change for reasons unrelated to its neighbors (new provider
   source; new output format; new host) without forcing recompilation or
   re-review of the stable core.
4. **Test-seam clarity.** The responsibility sits on an architectural seam
   that tests MUST substitute (provider adapter; renderer; auth mechanism;
   clock/ordering source). A project boundary gives the seam a stable
   assembly-qualified name and mock target.
5. **Threat-containment value.** Placing the responsibility behind its own
   boundary reduces the blast radius of its characteristic threat
   (provider-data parsing; tenant-string rendering; credential handling;
   filesystem writes).

Counter-criterion (against fragmentation): a boundary that satisfies none
of the above — e.g., splitting every logical collector, every rule, or
every renderer into its own assembly — adds versioning, signing, and review
cost without improving security or testability, and is therefore rejected.
Rules, graph projections, evidence shapes, and renderers vary *within*
their owning project via registration and namespace organization, not via
new assemblies per item.

---

## 4. Proposed repository/solution tree

Proposed — no files or directories are created by this document. The tree
below is the target the later implementation phase would scaffold after the
§9-equivalent implementation gate in `README.md` is met.

```text
entra-nhi/
  docs/
    00-product-research/
    01-requirements/
    02-architecture/
    03-implementation-design/      # this package (design only)
  src/
    EntraNHI.Core/                # normalized domain, graph, rules, findings/evidence contracts + logic
    EntraNHI.Application/         # use-case orchestration, pipeline coordination, port contracts
    EntraNHI.Infrastructure.Graph/# Microsoft Graph collection adapter (only provider-coupled project)
    EntraNHI.Output/              # CLI/JSON/SARIF/HTML renderers as isolated adapters
    EntraNHI.Cli/                 # V1 thin CLI host + composition root
  tests/
    EntraNHI.Core.Tests/
    EntraNHI.Application.Tests/
    EntraNHI.Infrastructure.Graph.Tests/
    EntraNHI.Output.Tests/
    EntraNHI.Cli.Tests/
    EntraNHI.Architecture.Tests/  # structural boundary enforcement (no-behavior tests)
```

Notes on the tree:

- `src/` contains five production projects; `tests/` contains six test
  projects. Section 5–6 define each project's responsibility; §7 defines the
  internal namespace boundaries that keep rule-engine, graph,
  findings/evidence, and renderer roles explicit without further assemblies.
- No solution (`.sln`) or project (`.csproj`) file is created now. File
  names, target frameworks, nullable/strictness settings, signing, and
  packaging layout are TBDs owned by later build-control work aligned with
  `dependency-boundaries.md` and the approved supply-chain posture
  (see §17–§18, §20).
- No future web/API host project is created in V1. Section 14 reserves its
  shape so V1 decisions do not block it.

---

## 5. Responsibility of each production project

### 5.1 `EntraNHI.Core` — normalized assessment authority (no I/O, no providers, no presentation)

- Owns the normalized domain contracts and logic: `IdentityRecord` /
  `IdentityKind`, credential-metadata (non-secret), permission/access,
  accountability, tenant assessment context, absent/unknown semantics,
  capability-state and provenance shapes **as defined by** `domain-types.md`,
  `capability-model.md`, and `core-contracts.md` (this document defines no
  fields).
- Owns the deterministic identity-graph projection: node/edge construction,
  observed-vs-derived classification, traversal/projection operations, and
  integrity rules INV-G1–INV-G8 **as contracted by**
  `identity-graph-contracts.md`.
- Owns the deterministic rule engine: rule definitions, pipeline stages
  (discovery → compatibility → applicability → capability validation →
  input validation → evaluation → state assignment → evidence handoff →
  diagnostics → aggregation), per-rule isolation intent, and configuration-
  validation semantics **as contracted by** `rule-engine-contracts.md`.
  Exactly five terminal states; no additional states.
- Owns findings/evidence/diagnostic construction: `RuleEvaluation` vs
  `Finding` distinction, evidence references, provenance chain, emission-
  policy hook, redaction hook, fabrication-defense obligations **as
  contracted by** `findings-evidence-schema.md`.
- Owns the shared error/result vocabulary used across the pipeline **as
  contracted by** `error-result-model.md`.
- Performs no network I/O, no filesystem writes beyond in-memory model
  construction, no provider calls, no authentication, no rendering, no
  process exit-code decisions.
- Has no dependency on any other `src/` project (see §8).

### 5.2 `EntraNHI.Application` — use-case orchestration (coordination only, no authority)

- Owns the end-to-end assessment use case: coordinate authenticate →
  detect capabilities → collect → normalize → build graph → evaluate →
  construct findings → hand canonical results to output, propagating
  capability state and structured failures at every step (collection window
  semantics, no transactional-snapshot claim unless the source provides it).
- Defines the port contracts (interfaces) that outer adapters implement:
  collector ports, capability-source ports, authentication-context ports,
  output ports, clock/ordering ports where determinism requires an explicit
  time reference. Concrete port shapes belong to `collector-contracts.md`,
  `authentication-design.md`, `output-renderer-contracts.md`, and
  `dependency-boundaries.md`; this document creates no interfaces.
- Holds no security/rule authority: it MUST NOT recompute rule logic,
  reinterpret verdicts, fabricate evidence, suppress ERROR/NOT_EVALUATED, or
  decide PASS/FAIL. It routes authoritative `Core` results.
- Holds no provider or renderer implementation: it depends on abstractions
  only. Provider SDK types and renderer libraries MUST NOT appear here.
- Carries tenant-context scoping for the run and ensures single-tenant
  execution; cross-tenant mixing is an integrity failure surfaced visibly.

### 5.3 `EntraNHI.Infrastructure.Graph` — provider-coupled collection adapter (the only Graph-coupled project)

- Owns all Microsoft Graph-coupled collection implementation: query
  execution, pagination, bounded retry/backoff, throttling handling,
  cancellation, partial-failure reporting, and translation of source
  observations toward the normalization handoff **as contracted by**
  `collector-contracts.md`.
- Implements the collector ports defined inward (see §8); preserves source
  references, provenance, tenant context, and per-category capability state;
  never constructs findings or verdicts; never passes secrets into the
  domain model.
- Is the only production project permitted to reference the Graph
  communication mechanism (SDK vs direct HTTP is a TBD owned by
  `collector-contracts.md` with `dependency-boundaries.md`; this document
  chooses neither).
- Contains no rule logic, no graph semantics beyond source-observation
  shaping, no renderer, no CLI parsing, no authentication-mechanism choice
  (it consumes the authorized context supplied through the auth boundary).

### 5.4 `EntraNHI.Output` — downstream renderer adapters (projection only, no authority)

- Owns the four V1 renderer adapters — CLI-text shaping, JSON, SARIF, HTML —
  behind one stable output contract each, plus shared encoding/file-safety
  helpers internal to this project, **as contracted by**
  `output-renderer-contracts.md`.
- Each renderer is a projection of the canonical output model supplied by
  the pipeline. Renderers MUST NOT rerun rules, call providers, request
  privilege, alter evaluation state, reinterpret severity into state,
  fabricate evidence/provenance, suppress diagnostics, or perform
  remediation.
- Treats all tenant/provider-originated strings as untrusted: HTML escaping/
  XSS defense, terminal/control-character defense, JSON correctness, SARIF
  field safety, and path/file-name safety obligations live here as
  implementation obligations of the contracts in
  `output-renderer-contracts.md` (no libraries chosen now).
- Performs filesystem writes only through validated, operator-configured
  destinations with safe-write/partial-artifact behavior defined by
  `output-renderer-contracts.md`; tenant-controlled values MUST NOT
  directly determine paths.

### 5.5 `EntraNHI.Cli` — V1 thin host and composition root

- Owns operator-facing CLI concerns only: argument parsing, command
  dispatch, configuration-file loading *shape* (format owned by
  `configuration-design.md`), exit-code mapping *policy* (mapping owned by
  `output-renderer-contracts.md`), human-readable console framing via the
  `EntraNHI.Output` CLI adapter, and file-destination wiring.
- Owns the V1 composition root: constructs the `Application` pipeline with
  the `Infrastructure.Graph` and `Output` adapters and the validated
  authentication mechanism, then starts the run. No other project composes
  the object graph (see §10).
- Holds no security/rule authority (see §13): no verdict logic, no evidence
  construction, no provider-query logic, no renderer-state reinterpretation,
  no authentication-policy decisions beyond delegating to the validated
  auth boundary.

---

## 6. Responsibility of each test project

Each test project mirrors its production project plus one structural
enforcement project. No tests are created in this phase; the catalog of
required scenarios belongs to `testing-seams.md` (derived from
`testing-architecture.md`).

| Test project | Responsibility |
| --- | --- |
| `EntraNHI.Core.Tests` | Unit/component/contract/property coverage of normalization, graph projection (INV-G1–INV-G8), deterministic rule-engine matrix (PASS/FAIL/NOT_EVALUATED/NOT_APPLICABLE/ERROR), false-PASS/false-FAIL defense, evidence/provenance integrity, error/result taxonomy — all from synthetic normalized fixtures, no live provider, no real secrets. |
| `EntraNHI.Application.Tests` | Orchestration coverage with faked adapters: capability/failure propagation, tenant-scoping, pipeline ordering, per-stage error mapping to NOT_EVALUATED vs ERROR, cancellation/timeout visibility, no-authority verification (orchestrator cannot change verdicts). |
| `EntraNHI.Infrastructure.Graph.Tests` | Adapter coverage with synthetic provider doubles: pagination, bounded retry/throttling, partial collection, authorization-denial and malformed-response handling, provenance/capability preservation, secret-exclusion, tenant-context carriage. No live tenant; no invented endpoint/permission claims. |
| `EntraNHI.Output.Tests` | Renderer coverage: five-state preservation per format, renderer-non-authority, injection payloads (HTML/XSS, ANSI/control, CR/LF, bidi, malformed Unicode), JSON/SARIF validity, path-traversal/overwrite/partial-artifact behavior, redaction semantics, no-network/no-telemetry verification. |
| `EntraNHI.Cli.Tests` | Host coverage: argument/config handling, exit-code mapping, composition wiring with faked adapters, safe file-destination behavior, failure visibility. No verdict logic tested here beyond pass-through. |
| `EntraNHI.Architecture.Tests` | Structural enforcement only (no behavior): project-reference direction, namespace dependency rules (e.g., rule-engine namespaces reference no provider/renderer/auth namespaces), forbidden-type references (Graph SDK, CLI framework, HTML/SARIF libraries, web framework, AI/LLM SDK, concrete logging backend) absent from `Core`. Tooling for enforcement is a TBD owned by `dependency-boundaries.md`. |

---

## 7. Internal module/namespace boundaries

Project boundaries alone are insufficient: the rule engine, identity graph,
findings/evidence, and renderer roles MUST remain explicit even where they
share an assembly. The following namespace/module organization is proposed
(logical; exact names are TBDs owned by `core-contracts.md`,
`dependency-boundaries.md`, and the respective contract documents):

- Inside `EntraNHI.Core` (no new assemblies):
  - `Domain` — normalized identity, credential-metadata, permission/access,
    accountability, tenant-context, absent/unknown semantics
    (`domain-types.md`).
  - `Capabilities` — capability-state vocabulary and propagation helpers
    (`capability-model.md`).
  - `Graph` — deterministic projection: node/edge construction, traversal,
    subgraph extraction, aggregation; enforces INV-G1–INV-G8
    (`identity-graph-contracts.md`).
  - `Rules` — rule definitions, pipeline stages, `RuleEvaluation`
    construction, isolation and determinism obligations
    (`rule-engine-contracts.md`). MUST NOT reference `Infrastructure`,
    `Output`, authentication-implementation, or provider namespaces.
  - `Findings` — `Finding`/evidence/provenance/diagnostic construction,
    emission-policy and redaction hooks, fabrication-defense obligations
    (`findings-evidence-schema.md`). MUST NOT recompute rule logic.
  - `Results` — shared error/result vocabulary and failure-category mapping
    (`error-result-model.md`).
- Inside `EntraNHI.Application`:
  - `UseCases` / `Pipeline` — assessment orchestration stages.
  - `Ports` — collector, capability, auth-context, output, and
    determinism-support (clock/ordering) abstractions. Port *shapes* belong
    to their owning contract documents.
- Inside `EntraNHI.Infrastructure.Graph`:
  - `Collectors` (per logical collector concern), `Pagination`,
    `Resilience` (retry/throttle/cancel), `Mapping` (source → normalization
    handoff shaping). No `Rules`, `Findings`, or renderer namespaces.
- Inside `EntraNHI.Output`:
  - One namespace per renderer (`Cli`, `Json`, `Sarif`, `Html`) plus
    internal `Encoding` and `Files` helpers. Renderers MUST NOT reference
    each other's state and MUST NOT reference `Infrastructure`, provider,
    or authentication namespaces.
- Inside `EntraNHI.Cli`:
  - `Commands`, `Configuration` (loading only; semantics owned by
    `configuration-design.md`), `Composition` (the single composition
    root), `ExitCodes` (mapping policy owned by
    `output-renderer-contracts.md`).

Namespace rules are enforced by `EntraNHI.Architecture.Tests` intent
(see §9, §15) once `dependency-boundaries.md` defines the rule set.

---

## 8. Dependency direction

Allowed project references (proposed; the per-interface seam catalog belongs
to `dependency-boundaries.md`):

```text
EntraNHI.Cli
  -> EntraNHI.Application
  -> EntraNHI.Output
  -> EntraNHI.Infrastructure.Graph
  -> EntraNHI.Core            (all adapters and host depend inward on Core)

EntraNHI.Application -> EntraNHI.Core
EntraNHI.Infrastructure.Graph -> EntraNHI.Core (+ Application ports where the seam catalog places them)
EntraNHI.Output -> EntraNHI.Core (+ Application output-port where the seam catalog places it)
EntraNHI.Core -> (no src/ project reference)
```

Principles:

- **Infrastructure depends inward on stable contracts.** Adapters implement
  ports defined inward; the core never reaches outward for provider data.
- **Core never depends outward on infrastructure, output, host, AI, auth
  implementation, or concrete logging.** Dependency inversion at the
  `Application` ports keeps `Core` compilable and testable with zero
  provider/renderer/host references.
- **No circular dependencies.** The graph is a DAG rooted at `Core`; the
  host sits at the outer edge and is referenced by nothing.
- **Composition at the outer boundary.** Only `EntraNHI.Cli` (V1) wires
  concrete adapters to ports (see §10). `Application` receives
  abstractions; it never `new`s an adapter.

---

## 9. Forbidden dependencies

The following are forbidden (each is an approved-architecture consequence,
not a new rule). The automated-enforcement rule set is a TBD owned by
`dependency-boundaries.md` and executed by
`EntraNHI.Architecture.Tests`.

1. `EntraNHI.Core` MUST NOT reference: Microsoft Graph SDK/HTTP provider
   libraries, CLI framework, HTML/SARIF renderer libraries, web framework,
   AI/LLM SDK, authentication implementation, concrete logging/telemetry
   backend, filesystem-write helpers beyond in-memory model needs, or any
   other `src/` project.
2. `Rules` namespaces (inside `Core`) MUST NOT reference: provider SDK
   types, raw payload types, HTTP clients, renderer namespaces, auth
   namespaces, or LLM types. Rules consume normalized contracts,
   capability states, provenance references, and deterministic
   configuration only (INV-03; INV-04).
3. `Graph` namespaces (inside `Core`) MUST NOT reference: provider SDK
   types, raw payloads, renderer or auth namespaces. The graph projects
   normalized contracts only.
4. `Findings` namespaces (inside `Core`) MUST NOT reference: provider or
   renderer namespaces, or AI/LLM types; MUST NOT recompute rule logic.
5. `EntraNHI.Application` MUST NOT reference: provider SDK types,
   renderer-implementation libraries, CLI framework, web framework,
   AI/LLM SDK, or authentication implementation. It references ports only.
6. `EntraNHI.Infrastructure.Graph` MUST NOT reference:
   `EntraNHI.Output`, `EntraNHI.Cli`, rule-evaluation namespaces for
   decision purposes, renderer namespaces, or AI/LLM SDK. It MUST NOT
   construct findings or verdicts.
7. `EntraNHI.Output` MUST NOT reference: `EntraNHI.Infrastructure.Graph`,
   provider SDK types, authentication material, AI/LLM SDK, or rule-engine
   internals beyond consuming authoritative `RuleEvaluation`/finding data.
   Renderers MUST NOT reference each other's mutable state.
8. `EntraNHI.Cli` MUST NOT be referenced by any other `src/` project. CLI
   parsing types MUST NOT appear outside `EntraNHI.Cli` (except through the
   `Output` CLI-text adapter contract).
9. **No project** in `src/` or `tests/` may introduce Unity-style
   service-location of authentication secrets, ambient credential access, or
   undisclosed network transmission. Test projects MUST NOT carry real
   secrets, production tenant data, or live-service dependencies.

---

## 10. Composition-root ownership

- **V1 composition root: `EntraNHI.Cli`.** Only the CLI host constructs the
  `Application` pipeline with concrete `Infrastructure.Graph`, `Output`,
  authentication-mechanism, configuration-source, and logging-backend
  instances, then starts the run.
- `Application` MUST NOT act as a second composition root: it declares
  required ports via constructor injection (exact injection mechanics are a
  TBD owned by `dependency-boundaries.md`; no container is chosen here) and
  never instantiates adapters, providers, renderers, or auth mechanisms.
- `Core` contains no composition logic: no service registration, no adapter
  selection, no configuration-file reading.
- A future host (see §14) provides its own composition root reusing the
  same `Application`/`Core` without modifying them; composition code is not
  shared by inheritance between hosts.

---

## 11. Microsoft Graph adapter isolation

- **Single provider-coupled project.** `EntraNHI.Infrastructure.Graph` is
  the only production project that knows the Graph communication mechanism
  exists. All Graph-coupled compilation risk (SDK upgrades, HTTP-client
  policy, serialization of provider payloads) is contained here.
- **Port-implemented, core-defined.** The adapter implements collector and
  capability ports whose abstract shapes are owned inward
  (`collector-contracts.md`, `capability-model.md`,
  `dependency-boundaries.md`). The core dictates what the adapter must
  deliver (observations + capability state + provenance + tenant context);
  the adapter never dictates rule or domain shapes outward.
- **No type leakage.** Microsoft Graph-specific types MUST NOT appear in
  normalized core contracts, rule signatures, graph node/edge contracts,
  finding/evidence schemas, or output-model contracts. Provider-specific
  facts cross the boundary only as explicitly normalized
  capability/provenance data (INV-04).
- **Resilience contained.** Pagination, bounded retry/backoff, throttling,
  cancellation, partial-failure shaping, and collection-window timestamping
  live in the adapter; deterministic evaluation downstream treats the
  adapter's output as a dated observation set, never as a live query
  surface.
- **No endpoint/permission invention.** This document specifies no Graph
  endpoints, permissions, scopes, SDK methods, property mappings, or Agent
  ID semantics. Those are TBDs owned by `collector-contracts.md` (endpoints,
  properties, Agent ID mappings after documentation validation),
  `capability-model.md` (capability granularity), and
  `authentication-design.md` (permission rationale), and MUST NOT be
  resolved here by speculation.

---

## 12. Output renderer isolation

- **Single renderer project, four isolated adapters.** `EntraNHI.Output`
  hosts the CLI-text, JSON, SARIF, and HTML adapters behind one stable
  contract each (contracts owned by `output-renderer-contracts.md`).
  Adding or changing one renderer MUST NOT require changes to rule logic,
  domain semantics, collectors, or other renderers.
- **Canonical-model in, format out.** Renderers consume the canonical output
  projection (assessment/rule/finding/evidence/capability/diagnostic data)
  and emit format-specific artifacts. The canonical model is a projection
  of authoritative results, never a second source of truth.
- **Authority firewall.** No renderer may alter, reinterpret, suppress, or
  re-derive evaluation state or severity-into-state; renderer failure is a
  system/output diagnostic, never a tenant finding and never silent success.
- **Untrusted-string discipline.** Every renderer treats tenant/provider
  strings as untrusted data with format-appropriate safe encoding (HTML/XSS,
  ANSI/control, CR/LF, bidi, malformed Unicode, SARIF field safety, JSON
  correctness). Shared helpers are internal to `EntraNHI.Output`.
- **Filesystem discipline.** Path construction, safe-write/atomic-
  publication behavior, overwrite policy, temporary-file handling, and
  artifact-permission posture are renderer obligations defined by
  `output-renderer-contracts.md`; tenant-controlled values never directly
  determine paths. No path library or permission mode is chosen here.

---

## 13. CLI responsibilities and non-responsibilities

**The CLI host does (V1):**

- Parse operator arguments and dispatch commands (exact syntax TBD owned by
  `output-renderer-contracts.md` with configuration semantics from
  `configuration-design.md`).
- Load configuration from operator-supplied sources without embedding
  credentials (format and secure-default semantics owned by
  `configuration-design.md`).
- Establish the execution context (interactive vs workload) by delegating
  to the validated authentication boundary (`authentication-design.md`);
  CLI business/command logic must not directly inspect, persist, log,
  serialize, expose, or embed authentication tokens or credentials;
  authentication/token handling belongs exclusively to the
  authentication/provider boundary defined by `authentication-design.md`;
  transient handling required by that validated implementation is not
  prohibited by this document.
- Compose the pipeline (composition root, §10) and stream results through
  the `EntraNHI.Output` adapters to console and operator-configured files.
- Map outcomes to deterministic machine-usable exit behavior (mapping TBD
  owned by `output-renderer-contracts.md`) and surface incomplete/error
  assessment visibly.

**The CLI host does NOT:**

- Contain security/rule authority: no verdict logic, no applicability or
  capability decisions, no evidence construction, no severity
  reinterpretation.
- Contain provider logic: no Graph calls, no pagination/retry policy, no
  permission requests beyond delegating to the auth boundary.
- Contain renderer authority: no state alteration, no diagnostic
  suppression, no template/injection-policy invention.
- Persist authentication material, embed credentials, or broaden privilege.
- Decide organizational CI gating policy (CI consumes machine-readable
  output; the CLI only preserves explicit incomplete/error conditions).

---

## 14. Future host extensibility

V1 ships the CLI host only (NG-004: no production web dashboard; NG-003: no
SaaS). The structure preserves a future host without requiring it now:

- A future host (e.g., `EntraNHI.Web`, `EntraNHI.Api`, or a library
  facade — names illustrative only, not created or reserved) would be a new
  outer project that references `EntraNHI.Application`, `EntraNHI.Core`,
  and `EntraNHI.Output` (and the provider adapter) through the same ports
  the CLI uses today. No `Core`/`Application`/`Output` modification is
  required to add it.
- The future host MUST NOT become a second rule engine, a second finding
  authority, or a cross-tenant aggregation point. It consumes the canonical
  output model under the same renderer-non-authority, tenant-isolation, and
  AI-non-authority constraints that bind V1 renderers (trust-boundary 15).
- Session security, browser security, persistence, multi-user authorization,
  and any SaaS deployment posture are out of scope for this document and
  would require separate architecture before any host beyond the V1 CLI is
  designed. No SaaS architecture is assumed; no dashboard is a prerequisite
  for the first implementation.
- `implementation-sequence.md` orders V1 work so host-extensibility seams
  (output ports, auth-context ports, configuration surfaces) exist before
  any second host is contemplated.

---

## 15. Testability implications

- **Core tests run provider-free.** `EntraNHI.Core` compiles without
  provider, renderer, host, or auth references, so `EntraNHI.Core.Tests`
  exercises normalization → graph → rules → findings purely from synthetic
  normalized fixtures (determinism, five-state matrix, false-PASS/false-
  FAIL, provenance, error taxonomy).
- **Adapter tests substitute the world.** `Infrastructure.Graph` and
  `Output` tests replace live services with synthetic provider doubles and
  adversarial string/pathological-input fixtures; no live tenant, network,
  or secret is required.
- **Orchestration tests fake the edges.** `Application` tests inject fake
  collectors, capability sources, auth contexts, clocks, and output ports
  to prove propagation (capability → NOT_EVALUATED, failure → ERROR,
  tenant mismatch → visible integrity failure, cancellation → visible
  state) without real I/O.
- **Structure tests guard the seams.** `EntraNHI.Architecture.Tests`
  encodes §§8–9 (plus namespace rules from §7) so a future change that
  smuggles a provider, renderer, AI, or auth-implementation reference into
  `Core` fails the build's test stage rather than relying on review alone.
  Enforcement tooling is a TBD owned by `dependency-boundaries.md`.
- **Full scenario catalog deferred.** The per-seam scenario matrix
  (adversarial, tenant-isolation, injection, failure-injection, golden/
  snapshot, resource-bound) belongs to `testing-seams.md`; this document
  only guarantees the seams exist to attach those scenarios to.

---

## 16. Security implications

Traceable to invariants and threat model; stated as structural support, not
as implemented controls:

- **Read-only posture (INV-01).** No project defines a mutating operation;
  the only network-capable project (`Infrastructure.Graph`) is constrained
  by collector contracts to read-only documented endpoints, and the auth
  boundary never grants mutation-capable scope.
- **Provider isolation (INV-03) / normalized boundary (INV-04).** Physical
  project separation plus §9 namespace rules keep provider types out of
  rule/graph/finding signatures; `Architecture.Tests` intent makes bypass
  structurally visible.
- **Determinism + five states (INV-02; INV-05).** Co-locating domain, graph,
  rules, and findings in one dependency-free `Core` keeps the deterministic
  pipeline auditable as a unit while §7 namespaces prevent authority
  blurring between evaluation and reporting.
- **Capability awareness (INV-07) / failure transparency (INV-14) /
  false-PASS resistance.** Capability/failure state travels the same
  inward path as observations (adapter → ports → `Core`), so evaluation
  always sees the completeness metadata its PASS gating requires.
- **Evidence/provenance (INV-06; INV-11).** The `Findings` namespace sits
  beside `Rules` and `Graph` in `Core`, sharing the provenance vocabulary
  without sharing authority: rules produce evaluations; findings project
  them with evidence — neither fabricates the other's output.
- **Secret exclusion (INV-09) / least privilege (INV-08).** Secrets and
  tokens live only in the transient auth context at the outer boundary
  (`Cli` composition → adapter consumption); they have no type, field, or
  namespace inside `Core`, `Findings`, or `Output` contracts.
- **Core/output separation (INV-12) / renderer non-authority.** A dedicated
  `Output` project with per-renderer namespaces and no back-reference into
  evaluation internals makes renderer-overreach a dependency violation.
- **AI non-authority (INV-13).** No project references an AI/LLM SDK; no
  port admits model output onto the evaluation, authorization, or
  collection path. Any future explanatory AI would attach as a new outer
  consumer of canonical output, never as a `Core` dependency.
- **Tenant isolation (INV-10).** Single-tenant scoping is carried as data
  (tenant assessment context) through every project; cross-project
  contamination is an integrity failure surfaced through the shared
  error/result vocabulary, and cross-tenant edges/evidence are rejected at
  graph and findings construction.
- **Supply-chain and network restraint (CON-003; CON-005).** Dependency
  concentration (third-party surface only in `Infrastructure.Graph`,
  `Output`, and `Cli`) minimizes the audited surface; `Core` and
  `Application` remain dependency-light by construction.

---

## 17. Dependency/package policy

Principles only at this stage. No package, SDK, library, framework, or
version is chosen in this document unless already explicitly approved
elsewhere (none are).

1. **Minimize dependencies.** Each project takes only what its §5
   responsibility requires. `Core` and `Application` take the least;
   provider, rendering, and hosting dependencies concentrate in
   `Infrastructure.Graph`, `Output`, and `Cli` respectively.
2. **Use maintained packages.** When `dependency-boundaries.md` and later
   build controls select packages, they MUST prefer actively maintained,
   compatibly licensed options and record the rationale.
3. **Pin and review through future build controls.** Versions, lockfiles,
   hashes, and update policy are owned by later build-control work aligned
   with the approved supply-chain posture — not by this document. No
   version is stated here.
4. **No unnecessary SDKs.** No AI/LLM SDK, web framework, Graph SDK beyond
   the single adapter's need, or extra telemetry/utility SDK anywhere
   without explicit approval (CON-003).
5. **No secrets in configuration or repository.** Dependency choices MUST
   NOT require credential-bearing configuration, checked-in secrets, or
   secret-bearing sample files (SEC-002; SEC-009).
6. **Supply-chain alignment.** Future SBOM, signing, pinning, scanning, and
   release-provenance controls MUST align with the approved architecture
   (threat-model path 29; testing-architecture §18) once that tooling is
   selected. This document claims none of those controls exist.

---

## 18. Build/release implications

- **Build order follows dependency direction:** `Core` → `Application` →
  `Infrastructure.Graph` + `Output` (either order) → `Cli`; tests mirror
  their production projects with `Architecture.Tests` last as a structural
  gate. Exact target frameworks, LangVersion, nullable/analysis settings,
  strong-naming/signing, packaging (NuGet/tool distribution), and CI
  workflow definitions are TBDs owned by later build-control work through
  `dependency-boundaries.md` (seam/enforcement needs) and
  `implementation-sequence.md` (ordering/gates).
- **No build claims now.** This document defines no SDK version, no
  `dotnet` command, no CI gate, and no release artifact. It creates no
  build scripts and runs no scaffolding.
- **Release-gate compatibility.** The structure is compatible with the
  testing-architecture release gates (read-only, least-privilege, tenant-
  isolation, secret-exclusion, determinism, false-PASS/false-FAIL,
  capability-awareness, provenance, renderer-non-authority, injection
  resistance, filesystem safety, bounded processing — testing-architecture
  §20) because each gate maps to at least one project plus its test
  project. Gate *evidence* belongs to tests, not to this document.

---

## 19. Explicit non-goals

The following are explicitly NOT part of this structural proposal (each
traces to requirements non-goals or architecture out-of-scope):

1. No SaaS hosting, multi-tenant service, or cross-tenant aggregation
   (NG-003).
2. No production web dashboard or user-facing web interface (NG-004); §14
   reserves extensibility only.
3. No remediation, credential rotation/lifecycle operations, permission or
   policy modification, or identity create/delete/disable (NG-001; NG-002;
   NG-007; SEC-003–SEC-007).
4. No autonomous or AI-driven security decisions or AI-generated verdicts
   (NG-005; SEC-010; VERD-007).
5. No real-time monitoring, alerting, or event-driven analysis (NG-008).
6. No replacement of Entra administration/governance products (NG-006).
7. No per-collector, per-rule, per-renderer, or per-tenant micro-assemblies;
   namespace organization ( §7) is used where a project boundary is not
   justified by §3.
8. No Graph endpoint/permission/property selection, no Agent ID mapping, no
   licensing-behavior claims, no SDK/method choices, no package versions, no
   schema finalization, no CLI syntax/exit-code finalization, no SARIF
   mapping, no HTML templating/CSP choice, no OAuth-flow choice — all
   deferred as TBDs to their owning documents.

---

## 20. Open implementation TBDs

Each TBD names the later Phase 0.3 document responsible for resolving it.
None is resolved here.

| # | TBD | Owned by |
| --- | --- | --- |
| T-01 | Exact C# namespace names and folder layout within each project (§7 is logical only). | `core-contracts.md` with `dependency-boundaries.md` |
| T-02 | Concrete interface/type shapes for ports, envelopes, domain records, graph, rules, findings, errors, and output model (this document names responsibilities, not members). | Respective contract document (`collector-contracts.md`, `domain-types.md`, `capability-model.md`, `identity-graph-contracts.md`, `rule-engine-contracts.md`, `findings-evidence-schema.md`, `error-result-model.md`, `output-renderer-contracts.md`) coordinated by `core-contracts.md` |
| T-03 | Graph SDK vs direct HTTP decision; pagination/retry/timeout/concurrency constants; caching strategy. | `collector-contracts.md` with `dependency-boundaries.md` |
| T-04 | Exact Graph endpoints, permission/scope names and rationale, property mappings, Agent ID endpoint/property/relationship/blueprint mappings, managed-identity classification mapping, optional ARM-enrichment scope. Require published Microsoft documentation validation; MUST NOT be invented. | `collector-contracts.md` (endpoints/properties/mappings), `authentication-design.md` (permission rationale), `capability-model.md` (capability granularity) |
| T-05 | `AuthorizedAccessContext` implementation shape; OAuth/OIDC flows; interactive vs workload mechanisms; token-cache/storage policy; tenant-validation mechanics; consent UX; sovereign-cloud endpoints. | `authentication-design.md` |
| T-06 | Canonical output projection shape; JSON schema and versioning; SARIF version/profile/mapping; HTML templating/CSP/asset strategy; CLI syntax and exit-code mapping; terminal/file-safety policies; redaction schema; renderer-failure isolation policy. | `output-renderer-contracts.md` (with `configuration-design.md` for output options) |
| T-07 | Configuration-file format, option catalog, secure defaults, unknown/invalid-configuration behavior. | `configuration-design.md` |
| T-08 | Capability-state names/serialization/schema and per-category granularity. | `capability-model.md` |
| T-09 | Identifier formats (assessment, evaluation, finding, evidence, provenance, identity keys); finding-emission policy; fingerprint/dedup strategy; hashing/signing/timestamping posture. | `findings-evidence-schema.md` (with `domain-types.md` for identity keys) |
| T-10 | Assessment-time representation; rule ordering/parallelism; fatal-vs-isolated failure policies; resource limits; severity taxonomy; rule registration/version mechanics. | `rule-engine-contracts.md` (with `error-result-model.md` for failure mapping) |
| T-11 | Graph query/traversal API shape; graph serialization for diagnostics; graph size/memory policy. | `identity-graph-contracts.md` |
| T-12 | Dependency-injection mechanics/container choice; logging/telemetry abstraction surface; `Architecture.Tests` enforcement tooling and rule set; target frameworks, build strictness, signing, packaging, SBOM/scanning workflow. | `dependency-boundaries.md` (with `implementation-sequence.md` for ordering) |
| T-13 | Per-seam fakes/mocks/fixtures, adversarial scenario catalog, golden/snapshot policy, test-framework choice. | `testing-seams.md` |
| T-14 | Build order, stage gates, and which TBDs block which stages. | `implementation-sequence.md` |

---

## 21. Acceptance criteria

The structure proposed here is accepted when:

1. **Boundary coverage.** Every approved pipeline responsibility — domain/
   core model, application orchestration, Graph collection adapter, rule/
   evaluation engine, identity graph, findings/evidence, output rendering,
   CLI host, future web/API host path, and automated tests — has an explicit
   home in §4–§7 with no orphan responsibility and no unjustified
   micro-assembly.
2. **Dependency soundness.** References form the DAG in §8; every forbidden
   reference in §9 is stated; Core depends on no `src/` project and on none
   of the §9 forbidden dependency classes; no circular dependency exists.
3. **Authority placement.** Rule authority lives in `Core.Rules`, evidence
   authority in `Core.Findings`, projection-only rendering in `Output`, and
   no authority in `Cli`, `Application` routing, adapters, or any future AI
   consumer — satisfying INV-02/INV-05/INV-06/INV-12/INV-13 structurally.
4. **Adapter isolation.** Graph-coupled code is confined to
   `Infrastructure.Graph` with no Graph types in core contracts (§11);
   renderer-coupled code is confined to `Output` with no renderer types in
   core (§12); CLI holds composition only (§10, §13).
5. **Host extensibility.** A future host can be added as a new outer project
   on existing ports without modifying `Core`/`Application`/`Output`, and
   without assuming SaaS or requiring a dashboard for V1 (§14).
6. **Testability.** Each production project has a mirroring test project
   plus structural enforcement (§6); `Core` is testable provider-free;
   seams exist for every testing-architecture layer `testing-seams.md`
   must cover (§15).
7. **Security alignment.** Sections 16–17 preserve least privilege, secret
   exclusion, tenant isolation, provenance, failure transparency,
   dependency restraint, and network restraint as structural properties
   without claiming any control is implemented.
8. **TBD explicitness.** Every implementation-sensitive unknown is listed in
   §20 with its owning later document; no Graph endpoint, permission,
   property, Agent ID mapping, package version, or numeric limit is invented
   in this document.
9. **Non-goal containment.** Nothing in §19 appears as an assumed
   capability of the proposed tree.

---

## Appendix — Project-boundary tradeoff considered and decided

| Option | Shape | Verdict |
| --- | --- | --- |
| A. Minimal (2–3 assemblies: e.g., `Core` + `Cli`, or `Core` + `Infra` + `Cli`) | Fewest projects; renderers, orchestration, and possibly Graph code share assemblies with core or host. | **Rejected.** Merges forbidden dependency classes into the core or host assembly (renderer libraries, Graph SDK, or CLI framework adjacent to rule logic), making INV-03/INV-04/INV-12 enforcement a review convention rather than a structural property; discourages independent renderer/host evolution. |
| B. Maximal (per-component assemblies: separate `Rules`, `Graph`, `Findings`, `Renderers/Json/Sarif/Html`, per-collector projects) | Finest granularity. | **Rejected.** Satisfies none of the §3 counter-criterion protections at this scale: versioning/signing/review overhead without additional invariant enforcement beyond what §7 namespaces plus `Architecture.Tests` already provide; rule and renderer additions would become assembly-management events. |
| C. Balanced (5 production + 6 test projects, §4) | Core authority, application coordination, one Graph adapter, one renderer project with internal per-format namespaces, thin CLI host; graph/rules/findings as explicit `Core` namespaces; structural tests as sixth test project. | **Adopted.** Each assembly boundary corresponds to at least one §3 criterion (dependency risk, authority, cadence, seam, containment); internal §7 namespaces keep rule/graph/finding/renderer roles explicit without fragmentation; the tree supports V1 CLI now and a future host later with no core rework. |

The starting direction sketched in the task (4 production + 5 test projects
with no dedicated output project) was assessed against the same criteria:
without an `Output` project, renderer libraries and filesystem/injection
policy would live in `Cli` or `Core` — coupling machine-readable output and
future-host reuse to the CLI host, or contaminating the dependency-free
core. The adopted option C therefore promotes rendering to the first-class
`EntraNHI.Output` project and adds the corresponding `EntraNHI.Output.Tests`
and `EntraNHI.Architecture.Tests` projects. All other aspects of the
starting direction (Core/Application/Graph/Cli separation and inward-only
core dependencies) are preserved.

(End of file)
