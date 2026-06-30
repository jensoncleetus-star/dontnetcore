/* BOS (Business Operating System) — service worker
 *
 * DESIGN (deliberately conservative for a live financial ERP):
 *   - Caches ONLY static assets (css/js/fonts/images/icons). These are versioned via ?v= stamps,
 *     so a theme bump fetches a fresh URL and the old entry is dropped on activate.
 *   - NEVER caches HTML navigations or data (AJAX/JSON) responses. Every page and every figure is
 *     fetched live from the server — no stale balances, no cross-user data served from cache.
 *   - NEVER touches non-GET requests (POST/PUT/DELETE = data writes) or cross-origin requests.
 *   - When a navigation fails (truly offline), shows a friendly offline page instead of the browser error.
 *
 * Bump CACHE_VERSION on any change to this file or the precache list to force a clean refresh.
 */
'use strict';

const CACHE_VERSION = 'qs-pwa-v3'; // v3: list redesign — purges old cached css/js so clients fetch fresh
const STATIC_CACHE = 'qs-static-' + CACHE_VERSION;
const OFFLINE_URL = '/offline.html';

// Keep the precache list tiny and stable — just the offline shell + its icon.
const PRECACHE_URLS = [
  OFFLINE_URL,
  '/Content/icons/icon-192.png'
];

// Only these extensions / folders are treated as cacheable static assets.
const STATIC_EXT = /\.(?:css|js|mjs|png|jpe?g|gif|svg|webp|ico|woff2?|ttf|eot|otf)$/i;
const STATIC_DIRS = ['/Content/', '/Scripts/', '/lib/', '/fonts/', '/css/', '/js/'];

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(STATIC_CACHE)
      .then((cache) => cache.addAll(PRECACHE_URLS))
      .then(() => self.skipWaiting())
      .catch(() => self.skipWaiting()) // never block install on a precache miss
  );
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys()
      .then((keys) => Promise.all(
        keys.filter((k) => k !== STATIC_CACHE).map((k) => caches.delete(k))
      ))
      .then(() => self.clients.claim())
  );
});

function isStaticAsset(url) {
  if (STATIC_EXT.test(url.pathname)) return true;
  for (let i = 0; i < STATIC_DIRS.length; i++) {
    if (url.pathname.indexOf(STATIC_DIRS[i]) === 0) return true;
  }
  return false;
}

self.addEventListener('fetch', (event) => {
  const req = event.request;

  // Data writes and anything non-GET: leave entirely to the network.
  if (req.method !== 'GET') return;

  let url;
  try { url = new URL(req.url); } catch (e) { return; }

  // Only same-origin. Third-party (CDN/analytics) goes straight to the network.
  if (url.origin !== self.location.origin) return;

  // HTML navigations: network-first, with the offline page as the only fallback.
  // We never cache the page itself (it may contain authenticated, time-sensitive data).
  if (req.mode === 'navigate') {
    event.respondWith(
      fetch(req).catch(() =>
        caches.match(OFFLINE_URL).then((r) => r || new Response(
          '<h1>Offline</h1><p>No internet connection.</p>',
          { headers: { 'Content-Type': 'text/html' } }
        ))
      )
    );
    return;
  }

  // Static assets: stale-while-revalidate (instant from cache, refreshed in the background).
  if (isStaticAsset(url)) {
    event.respondWith(
      caches.open(STATIC_CACHE).then((cache) =>
        cache.match(req).then((cached) => {
          const network = fetch(req).then((res) => {
            if (res && res.status === 200 && res.type === 'basic') {
              cache.put(req, res.clone());
            }
            return res;
          }).catch(() => cached);
          return cached || network;
        })
      )
    );
    return;
  }

  // Everything else (e.g. GET JSON/AJAX data): network-only — do not cache, keep figures live.
});

// Let the page tell a waiting worker to take over immediately (used after an update).
self.addEventListener('message', (event) => {
  if (event.data === 'SKIP_WAITING') self.skipWaiting();
});
