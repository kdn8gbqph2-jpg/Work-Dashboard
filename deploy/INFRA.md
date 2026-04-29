# Production / Infrastructure — BDA Work Dashboard

Handoff reference for future Claude sessions. Keep this file updated whenever the VPS, domain, DB, or deploy pipeline changes.

---

## 1. At a glance

| | |
|---|---|
| **App** | Blazor Server (.NET 8) — `Work-Dashboard.csproj`, target `net8.0` |
| **Repo** | https://github.com/kdn8gbqph2-jpg/Work-Dashboard (branch `develop` is deploy source) |
| **Production URL** | https://works.bdabharatpur.org |
| **VPS IP** | `69.62.80.7` (direct IP blocked — returns `444`) |
| **VPS SSH user** | `itadmin` (passwordless sudo). Port `2222`. |
| **SSL** | Let's Encrypt via `certbot --nginx` |

---

## 2. Server layout

### Systemd service
- Unit file (reference): `deploy/work-dashboard.service`
- Installed at: `/etc/systemd/system/work-dashboard.service`
- App binds: `http://localhost:5020` (loopback only; Nginx proxies)
- Env: `ASPNETCORE_ENVIRONMENT=Production`
- Working dir: `/var/www/work-dashboard`
- Runs as: `www-data`

### App deployment directory
```
/var/www/work-dashboard/
├── Work-Dashboard.dll             # published binary
├── appsettings.Production.json    # DB creds — NOT in git (placeholder only)
└── wwwroot/                       # static assets (CSS, images, JS)
```

### Nginx
- Config (reference): `deploy/nginx-work-dashboard`
- Installed at: `/etc/nginx/sites-available/work-dashboard`
- Symlinked: `/etc/nginx/sites-enabled/work-dashboard`
- Proxies `works.bdabharatpur.org` → `http://localhost:5020`
- **WebSocket headers required** for Blazor Server SignalR (`Upgrade`, `Connection`)

---

## 3. Database — Production

| | |
|---|---|
| Engine | MySQL (local to VPS, `localhost:3306`) |
| DB name | `bda_work_dashboard_prod` |
| App user | `bdaworksuser` / (password set during first-time setup — see VPS) |
| Schema bootstrap | EF Core `EnsureCreated()` / migrations on first app start |
| DB bootstrap SQL | `deploy/setup_prod_db.sql` — creates DB + user (run once) |

### Remote access for dev
SSH tunnel via MySQL Workbench:
- Hostname: `127.0.0.1`, Port `3306`
- SSH host: `69.62.80.7:2222`, SSH user: `itadmin`

---

## 4. Deploy pipeline

`.github/workflows/deploy.yml` — **manual trigger only** (`workflow_dispatch`).

### `build` job (ubuntu-latest)
1. Checkout → Setup .NET 8
2. `dotnet publish -c Release -r linux-x64 --no-self-contained`
3. `tar -czf deploy.tar.gz -C ./publish .`
4. Upload artifact `app-build` (3-day retention)

### `deploy` job (needs: build)
1. Download artifact
2. `appleboy/scp-action` → copy `deploy.tar.gz` to `/tmp` on VPS
3. `appleboy/ssh-action` → run deploy script:
   - `systemctl stop work-dashboard`
   - **Backup** `appsettings.Production.json` → `/tmp`
   - Extract tarball into `/var/www/work-dashboard`
   - **Restore** `appsettings.Production.json`
   - `chown -R www-data:www-data`
   - `systemctl start work-dashboard`
   - Verify with `systemctl is-active` / tail `journalctl` on failure

### GitHub Secrets (shared with other BDA services)
| Secret | Value |
|---|---|
| `VPS_HOST` | `69.62.80.7` |
| `VPS_PORT` | `2222` |
| `VPS_USER` | `itadmin` |
| `VPS_SSH_KEY` | Private key for `itadmin` |

---

## 5. First-time server setup (run once)

```bash
# 1. Install .NET 8 runtime (if not already present)
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
sudo ./dotnet-install.sh --channel 8.0 --runtime dotnet --install-dir /usr/share/dotnet
sudo ln -sf /usr/share/dotnet/dotnet /usr/bin/dotnet   # if not already linked

# 2. Create app directory
sudo mkdir -p /var/www/work-dashboard
sudo chown www-data:www-data /var/www/work-dashboard

# 3. Bootstrap the database (edit password first!)
sudo mysql < /tmp/setup_prod_db.sql

# 4. Create production appsettings on VPS (fill real DB password)
sudo nano /var/www/work-dashboard/appsettings.Production.json
# paste contents from deploy/setup_prod_db.sql, fill in bdaworksuser password

# 5. Install systemd service
sudo cp /tmp/work-dashboard.service /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable work-dashboard

# 6. Install Nginx config
sudo cp /tmp/nginx-work-dashboard /etc/nginx/sites-available/work-dashboard
sudo ln -s /etc/nginx/sites-available/work-dashboard /etc/nginx/sites-enabled/
sudo nginx -t && sudo systemctl reload nginx

# 7. Issue SSL certificate
sudo certbot --nginx -d works.bdabharatpur.org

# 8. Run first deploy from GitHub Actions → workflow_dispatch
```

---

## 6. App-level notes

- **Timezone**: Server is UTC. All `DateTime.UtcNow` in code. Use `.ToLocalTime()` carefully — server local = UTC. Display times in IST where needed via `TimeZoneInfo`.
- **Blazor SignalR**: Nginx must pass `Upgrade` / `Connection` WebSocket headers — already in `deploy/nginx-work-dashboard`.
- **Auth**: Cookie-based via `AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)`. Roles: `ADMIN`, `JEN`, `AEN`, `XEN`.
- **DB ENUM fix**: `engineers.role` column was expanded to `ENUM('ADMIN','JEN','AEN','XEN')` on dev DB on 2026-04-29. Run same ALTER on prod after first deploy if schema is pre-existing.

---

## 7. Services hub entry

Add to `services.bdabharatpur.org` static page:

```html
<a href="https://works.bdabharatpur.org">E-Works Dashboard</a>
```
