<?php

declare(strict_types=1);

namespace HostpanelMigration\Export;

use HostpanelMigration\Database\Dataset;

/**
 * Exports one record kind: names the dataset its rows come from and maps each row to the wire
 * record hostpanel reads.
 */
interface Exporter
{
    /**
     * The dataset the rows are read from.
     *
     * @return Dataset The dataset.
     */
    public function dataset(): Dataset;

    /**
     * Maps one row to its wire record.
     *
     * @param object $row The database row.
     *
     * @return array<string, mixed>|null The wire record, or null when the row is not sent.
     */
    public function map(object $row): ?array;
}
