<?php

declare(strict_types=1);

namespace HostpanelMigration\Http;

/**
 * The HTTP statuses the pull API answers with.
 */
enum HttpStatus: int
{
    /** The operation ran; the body is its answer. */
    case Ok = 200;

    /** The action is not one the API knows. */
    case BadRequest = 400;

    /** The key is missing, unconfigured or wrong. */
    case Unauthorized = 401;

    /** Too many wrong keys from this address within the throttle window. */
    case TooManyRequests = 429;

    /** The source system could not be loaded, or the operation failed. */
    case InternalServerError = 500;

    /** The failed attempt log cannot be read or written, so no key is checked. */
    case ServiceUnavailable = 503;
}
