using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickSoft.Models;
using QuickSoft.ViewModel;

namespace QuickSoft.Controllers
{
    // BOS — Custom Invoice Template Designer (drag-and-drop). NEW feature; additive.
    // Manages user-designed templates stored in the InvoiceTemplate table (JSON layout).
    [RedirectingAction]
    public class InvoiceTemplateController : BaseController
    {
        readonly ApplicationDbContext db;
        readonly Common com;
        readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment env;
        public InvoiceTemplateController(Microsoft.AspNetCore.Hosting.IWebHostEnvironment environment)
        {
            db = new ApplicationDbContext();
            com = new Common();
            env = environment;
        }

        // The document types the custom designer supports. Sale + Quotation share Common.SaleData;
        // Receipt/Payment/Cheque are wired per-type (their accurate data binders are added incrementally).
        static readonly string[] DocTypes = { "Sale", "Quotation", "Estimate", "ProForma", "SalesOrder", "DeliveryNote", "CreditNote", "PurchaseOrder", "PurchaseEntry", "Receipt", "Payment", "Cheque", "MaterialRequisition", "TenancyContract" };
        static string NormDoc(string d) => DocTypes.Contains(d, StringComparer.OrdinalIgnoreCase)
            ? DocTypes.First(x => x.Equals(d, StringComparison.OrdinalIgnoreCase)) : "Sale";

        // GET: /InvoiceTemplate/Hub  — central "Custom Design" settings page: every document type's
        // template designer in one place (reached from Settings/Company menu).
        [QkAuthorize(Roles = "Dev,Invoice Template,Custom Design,Sales Invoice,Credit Sale")]
        public ActionResult Hub()
        {
            var counts = db.InvoiceTemplates.Where(t => t.Status == Status.active)
                           .GroupBy(t => t.DocType)
                           .Select(g => new { DocType = g.Key, Count = g.Count() }).ToList();
            ViewBag.Counts = counts.ToDictionary(x => x.DocType ?? "Sale", x => x.Count);
            return View();
        }

        // GET: /InvoiceTemplate?docType=Sale  — management list for ONE document type
        [QkAuthorize(Roles = "Dev,Invoice Template,Custom Design,Sales Invoice,Credit Sale")]
        public ActionResult Index(string docType = "Sale")
        {
            docType = NormDoc(docType);
            var list = db.InvoiceTemplates
                         .Where(t => t.Status == Status.active && t.DocType == docType)
                         .OrderByDescending(t => t.IsDefault).ThenBy(t => t.Name)
                         .ToList();
            ViewBag.DocType = docType;
            return View(list);
        }

        // GET: /InvoiceTemplate/Designer/5  — drag-and-drop designer (no id = new template for ?docType=)
        [QkAuthorize(Roles = "Dev,Invoice Template,Sales Invoice,Credit Sale")]
        public ActionResult Designer(int? id, string docType = "Sale")
        {
            InvoiceTemplate t = null;
            if (id.HasValue) t = db.InvoiceTemplates.FirstOrDefault(x => x.Id == id.Value && x.Status == Status.active);
            if (t == null)
            {
                // start a new template from a sensible blank, for the requested document type
                t = new InvoiceTemplate { Name = "New Template", DocType = NormDoc(docType), DesignJson = "{\"paper\":\"A4\",\"orientation\":\"portrait\",\"elements\":[]}" };
            }
            ViewBag.DocType = t.DocType;
            return View(t);
        }

        // POST: /InvoiceTemplate/Save  — upsert a designed template (AJAX). Antiforgery via the
        // global header hook (_QuickLayout attaches RequestVerificationToken to jQuery AJAX).
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Invoice Template,Sales Invoice,Credit Sale")]
        public ActionResult Save(int id, string name, string paperSize, string orientation, string docType, string designJson)
        {
            if (string.IsNullOrWhiteSpace(name)) return Json(new { success = false, message = "Template name is required." });

            InvoiceTemplate t = id > 0 ? db.InvoiceTemplates.FirstOrDefault(x => x.Id == id) : null;
            bool isNew = t == null;
            if (isNew) { t = new InvoiceTemplate(); db.InvoiceTemplates.Add(t); }

            t.Name = name.Trim();
            t.PaperSize = string.IsNullOrWhiteSpace(paperSize) ? "A4" : paperSize;
            t.Orientation = string.IsNullOrWhiteSpace(orientation) ? "portrait" : orientation;
            t.DocType = string.IsNullOrWhiteSpace(docType) ? "Sale" : docType;
            t.DesignJson = designJson;
            t.ModifiedDate = DateTime.Now;
            if (isNew) { t.Status = Status.active; t.CreatedDate = DateTime.Now; }

            db.SaveChanges();
            return Json(new { success = true, id = t.Id });
        }

        // POST: /InvoiceTemplate/Delete  — soft delete
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Invoice Template,Sales Invoice,Credit Sale")]
        public ActionResult Delete(int id)
        {
            var t = db.InvoiceTemplates.FirstOrDefault(x => x.Id == id);
            if (t == null) return Json(new { success = false, message = "Template not found." });
            if (t.IsDefault) return Json(new { success = false, message = "Cannot delete the default template. Set another as default first." });
            t.Status = Status.inactive;
            t.ModifiedDate = DateTime.Now;
            db.SaveChanges();
            return Json(new { success = true });
        }

        // POST: /InvoiceTemplate/SetDefault  — make one template the default (clears the rest)
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Invoice Template,Sales Invoice,Credit Sale")]
        public ActionResult SetDefault(int id)
        {
            var target = db.InvoiceTemplates.FirstOrDefault(x => x.Id == id && x.Status == Status.active);
            if (target == null) return Json(new { success = false, message = "Template not found." });
            // default is per document type — only clear other defaults of the SAME DocType
            foreach (var other in db.InvoiceTemplates.Where(x => x.IsDefault && x.DocType == target.DocType && x.Id != id)) other.IsDefault = false;
            target.IsDefault = true;
            target.ModifiedDate = DateTime.Now;
            db.SaveChanges();
            return Json(new { success = true });
        }

        // GET: /InvoiceTemplate/Render/5?invoiceId=123  — render a template to a standalone printable page.
        // No invoiceId -> SAMPLE data (preview). With invoiceId -> REAL invoice data via the SAME source the
        // legacy invoice uses (Common.SaleData), so figures stay accurate. Standalone (Layout=null) — a parallel
        // path that does NOT touch the legacy print flow.
        [QkAuthorize(Roles = "Dev,Invoice Template,Sales Invoice,Credit Sale")]
        public ActionResult Render(int id, long? invoiceId)
        {
            var t = db.InvoiceTemplates.FirstOrDefault(x => x.Id == id && x.Status == Status.active);
            if (t == null) return Content("Template not found.");

            if (invoiceId.HasValue)
            {
                try
                {
                    var comp = db.companys.FirstOrDefault();
                    Func<decimal?, string> money = v => v.HasValue ? v.Value.ToString("N2") : "";
                    var map = new Dictionary<string, string>();
                    if (comp != null)
                    {
                        map["COMPANY_HEADER.Name"] = comp.CPName ?? "";
                        map["COMPANY_HEADER.Address"] = comp.CPAddress ?? "";
                        map["COMPANY_HEADER.TRN"] = string.IsNullOrWhiteSpace(comp.TRN) ? "" : ("TRN: " + comp.TRN);
                        map["COMPANY_HEADER.Phone"] = comp.CPPhone ?? comp.CPMobile ?? "";
                        map["COMPANY_HEADER.Email"] = comp.CPEmail ?? "";
                        map["COMPANY_HEADER.Logo"] = string.IsNullOrWhiteSpace(comp.CPLogo) ? "" : ("/uploads/company/" + comp.CPLogo);
                    }

                    var dt = string.IsNullOrWhiteSpace(t.DocType) ? "Sale" : t.DocType;
                    if (dt == "Sale" || dt == "Quotation" || dt == "Estimate" || dt == "ProForma"
                        || dt == "SalesOrder" || dt == "DeliveryNote" || dt == "PurchaseOrder" || dt == "PurchaseEntry")
                    {
                        // These documents share the pdfSummaryViewModel shape — call each one's OWN authoritative builder.
                        pdfSummaryViewModel d;
                        switch (dt)
                        {
                            case "Quotation": d = com.QuotationData(invoiceId.Value); break;
                            case "Estimate": d = com.QuotationData(invoiceId.Value); break;   // estimate is a quotation variant
                            case "ProForma": d = com.ProFormaData(invoiceId.Value); break;
                            case "SalesOrder": d = com.SalesOrderData(invoiceId.Value); break;
                            case "DeliveryNote": d = com.DeliveryNoteData(invoiceId.Value); break;
                            case "PurchaseOrder": d = com.PurchaseOrderData(invoiceId.Value); break;
                            case "PurchaseEntry": d = com.PurchaseData(invoiceId.Value); break;
                            default: d = com.SaleData(invoiceId.Value); break;
                        }
                        if (d != null)
                        {
                            var partyAddr = string.Join(", ", new[] { d.Address, d.City, d.State, d.Country }.Where(s => !string.IsNullOrWhiteSpace(s)));
                            map["CUSTOMER.Name"] = d.PartyName ?? "";
                            map["CUSTOMER.Address"] = partyAddr;
                            map["CUSTOMER.TRN"] = string.IsNullOrWhiteSpace(d.TRN) ? "" : ("TRN: " + d.TRN);
                            map["CUSTOMER.Phone"] = d.Phone ?? d.Mobile ?? "";
                            // PARTY.* aliases so supplier-side templates (PurchaseOrder) and generic palettes work too
                            map["PARTY.Name"] = d.PartyName ?? "";
                            map["PARTY.Address"] = partyAddr;
                            map["PARTY.TRN"] = map["CUSTOMER.TRN"];
                            map["PARTY.Phone"] = map["CUSTOMER.Phone"];
                            map["INVOICE.Number"] = d.BillNo ?? "";
                            map["INVOICE.Date"] = d.Date.ToString("dd-MM-yyyy");
                            map["INVOICE.PONumber"] = d.PONo ?? "";
                            map["INVOICE.SalesExecutive"] = d.Cashier ?? "";
                            map["INVOICE.PaymentTerms"] = d.PaymentTerms ?? "";
                            map["SUMMARY.SubTotal"] = money(d.SubTotal);
                            map["SUMMARY.TaxAmount"] = money(d.TaxAmount);
                            map["SUMMARY.TotalDiscount"] = money(d.Discount);
                            map["SUMMARY.GrandTotal"] = money(d.GrandTotal);
                            map["SUMMARY.OutstandingBalance"] = money(d.Balance);
                            try { map["SUMMARY.AmountInWords"] = d.GrandTotal.HasValue ? (com.ConvertToWords(d.GrandTotal.Value.ToString()) ?? "") : ""; }
                            catch { map["SUMMARY.AmountInWords"] = ""; }

                            var items = (d.pdfItem ?? new List<pdfItemViewModel>()).Select((it, ix) => new object[]
                            {
                                (ix + 1).ToString(),
                                string.IsNullOrWhiteSpace(it.ItemName) ? (it.ItemCode ?? "") : it.ItemName,
                                it.ItemUnit ?? "",
                                money(it.ItemQuantity),
                                money(it.ItemUnitPrice),
                                money(it.ItemTaxAmount),
                                money(it.ItemTotalAmount)
                            }).ToList();
                            ViewBag.ItemsJson = Newtonsoft.Json.JsonConvert.SerializeObject(items);
                            ViewBag.InvoiceNo = d.BillNo ?? invoiceId.Value.ToString();
                        }
                    }
                    else if (dt == "Receipt")
                    {
                        // Mirror ReceiptController.Editprint: party = PayFrom account (PayTo is the cash/bank
                        // account, Group 8/9); Bank/Cheque from the PDCs table. Same columns -> accurate figures.
                        var rpt = db.Receipts.FirstOrDefault(x => x.ReceiptId == invoiceId.Value);
                        if (rpt != null)
                        {
                            map["PARTY.Name"] = db.Accountss.Where(a => a.AccountsID == rpt.PayFrom).Select(a => a.Name).FirstOrDefault() ?? "";
                            map["RECEIPT.VoucherNo"] = rpt.VoucherNo ?? "";
                            map["RECEIPT.Date"] = rpt.Date.ToString("dd-MM-yyyy");
                            map["RECEIPT.MOPayment"] = rpt.MOPayment.ToString();
                            var pdc = db.PDCs.Where(p => p.Reference == rpt.ReceiptId && p.PDCType == "Receipt").Select(p => new { p.Bank, p.CheckNo }).FirstOrDefault();
                            map["RECEIPT.Bank"] = pdc != null ? (pdc.Bank ?? "") : "";
                            map["RECEIPT.CheckNo"] = pdc != null ? (pdc.CheckNo ?? "") : "";
                            map["RECEIPT.Ref1"] = rpt.Ref1 ?? ""; map["RECEIPT.Ref2"] = rpt.Ref2 ?? "";
                            map["SUMMARY.GrandTotal"] = money(rpt.GrandTotal);
                            map["SUMMARY.Discount"] = money(rpt.Discount);
                            try { map["SUMMARY.AmountInWords"] = com.ConvertToWords(rpt.GrandTotal.ToString()) ?? ""; } catch { map["SUMMARY.AmountInWords"] = ""; }
                            ViewBag.InvoiceNo = rpt.VoucherNo ?? invoiceId.Value.ToString();
                        }
                    }
                    else if (dt == "Payment")
                    {
                        // Mirror PaymentController.Editprint: party = PayTo account (Paid To); PayFrom is the
                        // cash/bank account; GrandTotal shown = pay.GrandTotal + pay.Discount (exact same expr).
                        var pay = db.Payments.FirstOrDefault(x => x.PaymentId == invoiceId.Value);
                        if (pay != null)
                        {
                            map["PARTY.Name"] = db.Accountss.Where(a => a.AccountsID == pay.PayTo).Select(a => a.Name).FirstOrDefault() ?? "";
                            map["PAYMENT.VoucherNo"] = pay.VoucherNo ?? "";
                            map["PAYMENT.Date"] = pay.Date.ToString("dd-MM-yyyy");
                            map["PAYMENT.MOPayment"] = pay.MOPayment.ToString();
                            map["PAYMENT.PDCDate"] = pay.PDCDate != null ? ((DateTime)pay.PDCDate).ToString("dd-MM-yyyy") : "";
                            map["PAYMENT.Remark"] = pay.Remark ?? "";
                            map["PAYMENT.Ref1"] = pay.Ref1 ?? ""; map["PAYMENT.Ref2"] = pay.Ref2 ?? "";
                            var pdc = db.PDCs.Where(p => p.Reference == pay.PaymentId && p.PDCType == "Payment").Select(p => new { p.Bank, p.CheckNo }).FirstOrDefault();
                            var grand = pay.GrandTotal + pay.Discount;
                            map["SUMMARY.GrandTotal"] = money(grand);
                            map["SUMMARY.SubTotal"] = money(pay.SubTotal);
                            map["SUMMARY.TaxAmount"] = money(pay.TaxAmount);
                            map["SUMMARY.TotalDiscount"] = money(pay.Discount);
                            try { map["SUMMARY.AmountInWords"] = com.ConvertToWords(grand.ToString()) ?? ""; } catch { map["SUMMARY.AmountInWords"] = ""; }
                            ViewBag.InvoiceNo = pay.VoucherNo ?? invoiceId.Value.ToString();
                        }
                    }
                    else if (dt == "Cheque")
                    {
                        // A cheque is printed from a Payment (the cheque payment): payee = PayTo account,
                        // amount = GrandTotal + Discount, cheque no + bank from the PDC row, dated by PDCDate.
                        var pay = db.Payments.FirstOrDefault(x => x.PaymentId == invoiceId.Value);
                        if (pay != null)
                        {
                            map["PAYEE.Name"] = db.Accountss.Where(a => a.AccountsID == pay.PayTo).Select(a => a.Name).FirstOrDefault() ?? "";
                            var pdc = db.PDCs.Where(p => p.Reference == pay.PaymentId && p.PDCType == "Payment").Select(p => new { p.Bank, p.CheckNo }).FirstOrDefault();
                            map["PAYEE.BankName"] = pdc != null ? (pdc.Bank ?? "") : "";
                            map["CHEQUE.ChequeNumber"] = pdc != null ? (pdc.CheckNo ?? "") : "";
                            map["CHEQUE.VoucherNo"] = pay.VoucherNo ?? "";
                            map["CHEQUE.Date"] = pay.PDCDate != null ? ((DateTime)pay.PDCDate).ToString("dd-MM-yyyy") : pay.Date.ToString("dd-MM-yyyy");
                            var grand = pay.GrandTotal + pay.Discount;
                            map["CHEQUE.Amount"] = money(grand);
                            try { map["CHEQUE.AmountInWords"] = com.ConvertToWords(grand.ToString()) ?? ""; } catch { map["CHEQUE.AmountInWords"] = ""; }
                            ViewBag.InvoiceNo = pay.VoucherNo ?? invoiceId.Value.ToString();
                        }
                    }
                    else if (dt == "CreditNote" || dt == "MaterialRequisition")
                    {
                        // These builders return a Dictionary of ANONYMOUS objects (not pdfSummaryViewModel).
                        // Serialize the "summary"/"item" objects to JSON and read fields by name — robust + accurate.
                        var dict = dt == "MaterialRequisition" ? com.MRNoteData(invoiceId.Value) : com.SalesReturnData(invoiceId.Value);
                        if (dict != null && dict.ContainsKey("summary") && dict["summary"] != null)
                        {
                            var jo = Newtonsoft.Json.Linq.JObject.FromObject(dict["summary"]);
                            map["CUSTOMER.Name"] = jo.Value<string>("PartyName") ?? "";
                            map["PARTY.Name"] = map["CUSTOMER.Name"];
                            map["CUSTOMER.Address"] = jo.Value<string>("Addres") ?? jo.Value<string>("Address") ?? "";
                            map["PARTY.Address"] = map["CUSTOMER.Address"];
                            map["PARTY.Phone"] = jo.Value<string>("Phone") ?? jo.Value<string>("Mobile") ?? "";
                            var trn = jo.Value<string>("TRN");
                            map["CUSTOMER.TRN"] = string.IsNullOrWhiteSpace(trn) ? "" : ("TRN: " + trn);
                            map["INVOICE.Number"] = jo.Value<string>("BillNo") ?? "";
                            map["INVOICE.Date"] = jo.Value<DateTime?>("Date")?.ToString("dd-MM-yyyy") ?? "";
                            map["INVOICE.PONumber"] = jo.Value<string>("AgainstInvoice") ?? "";
                            map["INVOICE.SalesExecutive"] = jo.Value<string>("CreatedBy") ?? jo.Value<string>("Cashier") ?? "";
                            map["SUMMARY.SubTotal"] = jo.Value<decimal?>("SubTotal")?.ToString("N2") ?? "";
                            map["SUMMARY.TaxAmount"] = jo.Value<decimal?>("TaxAmount")?.ToString("N2") ?? "";
                            map["SUMMARY.TotalDiscount"] = jo.Value<decimal?>("Discount")?.ToString("N2") ?? "";
                            var gt = jo.Value<decimal?>("GrandTotal");
                            map["SUMMARY.GrandTotal"] = gt?.ToString("N2") ?? "";
                            try { map["SUMMARY.AmountInWords"] = gt.HasValue ? (com.ConvertToWords(gt.Value.ToString()) ?? "") : ""; } catch { map["SUMMARY.AmountInWords"] = ""; }

                            if (dict.ContainsKey("item") && dict["item"] != null)
                            {
                                var ja = Newtonsoft.Json.Linq.JArray.FromObject(dict["item"]);
                                int ix = 1; var items = new List<object[]>();
                                foreach (var it in ja)
                                    items.Add(new object[]
                                    {
                                        (ix++).ToString(),
                                        it.Value<string>("ItemName") ?? it.Value<string>("ItemCode") ?? "",
                                        it.Value<string>("ItemUnit") ?? "",
                                        it.Value<decimal?>("ItemQuantity")?.ToString("N2") ?? "",
                                        it.Value<decimal?>("ItemUnitPrice")?.ToString("N2") ?? "",
                                        it.Value<decimal?>("ItemTaxAmount")?.ToString("N2") ?? "",
                                        it.Value<decimal?>("ItemTotalAmount")?.ToString("N2") ?? ""
                                    });
                                ViewBag.ItemsJson = Newtonsoft.Json.JsonConvert.SerializeObject(items);
                            }
                            ViewBag.InvoiceNo = jo.Value<string>("BillNo") ?? invoiceId.Value.ToString();
                        }
                    }
                    else if (dt == "TenancyContract")
                    {
                        // Real-estate tenancy contract — mirror TenancyContractController.Print data fetch (EF Core 10 safe).
                        var c = db.TenancyContracts.Where(x => x.Id == invoiceId.Value)
                            .Select(x => new { x.Id, x.Code, x.Tenant, x.Property, x.Unit, x.StartDate, x.EndDate, x.issuedate, x.Rent, x.Deposit, x.Duration, x.Schedule, x.DueDate, x.PaymentType, x.contractvalue, x.NumberofOccupants, x.WaterAndElectricityBill, x.PetsAllowed }).FirstOrDefault();
                        if (c != null)
                        {
                            long propId = c.Property ?? 0;
                            var prop = db.PropertyMains.Where(p => p.Id == propId).Select(p => new { p.Name, p.City, p.State, p.Address, p.LandlordID }).FirstOrDefault();
                            long llId = prop != null ? (prop.LandlordID ?? 0) : 0;
                            var landlord = (from l in db.Landlords where l.LandlordID == llId
                                            join ct in db.Contacts on l.Contact equals ct.ContactID into cc from ct in cc.DefaultIfEmpty()
                                            select new { l.LandlordName, ct.Mobile, ct.Phone, ct.Address }).FirstOrDefault();
                            long tnId = c.Tenant ?? 0;
                            var tenant = (from tn in db.Tenants where tn.TenantID == tnId
                                          join ct in db.Contacts on tn.Contact equals ct.ContactID into cc from ct in cc.DefaultIfEmpty()
                                          select new { tn.TenantName, ct.Mobile, ct.Phone, ct.Address }).FirstOrDefault();
                            string unitName = db.PropertyUnits.Where(u => u.Id == c.Unit).Select(u => u.Name).FirstOrDefault();
                            string durName = db.Durations.Where(d => d.Id == (c.Duration ?? 0)).Select(d => d.Name).FirstOrDefault();
                            string sched = c.Schedule == Schedule.Monthly ? "Monthly" : c.Schedule == Schedule.Month3 ? "Every 3 Months" : c.Schedule == Schedule.Month6 ? "Every 6 Months" : "Yearly";

                            map["LANDLORD.Name"] = landlord != null ? (landlord.LandlordName ?? "") : "";
                            map["LANDLORD.Phone"] = landlord != null ? ((landlord.Mobile ?? landlord.Phone) ?? "") : "";
                            map["LANDLORD.Address"] = landlord != null ? (landlord.Address ?? "") : "";
                            map["TENANT.Name"] = tenant != null ? (tenant.TenantName ?? "") : "";
                            map["TENANT.Phone"] = tenant != null ? ((tenant.Mobile ?? tenant.Phone) ?? "") : "";
                            map["TENANT.Address"] = tenant != null ? (tenant.Address ?? "") : "";
                            map["PARTY.Name"] = map["TENANT.Name"]; map["CUSTOMER.Name"] = map["TENANT.Name"];
                            map["PROPERTY.Name"] = prop != null ? (prop.Name ?? "") : "";
                            map["PROPERTY.Unit"] = unitName ?? "";
                            map["PROPERTY.Address"] = prop == null ? "" : string.Join(", ", new[] { prop.Address, prop.City, prop.State }.Where(s => !string.IsNullOrWhiteSpace(s)));
                            map["TENANCY.Code"] = c.Code ?? "";
                            map["TENANCY.IssueDate"] = (c.issuedate ?? c.StartDate).ToString("dd-MM-yyyy");
                            map["TENANCY.StartDate"] = c.StartDate.ToString("dd-MM-yyyy");
                            map["TENANCY.EndDate"] = c.EndDate.ToString("dd-MM-yyyy");
                            map["TENANCY.Duration"] = durName ?? "";
                            map["TENANCY.Rent"] = money(c.Rent);
                            map["TENANCY.Deposit"] = money(c.Deposit);
                            map["TENANCY.ContractValue"] = string.IsNullOrWhiteSpace(c.contractvalue) ? money(c.Rent) : c.contractvalue;
                            map["TENANCY.PaymentMode"] = (c.PaymentType == 1) ? "Cash" : "Cheque";
                            map["TENANCY.Schedule"] = sched;
                            map["TENANCY.Occupants"] = string.IsNullOrWhiteSpace(c.NumberofOccupants) ? "" : c.NumberofOccupants;
                            map["TENANCY.Pets"] = string.IsNullOrWhiteSpace(c.PetsAllowed) ? "" : c.PetsAllowed;
                            map["SUMMARY.GrandTotal"] = money(c.Rent);
                            try { map["SUMMARY.AmountInWords"] = c.Rent.HasValue ? (com.ConvertToWords(c.Rent.Value.ToString()) ?? "") : ""; } catch { map["SUMMARY.AmountInWords"] = ""; }
                            ViewBag.InvoiceNo = c.Code ?? invoiceId.Value.ToString();
                        }
                    }
                    else
                    {
                        // Unknown document type — layout preview with the company header only (no fabricated figures).
                        ViewBag.PreviewOnly = "Live data binding for " + dt + " documents is being finalised — showing the layout with the company header only.";
                        ViewBag.InvoiceNo = invoiceId.Value.ToString();
                    }
                    ViewBag.DataJson = Newtonsoft.Json.JsonConvert.SerializeObject(map);
                }
                catch (Exception ex)
                {
                    ViewBag.RenderError = "Could not load document #" + invoiceId.Value + ": " + ex.Message;
                }
            }
            return View(t);
        }

        // GET: /InvoiceTemplate/Print?invoiceId=123[&docType=Sale][&templateId=5] — print-time entry for a REAL document.
        // 0 templates -> message; 1 -> straight to it; >1 -> picker. Additive; legacy print untouched.
        [QkAuthorize(Roles = "Dev,Invoice Template,Sales Invoice,Credit Sale")]
        public ActionResult Print(long invoiceId, int? templateId, string docType = "Sale")
        {
            docType = NormDoc(docType);
            if (templateId.HasValue) return RedirectToAction("Render", new { id = templateId.Value, invoiceId });
            var actives = db.InvoiceTemplates.Where(t => t.Status == Status.active && t.DocType == docType)
                            .OrderByDescending(t => t.IsDefault).ThenBy(t => t.Name).ToList();
            if (actives.Count == 0) return Content("No " + docType + " templates yet. Create one under Design Templates.");
            if (actives.Count == 1) return RedirectToAction("Render", new { id = actives[0].Id, invoiceId });
            ViewBag.InvoiceId = invoiceId;
            ViewBag.DocType = docType;
            return View("Pick", actives);
        }

        // POST: /InvoiceTemplate/UploadImage  — upload an image (logo/stamp/signature) for use in a template.
        // Saved under wwwroot/uploads/templates; returns its URL. Antiforgery via the global jQuery header hook.
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Invoice Template,Sales Invoice,Credit Sale")]
        public ActionResult UploadImage(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0) return Json(new { success = false, message = "No file selected." });
            if (file.Length > 4 * 1024 * 1024) return Json(new { success = false, message = "Image too large (max 4 MB)." });
            var ext = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".png", ".jpg", ".jpeg", ".gif", ".svg", ".webp" };
            if (!allowed.Contains(ext)) return Json(new { success = false, message = "Unsupported file type." });
            try
            {
                var dir = System.IO.Path.Combine(env.WebRootPath, "uploads", "templates");
                System.IO.Directory.CreateDirectory(dir);
                var name = System.Guid.NewGuid().ToString("N") + ext;
                using (var fs = System.IO.File.Create(System.IO.Path.Combine(dir, name))) file.CopyTo(fs);
                return Json(new { success = true, url = "/uploads/templates/" + name });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // POST: /InvoiceTemplate/Clone  — duplicate an existing template
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Invoice Template,Sales Invoice,Credit Sale")]
        public ActionResult Clone(int id)
        {
            var src = db.InvoiceTemplates.FirstOrDefault(x => x.Id == id && x.Status == Status.active);
            if (src == null) return Json(new { success = false, message = "Template not found." });
            var copy = new InvoiceTemplate
            {
                Name = src.Name + " (Copy)",
                DocType = src.DocType,
                PaperSize = src.PaperSize,
                Orientation = src.Orientation,
                DesignJson = src.DesignJson,
                IsDefault = false,
                Status = Status.active,
                CreatedDate = DateTime.Now
            };
            db.InvoiceTemplates.Add(copy);
            db.SaveChanges();
            return Json(new { success = true, id = copy.Id });
        }
    }
}
