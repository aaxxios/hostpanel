# hostpanel-ssh-key-agent

A small, dependency-free Python 3 helper that HostPanel uses to add, list and revoke customers'
ED25519 public keys in their hosting accounts' `~/.ssh/authorized_keys`. It is installed
independently on **each** managed CWP server. CWP itself is not modified and its user-panel
authentication is not used.

## How HostPanel reaches it

```
HostPanel ──ssh (pinned host key, ED25519 client key)──▶ hpsshkeys@server
   authorized_keys: restrict,command="sudo -n /usr/local/sbin/hostpanel-ssh-key-agent" <key>
   sudoers:         hpsshkeys ALL=(root) NOPASSWD: /usr/local/sbin/hostpanel-ssh-key-agent ""
```

* The management key can run **only** the agent: `restrict` removes pty, port/agent/X11
  forwarding; the forced command ignores whatever command the client asks for.
* sudo allows that exact path with **no arguments**. The request travels on stdin as one JSON
  document; the agent never executes a shell or takes a path from the request.
* No new port is opened — it uses the server's existing sshd.

## Contract (version 1)

| Request | Success response fields |
|---|---|
| `{"v":1,"op":"ping"}` | `agent_version` |
| `{"v":1,"op":"status","account":"u"}` | `shell_enabled`, `managed_keys`, `unmanaged_keys` |
| `{"v":1,"op":"list","account":"u"}` | `keys: [{key_id, fingerprint}]`, `unmanaged_keys` |
| `{"v":1,"op":"add","account":"u","key_id":"<uuid>","public_key":"ssh-ed25519 …"}` | `key_id`, `fingerprint`, `changed` |
| `{"v":1,"op":"revoke","account":"u","key_id":"<uuid>"}` | `key_id`, `changed` |

Every response carries `"v":1` and `"ok"`. Refusals are `{"ok":false,"error":"<code>"}` with one
of: `invalid_request`, `unsupported_version`, `unknown_operation`, `request_too_large`,
`invalid_account`, `account_not_found`, `account_not_eligible`, `invalid_key`, `duplicate_key`,
`key_id_conflict`, `too_many_keys`, `ssh_disabled`, `unsafe_path`, `file_too_large`, `timeout`,
`internal_error`. A client that sends nothing within 10 s, or a request that takes over 60 s
(lock wait included), is answered `timeout` so no root process stays pinned. Unknown fields are refused, not ignored. `add` and `revoke` are idempotent by
`key_id`, so HostPanel can safely retry after a timeout.

## Safety properties

* **Account boundary** — strict name pattern; must exist in passwd; `uid >= min_uid`; not on
  the deny list; home must be exactly `<home_root>/<dir>`. The path comes from passwd, never from
  the request.
* **Least privilege for file work** — the root parent validates, takes a per-account `flock`,
  then forks a child that drops to the account's own uid/gid (no supplementary groups) before
  touching any file. A planted symlink can reach nothing the customer could not already write,
  and files get the correct owner by construction.
* **Symlink / traversal** — `.ssh` and `authorized_keys` are opened with `O_NOFOLLOW` relative
  to directory descriptors; a non-directory or foreign-owned `.ssh` is `unsafe_path`.
* **Atomic writes** — `O_EXCL` temp file in `.ssh`, `fsync`, `rename`, `fsync` of the directory.
  `.ssh` is `0700`, `authorized_keys` `0600`.
* **Only its own keys** — managed lines are `ssh-ed25519 <blob> hostpanel:<key_id>`; the
  customer's comment is discarded and the blob re-encoded. Keys added by other means are counted
  but never listed, changed or removed.
* **Shell gate** — `add` is refused (`ssh_disabled`) when the account has no login shell; SSH is
  never enabled by the agent. `revoke` always works.
* **Audit** — every `add`/`revoke` is appended to `audit_log` (timestamp, op, account, key id,
  fingerprint, outcome) by the root parent, successful or not. No key material is logged.

## Per-server policy (agent.conf)

Identity checks always run: strict name pattern, passwd entry, `uid >= min_uid`, not on
`deny_accounts`, home exactly under `home_root`. These optional checks are switched on per server:

| Setting | Effect |
|---|---|
| `account_check_command` | Absolute path of a root-owned helper run as `helper <account>` (no shell, empty environment, 10 s timeout). Exit 0 = panel account, 1 = not one (`account_not_found`), anything else = fault. `cwp/cwp-account-check` asks CWP's `root_cwp.user`. |
| `check_shadow_lock` | A locked password (`usermod -L`, how CWP suspends) makes the account `account_suspended`. `!!`/`*` (no password) is not a lock. |
| `ssh_group` | Only members of this group may receive keys (`account_not_eligible` otherwise). |
| `eligible_list` | Root-owned, not group/world-writable file, one account per line: an opt-in list. |

Suspension, group, list and login shell only decide whether an account may **receive** keys;
`list` and `revoke` keep working so access can always be removed. `status` answers
`shell_enabled: false` plus a `reason`.

`cwp/agent.conf.srv4` is the configuration for a CWP server with Jailkit customer SSH
(`sshd-cust`, `AllowGroups sftp-ssh sftp-only`): `ssh_group = sftp-ssh`, the existing opt-in list
`/usr/local/etc/innovayse-ssh-eligible.conf`, the CWP account check and the shadow lock check.
Keys written by other tools (for example `manage-ssh-key.sh`) are never listed or removed.

## Install on a server

```bash
scp -r ssh-key-agent root@<server>:/root/
ssh root@<server> '/root/ssh-key-agent/install.sh "ssh-ed25519 AAAA… hostpanel-<server>"'
```

For a CWP server: `install.sh --with-cwp-check --config cwp/agent.conf.srv4 "<key>"`. Every
step is idempotent and `--dry-run` prints what would change. `uninstall.sh` (also `--dry-run`)
removes the management key first, then sudoers, the user and the agent; the config is kept unless
`--purge`, the audit log always. Customers' keys are not touched by either script.

The script prints the server's host key fingerprints and runs a `ping` self-test. Use a
**separate** management key per server. Then configure the server in HostPanel — see *Adding a CWP server* below.

## Tests

```bash
python3 -m unittest discover -s ssh-key-agent/tests
```

They run unprivileged against a temporary home directory (no root, no real accounts).

## HostPanel side

Customers manage ED25519 public keys for their hosting services under
**My Services → (service) → SSH Keys**. This page is for operators: how the feature resolves a
request, what is stored, and how to switch it on for a server.

### Request path

```
customer ─▶ portal BFF (/api/portal/client/services/{id}/ssh…) ─▶ API (/api/me/services/{id}/ssh…)
   IServiceOwnership: the caller must own service {id}            (404 SERVICE_NOT_FOUND otherwise)
   SshServiceContextResolver: service ─▶ Status=Active ─▶ ServerId ─▶ Server (not disabled)
        ─▶ ISshKeyProviderFactory.Supports(Module) ─▶ ServerSshSettings (enabled, complete)
        ─▶ provider bound to that server ─▶ account = service.ProvisioningRef on that server
   AgentSshKeyProvider ─ssh─▶ hostpanel-ssh-key-agent on that server only
```

The browser sends only the service id (and, for revoke, the key's UUID). Server, hostname, port,
Linux account and path are all resolved on the backend. A provider refuses any account whose
server id is not its own, so a routing bug cannot reach another machine.

| Endpoint | Purpose |
|---|---|
| `GET /api/me/services/{id}/ssh` | access state, hostname, port, username, connect command |
| `GET /api/me/services/{id}/ssh/keys` | keys with fingerprint, created date, sync state, stale flag |
| `POST /api/me/services/{id}/ssh/keys` | `{ label, publicKey }` → 201 |
| `DELETE /api/me/services/{id}/ssh/keys/{keyId}` | 204 |
| `GET/PUT /api/admin/servers/{id}/ssh-settings` | per-server configuration (admin) |
| `POST /api/admin/servers/{id}/ssh-settings/test` | ping the agent, record health (admin) |

Reads use the `upstream` rate-limit policy, add/revoke the `strict` one. Refusals are the usual `ErrorResponse` (`type` = code, `detail` = localised sentence):
`SSH_KEY_INVALID` / `SSH_PRIVATE_KEY_REJECTED` (400), `SSH_KEY_ALREADY_INSTALLED` /
`SSH_KEY_LIMIT_REACHED` / `SSH_KEY_STALE` / `SSH_ACCESS_UNAVAILABLE` (409), `SSH_KEY_NOT_FOUND` (404),
`SSH_SERVER_UNAVAILABLE` (503); admin: `SSH_MODULE_NOT_SUPPORTED`, `SSH_SETTINGS_INCOMPLETE` (400).
Reads require `ViewProductsServices`, add/revoke `ViewModifyPasswords` (SSH keys are credentials).

### Source of truth and synchronisation

The server's `authorized_keys` decides who can log in. HostPanel's `SshKeys` table records what
was requested and what the server last **confirmed**:

* `add` saves the row as `Pending` first, then calls the agent: confirmed → `Installed`; a
  definite refusal (duplicate, shell disabled, …) → row deleted, nothing was written; a timeout or
  unreachable server → `InstallFailed` (outcome unknown, kept so it can be revoked).
* `revoke` → `Revoked` only on confirmation; otherwise `RevokeFailed` and the key stays listed.
* Listing asks the server and marks each key `presentOnServer`, so drift is visible.
* Agent `add`/`revoke` are idempotent by key id, so retries are safe.

### Server migration

Each key is **pinned** to the `ServerId` and account it was issued on. If an administrator moves
the service to another server or account, old keys show as stale, cannot be revoked from the
portal (`SSH_KEY_STALE`), and **no call is sent to either server** for them — the old server may
now host someone else under the same username. Remove them from the old server by hand (the agent
`revoke` op with the key id), then mark them in the database. New keys go to the new server.

### Adding a CWP server (second, third, …)

Nothing is hard-coded; every server is one row in `Servers` plus one in `ServerSshSettings`.

1. **Generate a management key for this server** (on a trusted machine, never reuse across servers):
   `ssh-keygen -t ed25519 -N "" -C hostpanel-<server> -f hp-<server>`
2. **Install the agent on the server** (as root):
   `./ssh-key-agent/install.sh "$(cat hp-<server>.pub)"` — note the printed `SHA256:` host key
   fingerprint and check the `ping` self-test answers `ok`.
3. **Configure it in HostPanel** (admin API; the server must already exist with module `Cwp7`):

   ```bash
   curl -X PUT https://<hostpanel>/api/admin/servers/<id>/ssh-settings \
     -H "Authorization: Bearer $ADMIN_TOKEN" -H 'Content-Type: application/json' \
     -d @- <<EOF
   { "isEnabled": false,
     "sshHostname": "<hostname customers ssh to>", "sshPort": 22,
     "agentHost": "<hostname HostPanel reaches>", "agentPort": 22, "agentUsername": "hpsshkeys",
     "agentPrivateKey": "$(sed ':a;N;$!ba;s/\n/\\n/g' hp-<server>)",
     "agentHostKeyFingerprint": "SHA256:…" }
   EOF
   ```

   The private key is encrypted at rest with `EncryptionKey` and never returned.
4. **Test**: `POST /api/admin/servers/<id>/ssh-settings/test` must answer `isHealthy: true`.
5. **Enable**: repeat step 3 with `"isEnabled": true` (the private key may be omitted to keep it).
6. Delete `hp-<server>` from the machine you generated it on.

Customers on servers without enabled settings see "not enabled on this server"; other modules
(cPanel, DirectAdmin, …) see "not available for this type of hosting" until an
`ISshKeyProvider` (`Application/Common/Interfaces/`) is added for them in
`Infrastructure/Integrations/SshAgent/SshKeyProviderFactory.cs`. Enabling a server does **not**
enable SSH for any account: an account without a login shell is reported as `ShellDisabled` and
cannot receive keys.
