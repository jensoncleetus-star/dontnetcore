# _archive — reference material (nothing deleted)

Created 2026-06-12 when the folder was tidied after the migration completed.
**The live application is `..\QuickNetCore\` — nothing in here is needed to build or run it.**

| Folder | What it is | Why kept |
|---|---|---|
| `legacy-mvc5\` | The ORIGINAL ASP.NET MVC5 / .NET 4.5.2 source (incl. its `.git` history, `Web.config`, `dbscript\`, and `MIGRATION-ASSESSMENT.md`) | The migration reference — used to verify 1:1 parity; keep until well after go-live. Note: one credential-stealer file was removed during the 2026-06-10 security scan; otherwise as cloned from github.com/shiyaska1/quicknet2025. |
| `QuickSoftPilot\` | The throwaway feasibility pilot (clean-rebuild experiment, 2026-06-10) | Superseded by QuickNetCore; historical only. Contains a stray unrelated git clone (Jenson-Cleetus.git) the owner may want to relocate. |
| `quicknetcore-scratch\` | Test/sweep logs, run outputs, build logs and one-off scripts produced during the migration sessions | Diagnostic history of the migration (sweep results, razor-build error lists). Safe to delete whenever. |
| `quicknetcore-scratch\wwwroot-duplicates\` | Backup/duplicate JS files (`* - Copy.js`, `(orginal)`, `saleinvoiceold.js`, the `js - Copy\` folder) that shipped inside the legacy `Content\js` | Were never referenced by any view/bundle; archived instead of deleted. |

Safe-deletion order, if disk space is ever needed: `quicknetcore-scratch\` first,
then `QuickSoftPilot\`, and `legacy-mvc5\` only after production go-live is signed off.
