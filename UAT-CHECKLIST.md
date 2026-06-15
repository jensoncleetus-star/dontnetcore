# QuickNet .NET 10 Port — Owner UAT Checklist

**Pilot URL:** `http://localhost:8080` (on the dev box) or `http://192.168.35.222:8080` (LAN)
**Login as:** `jenson` (full roles — sees ALL data; other logins are role-scoped and will look "empty")
**DB:** `emirtechlatest` (a safe COPY — type freely, nothing here touches production)

**How to use:** work top-to-bottom. For each row, do the action, check the result, and mark
**P** (pass) / **F** (fail) / **?** (unsure) in the box. For any **F**, jot the screen + what you
expected vs what you saw — that's all I need to fix it.

> 🎯 The goal of UAT is to confirm the **numbers, layout, and workflow are BUSINESS-correct** — things
> only you can judge. The app has already passed technical testing (screens load, data shows, no crashes).

---

## 0. Quick smoke test (~15 min) — do this first
| ✔ | Action | Expected |
|---|--------|----------|
| ☐ | Open the URL, log in as `jenson` | Dashboard/home loads, menus visible |
| ☐ | Open **Customers** list | Your real customers appear |
| ☐ | Open **Items** list | Your real items (cameras/DVR/NVR/cable…) appear |
| ☐ | Open **Quotation** list | Existing quotations appear |
| ☐ | Open any one quotation (View/Edit) | Header + line items + totals render correctly |
| ☐ | Open **Stock Report** | Stock rows appear with sensible quantities |
| ☐ | Log out / log back in | Works cleanly |

If the smoke test is clean, continue to the deep UAT below.

---

## 1. Sales cycle (the money path) — MOST IMPORTANT
| ✔ | Action | Verify |
|---|--------|--------|
| ☐ | Create a **new Lead** (a prospective CCTV customer) | Saves; appears in Leads list |
| ☐ | Create a **new Quotation** for a real customer with a few items (camera, DVR, cable, installation) | Line totals, sub-total, **VAT (5%)**, grand total all correct |
| ☐ | Apply a **discount** on the quotation | Totals + VAT recompute correctly |
| ☐ | **Print** the quotation (Download PDF) | PDF opens; company header, customer, items, totals, **TRN** all correct |
| ☐ | Convert/create a **Sales Order** from the quote | Carries items + prices correctly |
| ☐ | Create a **Delivery Note** | Reduces stock; correct items/qty |
| ☐ | Create the **Sales Invoice** (Credit Sale / Cash Sale) | **VAT + totals correct**; structured invoice data |
| ☐ | Print the **Invoice** PDF | Layout + TRN + amounts correct |
| ☐ | Record a **Receipt** (customer payment) against the invoice | Outstanding/balance updates correctly |
| ☐ | Re-open each saved record and **Edit** it | Changes save and persist |

## 2. Purchase cycle
| ✔ | Action | Verify |
|---|--------|--------|
| ☐ | Create a **Purchase Requisition** → **RFQ** → **Purchase Order** | Each carries data correctly |
| ☐ | Print the **PO** PDF | Supplier, items, totals correct |
| ☐ | Receive stock (**GRN**) | Stock increases correctly |
| ☐ | Enter the **Purchase Invoice** (Purchase Entry) | VAT/cost correct |
| ☐ | Record a **Payment** to the supplier | Supplier balance updates |

## 3. Inventory / Stock
| ✔ | Action | Verify |
|---|--------|--------|
| ☐ | **Stock Report** | On-hand quantities match reality |
| ☐ | **Stock valuation** (value report) | Totals look right (note: the all-items value report is slow ~8 min — that's expected) |
| ☐ | **Stock movement** for one item | In/out history correct |
| ☐ | **Stock Transfer** between locations | Moves stock correctly |

## 4. AMC (Annual Maintenance Contracts) — key CCTV revenue
| ✔ | Action | Verify |
|---|--------|--------|
| ☐ | Open **AMC** list | Existing contracts appear |
| ☐ | Open one AMC contract (View/Edit) | Customer, period, items, value correct |
| ☐ | Create a **new AMC** contract | Saves correctly |
| ☐ | Check **Periodic Maintenance** schedule | Visits/dates correct |
| ☐ | AMC **renewal** | Works |

## 5. Jobs / Installation (ProTask / Job Card)
| ✔ | Action | Verify |
|---|--------|--------|
| ☐ | Open **ProTask** (task list) | Recent jobs show (note: shows the 300 most-recently-updated) |
| ☐ | Create a **Job Card** for an installation | Saves; assign technician |
| ☐ | Add a **remark** / update status on a task | Saves |
| ☐ | Assign team members to a task | Saves |

## 6. CRM
| ✔ | Action | Verify |
|---|--------|--------|
| ☐ | Customer list + open a customer | Details, history correct |
| ☐ | Leads list + follow-up | Saves |
| ☐ | Global search (customer / lead / item) | Finds the right records |

## 7. Finance / Accounting
| ✔ | Action | Verify |
|---|--------|--------|
| ☐ | **Journal Voucher** entry | Debits = credits; saves |
| ☐ | **Trial Balance** | Balances |
| ☐ | **Profit & Loss** | Figures sensible |
| ☐ | **Balance Sheet** | Balances |
| ☐ | **VAT / Tax report** | Output/input VAT correct for a known period |
| ☐ | **Account Ledger** for one account | Transactions + running balance correct |

## 8. HR / Payroll (only if you use it)
| ✔ | Action | Verify |
|---|--------|--------|
| ☐ | Employee list | Correct |
| ☐ | Attendance entry | Saves |
| ☐ | Salary structure / Payhead | Correct |
| ☐ | Payroll voucher / settlement | Figures correct |

## 9. Master data (spot-check)
| ✔ | Action | Verify |
|---|--------|--------|
| ☐ | Create + edit + (test) delete an **Item** | Works end-to-end |
| ☐ | Create + edit a **Customer** | Works |
| ☐ | Create + edit a **Supplier** | Works |
| ☐ | Users & roles screen | Correct |

## 10. Cross-check against the legacy (highest confidence)
| ✔ | Action | Verify |
|---|--------|--------|
| ☐ | Pick 3–5 **key reports** you know well (e.g. this month's sales, top customer balance, stock value) | Numbers **match the old system** for the same date range |
| ☐ | Pick 2–3 **specific invoices/quotes** | Totals + VAT **identical** to the old system |

---

## ⚠️ Known limitations — please DON'T report these (already understood)
These are because the test DB (`emirtechlatest`) is an **older copy** than your live system — they
resolve automatically when we cut over to the real branch DB (all listed in `SNAPSHOT-DIVERGENCES.md`):
- A few **empty screens** because the copy genuinely has no data there (or a date filter) — not a bug.
- **Real-estate / Tenancy** module + **restaurant-POS** screens — missing columns/tables in this copy (and not used by the CCTV business).
- Some report **Print** layouts may need a multi-page header/footer tweak (PDFs generate fine).
- A handful of report screens reach via a button, not a direct `/Index` URL (legacy behaviour).
- `BOM / Production` cost fields, `delivery charge` on POS show **0** here (those columns aren't in this copy — real values return on the live DB).

## How to report an issue (anything marked F)
Just give me: **(1)** the screen/menu path, **(2)** what you did, **(3)** what you expected,
**(4)** what actually happened (a screenshot is perfect). I'll fix it and you re-test.

---
*Pilot is the .NET 10 faithful port of QuickNet. Technical testing complete: list/report screens,
open-record, full CRUD, Print (PDF), and Approve all verified. This UAT confirms business correctness.*
