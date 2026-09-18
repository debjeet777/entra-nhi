# Trust Boundaries

> **Status:** Phase 0.2.9
> **Date:** 2026-09-18

---

## Purpose

Define the complete trust-zone architecture for EntraNHI V1. This document identifies all trust zones, the transitions between them, the data crossing each boundary, security assumptions, validation requirements, failure behavior, and prohibited behavior. It establishes the foundation for the threat model and governs security-sensitive design decisions.

---

## Trust boundary overview

EntraNHI V1 is a read-only security assessment pipeline with the following conceptual data flow:

```
Operator / CI
    |
    v
Authentication / AuthorizedAccessContext
    |
    v
Microsoft Entra / Microsoft Graph
    |
    v
Collectors
    |
    v
SourceObservation
    |
    v
Normalized Domain Model
    |
    v
Identity Graph
    |
    v
Deterministic Rule Engine
    |
    v
RuleEvaluation
    |
    v
Findings / Evidence
    |
    v
Canonical Output Model
    |
    v
CLI / JSON / SARIF / HTML / future interfaces
```

Each arrow represents a trust boundary. The following sections document every significant boundary in detail.

---

## Boundary 1 — External identity-provider / source boundary

| Property | Description |
| --- | --- |
| **Trusted side** | EntraNHI runtime (operator-controlled execution context) |
| **Untrusted or less-trusted side** | Microsoft Entra identity provider, Microsoft Graph, Azure Resource Manager, licensed telemetry sources |
| **Data crossing the boundary** | Identity metadata, permission grants/assignments, credential lifecycle metadata (non-secret), ownership/accountability relationships, identity relationships, capability/availability signals, authentication tokens (transient) |
| **Security assumptions** | Microsoft services operate correctly and return documented response schemas; responses originate from the documented service; TLS transport integrity is maintained; responses reflect the actual tenant state at observation time |
| **Validation requirements** | EntraNHI MUST NOT assume all tenants expose identical APIs, licensing tiers, or endpoint behavior [CAP-003, INV-07]; response schemas MUST be validated against documented expectations; unexpected or malformed responses MUST produce structured error/diagnostic state, not silent inference; capability detection MUST distinguish unavailable data from absent data [INV-07] |
| **Failure behavior** | Malformed or unexpected provider responses produce structured collection errors propagated to capability state and downstream evaluation as NOT_EVALUATED or ERROR; authentication failures propagate as explicit authentication-failure state; transient failures use bounded retry with backoff |
| **Prohibited behavior** | EntraNHI MUST NOT invent undocumented Graph endpoints, permissions, or response properties [CAP-005, INV-15]; EntraNHI MUST NOT assume provider data is complete, accurate, or tamper-free; EntraNHI MUST NOT interpret provider service unavailability as a tenant security state; provider-originated strings MUST NOT be treated as trusted data in output rendering |

---

## Boundary 2 — Authentication-material boundary

| Property | Description |
| --- | --- |
| **Trusted side** | EntraNHI runtime authentication boundary (AuthorizedAccessContext) |
| **Untrusted or less-trusted side** | Transient authentication tokens, credential sources (environment variables, OS credential stores, CI secrets) |
| **Data crossing the boundary** | Access tokens, refresh tokens (transient, in-memory only), tenant context identifiers, authorization metadata |
| **Security assumptions** | Authentication material is obtained through approved Microsoft identity platform mechanisms; tokens are valid for the intended tenant scope; the operator controls credential sources outside EntraNHI; token lifetime and refresh behavior follow documented Microsoft identity platform semantics |
| **Validation requirements** | Authentication material MUST enter the runtime only through the authentication boundary [INV-09]; tokens MUST NOT become normalized domain data, findings, evidence, logs, or persisted assessment artifacts [SEC-001, SEC-011, INV-09]; tenant assessment context MUST be established or validated before collection data is accepted [INV-10]; authentication success MUST NOT imply authorization for every capability [INV-08]; the authenticated principal's permissions MUST be evaluated per capability, not as a global flag |
| **Failure behavior** | Authentication unavailable, rejected, expired, or tenant-mismatched states MUST produce explicit failure categorization [authentication-authorization.md §12]; no authentication failure may silently produce a successful assessment [INV-14]; workload authentication unavailability in CI MUST fail closed, not degrade to an insecure fallback [INV-16] |
| **Prohibited behavior** | Tokens MUST NOT be written to disk by EntraNHI; tokens MUST NOT appear in output artifacts or logs; the authentication boundary MUST NOT grant itself additional tenant privileges; authentication success MUST NOT bypass per-capability authorization checks; AI/LLM MUST NOT influence authentication or authorization decisions [INV-13] |

---

## Boundary 3 — Provider/API boundary

| Property | Description |
| --- | --- |
| **Trusted side** | EntraNHI collector layer |
| **Untrusted or less-trusted side** | Microsoft Graph API, Azure Resource Manager, licensed telemetry endpoints, any future optional enrichment APIs |
| **Data crossing the boundary** | Read-only API queries (outbound); identity metadata, permissions, relationships, credential metadata, telemetry data (inbound) |
| **Security assumptions** | EntraNHI operates with least-privilege read-only permissions [SEC-008, INV-08]; API responses are non-authoritative with respect to EntraNHI's assessment truth (EntraNHI produces its own deterministic evaluation); collection follows documented Microsoft Graph API behavior [INV-15]; the provider API does not return credential secret values through the endpoints EntraNHI queries |
| **Validation requirements** | All collection MUST occur through documented, approved Microsoft Graph endpoints [INV-15]; collection MUST NOT perform write, delete, or administrative operations [INV-01]; collected data MUST be validated against expected schema before normalization; optional enrichment APIs MUST be isolated behind their own collector/adaptor boundary and MUST NOT become implicit dependencies [collection-architecture.md §1]; permission requirements MUST be documented per capability [SEC-008] |
| **Failure behavior** | API unavailability, throttling, or transient errors produce structured collection failure categories [collection-architecture.md §10]; authorization denial produces capability-state metadata (NOT_EVALUATED for dependent rules, not FAIL) [INV-05, INV-07]; partial collection MUST be visible downstream with explicit capability state [INV-14] |
| **Prohibited behavior** | Collectors MUST NOT perform tenant-state mutation [INV-01]; collectors MUST NOT request elevated privileges dynamically; collectors MUST NOT silently expand collection scope beyond documented observations; collectors MUST NOT hide partial collection or manufacture missing observations; the provider boundary MUST NOT be bypassed by rule-engine code [INV-03] |

---

## Boundary 4 — Collector boundary

| Property | Description |
| --- | --- |
| **Trusted side** | Normalization / domain-model layer |
| **Untrusted or less-trusted side** | Collector layer (logical components responsible for acquiring source observations) |
| **Data crossing the boundary** | SourceObservation envelopes containing source family, source object reference, tenant assessment context, collection operation/context, observation timestamp, capability state, source data, provenance, structured error information |
| **Security assumptions** | Collectors acquire documented observable state from approved external sources; collectors do not perform security-rule evaluation; SourceObservation envelopes carry sufficient non-secret information for normalization; capability state accurately reflects collection success, failure, or limitation |
| **Validation requirements** | Collectors MUST NOT directly construct security findings [collection-architecture.md §11.3]; SourceObservation content MUST NOT contain authentication tokens or secret values [collection-architecture.md §4.2]; source observations MUST preserve tenant assessment context to prevent cross-tenant mixing [INV-10]; capability state MUST be sufficiently granular that one collector's failure does not invalidate unrelated successfully collected data [collection-architecture.md §5.2] |
| **Failure behavior** | Collection failures are propagated as structured capability-state metadata through SourceObservation envelopes; a failed collector MUST NOT silently return an empty successful collection [collection-architecture.md §10]; every collection error MUST be recorded and propagated to downstream capability state so the rule engine produces NOT_EVALUATED or ERROR [INV-14] |
| **Prohibited behavior** | Collectors MUST NOT evaluate PASS/FAIL or produce security verdicts; collectors MUST NOT perform remediation or modify tenant state; collectors MUST NOT manufacture missing observations; collectors MUST NOT infer undocumented provider semantics; collectors MUST NOT pass authentication secrets into the normalized domain model [INV-09]; collectors MUST NOT hide partial collection or missing data |

---

## Boundary 5 — Provider-data to normalized-domain boundary

| Property | Description |
| --- | --- |
| **Trusted side** | Normalized domain model (IdentityRecord, CredentialMetadata, PermissionRelationship, AccountabilitySubject, CapabilityState, Provenance) |
| **Untrusted or less-trusted side** | Provider-specific source objects within SourceObservation envelopes |
| **Data crossing the boundary** | Provider-specific data fields translated into stable normalized contracts; provenance references preserving source identity; capability/collection state |
| **Security assumptions** | Normalization preserves the semantic content of observations without introducing undocumented properties; provider-specific representation does not leak into deterministic rule contracts except through explicitly normalized capability/provenance data [INV-04]; normalized objects are deterministic translations of source observations |
| **Validation requirements** | Provider-specific source objects MUST be translated into stable normalized contracts before rule evaluation [INV-04]; provider-specific representation MUST NOT leak into rule contracts [INV-04]; normalization MUST distinguish observed facts from derived facts [domain-model.md Principle 2]; normalization MUST preserve null/unknown/absent semantics [domain-model.md Principle 3]; missing or unavailable data MUST NOT automatically imply security FAIL [INV-05, CAP-002]; deduplication MUST be deterministic [INV-02] |
| **Failure behavior** | Normalization-input validation failures produce structured error state; conflicting observations MUST remain detectable and MUST NOT be silently resolved by heuristic inference [INV-02]; normalization failure MUST NOT silently produce PASS or FAIL |
| **Prohibited behavior** | Normalization MUST NOT invent undocumented properties or relationships; normalization MUST NOT use one generic null for every missing-data condition; normalization MUST NOT allow provider SDK types to enter rule contracts; normalization MUST NOT introduce AI-inferred or probabilistic data; display names MUST NOT serve as identity keys [domain-model.md] |

---

## Boundary 6 — Normalized-domain to identity-graph boundary

| Property | Description |
| --- | --- |
| **Trusted side** | Identity graph (deterministic projection of normalized domain model) |
| **Untrusted or less-trusted side** | Normalized domain model records |
| **Data crossing the boundary** | Normalized identities, normalized relationships, capability states, provenance references projected into graph nodes and edges |
| **Security assumptions** | The identity graph is a projection, not a second source of truth; every node and edge is derived from normalized domain model records; graph construction is deterministic given identical inputs [INV-02]; the graph retains provenance and capability state references for all observations |
| **Validation requirements** | Graph MUST NOT be constructed from raw provider data [identity-graph.md]; graph MUST NOT introduce facts absent from the normalized domain model [identity-graph.md]; graph MUST NOT mutate after construction during a single assessment run [identity-graph.md]; edges MUST NOT silently join tenant contexts [INV-G1, INV-10]; display names MUST NOT establish identity equality [INV-G2]; derived edges MUST be distinguishable from observed edges [INV-G6]; rules MUST consume normalized graph contracts, not provider types [INV-G7, INV-03] |
| **Failure behavior** | Graph construction failure MUST be surfaced explicitly; a failed or partial graph MUST NOT produce false PASS outcomes [INV-G8, INV-14]; missing nodes or edges from unavailable collection MUST NOT be interpreted as confirmed absence [INV-G3] |
| **Prohibited behavior** | Graph MUST NOT introduce AI-inferred or probabilistic relationships; graph MUST NOT silently repair missing data; graph MUST NOT fabricate edges from display-name matching; graph MUST NOT assume undocumented Microsoft relationships; graph MUST NOT be mutable after construction |

---

## Boundary 7 — Rule-engine authority boundary

| Property | Description |
| --- | --- |
| **Trusted side** | Deterministic rule engine (authoritative for PASS/FAIL/NOT_EVALUATED/NOT_APPLICABLE/ERROR verdicts) |
| **Untrusted or less-trusted side** | Normalized domain model, identity graph, capability states, rule configuration, operator inputs |
| **Data crossing the boundary** | Normalized identities, graph edges/projections, capability states, provenance references, deterministic configuration, rule definitions |
| **Security assumptions** | Rule evaluation is deterministic given identical inputs, capability state, and rule version [INV-02]; rules evaluate only normalized data, never provider APIs or raw payloads [INV-03]; rule definitions are explicit and self-contained; configuration cannot silently weaken assessment integrity [INV-16] |
| **Validation requirements** | Rules MUST NOT directly query Microsoft Graph, Azure Resource Manager, or any provider API [INV-03]; rules MUST NOT use provider SDK types [INV-03, INV-04]; rules MUST NOT request additional privileges [INV-03]; rules MUST NOT use AI/LLM to determine verdicts [INV-13]; each rule MUST define applicability, required capabilities, required inputs, evaluation logic, PASS/FAIL semantics, and unavailable-input behavior [rule-engine.md §7]; missing data MUST NOT automatically become PASS or FAIL [rule-engine.md §4] |
| **Failure behavior** | Invalid normalized input, rule execution exception, inconsistent graph state, or invalid rule configuration produces ERROR [rule-engine.md §4.3]; ERROR MUST NOT be converted to PASS or FAIL to simplify reporting; per-rule failure isolation where safe continuation is possible [rule-engine.md §13] |
| **Prohibited behavior** | Rule engine MUST NOT access provider APIs [INV-03]; rule engine MUST NOT fabricate evidence; rule engine MUST NOT use LLM-generated text as factual evidence; rule engine MUST NOT convert ERROR to FAIL; rule engine MUST NOT allow configuration to silently disable security-critical rules; rule engine MUST NOT silently produce PASS from missing data [rule-engine.md §4.1] |

---

## Boundary 8 — Findings/evidence boundary

| Property | Description |
| --- | --- |
| **Trusted side** | Canonical findings and evidence records (RuleEvaluation-derived) |
| **Untrusted or less-trusted side** | Rule engine output (RuleEvaluation records), provenance references, diagnostic information |
| **Data crossing the boundary** | RuleEvaluation records, evidence references, provenance chain data, severity metadata, diagnostic context |
| **Security assumptions** | RuleEvaluation is authoritative and reflects the deterministic assessment outcome; evidence references trace back to collected source observations through normalized domain data; provenance chain is complete and non-fabricated; findings are derived from RuleEvaluation, not an independent source of truth |
| **Validation requirements** | Findings/evidence architecture MUST NOT recompute rule logic, change evaluation state, upgrade/downgrade PASS/FAIL, or convert ERROR to FAIL [findings-evidence.md §1]; evidence MUST preserve enough provenance to trace back toward the source observation [INV-06, INV-11]; PASS and FAIL require evidence appropriate to rule semantics [findings-evidence.md §7]; missing evidence MUST NOT fabricate PASS/FAIL; diagnostics MUST remain distinct from tenant security findings [findings-evidence.md §15] |
| **Failure behavior** | Evidence fabrication attempts produce explicit integrity-failure handling; broken evidence references produce structured error state; tenant-context mismatch is an integrity failure that must fail safely and visibly [INV-10]; evidence-processing failures MUST NOT silently convert assessment into successful/secure result |
| **Prohibited behavior** | Findings/evidence MUST NOT become an independent source of truth for evaluation state; evidence MUST NOT be fabricated; provenance MUST NOT be invented; AI MUST NOT create, modify, or suppress evidence [INV-13]; secret material MUST NOT become evidence [INV-09]; cross-tenant evidence correlation is prohibited [INV-10] |

---

## Boundary 9 — Output/rendering boundary

| Property | Description |
| --- | --- |
| **Trusted side** | Canonical Output Model (provider-independent representation of authoritative assessment results) |
| **Untrusted or less-trusted side** | Output renderers (CLI, JSON, SARIF, HTML), provider/tenant-originated strings within findings |
| **Data crossing the boundary** | Assessment context, RuleEvaluations, Findings, Evidence/provenance references, capability summary, diagnostics, tenant/provider-originated display names and textual metadata |
| **Security assumptions** | Output renderers are strictly downstream of deterministic assessment; renderers transform canonical data into format-specific contracts; core assessment logic is independent of output format [INV-12]; provider/tenant strings are treated as untrusted content requiring format-specific safe encoding |
| **Validation requirements** | Output renderers MUST NOT alter RuleEvaluation state, reinterpret PASS/FAIL, convert ERROR to FAIL, or convert NOT_EVALUATED to PASS [output-architecture.md §1]; all five evaluation states MUST be preserved and distinguishable [INV-05]; provider/tenant-originated strings MUST be treated as untrusted and safely encoded for target format [findings-evidence.md §17]; HTML renderers MUST defend against XSS/injection [output-architecture.md §8]; CLI renderers MUST defend against terminal/control-character injection [output-architecture.md §9]; renderers MUST NOT suppress ERROR/NOT_EVALUATED to create false secure impression |
| **Failure behavior** | Output-generation failure MUST NOT create a false PASS, suppress incomplete assessment, or fabricate replacement results [output-architecture.md §16]; renderer failure is a system diagnostic, not a tenant security finding; partial artifacts MUST NOT appear complete |
| **Prohibited behavior** | Renderers MUST NOT rerun security rules, call providers, collect additional data, or request additional privileges; renderers MUST NOT fabricate evidence or invent provenance; renderers MUST NOT permit AI/LLM to determine output security semantics; HTML MUST NOT execute tenant-supplied active content; CLI MUST NOT execute terminal control sequences derived from untrusted tenant data |

---

## Boundary 10 — Filesystem/artifact boundary

| Property | Description |
| --- | --- |
| **Trusted side** | EntraNHI output-generation logic writing to operator-controlled filesystem |
| **Untrusted or less-trusted side** | Filesystem paths, file names, output destinations, existing artifacts |
| **Data crossing the boundary** | Generated assessment output artifacts (CLI text, JSON, SARIF, HTML); temporary files; file metadata |
| **Security assumptions** | The operator controls the filesystem where output is written; output destination paths are operator-configured or use safe defaults; the filesystem is not adversarially controlled at the path-resolution level |
| **Validation requirements** | Tenant/provider-controlled values MUST NOT directly determine filesystem paths without validated safe transformation [output-architecture.md §10]; output generation MUST defend against path traversal, absolute-path injection, unsafe file names, reserved/special file names, symlink/reparse-point hazards [output-architecture.md §10]; output MUST NOT silently leave a partial artifact that appears complete [output-architecture.md §11]; existing artifacts MUST NOT be silently overwritten without explicit validated policy [output-architecture.md §11]; output files containing sensitive assessment data SHOULD use restrictive file permissions where supported |
| **Failure behavior** | Output-generation failures MUST produce visible error state; partial artifacts MUST be cleaned up or quarantined; failed temporary files MUST NOT persist as apparently-complete assessment artifacts; filesystem permission errors MUST produce explicit failure, not silent success |
| **Prohibited behavior** | Output MUST NOT write to locations outside the intended destination; output MUST NOT silently overwrite existing artifacts by default; output MUST NOT create executable content; temporary files MUST NOT be left in inconsistent states |

---

## Boundary 11 — CI/non-interactive execution boundary

| Property | Description |
| --- | --- |
| **Trusted side** | EntraNHI runtime operating in non-interactive/workload mode |
| **Untrusted or less-trusted side** | CI/CD platform (GitHub Actions, Azure DevOps, etc.), workload credentials, pipeline configuration, build artifacts |
| **Data crossing the boundary** | Workload credentials (externally configured, not embedded in source control), pipeline inputs, assessment output artifacts consumed by CI |
| **Security assumptions** | CI/CD platform is operator-configured and operator-trusted; workload credentials are managed outside EntraNHI source code [SEC-002]; CI execution obeys the same authentication, authorization, and evidence rules as interactive execution; CI logs are not confidential |
| **Validation requirements** | Non-interactive execution MUST support approved workload authentication mechanisms without interactive user input [FR-031]; credentials MUST NOT be embedded in source control [SEC-002]; CI execution MUST fail closed if required authentication material is unavailable [INV-16]; CI MUST NOT downgrade into insecure fallback automatically [INV-16]; output consumed by CI MUST preserve machine-readable state, explicit incomplete/error conditions, and deterministic semantic behavior [output-architecture.md §24]; CI must NOT assume its logs are confidential when handling output strings |
| **Failure behavior** | Authentication unavailability in CI mode MUST produce explicit failure, not insecure degradation; workload credential absence MUST fail closed; CI pipeline failures MUST NOT silently produce successful assessment output |
| **Prohibited behavior** | CI execution MUST NOT embed credentials in source control or configuration files; CI execution MUST NOT assume interactive authentication is available; CI output consumption MUST NOT alter assessment semantics; CI pipeline MUST NOT silently grant additional privileges |

---

## Boundary 12 — Tenant isolation boundary

| Property | Description |
| --- | --- |
| **Trusted side** | Assessment data correctly scoped to the intended tenant assessment context |
| **Untrusted or less-trusted side** | Data from other tenants, cross-tenant API responses, tenant-context mismatch conditions |
| **Data crossing the boundary** | Tenant assessment context identifiers, tenant-scoped identity data, tenant-scoped findings and evidence |
| **Security assumptions** | Each EntraNHI execution targets a single tenant [INV-10]; authentication establishes or validates the tenant assessment context; the operator configures the correct target tenant; tenant context is established before collection data is accepted |
| **Validation requirements** | Data from separate tenant assessment contexts MUST never be silently mixed [INV-10]; execution context, collected data, and output artifacts MUST be scoped to a single tenant per run [INV-10]; every RuleEvaluation, Evidence reference, and Finding MUST remain bound to one explicit tenant assessment context [findings-evidence.md §12]; cross-tenant evidence correlation is prohibited in V1; tenant-context mismatch MUST fail safely and visibly; authentication context and normalized tenant context MUST be reconcilable |
| **Failure behavior** | Tenant-context mismatch produces explicit failure state; cross-tenant data mixing is an integrity failure; mismatched tenant context MUST NOT silently produce assessment output |
| **Prohibited behavior** | Cross-tenant data aggregation or mixing paths MUST NOT exist [INV-10]; cross-tenant edges MUST NOT be silently created in the identity graph [INV-G1]; one tenant's evidence MUST NOT support another tenant's assessment; tenant context MUST NOT be assumed without validation |

---

## Boundary 13 — AI/LLM boundary

| Property | Description |
| --- | --- |
| **Trusted side** | Deterministic assessment pipeline (collectors → normalization → identity graph → rule engine → findings/evidence) |
| **Untrusted or less-trusted side** | AI/LLM functionality (post-assessment explanation, documentation assistance, future presentation features) |
| **Data crossing the boundary** | Deterministic findings and evidence (for AI explanation); AI-generated explanatory text (non-authoritative, downstream only) |
| **Security assumptions** | AI/LLM is not part of the authoritative assessment path; AI/LLM output is clearly non-authoritative and does not feed back into deterministic evaluation; AI availability is not required for core assessment correctness [INV-13] |
| **Validation requirements** | AI/LLM MUST NOT determine PASS, FAIL, NOT_EVALUATED, NOT_APPLICABLE, or ERROR verdicts [INV-13]; AI/LLM MUST NOT change rule outcomes, fabricate evidence, bypass authorization, expand collection privileges, or become required for core assessment correctness [system-context.md §8]; if AI is introduced, it MUST be limited to post-assessment explanation or presentation and MUST NOT alter verdicts [INV-13]; AI-generated explanation MUST be clearly downstream/non-authoritative [findings-evidence.md §21] |
| **Failure behavior** | AI/LLM unavailability MUST NOT affect deterministic assessment correctness; AI failure MUST NOT propagate back into assessment state; AI errors are presentation-layer diagnostics, not tenant security findings |
| **Prohibited behavior** | AI/LLM MUST NOT determine verdicts [INV-13, SEC-10]; AI/LLM MUST NOT change rule state [rule-engine.md §19.2]; AI/LLM MUST NOT supply missing evidence; AI/LLM MUST NOT fabricate capability state; AI/LLM MUST NOT override ERROR or NOT_EVALUATED; AI/LLM MUST NOT alter severity authoritatively; AI/LLM MUST NOT execute remediation; AI/LLM MUST NOT request additional privilege; AI/LLM MUST NOT modify authentication or authorization policy; AI/LLM MUST NOT receive tenant assessment data through undisclosed external transmission |

---

## Boundary 14 — Network/telemetry boundary

| Property | Description |
| --- | --- |
| **Trusted side** | EntraNHI runtime (operator-controlled execution environment) |
| **Untrusted or less-trusted side** | All network endpoints, including approved Microsoft services and any non-approved destinations |
| **Data crossing the boundary** | Outbound: read-only API queries to approved Microsoft endpoints. Inbound: identity data, authentication tokens (transient). Prohibited: undisclosed telemetry, third-party data transmission |
| **Security assumptions** | Runtime network communication is limited to explicitly documented and approved service endpoints [CON-005]; TLS transport integrity is maintained for approved endpoints; no undisclosed telemetry or automatic third-party tenant-data transmission occurs; normal development, build, package, and CI network access is not restricted by this constraint [CON-005] |
| **Validation requirements** | Runtime network communication MUST be limited to explicitly documented and approved service endpoints [CON-005]; EntraNHI MUST NOT perform undisclosed telemetry or transmit collected tenant assessment data to unrelated third parties [CON-005]; output renderers MUST NOT require network transmission of tenant assessment data [output-architecture.md §22]; static HTML reports MUST NOT silently fetch tenant-sensitive resources from external services; any future explicit upload/integration feature requires separate architecture, authorization, disclosure, and security review |
| **Failure behavior** | Network unavailability for approved endpoints produces explicit collection/authentication failure; network errors MUST NOT silently produce successful assessment; attempted transmission to non-approved endpoints MUST be blocked or fail visibly |
| **Prohibited behavior** | Undisclosed telemetry is prohibited [CON-005]; third-party tenant-data transmission is prohibited [CON-005]; output renderers MUST NOT automatically upload findings to third-party services; any future upload feature requires separate architecture and security review |

---

## Boundary 15 — Future UI/integration boundary

| Property | Description |
| --- | --- |
| **Trusted side** | Stable output/core contracts (CanonicalOutputModel, RuleEvaluation, Finding) |
| **Untrusted or less-trusted side** | Future dashboard, UI, web interface, third-party integrations |
| **Data crossing the boundary** | Assessment output artifacts, machine-readable findings, diagnostic data |
| **Security assumptions** | Future UI consumes stable output/core contracts; the dashboard does not become a separate rule engine; future UI security requires separate architecture for authentication, authorization, session security, browser security, data persistence, and tenant isolation |
| **Validation requirements** | Future UI MUST NOT recompute rule logic or alter evaluation state; future UI MUST NOT become a separate assessment engine; future UI MUST maintain tenant isolation boundaries; future UI MUST treat all tenant/provider data as sensitive; any future SaaS or hosted deployment requires separate security architecture |
| **Failure behavior** | UI failure MUST NOT affect deterministic core assessment; UI errors are presentation diagnostics, not tenant security findings |
| **Prohibited behavior** | Future UI MUST NOT bypass the canonical output contract; future UI MUST NOT assume SaaS hosting; future UI MUST NOT silently aggregate cross-tenant data; future UI MUST NOT introduce AI-generated security verdicts |

---

## Cross-cutting trust principles

### Provider data is not automatically trusted

Data originating from Microsoft Graph, Azure Resource Manager, or licensed telemetry is observational input to the assessment pipeline. It is NOT automatically authoritative or complete. EntraNHI produces its own deterministic evaluation based on collected observations. Provider-originated and tenant-controlled strings remain untrusted data in all rendering contexts. [INV-07, INV-15]

### Authentication success does NOT imply authorization for every capability

Authentication establishes identity and access context. Authorization is evaluated per required collection capability. A successfully authenticated principal may lack authorization for specific capabilities. Authorization denial produces NOT_EVALUATED or ERROR, never silent PASS. [INV-08]

### Read-only does NOT mean permissionless or low sensitivity

EntraNHI V1 is operationally read-only with respect to tenant mutation. However, reading directory and security-relevant metadata requires authorized directory/API access. Read-only does not mean anonymous, permissionless, or inherently low sensitivity. [authentication-authorization.md §7]

### Renderers are downstream and cannot change assessment truth

Output renderers transform canonical assessment results into format-specific representations. Renderers cannot alter RuleEvaluation state, reinterpret verdicts, fabricate evidence, or suppress diagnostics. [INV-12]

### AI/LLM functionality is non-authoritative

AI/LLM output must never determine or modify assessment verdicts, evidence, or severity. AI availability is not required for core assessment correctness. [INV-13]

### No tenant data may silently cross tenant boundaries

Each execution context targets a single tenant. Data from separate tenant contexts MUST never be silently mixed. Cross-tenant evidence correlation is prohibited in V1. [INV-10]

### No undisclosed telemetry or automatic third-party tenant-data transmission

EntraNHI V1 MUST NOT perform undisclosed telemetry or transmit collected tenant assessment data to unrelated third parties. [CON-005]

---

## Trust boundary traceability to architecture invariants

| Boundary | Primary invariants |
| --- | --- |
| 1 — External identity-provider / source | INV-07, INV-15, CAP-003, CAP-005 |
| 2 — Authentication-material | INV-08, INV-09, INV-10, INV-13, INV-14, SEC-001, SEC-011 |
| 3 — Provider/API | INV-01, INV-03, INV-08, INV-15, SEC-008 |
| 4 — Collector | INV-09, INV-10, INV-14, collection-architecture.md |
| 5 — Provider-to-normalized-domain | INV-02, INV-04, INV-05, CAP-002 |
| 6 — Normalized-domain-to-identity-graph | INV-02, INV-03, INV-10, INV-G1, INV-G2, INV-G6, INV-G7, INV-G8 |
| 7 — Rule-engine authority | INV-02, INV-03, INV-04, INV-05, INV-06, INV-13, INV-14, INV-16 |
| 8 — Findings/evidence | INV-02, INV-05, INV-06, INV-09, INV-10, INV-11, INV-13, INV-14 |
| 9 — Output/rendering | INV-05, INV-12, INV-13, INV-14, INV-16 |
| 10 — Filesystem/artifact | INV-14, INV-16, output-architecture.md §10, §11 |
| 11 — CI/non-interactive | INV-16, SEC-002, FR-031, output-architecture.md §24 |
| 12 — Tenant isolation | INV-10, INV-G1, find-evidence.md §12 |
| 13 — AI/LLM | INV-13, SEC-010, VERD-007, CON-004 |
| 14 — Network/telemetry | CON-005 |
| 15 — Future UI/integration | INV-12, INV-13 |

---

## Open items

- **TBD:** Formal trust-boundary classification model (e.g., DLP zones, data-classification tiers).
- **TBD:** Exact network allowlist configuration mechanism [CON-005].
- **TBD:** Certificate pinning or TLS validation requirements.
- **TBD:** Data retention and disposal policy for local assessment data.
- **TBD:** CI/CD runner trust model and credential isolation requirements.
- **TBD:** Token in-memory protection requirements and implementation.
- **TBD:** Exact filesystem permission model for output artifacts.
- **TBD:** Plugin/extension trust model and sandboxing requirements (future scope).
