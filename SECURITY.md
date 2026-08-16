# Security Policy

## Coordinated Vulnerability Disclosure (CVD)

The `coordinet-cs` security assessment framework is a specialized reconnaissance tool. We take security very seriously and are committed to responsible vulnerability management.

### Reporting Security Vulnerabilities

If you discover a security vulnerability, bug, operational bypass, or security-relevant issue in the `coordinet-cs` framework, please report it **privately and confidentially** to:

**Email:** voltsparx@gmail.com

### Disclosure Guidelines

1. **Do NOT** open a public GitHub issue for security vulnerabilities.
2. **Do NOT** post vulnerability details on social media, forums, or public channels.
3. **Email directly** with a detailed description of the vulnerability, including:
   - Affected component (e.g., WebServer.cs, SessionLogger, TemplateInjector)
   - Steps to reproduce
   - Potential security impact (information disclosure, privilege escalation, arbitrary code execution, etc.)
   - Proof-of-concept (if applicable)
   - Your recommended fix (if you have one)

4. **Allow reasonable time** for the maintainers to investigate and develop a patch before public disclosure (typically 7-30 days depending on severity).

### Response Timeline

- **Critical Vulnerabilities** (RCE, authentication bypass, data breach): Response within 24-48 hours
- **High Severity** (significant information disclosure, logic bypass): Response within 3-5 business days
- **Medium Severity** (limited impact, complex exploitation): Response within 7-14 days
- **Low Severity** (edge cases, defense-in-depth improvements): Response within 30 days

### Scope of Disclosures

We welcome reports on:
- Authentication & authorization bypass
- Arbitrary code execution (local or remote)
- Information disclosure / data leakage
- Denial of Service (DoS/DDoS vectors)
- Injection attacks (SQL, command, template)
- Cryptographic weakness
- Unsafe deserialization
- Path traversal & file system escape
- Default credentials or hardcoded secrets
- Privacy boundary violations

## Authorized Use Policy

### ⚠️ CRITICAL LEGAL NOTICE

This framework is designed **exclusively** for authorized defensive simulations, vulnerability assessments, and training scopes under explicit **Rules of Engagement (RoE)**.

**Use of this toolkit against unauthorized endpoints or systems outside a sandboxed testing architecture is strictly illegal and violates computer security laws:**

- **Computer Fraud and Abuse Act (CFAA)** - United States - 18 U.S.C. § 1030
- **Computer Misuse Act (CMA)** - United Kingdom - Computer Misuse Act 1990
- **Directive 2013/40/EU** - European Union - Criminal penalties for unauthorized access and interference
- **National Computer Security Regulations** - Other jurisdictions may have equivalent or stricter penalties

### Authorized Use Cases

This framework is **only** appropriate for:

✅ **Authorized Penetration Testing**
- Performed under written contract with explicit scope and Rules of Engagement
- With full authorization from system owner or delegated authority
- Within defined testing windows and target systems only

✅ **Security Research & Academic Study**
- Sandbox or isolated lab environments
- Proof-of-concept demonstrations in controlled settings
- Educational institutions with appropriate ethical oversight

✅ **Red Team Exercises**
- Authorized by organizational security leadership
- Confined to internal organizational systems or designated test range
- With documented scope, approval, and compliance oversight

✅ **Security Training & Certification Programs**
- Provided by accredited training organizations
- Using dedicated lab infrastructure or virtual ranges
- With explicit participant consent and defined boundaries

### Prohibited Uses

❌ **Against Any Unauthorized Systems or Endpoints**
❌ **Against Third-Party Infrastructure Without Explicit Permission**
❌ **Social Engineering or Deception of End Users**
❌ **Credential Harvesting or Identity Theft**
❌ **Corporate Espionage or Competitive Intelligence**
❌ **Disruption of Services or Data Destruction**
❌ **Use in Violation of Privacy Laws or Data Protection Regulations (GDPR, CCPA, etc.)**

### User Responsibility

By using this framework, you acknowledge:

1. You understand the legal implications of your actions
2. You have obtained written authorization from authorized personnel before any operational use
3. You are solely responsible for compliance with all applicable laws
4. The maintainers are **not responsible** for misuse of this framework
5. You will hold the maintainers harmless from any legal liability arising from your use

## Code Quality & Security Standards

All contributions to `coordinet-cs` must adhere to:

- **Safe Asynchronous C# Patterns**: All I/O operations use `async/await` with proper exception handling
- **Cross-Platform Path Isolation**: Use `System.IO.Path.Combine()` exclusively; no hardcoded path separators
- **Secure String Handling**: No embedding of API keys, credentials, or secrets in source code
- **SQL Injection Prevention**: All database queries use parameterized statements via Microsoft.Data.Sqlite
- **OWASP Top 10 Compliance**: Web routes validate and sanitize all user input
- **Minimal Dependency Surface**: Dependencies kept to essential libraries only
- **Comprehensive Logging**: All telemetry operations logged for forensic audit trails

## Reporting Non-Security Issues

For **non-security bugs**, feature requests, or general questions:
- Open a GitHub issue in the main repository
- Include reproduction steps, expected behavior, and actual behavior
- For crashes, include stack traces and environment details

## Acknowledgments

Security researchers and ethical professionals who responsibly disclose vulnerabilities help us improve the framework. Responsible disclosure enables us to develop patches and protect users.

We appreciate your cooperation in maintaining the integrity and security of this project.

---

**Last Updated:** 2026-08-17  
**Maintainer:** voltsparx (Niyor Kalita)  
**Contact:** voltsparx@gmail.com
