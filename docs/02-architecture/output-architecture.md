# Output Architecture

> **Status:** V1 Architecture Baseline
> **Date:** 2026-09-18

---

## Purpose

Define the enterprise-security architecture for EntraNHI output generation. Establish the trust boundaries, security controls, and semantic guarantees for all V1 output surfaces: CLI, JSON, SARIF, and HTML.

---

## 1. Authoritative Data Boundary

Output generation consumes authoritative assessment results produced by the core. The output layer is strictly downstream of deterministic assessment.

**The output layer MUST NOT:**

- rerun security rules
- call Microsoft Graph
- call ARM or another provider
- collect additional tenant data
- request additional privileges
- alter RuleEvaluation state
- reinterpret PASS/FAIL
- convert ERROR to FAIL
- convert NOT_EVALUATED to PASS
- fabricate evidence
- invent missing provenance
- perform remediation
- permit AI/LLM to determine output security semantics

Output is a projection of authoritative core assessment results. It is not a second source of truth.

---

## 2. Canonical Output Model

A conceptual provider-independent **CanonicalOutputModel** represents the information needed for output generation. It references or contains only:

- assessment identity/context
- tenant-context reference
- assessment-time/reference metadata
- tool/version metadata
- active rule-set/configuration reference
- RuleEvaluations
- Findings where emitted
- Evidence/provenance references appropriate for output
- capability summary
- diagnostics
- completeness/integrity status where defined
- output/redaction metadata where relevant

**Constraints:**

- Do NOT define concrete C# types in this document.
- Do NOT make the output model a second source of truth.
- It is a projection of authoritative core assessment results.

---

## 3. Semantic Preservation

All renderers must preserve the semantic distinction between the five evaluation states:

- **PASS**
- **FAIL**
- **NOT_EVALUATED**
- **NOT_APPLICABLE**
- **ERROR**

A renderer **MUST NOT** hide ERROR or NOT_EVALUATED in a manner that makes an assessment appear more secure or complete than it was.

Formatting differences are allowed. Security meaning differences are not.

Severity metadata must not change evaluation state.

---

## 4. Completeness / Assessment Status

Output must make incomplete assessment conditions visible. Examples conceptually include:

- unavailable capability
- partial collection
- rule NOT_EVALUATED
- rule ERROR
- evidence integrity problem
- output-generation failure

**Do NOT:**

- invent one global "secure/insecure" score
- reduce an assessment to a misleading green/red status

Exact assessment-summary semantics remain **TBD**.

---

## 5. CLI Output

CLI is a human/operator interface.

**Architecture must support:**

- concise default summary
- explicit evaluation-state counts
- visible errors/incomplete evaluation
- optional detailed findings/evidence references
- deterministic machine-usable exit behavior (eventually)
- non-interactive CI usage
- safe terminal rendering

**CLI MUST NOT:**

- hide failures because output is concise
- print tokens/secrets
- emit raw authentication material
- execute terminal control sequences derived from untrusted tenant data
- use AI-generated interpretation as authoritative status

Exact CLI command syntax and exit-code mapping remain **TBD**.

---

## 6. JSON Output

JSON is the canonical machine-readable export surface for V1 unless a later architecture decision defines otherwise.

**JSON architecture must:**

- preserve all five evaluation states
- preserve stable machine-readable identifiers
- preserve rule/version references
- preserve tenant context
- preserve capability/diagnostic information needed for correct interpretation
- preserve evidence/provenance references according to output policy
- support schema versioning
- use standards-compliant serialization
- treat strings as data, never executable content

**Do NOT:**

- define the final JSON schema yet
- claim byte-for-byte deterministic JSON unless canonical serialization is explicitly designed later

Semantic determinism is required; byte identity is **TBD**.

---

## 7. SARIF Output

SARIF is an interoperability representation, not the source of truth.

**Architecture must account for mapping EntraNHI concepts into SARIF without losing security meaning.**

Important considerations:

- FAIL findings may map naturally to SARIF results.
- PASS/NOT_EVALUATED/NOT_APPLICABLE/ERROR still require correct representation or accompanying metadata/diagnostics where SARIF semantics do not directly correspond.
- Do NOT force EntraNHI evaluation states into misleading SARIF meanings.
- Do NOT claim exact SARIF field mapping until validated against the relevant specification.
- Rule IDs/versions and evidence references should remain traceable.
- Untrusted strings must remain data.

Exact SARIF version/profile/mapping remains **TBD**.

Do not invent SARIF semantics.

---

## 8. HTML Output

HTML is a human-readable report representation.

**Treat ALL tenant/provider-derived text as untrusted.**

Future HTML rendering must safely encode/escape:

- identity display names
- application names
- descriptions
- owner/accountability text
- permission labels
- evidence text
- diagnostics
- rule metadata if externally configurable
- any other externally influenced string

**Architecture must anticipate:**

- HTML injection
- script injection/XSS
- URL/link injection
- unsafe inline content
- CSS/style injection where applicable
- unsafe embedded data
- dangerous URI schemes
- malformed Unicode

Prefer a self-contained static report architecture where practical, but do NOT mandate exact libraries/frameworks yet.

HTML report **MUST NOT** require execution of tenant-supplied active content.

Exact CSP, templating library, asset strategy, and browser hardening remain **TBD**.

---

## 9. Terminal / Control-Character Defense

CLI renderers must treat tenant/provider strings as hostile input.

Future implementation must address:

- ANSI escape/control sequences
- terminal title manipulation
- carriage-return/newline injection
- bidi/control Unicode
- log-forging effects
- malformed Unicode

Do not implement sanitization now. Do not assume a display name is safe because it came from Microsoft Graph.

---

## 10. File / Path Safety

Output artifact generation is a filesystem trust boundary.

Future implementation must defend against:

- path traversal
- absolute-path injection
- unsafe file names
- reserved/special file names
- symlink/reparse-point hazards where applicable
- accidental overwrite
- output path escaping intended destination
- race/partial-write behavior
- unsafe temporary files

Tenant/provider-controlled values **MUST NOT** directly determine filesystem paths without validated safe transformation.

Do NOT choose concrete path libraries/algorithms yet.

---

## 11. Safe Write / Partial Artifact Behavior

Output generation must not silently leave a partial artifact that appears to be a complete successful assessment.

Architecture should support future safe-write behavior such as:

- build/serialize before final publication where practical
- explicit completion/integrity state
- atomic replacement where supported and validated
- cleanup/quarantine of failed temporary artifacts
- visible output-generation errors

Do NOT prescribe exact filesystem implementation yet.

Do NOT silently overwrite existing artifacts by default without an explicit validated policy.

Exact overwrite/versioning policy remains **TBD**.

---

## 12. Output Confidentiality

Generated artifacts may contain sensitive enterprise security information.

Architecture must assume output files may expose:

- tenant identity information
- security findings
- privilege relationships
- permission exposure
- ownership/accountability information
- credential metadata
- tenant structure

Therefore output architecture must support future controls for:

- restrictive file permissions where applicable
- explicit output destinations
- safe CI artifact handling
- retention awareness
- redaction
- encryption where future persistence/threat model requires it
- operator warning/documentation where appropriate

**Do NOT:**

- claim OS-level confidentiality is already guaranteed
- choose exact permission modes/encryption mechanisms yet

---

## 13. Secret Exclusion

Outputs **MUST NEVER** intentionally contain:

- access tokens
- refresh tokens
- client-secret values
- passwords
- private keys
- authentication cookies
- recovery secrets
- authorization headers
- raw authentication material
- secret-bearing configuration

Output renderers must not log these values either.

Credential metadata is permitted only when explicitly classified as non-secret and required by the assessment/output policy.

---

## 14. Redaction

Future output-specific redaction may reduce disclosure.

**Redaction MUST NOT:**

- alter authoritative RuleEvaluation state
- fabricate evidence
- transform ERROR/NOT_EVALUATED into secure-looking output
- silently remove information necessary to understand assessment limitations

Where redaction materially affects interpretation, the artifact should indicate that information was withheld.

Exact redaction schema/policy remains **TBD**.

---

## 15. Diagnostics

System diagnostics must remain distinct from tenant security findings.

Output renderers should represent relevant diagnostics without turning them into tenant FAIL findings.

Diagnostics must be:

- structured where appropriate
- sanitized
- tenant-context aware where relevant
- free from authentication secrets
- visible when necessary to understand incomplete assessment

Renderer failure itself is a system/output diagnostic, not a tenant security finding.

---

## 16. Output Failure Is Fail-Safe

Output-generation failure **MUST NOT:**

- create a false PASS
- suppress an incomplete assessment and report success
- silently discard ERROR/NOT_EVALUATED
- mutate canonical assessment state
- fabricate a replacement result

If one renderer fails, other renderers may potentially continue only if the architecture can preserve clear independent success/failure status.

Exact fatal-vs-isolated renderer failure policy remains **TBD**.

---

## 17. Output Isolation

Each renderer should be isolated behind a stable output contract.

- Adding or changing an HTML renderer must not require changes to rule logic.
- Adding SARIF must not change normalized domain semantics.
- A renderer must not gain access to authentication credentials or provider clients merely for convenience.
- Future renderer interfaces should expose only required canonical output data.

---

## 18. Determinism

Given identical authoritative assessment input and identical output configuration, output must preserve identical security meaning.

**Do not overclaim:**

- byte-identical files
- identical timestamps
- identical whitespace
- identical property ordering
- identical environment-specific paths

Canonical serialization, reproducible builds, and byte-level reproducibility remain separate future decisions.

---

## 19. Versioning / Compatibility

Machine-readable output formats require explicit versioning.

Architecture must support:

- output schema/version identifier
- tool version
- rule IDs/versions
- forward compatibility strategy
- explicit handling of unsupported versions
- no silent reinterpretation of unknown fields/states

Exact versioning strategy remains **TBD**.

Breaking schema changes must be deliberate and documented.

---

## 20. Resource Exhaustion

Renderers must be designed defensively for large assessments.

Future implementation should address:

- bounded memory usage where practical
- streaming where appropriate
- cancellation
- large evidence sets
- pathological strings
- deeply nested structures
- excessive HTML/report size
- serialization failures

Resource exhaustion **MUST NOT** produce a misleading successful artifact.

Concrete limits remain **TBD**.

---

## 21. Logging Boundary

Output generation logs must not become a shadow copy of sensitive assessment artifacts.

Logging should:

- minimize tenant/security data
- exclude secrets
- avoid raw payloads
- sanitize untrusted strings
- distinguish operational diagnostics from report content

Exact logging schema remains **TBD**.

---

## 22. Telemetry / Network Boundary

Output rendering must not require network transmission of tenant assessment data.

- No renderer may automatically upload findings/evidence to third-party services.
- No undisclosed telemetry.
- A static HTML report must not silently fetch tenant-sensitive resources from external services.

Any future explicit upload/integration feature requires separate architecture, authorization, disclosure, and security review.

---

## 23. AI / LLM Boundary

AI may eventually generate a clearly non-authoritative explanation or summary from already-produced deterministic results.

**AI MUST NOT:**

- determine output evaluation state
- change canonical findings/evidence
- suppress diagnostics
- reinterpret incomplete assessment as secure
- fabricate evidence/provenance
- receive tenant assessment data through an undisclosed external transmission
- modify canonical machine-readable artifacts without controlled architecture

Core V1 outputs must not depend on AI availability.

---

## 24. CI / Automation Consumption

JSON/SARIF/CLI outputs may be consumed by CI systems.

Architecture must preserve:

- machine-readable state
- explicit incomplete/error conditions
- deterministic semantic behavior
- safe handling of untrusted output strings
- no assumption that CI logs are confidential
- no requirement for interactive authentication in the output layer

Exact CI policy/gating semantics remain **TBD**.

The output layer itself does not decide organizational deployment policy.

---

## 25. Future Dashboard / UI Boundary

A future dashboard/UI should consume stable output/core contracts.

The dashboard must not become a separate rule engine.

Do not design the dashboard implementation here. Do not assume SaaS hosting.

Future UI security requires separate architecture for:

- authentication
- authorization
- session security
- browser security
- data persistence
- tenant isolation

---

## 26. Testability

Future tests should cover at minimum:

- all five evaluation states preserved in output
- renderer cannot alter authoritative state
- PASS/FAIL evidence traceability
- NOT_EVALUATED/ERROR visibility
- tenant-context preservation
- secret exclusion
- HTML/script injection payloads
- dangerous URI schemes
- terminal ANSI/control injection
- CR/LF/log injection
- malformed Unicode and bidi controls
- JSON serialization correctness
- SARIF mapping validation
- path traversal
- absolute path attempts
- unsafe filenames
- overwrite prevention
- partial-write failure
- renderer exception isolation
- large/pathological assessment
- redaction semantics
- no network/telemetry by default
- AI non-authority
- schema/version compatibility behavior

Use synthetic fixtures. Do not create tests now.

---

## 27. Open Decisions / TBDs

Preserve TBDs including:

- concrete CanonicalOutputModel type
- JSON schema
- JSON schema versioning strategy
- canonical serialization strategy
- SARIF version/profile
- exact SARIF mapping
- HTML templating/rendering technology
- HTML CSP/security policy
- HTML asset strategy
- CLI command syntax
- CLI exit-code semantics
- terminal sanitization policy
- Unicode/bidi handling policy
- output file naming
- path validation strategy
- overwrite/versioning policy
- safe-write/atomic publication implementation
- temporary-file strategy
- artifact file permissions
- encryption requirements
- redaction schema/policy
- renderer failure isolation policy
- resource limits
- streaming strategy
- output logging schema
- CI gating policy
- artifact signing/hashing integration
- future dashboard contract

Do NOT resolve these by speculation.

---

## 28. Invariant Alignment

Cross-reference especially:

- **INV-02** deterministic assessment
- **INV-03** provider isolation
- **INV-04** normalized domain boundary
- **INV-05** explicit evaluation states
- **INV-06** evidence traceability
- **INV-09** secret exclusion
- **INV-10** tenant boundary preservation
- **INV-11** provenance preservation
- **INV-12** core/output separation
- **INV-13** AI non-authority
- **INV-14** failure transparency
- **INV-16** security-sensitive defaults

Do NOT modify architecture-invariants.md.

---

## 29. Self-Verification

Confirm:

1. Only output-architecture.md modified.
2. Nothing created/deleted/renamed/moved.
3. Output layer cannot alter RuleEvaluation state.
4. All five evaluation states remain distinguishable.
5. Incomplete/error assessment cannot appear falsely secure.
6. JSON remains machine-readable and version-aware.
7. SARIF mapping does not invent semantics.
8. HTML treats tenant strings as untrusted.
9. Terminal/control injection risk is addressed.
10. Filesystem/path boundary is addressed.
11. Partial artifact behavior is fail-safe.
12. Sensitive output confidentiality is recognized.
13. Secrets/tokens are excluded.
14. Tenant isolation is preserved.
15. Renderer failures are distinct from tenant findings.
16. No renderer requires provider/authentication access.
17. No undisclosed telemetry/network transmission.
18. AI remains non-authoritative.
19. No concrete Microsoft permissions/endpoints invented.
20. No application code/tests/dependencies created.
21. No Git operations performed.

---

(End of document)