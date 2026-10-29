<?php

declare(strict_types=1);

namespace HostpanelMigration\Export\Exporters;

use HostpanelMigration\Database\Dataset;
use HostpanelMigration\Export\Exporter;
use HostpanelMigration\Export\SourceDefaults;
use HostpanelMigration\Mapping\IsoDate;

/**
 * Exports network issues (`network_issues`).
 */
final class NetworkIssuesExporter implements Exporter
{
    /** The type of an issue that names none. */
    private const DEFAULT_TYPE = 'Outage';

    /**
     * {@inheritDoc}
     */
    public function dataset(): Dataset
    {
        return Dataset::NetworkIssues;
    }

    /**
     * {@inheritDoc}
     */
    public function map(object $row): ?array
    {
        return [
            'title'       => $row->title ?? '',
            'type'        => $row->type ?? self::DEFAULT_TYPE,
            'status'      => $row->status ?? SourceDefaults::STATUS,
            'priority'    => $row->priority ?? SourceDefaults::PRIORITY,
            'description' => $row->body ?? '',
            'startDate'   => IsoDate::from($row->date ?? null),
            'endDate'     => IsoDate::from($row->enddate ?? null),
        ];
    }
}
