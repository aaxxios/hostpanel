<?php

declare(strict_types=1);

namespace HostpanelMigration\Export;

use HostpanelMigration\Database\SourceDatabase;
use HostpanelMigration\Database\Table;

/**
 * An exporter whose rows belong to a client through their client id column. Every record it
 * sends names its client by email, and a row whose client cannot be found is not sent.
 */
abstract class ClientOwnedExporter implements Exporter
{
    /**
     * Creates the exporter.
     *
     * @param SourceDatabase $database The source database, for the client and any related rows.
     */
    public function __construct(
        /** The source database, for the client and any related rows. */
        protected readonly SourceDatabase $database,
    ) {
    }

    /**
     * Resolves the row's client and maps the row.
     *
     * @param object $row The database row.
     *
     * @return array<string, mixed>|null The wire record, or null when the client is gone or the row is not sent.
     */
    final public function map(object $row): ?array
    {
        $clientEmail = $this->database->clientEmail($row->{Table::CLIENT_ID} ?? null);
        if ($clientEmail === null) {
            return null;
        }

        return $this->record($row, $clientEmail);
    }

    /**
     * Maps a row whose client exists.
     *
     * @param object $row         The database row.
     * @param string $clientEmail The email of the row's client.
     *
     * @return array<string, mixed>|null The wire record, or null when the row is not sent.
     */
    abstract protected function record(object $row, string $clientEmail): ?array;
}
