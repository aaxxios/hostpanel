<?php

declare(strict_types=1);

namespace HostpanelMigration\Export\Exporters;

use HostpanelMigration\Database\Dataset;
use HostpanelMigration\Export\Exporter;
use HostpanelMigration\Mapping\OptionalId;

/**
 * Exports downloads (`downloads`).
 */
final class DownloadsExporter implements Exporter
{
    /** The type of a download that names none. */
    private const DEFAULT_TYPE = 'file';

    /**
     * {@inheritDoc}
     */
    public function dataset(): Dataset
    {
        return Dataset::Downloads;
    }

    /**
     * {@inheritDoc}
     */
    public function map(object $row): ?array
    {
        return [
            'title'       => $row->name ?? '',
            'description' => $row->description ?: null,
            'type'        => $row->type ?? self::DEFAULT_TYPE,
            'filename'    => $row->filename ?? '',
            'categoryId'  => OptionalId::from($row->catid),
            'clientsOnly' => (bool) ($row->clientsonly ?? false),
            'hidden'      => (bool) ($row->hidden ?? false),
        ];
    }
}
