<?php

declare(strict_types=1);

namespace HostpanelMigration;

use HostpanelMigration\Auth\ApiKeyAuthenticator;
use HostpanelMigration\Auth\AttemptLog;
use HostpanelMigration\Auth\AttemptLogUnavailable;
use HostpanelMigration\Auth\AuthResult;
use HostpanelMigration\Auth\FailedAttemptThrottle;
use HostpanelMigration\Database\SourceDatabase;
use HostpanelMigration\Export\DatasetExport;
use HostpanelMigration\Export\TotalsReport;
use HostpanelMigration\Http\ApiError;
use HostpanelMigration\Http\JsonResponse;
use HostpanelMigration\Http\Request;
use HostpanelMigration\Paging\KeysetPager;

/**
 * The pull API's flow for one request: authenticate, then dispatch the action. The key is checked
 * before the action, so an unknown action with a wrong key answers 401, not 400.
 */
final class ApiKernel
{
    /**
     * Creates the kernel.
     *
     * @param ApiKeyAuthenticator $authenticator Judges the presented key.
     * @param Router              $router        Runs the action.
     */
    public function __construct(
        /** Judges the presented key. */
        private readonly ApiKeyAuthenticator $authenticator,
        /** Runs the action. */
        private readonly Router $router,
    ) {
    }

    /**
     * Builds the kernel and everything it uses.
     *
     * @param SourceDatabase $database The source database.
     * @param AttemptLog     $attempts Where failed key attempts are kept.
     *
     * @return self The kernel.
     */
    public static function create(SourceDatabase $database, AttemptLog $attempts): self
    {
        $pager = new KeysetPager($database);

        return new self(
            new ApiKeyAuthenticator($database->migrationKey(), new FailedAttemptThrottle($attempts)),
            new Router($database, new TotalsReport($pager), new DatasetExport($pager)),
        );
    }

    /**
     * Answers one request.
     *
     * @param Request $request       The request.
     * @param string  $clientAddress The caller's address, as the connection reports it.
     * @param int     $now           The current Unix time.
     *
     * @return JsonResponse The answer.
     */
    public function handle(Request $request, string $clientAddress, int $now): JsonResponse
    {
        try {
            $result = $this->authenticator->authenticate($request->key, $clientAddress, $now);
        } catch (AttemptLogUnavailable) {
            return JsonResponse::failure(ApiError::AttemptLogUnavailable);
        }

        if ($result === AuthResult::Throttled) {
            return JsonResponse::failure(ApiError::TooManyAttempts);
        }

        if ($result === AuthResult::Rejected) {
            return JsonResponse::failure(ApiError::InvalidKey);
        }

        if ($request->action === null) {
            return JsonResponse::failure(ApiError::UnknownAction);
        }

        return JsonResponse::ok($this->router->dispatch($request->action, $request->paging));
    }
}
