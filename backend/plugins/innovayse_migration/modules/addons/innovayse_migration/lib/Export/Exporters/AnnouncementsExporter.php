<?php

declare(strict_types=1);

namespace HostpanelMigration\Export\Exporters;

use HostpanelMigration\Database\Dataset;
use HostpanelMigration\Export\Exporter;
use HostpanelMigration\Mapping\IsoDate;

/**
 * Exports announcements (`announcements`).
 */
final class AnnouncementsExporter implements Exporter
{
    /**
     * {@inheritDoc}
     */
    public function dataset(): Dataset
    {
        return Dataset::Announcements;
    }

    /**
     * {@inheritDoc}
     */
    public function map(object $row): ?array
    {
        return [
            'title'     => $row->title ?? '',
            'content'   => $row->announcement ?? '',
            'published' => (bool) ($row->published ?? false),
            'createdAt' => IsoDate::from($row->date ?? null),
        ];
    }
}
