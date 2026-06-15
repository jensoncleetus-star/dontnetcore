using System.Runtime.Versioning;

// This application is Windows-only BY DESIGN: it is hosted as a Windows Service (UseWindowsService),
// ships as a self-contained win-x64 bundle, and uses Windows-only System.Drawing image APIs
// (uploads/thumbnails, ported 1:1 from the legacy MVC5 app). Declaring that here makes the
// platform analyzer treat every call site as Windows-guaranteed, which truthfully resolves the
// ~2,000 CA1416 "only supported on windows" warnings instead of suppressing them.
[assembly: SupportedOSPlatform("windows")]
