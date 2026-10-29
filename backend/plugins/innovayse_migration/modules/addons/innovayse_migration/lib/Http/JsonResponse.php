<?php

declare(strict_types=1);

namespace HostpanelMigration\Http;

/**
 * One answer of the pull API: a status and the body that is written as JSON.
 */
final class JsonResponse
{
    /** The wire member every refusal carries its sentence in. */
    private const ERROR_MEMBER = 'error';

    /**
     * Creates an answer.
     *
     * @param HttpStatus           $status The response status.
     * @param array<string, mixed> $body   The body, written as a JSON object.
     */
    public function __construct(
        /** The response status. */
        public readonly HttpStatus $status,
        /** @var array<string, mixed> The body, written as a JSON object. */
        public readonly array $body,
    ) {
    }

    /**
     * A successful answer.
     *
     * @param array<string, mixed> $body The operation's answer.
     *
     * @return self The answer with status 200.
     */
    public static function ok(array $body): self
    {
        return new self(HttpStatus::Ok, $body);
    }

    /**
     * A refusal: `{"error": "<sentence>"}` with the refusal's status.
     *
     * @param ApiError $error The refusal.
     *
     * @return self The answer.
     */
    public static function failure(ApiError $error): self
    {
        return new self($error->status(), [self::ERROR_MEMBER => $error->value]);
    }
}
