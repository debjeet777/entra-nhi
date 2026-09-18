# Testing Architecture

> **Status:** Phase 0.2.10
> **Date:** 2026-09-18

---

## Purpose

Define the enterprise-grade V1 testing architecture for EntraNHI. This document establishes the verification strategy for security and correctness properties defined by the requirements, architecture invariants, trust boundaries, and threat model.

This is architecture only. Nothing in this document claims tests, controls, frameworks, CI gates, coverage levels, fuzzers, scanners, or security mechanisms are already implemented.

Where this document states DESIGNED / REQUIRED, the property or mechanism is architecturally required but not yet implemented. Where this document states IMPLEMENTED / VERIFIED, the property has been confirmed through completed implementation evidence. At this phase, nearly all properties are DESIGNED / REQUIRED.

---

## 1. Testing Principles

The following principles govern all EntraNHI testing.

### 1.1 Deterministic and repeatable tests

Tests MUST produce identical results across runs given identical inputs, configuration, and deterministic execution context. [INV-02, VERD-006]

### 1.2 Synthetic data by default

All tests, fixtures, mocks, and documentation examples use sanitized or synthetic data. No production tenant data, real credentials, or employer/customer information appears in the repository. [SEC-009, AC-010]

### 1.3 No production tenant dependency

Tests MUST NOT require a live Microsoft Entra tenant, live Microsoft Graph API access, or any production service dependency for core test execution. [SEC-009, NFR-002]

### 1.4 No real secrets, tokens, credentials, private keys, or sensitive tenant data in test fixtures

Fixtures MUST NOT contain live access tokens, refresh tokens, client secrets, passwords, private keys, authentication cookies, production certificates, recovery codes, or production tenant data. [SEC-001, SEC-002, SEC-009, INV-09]

### 1.5 Isolation between test tenants/contexts

Tests operating on multiple synthetic tenant contexts MUST NOT allow cross-tenant data mixing. Tenant isolation is a release-critical test property. [INV-10]

### 1.6 Provider-independent core testing

Core rule evaluation and deterministic assessment logic MUST be testable without live provider API responses. Tests use normalized synthetic fixtures. [INV-03, INV-04]

### 1.7 Capability-aware testing

Tests MUST verify behavior across different capability states (available, unavailable-authorization, unavailable-licensing, unsupported, failed, not-applicable). [INV-07, CAP-001, CAP-002]

### 1.8 Failure-state testing as a first-class requirement

Failure scenarios are not secondary. Tests MUST cover collection failures, normalization failures, rule evaluation failures, authentication failures, authorization denials, and output failures as primary test categories. [INV-14]

### 1.9 Negative and adversarial testing

Tests MUST include negative-path scenarios and adversarial inputs designed to provoke incorrect PASS, incorrect FAIL, cross-tenant contamination, credential leakage, and output injection. [threat-model.md]

### 1.10 Reproducible failure diagnostics

When a test fails, the failure MUST be diagnosable without requiring access to production systems, live tenants, or real credentials.

### 1.11 Bounded test execution

Tests MUST terminate within bounded time and resource limits. Tests MUST NOT depend on external services that could cause indefinite blocking.

### 1.12 Secure test artifacts

Test artifacts (logs, output files, temporary fixtures) MUST NOT contain real credential material, production tenant data, or sensitive information. [INV-09]

### 1.13 No external telemetry or undisclosed test-data transmission

Test execution MUST NOT transmit test data, fixture data, or synthetic tenant data to external services beyond what is required for the specific test purpose. [CON-005]

### 1.14 Tests must not weaken production security boundaries

Test configurations, test authentication contexts, and test fixtures MUST NOT weaken or bypass production security boundaries. Test isolation MUST NOT introduce insecure fallback paths that could leak into production. [INV-16]

### 1.15 Security properties require explicit verification

Security-critical properties MUST have explicit test verification. Absence of a test is not evidence of security. Architectural documentation of a control is DESIGNED / REQUIRED; a passing test is IMPLEMENTED / VERIFIED.

---

## 2. Test Taxonomy

Define the conceptual test layers and their responsibilities. Framework selection remains TBD.

### A. Unit tests

| Aspect | Description |
| --- | --- |
| **Purpose** | Validate individual components in isolation with mocked dependencies |
| **Test boundary** | Single function, method, class, or module |
| **Appropriate inputs** | Synthetic normalized fixtures, edge-case inputs, malformed inputs |
| **Expected outputs/assertions** | Correct return values, state changes, exception behavior, provenance preservation |
| **Prohibited dependencies** | Live APIs, real tenant data, real credentials, network access, file system (where avoidable) |
| **Security relevance** | Provider isolation [INV-03], normalization correctness [INV-04], secret exclusion [INV-09], deterministic evaluation [INV-02] |
| **Deterministic/repeatability** | Fully deterministic; identical inputs produce identical outputs |

### B. Component tests

| Aspect | Description |
| --- | --- |
| **Purpose** | Validate a component's behavior with its immediate collaborators mocked |
| **Test boundary** | One component with its direct interface contracts |
| **Appropriate inputs** | Synthetic SourceObservation envelopes, normalized domain records, graph projections |
| **Expected outputs/assertions** | Correct transformation, correct capability state propagation, correct error categorization |
| **Prohibited dependencies** | Live APIs, real tenant data, other unmocked components beyond direct collaborators |
| **Security relevance** | Boundary enforcement at each architectural transition [INV-04, INV-05] |
| **Deterministic/repeatability** | Fully deterministic |

### C. Contract tests

| Aspect | Description |
| --- | --- |
| **Purpose** | Validate that components interact correctly through their defined interfaces |
| **Test boundary** | Interface boundary between two components |
| **Appropriate inputs** | Synthetic data conforming to interface contracts |
| **Expected outputs/assertions** | Output of producer matches input expectations of consumer; schema conformance |
| **Prohibited dependencies** | Implementation details beyond the interface; live APIs |
| **Security relevance** | Normalized domain boundary preservation [INV-04], provider isolation [INV-03], core/output separation [INV-12] |
| **Deterministic/repeatability** | Fully deterministic |

### D. Integration tests

| Aspect | Description |
| --- | --- |
| **Purpose** | Validate end-to-end assessment flow through multiple components |
| **Test boundary** | Multiple components exercising a complete or near-complete pipeline |
| **Appropriate inputs** | Synthetic provider responses, synthetic normalized data, synthetic graph topologies |
| **Expected outputs/assertions** | Correct verdicts, correct evidence references, correct output serialization, correct capability state propagation |
| **Prohibited dependencies** | Live Microsoft Graph, live Azure Resource Manager, real tenant data, real credentials |
| **Security relevance** | False PASS/false FAIL prevention, tenant isolation, secret exclusion across full pipeline |
| **Deterministic/repeatability** | Fully deterministic given identical synthetic inputs |

### E. End-to-end tests

| Aspect | Description |
| --- | --- |
| **Purpose** | Validate complete assessment flow from collection configuration through output generation |
| **Test boundary** | Full pipeline with all major components |
| **Appropriate inputs** | Complete synthetic tenant fixture sets, synthetic authentication context |
| **Expected outputs/assertions** | Valid output in all formats (CLI, JSON, SARIF, HTML), correct verdict distribution, correct evidence traceability |
| **Prohibited dependencies** | Live Microsoft Entra tenant, real credentials, production services |
| **Security relevance** | Complete security property verification across all boundaries |
| **Deterministic/repeatability** | Fully deterministic with synthetic fixtures |

### F. Regression tests

| Aspect | Description |
| --- | --- |
| **Purpose** | Prevent known security bugs and semantic regressions from reoccurring |
| **Test boundary** | Specific known-buggy scenarios captured as permanent tests |
| **Appropriate inputs** | Scenarios derived from threat model, past defects, adversarial analysis |
| **Expected outputs/assertions** | Specific expected outcomes for known-buggy scenarios |
| **Prohibited dependencies** | Live APIs, real tenant data |
| **Security relevance** | Direct mapping to threat-model controls and adversarial analysis matrix |
| **Deterministic/repeatability** | Fully deterministic |

### G. Security/adversarial tests

| Aspect | Description |
| --- | --- |
| **Purpose** | Explicitly attempt to violate security invariants and trust boundaries |
| **Test boundary** | Security-critical boundaries (trust boundaries 1-15) |
| **Appropriate inputs** | Adversarial synthetic inputs: cross-tenant data, malicious strings, type-confusion payloads, missing data, fabricated provenance, credential-like strings |
| **Expected outputs/assertions** | Security invariants preserved; no false PASS, no false FAIL, no credential leakage, no cross-tenant contamination, no injection |
| **Prohibited dependencies** | Real credentials, real tenant data, production services |
| **Security relevance** | Direct verification of INV-01 through INV-16 and threat-model controls |
| **Deterministic/repeatability** | Fully deterministic |

### H. Property-based tests

| Aspect | Description |
| --- | --- |
| **Purpose** | Verify universal properties hold across generated input sets |
| **Test boundary** | Boundaries where universal invariants must hold (normalization, graph construction, rule evaluation, output serialization) |
| **Appropriate inputs** | Auto-generated inputs within defined domains |
| **Expected outputs/assertions** | Security properties hold for all generated inputs |
| **Prohibited dependencies** | Live APIs, real tenant data |
| **Security relevance** | Properties such as: arbitrary input cannot create cross-tenant relationships, arbitrary missing data cannot create PASS, arbitrary provider strings cannot become executable output |
| **Deterministic/repeatability** | Deterministic with seeded generation |

### I. Fuzz/malformed-input testing

| Aspect | Description |
| --- | --- |
| **Purpose** | Discover unexpected behavior from malformed, truncated, oversized, or adversarial inputs |
| **Test boundary** | Parsers, normalizers, serializers, output renderers, filesystem path handlers |
| **Appropriate inputs** | Malformed JSON, truncated responses, oversized strings, invalid Unicode, control characters, binary payloads |
| **Expected outputs/assertions** | Graceful failure, no crash, no security invariant violation, no secret leakage, no injection |
| **Prohibited dependencies** | Live APIs, real tenant data |
| **Security relevance** | Provider data parsing, normalization, graph construction, output serialization, filesystem safety |
| **Deterministic/repeatability** | Deterministic with seed |

### J. Golden/snapshot tests

| Aspect | Description |
| --- | --- |
| **Purpose** | Verify deterministic canonical output remains stable and correct |
| **Test boundary** | Output generation for each format (CLI, JSON, SARIF, HTML) |
| **Appropriate inputs** | Known-good synthetic assessment results |
| **Expected outputs/assertions** | Output matches approved golden snapshot; security-relevant snapshot changes require review |
| **Prohibited dependencies** | Live APIs, real tenant data |
| **Security relevance** | Output injection resistance, semantic preservation, state distinguishability |
| **Deterministic/repeatability** | Fully deterministic |

### K. Performance/resource-bound tests

| Aspect | Description |
| --- | --- |
| **Purpose** | Verify bounded processing and resource consumption under stress |
| **Test boundary** | Full pipeline under resource pressure |
| **Appropriate inputs** | Large synthetic identity sets, high-degree graphs, deep graphs, duplicate-heavy input |
| **Expected outputs/assertions** | Completion within bounded resources; resource exhaustion does not produce false PASS; cancellation remains visible |
| **Prohibited dependencies** | Live APIs, real tenant data |
| **Security relevance** | Resource exhaustion attacks, pathological input, denial-of-assessment prevention |
| **Deterministic/repeatability** | Repeatable within performance tolerances |

### L. Failure-injection/resilience tests

| Aspect | Description |
| --- | --- |
| **Purpose** | Verify correct behavior when components fail |
| **Test boundary** | Individual component failure propagation through the pipeline |
| **Appropriate inputs** | Injected failures at each component boundary |
| **Expected outputs/assertions** | Failure remains visible; no false PASS; correct NOT_EVALUATED/ERROR production; no silent success |
| **Prohibited dependencies** | Live APIs, real tenant data |
| **Security relevance** | INV-14 failure transparency, false PASS prevention, failure visibility |
| **Deterministic/repeatability** | Fully deterministic with injected failures |

---

## 3. Test Data Architecture

### 3.1 Safe test-data model

All repository fixtures MUST be fully synthetic. Default test data uses fabricated entities that follow the documented normalized domain concepts without requiring live provider data.

Required synthetic data types:

| Data type | Description |
| --- | --- |
| Synthetic identities | Fabricated application registrations, service principals, managed-identity representations, agent-identity representations according to documented normalized concepts |
| Synthetic application registrations | Fabricated application objects with normalized properties |
| Synthetic service principals | Fabricated service-principal objects with normalized properties |
| Synthetic managed-identity representations | Service-principal-backed identities with documented classification evidence |
| Synthetic AgentIdentity representations | Agent identities modeled only according to documented normalized concepts |
| Fake accountability relationships | Fabricated owner, sponsor, manager relationships |
| Fake credential metadata | Non-secret metadata containing no actual credential material (synthetic key IDs, synthetic thumbprints, synthetic timestamps) |
| Fake permission/access relationships | Fabricated application permissions, delegated permissions, role assignments |
| Synthetic graph topology | Fabricated identity-to-identity relationships, application-to-service-principal links |
| Fake capability states | Available, unavailable-authorization, unavailable-licensing, unsupported, failed, not-applicable |
| Fake collection failures | Authentication denial, authorization failure, service unavailability, throttling, malformed response |
| Fake provider errors | Structured error responses simulating provider-side failures |
| Malformed/untrusted strings | HTML payloads, ANSI escape sequences, Unicode edge cases, control characters, oversized strings, path-traversal strings |
| Pathological graph structures | Cyclic graphs, disconnected graphs, high-degree nodes, deep traversals, self-references |

### 3.2 Forbidden fixture content

Fixtures MUST NEVER contain:

- Live access tokens or refresh tokens
- Client secrets or passwords
- Private keys or certificate private-key material
- Authentication cookies or session tokens
- Production certificates or signing material
- Recovery codes or backup credentials
- Real production tenant identifiers or data
- Employer or customer confidential information
- Production tenant exports or real directory objects

### 3.3 Test-fixture provenance and classification

Each fixture SHOULD carry metadata identifying:

- Whether the fixture is fully synthetic (default)
- The fixture's purpose and scope
- The fixture's classification level
- Whether the fixture contains any provider-derived data (must undergo explicit security review)

Any future captured provider fixture MUST undergo explicit security review and sanitization before repository inclusion. Default repository fixtures MUST be fully synthetic.

---

## 4. Collector Testing

### 4.1 Test scenarios

| Scenario | Test expectation |
| --- | --- |
| Successful collection | SourceObservations produced with correct provenance and capability state = available |
| Empty-but-valid collection | Zero observations produced; capability state = available; no error |
| Partial collection | Some observations produced; capability state reflects partial success; failure visible |
| Authorization denial | Capability state = unavailable-authorization; structured error; no false empty success |
| Authentication-related upstream failure | Authentication failure propagated; collection does not proceed silently |
| Provider/service failure | Capability state = failed; structured error; no false empty success |
| Throttling/retry-relevant conditions | Bounded retry behavior; capability state reflects exhaustion if retry budget exceeded |
| Malformed provider responses | Structured normalization-input validation failure; ERROR or NOT_EVALUATED downstream |
| Unexpected/missing properties | Missing properties preserved as absent semantics; not silently filled |
| Duplicate observations | Deterministic deduplication; single canonical record produced |
| Pagination behavior | Multi-page results collected completely; partial collection visible if interrupted |
| Cancellation | Collection cancelled before completion; cancellation state visible downstream |
| Timeout behavior | Bounded timeout; timeout produces structured failure, not silent empty success |
| Capability unavailable | Capability state reflects unavailability; no false empty success |
| Unsupported capability | Capability state = unsupported; no false empty success |
| Tenant-context mismatch | Tenant-context validation fails safely and visibly |
| Large/bounded datasets | Collection completes within bounded resources for large synthetic tenant |

### 4.2 Verification requirements

Tests MUST verify:

- Failures do not silently become successful empty collections
- Collectors do not generate findings (findings are produced downstream)
- Collectors do not determine PASS/FAIL
- Collector output carries required provenance/context
- No secret material enters SourceObservation
- Provider data remains outside the deterministic rule layer
- Authentication tokens do not enter SourceObservation content

Use mocks/fakes/synthetic provider contracts. Do not invent exact Graph payload schemas or endpoints.

---

## 5. Authentication/Authorization Testing

### 5.1 Test scenarios

| Scenario | Test expectation |
| --- | --- |
| Valid runtime access context | Authorized context established; collection proceeds |
| Authentication failure | Explicit failure category; no successful assessment |
| Authorization denial | Capability state = unavailable-authorization; NOT_EVALUATED for dependent rules |
| Insufficient capability | Capability state reflects specific limitation; NOT_EVALUATED |
| Wrong tenant | Tenant-context mismatch; fails safely and visibly |
| Tenant mismatch | Authentication context does not match expected tenant; explicit failure |
| Expired/invalid runtime context | Access context unusable; explicit failure |
| Workload/non-interactive failure | CI/workload auth fails closed; no insecure fallback |
| Interactive failure | Interactive auth fails explicitly; no silent degradation |
| Secure failure with no insecure fallback | No automatic downgrade to less-secure mode |
| No privilege escalation | Authentication does not grant additional privileges dynamically |
| No automatic broader-scope request | Only minimum required scopes requested |
| Token/credential non-propagation | Tokens do not enter normalized state, graph, findings, evidence, reports, or logs |

### 5.2 Verification requirements

Tests MUST verify:

- Authentication success != authorization for every capability
- Authorization denial cannot become PASS
- Authentication/system failure cannot become tenant FAIL
- Credentials/tokens cannot enter normalized state, graph, findings, evidence, reports, or logs
- AuthorizedAccessContext remains runtime-only security-sensitive state

Exact OAuth/OIDC flows and exact Microsoft permissions remain TBD.

---

## 6. Normalization Testing

### 6.1 Test scenarios

| Scenario | Test expectation |
| --- | --- |
| Valid synthetic observations | Correctly normalized domain records with provenance |
| Missing optional fields | Absent semantics preserved per domain-model null/unknown/absent semantics |
| Missing required semantic inputs | Normalization failure; structured error; NOT_EVALUATED or ERROR downstream |
| Malformed values | Validation failure; structured error; no silent inference |
| Duplicate provider objects | Deterministic deduplication; single canonical record |
| Conflicting observations | Conflict remains detectable; not silently resolved by heuristic inference |
| Unknown object kinds | Explicit unknown-kind handling; not silently coerced |
| Unsupported capability | Capability state = unsupported; no false normalization success |
| Partial collection | Partial normalization with explicit capability state |
| Tenant mismatch | Cross-tenant normalization prevented; fails safely |
| Invalid provenance | Provenance validation failure; structured error |
| Unexpected provider extensions | Unknown extensions preserved as unknown; not silently discarded or invented |
| Type-confusion attempts | Type confusion rejected; not silently coerced into valid domain type |

### 6.2 Verification requirements

Tests MUST verify:

- Provider-specific objects cannot bypass normalization
- Unknown/missing semantics remain explicit
- No invented facts, no fabricated relationships, no fabricated provenance
- No cross-tenant normalization
- Normalization failure cannot silently create a valid security fact

---

## 7. Identity Graph Testing

### 7.1 Test scenarios

| Scenario | Test expectation |
| --- | --- |
| Node construction from normalized entities | Correct node kinds, domain entity references, provenance |
| Observed relationships | Edges correctly represent observed source relationships |
| Derived relationships where explicitly supported | Derived edges correctly computed, labeled as derived, traceable to source facts |
| Provenance preservation | Every node and edge carries provenance references |
| Duplicate nodes/edges | Deterministic deduplication; single canonical node per identity key |
| Missing relationships | Missing edges from unavailable collection reflected in capability state |
| Cycles | Cycles detected and handled; traversal bounded |
| Self-references | Self-references handled correctly; no infinite traversal |
| Disconnected graphs | Disconnected subgraphs handled; no silent joining |
| High-degree nodes | High-degree nodes processed within bounded resources |
| Deep graph structures | Deep traversals bounded; no unbounded recursion |
| Pathological/cyclic traversal | Traversal terminates; resource exhaustion visible |
| Cross-tenant edge attempts | Cross-tenant edges rejected; INV-G1 preserved |
| Unknown relationship types | Unknown types explicitly represented; not silently interpreted |
| Incomplete graph construction | Incomplete graph reflected in capability state; no false PASS |
| Deterministic graph projection | Identical normalized inputs produce identical graph structure |

### 7.2 Verification requirements

Tests MUST verify:

- Graph is a projection, not a second source of truth
- No display-name identity joins
- No cross-tenant joins
- Derived edges remain distinguishable from observed edges
- Graph failures cannot silently result in PASS
- Traversal is architecturally bounded
- Pathological structures cannot cause unbounded processing by design

---

## 8. Deterministic Rule Engine Testing

This is a critical section. The rule-state matrix and verification requirements are release-gate material.

### 8.1 Rule-state matrix

| Verdict | Requirements | Tests must prove |
| --- | --- | --- |
| **PASS** | Requires affirmative rule conditions satisfied; requires sufficient input completeness; cannot result merely because evidence is absent | PASS occurs only when rule conditions are affirmatively met; PASS requires the specific input completeness defined by the rule; absence of data alone does not produce PASS |
| **FAIL** | Requires deterministic affirmative evidence; cannot result solely from missing data; cannot result solely from authentication/system failure | FAIL occurs only when the rule's failure predicate is deterministically satisfied; FAIL requires traceable evidence; missing data alone does not produce FAIL; system error alone does not produce FAIL |
| **NOT_EVALUATED** | Correctly represents inability to evaluate when required capability/input is unavailable according to rule semantics | NOT_EVALUATED produced when required capability is unavailable; NOT_EVALUATED produced when required input is missing; NOT_EVALUATED carries structured reason identifying the gap |
| **NOT_APPLICABLE** | Distinct from NOT_EVALUATED; represents a rule that genuinely does not apply to the identity type or context | NOT_APPLICABLE produced when applicability predicate returns false; NOT_APPLICABLE carries structured reason; NOT_APPLICABLE is not used to hide unsupported functionality |
| **ERROR** | Represents evaluation/system failure; remains distinct from tenant security FAIL | ERROR produced on evaluation exception or invalid state; ERROR carries structured reason; ERROR is not converted to FAIL to simplify reporting |

### 8.2 Determinism verification

Tests MUST prove:

- Same normalized input + same configuration + same assessment-time reference => same result
- No hidden wall-clock dependency in rule evaluation
- No randomness affecting verdict
- No AI/LLM affecting verdict
- No provider API calls from rule logic
- Per-rule failure isolation
- Shared-state integrity failure behavior
- Malformed rule input handling
- Capability-state combinations
- Missing provenance handling
- Missing evidence handling
- Deterministic ordering where semantically required

Exact implementation interfaces remain TBD.

---

## 9. False PASS / False FAIL Defense Testing

These are release-critical semantic security tests.

### 9.1 False PASS defense tests

| Attack path | Test expectation |
| --- | --- |
| Incomplete collection -> PASS | Incomplete collection produces NOT_EVALUATED or ERROR, not PASS |
| Authorization denial -> PASS | Authorization denial produces NOT_EVALUATED, not PASS |
| Unsupported capability -> PASS | Unsupported capability produces NOT_EVALUATED or NOT_APPLICABLE, not PASS |
| Collection error -> PASS | Collection error produces ERROR or NOT_EVALUATED, not PASS |
| Normalization failure -> PASS | Normalization failure produces ERROR, not PASS |
| Graph failure -> PASS | Graph failure affecting required input produces ERROR, not PASS |
| Resource exhaustion -> successful assessment | Resource exhaustion produces visible failure, not successful assessment |
| Cancellation -> successful assessment | Cancellation produces visible cancellation state, not successful assessment |
| Missing evidence -> PASS | Missing evidence for PASS prevents PASS emission; PASS requires evidence |
| Missing provenance -> fabricated evidence | Missing provenance is represented explicitly; evidence is not fabricated |

### 9.2 False FAIL defense tests

| Incorrect path | Test expectation |
| --- | --- |
| Authentication failure -> tenant FAIL | Authentication failure produces ERROR or NOT_EVALUATED, not tenant FAIL |
| Authorization denial -> tenant FAIL | Authorization denial produces NOT_EVALUATED, not tenant FAIL |
| Unsupported capability -> tenant FAIL | Unsupported capability produces NOT_EVALUATED or NOT_APPLICABLE, not tenant FAIL |
| Missing data alone -> tenant FAIL | Missing data alone does not produce FAIL; FAIL requires affirmative evidence |
| System error -> tenant FAIL | System error produces ERROR, not tenant FAIL |

### 9.3 Verification requirements

Tests MUST verify:

- Every false-PASS attack path produces the correct non-PASS state
- Every false-FAIL attack path produces the correct non-FAIL state
- ERROR remains distinct from FAIL at every boundary
- NOT_EVALUATED remains distinct from PASS
- These tests are executed against the full pipeline, not just individual components

---

## 10. Findings/Evidence/Provenance Testing

### 10.1 Test scenarios

| Scenario | Test expectation |
| --- | --- |
| RuleEvaluation remains authoritative | Findings cannot change evaluation state |
| Evidence cannot upgrade/downgrade state | Evidence does not alter RuleEvaluation |
| Provenance chain preservation | Every finding/evidence traces to source observation through normalization |
| Unknown provenance remains explicit | Unknown provenance is not invented |
| No evidence fabrication | Evidence cannot be created without corresponding source data |
| No cross-tenant evidence | Evidence from tenant A cannot support tenant B findings |
| PASS evidence sufficiency | PASS carries evidence sufficient to establish required completeness |
| FAIL evidence sufficiency | FAIL carries affirmative deterministic evidence |
| Structured reason for NOT_EVALUATED | NOT_EVALUATED carries reason identifying missing capability/data |
| Structured reason for NOT_APPLICABLE | NOT_APPLICABLE carries reason identifying applicability criterion |
| Structured reason for ERROR | ERROR carries reason describing the failure |
| Diagnostic separation | Diagnostics remain distinct from tenant security findings |
| Redaction cannot change assessment truth | Redaction does not alter evaluation state or fabricate evidence |

### 10.2 Secret injection testing

Inject secret-like synthetic strings (synthetic token patterns, synthetic key patterns, synthetic password patterns) into test fixtures and verify they are rejected or redacted according to future implementation contracts. This uses no real secrets.

Exact redaction implementation remains TBD.

---

## 11. Output/Renderer Security Testing

### 11.1 CLI security tests

| Test | Expected behavior |
| --- | --- |
| ANSI/control-character injection | Control characters safely encoded or stripped; no terminal manipulation |
| Newline manipulation | Newlines in tenant strings do not corrupt output structure |
| Unicode/bidirectional control | Bidi control characters do not mislead terminal rendering |
| Misleading terminal rendering | Tenant strings cannot manipulate terminal display to mislead |

### 11.2 JSON security tests

| Test | Expected behavior |
| --- | --- |
| Valid serialization | JSON output is valid and parseable |
| Malicious/untrusted strings | Strings safely serialized; no JSON injection |
| Schema compatibility | Output conforms to defined schema (when schema is defined) |
| Semantic preservation | All five evaluation states preserved |

### 11.3 SARIF security tests

| Test | Expected behavior |
| --- | --- |
| Semantic preservation | Assessment semantics preserved through SARIF mapping |
| Safe serialization | SARIF output is valid and parseable |
| No misleading state conversion | Evaluation states not forced into incorrect SARIF meanings |
| Interoperability mapping | Mapping follows SARIF specification (when mapping is defined) |

### 11.4 HTML security tests

| Test | Expected behavior |
| --- | --- |
| HTML/script injection | All tenant strings safely encoded; no script execution |
| XSS | No cross-site scripting possible through tenant-controlled strings |
| Malicious links/URIs | Dangerous URI schemes safely handled |
| Malformed Unicode | Malformed Unicode does not break rendering |
| External-resource leakage | Static report does not fetch external resources |
| Static/self-contained behavior | Report is self-contained where required |

### 11.5 Cross-renderer verification

Tests MUST verify across all renderers:

- Renderer cannot change RuleEvaluation truth
- All five states remain distinguishable
- ERROR/NOT_EVALUATED cannot be hidden as success
- No renderer can call provider APIs
- No renderer receives authentication credentials
- No tenant/provider string is treated as executable control content
- No secret material is emitted

---

## 12. Filesystem/Artifact Testing

### 12.1 Test scenarios

| Scenario | Test expectation |
| --- | --- |
| Path traversal | Path traversal attempts blocked; output stays within intended destination |
| Absolute-path injection | Absolute paths from tenant strings rejected or sanitized |
| Unsafe filenames | Unsafe filenames sanitized |
| Reserved filenames | Reserved filenames handled safely |
| Symlink/reparse-point behavior | Symlink/reparse-point hazards addressed |
| Overwrite behavior | Existing artifacts not silently overwritten without explicit policy |
| Interrupted writes | Interrupted writes produce visible failure; partial artifacts cleaned up |
| Partial artifacts | Partial artifacts do not appear as successfully completed reports |
| Temporary-file handling | Temporary files cleaned up; no inconsistent state left |
| Concurrent write/race scenarios | Concurrent writes handled safely |
| Permission failures | Permission failures produce visible error |
| Disk-full conditions | Disk-full produces visible error; no silent success |
| Cancellation during publication | Cancellation produces visible state; no partial success |
| Artifact integrity | Artifact integrity mechanisms verified (when implemented) |

### 12.2 Verification requirements

Tests MUST verify:

- Tenant-controlled values cannot directly determine unsafe paths
- Partial output cannot appear as a successfully completed report
- Write failure is visible
- Failure cannot silently mutate assessment truth

---

## 13. Tenant Isolation Testing

Tenant isolation is a release-critical test property.

### 13.1 Test scenarios

| Scenario | Test expectation |
| --- | --- |
| Tenant A observations cannot normalize into tenant B | Cross-tenant normalization rejected |
| Tenant A nodes cannot join tenant B nodes | Cross-tenant graph edges rejected [INV-G1] |
| Tenant A evidence cannot support tenant B findings | Cross-tenant evidence correlation rejected |
| Tenant A capability state cannot influence tenant B | Capability state remains tenant-scoped |
| Tenant A diagnostics/artifacts cannot be mixed with tenant B | Diagnostics and artifacts remain tenant-scoped |
| Runtime tenant mismatch fails safely | Tenant-context mismatch produces explicit failure |
| Sequential assessments do not leak tenant state | No state leakage between sequential assessments |
| Concurrent assessments preserve tenant isolation | If supported later, concurrent assessments maintain isolation |
| Caches preserve tenant isolation | If introduced later, caches are tenant-safe |

### 13.2 Verification requirements

Tests MUST verify:

- No cross-tenant data mixing at any pipeline stage
- Tenant-context mismatch fails visibly
- Each assessment context is fully isolated
- Output artifacts are scoped to single tenant

---

## 14. Resource Exhaustion/Resilience Testing

### 14.1 Test scenarios

| Scenario | Test expectation |
| --- | --- |
| Very large synthetic identity sets | Processing completes within bounded resources |
| High-degree graphs | Traversal bounded; no unbounded memory growth |
| Deep graphs | Traversal depth bounded; no infinite recursion |
| Cyclic graphs | Cycles detected; traversal terminates |
| Duplicate-heavy input | Deduplication deterministic; bounded resource use |
| Oversized strings | Strings handled within bounds; no unbounded allocation |
| Malformed data | Malformed input produces structured failure |
| Repeated provider failures | Bounded retry; failure visible after budget exhausted |
| Cancellation | Cancellation produces visible state; resources released |
| Timeouts | Timeouts produce visible failure; no silent partial success |
| Bounded retries | Retry budget enforced; exhaustion visible |
| Memory pressure conceptually | Memory pressure produces visible failure; no silent success |
| Output size pressure | Large output handled within bounds |
| Disk failure | Disk failure produces visible error |
| Pathological rule input | Pathological input produces structured failure |

### 14.2 Verification requirements

Tests MUST verify:

- Resource exhaustion cannot be reported as successful assessment
- Partial processing cannot silently become PASS
- Cancellation remains visible
- Failures remain deterministic and auditable where possible
- Bounded processing is enforced once concrete limits exist

Exact numerical limits remain TBD.

---

## 15. Security/Adversarial Test Suite

Define an explicit adversarial suite corresponding to the threat model. Each test maps conceptually to threat-model controls.

### 15.1 Adversarial test categories

| Category | Threat-model mapping | Test approach |
| --- | --- | --- |
| Credential leakage | Threat 1 (Token/credential disclosure), Threat 23 (Secret leakage) | Inject synthetic credential patterns; verify no leakage to output/logs/evidence |
| Excessive privilege assumptions | Threat 2 (Excessive privilege) | Verify least-privilege enforcement; verify no implicit broader-scope request |
| Wrong tenant | Threat 3 (Wrong-tenant execution) | Simulate tenant-context mismatch; verify safe failure |
| Cross-tenant contamination | Threat 4 (Cross-tenant data contamination) | Inject cross-tenant data; verify no mixing at any pipeline stage |
| Provider normalization bypass | Threat 5, 7, 8 (Provider-object spoofing, normalization bypass) | Attempt to pass provider-specific types through normalization boundary; verify rejection |
| Type confusion | Threat 7, 8 | Inject type-confusion payloads; verify correct handling |
| Capability manipulation | Threat 9 (Capability-state confusion) | Manipulate capability states; verify correct evaluation behavior |
| Provenance fabrication | Threat 13 (Evidence fabrication), Threat 14 (Provenance loss) | Attempt to inject fabricated provenance; verify rejection |
| Evidence manipulation | Threat 13 | Attempt to modify evidence references; verify integrity |
| Rule/config tampering | Threat 15 | Inject modified rule configuration; verify detection and safe failure |
| False PASS | Threat 10, 11 (Incomplete collection, authorization denial) | Execute every false-PASS attack path; verify correct non-PASS state |
| False FAIL | Threat 12 (Auth failure -> tenant FAIL) | Execute every false-FAIL attack path; verify correct non-FAIL state |
| Renderer state reinterpretation | Threat 16 (Renderer changing security meaning) | Verify renderers cannot alter evaluation state |
| XSS | Threat 17 (HTML/XSS injection) | Inject HTML/JS payloads into tenant strings; verify safe encoding |
| Terminal injection | Threat 18 (Terminal/control-character injection) | Inject ANSI/control characters; verify safe handling |
| Path traversal | Threat 20 (Filesystem path traversal) | Inject path-traversal strings; verify output stays within bounds |
| Symlink/reparse attacks | Threat 20 | Test symlink/reparse-point behavior |
| Unsafe overwrite | Threat 21 | Test overwrite behavior; verify no silent destruction |
| Partial artifact publication | Threat 22 | Simulate mid-generation failure; verify partial artifacts not presented as complete |
| Malicious repository fixture | Threat 29 (Dependency/supply-chain) | Verify fixture validation; no execution of fixture content as code |
| Resource exhaustion | Threat 24, 25 | Inject pathological input; verify bounded processing and visible failure |
| Future dependency/supply-chain compromise | Threat 29 | Architecture anticipates future SBOM and signing verification (TBD) |
| Future plugin abuse | Threat 31 | Architecture anticipates future sandboxing (TBD) |
| Future AI/LLM authority violation | Threat 30 | Verify AI cannot determine or modify verdicts |

---

## 16. Property-Based/Fuzz Testing

### 16.1 Suitable boundaries for property-based testing

| Boundary | Security property |
| --- | --- |
| Provider-data parsing | Arbitrary provider input cannot crash parser or produce insecure state |
| Normalization | Arbitrary provider input cannot create cross-tenant relationships; arbitrary missing data cannot create PASS without completeness; arbitrary input cannot fabricate provenance |
| Graph construction | Arbitrary normalized input cannot create cross-tenant edges; arbitrary input cannot produce unbounded traversal |
| Graph traversal | Arbitrary graph structures cannot cause infinite loops or unbounded resource use |
| Rule input handling | Arbitrary malformed rule input cannot produce PASS or FAIL; missing data cannot silently become PASS |
| Output serialization | Arbitrary assessment data cannot produce invalid output format |
| HTML/terminal sanitization | Arbitrary provider strings cannot become executable output |
| Filesystem path handling | Arbitrary tenant strings cannot determine unsafe filesystem paths |

### 16.2 Fuzzing boundaries

| Boundary | Fuzz approach |
| --- | --- |
| Provider response parsing | Fuzz with malformed JSON, truncated payloads, oversized strings, invalid Unicode |
| Normalization | Fuzz with type-confusion inputs, missing fields, conflicting observations |
| Graph construction | Fuzz with pathological graph structures, cyclic inputs, extreme cardinality |
| Rule evaluation | Fuzz with malformed normalized inputs, missing capability states, invalid configuration |
| Output rendering | Fuzz with adversarial strings, control characters, injection payloads |
| Filesystem paths | Fuzz with path-traversal strings, absolute paths, unsafe filenames, Unicode paths |

Do not select a concrete fuzzing framework yet.

---

## 17. Regression/Golden Testing

### 17.1 Regression test strategy

Known security bugs and semantic regressions become permanent regression tests. Each regression test captures:

- The specific scenario that caused the regression
- The expected correct behavior
- The threat-model or invariant it protects
- The classification of the test (security-critical, functional, etc.)

### 17.2 Golden/snapshot testing

Golden/snapshot testing is used where useful for:

| Output | Usage |
| --- | --- |
| Deterministic canonical output | Verify output stability for known-good inputs |
| JSON semantic structure | Verify JSON output structure and evaluation states |
| SARIF structure | Verify SARIF mapping (once mapping is defined) |
| HTML safe rendering | Verify HTML encoding of tenant strings |
| CLI rendering | Verify CLI output correctness |

### 17.3 Golden test discipline

Golden tests MUST NOT become an excuse to approve incorrect output merely because a snapshot changed. Security-relevant snapshot changes require review. A snapshot update that changes an evaluation state from PASS to FAIL (or vice versa) is a security-relevant change requiring explicit review and approval.

---

## 18. CI Testing Architecture

This section designs future CI test principles. Do NOT claim CI currently implements these controls.

### 18.1 CI test principles (DESIGNED / REQUIRED)

| Principle | Description |
| --- | --- |
| Least-privilege CI | CI execution uses only minimum required permissions |
| No production tenant credentials | CI does not use real tenant credentials for ordinary test execution |
| No real secrets for ordinary PR tests | Pull request tests use synthetic fixtures only |
| Untrusted PRs cannot access privileged secrets | Untrusted pull request context cannot access secrets required for privileged tests |
| Deterministic synthetic test suite | CI runs the full synthetic test suite deterministically |
| Security/adversarial suite | CI includes security and adversarial tests as required gates |
| Dependency/supply-chain checks | When tooling is selected, CI validates dependency integrity |
| Secret scanning | CI includes secret scanning to prevent credential commits |
| Build/test isolation | CI builds and tests in isolated environments |
| Pinned/verified dependencies/actions | Where later implemented, dependencies and actions are pinned |
| Artifact integrity/provenance | Where later implemented, release artifacts carry integrity verification |
| Clear failure gating | Test failures block merge/release |
| No automatic publication from untrusted PR context | Untrusted PR context cannot trigger publication |

Exact GitHub Actions workflow, permissions, action SHAs, scanners, SBOM tooling, signing, and release pipeline remain TBD.

### 18.2 CI security boundaries (DESIGNED / REQUIRED)

| Boundary | Requirement |
| --- | --- |
| PR test isolation | Untrusted PR tests run with synthetic fixtures only; no access to privileged secrets or live tenant contexts |
| Secret boundary | CI secrets (if any) are not accessible to untrusted PR contexts |
| Publication boundary | Only trusted (merged) context can publish release artifacts |
| Dependency boundary | Dependencies validated before use |

Do not claim CI currently implements these controls.

---

## 19. Test Traceability

### 19.1 Conceptual traceability chain

```
Requirement
    ->
Architecture invariant
    ->
Threat / trust boundary
    ->
Test property
    ->
Test case
    ->
Result/evidence
```

### 19.2 Traceability requirements

- Security-critical invariants SHOULD eventually have explicit verification evidence
- Each test SHOULD be traceable to one or more requirements, invariants, or threat-model entries
- Test results SHOULD be attributable to specific test cases and input fixtures
- Regression tests SHOULD reference the specific defect or threat they protect against

Do not invent a concrete traceability tool yet.

---

## 20. Release Security Gates

### 20.1 Conceptual V1 release gates

A release MUST NOT be considered security-verified merely because it compiles. Before V1 release, architecture requires evidence that critical properties have been tested.

### 20.2 Required release-gate properties

| Property | Gate requirement |
| --- | --- |
| Read-only behavior | VERIFIED that no write/delete/administrative operations occur against provider APIs |
| Least privilege | VERIFIED that authentication requests only minimum required permissions |
| Tenant isolation | VERIFIED that cross-tenant data mixing does not occur at any pipeline stage |
| Secret exclusion | VERIFIED that no credential material enters output, logs, evidence, or normalized state |
| Deterministic rule states | VERIFIED that identical inputs produce identical evaluation outcomes |
| False-PASS prevention | VERIFIED that every false-PASS attack path produces correct non-PASS state |
| False-FAIL prevention | VERIFIED that every false-FAIL attack path produces correct non-FAIL state |
| Capability-aware semantics | VERIFIED that missing capability produces NOT_EVALUATED, not PASS or FAIL |
| Provenance/evidence integrity | VERIFIED that evidence traces to source observations; no fabrication |
| Renderer non-authority | VERIFIED that output renderers cannot alter evaluation state |
| Output injection resistance | VERIFIED that tenant strings are safely encoded in all output formats |
| Filesystem safety | VERIFIED that path traversal and unsafe overwrite are prevented |
| Bounded processing | VERIFIED that pathological input produces visible failure, not silent success |
| Cancellation/failure visibility | VERIFIED that cancellation and failure are visible, not silent success |
| CI/supply-chain controls | VERIFIED that CI implements required security controls (once implemented) |

### 20.3 Gate semantics

A release gate is pass/fail based on required security properties and unresolved critical defects, not marketing readiness. No arbitrary numeric pass percentages are established.

---

## 21. Architecture Invariant Coverage

Every architecture invariant (INV-01 through INV-16) MUST have at least one corresponding verification strategy.

| Invariant | Verification strategy | Test layer(s) |
| --- | --- | --- |
| INV-01 Read-only tenant operation | Verify no write/delete/admin operations in collector and provider-boundary tests | Unit, component, integration, security |
| INV-02 Deterministic assessment | Verify same inputs + same config + same reference => same outcome across rule-engine tests | Unit, component, property-based |
| INV-03 Provider isolation | Verify rule engine has no provider API references, SDK imports, or HTTP calls; test through contract and component tests | Unit, component, contract |
| INV-04 Normalized domain boundary | Verify provider-specific types do not enter rule contracts; normalization tests with type-confusion inputs | Unit, component, contract, adversarial |
| INV-05 Explicit evaluation states | Verify exactly five terminal states; no silent coercion of missing capability into PASS or FAIL | Unit, component, rule-state matrix |
| INV-06 Evidence traceability | Verify PASS/FAIL carry evidence references; non-terminal states carry structured reason | Unit, component, findings/evidence tests |
| INV-07 Capability awareness | Verify capability state propagation; verify missing capability produces NOT_EVALUATED | Unit, component, integration, capability-state tests |
| INV-08 Least privilege | Verify authentication requests minimum scopes; no dynamic privilege expansion | Authentication/authorization tests, security tests |
| INV-09 Secret exclusion | Verify no credential material in output, logs, evidence, normalized state; inject synthetic secrets | Security, adversarial, output tests |
| INV-10 Tenant boundary preservation | Verify cross-tenant mixing prevented at every pipeline stage; tenant-isolation tests | Unit, component, integration, tenant-isolation tests |
| INV-11 Provenance preservation | Verify provenance traces from source through normalization through graph through findings | Unit, component, contract, provenance tests |
| INV-12 Core/output separation | Verify renderers cannot alter evaluation state; verify all five states preserved | Output, renderer, contract tests |
| INV-13 AI non-authority | Verify AI cannot determine or modify verdicts; verify rule engine produces deterministic results without AI | Rule-engine, security, adversarial tests |
| INV-14 Failure transparency | Verify failures produce ERROR/NOT_EVALUATED, never silent PASS; verify false-PASS attack paths | Failure-injection, false-PASS, security tests |
| INV-15 No undocumented capability dependency | Verify rules reference documented properties only; verify no invented Graph endpoints/permissions | Unit, component, contract tests |
| INV-16 Security-sensitive defaults | Verify secure defaults; verify no insecure fallback; verify weakening requires explicit opt-in | Configuration, authentication, security tests |

---

## 22. TBD Register

The following implementation-sensitive decisions remain TBD. Do NOT resolve these without implementation evidence.

| TBD | Category |
| --- | --- |
| Test framework(s) | Tooling |
| Mocking/faking libraries | Tooling |
| Property-based framework | Tooling |
| Fuzzing framework | Tooling |
| Integration-test environment | Environment |
| Microsoft Graph contract-fixture mechanism | Fixtures |
| Optional live integration-test strategy | Environment |
| Coverage tooling/thresholds | Metrics |
| Performance thresholds | Metrics |
| Timeout/retry limits | Configuration |
| Graph size/depth limits | Configuration |
| Concurrency model | Architecture |
| Filesystem test strategy across supported OSes | Environment |
| Sanitizer/encoding libraries | Tooling |
| Secret scanner | Tooling |
| SAST/dependency scanning | Tooling |
| SBOM tooling | Tooling |
| CI workflow | CI |
| CI permissions | CI |
| Action pinning | CI |
| Artifact provenance | Release |
| Release signing | Release |
| Cryptographic verification | Security |
| Test-result retention | CI |
| Future plugin test sandbox | Future |
| Future AI governance tests | Future |

---

## Self-Verification

Before finishing, verify the following:

1. Only `testing-architecture.md` was modified. **CONFIRMED**
2. No files were created/deleted/renamed/moved. **CONFIRMED**
3. No application code or executable tests were created. **CONFIRMED**
4. No dependencies were added. **CONFIRMED**
5. No Git operations were performed. **CONFIRMED**
6. Testing remains synthetic by default. **CONFIRMED**
7. No real credentials/tenant data are required. **CONFIRMED**
8. All five rule states are tested distinctly (Section 8). **CONFIRMED**
9. False PASS defenses are explicit (Section 9). **CONFIRMED**
10. False FAIL defenses are explicit (Section 9). **CONFIRMED**
11. Authentication and authorization failures remain distinct (Section 5). **CONFIRMED**
12. Tenant isolation is release-critical (Section 13, Section 20). **CONFIRMED**
13. Provider isolation/normalization bypass is tested (Section 6, Section 15). **CONFIRMED**
14. Evidence/provenance security is tested (Section 10). **CONFIRMED**
15. Renderer/output injection is tested (Section 11). **CONFIRMED**
16. Filesystem attacks are tested (Section 12). **CONFIRMED**
17. Resource exhaustion/cancellation is tested (Section 14). **CONFIRMED**
18. CI/supply-chain testing is architected but not falsely claimed implemented (Section 18). **CONFIRMED**
19. AI/LLM non-authority is tested (Section 15, Section 21). **CONFIRMED**
20. INV-01 through INV-16 have verification coverage (Section 21). **CONFIRMED**
21. Implementation-specific choices remain TBD (Section 22). **CONFIRMED**
22. No undocumented Microsoft behavior was invented. **CONFIRMED**
23. No exact Graph endpoints/permissions were invented. **CONFIRMED**
24. No security control is falsely claimed as verified merely because it is documented. **CONFIRMED**
