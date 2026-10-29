<?php

declare(strict_types=1);

namespace HostpanelMigration\Export\Exporters;

use HostpanelMigration\Database\Dataset;
use HostpanelMigration\Export\Exporter;
use HostpanelMigration\Mapping\OptionalId;

/**
 * Exports knowledgebase articles (`knowledgebase`).
 */
final class KnowledgebaseExporter implements Exporter
{
    /** The title of an article that names none. */
    private const DEFAULT_TITLE = 'Article';

    /**
     * {@inheritDoc}
     */
    public function dataset(): Dataset
    {
        return Dataset::Knowledgebase;
    }

    /**
     * {@inheritDoc}
     */
    public function map(object $row): ?array
    {
        return [
            'title'      => $row->title ?? self::DEFAULT_TITLE,
            'content'    => $row->article ?? '',
            'categoryId' => OptionalId::from($row->catid),
            'published'  => (bool) ($row->active ?? false),
        ];
    }
}
