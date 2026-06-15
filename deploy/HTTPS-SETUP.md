# BOS — HTTPS / TLS setup

HTTPS is required for two things:
1. **Installing the PWA on phones.** Browsers only register a service worker (needed for "Add to Home
   Screen" / app install) on a **secure context** — HTTPS with a trusted certificate, or `localhost`.
   Plain `http://192.168.x.x:8080` will **not** install on a phone.
2. **Protecting login + ERP data** in transit on the LAN.

The daily HTTP pilots on **8080 / 8081 stay as they are** — HTTPS runs on a **separate port (8443)**, so
nothing about the current setup breaks.

---

## A. LAN / on-prem (self-signed certificate) — what's set up now

A self-signed certificate has been generated at `C:\Quick Soft 10-06-2026\certs\`:
- `bos-https.pfx` — server certificate **+ private key** (used by the app; keep private).
- `bos-https.cer` — public certificate (install this on phones/PCs to make them trust the server).

SAN (names the cert is valid for): `localhost`, `DESKTOP-UH8DQ0M`, `127.0.0.1`, `192.168.35.222`.
Valid until **2029-06-14**. PFX password: `BosHttps#2026` (change it for a real rollout).

### 1. Run the server over HTTPS
```powershell
cd "C:\Quick Soft 10-06-2026\QuickNetCore\deploy"
# first time on this machine (also trusts the cert locally — click "Yes" if Windows prompts):
.\enable-https.ps1 -Branch emirtechlatest -Port 8443 -Trust
# trading branch on another port:
.\enable-https.ps1 -Branch quicknetlatest-1200 -Port 8444
```
Verified working: `https://localhost:8443` and `https://192.168.35.222:8443` both serve 200.

### 2. Trust the certificate on each phone / PC that will use it
A self-signed cert is not trusted until you install it. Per device, **once**:

**Windows PC:** double-click `bos-https.cer` → *Install Certificate* → *Current User* →
*Place all certificates in the following store* → **Trusted Root Certification Authorities** → Finish.

**Android:** copy `bos-https.cer` to the phone → Settings → Security → *Encryption & credentials* →
*Install a certificate* → **CA certificate** → pick the file. Then open `https://192.168.35.222:8443`
in Chrome → menu → **Install app**.

**iPhone/iPad:** AirDrop/email `bos-https.cer` → install the profile (Settings → *Profile Downloaded*) →
then Settings → General → About → *Certificate Trust Settings* → enable full trust for it. Open the URL
in Safari → Share → **Add to Home Screen**.

> If managing certs on every phone is too much, prefer Option B below.

---

## B. Internet / SaaS / many devices (real domain — recommended for production)

For a public or multi-tenant SaaS deployment, don't use self-signed — use a **real domain + automatic TLS**.
No per-device trust needed; every browser trusts it; PWA installs with zero setup.

Pick one in front of the app (the app keeps serving plain HTTP on its port; the proxy terminates TLS):

- **Caddy** (simplest — automatic Let's Encrypt): a 2-line `Caddyfile`
  ```
  erp.yourdomain.com {
      reverse_proxy localhost:8080
  }
  ```
  Caddy fetches + renews the certificate automatically.
- **IIS + win-acme** (native Windows): bind the site to 443, run `wacs.exe` to get + auto-renew a
  Let's Encrypt cert.
- **Nginx / Cloudflare Tunnel** are equally fine.

Requirements: a domain name pointing at the server's public IP, and ports 80/443 reachable for the
certificate challenge.

---

## C. Going live: move the main pilots to HTTPS

When ready to make HTTPS the primary access (instead of a side port):
1. Point users at the HTTPS URL (e.g. `https://192.168.35.222:8443`, or `https://erp.yourdomain.com`).
2. Optional: enable HTTP→HTTPS redirect + HSTS. The app already calls `UseHsts()` outside Development;
   add `app.UseHttpsRedirection()` in `Program.cs` **only once every access path is HTTPS** (adding it
   while users still use plain HTTP would break them).
3. Rotate the PFX password and keep `bos-https.pfx` off any shared/source location.
