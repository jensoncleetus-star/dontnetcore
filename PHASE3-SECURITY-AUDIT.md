# Phase 3 — Security Audit (full-codebase pass)

**Date:** 2026-06-29 · **Scope:** the whole .NET 10 port (Controllers/, Areas/, Helpers/, Models/, Views/, Program.cs, config, wwwroot/uploads).
**Method:** four parallel auditor passes (SQL injection · authorization/IDOR · XSS · secrets/config/upload/crypto), each finding then spot-verified against source. Build-output / backup trees (`bin/`, `publish-verify/`, `.net core before cs5 Migration/`, `.security-backup-controllers/`) were **excluded** — they duplicate live code and are not deployed.

> Continues the `S`-series from PHASE1-AUDIT.md. New findings are **S8+**. The Phase-1 "resolved" items were re-verified (see §0).

---

## 0. Phase-1 items — re-verified status
| # | Phase-1 claim | Verified now |
|---|---|---|
| S1 | Password policy raised to ≥8 + digit | ✅ Confirmed (`Program.cs:42-46`) |
| S2 | Anti-forgery 100% (global `AutoValidateAntiforgeryToken`) | ✅ Confirmed (`Program.cs:97`) |
| S3 | Security headers added | ✅ Confirmed — X-Content-Type-Options, X-Frame-Options, Referrer-Policy, Permissions-Policy. **CSP strengthened 2026-06-29** to `frame-ancestors 'self'; object-src 'none'; base-uri 'self'; form-action 'self'` (verified-safe: 0 `<object>`/`<base>`/cross-origin-form usage; live header confirmed). Full `script-src` CSP still deferred to the UI phase (legacy inline scripts + CDN allow-list). |
| S7 | Login lockout 5/5min | ✅ Confirmed (`Program.cs:51-53`) |
| S6 | "Raw SQL parameterized, **0** string-concat sites" | ⚠️ **Letter false, spirit true** — see S8-note below. |

---

## 1. Findings by severity

### 🔴 CRITICAL

| # | Finding | Evidence | Fix |
|---|---|---|---|
| **S8** ✅FIXED 2026-06-29 | **Authorization was open-by-default.** `BaseController` had no `[Authorize]`; `Program.cs` had no `FallbackPolicy`. Auth was opt-in per action; only **3** controllers had a class-level guard. Any action without an explicit `[QkAuthorize]` was reachable **unauthenticated**. | Verified: only 3 class-level `[QkAuthorize]`; `QkAuthorize` didn't honor `[AllowAnonymous]`; public endpoints (login/register/forgot/OTP-downloads/error/installation/loginapp) already carry `[AllowAnonymous]`. | **Done (4 parts):** (1) added a global `FallbackPolicy` requiring an authenticated user (`Program.cs`); (2) taught `QkAuthorize` to honor `[AllowAnonymous]` so the 3 class-level controllers don't block their own anon actions; (3) added `[AllowAnonymous]` to `ErrorController`; (4) added `.AllowAnonymous()` to the dev `/dev/*` minimal-APIs. **Gated** behind `Security:RequireAuthByDefault` (default `true`) as an emergency config rollback. ⚠️**MUST smoke-test after restart** — see checklist below. |
| **S9** ✅FIXED 2026-06-29 | **Unauthenticated privilege escalation** in `UsersController` — `copypermissionAsync` (`:1426`, clones a user's role-set onto an arbitrary target), `assignpermission`/`removepermission` (GET+POST) carried no authorize attribute. **Correction to the auditor's first pass:** `Edit` (`:2029/2121`), `Ban`/`UnBan` (`:2275/2320`), `DeleteConfirmed` (`:2366`) were **already guarded** with `[QkAuthorize(Roles="Dev,Edit User"/"Dev,Delete User")]` — not vulnerable. | Verified each attribute directly. | **Done:** added `[QkAuthorize(Roles="Dev,Edit User")]` (the same guard the sibling actions use) to `copypermissionAsync`, `assignpermission` ×2, `removepermission` ×2. |
| **S10** ✅FIXED 2026-06-29 | **Unauthenticated approval bypass** — the approval **write** `EditStatus(ApprovalUpdate App, long id)` in `PaymentApprovalController` (`:2516`) and `ReceiptApprovalController` (`:2683`) was `[HttpPost]`-only (the commented role guards were on the *list/read* actions, not the write). | Verified both write actions had no guard. | **Done:** added bare `[QkAuthorize]` (authentication-only) to both `EditStatus` POSTs — kills the unauthenticated bypass with no role-mismatch lockout risk. Role-level hardening (restore `Roles="Dev, Approvals"`) left as a tested follow-up. |
| **S11** ⚠️PARTIAL 2026-06-29 | **Hardcoded live secret** — legacy **FCM server key** + a device token committed in source. | `UsersController.cs:1226` `serverKey = "AAAASPbOfEM:APA91bH…"`. Verified. | **Code done:** key now read from `Fcm:ServerKey`/`Fcm:SenderId` config (env `Fcm__ServerKey`) with the literal as a temporary fallback so the live feature keeps working. **STILL REQUIRED (manual):** rotate the key in the Firebase console — it is compromised in git history — set the new value via env, then delete the fallback literal. |

### 🟠 HIGH

| # | Finding | Evidence | Fix |
|---|---|---|---|
| **S12** ✅FIXED 2026-06-29 | **IDOR / broken object-level auth.** By-id voucher fetches neutralized their ownership check with a hardcoded `userpermission = true`, returning **any** record by `id`. Sites: `PaymentController.Edit` (`:1015`) + `Editprint` (`:796`); **`ReceiptController` `:609` + `:809`** — even worse: `var userpermission = true; User.IsInRole("All Receipt Entry");` (the role call's result was **discarded**, dead code). | Verified all 4 sites; `"All Payment Entry"`/`"All Receipt Entry"` are real roles used across the app. No other discarded-`IsInRole` statements exist (grep clean). | **Done:** restored `User.IsInRole("All Payment Entry")` (Payment ×2) and `User.IsInRole("All Receipt Entry")` (Receipt ×2). Role-holders unchanged; others scoped to their own. **Verify** staff hold the roles. |
| **S21** ✅FIXED 2026-06-29 | **List/grid visibility IDOR** — `userpermission`/`allentry` hardcoded `true` feeding a `\|\| ownership` grid filter. | Resolved by checking each flag's usage + same-file siblings. | **Done:** `VehicleUpdatesController:40` → `User.IsInRole("All vehicle")` (matches its own `:206`); `LandlordsController:79` → `User.IsInRole("All Landlords")` (matches `:649`). Role-holders unchanged; non-holders now correctly scoped. **Not bugs (left alone):** `CustomerController:948` and `MRNoteController:308` are **dead variables** — declared but never used in their method (the scoped queries live in sibling methods that already have the correct `IsInRole`). |
| **S10 (role hardening)** ✅DONE 2026-06-29 | Approval read/write actions only auth-guarded after the first pass. | `"Dev, Approvals"` confirmed **actively enforced** elsewhere (real, assigned role). | **Done:** restored `[QkAuthorize(Roles="Dev, Approvals")]` on ReceiptApproval `ApprovalList`/`GetReceiptApproval`, PaymentApproval `GetPaymentApproval`, and **upgraded both `EditStatus` write paths** from bare auth to this role. **Left auth-only (ACTIVE:0 roles — enabling would lock out a 0-member role):** the `"View Payment"`/`"View Receipt"`/`"Petty Cash Payment Approvals"` commented guards — create+assign those roles first, then I'll enable. ⚠️Confirm approvers hold `"Dev, Approvals"`. |
| **S13** ✅FIXED 2026-06-29 | **Stored XSS** in **47 transaction Details views** (`@Html.Raw(Model.Remarks/Note/TermsCondition/…)`) + the **Leads grid** JS sinks. All Remarks/Notes/Terms fields confirmed plain `@Html.TextAreaFor`. | 47 views + 5 Leads views. | **Done (both):** (1) swept all 47 `@Html.Raw(Model.X)` → `@Model.X`; (2) added a shared `qkEsc()` HTML-escape helper to `_QuickLayout.cshtml`, escaped the Leads `Index.cshtml` link-builders (LeadName/CustomerName/Mobile cols), and converted the 5 `$("#remarkMain").html(...)` modal sinks → `.text()` across Index/MyLeads/modallist/leadreport/Index_old. *(Views are runtime-compiled — smoke-test the Leads grid after restart.)* |
| **S14** ✅FIXED 2026-06-29 (serving layer) | **Unrestricted upload extension → stored XSS / malware hosting.** ~67 upload handlers apply **no extension/MIME allowlist**, and `wwwroot/uploads/` is served by `UseStaticFiles`. `.html`/`.svg`/`.js` get a same-origin URL ⇒ stored XSS. | Verified: two `.cshtml` files are git-tracked under `wwwroot/uploads/`. | **Done (both layers):** (1) `Program.cs` middleware **404s** script/markup extensions under `/uploads`; (2) the central `LegacyExtensions.SaveAs` choke point now **throws** on a blocklist of executable/script/markup extensions — covers all ~67 upload sites at write time. Images/pdf/office/csv unaffected. **Follow-up (your call):** delete the two committed `.cshtml` files under `wwwroot/uploads/` (already neutralized by the serving block — they're tracked user data, so left in place). |
| **S15** | **Plaintext credential storage/exposure** — the customer "password manager" stores third-party passwords and renders them in cleartext into list JSON. | `CustomerController.cs:444` `Remarks = "… Password : " + a.Password + …`. Verified. | Encrypt at rest; reveal only on explicit action; never inline into list payloads. |

### 🟡 MEDIUM

| # | Finding | Evidence | Fix |
|---|---|---|---|
| **S16** ✅FIXED 2026-06-29 | Auth & session cookies not explicitly hardened. | `Program.cs` cookie/session config | **Done:** `HttpOnly` + `SameSite=Lax` on both the auth cookie and the session cookie; `SecurePolicy=Always` gated on `Security:RequireHttps` (so plain-HTTP pilots keep working; flips automatically at TLS cutover). |
| **S17** ✅FIXED 2026-06-29 | **SSRF** — `ResolveShortUrl(shortUrl)` fetched an arbitrary user-supplied URL (Google-Maps link resolution) → server could be coerced to hit `169.254.169.254`/intranet. | `Models/Common.cs:66`, `Controllers/ProTaskController.cs:2612` | **Done (both copies):** validate scheme is http/https and host is a Google-Maps domain (`*.google.com`/`goo.gl`) before any fetch; otherwise return the input unmodified (no request). Blocks pointing the fetch at internal hosts. |
| **S18** ✅FIXED 2026-06-29 | SMS-gateway username/password sent in **URL query string over GET**, params concatenated **unescaped** (param-injection). | `Models/SendMail.cs:319-323`, `UsersController.cs:1192-1196` | **Done:** every value now wrapped in `Uri.EscapeDataString(x ?? "")`. (Switching GET→POST and removing creds-from-URL remains a deeper follow-up.) |
| **S19** | Dev fallback connection string hard-coded (server/DB name leak). Prod path correctly **throws** if unset — not a fallback risk, just disclosure. | `Program.cs:29` | Move dev string to user-secrets / `appsettings.Development.json`. |
| **S20** | `/dev/*` endpoints (`/dev/setpw` resets **any** password with no auth) are correctly gated to `IsDevelopment()` — residual risk is **environment misconfiguration** (prod accidentally set to Development ⇒ instant takeover). | `Program.cs:236-327` | Delete before deploy (comments already say "Remove before deploy") and/or `#if DEBUG` + startup assert env≠Development. |

### 🟢 LOW / INFO
- **S6 correction:** 11 string-concat-into-SQL sites exist (`ItemController.cs:6222/6301/6304`, `SalesReportController.cs:8849+`, `PurchaseReportController.cs` MonthWise) — Phase-1's scanner missed them. **None exploitable**: every concatenated value is `int`/`long` parsed by model binding. Recommend parameterizing anyway + fixing the scanner regex.
- Weak crypto in `Models/Security.cs:12-61` — static "Ivan Medvedev" salt + deterministic IV (low-value data; not the auth path, which uses Identity PBKDF2). `System.Random` used for non-security letters only.
- ~15 `Redirect(Request.GetUrlReferrer())` sites — low-risk (attacker can't set victim's Referer); will NRE if Referer absent. Add `Url.IsLocalUrl` guard.
- `Encrypt=False;TrustServerCertificate=True` — accepted pilot risk; flip at go-live.

**Good news (verified clean):** login `returnUrl` open-redirect is properly guarded (`Url.IsLocalUrl`, `UsersController.cs:823`); **no** unsafe deserialization (`TypeNameHandling`/`BinaryFormatter`/`XmlSerializer` — zero matches); `AjaxErrorFilter` does **not** leak stack traces to clients (stderr only); path traversal mitigated via `Path.GetFileName`; secrets externalized in config (only the FCM key S11 is in source).

---

## 2. Priority remediation order
1. **S8** — add the fallback authorization policy (one change; closes the unauthenticated surface that makes S9/S10/S12 remotely exploitable).
2. **S11** — rotate the FCM key.
3. **S9 / S10** — restore the user-management & approval guards (defense-in-depth even after S8).
4. **S14** — upload extension allowlist + move uploads off the web root.
5. **S13** — encode/sanitize the Remarks/Notes/Terms render path (one mechanical sweep across ~47 views once textarea-vs-richtext is settled per field).
6. **S15, S17, S18, S16** — credential storage, SSRF allowlist, SMS param escaping, cookie hardening (the last at HTTPS cutover).

## 3. The one-line root fix for S8
```csharp
builder.Services.AddAuthorization(o =>
    o.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
```
Then confirm the ~44 `[AllowAnonymous]` actions are only true public endpoints (Login, ForgotPassword, register, error, mobile `loginapp`). Per-action `[QkAuthorize(Roles=…)]` then layers authorization on top.

---
*No source files were modified by this audit.*
