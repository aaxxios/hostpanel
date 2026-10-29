<?php

declare(strict_types=1);

/**
 * Hostpanel migration addon module for the source billing system.
 *
 * Exposes a pull API (api.php) so hostpanel can fetch data from this billing system. The source
 * system calls the functions below by name; each one builds the addon's object for it and
 * delegates.
 *
 * Installation:
 *   1. Upload this folder to /modules/addons/innovayse_migration/
 *   2. Activate it in the billing system's admin area under the addon modules setup
 *   3. Enter the Migration Key from hostpanel in the module settings
 *
 * The folder and module id stay `innovayse_migration`: existing installations store their key
 * under that id.
 */

use HostpanelMigration\Addon\AddonModule;
use HostpanelMigration\Addon\AdminPage;

// The source billing system defines this constant before it loads a module.
if (!defined('WHMCS')) {
    die('This file cannot be accessed directly');
}

require_once __DIR__ . '/autoload.php';

/**
 * The module's configuration: name, version and the migration key setting.
 *
 * @return array<string, mixed> The configuration in the source system's shape.
 */
function innovayse_migration_config(): array
{
    return (new AddonModule())->config();
}

/**
 * Called when an administrator activates the module.
 *
 * @return array<string, string> The answer in the source system's shape.
 */
function innovayse_migration_activate(): array
{
    return (new AddonModule())->activate();
}

/**
 * Called when an administrator deactivates the module.
 *
 * @return array<string, string> The answer in the source system's shape.
 */
function innovayse_migration_deactivate(): array
{
    return (new AddonModule())->deactivate();
}

/**
 * Renders the module's admin page.
 *
 * @param array<string, mixed> $vars The module variables the source system passes.
 */
function innovayse_migration_output(array $vars): void
{
    echo (new AdminPage())->render($vars, $_SERVER);
}
