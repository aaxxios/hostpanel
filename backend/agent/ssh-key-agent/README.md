# hostpanel-ssh-key-agent

A small, dependency-free Python 3 helper that lets HostPanel add, list and revoke customers'
ED25519 public keys in their hosting accounts' `~/.ssh/authorized_keys`. It is installed once on
each managed server, by that server's administrator, with a one-command installer HostPanel
generates. Nothing in it is specific to one hosting company, network or server.

## Supported providers

| Profile | Status | Account checks on top of the generic ones |
|---|---|---|
| `cwp` (CentOS Web Panel) | Supported; adapter, unit tests and isolated lab tests | Account must exist in CWP (`root_cwp.user`, via `providers/cwp/account-check`); suspended (`usermod -L`) accounts cannot receive keys. |
| `generic-linux` | Agent profile; lab-tested. **Not selectable in HostPanel yet** — HostPanel has no server module for a plain Linux host. | passwd identity checks and the suspension lock only. |
| cPanel/WHM, DirectAdmin | **Not supported.** Extension point only. | A provider is a profile in `profiles/<name>.conf` plus, if it needs one, a root-owned helper in `providers/<name>/` that answers `account_check_command`; HostPanel maps the server module to the profile in `SshKeyProviderFactory.AgentProfileFor`. Neither exists for these panels. |

HostPanel only offers SSH key management on servers whose module maps to a profile (today: the
v7 hosting panel module → `cwp`). Every other server answers "not available for this type of
hosting" to customers.

## Setting up a server (HostPanel admin → Servers → *SSH*)

1. **Connection.** Enter where customers connect (SSH hostname and port) and where HostPanel
   reaches the agent (agent host, port, management user — `hpsshkeys` by default). Save.
2. **Install.** *Generate identity*: HostPanel creates an ED25519 key pair for this server only,
   stores the private key encrypted, and shows a one-command installer. Run it as root on the
   server. It downloads the agent bundle from your HostPanel, **stops unless the SHA256 matches the
   value shown in the authenticated admin panel**, unpacks to a fresh temporary directory and runs
   `install.sh --provider <profile> --binding <id> "<public key>"`. The installer prints the
   server's host key fingerprints.
3. **Host key.** *Read host key*: HostPanel shows the key the agent endpoint presents (without
   authenticating). Compare it with what the installer printed; if identical, *Use this
   fingerprint* and save. Connections presenting any other key are refused.
4. **Test.** The agent must answer **and** report the binding of this server record. An agent
   installed for another record is reported as `binding_mismatch`.
5. **Enable.** Refused by the API until a test with the saved settings succeeded, and refused if
   another enabled server already uses the same agent host and port. Changing the agent host,
   port, user, key or pinned fingerprint clears the test.

*Rotate identity* issues a new key (same binding) and switches customer access off until the
installer is run again. *Advanced* lets an administrator paste an existing private key instead.

## Network

HostPanel needs TCP access from its API host to the agent host and port. Any of these works:

* a private network or VPN between HostPanel and the server (recommended);
* the server's ordinary SSH port over the internet, ideally restricted by firewall to HostPanel's
  source address.

Either way the connection is authenticated by the per-server key, the host key is pinned, and the
management account can do nothing but run the agent. No VPN is required by the software.

## How HostPanel reaches the agent

```
HostPanel ──ssh (pinned host key, per-server ED25519 key)──▶ hpsshkeys@server
   authorized_keys: restrict,command="sudo -n /usr/local/sbin/hostpanel-ssh-key-agent" <key>
   sudoers:         hpsshkeys ALL=(root) NOPASSWD: /usr/local/sbin/hostpanel-ssh-key-agent ""
```

`restrict` removes pty, port/agent/X11 forwarding and user rc; the forced command ignores whatever
the client asks to run; sudo allows that exact path with no arguments. The request travels on
stdin as one JSON document; the agent never executes a shell or takes a path from the request.

## Contract (version 1)

| Request | Success response fields |
|---|---|
| `{"v":1,"op":"ping"}` | `agent_version`, `provider`, `binding` |
| `{"v":1,"op":"status","account":"u"}` | `shell_enabled`, `managed_keys`, `unmanaged_keys`, `reason` when disabled |
| `{"v":1,"op":"list","account":"u"}` | `keys: [{key_id, fingerprint}]`, `unmanaged_keys` |
| `{"v":1,"op":"add","account":"u","key_id":"<uuid>","public_key":"ssh-ed25519 …"}` | `key_id`, `fingerprint`, `changed` |
| `{"v":1,"op":"revoke","account":"u","key_id":"<uuid>"}` | `key_id`, `changed` |

Every response carries `"v":1` and `"ok"`. Refusals are `{"ok":false,"error":"<code>"}`:
`invalid_request`, `unsupported_version`, `unknown_operation`, `request_too_large`,
`invalid_account`, `account_not_found`, `account_not_eligible`, `account_suspended`,
`invalid_key`, `duplicate_key`, `key_id_conflict`, `too_many_keys`, `ssh_disabled`, `unsafe_path`,
`file_too_large`, `timeout`, `internal_error`. Unknown request fields are refused. `add` and
`revoke` are idempotent by `key_id`. Fields added since 1.0 (`provider`, `binding`, `reason`) are
additive; older HostPanels ignore them and older agents simply omit them.

## Safety properties

* **Account boundary** — strict name pattern; must exist in passwd; `uid >= min_uid`; not on
  `deny_accounts`; home exactly `<home_root>/<dir>`. The path comes from passwd, never from the
  request. Provider checks (panel account, suspension, group, opt-in list) run before anything is
  written; suspension and eligibility only gate *receiving* keys — `list` and `revoke` keep
  working, so access can always be removed.
* **Least privilege for file work** — the root parent validates, takes a per-account `flock`, then
  forks a child that drops to the account's uid/gid before touching any file.
* **Symlink / traversal** — `.ssh` and `authorized_keys` are opened with `O_NOFOLLOW` relative to
  directory descriptors; a foreign-owned or non-directory `.ssh` is `unsafe_path`.
* **Atomic writes** — `O_EXCL` temp file, `fsync`, `rename`, `fsync` of the directory.
* **Only its own keys** — managed lines are `ssh-ed25519 <blob> hostpanel:<key_id>`; keys added by
  other means are counted but never listed, changed or removed.
* **Timeouts** — 10 s for stdin, 60 s per request, so a silent client cannot pin a root process.
* **Audit** — every `add`/`revoke` is appended to the root-only audit log, with fingerprint, never
  key material.

## Per-server policy (`/etc/hostpanel-ssh-key-agent/agent.conf`)

| Setting | Effect |
|---|---|
| `provider`, `binding` | Reported by ping. Set by the installer. |
| `account_check_command` | Absolute path of a root-owned helper run as `helper <account>` (no shell, empty environment, 10 s timeout). Exit 0 = panel account, 1 = not one, anything else = fault. |
| `check_shadow_lock` | A locked password (`usermod -L`) makes the account `account_suspended`. `!!`/`*` alone is not a lock. |
| `ssh_group` | Only members may receive keys — set it when only one group may log in over SSH (e.g. a jailed-shell group). |
| `eligible_list` | Root-owned, not group/world-writable file, one account per line: an SSH opt-in list. |

## Lifecycle

| Task | Command (as root on the server, in the unpacked bundle) |
|---|---|
| Install / reinstall | the command HostPanel shows (or `./install.sh --provider P --binding ID "<key>"`) |
| Preview | add `--dry-run` |
| Upgrade | run the current installer command again; prints `Agent <old> -> <new>`, keeps `agent.conf`, updates the binding |
| Diagnose | `./install.sh --check` — user, lock, agent owner/mode, sudoers, forced-command key, config, self-test, host key |
| Remove / roll back | `./uninstall.sh [--dry-run] [--purge]` — management key first, then sudoers, user and agent; config kept unless `--purge`; audit log always kept |

Neither script touches sshd, the firewall, any VPN, Jailkit or customers' keys. **Before
uninstalling, revoke the server's managed keys from HostPanel**
(`POST /api/admin/servers/{id}/ssh-keys/revoke-all`); keys left behind keep working until removed
by hand — list them with `grep -l 'hostpanel:' /home/*/.ssh/authorized_keys`.

## What happens to keys over a service's life

Every key is pinned to the server and account it was issued on. Removals always go there, by the
key's own tag, so they can never touch a key HostPanel did not write — not even on a server whose
account has since been reused.

| Event | Keys on the server | In HostPanel |
|---|---|---|
| Hosting account suspended | Stay; the account cannot log in (locked, no shell) and cannot receive new keys | Customer cannot change keys while the service is suspended; unchanged on unsuspend |
| Service terminated | Revoked automatically on their server | `Revoked`; if the server did not confirm, `RevokeFailed` with the reason, visible to staff |
| Service moved to another server | Stay on the old server until revoked | Listed as from the previous server; the customer (or revoke-all on the old server) removes them there; nothing is ever sent to the new server for them |
| SSH key management switched off on the server | Stay | Customer can no longer add, but can still revoke |
| Agent uninstalled | Stay unless revoke-all ran first | Revokes fail (`RevokeFailed`) until reinstalled |
| Identity rotated | Unaffected | Customer access off until the installer runs with the new key |

## Tests

```bash
python3 -m unittest discover -s backend/agent/ssh-key-agent/tests
```

They run unprivileged against a temporary home directory (no root, no real accounts).
