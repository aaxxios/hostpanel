<?php

declare(strict_types=1);

namespace HostpanelMigration\Export;

use HostpanelMigration\Paging\KeysetPager;
use HostpanelMigration\Paging\Paging;

/**
 * Answers a dataset action: reads one page through the pager and maps its rows with the action's
 * exporter. The answer is `{"items": [...], "totalPages": n, "nextAfterId": id|null}`.
 */
final class DatasetExport
{
    /**
     * Creates the export.
     *
     * @param KeysetPager $pager Reads the page and counts the dataset.
     */
    public function __construct(
        /** Reads the page and counts the dataset. */
        private readonly KeysetPager $pager,
    ) {
    }

    /**
     * One page of an exporter's records.
     *
     * @param Exporter $exporter The record kind.
     * @param Paging   $paging   The window.
     *
     * @return array<string, mixed> The page answer.
     */
    public function page(Exporter $exporter, Paging $paging): array
    {
        $page = $this->pager->read($exporter->dataset(), $paging);

        $items = [];
        foreach ($page->rows as $row) {
            $record = $exporter->map($row);
            if ($record !== null) {
                $items[] = $record;
            }
        }

        return [
            'items'       => $items,
            'totalPages'  => $page->totalPages,
            'nextAfterId' => $page->nextAfterId,
        ];
    }
}
