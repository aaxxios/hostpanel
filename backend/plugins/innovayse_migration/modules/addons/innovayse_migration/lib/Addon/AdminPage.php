<?php

declare(strict_types=1);

namespace HostpanelMigration\Addon;

use HostpanelMigration\Module;

/**
 * The module's page in the source system's admin area: whether a key is configured, and the
 * source URL to enter in hostpanel.
 */
final class AdminPage
{
    /**
     * Renders the page.
     *
     * @param array<string, mixed> $vars   The module variables the source system passes, the key setting among them.
     * @param array<string, mixed> $server The server variables of the current request, for the page's own address.
     *
     * @return string The page's HTML.
     */
    public function render(array $vars, array $server): string
    {
        $status = empty($vars[Module::KEY_SETTING]) ? self::notConfigured() : self::ready();
        $endpoint = htmlspecialchars(self::apiEndpoint($server), ENT_QUOTES | ENT_SUBSTITUTE, 'UTF-8');

        return <<<HTML
            <div style="font-family:sans-serif;max-width:620px;padding:20px">

                <h2 style="margin-top:0">Hostpanel Migration</h2>

                {$status}

                <div style="background:#f8f9fa;border:1px solid #dee2e6;border-radius:8px;padding:20px">
                    <p style="margin:0 0 6px 0;font-weight:600;font-size:14px">Source URL</p>
                    <p style="margin:0 0 10px 0;color:#666;font-size:13px">
                        Enter this URL as the <strong>Source URL</strong> when creating a migration in hostpanel:
                    </p>
                    <code style="display:block;background:#e9ecef;padding:8px 12px;border-radius:6px;
                                  font-size:13px;word-break:break-all">
                        {$endpoint}
                    </code>
                </div>

            </div>
            HTML;
    }

    /**
     * The address the pull API answers on, built from the current request's scheme and host.
     *
     * @param array<string, mixed> $server The server variables of the current request.
     *
     * @return string The address, not yet escaped.
     */
    private static function apiEndpoint(array $server): string
    {
        $https = $server['HTTPS'] ?? '';
        $scheme = (!empty($https) && $https !== 'off') ? 'https' : 'http';
        $host = is_string($server['HTTP_HOST'] ?? null) ? $server['HTTP_HOST'] : '';

        return $scheme . '://' . $host . Module::API_PATH;
    }

    /**
     * The notice shown while no key is configured.
     *
     * @return string The notice's HTML.
     */
    private static function notConfigured(): string
    {
        return <<<HTML
            <div style="background:#fef2f2;border:1px solid #fca5a5;border-radius:8px;
                        padding:14px 18px;color:#991b1b;font-weight:600;margin-bottom:20px">
                &#9888; Not configured. Enter your Migration Key in the settings above and save.
            </div>
            HTML;
    }

    /**
     * The notice shown once a key is configured.
     *
     * @return string The notice's HTML.
     */
    private static function ready(): string
    {
        return <<<HTML
            <div style="background:#f0fdf4;border:1px solid #86efac;border-radius:8px;
                        padding:14px 18px;color:#166534;font-weight:600;margin-bottom:20px">
                &#10003; Plugin is ready. Hostpanel can now connect and pull your data.
            </div>
            HTML;
    }
}
