<?php

declare(strict_types=1);

namespace HostpanelMigration\Http;

/**
 * Every refusal the pull API can answer, with its fixed sentence. A sentence never carries caller
 * input or an exception's text.
 */
enum ApiError: string
{
    /** The source system's bootstrap file is not where the addon expects it. */
    case BootstrapMissing = 'Source system bootstrap not found.';

    /** The key is missing, the module has no key configured, or the two differ. */
    case InvalidKey = 'Invalid migration key.';

    /** Too many wrong keys from this address within the throttle window. */
    case TooManyAttempts = 'Too many failed attempts.';

    /** The failed attempt log cannot be read or written. */
    case AttemptLogUnavailable = 'Attempt log unavailable.';

    /** The action is not one the API knows; it is not echoed back. */
    case UnknownAction = 'Unknown action.';

    /** The operation failed on the source side. */
    case ExportFailed = 'Export failed.';

    /**
     * The status this refusal is answered with.
     *
     * @return HttpStatus The response status.
     */
    public function status(): HttpStatus
    {
        return match ($this) {
            self::InvalidKey => HttpStatus::Unauthorized,
            self::TooManyAttempts => HttpStatus::TooManyRequests,
            self::AttemptLogUnavailable => HttpStatus::ServiceUnavailable,
            self::UnknownAction => HttpStatus::BadRequest,
            self::BootstrapMissing, self::ExportFailed => HttpStatus::InternalServerError,
        };
    }
}
