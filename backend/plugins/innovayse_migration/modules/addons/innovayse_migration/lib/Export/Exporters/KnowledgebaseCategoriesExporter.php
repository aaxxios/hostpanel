<?php

declare(strict_types=1);

namespace HostpanelMigration\Export\Exporters;

use HostpanelMigration\Database\Dataset;
use HostpanelMigration\Export\Exporter;
use HostpanelMigration\Mapping\OptionalId;

/**
 * Exports knowledgebase categories (`knowledgebase_categories`).
 */
final class KnowledgebaseCategoriesExporter implements Exporter
{
    /**
     * {@inheritDoc}
     */
    public function dataset(): Dataset
    {
        return Dataset::KnowledgebaseCategories;
    }

    /**
     * {@inheritDoc}
     */
    public function map(object $row): ?array
    {
        return [
            'id'          => $row->id,
            'name'        => $row->name,
            'description' => $row->description ?? '',
            'parentId'    => OptionalId::from($row->parentid),
        ];
    }
}
