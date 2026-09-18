# EntraNHI Market Landscape & Competitive Benchmark

> **Status:** Phase 0.2.12-C
> **Date:** 2026-09-18
> **Research baseline:** Supplied high-level characterization. External claims require source revalidation before publication or major roadmap decisions.

---

## 1. Title

EntraNHI — Market Landscape & Competitive Benchmark

---

## 2. Purpose

This document establishes a living, evidence-conscious competitive benchmark framework for EntraNHI. It exists to:

1. Understand the strongest relevant products and projects in the identity security and NHI assessment space.
2. Prevent EntraNHI from blindly duplicating existing functionality.
3. Identify benchmark dimensions across multiple product categories.
4. Guide future product differentiation decisions.
5. Record areas where competitors may have stronger capabilities.
6. Prevent unsupported superiority or uniqueness claims.
7. Support future roadmap decisions with comparative context.
8. Provide a repeatable framework for periodic market reassessment.

This document does not rank products. It does not declare winners or losers. It is an engineering decision-support tool, not a marketing comparison.

---

## 3. Product Family

EntraNHI belongs to the following product family:

- **Identity Security**
  - **Non-Human Identity Security**
  - **Microsoft Entra Workload & AI Agent Security**
  - **Security Assessment / Posture Management**

Products benchmarked below span adjacent and overlapping areas within this family, but are not equivalent products. Each benchmark class represents a different dimension of capability, market presence, or technical approach.

---

## 4. Research Principles

1. **No invented capabilities.** Competitor capability statements are limited to the supplied research baseline. No capabilities are inferred from silence.
2. **No rankings.** No product is declared a winner, loser, best, or worst. The matrix is for engineering decisions, not marketing.
3. **No superiority claims.** EntraNHI is not described as superior to any benchmark product.
4. **No design-as-implementation.** EntraNHI's design intentions are clearly distinguished from implemented and verified capabilities.
5. **No unsourced technical claims.** No Microsoft Graph permissions, endpoints, properties, or relationships are invented. All platform claims reference documented capabilities only.
6. **Factual acquisition context.** Market-change context (acquisitions) is stated neutrally without speculation about strategy or roadmap.
7. **Capability-aware benchmarking.** Each benchmark class is evaluated on different dimensions. Cross-class equivalence is not claimed.
8. **Honest uncertainty.** Unknown or unvalidated capabilities are marked with controlled values, not guessed.

---

## 5. Benchmark Taxonomy

Benchmarks are organized into classes reflecting different product categories, capability dimensions, and competitive relationships. These classes are not mutually exclusive, but each benchmark is primarily relevant to one dimension.

| Class | Purpose | Benchmarks |
| --- | --- | --- |
| **Authoritative Platform** | Reference implementation for Microsoft identity semantics | Microsoft Entra |
| **Primary Commercial NHI** | Broadest commercial NHI security capabilities | Oasis Security / Cyera |
| **AI-Agent + Identity-Graph** | Agent and NHI identity graph analysis | Astrix Security / Cisco |
| **NHI Lifecycle / Security** | NHI discovery, classification, lifecycle, and security monitoring | Entro Security |
| **Runtime Workload IAM** | Workload identity access management and policy enforcement | Aembit |
| **Open-Source Microsoft Assessment** | Open-source Microsoft security testing quality benchmarks | Maester, Monkey365 |
| **Graph / Security Analysis** | Identity graph construction and attack-path analysis | BloodHound |

---

## 6. Authoritative Platform — Microsoft Entra

Microsoft Entra is the authoritative platform and reference implementation for the identity types, APIs, and semantics that EntraNHI assesses. It is not a competitor; it is the source system.

### Relevant Current Areas

The following Microsoft Entra areas are relevant to EntraNHI's scope:

- **Workload identities** — application registrations, service principals, managed identities.
- **Applications** — application objects and their configurations.
- **Service principals** — security principals created for application access.
- **Managed identities** — system-assigned and user-assigned managed identities backed by Azure-managed credentials.
- **Microsoft Entra Agent ID** — first-class agent identity resources where supported by documented Microsoft Graph capabilities.
- **Agent identities** — emerging identity category with evolving API surface and data model.
- **Agent identity blueprints** — agent-related application-derived resources.
- **Documented ownership and accountability concepts** — owner, sponsor, and manager relationships where surfaced through documented APIs.
- **Documented Agent ID relationships and capabilities** — where available through documented Microsoft Graph capabilities.

### Constraints on Research Claims

- Do not invent Graph mappings, endpoints, properties, permissions, or relationships.
- Agent identity capabilities are evolving and partially documented. EntraNHI must remain capability-aware.
- Exact provider mappings and relationship semantics for agent identities are implementation-design decisions and must not be inferred from this research baseline.

---

## 7. Commercial Benchmark Profiles

### 7.1 Oasis Security / Cyera

**Benchmark class:** Primary Commercial NHI Benchmark

**Approved high-level characterization (supplied research baseline):**

- Commercial NHI security benchmark.
- NHI inventory and discovery capabilities.
- Governance capabilities for non-human identities.
- Agentic access and security capabilities.
- Broad environment coverage across multiple technology environments.
- Commercial breadth is materially broader than EntraNHI V1 scope.

**Market change context:** Oasis became part of Cyera. This is stated factually; no speculation about acquisition strategy or future roadmap is made.

**EntraNHI comparison note:** EntraNHI V1 does not attempt to match this breadth. Oasis/Cyera's multi-environment commercial scope is out of EntraNHI V1's intended scope.

**Unresolved uncertainty:**
- Exact scope of agentic access/security capabilities requires validation against current product documentation.
- Integration status and product direction post-acquisition requires source revalidation.

---

### 7.2 Astrix Security / Cisco

**Benchmark class:** AI-Agent + Identity-Graph Benchmark

**Approved high-level characterization (supplied research baseline):**

- Important technical benchmark for NHI and AI-agent security.
- Identity graph concepts for NHI and agent relationships.
- Relationships involving agents, NHIs, secrets, permissions, owners, and resources.
- Posture and security analysis capabilities.
- Threat detection and remediation capabilities.
- Agent access and security capabilities.

**Market change context:** Astrix became part of Cisco in 2026. This is stated factually; no speculation about future product integration is made.

**EntraNHI comparison note:** Astrix Security represents an important technical benchmark for identity graph and agent security concepts. EntraNHI V1 does not implement full identity graph analysis or threat detection/remediation.

**Unresolved uncertainty:**
- Exact scope of current capabilities post-acquisition requires validation.
- Integration with Cisco's broader security portfolio is not characterized.

---

### 7.3 Entro Security

**Benchmark class:** NHI Lifecycle / Security Benchmark

**Approved high-level characterization (supplied research baseline):**

- NHI and security benchmark.
- Discovery and classification of non-human identities.
- Ownership and accountability capabilities.
- Permissions assessment.
- Posture management.
- Secrets and security lifecycle concerns.
- Behavioral and security monitoring.
- Remediation capabilities.

**EntraNHI comparison note:** Entro Security covers a broader lifecycle scope than EntraNHI V1, including behavioral monitoring and remediation. EntraNHI V1 is read-only and does not include runtime monitoring or remediation.

**Unresolved uncertainty:**
- Exact scope of discovery and classification capabilities requires validation.
- Monitoring and behavioral analysis specifics require source revalidation.

---

### 7.4 Aembit

**Benchmark class:** Runtime Workload IAM Benchmark

**Approved high-level characterization (supplied research baseline):**

- Workload identity and access management benchmark.
- Policy enforcement capabilities.
- Secretless and short-lived access concepts.
- Workload and agent access management.
- Runtime-context-based access decisions.
- Not primarily the same product category as offline/posture assessment.

**EntraNHI comparison note:** Aembit operates in a different product category — runtime workload access enforcement — than EntraNHI's offline/posture assessment focus. This benchmark is included because runtime workload IAM represents a related engineering concern, but the products serve different purposes.

**Unresolved uncertainty:**
- Exact scope of current capabilities and Entra integration depth requires validation.

---

## 8. Open-Source Benchmark Profiles

### 8.1 Maester

**Benchmark class:** Open-Source Microsoft Security Assessment Benchmark

**Approved high-level characterization (supplied research baseline):**

- Serious open-source Microsoft security assessment benchmark.
- Microsoft 365 and Entra security testing.
- Customizable tests.
- Reporting capabilities.
- Application and service-principal-related assessment coverage.
- Useful benchmark for open-source Microsoft security testing quality.

**EntraNHI comparison note:** Maester represents a quality benchmark for open-source Microsoft security testing. EntraNHI's evaluation model (five explicit states, evidence/provenance, capability awareness) is a different architectural approach, but Maester's testing rigor and reporting quality are relevant benchmarks.

**Unresolved uncertainty:**
- Exact scope of application/service-principal test coverage requires validation against current Maester documentation.
- Test-count statistics are intentionally not included; they change over time.

---

### 8.2 Monkey365

**Benchmark class:** Open-Source Microsoft Security Assessment Benchmark

**Approved high-level characterization (supplied research baseline):**

- Open-source Microsoft security assessment benchmark.
- Microsoft 365, Azure, and Entra assessment.
- Collector/rules architecture.
- Structured reporting.
- Entra-related assessment coverage including application and enterprise-application areas.

**EntraNHI comparison note:** Monkey365's collector/rules architecture is a relevant structural benchmark. EntraNHI's separation of collectors from the rule engine is a similar architectural principle, though the specific implementation approaches may differ.

**Unresolved uncertainty:**
- Exact scope of Entra-specific application/enterprise-application assessment coverage requires validation.
- Check-count statistics are intentionally not included; they change over time.

---

## 9. Graph / Security Analysis Benchmark

### 9.1 BloodHound

**Benchmark class:** Graph / Security Analysis Benchmark

**Approved high-level characterization (supplied research baseline):**

- Graph and security-analysis benchmark.
- Mature relationship and attack-path concepts.
- Azure and Entra service-principal-related graph concepts.
- Not a direct one-for-one EntraNHI product competitor.

**EntraNHI comparison note:** BloodHound is a mature graph-based security analysis tool with well-established attack-path and relationship modeling. EntraNHI V1 does not implement attack-path analysis. BloodHound's graph construction and relationship modeling approaches are a technical reference point, not a direct competitive comparison.

**Unresolved uncertainty:**
- Exact current scope of Entra-specific service-principal graph features requires validation.

**Important constraint:** EntraNHI V1 must not be described as an attack-path engine. Attack-path analysis is explicitly out of scope for V1.

---

## 10. Competitive Capability Framework

The following framework defines the benchmark dimensions across which products are compared. These dimensions reflect EntraNHI's intended engineering focus areas and areas where competitive context is relevant.

| Dimension | Description |
| --- | --- |
| **Microsoft Entra depth** | Depth of specialization in Microsoft Entra identity semantics, APIs, and data model |
| **NHI discovery/inventory** | Ability to discover and inventory non-human identities across environments |
| **Application registrations** | Assessment of application registration configurations and risks |
| **Service principals** | Assessment of service principal configurations and risks |
| **Managed identities** | Assessment of managed identity configurations and risks |
| **AI/agent identities** | Support for emerging AI and agent identity types |
| **Ownership/accountability** | Identification and assessment of ownership and accountability for NHIs |
| **Credential metadata/posture** | Assessment of credential metadata, hygiene, and lifecycle posture |
| **Permissions/exposure** | Assessment of API permissions, roles, and permission exposure |
| **Identity relationships/graph** | Construction and analysis of identity relationships and graphs |
| **Deterministic assessment** | Deterministic, reproducible evaluation from identical inputs |
| **Explicit assessment completeness** | Explicit representation of what was and was not evaluated |
| **PASS/FAIL/NOT_EVALUATED/NOT_APPLICABLE/ERROR semantics** | Five distinct evaluation states with clear semantic meaning |
| **Evidence/provenance** | Evidence and provenance attached to all findings |
| **False-PASS resistance** | Resistance to silently coercing missing data into false confidence |
| **Read-only assessment** | Assessment that performs no state mutations |
| **Attack-path analysis** | Analysis of attack paths, blast radius, or privilege escalation paths |
| **Real-time monitoring/threat detection** | Event-driven monitoring, alerting, or threat detection |
| **Remediation** | Automatic or guided remediation of security findings |
| **Policy enforcement** | Runtime policy enforcement for workload access |
| **CI/CD integration** | Integration with CI/CD pipelines and automation workflows |
| **JSON output** | Machine-readable JSON output |
| **SARIF output** | SARIF-format output for security tool integration |
| **HTML/human-readable reporting** | Human-readable report output |
| **Open-source inspectability** | Open-source code available for inspection and review |
| **Extensibility** | Ability to extend with custom rules, collectors, or integrations |
| **Multi-provider breadth** | Coverage across multiple identity providers or cloud platforms |
| **Deployment/operational simplicity** | Ease of deployment and operational use |
| **Security architecture transparency** | Transparent documentation of security architecture and constraints |
| **Adversarial/security testing** | Adversarial testing and security-focused quality assurance |
| **Release/supply-chain engineering** | Release engineering, provenance, and supply-chain security |
| **Documentation/developer experience** | Quality and completeness of documentation and developer experience |

---

## 11. Detailed Benchmark Matrix

The following matrix maps each benchmark product against the capability framework dimensions. Cells use controlled values as defined in the research principles.

### Legend

| Value | Meaning |
| --- | --- |
| Documented capability | Capability documented in supplied research baseline |
| Relevant capability | Capability relevant to this dimension based on product category |
| Benchmark area | Primary area this benchmark is used to evaluate |
| Not primary focus | Not a primary focus of this product |
| EntraNHI V1 intended | EntraNHI V1 intends to address this dimension |
| EntraNHI future consideration | May be addressed in future EntraNHI versions |
| EntraNHI V1 out of scope | Explicitly not in EntraNHI V1 scope |
| Not established in this research baseline | Capability not established in current research |
| Requires validation | Requires source revalidation before publication |

### 11.1 Microsoft Entra

| Dimension | Value |
| --- | --- |
| Microsoft Entra depth | Benchmark area |
| NHI discovery/inventory | Documented capability |
| Application registrations | Documented capability |
| Service principals | Documented capability |
| Managed identities | Documented capability |
| AI/agent identities | Documented capability (emerging) |
| Ownership/accountability | Documented capability |
| Credential metadata/posture | Documented capability |
| Permissions/exposure | Documented capability |
| Identity relationships/graph | Documented capability |
| Deterministic assessment | Not primary focus |
| Explicit assessment completeness | Not primary focus |
| PASS/FAIL/NOT_EVALUATED/NOT_APPLICABLE/ERROR semantics | Not primary focus |
| Evidence/provenance | Not primary focus |
| False-PASS resistance | Not primary focus |
| Read-only assessment | Relevant capability |
| Attack-path analysis | Not primary focus |
| Real-time monitoring/threat detection | Documented capability |
| Remediation | Documented capability |
| Policy enforcement | Documented capability |
| CI/CD integration | Not primary focus |
| JSON output | Documented capability |
| SARIF output | Not primary focus |
| HTML/human-readable reporting | Documented capability |
| Open-source inspectability | Not primary focus |
| Extensibility | Relevant capability |
| Multi-provider breadth | Not primary focus |
| Deployment/operational simplicity | Not primary focus |
| Security architecture transparency | Relevant capability |
| Adversarial/security testing | Not established in this research baseline |
| Release/supply-chain engineering | Not established in this research baseline |
| Documentation/developer experience | Relevant capability |

### 11.2 Oasis Security / Cyera

| Dimension | Value |
| --- | --- |
| Microsoft Entra depth | Not primary focus |
| NHI discovery/inventory | Documented capability |
| Application registrations | Documented capability |
| Service principals | Documented capability |
| Managed identities | Requires validation |
| AI/agent identities | Documented capability (agentic) |
| Ownership/accountability | Documented capability |
| Credential metadata/posture | Documented capability |
| Permissions/exposure | Documented capability |
| Identity relationships/graph | Requires validation |
| Deterministic assessment | Requires validation |
| Explicit assessment completeness | Requires validation |
| PASS/FAIL/NOT_EVALUATED/NOT_APPLICABLE/ERROR semantics | Requires validation |
| Evidence/provenance | Requires validation |
| False-PASS resistance | Requires validation |
| Read-only assessment | Requires validation |
| Attack-path analysis | Requires validation |
| Real-time monitoring/threat detection | Requires validation |
| Remediation | Requires validation |
| Policy enforcement | Requires validation |
| CI/CD integration | Requires validation |
| JSON output | Requires validation |
| SARIF output | Requires validation |
| HTML/human-readable reporting | Requires validation |
| Open-source inspectability | Not primary focus |
| Extensibility | Requires validation |
| Multi-provider breadth | Documented capability |
| Deployment/operational simplicity | Not primary focus |
| Security architecture transparency | Requires validation |
| Adversarial/security testing | Not established in this research baseline |
| Release/supply-chain engineering | Not established in this research baseline |
| Documentation/developer experience | Requires validation |

### 11.3 Astrix Security / Cisco

| Dimension | Value |
| --- | --- |
| Microsoft Entra depth | Requires validation |
| NHI discovery/inventory | Documented capability |
| Application registrations | Requires validation |
| Service principals | Requires validation |
| Managed identities | Requires validation |
| AI/agent identities | Documented capability |
| Ownership/accountability | Requires validation |
| Credential metadata/posture | Documented capability |
| Permissions/exposure | Documented capability |
| Identity relationships/graph | Documented capability (benchmark area) |
| Deterministic assessment | Requires validation |
| Explicit assessment completeness | Requires validation |
| PASS/FAIL/NOT_EVALUATED/NOT_APPLICABLE/ERROR semantics | Requires validation |
| Evidence/provenance | Requires validation |
| False-PASS resistance | Requires validation |
| Read-only assessment | Requires validation |
| Attack-path analysis | Documented capability |
| Real-time monitoring/threat detection | Documented capability |
| Remediation | Documented capability |
| Policy enforcement | Requires validation |
| CI/CD integration | Requires validation |
| JSON output | Requires validation |
| SARIF output | Requires validation |
| HTML/human-readable reporting | Requires validation |
| Open-source inspectability | Not primary focus |
| Extensibility | Requires validation |
| Multi-provider breadth | Requires validation |
| Deployment/operational simplicity | Not primary focus |
| Security architecture transparency | Requires validation |
| Adversarial/security testing | Not established in this research baseline |
| Release/supply-chain engineering | Not established in this research baseline |
| Documentation/developer experience | Requires validation |

### 11.4 Entro Security

| Dimension | Value |
| --- | --- |
| Microsoft Entra depth | Requires validation |
| NHI discovery/inventory | Documented capability (benchmark area) |
| Application registrations | Documented capability |
| Service principals | Documented capability |
| Managed identities | Requires validation |
| AI/agent identities | Requires validation |
| Ownership/accountability | Documented capability |
| Credential metadata/posture | Documented capability (benchmark area) |
| Permissions/exposure | Documented capability |
| Identity relationships/graph | Requires validation |
| Deterministic assessment | Requires validation |
| Explicit assessment completeness | Requires validation |
| PASS/FAIL/NOT_EVALUATED/NOT_APPLICABLE/ERROR semantics | Requires validation |
| Evidence/provenance | Requires validation |
| False-PASS resistance | Requires validation |
| Read-only assessment | Requires validation |
| Attack-path analysis | Requires validation |
| Real-time monitoring/threat detection | Documented capability |
| Remediation | Documented capability |
| Policy enforcement | Requires validation |
| CI/CD integration | Requires validation |
| JSON output | Requires validation |
| SARIF output | Requires validation |
| HTML/human-readable reporting | Requires validation |
| Open-source inspectability | Not primary focus |
| Extensibility | Requires validation |
| Multi-provider breadth | Requires validation |
| Deployment/operational simplicity | Not primary focus |
| Security architecture transparency | Requires validation |
| Adversarial/security testing | Not established in this research baseline |
| Release/supply-chain engineering | Not established in this research baseline |
| Documentation/developer experience | Requires validation |

### 11.5 Aembit

| Dimension | Value |
| --- | --- |
| Microsoft Entra depth | Requires validation |
| NHI discovery/inventory | Not primary focus |
| Application registrations | Not primary focus |
| Service principals | Requires validation |
| Managed identities | Requires validation |
| AI/agent identities | Requires validation |
| Ownership/accountability | Not primary focus |
| Credential metadata/posture | Relevant capability |
| Permissions/exposure | Relevant capability |
| Identity relationships/graph | Not primary focus |
| Deterministic assessment | Requires validation |
| Explicit assessment completeness | Requires validation |
| PASS/FAIL/NOT_EVALUATED/NOT_APPLICABLE/ERROR semantics | Requires validation |
| Evidence/provenance | Requires validation |
| False-PASS resistance | Requires validation |
| Read-only assessment | Not primary focus |
| Attack-path analysis | Not primary focus |
| Real-time monitoring/threat detection | Not primary focus |
| Remediation | Not primary focus |
| Policy enforcement | Documented capability (benchmark area) |
| CI/CD integration | Requires validation |
| JSON output | Requires validation |
| SARIF output | Not primary focus |
| HTML/human-readable reporting | Requires validation |
| Open-source inspectability | Not primary focus |
| Extensibility | Requires validation |
| Multi-provider breadth | Requires validation |
| Deployment/operational simplicity | Not primary focus |
| Security architecture transparency | Requires validation |
| Adversarial/security testing | Not established in this research baseline |
| Release/supply-chain engineering | Not established in this research baseline |
| Documentation/developer experience | Requires validation |

### 11.6 Maester

| Dimension | Value |
| --- | --- |
| Microsoft Entra depth | Relevant capability |
| NHI discovery/inventory | Not primary focus |
| Application registrations | Documented capability |
| Service principals | Documented capability |
| Managed identities | Requires validation |
| AI/agent identities | Not primary focus |
| Ownership/accountability | Requires validation |
| Credential metadata/posture | Requires validation |
| Permissions/exposure | Requires validation |
| Identity relationships/graph | Not primary focus |
| Deterministic assessment | Relevant capability |
| Explicit assessment completeness | Requires validation |
| PASS/FAIL/NOT_EVALUATED/NOT_APPLICABLE/ERROR semantics | Requires validation |
| Evidence/provenance | Requires validation |
| False-PASS resistance | Requires validation |
| Read-only assessment | Relevant capability |
| Attack-path analysis | Not primary focus |
| Real-time monitoring/threat detection | Not primary focus |
| Remediation | Not primary focus |
| Policy enforcement | Not primary focus |
| CI/CD integration | Requires validation |
| JSON output | Requires validation |
| SARIF output | Requires validation |
| HTML/human-readable reporting | Documented capability |
| Open-source inspectability | Documented capability (benchmark area) |
| Extensibility | Documented capability (customizable tests) |
| Multi-provider breadth | Not primary focus |
| Deployment/operational simplicity | Requires validation |
| Security architecture transparency | Requires validation |
| Adversarial/security testing | Not established in this research baseline |
| Release/supply-chain engineering | Not established in this research baseline |
| Documentation/developer experience | Requires validation |

### 11.7 Monkey365

| Dimension | Value |
| --- | --- |
| Microsoft Entra depth | Relevant capability |
| NHI discovery/inventory | Not primary focus |
| Application registrations | Documented capability |
| Service principals | Documented capability |
| Managed identities | Requires validation |
| AI/agent identities | Not primary focus |
| Ownership/accountability | Requires validation |
| Credential metadata/posture | Requires validation |
| Permissions/exposure | Requires validation |
| Identity relationships/graph | Not primary focus |
| Deterministic assessment | Relevant capability |
| Explicit assessment completeness | Requires validation |
| PASS/FAIL/NOT_EVALUATED/NOT_APPLICABLE/ERROR semantics | Requires validation |
| Evidence/provenance | Requires validation |
| False-PASS resistance | Requires validation |
| Read-only assessment | Relevant capability |
| Attack-path analysis | Not primary focus |
| Real-time monitoring/threat detection | Not primary focus |
| Remediation | Not primary focus |
| Policy enforcement | Not primary focus |
| CI/CD integration | Requires validation |
| JSON output | Requires validation |
| SARIF output | Requires validation |
| HTML/human-readable reporting | Documented capability |
| Open-source inspectability | Documented capability (benchmark area) |
| Extensibility | Relevant capability (collector/rules architecture) |
| Multi-provider breadth | Documented capability (M365, Azure, Entra) |
| Deployment/operational simplicity | Requires validation |
| Security architecture transparency | Requires validation |
| Adversarial/security testing | Not established in this research baseline |
| Release/supply-chain engineering | Not established in this research baseline |
| Documentation/developer experience | Requires validation |

### 11.8 BloodHound

| Dimension | Value |
| --- | --- |
| Microsoft Entra depth | Relevant capability (service principals) |
| NHI discovery/inventory | Not primary focus |
| Application registrations | Requires validation |
| Service principals | Documented capability |
| Managed identities | Requires validation |
| AI/agent identities | Not primary focus |
| Ownership/accountability | Requires validation |
| Credential metadata/posture | Not primary focus |
| Permissions/exposure | Relevant capability |
| Identity relationships/graph | Documented capability (benchmark area) |
| Deterministic assessment | Relevant capability |
| Explicit assessment completeness | Requires validation |
| PASS/FAIL/NOT_EVALUATED/NOT_APPLICABLE/ERROR semantics | Not primary focus |
| Evidence/provenance | Requires validation |
| False-PASS resistance | Not primary focus |
| Read-only assessment | Relevant capability |
| Attack-path analysis | Documented capability (benchmark area) |
| Real-time monitoring/threat detection | Not primary focus |
| Remediation | Not primary focus |
| Policy enforcement | Not primary focus |
| CI/CD integration | Requires validation |
| JSON output | Documented capability |
| SARIF output | Requires validation |
| HTML/human-readable reporting | Documented capability |
| Open-source inspectability | Documented capability (benchmark area) |
| Extensibility | Requires validation |
| Multi-provider breadth | Not primary focus |
| Deployment/operational simplicity | Requires validation |
| Security architecture transparency | Requires validation |
| Adversarial/security testing | Not established in this research baseline |
| Release/supply-chain engineering | Not established in this research baseline |
| Documentation/developer experience | Requires validation |

---

## 12. EntraNHI Intended Competitive Focus

The following table documents EntraNHI's intended competitive engineering dimensions. These are **design intentions**, not implemented capabilities. They require implementation and evidence before being claimed as competitive differentiators.

| Dimension | EntraNHI V1 Status | Notes |
| --- | --- | --- |
| Microsoft Entra depth | EntraNHI V1 intended | Deep specialization in Microsoft Entra identity semantics |
| NHI discovery/inventory | EntraNHI V1 intended | Collection of NHI inventory from documented Graph APIs |
| Application registrations | EntraNHI V1 intended | Assessment of application registration configurations |
| Service principals | EntraNHI V1 intended | Assessment of service principal configurations |
| Managed identities | EntraNHI V1 intended | Assessment where documented capabilities support it |
| AI/agent identities | EntraNHI V1 intended | Capability-aware support as documented Microsoft capabilities evolve |
| Ownership/accountability | EntraNHI V1 intended | Assessment of ownership and accountability data |
| Credential metadata/posture | EntraNHI V1 intended | Assessment from available metadata, never secret values |
| Permissions/exposure | EntraNHI V1 intended | Assessment of API permissions and roles |
| Identity relationships/graph | EntraNHI V1 intended | Normalized identity graph from observed facts |
| Deterministic assessment | EntraNHI V1 intended | Deterministic, reproducible evaluation |
| Explicit assessment completeness | EntraNHI V1 intended | Five explicit evaluation states |
| PASS/FAIL/NOT_EVALUATED/NOT_APPLICABLE/ERROR semantics | EntraNHI V1 intended | Exact five-state evaluation model |
| Evidence/provenance | EntraNHI V1 intended | Evidence and provenance attached to all findings |
| False-PASS resistance | EntraNHI V1 intended | Explicit handling of unavailable data |
| Read-only assessment | EntraNHI V1 intended | No state mutations by design |
| Attack-path analysis | EntraNHI V1 out of scope | May be future consideration |
| Real-time monitoring/threat detection | EntraNHI V1 out of scope | May be future consideration |
| Remediation | EntraNHI V1 out of scope | May be future consideration |
| Policy enforcement | EntraNHI V1 out of scope | May be future consideration |
| CI/CD integration | EntraNHI V1 intended | CLI and machine-readable output |
| JSON output | EntraNHI V1 intended | Structured JSON output |
| SARIF output | EntraNHI V1 intended | SARIF-format output |
| HTML/human-readable reporting | EntraNHI V1 intended | Human-readable report output |
| Open-source inspectability | EntraNHI V1 intended | Open-source publication |
| Extensibility | EntraNHI future consideration | Modular collectors and analyzers |
| Multi-provider breadth | EntraNHI V1 out of scope | May be future consideration |
| Deployment/operational simplicity | EntraNHI V1 intended | CLI-based operator-run assessment |
| Security architecture transparency | EntraNHI V1 intended | Transparent documentation of security constraints |
| Adversarial/security testing | EntraNHI V1 intended | Adversarial test cases per testing architecture |
| Release/supply-chain engineering | EntraNHI V1 intended | Versioning, provenance, reproducible builds |
| Documentation/developer experience | EntraNHI V1 intended | Architecture documentation sufficient for security review |

---

## 13. EntraNHI Primary Competitive Engineering Objective

EntraNHI intends to become an exceptionally rigorous open-source Microsoft Entra NHI and AI-agent security assessment platform.

The intended areas of differentiation to **validate later** (not current achievements) include:

- **Deep Microsoft Entra specialization** — focused exclusively on Microsoft Entra identity semantics rather than broad multi-provider coverage.
- **Deterministic rule evaluation** — every evaluation produces the same outcome from identical inputs, without AI or probabilistic inference.
- **Explicit assessment-completeness semantics** — five distinct evaluation states that honestly represent what was and was not evaluated.
- **Evidence and provenance** — every finding carries traceable evidence references.
- **Transparent rule behavior** — assessment logic is inspectable in source code.
- **Normalized identity relationships** — consistent domain model across heterogeneous NHI types.
- **Resistance to false-PASS outcomes** — missing data is never silently coerced into false confidence.
- **Read-only architecture** — no state mutations against the assessed tenant.
- **Open-source inspectability** — community review of security-critical evaluation code.
- **Adversarial/security testing** — adversarial test cases as part of quality assurance.
- **Capability-aware Agent ID support** — as documented Microsoft capabilities evolve.

**None of these are claimed as current competitive advantages.** They are intended engineering differentiators requiring implementation and evidence.

---

## 14. Deliberate V1 Non-Competition Areas

EntraNHI V1 explicitly does not attempt to match commercial platforms in the following areas. These are deliberate scope decisions, not claims that these capabilities lack value.

| Area | Reason |
| --- | --- |
| Number of SaaS/cloud integrations | V1 is focused exclusively on Microsoft Entra |
| Real-time threat detection | V1 is point-in-time offline assessment |
| Automatic remediation | V1 is read-only by design |
| Secrets-management/lifecycle platform functionality | V1 does not manage secrets |
| Runtime workload access enforcement | V1 does not enforce access policies |
| Broad multi-provider coverage | V1 is Microsoft Entra-only |
| Full attack-path analysis | V1 is posture assessment, not attack-path analysis |

---

## 15. Competitive Claims Policy

The following rules govern competitive claims in all EntraNHI documentation and communication:

1. **No overall rankings.** No product is declared a winner, loser, best, or worst.
2. **No superiority claims.** EntraNHI is not described as superior to any named competitor.
3. **No uniqueness claims.** Statements like "nobody else does this" are prohibited.
4. **No first/only claims.** EntraNHI does not claim to be the first or only product in any category.
5. **No design-as-implementation.** Design intentions are never presented as implemented capabilities.
6. **Evidence required.** Any competitive comparison must be supported by validated evidence.
7. **Source revalidation required.** All competitor capability claims from this research baseline require source revalidation before publication or major roadmap decisions.
8. **No unsupported inferences.** Missing data about a competitor does not imply the competitor lacks the capability.

---

## 16. Evidence / Validation Policy

This document is based on supplied high-level characterization. The following evidence rules apply:

1. **No invented citations.** No URLs, publication dates, or specific source references are invented.
2. **No invented dates.** The only date in this document is the research date: 2026-09-18.
3. **Research baseline marking.** Competitor capability statements are marked as supplied research baseline.
4. **Source revalidation required.** External claims require source revalidation before publication or major roadmap decisions.
5. **No conversion to specific claims.** Supplied high-level characterization is not converted into more specific technical claims.
6. **No inference from silence.** Missing information about a capability does not imply the capability does not exist.
7. **Controlled uncertainty values.** Unknown cells use controlled values (Requires validation, Not established in this research baseline) rather than guesses.

---

## 17. Market-Change Tracking

The following market changes are recorded in this research baseline:

| Event | Date | Context |
| --- | --- | --- |
| Oasis became part of Cyera | Not specified in research baseline | Stated factually; no speculation about acquisition strategy or future roadmap |
| Astrix became part of Cisco | 2026 | Stated factually; no speculation about future product integration |

Future market changes should be recorded here with factual, neutral context. No speculation about acquisition strategy, product integration, or roadmap implications.

---

## 18. Reassessment Cadence

This market landscape document should be revalidated:

1. **Before each major release** — to ensure competitive context remains current.
2. **Before publishing competitor comparisons** — all claims must be source-validated.
3. **Before major roadmap decisions** — competitive context should inform scope and prioritization.
4. **When Microsoft materially changes Entra or Agent ID capabilities** — may affect benchmark positioning.
5. **When benchmark products undergo major product or ownership changes** — acquisitions, major feature releases, or category shifts.
6. **When new significant competitors enter the NHI security space** — to maintain benchmark completeness.

---

## 19. Open Research Questions

The following questions remain unresolved in this research baseline and should be investigated during future reassessment:

1. **Oasis/Cyera integration status** — What is the current product direction and capability scope post-acquisition?
2. **Astrix/Cisco integration status** — What is the current product direction and capability scope post-acquisition?
3. **Entro Security scope** — What is the exact current scope of discovery, classification, and monitoring capabilities?
4. **Aembit Entra depth** — What is the exact current scope of Microsoft Entra integration and workload identity coverage?
5. **Maester test coverage** — What is the current scope of application/service-principal-related assessment tests?
6. **Monkey365 Entra coverage** — What is the current scope of Entra-specific application/enterprise-application assessment?
7. **BloodHound Entra features** — What is the current scope of Entra-specific service-principal graph features?
8. **Commercial NHI evaluation models** — Do Oasis/Cyera, Astrix, or Entro use explicit evaluation states or evidence/provenance models?
9. **Open-source competitive landscape** — Are there other significant open-source NHI assessment tools not covered in this baseline?
10. **Agent identity market evolution** — How is the AI-agent identity security market evolving as Microsoft Entra Agent ID matures?

---

## 20. Current Maturity Disclaimer

EntraNHI is in **early-stage design and development.** This market landscape document describes the competitive context for engineering decisions. It does not:

- Claim EntraNHI currently matches or exceeds any benchmark product's implemented capabilities.
- Present design intentions as competitive advantages.
- Provide validated, source-cited competitor capability claims (all claims require source revalidation).
- Replace independent market research for publication or major decisions.

The competitive benchmark framework is a tool for informed engineering decisions. Its value depends on periodic reassessment and source validation of the underlying research baseline.

---

## Appendix A: Matrix Value Reference

| Value | Definition |
| --- | --- |
| Documented capability | Capability documented in supplied research baseline |
| Relevant capability | Capability relevant to this dimension based on product category analysis |
| Benchmark area | Primary area this benchmark product is used to evaluate |
| Not primary focus | Not a primary focus of this product based on supplied characterization |
| EntraNHI V1 intended | EntraNHI V1 intends to address this dimension (design intention, not implemented) |
| EntraNHI future consideration | May be addressed in future EntraNHI versions |
| EntraNHI V1 out of scope | Explicitly not in EntraNHI V1 scope (deliberate scope decision) |
| Not established in this research baseline | Capability not established in current research; requires investigation |
| Requires validation | Requires source revalidation before publication or major roadmap decisions |

---

(End of file)
