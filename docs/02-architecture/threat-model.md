# Threat Model

> **Status:** Phase 0.2.9
> **Date:** 2026-09-18

---

## Purpose

Define the enterprise-grade V1 threat model for EntraNHI. This document identifies assets, threat actors/sources, threat categories, existing architectural mitigations, residual risks, and the adversarial analysis matrix. It is based on the actual architecture documented in the EntraNHI architecture documents and does not claim controls that are not yet implemented.

---

## Threat-model approach

EntraNHI V1 uses a structured threat-model approach covering:

1. Asset identification
2. Threat actor/source identification
3. Threat enumeration per category
4. Existing architectural mitigation analysis
5. Residual risk identification
6. Adversarial analysis matrix (A-L)
7. Architecture-invariant cross-references

The approach draws on STRIDE concepts where applicable but does not force threats into inappropriate categories.

---

## Assets

| Asset | Sensitivity | Description |
| --- | --- | --- |
| Tenant directory/security assessment data | High | Identity metadata, permission grants/assignments, ownership/accountability relationships, identity relationships collected from the target tenant |
| Authentication/access material | Critical | Access tokens, refresh tokens where used by a validated mechanism, client secrets, private keys, workload credentials; all are excluded from assessment artifacts, while exact caching/storage and in-memory protection remain deferred |
| Normalized identity data | High | IdentityRecord, CredentialMetadata, PermissionRelationship, AccountabilitySubject, CapabilityState, Provenance after translation from provider-specific objects |
| Identity graph | High | Deterministic projection of normalized domain model containing nodes, edges, capability states, and provenance references |
| Rule definitions/configuration | Medium | Deterministic security rules, rule versions, evaluation predicates, required capabilities, applicability criteria |
| RuleEvaluation records | High | Deterministic evaluation outcomes (PASS, FAIL, NOT_EVALUATED, NOT_APPLICABLE, ERROR) with evidence references and provenance |
| Findings/evidence/provenance | High | Security findings derived from RuleEvaluation records, evidence references tracing to source observations, provenance chain |
| Generated reports/artifacts | High | CLI output, JSON exports, SARIF files, HTML reports containing assessment results, identity metadata, and security findings |
| Diagnostic information | Medium | Collection errors, normalization failures, rule evaluation errors, system diagnostics |
| Tenant context | Medium | Tenant identifier, assessment context, execution context scoping |
| Software/release integrity | Medium | EntraNHI source code, compiled artifacts, dependency chain, build provenance |

---

## Threat actors/sources

| Actor/source | Motivation | Capability | Notes |
| --- | --- | --- | --- |
| Malicious or compromised tenant data | Assessment manipulation, data exfiltration, evasion | Control over tenant-directory objects, API responses, identity metadata | Do NOT claim Microsoft is malicious; provider data may be compromised or manipulated |
| Malicious/untrusted repository input | Supply-chain compromise, code execution | Contribution to source code, dependency injection, rule/configuration tampering | Open-source repository is publicly editable |
| Compromised execution environment | Credential theft, data exfiltration, assessment manipulation | Access to process memory, filesystem, environment variables, runtime state | Operator or CI-controlled machine |
| Unauthorized local user/process | Credential theft, data exfiltration, assessment tampering | Local filesystem access, process memory inspection, artifact access | Other users or processes on operator machine |
| Compromised CI environment | Credential theft, artifact tampering, data exfiltration | Access to CI secrets, build pipeline, output artifacts | CI/CD platform compromise |
| Dependency/supply-chain compromise | Code execution, data exfiltration, assessment manipulation | Malicious library code execution within EntraNHI runtime | Third-party package compromise |
| Network/provider failures or manipulation | Data corruption, denial of service, man-in-the-middle | Network interception, DNS manipulation, TLS interception | Network-layer attacks |
| Accidental operator misconfiguration | Incorrect assessment, credential exposure, insecure defaults | Misconfigured authentication, incorrect tenant targeting, insecure output settings | Human error |
| Malformed/pathological provider data | Assessment manipulation, resource exhaustion, crash | Crafted API responses, oversized payloads, pathological data structures | Provider API returning unexpected data |
| Future malicious plugin/integration | Assessment manipulation, data exfiltration | Third-party rule packs, integrations, extensions | Out of scope for V1 but architecture must anticipate |
| AI/LLM misuse | Verdict manipulation, evidence fabrication, unauthorized access | Prompt injection, model manipulation, unauthorized inference | If AI is introduced post-V1 |

---

## Threat categories and analysis

### 1. Token/credential disclosure

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Authentication/access material (tokens, client secrets, private keys) |
| **Attack/failure path** | Tokens exposed in output artifacts, logs, error messages, diagnostic dumps, process memory dumps, filesystem persistence, CI logs |
| **Security impact** | Credential theft enabling unauthorized tenant access; lateral movement; data exfiltration from target tenant |
| **Existing architectural mitigation** | SEC-001: No credential secret collection or storage; SEC-011: Preserve authorization boundary; INV-09: Secret exclusion — tokens/secrets are runtime-only, excluded from all artifacts; authentication boundary isolates tokens from normalized domain data, findings, evidence, logs, and reports; output renderers exclude secret material [OUT-006]; structured logging excludes credential material [NFR-004] |
| **Residual risk** | Token exposure in process memory readable by other local processes (TBD: in-memory protection); token persistence in CI environment logs if CI platform logs environment variables; exact token-zeroing behavior after use (TBD) |
| **Status** | DESIGNED / REQUIRED (architecture established; implementation TBD for in-memory protection) |

### 2. Excessive privilege

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Tenant directory/security assessment data; authentication/access material |
| **Attack/failure path** | EntraNHI application registration granted more Microsoft Graph permissions than necessary; permission creep over time; unnecessary write permissions granted |
| **Security impact** | Larger blast radius if credentials are compromised; violation of least-privilege principle; potential for unintended tenant-state modification if write permissions exist |
| **Existing architectural mitigation** | INV-08: Least privilege — authentication and authorization use only permissions required for enabled collection capabilities; SEC-008: Least-privilege Graph permissions; FR-030: Minimum permissions for data collection; FR-040: Read-only operation; permission requirements documented per capability [NFR-006] |
| **Residual risk** | Exact Microsoft Graph permission mapping is TBD; permission minimization validation is not yet implemented; no automated permission-audit mechanism exists |
| **Status** | DESIGNED / REQUIRED (architecture established; exact permission mapping and validation TBD) |

### 3. Wrong-tenant execution

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Tenant context; assessment data; findings/evidence |
| **Attack/failure path** | EntraNHI executes against incorrect tenant; operator configures wrong tenant ID; authentication context does not match expected tenant; cross-tenant API response accepted without validation |
| **Security impact** | Assessment of wrong tenant; cross-tenant data contamination; misleading security findings; tenant boundary violation |
| **Existing architectural mitigation** | INV-10: Tenant boundary preservation — data from separate tenant contexts never silently mixed; tenant assessment context established before collection; authentication context reconcilable with tenant context; mismatch must fail safely and visibly [authentication-authorization.md §9] |
| **Residual risk** | Tenant-context validation implementation is TBD; exact tenant-ID validation mechanics not specified; no automated tenant-scope verification |
| **Status** | DESIGNED / REQUIRED (architecture established; validation implementation TBD) |

### 4. Cross-tenant data contamination

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Tenant context; normalized domain data; identity graph; findings/evidence |
| **Attack/failure path** | Data from one tenant silently mixed into another tenant's assessment; cross-tenant edges in identity graph; cross-tenant evidence correlation; output artifacts mixing tenant data |
| **Security impact** | Incorrect security findings; tenant boundary violation; misleading assessment results; enterprise security decisions based on contaminated data |
| **Existing architectural mitigation** | INV-10: Tenant boundary preservation; SourceObservation carries tenant assessment context; identity graph scoped to single tenant per run [INV-G1]; every RuleEvaluation and Finding bound to single tenant context [findings-evidence.md §12]; cross-tenant evidence correlation prohibited in V1 |
| **Residual risk** | Cross-tenant edge prevention relies on correct SourceObservation tenant-context propagation; no formal verification of tenant-context consistency |
| **Status** | DESIGNED / REQUIRED (architecture established; formal verification TBD) |

### 5. Provider-object spoofing/confusion

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Normalized domain data; identity graph; rule evaluation |
| **Attack/failure path** | Malicious or compromised provider returns objects with forged identifiers; provider object spoofed to impersonate another identity; provider response contains objects from different tenant scope |
| **Security impact** | Incorrect identity classification; false relationships in identity graph; misleading findings; potential for attacker-controlled identity data influencing assessment |
| **Existing architectural mitigation** | INV-04: Normalized domain boundary — provider objects translated into stable normalized contracts; INV-03: Provider isolation — rule engine does not directly query providers; capability detection validates collection success; SourceObservation envelope carries source object reference and collection context |
| **Residual risk** | Provider object authenticity relies on TLS transport integrity; no cryptographic verification of provider object provenance; spoofed objects within the same tenant scope may pass normalization without detection |
| **Status** | DESIGNED / REQUIRED (architecture established; cryptographic provenance verification TBD) |

### 6. Malformed provider data

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Normalized domain data; identity graph; rule evaluation; system stability |
| **Attack/failure path** | Provider returns malformed, oversized, or unexpected response structures; pathological data causes normalization failure or resource exhaustion; schema mismatch between expected and actual response |
| **Security impact** | Normalization failure; rule evaluation failure; resource exhaustion; potential for incorrect evaluation if malformed data partially normalizes; denial of assessment |
| **Existing architectural mitigation** | Response schema validation against documented expectations [trust-boundaries.md §1]; structured collection failure categories [collection-architecture.md §10]; normalization-input validation [collection-architecture.md §11.2]; ERROR verdict for unexpected evaluation failures [VERD-005]; INV-14: Failure transparency |
| **Residual risk** | Exact schema validation approach is TBD; pathological data resilience testing not implemented; memory/resource bounds for large payloads TBD |
| **Status** | DESIGNED / REQUIRED (architecture established; schema validation and resource limits TBD) |

### 7. Normalization bypass

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Normalized domain boundary; identity graph; rule evaluation |
| **Attack/failure path** | Provider-specific objects leak into rule contracts; rules directly access provider SDK types; normalization layer bypassed or circumvented; provider-specific properties used in rule logic |
| **Security impact** | Rules operating on unvalidated provider data; provider schema changes silently breaking rule logic; loss of provider isolation; potential for provider-specific behavior influencing assessment |
| **Existing architectural mitigation** | INV-03: Provider isolation — rule engine MUST NOT query providers directly; INV-04: Normalized domain boundary — provider-specific representation MUST NOT leak into rule contracts; INV-G7: Rules consume normalized contracts only; rule engine MUST NOT use provider SDK types [rule-engine.md §2.2] |
| **Residual risk** | Enforcement depends on implementation discipline; no compile-time or runtime verification that rules do not import provider SDK types; normalization boundary not formally verified |
| **Status** | DESIGNED / REQUIRED (architecture established; enforcement mechanism TBD) |

### 8. Provider-specific object leaking into rule logic

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Deterministic rule evaluation; assessment integrity |
| **Attack/failure path** | Rule code references Microsoft Graph SDK object types directly; rule logic depends on provider HTTP semantics; rule uses undocumented provider properties; normalization boundary violated |
| **Security impact** | Rules become coupled to provider schema; provider API changes silently break rules; non-deterministic behavior from provider-specific logic; loss of provider isolation |
| **Existing architectural mitigation** | INV-03: Provider isolation; INV-04: Normalized domain boundary; INV-G7: Rules consume normalized contracts; rule engine MUST NOT use provider SDK types, raw payloads, or API responses [rule-engine.md §2.2, §2.3] |
| **Residual risk** | No automated boundary enforcement; relies on code review and architectural discipline |
| **Status** | DESIGNED / REQUIRED (architecture established; automated enforcement TBD) |

### 9. Capability-state confusion

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Capability state; rule evaluation; findings/evidence |
| **Attack/failure path** | Authorization denial silently converted to PASS; missing capability treated as empty/secure state; capability state lost during normalization; incomplete collection producing false assessment |
| **Security impact** | False PASS verdicts; incomplete assessment appearing complete; security weaknesses not detected; misleading findings |
| **Existing architectural mitigation** | INV-05: Explicit evaluation states — no silent coercion of missing capability into FAIL or PASS; INV-07: Capability awareness — unavailable capability MUST NOT automatically resolve to FAIL; CAP-002: Not-auto-FAIL for missing capabilities; rule engine validates required capabilities before evaluation [rule-engine.md §10.4]; capability state flows into evaluation through explicit mechanism, not authentication-layer override |
| **Residual risk** | Capability-state propagation correctness depends on implementation; no formal verification that capability gaps remain explicit and follow each rule's documented unavailable-input semantics rather than producing false PASS/FAIL |
| **Status** | DESIGNED / REQUIRED (architecture established; verification TBD) |

### 10. Incomplete collection producing false PASS

| Aspect | Description |
| --- | --- |
| **Threatened asset** | PASS verdicts; findings/evidence; assessment integrity |
| **Attack/failure path** | Collection partially fails; missing data treated as "nothing found"; rule evaluates absence of data as PASS without verifying collection completeness; partial collection not propagated to rule engine |
| **Security impact** | Security weakness not detected; false sense of security; misleading assessment; identity with security issue receiving PASS verdict |
| **Existing architectural mitigation** | INV-14: Failure transparency — collection failures produce explicit state; rule engine §4.1: Missing data MUST NOT automatically become PASS; PASS requires sufficient input completeness [rule-engine.md §5]; capability state propagated from collectors through SourceObservation to rule engine; rule definitions must specify required capabilities and behavior when inputs are unavailable |
| **Residual risk** | PASS semantics depend on correct rule definitions specifying completeness requirements; no automated verification that all rules correctly gate PASS on collection completeness |
| **Status** | DESIGNED / REQUIRED (architecture established; rule-level completeness verification TBD) |

### 11. Authorization denial producing false PASS

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Verdicts; findings/evidence; assessment integrity |
| **Attack/failure path** | Authorization denied for a capability; capability state not propagated to rule engine; rule evaluates missing authorization data as "no issues found"; authorization denial converted to PASS |
| **Security impact** | Security weakness not detected; assessment appears more complete than it is; misleading findings; enterprise security team makes decisions based on incomplete assessment |
| **Existing architectural mitigation** | INV-05: Explicit evaluation states; INV-07: Capability awareness; authorization denial MUST NOT automatically become FAIL or PASS [authentication-authorization.md §5.2]; capability state flows through explicit mechanism to rule engine; rules gated on required capabilities [rule-engine.md §11] |
| **Residual risk** | Capability-state propagation correctness depends on implementation; no automated verification |
| **Status** | DESIGNED / REQUIRED (architecture established; verification TBD) |

### 12. Authentication/system failure producing tenant FAIL

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Verdicts; findings/evidence; assessment integrity |
| **Attack/failure path** | Authentication fails; system error occurs; infrastructure failure; error condition incorrectly mapped to tenant security FAIL |
| **Security impact** | Misleading FAIL finding attributed to tenant security posture; tenant unfairly characterized as insecure; false positive security finding |
| **Existing architectural mitigation** | INV-14: Failure transparency — operational failures remain explicit and preserve relevant context; they do not themselves select an assessment state and never silently become PASS or tenant-security FAIL. When an operational condition merely prevents assessment, applicable evaluation/rule semantics and later failure-mapping mechanics determine the legitimate handling [rule-engine.md §6]. A separate rule may explicitly evaluate the condition as assessment data under a documented requirement, in which case normal deterministic rule semantics govern its state; diagnostics remain distinct from tenant findings [findings-evidence.md §15]. |
| **Residual risk** | Implementation must correctly distinguish infrastructure errors from security failures; error categorization depends on correct implementation |
| **Status** | DESIGNED / REQUIRED (architecture established; implementation TBD) |

### 13. Evidence fabrication

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Findings/evidence; provenance; assessment integrity |
| **Attack/failure path** | Evidence references invented without corresponding source observations; provenance chain broken or fabricated; evidence attached to verdicts without supporting collected data; AI-generated text used as factual evidence |
| **Security impact** | Misleading findings; audit trail integrity compromised; assessment results not trustworthy; enterprise security decisions based on fabricated evidence |
| **Existing architectural mitigation** | INV-06: Evidence traceability — PASS/FAIL require traceable evidence; provenance chain explicit [findings-evidence.md §6]; rule engine MUST NOT fabricate evidence [rule-engine.md §15.2]; AI MUST NOT create factual evidence [findings-evidence.md §21]; evidence fabrication defense [findings-evidence.md §18]; if required evidence cannot be constructed, do not silently emit trustworthy PASS/FAIL |
| **Residual risk** | Evidence fabrication prevention depends on correct implementation; no cryptographic integrity verification of evidence chain (TBD) |
| **Status** | DESIGNED / REQUIRED (architecture established; integrity verification TBD) |

### 14. Provenance loss

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Provenance; evidence traceability; audit integrity |
| **Attack/failure path** | Provenance references lost during normalization or graph construction; evidence cannot trace back to source observation; provenance chain incomplete; collection context lost |
| **Security impact** | Findings not auditable; evidence cannot be validated against source data; assessment reproducibility compromised; enterprise audit requirements not met |
| **Existing architectural mitigation** | INV-11: Provenance preservation — normalized observations retain source and collection context; provenance model captures source system, source object reference, collection operation, assessment context, observation time [domain-model.md §Provenance]; provenance flows through SourceObservation to normalized domain to identity graph to findings |
| **Residual risk** | Provenance preservation correctness depends on implementation at each transformation boundary; no formal verification of provenance chain integrity |
| **Status** | DESIGNED / REQUIRED (architecture established; verification TBD) |

### 15. Rule/configuration tampering

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Rule definitions/configuration; deterministic evaluation; assessment integrity |
| **Attack/failure path** | Rule definitions modified to produce different verdicts; rules disabled or skipped without notification; configuration weakened to bypass security-critical rules; malicious rule pack introduced |
| **Security impact** | Incorrect verdicts; security weaknesses not detected; assessment integrity compromised; tampered assessment results |
| **Existing architectural mitigation** | INV-16: Security-sensitive defaults — configuration defaults favor assessment integrity; rule identity and versioning [rule-engine.md §8]; active rule set and version recorded in assessment metadata; disabled rules diagnosable; invalid configuration fails visibly; unknown configuration options MUST NOT silently change security semantics; third-party extension model outside V1 scope [rule-engine.md §18.3] |
| **Residual risk** | Rule integrity verification (signing, hashing) is TBD; configuration tampering detection is TBD; no automated rule-integrity validation |
| **Status** | DESIGNED / REQUIRED (integrity verification TBD) |

### 16. Renderer changing security meaning

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Output artifacts; findings/evidence; assessment integrity |
| **Attack/failure path** | Renderer alters RuleEvaluation state; ERROR converted to FAIL; NOT_EVALUATED hidden; PASS/FAIL reversed; severity reinterpreted; output appears more secure than assessment |
| **Security impact** | Misleading assessment output; security weaknesses hidden; enterprise decisions based on inaccurate output |
| **Existing architectural mitigation** | INV-12: Core/output separation — output renderers cannot alter evaluation semantics; output layer MUST NOT rerun rules, alter state, reinterpret verdicts, or fabricate evidence [output-architecture.md §1]; all five evaluation states preserved and distinguishable [output-architecture.md §3]; renderer failure is system diagnostic, not tenant finding |
| **Residual risk** | Renderer isolation depends on implementation; no formal verification that renderers do not access or modify canonical state |
| **Status** | DESIGNED / REQUIRED (architecture established; formal verification TBD) |

### 17. HTML/XSS injection

| Aspect | Description |
| --- | --- |
| **Threatened asset** | HTML output; output consumers; assessment integrity |
| **Attack/failure path** | Provider/tenant-originated strings (display names, descriptions, owner names) contain HTML/JavaScript; XSS executes in browser viewing HTML report; script injection compromises report integrity or exfiltrates data |
| **Security impact** | Report consumer browser compromise; data exfiltration from report viewer; phishing; session hijacking via report |
| **Existing architectural mitigation** | Provider/tenant-originated strings treated as untrusted in HTML rendering [output-architecture.md §8]; HTML renderers MUST safely encode/escape all externally influenced strings; architecture anticipates HTML injection, script injection/XSS, URL/link injection, unsafe inline content, CSS injection, dangerous URI schemes, malformed Unicode [output-architecture.md §8]; HTML report MUST NOT require execution of tenant-supplied active content |
| **Residual risk** | Exact sanitization libraries/approaches are TBD; CSP policy is TBD; HTML templating technology is TBD |
| **Status** | DESIGNED / REQUIRED (implementation TBD) |

### 18. Terminal/control-character injection

| Aspect | Description |
| --- | --- |
| **Threatened asset** | CLI output; terminal session; output consumers |
| **Attack/failure path** | Provider/tenant-originated strings contain ANSI escape sequences, control characters, bidi Unicode, terminal title manipulation sequences; terminal interprets malicious sequences |
| **Security impact** | Terminal session manipulation; log forging; misleading CLI output; potential terminal-based attacks |
| **Existing architectural mitigation** | CLI renderers treat tenant/provider strings as hostile input [output-architecture.md §9]; architecture anticipates ANSI escape/control sequences, terminal title manipulation, CR/LF injection, bidi/control Unicode, log forging, malformed Unicode [output-architecture.md §9] |
| **Residual risk** | Exact terminal sanitization policy and implementation are TBD |
| **Status** | DESIGNED / REQUIRED (implementation TBD) |

### 19. JSON/SARIF unsafe handling

| Aspect | Description |
| --- | --- |
| **Threatened asset** | JSON/SARIF output; output consumers; assessment integrity |
| **Attack/failure path** | Malformed JSON produced; SARIF schema violations; unescaped strings breaking output format; incorrect SARIF mapping producing misleading results |
| **Security impact** | Output consumers unable to parse results; incorrect integration with security tooling; SARIF results misinterpreted |
| **Existing architectural mitigation** | JSON output uses standards-compliant serialization [output-architecture.md §6]; all five evaluation states preserved in JSON; SARIF mapping does not invent semantics [output-architecture.md §7]; strings treated as data, never executable content [output-architecture.md §6] |
| **Residual risk** | Exact JSON schema and SARIF mapping are TBD; canonical serialization strategy is TBD |
| **Status** | DESIGNED / REQUIRED (implementation TBD) |

### 20. Filesystem path traversal

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Output artifacts; filesystem; operator-controlled storage |
| **Attack/failure path** | Tenant/provider-controlled values determine filesystem paths; path traversal writes outside intended destination; absolute path injection; unsafe file names; symlink/reparse-point hazards |
| **Security impact** | Arbitrary file write; overwrite of critical files; assessment output written to unintended location; potential code execution via path manipulation |
| **Existing architectural mitigation** | Tenant/provider-controlled values MUST NOT directly determine filesystem paths without validated safe transformation [output-architecture.md §10]; architecture defends against path traversal, absolute-path injection, unsafe file names, reserved/special file names, symlink/reparse-point hazards, accidental overwrite, output path escaping [output-architecture.md §10] |
| **Residual risk** | Exact path validation library/algorithm is TBD |
| **Status** | DESIGNED / REQUIRED (implementation TBD) |

### 21. Unsafe overwrite

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Existing output artifacts; assessment history |
| **Attack/failure path** | Output generation silently overwrites existing assessment artifacts; partial write leaves corrupted artifact; race condition between write and read |
| **Security impact** | Loss of previous assessment results; partial artifact appears complete; assessment history corrupted |
| **Existing architectural mitigation** | Output MUST NOT silently overwrite existing artifacts by default [output-architecture.md §11]; safe-write behavior: build/serialize before publication, explicit completion state, atomic replacement, cleanup of failed artifacts [output-architecture.md §11]; overwrite/versioning policy TBD |
| **Residual risk** | Exact overwrite prevention and atomic-write implementation are TBD |
| **Status** | DESIGNED / REQUIRED (implementation TBD) |

### 22. Partial artifact appearing complete

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Output artifacts; assessment integrity; consumers |
| **Attack/failure path** | Output generation fails mid-stream; partial artifact written to filesystem; consumer reads partial artifact as complete assessment; incomplete assessment presented as complete |
| **Security impact** | Incomplete assessment treated as complete; security weaknesses in un-assessed portion not detected; misleading findings |
| **Existing architectural mitigation** | Output generation MUST NOT silently leave partial artifact that appears complete [output-architecture.md §11]; explicit completion/integrity state; cleanup/quarantine of failed temporary artifacts; visible output-generation errors [output-architecture.md §11]; partial artifact behavior is fail-safe [output-architecture.md §16] |
| **Residual risk** | Exact safe-write/atomic publication implementation is TBD; temporary-file strategy is TBD |
| **Status** | DESIGNED / REQUIRED (implementation TBD) |

### 23. Secret leakage into domain/evidence/output/logs

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Authentication material; assessment integrity; operator security |
| **Attack/failure path** | Credential material enters normalized domain model; tokens appear in findings/evidence; secrets logged in structured logging; credential values in output artifacts; tokens in error messages |
| **Security impact** | Credential exposure; unauthorized tenant access; credential theft; compliance violations |
| **Existing architectural mitigation** | INV-09: Secret exclusion — secrets never enter findings, evidence, logs, reports, or persisted artifacts; SEC-001: No credential secret collection or storage; SEC-002: No credentials in source control; authentication boundary isolates tokens from all downstream artifacts; output renderers exclude secret material [OUT-006]; structured logging excludes credential material [NFR-004]; evidence MUST NOT contain secret material [findings-evidence.md §10] |
| **Residual risk** | Secret-detection/sanitization implementation is TBD; unexpected secret-like material reaching evidence boundary requires validated policy |
| **Status** | DESIGNED / REQUIRED (implementation TBD) |

### 24. Resource exhaustion / pathological graph traversal

| Aspect | Description |
| --- | --- |
| **Threatened asset** | System stability; assessment availability; evaluation integrity |
| **Attack/failure path** | Large tenant produces oversized identity graph; pathological graph traversal causes memory exhaustion; excessive rule evaluation consumes resources; malicious provider data causes oversized normalization |
| **Security impact** | Denial of assessment; process crash; incomplete assessment; resource exhaustion appearing as successful or partial result |
| **Existing architectural mitigation** | Rule engine designed for defensive execution against large/pathological state [rule-engine.md §21]; bounded processing, memory safeguards, cancellation, deterministic termination where feasible; resource exhaustion MUST NOT produce PASS [rule-engine.md §21.2]; large-tenant enumeration without unbounded memory growth [collection-architecture.md §8] |
| **Residual risk** | Concrete resource limits are TBD; memory bounds are TBD; performance characteristics for large tenants TBD |
| **Status** | DESIGNED / REQUIRED (limits TBD) |

### 25. Denial of service

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Assessment availability; system stability |
| **Attack/failure path** | Provider API throttling exhausts retry budget; authentication service unavailable; pathological input causes infinite processing; resource exhaustion prevents assessment completion |
| **Security impact** | Assessment cannot complete; operator unable to assess tenant security posture; no security findings produced |
| **Existing architectural mitigation** | Pagination, scale, and retry with bounded behavior [collection-architecture.md §8]; structured collection failure categories [collection-architecture.md §10]; throttling/transient failure handling; cancellation support; authentication failure produces explicit state [authentication-authorization.md §12] |
| **Residual risk** | Exact retry counts, timeout values, and throttling thresholds are TBD; no automated DoS detection |
| **Status** | DESIGNED / REQUIRED (operational parameters TBD) |

### 26. Insecure fallback behavior

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Authentication boundary; assessment integrity; security posture |
| **Attack/failure path** | Authentication fails; insecure fallback authentication mechanism used; workload credentials unavailable; automatic degradation to less-secure mode; tenant context not validated |
| **Security impact** | Authentication against wrong tenant; unauthorized access; credential exposure; assessment of unintended scope |
| **Existing architectural mitigation** | INV-16: Security-sensitive defaults — secure behavior is default, weakening requires explicit opt-in; CI MUST fail closed if authentication unavailable [authentication-authorization.md §11]; no automatic downgrade to insecure fallback; authentication failure produces explicit failure [authentication-authorization.md §12] |
| **Residual risk** | Exact fallback prevention implementation depends on authentication mechanism chosen (TBD); no automated insecure-fallback detection |
| **Status** | DESIGNED / REQUIRED (implementation TBD) |

### 27. Undisclosed telemetry/data exfiltration

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Tenant assessment data; tenant context; assessment confidentiality |
| **Attack/failure path** | Assessment data transmitted to EntraNHI maintainers without disclosure; output renderer fetches external resources; undisclosed telemetry collection; third-party data transmission |
| **Security impact** | Tenant data exposure; compliance violation; enterprise security data leaked; loss of assessment confidentiality |
| **Existing architectural mitigation** | CON-005: Network communication limited to approved endpoints; no undisclosed telemetry; no third-party tenant-data transmission; output renderers MUST NOT require network transmission [output-architecture.md §22]; static HTML reports MUST NOT silently fetch external resources; any future upload feature requires separate architecture and security review |
| **Residual risk** | No automated network-traffic monitoring to detect undisclosed transmission; relies on architectural discipline |
| **Status** | DESIGNED / REQUIRED (automated monitoring TBD) |

### 28. CI credential exposure

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Workload credentials; CI/CD pipeline security; tenant data |
| **Attack/failure path** | CI secrets logged in pipeline output; credentials embedded in CI configuration; CI logs exposed to unauthorized parties; build artifacts contain credential material; CI environment compromised |
| **Security impact** | Credential theft; unauthorized tenant access; tenant data exfiltration through CI pipeline; supply-chain compromise |
| **Existing architectural mitigation** | SEC-002: No credentials in source control; CI credentials externally configured [trust-boundaries.md §CI zone]; authentication boundary isolates tokens from all artifacts; CI execution must fail closed if credentials unavailable [INV-16]; output consumed by CI must preserve machine-readable state without embedding credentials |
| **Residual risk** | CI platform-specific credential protection is TBD; CI log confidentiality is operator responsibility; no automated CI credential scanning |
| **Status** | DESIGNED / REQUIRED (CI-specific security TBD) |

### 29. Dependency/supply-chain compromise

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Software/release integrity; execution environment; assessment results |
| **Attack/failure path** | Malicious package dependency executes code in EntraNHI runtime; build-time code injection; dependency version tampering; compromised build pipeline |
| **Security impact** | Arbitrary code execution; credential theft; data exfiltration; assessment manipulation; supply-chain attack on downstream users |
| **Existing architectural mitigation** | CON-003: No unapproved new dependencies; dependency management restricted; open-source repository enables community review; architecture designed for future SBOM and signing |
| **Residual risk** | SBOM implementation is TBD; dependency signing/verification is TBD; build integrity verification is TBD; no automated supply-chain security tooling |
| **Status** | DESIGNED / REQUIRED (supply-chain controls TBD) |

### 30. AI altering authoritative assessment state

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Deterministic rule evaluation; findings/evidence; assessment integrity |
| **Attack/failure path** | AI/LLM determines PASS/FAIL verdicts; AI changes rule outcomes; AI fabricates evidence; AI bypasses authorization; AI expands collection privileges; AI becomes required for assessment correctness |
| **Security impact** | Non-deterministic assessment; hallucinated findings; compromised audit trail; assessment integrity destroyed |
| **Existing architectural mitigation** | INV-13: AI non-authority — AI/LLM cannot determine or modify rule outcomes; SEC-010: No AI-generated verdicts; VERD-007: No AI/ML-based verdicts; CON-004: No autonomous AI decisions; AI/LLM MUST NOT determine verdicts, change rule state, supply evidence, fabricate capability state, override ERROR/NOT_EVALUATED, alter severity, execute remediation, or request privilege [rule-engine.md §19.2]; AI availability not required for core assessment correctness |
| **Residual risk** | AI/LLM boundary enforcement depends on implementation; prompt injection if AI is introduced; AI integration point security TBD |
| **Status** | DESIGNED / REQUIRED (boundary enforcement TBD if AI introduced) |

### 31. Future extension/plugin trust risks

| Aspect | Description |
| --- | --- |
| **Threatened asset** | Rule evaluation; assessment integrity; execution environment |
| **Attack/failure path** | Third-party rule packs execute untrusted code; malicious plugin accesses provider APIs; extension bypasses normalization; plugin modifies assessment state |
| **Security impact** | Arbitrary code execution; assessment manipulation; credential theft; provider isolation violation |
| **Existing architectural mitigation** | Third-party extension model outside V1 scope [rule-engine.md §18.3]; V1 extension constrained — no arbitrary untrusted code execution by default [rule-engine.md §18.2]; architecture designed for future sandboxing/signing if extensions introduced |
| **Residual risk** | Plugin sandboxing/signing model is TBD; no V1 extension security architecture exists |
| **Status** | DESIGNED / REQUIRED (future scope; security architecture TBD) |

---

## Adversarial analysis matrix (A-L)

This section explicitly analyzes whether any plausible architecture path could cause the specified adverse outcomes.

### A. Incomplete collection becoming PASS

| Aspect | Assessment |
| --- | --- |
| **Architecture path** | Collection partially fails → capability state records failure → SourceObservation carries capability state → normalization preserves capability state → identity graph reflects capability state → rule engine validates required capabilities before evaluation → PASS requires sufficient input completeness [rule-engine.md §5] |
| **Analysis** | The architecture explicitly requires: (1) capability state propagated from collectors through the pipeline; (2) rule engine validates required capabilities before evaluation [rule-engine.md §10.4]; (3) PASS requires sufficient input completeness defined per rule [rule-engine.md §5]; (4) missing data MUST NOT automatically become PASS [rule-engine.md §4.1]. However, the correctness of this protection depends on: each rule correctly specifying its completeness requirements; capability state being correctly propagated at each boundary; no implementation bypass of the capability-validation step. |
| **Classification** | **PROTECTED** — The architecture explicitly requires capability validation before PASS and requires PASS to depend on input completeness. The protection is architecturally sound, though implementation correctness remains to be verified. |

### B. Authentication failure becoming tenant FAIL

| Aspect | Assessment |
| --- | --- |
| **Architecture path** | Authentication fails → authentication boundary produces explicit failure category → collection cannot proceed → no assessment data collected → no fabricated RuleEvaluation or tenant-security FAIL produced |
| **Analysis** | The architecture explicitly requires: (1) authentication failures produce explicit failure categories [authentication-authorization.md §12]; (2) no authentication failure may silently produce successful assessment [INV-14]; (3) operational failures preserve failure context and do not directly assign rule states [rule-engine.md §6]; (4) applicable evaluation/rule semantics and later failure-mapping mechanics determine whether a RuleEvaluation can legitimately be produced; (5) system diagnostics remain distinct from tenant findings [findings-evidence.md §15]. |
| **Classification** | **PROTECTED** — The architecture explicitly prevents authentication failure from producing tenant FAIL or silent PASS without establishing a universal operational-failure-to-assessment-state mapping. |

### C. Authorization denial becoming PASS

| Aspect | Assessment |
| --- | --- |
| **Architecture path** | Authorization denied for capability → capability state records authorization denial → capability/failure context propagates to the rule engine → required capabilities are checked → applicable rule semantics determine whether and how evaluation proceeds |
| **Analysis** | The architecture explicitly requires: (1) authorization denial MUST NOT automatically become PASS or FAIL [authentication-authorization.md §5.2]; (2) capability state flows through an explicit mechanism to the rule engine; (3) rules are gated on required capabilities [rule-engine.md §11]; (4) there is no universal capability-state-to-evaluation-state mapping [rule-engine.md §11.3]. |
| **Classification** | **PROTECTED** — Authorization denial remains explicit and cannot become PASS merely because observations are absent; exact state selection remains rule-specific. |

### D. One tenant's evidence supporting another tenant

| Aspect | Assessment |
| --- | --- |
| **Architecture path** | Tenant A data collected → SourceObservation carries tenant A context → normalized to tenant A scope → identity graph scoped to tenant A → RuleEvaluation bound to tenant A → Finding bound to tenant A → output scoped to tenant A |
| **Analysis** | The architecture explicitly requires: (1) tenant boundary preservation [INV-10]; (2) SourceObservation carries tenant assessment context; (3) identity graph scoped to single tenant [INV-G1]; (4) every RuleEvaluation and Finding bound to single tenant context [findings-evidence.md §12]; (5) cross-tenant evidence correlation prohibited in V1. |
| **Classification** | **PROTECTED** — The architecture explicitly prevents cross-tenant data mixing at every boundary. |

### E. Renderer changing security truth

| Aspect | Assessment |
| --- | --- |
| **Architecture path** | RuleEvaluation produced → findings/evidence layer transforms to Finding → canonical output model → renderer transforms to format-specific output |
| **Analysis** | The architecture explicitly requires: (1) output layer MUST NOT alter RuleEvaluation state [output-architecture.md §1]; (2) all five evaluation states preserved and distinguishable [output-architecture.md §3]; (3) renderers MUST NOT rerun rules, reinterpret verdicts, or fabricate evidence [output-architecture.md §1]; (4) core/output separation [INV-12]; (5) renderer failure is system diagnostic, not tenant finding. |
| **Classification** | **PROTECTED** — The architecture explicitly prevents renderers from altering security truth. |

### F. AI/LLM changing authoritative security truth

| Aspect | Assessment |
| --- | --- |
| **Architecture path** | Deterministic assessment pipeline produces RuleEvaluation → AI/LLM receives results for explanation → AI generates explanatory text → explanatory text is non-authoritative, downstream only |
| **Analysis** | The architecture explicitly requires: (1) AI/LLM cannot determine or modify rule outcomes [INV-13]; (2) AI availability not required for core assessment correctness; (3) AI limited to post-assessment explanation, must not alter verdicts [INV-13]; (4) AI MUST NOT create/modify evidence [findings-evidence.md §21]; (5) AI-generated explanation clearly downstream/non-authoritative. |
| **Classification** | **PROTECTED** — AI/LLM is explicitly positioned as non-authoritative and cannot modify assessment truth. |

### G. Secret/token entering normalized data/evidence/output

| Aspect | Assessment |
| --- | --- |
| **Architecture path** | Authentication obtains tokens → tokens exist in AuthorizedAccessContext → collectors consume authorized context → normalization excludes tokens → domain model excludes tokens → findings/evidence exclude tokens → output excludes tokens |
| **Analysis** | The architecture explicitly requires: (1) INV-09: Secret exclusion — tokens never enter findings, evidence, logs, reports, or persisted artifacts; (2) authentication boundary isolates tokens from all downstream artifacts; (3) SourceObservation MUST NOT contain authentication tokens [collection-architecture.md §4.2]; (4) rule engine MUST NOT store authentication secrets as evidence [rule-engine.md §15.2]; (5) output excludes secret material [OUT-006]; (6) logging excludes credential material [NFR-004]. |
| **Classification** | **PROTECTED** — Secret exclusion is enforced at every boundary. The architecture explicitly prevents secret material from entering normalized data, evidence, or output. However, in-memory token protection is TBD. |

### H. Provider object bypassing normalization into rule logic

| Aspect | Assessment |
| --- | --- |
| **Architecture path** | Provider returns objects → collectors acquire SourceObservation → normalization translates to domain contracts → identity graph projects from normalized data → rule engine consumes normalized contracts |
| **Analysis** | The architecture explicitly requires: (1) INV-03: Provider isolation — rule engine MUST NOT query providers; (2) INV-04: Normalized domain boundary — provider representation MUST NOT leak into rule contracts; (3) INV-G7: Rules consume normalized contracts only; (4) rule engine MUST NOT use provider SDK types, raw payloads, or API responses [rule-engine.md §2.2, §2.3]. |
| **Classification** | **PROTECTED** — The architecture explicitly enforces provider isolation through the normalized domain boundary. Provider objects cannot reach rule logic. |

### I. Partial output artifact appearing complete

| Aspect | Assessment |
| --- | --- |
| **Architecture path** | Output generation begins → generation fails mid-stream → partial artifact written → consumer reads partial artifact |
| **Analysis** | The architecture explicitly requires: (1) output MUST NOT silently leave partial artifact that appears complete [output-architecture.md §11]; (2) explicit completion/integrity state; (3) cleanup/quarantine of failed temporary artifacts; (4) visible output-generation errors [output-architecture.md §11]; (5) partial artifact behavior is fail-safe [output-architecture.md §16]. |
| **Classification** | **AMBIGUOUS** — The architecture requires fail-safe partial-artifact behavior, but exact safe-write/atomic publication implementation is TBD. The architectural intent is sound, but without concrete implementation, the protection is not yet verified. |

### J. Resource exhaustion appearing as successful assessment

| Aspect | Assessment |
| --- | --- |
| **Architecture path** | Large/pathological input → resource exhaustion during collection/normalization/rule evaluation → process terminates or hangs → partial results produced → output generated from partial results |
| **Analysis** | The architecture requires: (1) resource exhaustion MUST NOT produce PASS [rule-engine.md §21.2]; (2) resource exhaustion MUST NOT produce misleading successful artifact [output-architecture.md §20]; (3) defensive execution with bounded processing, memory safeguards, cancellation [rule-engine.md §21]. However: concrete resource limits are TBD; no automated resource-exhaustion detection exists; the exact behavior when resource limits are hit during evaluation is not fully specified. |
| **Classification** | **AMBIGUOUS** — The architecture requires resource exhaustion to fail visibly, but concrete limits and detection mechanisms are TBD. The protection is architecturally sound but not yet implemented. |

### K. Missing provenance being silently fabricated

| Aspect | Assessment |
| --- | --- |
| **Architecture path** | Source observation → normalization → provenance preserved → identity graph → rule evaluation → evidence references provenance → finding includes provenance |
| **Analysis** | The architecture requires: (1) INV-11: Provenance preservation — observations retain source and collection context; (2) provenance MUST NOT include secret material; (3) rule engine MUST NOT fabricate evidence [rule-engine.md §15.2]; (4) evidence fabrication defense [findings-evidence.md §18]; (5) unknown/unavailable provenance represented explicitly, not invented [findings-evidence.md §6]; (6) AI MUST NOT invent missing provenance [findings-evidence.md §21]. |
| **Classification** | **AMBIGUOUS** — The architecture explicitly prohibits provenance fabrication and requires explicit representation of missing provenance. However, no cryptographic integrity verification of provenance chains exists yet, and enforcement depends on correct implementation at each transformation boundary. |

### L. Untrusted tenant strings becoming executable/rendered control content

| Aspect | Assessment |
| --- | --- |
| **Architecture path** | Tenant/provider-originated strings (display names, descriptions) → normalization → domain model → findings → output → rendered in CLI/HTML/JSON/SARIF |
| **Analysis** | The architecture requires: (1) provider/tenant strings treated as untrusted in rendering [findings-evidence.md §17]; (2) HTML renderers safely encode/escape all externally influenced strings [output-architecture.md §8]; (3) CLI renderers treat tenant strings as hostile input [output-architecture.md §9]; (4) JSON treats strings as data, never executable content [output-architecture.md §6]; (5) architecture anticipates HTML injection, XSS, terminal control injection, log forging [output-architecture.md §8, §9]. |
| **Classification** | **AMBIGUOUS** — The architecture requires safe encoding/escaping of untrusted strings in all output formats. The intent is clear and sound. However, exact sanitization libraries, CSP policies, and terminal sanitization implementations are TBD. The architectural protection is designed but not yet verified through implementation. |

---

## Adversarial matrix summary

| ID | Adverse outcome | Classification | Reason |
| --- | --- | --- | --- |
| A | Incomplete collection becoming PASS | PROTECTED | Capability validation and PASS completeness requirements explicitly enforced |
| B | Authentication failure becoming tenant FAIL | PROTECTED | Authentication failure remains explicit and does not become tenant FAIL; exact state handling is not universal |
| C | Authorization denial becoming PASS | PROTECTED | Authorization denial remains explicit and cannot produce PASS from absent observations; state selection is rule-specific |
| D | One tenant's evidence supporting another | PROTECTED | Tenant isolation enforced at every boundary |
| E | Renderer changing security truth | PROTECTED | Output layer cannot alter RuleEvaluation state |
| F | AI/LLM changing authoritative security truth | PROTECTED | AI explicitly non-authoritative; cannot modify verdicts |
| G | Secret/token entering normalized data/evidence/output | PROTECTED | Secret exclusion enforced at every boundary |
| H | Provider object bypassing normalization into rule logic | PROTECTED | Provider isolation through normalized domain boundary |
| I | Partial output artifact appearing complete | AMBIGUOUS | Architecturally required but implementation TBD |
| J | Resource exhaustion appearing as successful assessment | AMBIGUOUS | Architecturally required to fail visibly but limits TBD |
| K | Missing provenance being silently fabricated | AMBIGUOUS | Prohibited but no cryptographic verification yet |
| L | Untrusted strings becoming executable content | AMBIGUOUS | Required safe encoding but implementations TBD |

---

## Architecture-invariant cross-references

| Invariant | Threats addressed |
| --- | --- |
| INV-01 — Read-only tenant operation | Excessive privilege, tenant-state mutation |
| INV-02 — Deterministic assessment | Rule tampering, capability-state confusion, non-deterministic evaluation |
| INV-03 — Provider isolation | Normalization bypass, provider-object leaking, rule logic coupling |
| INV-04 — Normalized domain boundary | Provider-object leaking, normalization bypass, provider-specific coupling |
| INV-05 — Explicit evaluation states | False PASS from missing data, capability-state confusion, incomplete collection |
| INV-06 — Evidence traceability | Evidence fabrication, provenance loss, audit integrity |
| INV-07 — Capability awareness | Authorization denial producing false PASS, incomplete collection |
| INV-08 — Least privilege | Excessive privilege, credential blast radius |
| INV-09 — Secret exclusion | Token/credential disclosure, secret leakage |
| INV-10 — Tenant boundary preservation | Wrong-tenant execution, cross-tenant contamination |
| INV-11 — Provenance preservation | Provenance loss, evidence fabrication |
| INV-12 — Core/output separation | Renderer changing security truth, output manipulation |
| INV-13 — AI non-authority | AI altering assessment state, AI-generated verdicts |
| INV-14 — Failure transparency | False PASS from failures, insecure fallback, silent success |
| INV-15 — No undocumented capability dependency | Provider spoofing, undocumented behavior dependency |
| INV-16 — Security-sensitive defaults | Insecure fallback, configuration tampering, CI credential exposure |

---

## Security-sensitive TBDs requiring later resolution

| TBD | Security sensitivity | Related threats |
| --- | --- | --- |
| Exact OAuth/OIDC flows | Critical — authentication security | Token/credential disclosure, wrong-tenant, insecure fallback |
| Exact Microsoft Graph permissions | High — least privilege | Excessive privilege, blast radius |
| Concrete token cache/storage | Critical — credential protection | Token/credential disclosure, CI credential exposure |
| Cryptographic integrity mechanisms | High — tamper detection | Evidence fabrication, provenance loss, rule tampering |
| Artifact signing/hashing | High — supply chain, audit | Dependency compromise, output tampering |
| Exact filesystem security implementation | High — artifact protection | Path traversal, unsafe overwrite, partial artifacts |
| Exact sanitization libraries | High — injection defense | HTML/XSS, terminal injection, log injection |
| Dependency/supply-chain tooling | High — supply chain | Dependency compromise, build integrity |
| SBOM implementation | Medium — transparency | Supply chain, release integrity |
| CI security configuration | High — CI credential protection | CI credential exposure, build compromise |
| Plugin sandboxing/signing | High — extension trust | Future extension/plugin trust risks |
| Telemetry implementation (if introduced) | Critical — data exfiltration | Undisclosed telemetry |
| Future UI security architecture | High — web security | XSS, session security, browser attacks |
| Concrete resource limits | Medium — availability | Resource exhaustion, denial of service |
| Token in-memory protection | Critical — credential protection | Token/credential disclosure via memory |
| Rule integrity verification | High — assessment integrity | Rule tampering, configuration tampering |
| Fatal vs isolated engine failure policy | Medium — availability, correctness | Resource exhaustion, partial results |

---

## Open items

- **TBD:** Formal STRIDE or DREAD scoring for each threat (deferred per approach; structured analysis provided instead).
- **TBD:** Mitigation design for token exposure in memory.
- **TBD:** Mitigation design for output sharing controls.
- **TBD:** Mitigation design for rule integrity verification.
- **TBD:** Mitigation design for supply-chain integrity.
- **TBD:** Mitigation design for CI/CD secret protection.
- **TBD:** Threat model review and validation process.
- **TBD:** Automated network-traffic monitoring for undisclosed transmission detection.
- **TBD:** Formal verification of tenant-context consistency.
- **TBD:** Automated boundary enforcement for provider isolation.
