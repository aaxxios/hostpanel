<?php

declare(strict_types=1);

/**
 * Migration pull API, the front controller.
 *
 * Called by hostpanel to pull the source billing system's data one page at a time: a JSON POST
 * carrying `key`, `action` and, for a dataset, `page`, `perPage` and optionally `afterId`. It
 * bootstraps the source system, authenticates the key, dispatches the one action and writes the
 * JSON answer. The flow itself lives in lib/ (HostpanelMigration\ApiKernel).
 */

use HostpanelMigration\ApiKernel;
use HostpanelMigration\Auth\FileAttemptLog;
use HostpanelMigration\Database\SourceDatabase;
use HostpanelMigration\Http\ApiError;
use HostpanelMigration\Http\JsonResponder;
use HostpanelMigration\Http\JsonResponse;
use HostpanelMigration\Http\Request;

require_once __DIR__ . '/autoload.php';

$responder = new JsonResponder();

// The source system's root is three levels up: innovayse_migration/ -> addons/ -> modules/ -> root.
$sourceRoot = dirname(__DIR__, 3);
if (!is_file($sourceRoot . '/init.php')) {
    $responder->send(JsonResponse::failure(ApiError::BootstrapMissing));
    exit;
}

define('CLIENTAREA', true);
require $sourceRoot . '/init.php';

try {
    $kernel = ApiKernel::create(
        SourceDatabase::fromSourceSystem(),
        new FileAttemptLog(sys_get_temp_dir() . DIRECTORY_SEPARATOR . 'hostpanel-migration-attempts'),
    );
    $response = $kernel->handle(
        Request::fromJson((string) file_get_contents('php://input')),
        (string) ($_SERVER['REMOTE_ADDR'] ?? ''),
        time(),
    );
} catch (Throwable) {
    // Nothing of the failure reaches the caller: its text can name tables or carry row data.
    $response = JsonResponse::failure(ApiError::ExportFailed);
}

$responder->send($response);
