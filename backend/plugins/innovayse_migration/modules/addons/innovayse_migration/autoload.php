<?php

declare(strict_types=1);

/**
 * Class loader for the addon's own classes: the namespace HostpanelMigration\ maps to lib/
 * (PSR-4). Both entry points, the addon module file and api.php, require it before they use a
 * class. The source billing system has no loader for this namespace, so the addon brings its own.
 */

spl_autoload_register(static function (string $class): void {
    $prefix = 'HostpanelMigration\\';
    if (!str_starts_with($class, $prefix)) {
        return;
    }

    $file = __DIR__ . '/lib/' . str_replace('\\', '/', substr($class, strlen($prefix))) . '.php';
    if (is_file($file)) {
        require $file;
    }
});
