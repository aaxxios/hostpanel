<?php

declare(strict_types=1);

namespace HostpanelMigration\Export\Exporters;

use HostpanelMigration\Database\Dataset;
use HostpanelMigration\Export\Exporter;

/**
 * Exports product groups (`product_groups`).
 */
final class ProductGroupsExporter implements Exporter
{
    /**
     * {@inheritDoc}
     */
    public function dataset(): Dataset
    {
        return Dataset::ProductGroups;
    }

    /**
     * {@inheritDoc}
     */
    public function map(object $row): ?array
    {
        return [
            'id'          => $row->id,
            'name'        => $row->name,
            'description' => $row->description ?: null,
        ];
    }
}
