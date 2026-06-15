# QuickSoft ERP — Modernization & Optimization Program

**Owner requirements:** 2026-06-12 (10-section brief) · **Prime directive:** same look/workflow, modern inside,
production data untouchable. Each phase is completed → tested → **owner-approved** before the next starts.

## Where we already stand (the .NET 10 migration delivered a head start)

| Requirement section | Status |
|---|---|
| §5 Technology Modernization (backend) | ✅ **DONE** — .NET 10 + ASP.NET Core MVC + EF Core 10 + Core Identity (latest everything); behaviour 1:1 verified (2,017 views, 486 grids, CRUD/PDF/approvals) |
| §8 Database compatibility | ✅ **PROVEN** — runs on the existing DBs unchanged (tested on 2 real company copies); the only change is the additive, idempotent Identity script; legacy + new can share one DB (rollback built in) |
| §9 Deployment & transition | ✅ **KIT READY** — self-contained bundle + Windows-service installer + runbook incl. backup/rollback; parallel-run transition = near-zero downtime |
| §2 Audit (technical layer) | �half — migration-era sweeps covered endpoints/views/schema; Phase 1 below formalizes + extends it |
| §1 / §6 UI preservation + polish | Structure preserved by construction; beautification = Phase 4 |

## Phases (mapped to this codebase)

### Phase 1 — Full System Audit *(IN PROGRESS — see PHASE1-AUDIT.md)*
- Codebase quality metrics (giant files, dead code, duplication) ✅ baseline captured
- Performance hotspot inventory (slow SPs, heavy queries, caching gaps) ✅ initial list
- Security review (authn/z, headers, anti-forgery coverage, credentials, TLS) ✅ initial findings
- DB review (compat level, indexes, statistics) + per-module consistency review
- **Gate:** owner reviews PHASE1-AUDIT.md findings + approves the Phase 2/3 backlog.

### Phase 2 — Core Bug Fixing & Business-Logic Validation
- Fix all confirmed bugs from the audit + UAT (the 2026-06 QA round already fixed 7).
- Calculation validation = **golden-output comparison**: same DB copy → same screen/report in
  legacy vs new → outputs must match; differences are triaged with the owner/accountant
  (some will be *pre-existing* 2018-era bugs — owner decides keep-or-correct, per report).
- Priority order: Accounts/VAT → Inventory costing (AVCO) → Sales/Purchase totals → HR/payroll.

### Phase 3 — Backend Refactoring, DB & Security Hardening
- Refactor the top-10 giant files (Common.cs 15.8K lines, SalesReport 14.2K, …) into services —
  *behaviour-frozen* (golden tests from Phase 2 guard every refactor).
- Remove dead code (37K commented-out lines), duplicate classes, unused actions.
- DB: index tuning from live query stats, statistics refresh, archive strategy for big tables.
- Security: password policy, anti-forgery coverage (273/1,236 POSTs today), security headers,
  HTTPS/HSTS, credential rotation, raw-SQL parameterization re-verify (471 call sites).
- Performance: cache lookups (response/memory cache), server-side paging on heavy grids,
  SP_AVCOMethod optimization (8.6 min → target seconds), static-asset cache headers.

### Phase 4 — UI/UX Enhancement (structure unchanged)
- One shared CSS theme layer over the existing Bootstrap-3 markup: typography, spacing,
  colors, buttons, cards, tables, toasts — *no layout/menu/workflow changes*.
- Modern login page (per brief) — same fields/flow, new visual.
- Responsiveness pass at 360/768/1024/1440/1920 on the 20 most-used screens.
- Icons + empty-state/loading polish on DataTables screens.

### Phase 5 — Validation
- Migration-script dry-runs on fresh production copies (per branch) + schemadiff = 0.
- Full regression: the 486-grid sweep + CRUD + PDF + approvals, BOTH companies.
- Performance test: top-20 screens timed before/after (report in ms).
- Owner UAT with sign-off checklist (UAT-CHECKLIST.xlsx).

### Phase 6 — Production Deployment & Support
- Per-branch go-live via DEPLOY-RUNBOOK.md (backup → script → service → verify; rollback ready).
- Parallel-run window with the legacy app on the same DB until sign-off.
- Monitoring: health endpoint + Windows Event Log + slow-query log review weekly.
- 30-day hypercare: every reported issue fixed + regression-tested.

## Ground rules (from the brief, enforced throughout)
1. No layout/navigation/workflow changes — beautification only (Phase 4).
2. Production DBs are sacred: additive, scripted, copy-tested migrations only; backup+rollback before anything.
3. Every fix/refactor is verified against the golden outputs before merge.
4. Phase gates: nothing proceeds without owner approval of the previous phase's deliverable.
