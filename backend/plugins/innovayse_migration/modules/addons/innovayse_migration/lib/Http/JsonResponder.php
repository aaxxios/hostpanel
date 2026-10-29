<?php

declare(strict_types=1);

namespace HostpanelMigration\Http;

use JsonException;

/**
 * Writes an answer to the client: the status, the JSON headers and the body.
 *
 * The body is encoded with PHP's default flags, so slashes and non-ASCII characters are escaped
 * exactly as the API has always written them. An answer that cannot be encoded (text that is not
 * valid UTF-8 in a source row) is replaced by the export failure, never by an empty 200.
 */
final class JsonResponder
{
    /**
     * Sends the answer.
     *
     * @param JsonResponse $response The answer to write.
     */
    public function send(JsonResponse $response): void
    {
        try {
            $json = json_encode($response->body, JSON_THROW_ON_ERROR);
        } catch (JsonException) {
            $response = JsonResponse::failure(ApiError::ExportFailed);
            $json = json_encode($response->body, JSON_THROW_ON_ERROR);
        }

        http_response_code($response->status->value);
        header('Content-Type: application/json');
        header('X-Content-Type-Options: nosniff');
        echo $json;
    }
}
