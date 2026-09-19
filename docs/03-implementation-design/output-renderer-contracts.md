# Output Renderer Contracts

> **Phase:** 0.3.9
> **Package:** Implementation Design
> **Status:** Proposed — design only, no code or project files created
> **Date:** 2026-09-18

---

## 0. Status, scope, and precedence

- **Status:** Proposed implementation-level renderer contracts. This document is design output only. It authorizes no implementation.
- **Scope:** Defines the canonical assessment-states boundary, renderer contract model, semantic-preservation rules, per-format (CLI/text, JSON, SARIF, HTML) projection obligations, failure/destination/security/tenant/provenance/resource/versioning/testing expectations, and explicit TBDs. It creates no code, no schemas, no templates, no dependencies, and no concrete library choices.
- **What this document is:** The owning document for the canonical output model contract and per-renderer contracts, semantic-preservation rules, injection-defense obligations, file/path-safety obligations, and renderer-failure semantics per `docs/03-implementation-design/README.md` §5–§6. Other Phase 0.3 documents reference renderer rules defined here; they MUST NOT redefine them.
- **What this document is not:** It is not a JSON schema, SARIF mapping specification, HTML template, CLI syntax manual, type/interface catalog with concrete members, filesystem implementation, or test catalog (scenarios belong to `testing-seams.md`).
- **Precedence:** Requirements (`docs/01-requirements/product-requirements.md`) and architecture (`docs/02-architecture/`, especially `output-architecture.md`, `architecture-invariants.md`, `rule-engine.md`, `findings-evidence.md`, `trust-boundaries.md`, `threat-model.md`, `testing-architecture.md`) are engineering truth. Where this document and an approved requirement or invariant appear to conflict, the requirement/invariant governs and the conflict is escalated through change control per `README.md` §8.
- **Language discipline:** **MUST / MUST NOT** carry forward genuine constraints. **SHOULD** marks intended flexibility. **TBD** marks an implementation-sensitive unknown resolved only by its named owning document, never by speculation.
- **Canonical assessment states:** Exactly five — **PASS**, **FAIL**, **NOT_EVALUATED**, **NOT_APPLICABLE**, **ERROR**. This document creates no sixth state, alias state, presentation state, or renderer-specific verdict. Operational outcomes such as "renderer succeeded / renderer failed" or "write completed / write truncated" are NOT assessment states and MUST be represented in a separate diagnostic channel (see §§2, 12).

---

## 1. Purpose and authority boundary

### 1.1 Purpose

Renderers project authoritative, output-neutral assessment results into V1 output formats (CLI/text, JSON, SARIF, HTML) for human and machine consumers. Rendering is presentation/projection only. Assessment is complete before rendering begins.

### 1.2 Renderer input authority

Renderers consume the authoritative result produced upstream: the `RuleEvaluation` records defined by `rule-engine-contracts.md`, the `Finding`/evidence/provenance/diagnostic projections defined by `findings-evidence-schema.md`, capability/completeness context defined by `capability-model.md`, and the shared error/result vocabulary defined by `error-result-model.md`. The canonical output model (see output-architecture §2) is a projection of those authoritative results — never a second source of truth. This document defines renderer obligations against that projection; it does not redefine `RuleEvaluation`, `Finding`, evidence, capability, or error shapes (per `README.md` §6 decision ownership).

### 1.3 Prohibited renderer authority

Renderers MUST NOT:

1. evaluate rules or re-derive verdicts (no predicate execution, no severity-into-state reinterpretation);
2. recollect provider data or resolve additional tenant data (no Graph/ARM calls, no live queries, no enrichment fetches);
3. call Microsoft Graph, ARM, or any other provider;
4. authenticate, request privilege, or handle secret-bearing authentication material;
5. change assessment states (no mapping of one of the five states to another, no collapsing, no aliasing);
6. invent findings or fabricate evidence/provenance;
7. infer **PASS** from missing information (no "no finding emitted therefore secure" logic);
8. reinterpret capability/completeness semantics (no declaring partial collection complete, no hiding unavailable-capability conditions);
9. use AI/LLM judgment to alter, summarize-into, or override results (INV-13; output-architecture §23 — any future explanatory AI is downstream, non-authoritative, and outside these renderer contracts);
10. perform remediation or any tenant-mutating operation (INV-01);
11. suppress diagnostics, errors, or incomplete-assessment conditions for visual or size convenience.

A renderer that performs any of the above is defective regardless of output validity. Renderer-non-authority is enforced structurally by placing renderers in `EntraNHI.Output` with no reference to rule-engine internals beyond consuming authoritative data (per `solution-structure.md` §§5.4, 8–9, 12).

---

## 2. Renderer contract model

Contracts are conceptual and implementation-oriented. No concrete C# types, interfaces, method signatures, or libraries are chosen in this document (see §20). Each V1 renderer (CLI/text, JSON, SARIF, HTML) MUST satisfy the same contract shape:

| Contract element | Conceptual obligation |
| --- | --- |
| Renderer identity | Each renderer has a stable, machine-readable identity (e.g., which format it produces) traceable in diagnostics and provenance so a consumer can determine which projection produced an artifact. Identity MUST NOT imply assessment authority. |
| Supported format | The format(s) a renderer claims to produce. V1 establishes exactly four: CLI/text, JSON, SARIF, HTML. A renderer MUST NOT claim a format whose semantic-preservation obligations (see §4) it cannot meet. |
| Renderer input | The canonical output-model projection actually supplied: assessment identity/context, tenant-context reference, assessment-time/reference metadata, tool/version metadata, active rule-set/configuration reference, `RuleEvaluation` records for all five states, emitted findings, evidence/provenance references appropriate for output, capability summary, diagnostics, completeness/integrity status, and output/redaction metadata (per output-architecture §2). Renderers MUST treat missing required input as renderer-input failure (see §12), never as grounds to invent content. |
| Renderer options | Presentation-only options routed from the configuration snapshot per `configuration-design.md` §9 (verbosity/detail depth within semantic-preserving bounds, destination requests, redaction-level requests within policy). Options MUST NOT carry assessment semantics and MUST NOT reach rule evaluation. |
| Render operation | A bounded, cancellable projection from renderer input + options to a format-specific artifact (in-memory representation and/or serialized form). The operation MUST be deterministic per §7, sanitizing per §14, tenant-scoped per §15, and resource-bounded per §17. |
| Render result | Either a completed artifact plus explicit completion/integrity indication, or an explicit renderer failure. Partial/truncated output MUST be marked as such and MUST NOT be presentable as complete (see §§13, 17). The result MUST preserve the authoritative assessment result unchanged. |
| Rendering diagnostics / failure | Structured, sanitized diagnostics distinct from tenant findings (see output-architecture §15): what was rendered, what failed, which scope is affected, and whether other renderers may still proceed. Diagnostics MUST exclude secrets and raw provider payloads where prohibited (see §14). |
| Destination / write boundary where applicable | Separation of projection (building the representation) from persistence (writing it somewhere). Write concerns — path validation, safe-write/publication, overwrite policy, artifact permissions — apply per §§13–14 and MUST NOT be conflated with semantic correctness of the projection itself. |

Cross-cutting rules:

- Renderer contracts expose ONLY the canonical data required for projection (output-architecture §17). Renderers MUST NOT receive provider clients, authentication material, or rule-engine internals.
- Renderers MUST NOT share mutable state with each other. Adding or changing one renderer MUST NOT require changes to rule logic, domain semantics, collectors, or other renderers.
- Fatal-vs-isolated renderer failure policy (whether one renderer's failure halts others) remains TBD (see §20) within the fail-safe constraints of §12.

---

## 3. Canonical assessment states

Renderers MUST render exactly the established five assessment states:

- **PASS**
- **FAIL**
- **NOT_EVALUATED**
- **NOT_APPLICABLE**
- **ERROR**

Rules:

1. No sixth state, alias state, presentation state, or renderer-specific verdict may be introduced in any format — including SARIF, HTML summaries, CLI exit behavior, or JSON enumerations (see §§10–11 for SARIF/HTML mapping discipline).
2. Presentation labels (human-readable text, icons, colors, groupings) MAY differ by format but MUST NOT change semantic meaning. A label that causes a reasonable consumer to confuse **NOT_EVALUATED** with **PASS**, **ERROR** with **FAIL**, or **NOT_APPLICABLE** with any verdict is prohibited.
3. Severity metadata MUST NOT change evaluation state. A high-severity **NOT_EVALUATED** remains **NOT_EVALUATED**; a low-severity **FAIL** remains **FAIL** (rule-engine §17).
4. Renderers MUST NOT invent a global "secure / insecure" score or reduce an assessment to a misleading green/red status (output-architecture §4). Explicit per-state counts Summaries MUST preserve incomplete/error visibility (see §8).
5. Any future format beyond V1 inherits this section unchanged unless requirements/architecture explicitly authorize otherwise through change control.

---

## 4. Semantic preservation

All formats MUST preserve sufficient semantics for the authoritative result. Formats need not be byte-for-byte or structurally identical; they MUST be semantically sufficient within their medium.

### 4.1 Minimum preserved semantics (as applicable per format)

- assessment identity and context (which assessment run this artifact represents);
- tenant-safe subject identity (attribution without leaking sensitive detail beyond output policy);
- rule identity and version for each evaluation;
- canonical assessment state for each evaluation (exactly one of the five);
- findings where emitted, with their linkage back to evaluations;
- evidence and provenance references sufficient to explain **PASS**/**FAIL** per `findings-evidence-schema.md` and output policy;
- structured reason/context for each **NOT_EVALUATED**, **NOT_APPLICABLE**, and **ERROR** sufficient to explain why normal evaluation did not occur or failed;
- capability and completeness information required to understand evaluation (requested vs achieved capability, partial-collection indicators, integrity status);
- errors and failures required by `error-result-model.md`, including renderer/input failures affecting the artifact itself;
- effective-configuration and tool/version references sufficient to interpret the above (per `configuration-design.md` §13);
- redaction/omission indicators where output policy withheld detail (withholding MUST be indicated, never silent where it affects interpretation).

### 4.2 Preservation obligations

- A renderer MUST NOT omit required evidence in a way that makes output semantically misleading (see §5).
- Non-verdict reasons MUST survive rendering (see §6).
- Capability/completeness context MUST survive rendering where required to prevent false-secure reading.
- Diagnostics MUST remain distinguishable from tenant findings (output-architecture §15).
- Where a format cannot directly encode a distinction, §10 (SARIF) and analogous format-specific mapping rules require a documented deterministic mapping that preserves the original state/context — never a collapse.

---

## 5. PASS/FAIL evidence

**PASS** and **FAIL** evidence requirements MUST survive rendering (INV-06; rule-engine §§5–6, 15).

1. Each rendered **PASS**/**FAIL** MUST carry or reference the traceable evidence/provenance references supplied by the authoritative result, per the output/redaction policy. Evidence references identify collected normalized facts; renderers project those references — they do not resolve, expand, or manufacture them.
2. A renderer MUST NOT omit required evidence in a way that makes the output semantically misleading. Detail-depth options (see `configuration-design.md` §9) MAY control optional elaboration but MUST NOT remove the evidence minimum required to justify the verdict.
3. Redaction that withholds evidence detail MUST indicate withholding per output-architecture §14 and MUST NOT transform the verdict, fabricate substitute evidence, or present the assessment as more complete than it is.
4. Evidence text derived from provider/tenant strings MUST be sanitized per §14 (escaping/encoding, no executable interpretation) while preserving its explanatory content.
5. No renderer may copy raw provider payloads, secret-bearing values, or authentication material into evidence display (see §14).

---

## 6. Non-verdict states

**NOT_EVALUATED**, **NOT_APPLICABLE**, and **ERROR** MUST remain mutually distinguishable in every format, with their structured reasons preserved.

1. A renderer MUST NOT collapse non-verdict states into generic success/failure buckets (e.g., rendering **NOT_EVALUATED** as passing, **ERROR** as failing, or all three as "skipped/informational" without preserving the original state).
2. Each non-verdict evaluation MUST carry its structured reason/context: for **NOT_EVALUATED**, the missing capability/data and capability state that caused the gap; for **NOT_APPLICABLE**, the identity kind/context and applicability criterion; for **ERROR**, the failure explanation sufficient to understand the loss of trust, without secret or raw-payload exposure.
3. Incomplete-assessment conditions (unavailable capability, partial collection, per-rule **NOT_EVALUATED**/**ERROR**, evidence-integrity problems, output-generation failures) MUST be visible in summaries, not only in detail sections (see §§8–11).
4. Counts, groupings, and summaries MUST enumerate non-verdict states separately. A summary that reports only pass/fail counts while non-verdict evaluations exist is semantically misleading and prohibited.
5. Where SARIF or another constrained format cannot natively distinguish the three states, §10 requires a documented deterministic mapping that preserves each original state/context through a supported representation — never a lossy collapse.

---

## 7. Determinism

Given the same authoritative result and the same renderer configuration/options, rendering MUST be semantically deterministic (INV-02 applied to projection; output-architecture §18).

### 7.1 Required stability

- **Stable ordering where observable:** rule-evaluation ordering, finding ordering, evidence-reference ordering, and diagnostic ordering MUST follow a deterministic order (e.g., rule-ID ordering or another explicitly documented stable order owned by the renderer contract). Source-iteration, filesystem-enumeration, or concurrency-completion order MUST NOT leak into artifacts.
- **Stable identifier representation:** rule IDs/versions, assessment/subject references, and provenance references MUST render identically across runs given identical input.
- **Deterministic collection ordering where required:** any collection whose order affects interpretation MUST be normalized to a stable order before serialization.
- **No random semantic changes:** no randomness, probabilistic branching, sampling, or AI inference on the rendering path.
- **No ambient wall-clock values that alter assessment meaning:** wall-clock reads MUST NOT change states, evidence, completeness, or capability representation. Presentation-only timestamps (if ever introduced), file-metadata times, and other inherently non-semantic values MUST be explicitly separated from assessment meaning and MUST NOT be interpretable as assessment-time references.

### 7.2 Non-overclaim

Determinism here means semantic determinism (identical security meaning), NOT byte identity. Byte-identical files, identical whitespace, identical property ordering beyond the stable-ordering requirement, identical timestamps, and identical environment-specific paths are NOT claimed unless a canonical serialization is explicitly designed later (output-architecture §§6, 18 — TBD, see §20).

---

## 8. CLI/text renderer

Human-readable, operator-facing projection. No concrete CLI framework, formatting library, command syntax, or exit-code mapping is chosen in this document (see §20).

### 8.1 Responsibilities

The CLI/text renderer SHOULD communicate, within human-readable constraints:

- per-evaluation state using the five states without aliasing;
- subject and rule context sufficient to locate each evaluation (tenant-safe identifiers, rule ID/version);
- findings with their linkage to evaluations;
- evidence and reason context: evidence references for **PASS**/**FAIL**, structured reasons for **NOT_EVALUATED**/**NOT_APPLICABLE**/**ERROR**;
- capability/completeness context and explicit state counts that make incomplete/error conditions visible;
- renderer/output failures and truncation indicators where applicable;
- deterministic machine-usable exit behavior owned by the exit-mapping policy (mapping itself TBD — the renderer contract defines that incomplete/error conditions MUST be mappable to a non-success indication, not what the codes are).

### 8.2 Prohibitions

- Visually convenient simplification MUST NOT hide **NOT_EVALUATED**/**ERROR**/partial conditions. A concise default summary MUST still surface explicit state counts and visible error/incomplete indicators (output-architecture §5).
- The renderer MUST NOT print tokens/secrets, emit raw authentication material, execute terminal control sequences derived from untrusted data, or use AI-generated interpretation as authoritative status (output-architecture §5; see also §14).
- The renderer MUST NOT decide organizational CI gating policy. It preserves explicit incomplete/error conditions for CI consumption; CI policy itself remains TBD (output-architecture §24).

---

## 9. JSON renderer

Machine-readable projection. V1 treats JSON as the canonical machine-readable export surface unless a later architecture decision defines otherwise (output-architecture §6). No final JSON schema, schema-version value, property-naming choice, or serialization library is invented in this document (see §20).

### 9.1 Expectations

- Preserve all five evaluation states as distinct machine-readable values (no aliasing, no sixth value).
- Preserve stable machine-readable identifiers (assessment, rule ID/version, subject/provenance references) so consumers can join evaluations to findings, evidence, capability state, and effective configuration.
- Preserve tenant context, capability/diagnostic information needed for correct interpretation, and evidence/provenance references per output policy.
- Support schema versioning: artifacts MUST carry or be associable with an output-schema/version identifier plus tool and rule-version references sufficient for forward-compatibility handling (see §18). Exact representation TBD.
- Use standards-compliant serialization; treat all tenant/provider-derived strings as data, never executable content.
- Maintain the §7 determinism posture at the semantic level; byte-level canonicalization (if ever required) is a separate TBD.

### 9.2 Non-invention

This document defines NO JSON property names, schema structure, required/optional field lists, enum spellings, or version-identifier format. Claiming a final schema here would be invention. The schema and its versioning strategy are TBDs (see §20).

---

## 10. SARIF renderer

Interoperability representation only — NOT the source of truth (output-architecture §7).

### 10.1 Boundary

- This document does NOT claim SARIF natively represents all EntraNHI semantics one-to-one. Known asymmetries include: **PASS**/**NOT_EVALUATED**/**NOT_APPLICABLE**/**ERROR** have no single natural SARIF-result counterpart in the way **FAIL** findings may map to SARIF results; capability/completeness context, provenance chains, and structured non-verdict reasons require accompanying metadata/diagnostics rather than direct field correspondence.
- Where SARIF cannot directly encode a semantic distinction, the renderer MUST apply a documented deterministic mapping that preserves the original EntraNHI state and context through an appropriate supported representation (e.g., state-preserving metadata/diagnostics/taxonomy associations — without prescribing which SARIF fields here, since exact mapping is TBD). The mapping MUST be deterministic: the same EntraNHI state/context always maps the same way, and the original state MUST be recoverable or explicitly stated in the artifact.
- NEVER collapse the five-state model merely to fit SARIF. In particular: **ERROR** MUST NOT become a **FAIL**-equivalent, **NOT_EVALUATED** MUST NOT become a **PASS**-equivalent, and **NOT_APPLICABLE** MUST NOT disappear. Each MUST remain distinguishable with its reason/context.
- Rule IDs/versions and evidence references MUST remain traceable through the mapping.
- Untrusted strings MUST remain data in all SARIF string positions (see §14).

### 10.2 Non-invention

Exact SARIF version, profile, field mapping, and conformance claims remain TBD unless already established elsewhere (none are in V1 architecture). This document MUST NOT be read as selecting SARIF fields, levels, kinds, or URIs. Do not invent SARIF semantics. Validation of any future mapping against the relevant SARIF specification is required before the mapping is normative.

---

## 11. HTML renderer

Standalone human-readable report projection. No templating approach, asset strategy, CSP value, or library is chosen in this document (see §20).

### 11.1 Expectations

- Project the same semantic content as §§4–7: states, subject/rule context, findings, evidence/reason context, capability/completeness context, diagnostics, and withholding indicators — in a self-contained static-report posture where practical (output-architecture §8).
- Contain NO executable assessment authority: the report MUST NOT re-evaluate, re-query, authenticate, or mutate tenant state. Scripting that re-derives verdicts, fetches tenant data, or transmits assessment data is prohibited. Whether any scripting exists at all, and under what hardening, is TBD — the secure posture is a static report requiring no execution of tenant-supplied active content.
- Exclude secrets and raw provider payloads where prohibited (see §14).
- Address safe rendering and escaping of ALL untrusted or provider-derived normalized text: display names, application names, descriptions, owner/accountability text, permission labels, evidence text, diagnostics, and externally configurable rule metadata. Anticipate HTML/script injection, URL/link injection, unsafe inline content, style injection where applicable, unsafe embedded data, dangerous URI schemes, and malformed Unicode (output-architecture §8). Dangerous URI schemes MUST NOT be rendered as actionable links.
- Prefer (but do not mandate here) a self-contained static asset posture; any external resource reference that would transmit tenant-sensitive data or permit active-content injection is prohibited (see output-architecture §22).

---

## 12. Output failure semantics

Rendering failure is operational/output failure. It MUST NOT mutate the underlying authoritative assessment result.

1. A failed renderer MUST NOT turn **ERROR**/**NOT_EVALUATED** into **PASS**/**FAIL** or vice versa. The authoritative `RuleEvaluation` set is unchanged by renderer outcomes.
2. If one renderer fails while another succeeds, the authoritative assessment result is preserved and each renderer's independent success/failure status is exposed explicitly through the surrounding result/error contracts owned by `error-result-model.md` (structured diagnostics indicating which renderer, which scope, and whether the artifact is absent, partial, or complete).
3. No sixth assessment state is introduced to represent renderer failure. Renderer failure is a system/output diagnostic (output-architecture §§15–16), NOT a tenant finding and NOT a new verdict.
4. Exact fatal-vs-isolated renderer failure policy (whether one failure halts the pipeline, which failures are fatal, exit-behavior consequences) remains TBD (see §20) within these fail-safe constraints: failure MUST be visible, MUST NOT suppress incomplete-assessment visibility, and MUST NOT fabricate a replacement result.
5. Cancellation and resource-exhaustion during rendering are renderer failures for purposes of this section (see §17).

---

## 13. Destination/write boundary

Separate rendering/projection from persistence/write concerns where architecturally appropriate. No filesystem/storage APIs, path libraries, atomicity mechanisms, or permission modes are chosen in this document (see §20).

### 13.1 Separation

- **Projection** builds the format-specific representation (and its in-memory integrity indication). **Persistence** validates destinations, publishes artifacts, and reports write outcomes. A semantically correct projection paired with a failed write is a failed delivery, NOT a successful assessment report.
- Overwrite, versioning, retention, and artifact-permission posture are delivery policies, not rendering semantics. Do NOT silently overwrite existing artifacts by default without an explicit validated policy (output-architecture §11).

### 13.2 Safe publication

Future implementation MUST (as implementation obligations of these contracts, per `solution-structure.md` §5.4):

- validate operator-configured destinations; tenant/provider-controlled values MUST NOT directly determine filesystem paths without validated safe transformation (output-architecture §10);
- defend against path traversal, absolute-path injection, unsafe/reserved file names, symlink/reparse-point hazards where applicable, escaping the intended destination, race/partial-write hazards, and unsafe temporary files;
- build/serialize before final publication where practical, carry an explicit completion/integrity indication, prefer atomic replacement where supported and validated, and clean up or quarantine failed temporary artifacts;
- never leave a partial/truncated artifact that appears complete. Truncation, size-limit enforcement, or write failure MUST be explicit (visible error plus integrity indication), never a silently "complete" report.

Exact file-naming, path-validation, overwrite/versioning, safe-write, temporary-file, and permission mechanisms remain TBD (see §20).

---

## 14. Security and sanitization

### 14.1 Excluded content (INV-09; output-architecture §13)

Outputs MUST exclude — in artifacts, console output, diagnostics, and logs produced by rendering:

- passwords and client-secret values;
- access tokens, refresh tokens, authentication cookies/headers, and other raw authentication material;
- private keys and certificate private material;
- recovery secrets;
- other secret-bearing values and secret-bearing configuration.

Credential metadata explicitly classified as non-secret by the owning contract MAY appear only where output policy permits it; this document permits none by itself. Renderers MUST NOT log excluded values.

### 14.2 Required safe encoding/escaping

Require safe encoding/escaping appropriate to each format for ALL untrusted normalized strings (display names, evidence text, rule metadata where externally configurable, diagnostics, provider-derived labels):

- **HTML:** element/attribute/URL-context encoding, dangerous-scheme neutralization, malformed-Unicode handling (see §11).
- **Terminal/text:** ANSI/control-sequence neutralization, terminal-title-manipulation defense, carriage-return/line-feed injection defense, bidi/control-character handling, malformed-Unicode handling, log-forging defense (output-architecture §9).
- **JSON:** standards-compliant string encoding preserving data fidelity while preventing executable interpretation; no string concatenation that breaks structural validity.
- **SARIF:** field-safety treatment so untrusted strings cannot alter SARIF structural meaning or introduce executable/URI behavior.

No display name is assumed safe because it came from a provider. Sanitization MUST NOT alter assessment states or remove required semantic information (see §§4–6); where sanitization necessarily alters display fidelity (e.g., neutralized control characters), the alteration MUST NOT change meaning and SHOULD be handled per the future sanitization policy TBD (see §20).

### 14.3 Confidentiality posture

Artifacts may contain sensitive enterprise security information (tenant identity detail, findings, privilege relationships, permission exposure, ownership/accountability, credential metadata, tenant structure — output-architecture §12). Renderer contracts therefore support, but do not themselves implement: restrictive artifact handling, explicit destinations, safe CI-artifact handling awareness, retention awareness, redaction (see below), and operator warning/documentation hooks. Exact permission modes and encryption mechanisms are TBD (see §20); this document claims no OS-level confidentiality guarantee.

### 14.4 Redaction interaction

Output-specific redaction MAY reduce disclosure per output-architecture §14, subject to: redaction MUST NOT alter authoritative states, fabricate evidence, transform **ERROR**/**NOT_EVALUATED** into secure-looking output, or silently remove information necessary to understand limitations. Where redaction materially affects interpretation, the artifact MUST indicate that information was withheld. Exact redaction schema/policy is TBD (see §20).

---

## 15. Tenant isolation

### 15.1 Single-tenant artifact scope (INV-10)

Each V1 assessment run targets a single tenant assessment context. Each output artifact MUST be scoped to exactly one tenant assessment scope. An artifact MUST NOT combine, correlate, or co-render data from different tenant assessment scopes unless a future explicitly authorized aggregation contract exists — none exists in V1.

### 15.2 Contamination as integrity failure

Cross-tenant output contamination — evaluations, findings, evidence, provenance, capability state, diagnostics, or configuration provenance from tenant A appearing in tenant B's artifact — is an integrity failure. It MUST fail safely and visibly through the shared error/result vocabulary (never silent continuation, never **PASS**-by-contamination). Tenant-context mismatch detected at render time MUST prevent publication of the contaminated artifact.

### 15.3 Renderer obligations

- Renderers MUST verify tenant-scope carriage on their input (tenant-context reference consistent across assessment identity, evaluations, evidence, and provenance) rather than assuming it.
- Renderers MUST NOT introduce cross-tenant correlation (no joins, rollups, or comparisons across scopes), cross-run caching of tenant-derived strings, or shared mutable rendering state across runs.
- Destination handling MUST NOT mix scopes: output paths, artifact names, and publication steps MUST NOT allow one tenant's artifact to overwrite, append to, or merge with another's. Tenant-controlled values MUST NOT determine paths (see §13).
- Tests MUST prove isolation with synthetic tenant identifiers; real tenant/customer/employer data MUST NOT be used (see §19).

---

## 16. Provenance and evidence

1. Renderers project normalized evidence and provenance references supplied by the authoritative result (source-system reference, object reference, collection operation/context, assessment context, rule ID/version, effective-configuration reference). They do NOT independently resolve provider data, fetch source objects, expand raw payloads, or manufacture provenance.
2. Provenance MUST be preserved across the render boundary sufficient for audit: a consumer of any format MUST be able to trace a **PASS**/**FAIL** back to the collected normalized facts and collection context that support it (within output/redaction policy), and to trace each non-verdict state to its structured reason.
3. Provenance MUST NOT include secret material (see §14). Collection-window/provenance detail that would expose secrets or raw payloads MUST be represented only through its non-secret reference form.
4. Effective-configuration provenance (per `configuration-design.md` §13) is projected, not altered: renderers MUST NOT reinterpret which configuration affected the run, and MUST indicate defaulted-vs-supplied and withheld-under-redaction conditions where the output contract requires it.

---

## 17. Large-output/resource behavior

Rendering MUST be bounded. No concrete limits are set in this document (see §20).

### 17.1 Defensive posture

Future implementation MUST address, as implementation obligations of these contracts: bounded memory usage where practical; streaming where appropriate; cancellation responsiveness; large evidence sets; pathological strings; deeply nested structures; excessive report size; and serialization failures (output-architecture §20). Streaming/buffering strategy and exact bounds are TBD.

### 17.2 Fail-visible truncation/exhaustion

Truncation, size-limit enforcement, cancellation, or resource exhaustion MUST fail visibly or be explicitly represented per the surrounding operational contracts owned by `error-result-model.md`:

- The artifact MUST carry an explicit incomplete/truncated indication where partially produced, OR be withheld/quarantined as failed with an explicit diagnostic — never silently presented as complete.
- Exhaustion MUST NEVER silently produce a misleading "complete" report and MUST NEVER convert **NOT_EVALUATED**/**ERROR** toward **PASS**/**FAIL**.
- Summaries MUST NOT claim full coverage where detail was truncated. If counts cannot be trusted due to truncation, the counts themselves MUST be marked untrustworthy or omitted with an explicit reason — never fabricated.

---

## 18. Output compatibility/versioning

Contract-level expectations only. No final schema, version-identifier format, numbering scheme, or migration mechanism is invented in this document (see §20).

1. Machine-readable outputs (JSON, SARIF) require explicit versioning: an output-schema/version identifier, tool version, and rule ID/version references sufficient to interpret the artifact and to handle unsupported versions explicitly (output-architecture §19).
2. Forward-compatibility strategy MUST be explicit: unknown fields/states in a newer artifact consumed by an older reader MUST NOT be silently reinterpreted. Readers MUST handle unsupported versions visibly (reject, warn-and-constrain to supported subset with explicit indication, or another explicitly specified behavior — chosen later, not here).
3. Breaking schema changes MUST be deliberate and documented. Additive extension is preferred per `README.md` §8.
4. Human-readable outputs (CLI/text, HTML) SHOULD carry sufficient version/context references to bind the rendered view to its schema revision and rule set, so a screenshot or printed section remains attributable.
5. Historical artifacts MUST remain interpretable against the schema revision that produced them; reinterpreting history under a newer schema without explicit migration semantics is prohibited.

---

## 19. Testing implications

Renderer design MUST support deterministic tests covering at least the following. No test framework is chosen here; no tests are created in this phase. The scenario catalog belongs to `testing-seams.md` (derived from `testing-architecture.md` and output-architecture §26); this section states only what renderer contracts MUST make testable.

| # | Testable property | Implication for contracts |
| --- | --- | --- |
| T-OUT-01 | All five states preserved per format | Synthetic five-state fixtures MUST render distinctly in CLI/text, JSON, SARIF, and HTML with reasons preserved for non-verdict states. |
| T-OUT-02 | Evidence preservation | **PASS**/**FAIL** fixtures with evidence/provenance references MUST prove references survive rendering (within redaction policy) and that omission-without-indication is a test failure. |
| T-OUT-03 | Non-verdict reasons | **NOT_EVALUATED**/**NOT_APPLICABLE**/**ERROR** fixtures MUST prove structured reasons survive and remain distinguishable; collapse-into-generic-bucket fixtures MUST fail. |
| T-OUT-04 | Malformed / untrusted text | Adversarial display-name/evidence/diagnostic payloads (markup, script, URI schemes, control sequences, CR/LF, bidi, malformed Unicode) MUST prove safe encoding per format without state/semantic change. |
| T-OUT-05 | Escaping / encoding | Format-specific encoding tests (HTML/XSS, ANSI/control, JSON structural validity, SARIF field safety) MUST be expressible against renderer contracts without live tenant data. |
| T-OUT-06 | Tenant isolation | Synthetic multi-tenant fixtures MUST prove no cross-scope mixing in any format; mismatch fixtures MUST yield visible integrity failure and prevent contaminated publication. |
| T-OUT-07 | Deterministic ordering | Reordered-but-equivalent input fixtures MUST yield semantically identical artifacts; ordering-sensitive fixtures MUST prove stable order. |
| T-OUT-08 | Renderer failure | Per-renderer exception/failure-injection fixtures MUST prove failure is explicit, preserves the authoritative result, exposes independent per-renderer status, and introduces no sixth state. |
| T-OUT-09 | Cancellation / resource limits | Oversized/pathological fixtures and cancellation fixtures MUST prove fail-visible truncation/exhaustion handling (no silent "complete" artifact) without asserting invented numeric bounds from this document. |
| T-OUT-10 | Secret exclusion | Secret-bearing and raw-payload fixtures (synthetic placeholders, never real secrets) MUST prove exclusion from artifacts, console output, diagnostics, and logs. |
| T-OUT-11 | Semantic equivalence across formats | Same-authoritative-result fixtures rendered to multiple formats MUST prove semantic equivalence (same states, same evidence/reason coverage, same completeness posture) despite structural differences. |
| T-OUT-12 | Renderer non-authority | Mutation-attempt fixtures MUST prove renderers cannot alter states, invent findings/evidence, recollect data, or reach provider/auth/AI surfaces. |
| T-OUT-13 | Destination / safe-write behavior | Path-traversal, absolute-path, unsafe-name, overwrite, and partial-write fixtures MUST prove safe handling per §13 without a live filesystem dependency beyond synthetic doubles. |
| T-OUT-14 | Versioning / compatibility | Unknown-field/version fixtures MUST prove explicit handling (no silent reinterpretation) once versioning representation is defined. |

All renderer tests MUST use synthetic fixtures, MUST NOT require a live tenant, production credentials, network transmission, or real customer/employer data, and MUST verify no renderer requires network/telemetry by default (output-architecture §§22, 26).

---

## 20. Implementation TBDs

Unresolved choices are preserved explicitly rather than guessed. Each TBD states what is unknown, why it cannot be resolved here, and which document owns its resolution. None is resolved in this document.

| # | TBD | Why unresolved here | Owned by |
| --- | --- | --- | --- |
| OUT-T-01 | Concrete renderer interfaces/types and canonical output-model type shape. | Member-level shapes belong to the coordinated contract surface, not to this obligation-level document (per `README.md` §6). | `core-contracts.md` coordinating this document with `rule-engine-contracts.md`, `findings-evidence-schema.md`, `capability-model.md`, `error-result-model.md`. |
| OUT-T-02 | Serialization library and canonical-serialization strategy (including whether byte-identity is ever claimed). | Library/strategy selection requires later detailed design; architecture explicitly defers byte-identity claims. | Later detailed design constrained by §§7, 9 of this document; no library chosen here. |
| OUT-T-03 | Final JSON schema, property names, enum spellings, required/optional lists, and schema-versioning representation/strategy. | Defining a schema here would invent a machine-readable contract without implementation validation. | Later JSON-schema design under this document's §§4–7, 9, 18 constraints; format owned by this document, representation TBD. |
| OUT-T-04 | SARIF version/profile and exact SARIF field mapping (including how each of the five states plus reasons is represented). | Mapping requires validation against the relevant SARIF specification; inventing field correspondence would risk misleading semantics. | Later SARIF-mapping design under this document's §10 constraints; MUST be validated against the specification before normative. |
| OUT-T-05 | HTML templating approach, asset strategy, CSP/security policy, and browser-hardening posture. | Rendering-technology and hardening choices require later detailed design; architecture defers them explicitly. | Later HTML-rendering design under this document's §11 + §14 constraints. |
| OUT-T-06 | CLI command syntax, formatting library, terminal-sanitization policy, Unicode/bidi handling policy, and exit-code mapping. | Host/presentation detail requiring later design; choosing it here would preempt host contracts. | Later CLI design under this document's §8 + §14 constraints with configuration semantics from `configuration-design.md`; composition in `EntraNHI.Cli` per `solution-structure.md` §13. |
| OUT-T-07 | Destination abstraction: file-naming, path-validation strategy, overwrite/versioning policy, safe-write/atomic-publication implementation, temporary-file strategy, artifact file permissions, encryption requirements. | Filesystem implementation detail explicitly deferred by architecture. | Later detailed design under this document's §§13–14 constraints. |
| OUT-T-08 | Output versioning representation and forward/backward-compatibility mechanics. | Cross-contract decision (output + configuration + rule versions) requiring later coordination. | This document owns the requirement (§18); representation TBD coordinated with `core-contracts.md` and `configuration-design.md`; no format chosen here. |
| OUT-T-09 | Streaming/buffering strategy and exact size/resource bounds per renderer. | Bounds require later performance and serialization analysis; inventing numbers would be speculation. | Later detailed design under this document's §17 constraints with `dependency-boundaries.md` for enforcement mechanics. |
| OUT-T-10 | Redaction schema/policy (what may be withheld, how withholding is indicated). | Disclosure policy requires later requirements/architecture coordination. | Later redaction design under output-architecture §14 and this document's §§4–5, 14 constraints. |
| OUT-T-11 | Renderer failure-isolation policy (fatal vs isolated) and output logging schema. | Requires coordination with engine error taxonomy and pipeline orchestration. | `rule-engine-contracts.md` + `error-result-model.md` (failure mapping) with orchestration in Application; logging schema TBD under output-architecture §21. |
| OUT-T-12 | CI gating policy semantics consuming these outputs. | Organizational policy decision outside renderer authority. | TBD outside this document; renderers only preserve explicit conditions for CI consumption (output-architecture §24). |
| OUT-T-13 | Per-seam fakes/mocks/fixtures, adversarial payload catalog, golden/snapshot policy for outputs, test-framework choice, artifact signing/hashing integration, future dashboard contract. | Test authoring and supply-chain/build integration belong to their owning documents. | `testing-seams.md` (fixtures/scenarios), later build controls (signing/hashing), future UI architecture (dashboard) — this document only guarantees the seams exist. |

---

## 21. Traceability and acceptance

### 21.1 Invariant traceability

| Invariant | Preserved by |
| --- | --- |
| INV-02 deterministic assessment | §§2, 7, 18 |
| INV-03 provider isolation / INV-04 normalized boundary | §§1–2, 16 |
| INV-05 explicit five states | §§3–6, 8–11 |
| INV-06 evidence traceability / INV-11 provenance | §§4–5, 16 |
| INV-08 least privilege | §§1–2 (no privilege requests from renderers) |
| INV-09 secret exclusion | §14 (+ §§2, 12–13, 16, 19) |
| INV-10 tenant isolation | §15 (+ §§2, 13, 19) |
| INV-12 core/output separation | §§1–2, 4, 12 |
| INV-13 AI non-authority | §1 (+ §§2, 19) |
| INV-14 failure transparency | §§4, 6, 8, 12–13, 17 |
| INV-15 no undocumented dependency | §§1–2, 10 (no invented provider/SARIF semantics) |
| INV-16 security-sensitive defaults | Via `configuration-design.md` §§7, 9 interaction (output options default to semantic-preserving posture) |

### 21.2 Acceptance criteria

This document is accepted when:

1. Renderer non-authority (§1) is stated without exception paths permitting evaluation, recollection, authentication, state alteration, evidence fabrication, or AI judgment.
2. The contract model (§2) covers identity, format, input, options, operation, result, diagnostics/failure, and destination/write separation without choosing concrete C# types/APIs or libraries.
3. Exactly the five states **PASS**, **FAIL**, **NOT_EVALUATED**, **NOT_APPLICABLE**, **ERROR** appear as assessment states (§3); no sixth state, alias, or renderer-specific verdict is introduced in any section including SARIF/HTML/CLI handling.
4. Semantic preservation (§4), evidence (§5), non-verdict (§6), and determinism (§7) obligations are stated such that no format may mislead about security meaning.
5. CLI (§8), JSON (§9), SARIF (§10), and HTML (§11) sections each preserve semantics within their medium without inventing schemas, mappings, templates, syntax, libraries, or specifications.
6. Failure (§12), destination (§13), security (§14), tenant (§15), provenance (§16), resource (§17), versioning (§18), and testing (§19) sections each preserve fail-safe, isolation, exclusion, and determinism properties without inventing mechanisms, modes, bounds, or formats.
7. Every implementation-sensitive unknown is listed in §20 with its owning document; no TBD is resolved by speculation and no Graph endpoint/permission/SDK/licensing/property/Agent Identity mapping is invented.

---

*(End of file)*
