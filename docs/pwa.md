# PWA Support — StaySync

StaySync includes Progressive Web App (PWA) support via the Angular Service Worker. The app is installable on desktop and mobile and caches static assets for faster repeat loads.

---

## What's Included

- **Web App Manifest** (`src/manifest.webmanifest`) — makes the app installable in standalone mode
- **Service Worker** (`ngsw-worker.js`) — generated at build time from `ngsw-config.json`
- **Caching strategy** — aggressive static asset caching, network-first for API data

---

## Caching Behaviour

### Static Assets (`assetGroups`)

| Group | Mode | Files |
|---|---|---|
| `app-shell` | prefetch (eager) | `index.html`, JS, CSS, manifest, favicon |
| `assets` | lazy (on demand) | Images, fonts, SVGs in `/assets/` |

The app shell is pre-cached on install so pages load instantly on repeat visits.

### API Data (`dataGroups`)

| Group | Strategy | URL Pattern | TTL | Max entries |
|---|---|---|---|---|
| `api-freshness` | freshness (network-first) | `https://api.staysync.app/api/**` | 1 hour | 50 |

- Always tries the network first with a 5-second timeout
- Falls back to the cache only if the network is unavailable
- Cached responses expire after 1 hour

**Auth tokens are never cached.** All authenticated requests still hit the network first.

---

## Limitations

- **Not fully offline.** Booking data requires a network connection to load fresh. Cache only provides a short-term fallback.
- **API URL is hardcoded** in `ngsw-config.json`. If you change the production API URL, update the `urls` entry in `dataGroups`.
- **Service worker is disabled in dev** (`ng serve`). Test PWA features only with a production build (`ng build --configuration=production`).

---

## Icons

The manifest references PNG icons at `src/assets/icons/`. These must be created before the app is considered fully installable. Required sizes:

```
icon-72x72.png
icon-96x96.png
icon-128x128.png
icon-144x144.png
icon-152x152.png
icon-192x192.png   ← required for Chrome installability
icon-384x384.png
icon-512x512.png   ← required for Chrome installability
```

Generate them from a master SVG using a tool like [RealFaviconGenerator](https://realfavicongenerator.net) or Inkscape.

---

## Production Build

The service worker is only active in production builds:

```bash
cd frontend
ng build --configuration=production
```

The build output in `dist/stay-sync-frontend/` will include `ngsw-worker.js` and `ngsw.json` (the runtime manifest).

---

## Vercel Deployment

`vercel.json` is configured to rewrite all non-file routes to `index.html`, which is required for Angular's client-side routing to work correctly alongside the service worker.

---

## Testing the Service Worker

1. Run `ng build --configuration=production`
2. Serve the `dist/` folder with a static server that supports HTTPS (or localhost)
3. Open Chrome DevTools → Application → Service Workers
4. Verify the service worker is registered and active
5. Use the Cache Storage panel to inspect cached assets
