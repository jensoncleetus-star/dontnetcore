using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Html;
using QuickSoft.Models;

namespace QuickSoft.Web
{
    // Faithful replacement for MVC5 System.Web.Optimization @Styles.Render / @Scripts.Render.
    // The bundle map below is generated from the legacy App_Start/BundleConfig.cs. Each bundle's
    // virtual path resolves to its included files (wildcards {version}/* are globbed against wwwroot
    // via LegacyWeb.MapPath) and emitted as plain <link>/<script> tags (no minification = debug parity).
    public static class BundleRegistry
    {
        public static readonly Dictionary<string, string[]> Map = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "~/js/jquery", new[]{ "~/Scripts/jquery-{version}.js" } },
            { "~/js/jqueryval", new[]{ "~/Scripts/jquery.validate*" } },
            { "~/js/modernizr", new[]{ "~/Scripts/modernizr-*" } },
            { "~/js/bootstrap", new[]{ "~/Scripts/bootstrap.js", "~/Scripts/respond.js" } },
            { "~/js/js", new[]{ "~/Content/plugins/jquery/jquery.min.js", "~/Scripts/bootstrap.js", "~/Scripts/respond.js", "~/Content/plugins/jquery-slimscroll/jquery.slimscroll.min.js", "~/Content/plugins/fastclick/fastclick.js", "~/Scripts/moment.js", "~/Content/js/adminlte.js", "~/Scripts/custom.js" } },
            { "~/js/select2", new[]{ "~/Content/plugins/select2/select2.min.js" } },
            { "~/css/select2", new[]{ "~/Content/plugins/select2/select2.min.css" } },
            { "~/js/ckeditor", new[]{ "~/Content/plugins/ckeditor/ckeditor.js" } },
            { "~/js/numpad", new[]{ "~/Content/plugins/numpad/jquery.numpad.js" } },
            { "~/css/numpad", new[]{ "~/Content/plugins/numpad/jquery.numpad.css" } },
            { "~/js/bootstrap-fileinput", new[]{ "~/Content/plugins/bootstrap-fileinput/js/fileinput.min.js", "~/Content/plugins/bootstrap-fileinput/js/fileinput.js" } },
            { "~/js/bootstrap-piexif", new[]{ "~/Content/plugins/bootstrap-fileinput/js/plugins/piexif.min.js", "~/Content/plugins/bootstrap-fileinput/js/plugins/piexif.js" } },
            { "~/js/bootstrap-purify", new[]{ "~/Content/plugins/bootstrap-fileinput/js/plugins/purify.min.js", "~/Content/plugins/bootstrap-fileinput/js/plugins/purify.js" } },
            { "~/js/bootstrap-sortable", new[]{ "~/Content/plugins/bootstrap-fileinput/js/plugins/sortable.min.js", "~/Content/plugins/bootstrap-fileinput/js/plugins/sortable.js" } },
            { "~/css/bootstrap-fileinput", new[]{ "~/Content/plugins/bootstrap-fileinput/css/fileinput.min.css", "~/Content/plugins/bootstrap-fileinput/css/fileinput.css" } },
            { "~/css/bootstrap-fileinput-rtl", new[]{ "~/Content/plugins/bootstrap-fileinput/css/fileinput-rtl.min.css", "~/Content/plugins/bootstrap-fileinput/css/fileinput-rtl.css" } },
            { "~/js/wysihtml5", new[]{ "~/Content/plugins/bootstrap-wysihtml5/bootstrap3-wysihtml5.alll.js" } },
            { "~/css/wysihtml5", new[]{ "~/Content/plugins/bootstrap-wysihtml5/bootstrap3-wysihtml5.css" } },
            { "~/js/chartjs", new[]{ "~/Content/plugins/chartjs/Chart.min.js" } },
            { "~/css/css", new[]{ "~/Content/bootstrap.css", "~/Content/font-awesome.css", "~/Content/css/AdminLTE.css", "~/Content/css/skin.css", "~/Content/css/colorbox.min.css", "~/Content/css/custom.css" } },
            { "~/css/datatable", new[]{ "~/Content/plugins/datatables/media/css/dataTables.bootstrap.css", "~/Content/plugins/datatables/jquery.dataTables.css", "~/Content/plugins/datatables/extensions/TableTools/css/dataTables.tableTools.css", "~/Content/plugins/datatables/extensions/Buttons/css/buttons.dataTables.css", "~/Content/Plugins/datatables/extensions/Responsive/css/dataTables.responsive.css", "~/Content/Plugins/datatables/extensions/Responsive/css/responsive.bootstrap.css" } },
            { "~/js/datatable", new[]{ "~/Content/Plugins/datatables/media/js/jquery.dataTables.js", "~/Content/Plugins/datatables/media/js/dataTables.bootstrap.js", "~/Content/Plugins/datatables/extensions/Buttons/js/dataTables.buttons.js", "~/Content/Plugins/datatables/extensions/Buttons/js/buttons.flash.js", "~/Content/Plugins/datatables/extensions/Buttons/js/buttons.html5.js", "~/Content/Plugins/datatables/extensions/Buttons/js/buttons.print.js", "~/Content/Plugins/datatables/extensions/Buttons/js/buttons.colVis.min.js", "~/Content/Plugins/datatables/pdfmake.min.js", "~/Content/Plugins/datatables/jszip.min.js", "~/Content/Plugins/datatables/vfs_fonts.js", "~/Content/Plugins/datatables/extensions/Responsive/js/dataTables.responsive.js", "~/Content/Plugins/datatables/extensions/Responsive/js/responsive.bootstrap.js" } },
            { "~/js/sale-invoice", new[]{ "~/Content/js/saleinvoice.js" } },
            { "~/js/sale-invoice2", new[]{ "~/Content/js/saleinvoice2.js" } },
            { "~/js/num2words", new[]{ "~/Content/js/num2words.js" } },
            { "~/js/pos", new[]{ "~/Content/js/pos.js" } },
            { "~/js/payment", new[]{ "~/Content/js/payment.js" } },
            { "~/js/receipt", new[]{ "~/Content/js/receipt.js" } },
            { "~/css/pos", new[]{ "~/Content/css/pos.css" } },
            { "~/css/posres", new[]{ "~/Content/css/posres.css" } },
            { "~/css/dineinpos", new[]{ "~/Content/css/dineinpos.css" } },
            { "~/js/additem", new[]{ "~/Content/js/additem.js" } },
            { "~/js/bom", new[]{ "~/Content/js/bom.js" } },
            { "~/js/bom2", new[]{ "~/Content/js/bom2.js" } },
            { "~/js/prodvoucher", new[]{ "~/Content/js/prodvoucher.js" } },
            { "~/js/unasblevoucher", new[]{ "~/Content/js/unasblevoucher.js" } },
            { "~/js/mixitup", new[]{ "~/Content/plugins/mixitup/mixitup.min.js" } },
            { "~/js/partymaterials", new[]{ "~/Content/js/partymaterials.js" } },
            { "~/js/stockjournal", new[]{ "~/Content/js/stockjournal.js" } },
            { "~/js/bootstrap-datepicker", new[]{ "~/Content/plugins/bootstrap-datepicker/js/bootstrap-datepicker.js", "~/Content/plugins/bootstrap-datepicker/js/bootstrap-datepicker.min.js" } },
            { "~/css/bootstrap-datepicker", new[]{ "~/Content/plugins/bootstrap-datepicker/css/bootstrap-datepicker.css", "~/Content/plugins/bootstrap-datepicker/css/bootstrap-datepicker.min.css" } },
            { "~/js/bootstrap-timepicker", new[]{ "~/Content/plugins/bootstrap-timepicker/js/bootstrap-timepicker.min.js" } },
            { "~/css/bootstrap-timepicker", new[]{ "~/Content/plugins/bootstrap-timepicker/css/bootstrap-timepicker.css" } },
            { "~/css/posprint", new[]{ "~/Content/css/posprint.css" } },
            { "~/js/enter", new[]{ "~/Content/js/enter_key.js" } },
            { "~/css/print", new[]{ "~/Content/css/print.css" } },
            { "~/js/JsBarcode", new[]{ "~/Content/js/JsBarcode.all.min.js" } },
            { "~/js/jobcard", new[]{ "~/Content/js/jobcard.js" } },
            { "~/js/journal", new[]{ "~/Content/js/journal.js" } },
            { "~/js/printThis", new[]{ "~/Content/js/printThis.js" } },
            { "~/js/creditnote", new[]{ "~/Content/js/creditnote.js" } },
            { "~/js/drnote", new[]{ "~/Content/js/drnote.js" } },
            { "~/js/returnnote", new[]{ "~/Content/js/returnnote.js" } },
            { "~/js/ContraVoucher", new[]{ "~/Content/js/ContraVoucher.js" } },
            { "~/js/scaner", new[]{ "~/Content/plugins/ScannerDetection/jquery.scannerdetection.js" } },
            { "~/js/sheetjs", new[]{ "~/Content/plugins/sheetjs/xlsx.core.min.js", "~/Content/plugins/sheetjs/jquery.csv-0.71.min.js" } },
            { "~/js/stockveryfy", new[]{ "~/Content/js/stockveryfy.js" } },
            { "~/js/bootstrap-treeview", new[]{ "~/Content/plugins/bootstrap-treeview/js/bootstrap-treeview.min.js" } },
            { "~/css/bootstrap-treeview", new[]{ "~/Content/plugins/bootstrap-treeview/css/bootstrap-treeview.css" } },
            { "~/js/sweetalert2", new[]{ "~/Content/plugins/sweetalert2/js/sweetalert2.min.js" } },
            { "~/css/sweetalert2", new[]{ "~/Content/plugins/sweetalert2/css/sweetalert2.min.css" } },
            { "~/js/journalvoucher", new[]{ "~/Content/js/journalvoucher.js" } },
            { "~/js/jstree", new[]{ "~/Content/plugins/jstree/jstree.js" } },
            { "~/css/jstree", new[]{ "~/Content/plugins/jstree/themes/default/style.css" } },
            { "~/js/itembundle", new[]{ "~/Content/js/itembundle.js" } },
            { "~/js/returninsale", new[]{ "~/Content/js/returninsale.js" } },
            { "~/js/packinglist", new[]{ "~/Content/js/packinglist.js" } },
            { "~/js/materialrequisition", new[]{ "~/Content/js/materialrequisition.js" } },
            { "~/js/mrnote", new[]{ "~/Content/js/mrnote.js" } },
            { "~/js/purchasequotation", new[]{ "~/Content/js/purchasequotation.js" } },
            { "~/js/porder", new[]{ "~/Content/js/porder.js" } },
            { "~/js/lightbox", new[]{ "~/Content/plugins/lightbox/js/lightbox.min.js" } },
            { "~/css/lightbox", new[]{ "~/Content/plugins/lightbox/css/lightbox.min.css" } },
            { "~/js/batchwise", new[]{ "~/Content/js/batchwise.js" } },
            { "~/js/StockAdjustment", new[]{ "~/Content/js/StockAdjustment.js" } },
            { "~/js/usedmaterials", new[]{ "~/Content/js/UsedMaterials.js" } },
            { "~/js/commission", new[]{ "~/Content/js/commission.js" } },
            { "~/js/dropzone", new[]{ "~/Content/plugins/dropzone/js/dropzone.min.js" } },
            { "~/css/dropzone", new[]{ "~/Content/plugins/dropzone/css/basic.css", "~/Content/plugins/dropzone/css/dropzone.min.css" } },
            { "~/js/checklist", new[]{ "~/Content/js/checklist.js" } },
            { "~/js/optionalfield", new[]{ "~/Content/js/optionalfield.js" } },
            { "~/js/Mobile", new[]{ "~/Content/js/Mobile.js" } },
            { "~/js/employee", new[]{ "~/Content/js/employee.js" } },
            { "~/js/attendance", new[]{ "~/Content/js/attendance.js" } },
            { "~/js/salarystructure", new[]{ "~/Content/js/salarystructure.js" } },
            { "~/js/payrollvoucher", new[]{ "~/Content/js/payrollvoucher.js" } },
            { "~/js/employeegrade", new[]{ "~/Content/js/employeegrade.js" } },
            { "~/js/property", new[]{ "~/Content/js/property.js" } },
            { "~/js/cheque", new[]{ "~/Content/js/Property/cheque.js" } },
            { "~/js/document", new[]{ "~/Content/js/Property/documenttype.js" } },
            { "~/js/docexp", new[]{ "~/Content/js/Property/doctypeexp.js" } },
            { "~/js/fullcalendar", new[]{ "~/Content/plugins/fullcalendar/js/fullcalendar.min.js" } },
            { "~/css/fullcalendar", new[]{ "~/Content/plugins/fullcalendar/css/fullcalendar.min.css" } },
            { "~/js/dailyattendance", new[]{ "~/Content/js/dailyattendance.js" } },
            { "~/js/Gratuity", new[]{ "~/Content/js/Gratuity.js" } },
            { "~/js/FinalSettlement", new[]{ "~/Content/js/FinalSettlement.js" } },
            { "~/js/Depositcheque", new[]{ "~/Content/js/Property/DepositeCheque.js" } },
            { "~/css/demo_1/style", new[]{ "~/Content/css/demo_1/style.css" } },
            { "~/css/shared/style", new[]{ "~/Content/css/shared/style.css" } },
        };

        public static IEnumerable<string> Resolve(string bundleOrFile)
        {
            if (Map.TryGetValue(bundleOrFile, out var patterns))
            {
                foreach (var p in patterns)
                    foreach (var u in ResolvePattern(p)) yield return u;
            }
            else { yield return ToUrl(bundleOrFile); }
        }

        private static IEnumerable<string> ResolvePattern(string pattern)
        {
            bool wildcard = pattern.Contains("{version}") || pattern.Contains("*");
            if (!wildcard) { yield return ToUrl(pattern); yield break; }
            bool single = pattern.Contains("{version}");
            var glob = pattern.Replace("{version}", "*");
            int slash = glob.LastIndexOf('/');
            var dirV = glob.Substring(0, slash + 1);
            var fileGlob = glob.Substring(slash + 1);
            string dirPhys;
            try { dirPhys = LegacyWeb.MapPath(dirV); } catch { yield break; }
            if (!Directory.Exists(dirPhys)) yield break;
            string[] files;
            try { files = Directory.GetFiles(dirPhys, fileGlob); } catch { yield break; }
            var filtered = files.Select(Path.GetFileName)
                .Where(f => !f.EndsWith(".intellisense.js", StringComparison.OrdinalIgnoreCase)
                         && !f.EndsWith("-vsdoc.js", StringComparison.OrdinalIgnoreCase)
                         && !f.EndsWith(".map", StringComparison.OrdinalIgnoreCase)
                         // Pre-compressed variants are NOT JS/CSS source. Emitting them as <script>/<link> makes the
                         // browser fetch raw gzip/brotli bytes (served as application/x-gzip with no Content-Encoding)
                         // and try to execute them -> "SyntaxError: Invalid or unexpected token". Never bundle these;
                         // the static-file middleware (+ response compression) serves the plain asset compressed instead.
                         && !f.EndsWith(".gz", StringComparison.OrdinalIgnoreCase)
                         && !f.EndsWith(".br", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f).ToList();
            if (single)
            {
                var pick = filtered.FirstOrDefault(f => f.EndsWith(".min.js", StringComparison.OrdinalIgnoreCase)) ?? filtered.FirstOrDefault();
                if (pick != null) yield return dirV.Replace("~", "") + pick;
            }
            else { foreach (var f in filtered) yield return dirV.Replace("~", "") + f; }
        }

        private static string ToUrl(string v) => v.Replace("~", "");

        // Cache-busting: append ?v=<file-last-write-ticks> so a changed asset gets a NEW url and
        // bypasses the static-file 24h cache (Cache-Control: max-age=86400) automatically — no more
        // "hard-refresh to pick up the new JS/CSS". Falls back to the bare url if the file is absent
        // or unmappable (e.g. external/CDN paths).
        public static string Versioned(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url) || url.StartsWith("http", StringComparison.OrdinalIgnoreCase) || url.StartsWith("//"))
                    return url;
                var phys = LegacyWeb.MapPath("~" + url);
                if (File.Exists(phys))
                {
                    var ticks = File.GetLastWriteTimeUtc(phys).Ticks;
                    return url + (url.Contains("?") ? "&" : "?") + "v=" + ticks;
                }
            }
            catch { /* fall through to the unversioned url */ }
            return url;
        }
    }

    public static class Scripts
    {
        public static IHtmlContent Render(params string[] paths)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var p in paths) foreach (var u in BundleRegistry.Resolve(p))
                sb.Append("<script src=\"").Append(BundleRegistry.Versioned(u)).Append("\"></script>\n");
            return new HtmlString(sb.ToString());
        }
    }

    public static class Styles
    {
        public static IHtmlContent Render(params string[] paths)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var p in paths) foreach (var u in BundleRegistry.Resolve(p))
                sb.Append("<link rel=\"stylesheet\" href=\"").Append(BundleRegistry.Versioned(u)).Append("\" />\n");
            return new HtmlString(sb.ToString());
        }
    }
}