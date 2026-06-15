using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace QuickSoft.Helpers
{
    // Performance (audit P3): views are RUNTIME-compiled by design (2,000+ large legacy views exceed
    // the single-assembly precompile limit), so the FIRST visitor of each screen after a service
    // restart paid the Razor compile (~50-300 ms/view). This background service pre-compiles the
    // hottest screens shortly after startup, so the morning's first user gets warm pages.
    public sealed class ViewWarmupService : BackgroundService
    {
        private static readonly string[] HotViews =
        {
            "/Views/Users/Login.cshtml",
            "/Views/Shared/_QuickLayout.cshtml",
            "/Views/Home/Index.cshtml",
            "/Views/CreditSale/Index.cshtml",
            "/Views/CreditSale/Create.cshtml",
            "/Views/Quotation/Index.cshtml",
            "/Views/Quotation/Create.cshtml",
            "/Views/Customer/Index.cshtml",
            "/Views/Customer/Create.cshtml",
            "/Views/Item/Index.cshtml",
            "/Views/Item/Create.cshtml",
            "/Views/Payment/Index.cshtml",
            "/Views/Receipt/Index.cshtml",
            "/Views/PurchaseEntry/Index.cshtml",
            "/Views/PurchaseOrder/Index.cshtml",
            "/Views/SalesOrder/Index.cshtml",
            "/Views/Deliverynote/Index.cshtml",
            "/Views/ProTask/Index.cshtml",
            "/Views/AMC/Index.cshtml",
            "/Views/Leads/Index.cshtml",
            "/Views/StockReport/Index.cshtml",
            "/Views/Supplier/Index.cshtml",
            "/Views/Estimate/Index.cshtml",
            "/Views/ProForma/Index.cshtml",
            "/Views/JobCard/Index.cshtml"
        };

        private readonly IServiceProvider _services;
        private readonly ILogger<ViewWarmupService> _logger;

        public ViewWarmupService(IServiceProvider services, ILogger<ViewWarmupService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // let the host finish binding/first requests before burning CPU on compiles
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            int ok = 0, fail = 0;
            using var scope = _services.CreateScope();
            var engine = scope.ServiceProvider.GetRequiredService<IRazorViewEngine>();

            foreach (var path in HotViews)
            {
                if (stoppingToken.IsCancellationRequested) return;
                try
                {
                    var page = engine.GetPage("/", path); // forces runtime compilation
                    if (page.Page != null) ok++; else fail++;
                }
                catch (Exception ex)
                {
                    fail++;
                    _logger.LogDebug(ex, "view warmup failed for {View}", path);
                }
                // tiny breather so warmup never starves real requests
                await Task.Delay(50, stoppingToken);
            }

            _logger.LogInformation("View warmup: {Ok} compiled, {Fail} skipped in {Ms} ms", ok, fail, sw.ElapsedMilliseconds);
        }
    }
}
