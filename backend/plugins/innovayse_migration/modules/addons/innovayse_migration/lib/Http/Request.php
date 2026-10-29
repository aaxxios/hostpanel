<?php

declare(strict_types=1);

namespace HostpanelMigration\Http;

use HostpanelMigration\Paging\Paging;

/**
 * One call to the pull API, read from its JSON body: the key, the action and the paging window.
 * A body that is not a JSON object reads as an empty one, so it fails the key check.
 */
final class Request
{
    /**
     * Creates a request.
     *
     * @param string      $key    The presented key, trimmed.
     * @param Action|null $action The action, or null when the caller sent one the API does not know.
     * @param Paging      $paging The paging window.
     */
    private function __construct(
        /** The presented key, trimmed; empty when none was sent. */
        public readonly string $key,
        /** The action, or null when the caller sent one the API does not know. */
        public readonly ?Action $action,
        /** The paging window; only dataset actions read it. */
        public readonly Paging $paging,
    ) {
    }

    /**
     * Reads a request from the raw body.
     *
     * @param string $json The raw request body.
     *
     * @return self The request.
     */
    public static function fromJson(string $json): self
    {
        $body = json_decode($json, true);

        return self::fromBody(is_array($body) ? $body : []);
    }

    /**
     * Reads a request from a decoded body.
     *
     * @param array<mixed> $body The decoded request body.
     *
     * @return self The request.
     */
    public static function fromBody(array $body): self
    {
        $key = $body['key'] ?? '';
        $action = $body['action'] ?? null;

        return new self(
            is_scalar($key) ? trim((string) $key) : '',
            is_string($action) ? Action::tryFrom($action) : null,
            Paging::fromBody($body),
        );
    }
}
