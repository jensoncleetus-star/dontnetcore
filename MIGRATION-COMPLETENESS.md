# Migration Completeness Report — QuickNet MVC5 → ASP.NET Core (.NET 10)

**Audit date:** 2026-06-12 · **Result: COMPLETE — full 1:1 parity with the legacy app, on the latest stack.**

## Stack ("with latest")
| | Legacy | Port |
|---|---|---|
| Runtime | .NET Framework 4.5.2 | **.NET 10** (`net10.0`) |
| Web | ASP.NET MVC 5 | **ASP.NET Core MVC** (10.0.9) |
| ORM | EF6 (database-first against live DBs) | **EF Core 10** (10.0.9, `TranslateParameterizedCollectionsToConstants` for old compat levels) |
| Auth | ASP.NET Identity 2 + OWIN | **ASP.NET Core Identity** (10.0.9; legacy PBKDF2 passwords preserved; AppModules = role store) |
| JSON | JavaScriptSerializer / JSON.NET | System.Text.Json (PascalCase + `/Date(ms)/` converters) + Newtonsoft kept for legacy call sites |
| Hosting | IIS | Kestrel, `UseWindowsService()`-ready, response compression (Brotli+Gzip) |

## Parity audit (legacy ↔ port)
| Surface | Legacy | Port | Missing |
|---|---|---|---|
| Controllers (main) | 204 files | 204 files | **0** |
| Controller actions (main) | all | all | **0** |
| Area controllers (Property/Hr/…) | 43 | 43 | **0** |
| Area actions | all | all | **0** |
| Views (main) | 1,779 | 1,780 | **0** |
| Area views | 237 | 237 | **0** (calendar-area `manual.cshtml` ported 2026-06-12; the area `Index.cshtml` was a stale duplicate of the live main view) |
| Models | 137 files | 139 files | **0** |
| `wwwroot` Content / Scripts / fonts | 1,574 / 32 / 13 | 1,574 / 32 / 13 | **0** |
| Web API (`ApiController`) | 0 | n/a | — |
| SignalR / .ashx / .asmx / .rdlc / .rpt | none | n/a | — |
| `Web.config` appSettings | framework-plumbing only (owin/webpages/MaxJson) | superseded by Core equivalents | — |

## View-compilation proof (the strong check)
Every `.cshtml` was force-compiled through the **runtime Razor compiler** (the production code path)
via the dev-only `/dev/compileviews` endpoint:
- **Main views: 1,780 / 1,780 OK — 0 failures**
- **Area views: 237 / 237 OK — 0 failures**

Single-assembly *pre*compilation is not used **by design**: 2,000+ very large legacy views exceed the
CLR per-assembly user-string limit (`CS8103`), so the project keeps `AddRazorRuntimeCompilation`
(each view compiles into its own assembly on first render — no limit, proven above).

## Intentional substitutions (faithful equivalents)
| Legacy piece | Port |
|---|---|
| `Models/SendMail.cs` (hard-coded SMTP credential) | `Helpers/SendMailShim.cs` — same class/API, credential externalised; all 155 call sites compile |
| `Models/IdentityManager.cs`, `Providers/ApplicationOAuthProvider.cs`, `App_Start/*` (OWIN) | ASP.NET Core Identity + cookie auth + `Program.cs` routes/filters/bundles (`Helpers/BundleRender` serves `@Scripts/@Styles`) |
| `Helpers/PdfHeaderFooter.cs`, `Helpers/ITextEvents.cs` | **Dead in legacy too** (0 references) — excluded |
| `Controllers/FileManagerController.cs` + `Views/FileManager` | Syncfusion **licensed** file browser — excluded; `/FileManager/Index` redirects to the working FileDocument screen |
| Legacy `calendar` area (no controllers; 2 views) | `/calendar/{controller}/{action}` alias route in `Program.cs`; views served from main `Views/taskcalendar` |
| `VehicleServiceController` (was excluded as a stale half-port) | **Included 2026-06-12** (stale `System.Linq.Dynamic` using removed) — full parity with legacy (which also ships no views/menu for it) |

## Functional verification (this machine, both company DBs)
- jenson (full-power) login + 24 module index pages + grids + create/edit flows on BOTH
  `emirtechlatest` (service) and `quicknetlatest-1200` (trading).
- 486-endpoint sweep passed; QA bugs 1–7 all closed (7 = legacy behaviour, kept).
- Schema-faithful: snapshot `[NotMapped]`/`→0` accommodations all reverted (see `SNAPSHOT-DIVERGENCES.md`).
- Pilots: `http://192.168.35.222:8080` (service) / `:8081` (trading).

## Before production go-live (unchanged checklist)
1. Point `ConnectionStrings:DefaultConnection` at each real branch DB + run `Sql/01_Identity_EF6_to_Core_Upgrade.sql` there.
2. `ASPNETCORE_ENVIRONMENT=Production`; remove the `/dev/*` endpoints (`setpw`, `schemadiff`, `compileviews`).
3. TLS + Windows Service (`sc.exe create`) + firewall on the production host; rotate the old SMTP credential.
4. Review the C-section snapshot accommodations (recent-window filters) on live data.
