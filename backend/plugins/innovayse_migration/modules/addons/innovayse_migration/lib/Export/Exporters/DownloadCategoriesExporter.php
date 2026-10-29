<?php

declare(strict_types=1);

namespace HostpanelMigration\Export\Exporters;

use HostpanelMigration\Database\Dataset;
use HostpanelMigration\Export\Exporter;
use HostpanelMigration\Mapping\OptionalId;

/**
 * Exports download categories (`download_categories`).
 */
final class DownloadCategoriesExporter implements Exporter
{
    /**
     * {@inheritDoc}
     */
    public function dataset(): Dataset
    {
        return Dataset::DownloadCategories;
    }

    /**
     * {@inheritDoc}
     */
    public function map(object $row): ?array
    {
        return [
            'id'          => $row->id,
            'name'        => $row->name ?? '',
            'description' => $row->description ?: null,
            'hidden'      => (bool) ($row->hidden ?? false),
            'parentId'    => OptionalId::from($row->parentid),
        ];
    }
}
