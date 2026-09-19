# Dependency Boundaries

> **Phase:** 0.3.10
> **Package:** Implementation Design
> **Status:** Proposed — design only, no code or project files created
> **Date:** 2026-09-18

---

## 1. Status and scope

- **Status:** Proposed implementation-level dependency rules. This document
  is design output only. It authorizes no implementation.
- **Scope:** Defines the enforceable implementation-level dependency
  boundaries for the approved EntraNHI solution structure. It translates
  the project topology and dependency direction established by
  `solution-structure.md` §§4–10 into explicit allowed/prohibited
  project-reference rules, conceptual placement rules, and enforcement
  intent, without redesigning the topology.
- **What this document is:** The owner of the per-seam dependency catalog:
  which project may reference which, which conceptual concern belongs
  where, where provider types and secret material may never flow, and how
  those constraints will later be enforced (`README.md` §§5–6). Structural
  ownership of the project tree remains with `solution-structure.md`; in
  case of apparent conflict, the forbidden-dependency rule (deny) governs
  until both documents are reconciled through change control.
- **What this document is not:** It is not a type catalog, interface
  specification, Graph binding, permission list, package selection, build
  script, test suite, or implementation sequence. Detailed shapes belong
  to their owning documents: `core-contracts.md` (shared core surface),
  `domain-types.md` (normalized domain types), `collector-contracts.md`
  (collector interfaces), `capability-model.md` (capability states),
  `identity-graph-contracts.md` (graph surface),
  `rule-engine-contracts.md` (rule pipeline and evaluation surface),
  `findings-evidence-schema.md` (findings/evidence/provenance),
  `error-result-model.md` (error/result taxonomy),
  `output-renderer-contracts.md` (canonical output model and renderers),
  `authentication-design.md` (authentication/provider boundary),
  `configuration-design.md` (configuration surfaces),
  `testing-seams.md` (test scenario catalog), and
  `implementation-sequence.md` (build order).
- **Language discipline:** Per `README.md` §1, this document distinguishes
  (a) **approved architectural requirements** traced to requirement/
  invariant IDs, (b) **proposed dependency rules** owned by this document,
  and (c) **unresolved TBDs** naming the owning later concern (§20).
  Nothing herein claims a boundary is implemented, tested, or enforced.
  Where architecture states `DESIGNED / REQUIRED`, this document preserves
  that classification.
- **Canonical assessment states (referenced, not redefined):** The
  authoritative assessment states remain EXACTLY `PASS`, `FAIL`,
  `NOT_EVALUATED`, `NOT_APPLICABLE`, `ERROR`, with semantics owned by
  `core-contracts.md` §4 and the rule pipeline owned by
  `rule-engine-contracts.md`. No sixth authoritative assessment state is
  created here. Operational terms used below (for example, collection
  incompleteness, authorization limitation, provider failure,
  cancellation, resource exhaustion) are explicitly typed in their correct
  layer per `error-result-model.md` and `capability-model.md` — never as
  assessment states.
- **Provider discipline:** No Microsoft Graph endpoint, permission/scope
  name, SDK type or method, Entra property, Agent Identity mapping,
  licensing-behavior claim, or undocumented provider relationship is
  invented or selected here (INV-15). Provider references below mean "the
  Graph communication mechanism (TBD owned by `collector-contracts.md`
  with this document)" and nothing more specific.
- **Package discipline:** No framework, library, SDK, test-framework,
  structural-test tooling, logging backend, dependency-injection
  container, or version is selected here (CON-003). Enforcement mechanisms
  are described as capabilities with TBD tooling (§§17, 20).

---

## 2. Purpose

Dependency direction is a **security, determinism, maintainability, and
provider-isolation control** — not merely source organization.

1. **Security control.** The topology keeps credential material, provider
   parsing risk, renderer injection risk, and host parsing risk at the
   outer boundary and out of the authoritative core. A forbidden reference
   (for example, a provider SDK type imported into rule evaluation) is a
   security-boundary breach, not a style issue (INV-03; INV-04; INV-09;
   trust-boundary architecture).
2. **Determinism control.** The deterministic pipeline (normalized inputs
   + capability state + rule version/configuration + deterministic context
   → one of five states) is auditable only if evaluation cannot reach
   outward for live provider state, wall-clock time, ambient
   configuration, randomness, or model inference. Inward-only dependencies
   make that property structurally visible (INV-02; INV-05).
3. **Maintainability control.** Stable inward contracts (`Core`) change for
   domain/semantic reasons; volatile outer concerns (provider
   communication, rendering libraries, CLI parsing) change for external
   reasons. The DAG direction ensures outer volatility never forces core
   rework, and core evolution never silently breaks outer adapters except
   through explicit contract change (change-control policy,
   `README.md` §8).
4. **Provider-isolation control.** Confining all provider-coupled
   compilation risk to a single adapter project makes provider upgrades,
   serialization quirks, and throttling policy local events. Rules, graph
   projections, findings, and renderers cannot observe provider evolution
   except through explicitly normalized capability/provenance data
   (INV-03; INV-04).

Accordingly, every rule in §§3–16 is stated as a **MUST / MUST NOT**
constraint on future implementation, with enforcement intent in §17 and
violation handling in §18.

---

## 3. Dependency principles

The following principles are carried forward from requirements,
architecture, and `solution-structure.md` §§8–9. They are constraints,
not claims of implemented behavior.

1. **Inward dependency direction.** Dependencies flow inward toward
   stability: outer adapters and the host depend on inward contracts;
   inward projects never reach outward for data, behavior, or
   configuration. The dependency graph is a DAG rooted at `Core` (§15).
2. **Core as provider/I/O/presentation independent.** `EntraNHI.Core`
   compiles and is testable with zero references to any other production
   project and zero references to provider, I/O-implementation,
   presentation, authentication-implementation, or AI/LLM dependencies
   (§4). It owns normalized assessment authority and nothing else.
3. **Application as orchestration/use-case layer.** `EntraNHI.Application`
   coordinates the approved assessment use case through ports defined
   inward. It holds no rule, provider, renderer, secret, or composition
   authority (§5).
4. **Infrastructure.Graph as provider adapter.** All Microsoft
   Graph-coupled implementation lives in `EntraNHI.Infrastructure.Graph`
   and only there. It implements inward-defined ports and normalizes
   provider observations into inward contracts. It decides no verdict
   (§6).
5. **Output as projection/rendering adapter.** `EntraNHI.Output` projects
   authoritative inward results into CLI/text, JSON, SARIF, and HTML
   artifacts. It evaluates no rules, calls no provider, authenticates
   nothing, and mutates no verdict (§7).
6. **Cli as V1 host and sole approved composition root.** Only
   `EntraNHI.Cli` wires concrete adapters to ports and starts the run. No
   other production project composes the object graph (§8; §15 of
   `solution-structure.md` intent preserved).
7. **No circular production dependencies.** No reference cycle among
   production projects, and no conceptual back-reference (for example, a
   core contract importing an adapter type, or an adapter dictating a
   domain shape outward) that effectively inverts the architecture, is
   permitted (§15).

Where existing Phase 0.3 documentation is more precise than this summary
— notably `solution-structure.md` §§5–9, `core-contracts.md`,
`authentication-design.md`, and `collector-contracts.md` — that
documentation governs and this document references it rather than
restating it normatively.

---

## 4. Explicit production dependency matrix

Allowed project references (normative; derived from
`solution-structure.md` §8). "A → B" means A may reference B. Anything
not listed as allowed is prohibited.

| # | Reference | Permission | Rationale / owner |
| --- | --- | --- | --- |
| R-01 | `EntraNHI.Core` → (no other production project) | **Prohibited in all cases** | Core independence (INV-02; INV-03; INV-04; CON-003). See §4. |
| R-02 | `EntraNHI.Application` → `EntraNHI.Core` | **Allowed** | Orchestration consumes inward contracts and ports. See §5. |
| R-03 | `EntraNHI.Application` → `EntraNHI.Infrastructure.Graph` | **Prohibited** | Application depends on abstractions (ports) only; never on a concrete adapter. |
| R-04 | `EntraNHI.Application` → `EntraNHI.Output` | **Prohibited** | Application depends on output ports only; never on renderer implementation. |
| R-05 | `EntraNHI.Application` → `EntraNHI.Cli` | **Prohibited** | Host is outer; nothing depends on the host. |
| R-06 | `EntraNHI.Infrastructure.Graph` → `EntraNHI.Core` | **Allowed** | Adapter consumes normalized contract vocabulary and observation-handoff shapes. See §6. |
| R-07 | `EntraNHI.Infrastructure.Graph` → `EntraNHI.Application` contracts | **Allowed only where the seam catalog requires it** | Where a collector/capability/auth-context port is owned by `Application.Ports` (per `collector-contracts.md`, `authentication-design.md`, `dependency-boundaries.md` seam placement), the adapter may reference that port contract. It MUST NOT reference orchestrator logic, pipeline behavior, or any concrete `Application` implementation beyond the port. Where the port is owned in `Core`, R-06 applies instead. |
| R-08 | `EntraNHI.Infrastructure.Graph` → `EntraNHI.Output` | **Prohibited** | Provider adapter never renders. See §6. |
| R-09 | `EntraNHI.Infrastructure.Graph` → `EntraNHI.Cli` | **Prohibited** | Adapter never depends on the host. |
| R-10 | `EntraNHI.Output` → `EntraNHI.Core` | **Allowed** | Renderers consume authoritative inward results and the canonical output-model vocabulary. See §7. |
| R-11 | `EntraNHI.Output` → `EntraNHI.Application` contracts | **Allowed only where the seam catalog requires it** | Where an output port is owned by `Application.Ports` (per `output-renderer-contracts.md` with this document), the renderer project may reference that port contract. It MUST NOT reference orchestrator logic or pipeline behavior. |
| R-12 | `EntraNHI.Output` → `EntraNHI.Infrastructure.Graph` | **Prohibited** | Renderers never call providers. See §7. |
| R-13 | `EntraNHI.Output` → `EntraNHI.Cli` | **Prohibited** | Renderer project never depends on the host; the host depends on renderers. |
| R-14 | `EntraNHI.Cli` → `EntraNHI.Application` | **Allowed** | Host composes the pipeline. See §8. |
| R-15 | `EntraNHI.Cli` → `EntraNHI.Infrastructure.Graph` | **Allowed** | Host wires the concrete provider adapter. See §8. |
| R-16 | `EntraNHI.Cli` → `EntraNHI.Output` | **Allowed** | Host wires concrete renderers. See §8. |
| R-17 | `EntraNHI.Cli` → `EntraNHI.Core` | **Allowed** | Host references inward vocabulary (tenant context, configuration validation results, exit-code mapping inputs) as established by `solution-structure.md`. Host MUST NOT use this reference to bypass `Application` orchestration or to reinterpret verdicts. |
| R-18 | Any production project → `EntraNHI.Cli` | **Prohibited** | `Cli` is referenced by nothing. CLI parsing types MUST NOT appear outside `Cli` except through the `Output` CLI-text adapter contract. |
| R-19 | Any new production project reference | **Prohibited** | No new production project is introduced in V1. Future adapters/hosts require approved requirements/architecture change first (§19). |

Text form of the DAG (identical to `solution-structure.md` §8):

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

No new production project is introduced by this document.

---

## 5. Core prohibited dependencies

`EntraNHI.Core` MUST NOT depend on or contain any of the following,
whether by project reference, package reference, namespace import, type
usage, or conceptual reliance:

1. Microsoft Graph SDK / provider SDK types, provider HTTP clients,
   provider payload types, or provider serialization helpers.
2. HTTP/network transport of any kind.
3. Authentication or token-acquisition implementation (flows, caches,
   stores, credential helpers); the consumable authorized-context
   abstraction is defined by `authentication-design.md` and consumed
   outside `Core`.
4. Filesystem/storage implementation (readers, writers, path helpers
   beyond in-memory model needs).
5. CLI/presentation framework types.
6. Renderer implementation (CLI-text/JSON/SARIF/HTML libraries or
   helpers).
7. Logging/telemetry implementation (concrete backends, sinks,
   exporters). A logging/telemetry abstraction surface, if any, is a TBD
   (§20, T-05); even if introduced later, the concrete backend MUST NOT
   live in or be referenced by `Core`.
8. Environment/configuration-source implementation (file readers,
   environment-variable readers, secret stores). `Core` consumes
   validated, normalized/effective configuration contracts only (shapes
   owned by `configuration-design.md`); it never reads configuration
   sources.
9. Dependency-injection container implementation. `Core` declares no
   service registration and performs no adapter selection.
10. AI/LLM provider implementation (model SDKs, inference clients,
    prompt/rendering helpers, embedding stores).

Clarification — abstract inward-facing contracts are NOT prohibited
merely because an outer adapter implements them. `Core` (and its
consumers) may define or consume abstractions such as collector ports,
capability observations, authorized-context references, output-neutral
result shapes, clock/ordering references, and logging/telemetry
abstraction surfaces, provided the implementation lives outside `Core`
and no implementation type leaks inward (§§9–10). The prohibition is on
outward implementation dependence, not on inward-owned abstraction.

Within `Core`, the namespace rules from `solution-structure.md` §7 are
preserved: `Rules` MUST NOT reference provider/renderer/auth namespaces;
`Graph` MUST NOT reference provider/renderer/auth namespaces; `Findings`
MUST NOT reference provider/renderer/AI namespaces and MUST NOT
recompute rule logic.

---

## 6. Application boundary

`EntraNHI.Application` coordinates the approved assessment use case and
the ports outer adapters implement. Port shapes belong to their owning
documents (`collector-contracts.md`, `authentication-design.md`,
`output-renderer-contracts.md`, `core-contracts.md` накопительно with
this document for placement); this document creates no interfaces.

1. **Authority prohibition.** `Application` MUST NOT become:
   - a second rule engine (no verdict recomputation, no applicability or
     capability reinterpretation, no severity-into-state mapping);
   - a provider implementation (no provider calls, pagination, retry
     policy, or permission requests beyond delegating through ports);
   - an output renderer (no state alteration, diagnostic suppression, or
     template/injection-policy invention);
   - an authentication-secret owner (no credential/token inspection,
     persistence, logging, serialization, or exposure; see §12 and
     `authentication-design.md`);
   - an alternative composition root (no adapter instantiation; required
     ports are received as abstractions; exact injection mechanics are a
     TBD in §20, T-05 — no container is chosen here).
2. **Semantic non-reinterpretation.** `Application` MUST NOT reinterpret
   canonical assessment semantics. It routes authoritative `Core`
   results: exactly `PASS`, `FAIL`, `NOT_EVALUATED`, `NOT_APPLICABLE`,
   `ERROR` with their evidence/reason payloads intact. It MUST NOT
   suppress `ERROR`/`NOT_EVALUATED`, coerce missing data to `PASS`/`FAIL`,
   convert `ERROR` to `FAIL`, or fabricate evidence/provenance.
3. **Tenant scoping.** `Application` carries the single-tenant execution
   scope for the run and ensures cross-tenant mixing fails safely and
   visibly through the shared error/result vocabulary (see §13).
4. **Failure propagation.** Capability state and structured failures
   (collection, normalization, graph, rule, evidence, output, auth,
   cancellation, resource exhaustion) propagate at every orchestration
   step with their scope identified; mapping of failures to
   `NOT_EVALUATED` vs `ERROR` follows `error-result-model.md` and is
   referenced — not redefined — here.

---

## 7. Infrastructure.Graph boundary

`EntraNHI.Infrastructure.Graph` is the only established Microsoft
Graph-coupled production project.

1. **Provider containment.** All provider SDK/API/payload types, HTTP
   semantics, pagination mechanics, bounded retry/backoff, throttling
   handling, cancellation cooperation, and raw-response parsing remain
   inside this project. No exact endpoint, permission, SDK type, or
   payload claim is made here (TBD owned by `collector-contracts.md`
   with `capability-model.md` / `authentication-design.md`).
2. **Normalization obligation.** The adapter MUST translate provider
   observations into inward-facing contracts: normalized observations +
   per-category capability state + provenance + tenant context, per the
   collector-to-normalization handoff owned by `collector-contracts.md`.
   It MUST preserve and never silently drop:
   - completeness (complete, confirmed-empty, partial, indeterminate,
     failed, with the missing subset and cause identified);
   - capability limitations (authorization-limited, licensing/service-
     limited, unsupported, unknown);
   - authorization limitations (denials as explicit limitations, never as
     emptiness);
   - provider failures (explicit, scoped, sanitized);
   - provenance (source system, object reference, collection
     operation/context, assessment context);
   - tenant scope (every observation carries the tenant assessment
     context; see §13).
3. **Verdict prohibition.** The adapter MUST NOT decide `PASS`/`FAIL`,
   construct findings or verdicts, reinterpret severity into state, or
   reference rule-evaluation namespaces for decision purposes.
4. **Reference prohibitions.** The adapter MUST NOT reference
   `EntraNHI.Output`, `EntraNHI.Cli`, renderer namespaces, or AI/LLM
   types. It consumes the authorized context supplied through the
   authentication/provider boundary; it owns no authentication-mechanism
   choice.
5. **Secret discipline.** The adapter MUST NOT pass secrets into the
   domain model, findings, evidence, results, logs, or diagnostics (see
   §12).

---

## 8. Output boundary

`EntraNHI.Output` depends on authoritative inward results and projects
them into the four V1 artifacts: CLI/text, JSON, SARIF, HTML (contracts
owned by `output-renderer-contracts.md`; this document creates no
renderer contracts).

1. **Projection only.** Each renderer consumes the canonical output model
   (a projection of authoritative results) and emits format-specific
   artifacts. Renderers MUST NOT evaluate rules, call Graph,
   authenticate, mutate verdicts, reinterpret severity into state,
   fabricate evidence/provenance, suppress diagnostics, or perform
   remediation.
2. **State preservation.** All five states (`PASS`, `FAIL`,
   `NOT_EVALUATED`, `NOT_APPLICABLE`, `ERROR`) MUST survive every
   renderer with their evidence/reason payloads intact. Renderer failure
   is a system/output diagnostic — never a tenant finding and never
   silent success.
3. **Renderer isolation.** Renderers MUST NOT reference each other's
   mutable state, MUST NOT reference `Infrastructure.Graph`, provider
   types, authentication material, AI/LLM types, or rule-engine internals
   beyond consuming authoritative evaluation/finding data.
4. **Untrusted-string and filesystem obligations.** Tenant/provider-
   originated strings are untrusted in every renderer (encoding
   obligations owned by `output-renderer-contracts.md`); filesystem
   writes occur only through validated operator-configured destinations
   with safe-write/partial-artifact behavior defined there.
   Tenant-controlled values MUST NOT directly determine paths.

---

## 9. CLI boundary

`EntraNHI.Cli` is the V1 host and sole approved composition root.

1. **Composition responsibilities.** The CLI host:
   - parses operator arguments and dispatches commands (exact syntax TBD
     owned by `output-renderer-contracts.md` with configuration semantics
     from `configuration-design.md`);
   - loads configuration from operator-supplied sources without embedding
     credentials (format and secure-default semantics owned by
     `configuration-design.md`);
   - establishes the execution context by delegating to the validated
     authentication boundary (`authentication-design.md`);
   - constructs the `Application` pipeline with the concrete
     `Infrastructure.Graph` and `Output` adapters and the validated
     authentication mechanism, then starts the run;
   - streams results through the `Output` adapters and maps outcomes to
     deterministic machine-usable exit behavior (mapping owned by
     `output-renderer-contracts.md`), surfacing incomplete/error
     assessment visibly.
2. **Business-logic prohibition.** The CLI holds no security/rule
   authority: no verdict logic, no applicability or capability decisions,
   no evidence construction, no severity reinterpretation; no provider
   logic; no renderer authority; no organizational CI gating-policy
   decisions.
3. **Authentication wording (preserved).** The following established
   wording is preserved without alteration:
   > CLI business/command logic must not directly inspect, persist, log,
   > serialize, expose or embed authentication tokens/credentials.
   > Authentication/token handling belongs exclusively to the
   > authentication/provider boundary defined by
   > `authentication-design.md`.
   > Do not prohibit transient handling required by a future validated
   > implementation at the proper boundary.

   Concretely: transient handling required by the future validated
   implementation at the proper boundary (the authentication/provider
   boundary) is not prohibited by this document; all other inspection,
   persistence, logging, serialization, exposure, or embedding of
   authentication tokens/credentials by CLI business/command logic is
   prohibited.
4. **Reference rule.** `Cli` is referenced by nothing (§4, R-18).

The CLI is kept thin by construction: business logic that would require
`Cli` to interpret verdicts, query providers, invent rendering policy,
or own authentication policy is a boundary violation (§18), not a host
feature.

---

## 10. Cross-cutting implementation placement

For each conceptual concern, the table states where authority belongs
and where it is prohibited. No new project is introduced to place these
concerns; placement uses only the five established production projects.

| Concern | Authority belongs in | Prohibited in |
| --- | --- | --- |
| Normalized domain types (`IdentityRecord`, kinds, credential-metadata non-secret, permission/access, accountability, tenant context, absent/unknown semantics; shapes owned by `domain-types.md`) | `Core` (`Domain` namespace intent) | `Infrastructure.Graph` (beyond handoff shaping), `Output`, `Cli`, `Application` logic (consumes only) |
| Identity graph (node/edge construction, observed-vs-derived classification, traversal/projection, INV-G1–INV-G8; surface owned by `identity-graph-contracts.md`) | `Core` (`Graph` namespace intent) | Provider/renderer/auth namespaces; `Infrastructure.Graph` beyond source-observation shaping; `Output`; `Cli` |
| Deterministic rule engine (definitions, pipeline stages, evaluation, state assignment, isolation; surface owned by `rule-engine-contracts.md`) | `Core` (`Rules` namespace intent) | `Application`, `Infrastructure.Graph`, `Output`, `Cli`, AI consumers |
| Findings/evidence (evaluation-vs-finding distinction, evidence references, provenance chain, emission/redaction hooks; schemas owned by `findings-evidence-schema.md`) | `Core` (`Findings` namespace intent) | `Infrastructure.Graph` (never constructs findings), `Output` (projects only), `Cli`, `Application` routing |
| Capability semantics (state vocabulary, granularity, propagation; owned by `capability-model.md`) | Vocabulary in `Core` (`Capabilities` namespace intent); observations produced by `Infrastructure.Graph`; orchestration propagation in `Application` | Rules MUST NOT invent capability states; renderers/CLI MUST NOT reinterpret them |
| Assessment/result semantics (five states, aggregation; owned by `core-contracts.md` §4 with `rule-engine-contracts.md`) | `Core` | Collectors, adapters, loaders, renderers, CLI, AI components (see §14) |
| Orchestration (authenticate → capabilities → collect → normalize → graph → evaluate → findings → output-neutral results) | `Application` (`UseCases`/`Pipeline` intent) | `Core` (no composition), adapters (no orchestration), `Cli` beyond composition |
| Collector/provider implementation (query execution, pagination, retry, mapping to handoff) | `Infrastructure.Graph` | `Core`, `Application` logic, `Output`, `Cli` |
| Authentication/provider boundary (execution contexts, authorized-context abstraction, per-capability authorization, secret handling, tenant establishment; owned by `authentication-design.md`) | Outer boundary consumed via composition (`Cli` wires; `Infrastructure.Graph` consumes; `Application` carries the reference without inspection) | `Core` (no inspection), domain/graph/rules/findings/evidence/results, all renderers, logs/telemetry, test fixtures (see §12) |
| Configuration loading (reading operator-supplied sources) | `Cli` (`Configuration` loading intent only) | `Core` (never reads sources), `Application` (consumes validated contracts only), adapters beyond their own operational needs |
| Normalized/effective configuration contracts (validated options, rule-set selection, secure defaults; owned by `configuration-design.md`) | Vocabulary referenced in `Core`; validation/authority per `configuration-design.md` with `rule-engine-contracts.md` | Unvalidated source data MUST NOT reach evaluation; unknown/invalid options MUST fail visibly |
| Output rendering (four V1 adapters + internal encoding/file helpers) | `Output` (one namespace per renderer) | `Core`, `Application`, `Infrastructure.Graph`, `Cli` business logic |
| Persistence/write boundary where later required (safe-write, atomic publication, artifact permissions; policy owned by `output-renderer-contracts.md`) | `Output` internals and `Cli` destination wiring | `Core` (no filesystem writes beyond in-memory model construction), `Application` routing, `Infrastructure.Graph` |
| Telemetry/logging adapter concerns | Implementation (if any) lives outside `Core` at the outer boundary; abstraction surface (if any) is a TBD (§20, T-05) | `Core` MUST NOT reference a concrete backend; secret-bearing values MUST NOT enter logs/telemetry (see §12) |
| Composition (object-graph wiring) | `Cli` only (V1 composition root) | `Core`, `Application`, `Infrastructure.Graph`, `Output` (see §9, R-19) |

---

## 11. Provider type leakage

Graph SDK/API/raw payload types MUST NOT cross into any of the
following:

- `Core` contracts (domain records, graph nodes/edges, capability
  vocabulary, provenance references, configuration contracts);
- findings/evidence schemas;
- rule evaluation (definitions, inputs, evaluation surface,
  `RuleEvaluation` construction);
- output-neutral authoritative results (including the canonical output
  model consumed by renderers).

Provider-specific facts cross the normalization boundary only as
explicitly normalized capability/provenance data (INV-04): the adapter
asserts what was observed, how completely, under which capability state,
with which source references — never the provider object itself.

Future enforcement is architectural, not ad hoc: project-reference
review, namespace/type leakage checks where appropriate, architecture
tests, build/CI checks, and code review (§17). No specific enforcement
package is selected here (TBD §20, T-06).

---

## 12. Secret boundary

Credentials, access tokens, refresh tokens, client secrets, private
keys, passwords, recovery codes, and any other secret-bearing values
MUST NOT flow into:

- `Core` (including the normalized domain model, graph, rules,
  findings, evidence, results, diagnostics consumed as assessment data);
- rule inputs or rule results;
- findings/evidence schemas or provenance records;
- output-neutral results or the canonical output model;
- renderer inputs or emitted artifacts (all four renderers, console and
  files);
- logs or telemetry (including diagnostics, errors, and sanitized-event
  envelopes — sanitization obligations per `authentication-design.md`
  §§5–6, 18 and `error-result-model.md`);
- test fixtures, factories, golden artifacts, or test configuration.

Authentication material remains confined to the approved
authentication/provider boundary: transient, runtime-only handling at the
outer boundary as defined by `authentication-design.md`, wired through
the `Cli` composition root and consumed by the provider adapter. No
storage mechanism, cache policy, flow, or SDK is selected here; exact
flows remain TBD until reviewed (see `implementation-sequence.md` for
sequencing). Repository-stored credentials, secret-bearing sample files,
and credential-bearing configuration are prohibited by repository policy
and by `solution-structure.md` §17.

---

## 13. Tenant boundary

1. Each execution context targets a single tenant. Every contract that
   carries tenant-derived data carries the tenant assessment context
   (shapes owned by `domain-types.md` / `core-contracts.md`); there is no
   ambient or implied tenancy.
2. No dependency or shared-state design may permit cross-tenant
   data/configuration/capability/evidence/result contamination — whether
   through shared singletons, static caches, reused contexts, pooled
   adapters, aggregated outputs, or graph edges spanning tenants.
3. Cross-tenant contamination is an integrity failure: it MUST fail
   safely and visibly through the shared error/result vocabulary. Tenant-
   context mismatch handling (wrong-tenant rejection, incompatible-context
   refusal, no cross-tenant edges/evidence) is owned jointly by
   `domain-types.md`, `identity-graph-contracts.md`,
   `findings-evidence-schema.md`, `error-result-model.md`, and
   `authentication-design.md`; this document only fixes the dependency
   consequence (no shared-state path may bypass those checks).
4. Test projects MUST prove isolation with synthetic cross-tenant
   contamination scenarios before provider-backed integrated assessment
   is trusted (see `testing-seams.md` §9 and
   `implementation-sequence.md` tenant-isolation gates). No production
   tenant data is used for such tests.

---

## 14. State authority boundary

Exactly five canonical assessment states exist:

- `PASS`
- `FAIL`
- `NOT_EVALUATED`
- `NOT_APPLICABLE`
- `ERROR`

1. Only approved assessment/rule semantics (owned by
   `core-contracts.md` §4 with the pipeline owned by
   `rule-engine-contracts.md` and failure mapping owned by
   `error-result-model.md`) may determine these states.
2. Collectors, provider adapters, configuration loaders, renderers, CLI,
   and AI components cannot invent or reinterpret assessment states.
   Operational conditions they observe (incomplete collection,
   authorization denial, provider failure, unknown completeness,
   cancellation, resource exhaustion, confirmed-empty) are typed in their
   correct layer and mapped to assessment states only through the
   approved failure/capability semantics — never by local reinterpretation.
3. No sixth state, alias state, sub-verdict presented as a state, or
   severity-into-state mapping is permitted in V1. Renderer-specific
   display labels, exit-code mappings, and diagnostic categories MUST NOT
   be mistaken for or promoted to assessment states.

---

## 15. AI/LLM boundary

AI/LLM functionality, if introduced later within approved scope, cannot
become authoritative for any of the following:

- authentication;
- authorization;
- capability truth;
- completeness;
- applicability;
- `PASS`/`FAIL` determination or modification;
- evidence provenance (including supply or fabrication of evidence);
- tenant isolation;
- security-policy enforcement.

Consequences for dependencies:

1. No production project references an AI/LLM SDK in V1. No port admits
   model output onto the evaluation, authorization, collection, or
   authentication path (INV-13).
2. No AI project or AI implementation is introduced in V1. Any future
   explanatory AI attaches as a new outer consumer of canonical output —
   downstream explanation only — never as a `Core` dependency and never
   on the verdict path. Such an addition requires approved
   requirements/architecture change first (§19).
3. AI context inputs MUST NOT carry secret-bearing values, tenant data
   beyond the already-emitted canonical output it explains, or
   provenance it cannot verify; any future AI surface MUST NOT
   reintroduce provider types into `Core` or mutate assessment states.

---

## 16. Dependency-cycle prevention

1. **Project-reference prohibition.** The production reference graph MUST
   remain the DAG in §4. Any project reference that creates a cycle
   (including a transitive cycle via `Application` ports) is prohibited.
2. **Conceptual back-reference prohibition.** The following effectively
   invert the architecture even without a literal project cycle and are
   likewise prohibited:
   - a `Core` contract importing, naming, or structurally mirroring an
     adapter/renderer/host/auth-implementation type;
   - an adapter dictating a normalized domain, graph, rule, finding, or
     output-model shape outward (adapters implement inward ports; they
     never define domain semantics);
   - `Application` instantiating an adapter, renderer, or auth mechanism
     (second composition root);
   - `Output` reaching back into evaluation internals to re-derive state;
   - shared mutable state or ambient service location that lets an outer
     concern drive inner behavior outside declared ports.
3. **Host rule.** `Cli` is at the outer edge and is referenced by nothing.
   A future host, if ever approved, is a second outer edge reusing the
   same ports — never a dependency of existing projects (§19).

---

## 17. Test-project dependency rules

Conceptual dependency expectations for the six established test
projects (structures per `solution-structure.md` §6; scenarios per
`testing-seams.md`):

| Test project | May depend on | Must not do |
| --- | --- | --- |
| `EntraNHI.Core.Tests` | `EntraNHI.Core` and required inward contract vocabulary; synthetic fixtures, fakes for seams | Require network, live tenant, provider SDK presence, secrets, renderer/host behavior |
| `EntraNHI.Application.Tests` | `EntraNHI.Application` + `EntraNHI.Core`; faked adapters (collectors, capability sources, auth contexts, clocks, output ports) | Force production orchestration to expose provider/secret internals; test verdict logic here beyond pass-through propagation |
| `EntraNHI.Infrastructure.Graph.Tests` | `EntraNHI.Infrastructure.Graph` + inward contracts; synthetic provider doubles | Require live tenant/network; invent endpoint/permission/payload claims; carry real secrets or production data |
| `EntraNHI.Output.Tests` | `EntraNHI.Output` + `EntraNHI.Core` (authoritative results); adversarial string/path fixtures | Require network/telemetry; weaken renderer-non-authority for test convenience |
| `EntraNHI.Cli.Tests` | `EntraNHI.Cli` + `EntraNHI.Core`/`Application`/`Output` contracts as needed for composition-wiring verification with faked adapters | Test verdict logic beyond pass-through; require secrets or live services |
| `EntraNHI.Architecture.Tests` | Whatever references are needed to observe structure (production projects as analysis subjects); no behavior | Test behavior; depend on live services; introduce production-weakening hooks |

Cross-cutting test rules:

1. Tests may depend on their subject and required inward contracts but
   MUST NOT force production code to expose unsafe provider/secret/
   internal implementation details merely for testing (no test-only
   secret accessors, no provider-type surfacing, no insecure fallback
   paths; INV-16).
2. Test projects MUST NOT carry real secrets, production tenant data, or
   live-service dependencies. Sanitized synthetic fixtures are the
   default; any captured provider fixture requires explicit security
   review and sanitization before repository inclusion.
3. Architecture tests SHOULD verify the dependency constraints in §§3–16
   (project-reference direction, namespace rules, forbidden-type absence
   in `Core`). Tooling is a TBD (§20, T-06); this document selects none.

---

## 18. Enforcement strategy

Future enforceability is layered. Nothing below is implemented here; the
list fixes what later implementation/CI work MUST provide, not how it is
tooled (mechanism choices are TBD in §20, T-06):

1. **Project-reference review.** The §4 matrix is reviewed on every
   structural change; any new reference requires justification against
   §§3–16 and, where it touches the tree, reconciliation with
   `solution-structure.md` through change control.
2. **Namespace/type leakage checks where appropriate.** Rules such as
   "no provider namespace in `Core.Rules`" and "no renderer namespace in
   `Core`" are checked mechanically once the enforcement mechanism is
   chosen, in addition to review.
3. **Architecture tests.** `EntraNHI.Architecture.Tests` encodes §§3–16
   as structural assertions (reference direction, forbidden-type
   absence, composition-root singularity). It runs as a structural gate;
   it owns no behavior tests.
4. **Build/CI checks.** Dependency and structural gates run in the same
   pre-merge path as build/test, with least-privilege workflow
   permissions; exact workflow design belongs to future CI/release work
   (see `implementation-sequence.md`).
5. **Code review.** Human review remains the backstop for conceptual
   back-references (§16.2) that mechanical checks cannot fully capture
   (for example, an adapter-shaped domain field, or severity smuggled
   into state).

No enforcement package, analyzer, scanner, workflow, or version is
selected in this document.

---

## 19. Boundary violation handling

1. Dependency violations are engineering/architecture failures and MUST
   be corrected rather than normalized as runtime assessment states. A
   forbidden reference, leaked provider type, secret-bearing field, or
   architectural bypass is fixed by restructuring the change (move the
   concern to its owning project, invert the dependency through the
   port, or escalate through change control) — never by emitting a new
   verdict or diagnostic as if the violation were tenant data.
2. Do not invent a sixth state. In particular, a boundary violation in
   development MUST NOT be represented as a new assessment state, an
   alias state, or a severity level promoted to state semantics. Runtime
   handling of already-shipped failure modes continues to use exactly
   `PASS`, `FAIL`, `NOT_EVALUATED`, `NOT_APPLICABLE`, `ERROR` with the
   failure taxonomy owned by `error-result-model.md`.
3. If a required contract cannot represent observed provider behavior
   without violating §§3–16, implementation stops for design review (see
   `implementation-sequence.md` stop/go gates) instead of working around
   the boundary silently.

---

## 20. Extensibility

1. Future provider/output adapters MUST preserve inward contracts and
   authority boundaries: a new provider adapter implements the same
   collector/capability ports and normalization obligations as the V1
   Graph adapter; a new renderer or host consumes the same canonical
   output model under the same non-authority, tenant-isolation, secret-
   exclusion, and AI-non-authority constraints.
2. Do not generalize V1 into an unapproved plugin marketplace/framework.
   No dynamic plugin loading, third-party extension hosting, remote-code
   acquisition, or cross-tenant aggregation point is introduced by this
   document. Any such capability requires approved requirements/
   architecture change first (NG-series non-goals; `README.md` §8).
3. A future host beyond the V1 CLI (illustrative only; not created or
   reserved) would be a new outer project on existing ports, per
   `solution-structure.md` §14, subject to the same composition-root
   singularity within its own deployment (one composition root per host;
   no shared composition inheritance between hosts).

---

## 21. Implementation TBDs

Explicitly unresolved enforcement/tooling/package/mechanism choices.
None is resolved here by speculation.

| ID | Unknown | Why unresolved at this stage | Owner |
| --- | --- | --- | --- |
| T-01 | Exact namespace names and folder layout within each project. | Logical organization is fixed by `solution-structure.md` §7; physical names require coordination with the core contract surface. | `core-contracts.md` with this document |
| T-02 | Concrete port/interface placement where `Application` vs `Core` ownership is ambiguous (collector ports, capability-source ports, auth-context ports, output ports, clock/ordering ports). | Seam placement needs joint agreement across collector, capability, auth, output, and core contracts. | This document with `collector-contracts.md`, `capability-model.md`, `authentication-design.md`, `output-renderer-contracts.md`, `core-contracts.md` |
| T-03 | Graph communication mechanism (SDK vs direct HTTP) and its dependency footprint. | Requires a dedicated collection-implementation decision with supply-chain review; no mechanism is approved yet. | `collector-contracts.md` with this document |
| T-04 | Any Graph endpoint, permission/scope name, property/relationship mapping, Agent Identity mapping, or licensing-behavior claim. | Requires validation against published Microsoft documentation; invention is prohibited (INV-15). | `collector-contracts.md` / `capability-model.md` / `authentication-design.md` after documentation validation |
| T-05 | Dependency-injection mechanics/container choice; logging/telemetry abstraction surface and backend selection. | Seam-catalog and supply-chain decisions belong downstream; no container or backend is approved. | This document (mechanics/surface); backend selection through future build-control work |
| T-06 | Architecture-test enforcement tooling and rule set; namespace/type leakage check mechanics; build/CI wiring. | Requires a dedicated tooling decision with supply-chain review; no package is approved yet. | This document (rule set) with future CI/release design (wiring) |
| T-07 | Identifier formats, signing/hashing/timestamping posture touching provenance references. | Integrity mechanisms need separate security review. | `findings-evidence-schema.md` (with `domain-types.md`) |
| T-08 | Target frameworks, build strictness, signing, packaging, SBOM/scanning workflow, dependency pinning/review policy. | Requires dedicated build/release design; inventing versions here would pre-empt it. | Future build-control work through this document (principles) and `implementation-sequence.md` (ordering) |
| T-09 | Test-framework choice and fixture-harness mechanics for architecture tests. | Requires a dedicated tooling decision; no framework is approved yet. | Future test-implementation decision; scenario catalog owned by `testing-seams.md` |

---

## 22. Acceptance criteria

This document is complete for Phase 0.3.10 when:

1. The DAG rooted at `Core` (§§3–4) matches `solution-structure.md`
   §§8–9, with all five production projects and all six test projects
   placed and no new project introduced.
2. Core prohibitions (§5) cover provider SDK, transport, authentication
   implementation, storage implementation, CLI/presentation, renderer
   implementation, logging/telemetry implementation, configuration-source
   implementation, container implementation, and AI/LLM implementation —
   without prohibiting abstract inward-facing contracts.
3. Application (§6), Graph-adapter (§7), Output (§8), and CLI (§9)
   boundaries preserve their established authorities, including the
   verbatim CLI authentication wording and the transient-handling
   clarification.
4. Cross-cutting placement (§10), provider leakage (§11), secret (§12),
   tenant (§13), state-authority (§14), AI (§15), cycle-prevention (§16),
   test-project (§17), enforcement (§18), violation-handling (§19), and
   extensibility (§20) rules are all explicit with no sixth state and no
   invented provider detail.
5. Every implementation-sensitive unknown is listed in §21 as a TBD with
   a named owner; no package, mechanism, endpoint, permission, property,
   mapping, or numeric limit is invented or selected.
6. `git diff --check` is clean and `git status --short` shows only the
   two authorized files modified.

---

## 23. Invariant and requirement traceability

| Concern | Traces to |
| --- | --- |
| Inward DAG, Core independence, adapter isolation | INV-03; INV-04; CON-003; `solution-structure.md` §§8–9 |
| Deterministic evaluation protected by dependency direction | INV-02; VERD-006 |
| Five states, no sixth state, no reinterpretation | INV-05; VERD-001–VERD-005 |
| Completeness, capability, authorization, provenance preserved across the provider boundary | INV-07; INV-14; CAP-001–CAP-004 |
| Evidence/provenance authority placement | INV-06; INV-11 |
| Secret exclusion and least privilege | INV-08; INV-09; SEC-001; SEC-002; SEC-008; SEC-009 |
| Renderer/output non-authority | INV-12 |
| AI non-authority | INV-13; SEC-010; VERD-007; CON-004 |
| Tenant isolation, no cross-tenant contamination | INV-10 |
| Read-only V1, no SaaS/plugin generalization | INV-01; FR-040; SEC-003–SEC-007; CON-001; NG-003–NG-005 |
| No invented provider behavior | INV-15 |
| Testability without live tenant; architecture enforcement | Testing-architecture §§1–2, 4–7, 18, 20 |

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
