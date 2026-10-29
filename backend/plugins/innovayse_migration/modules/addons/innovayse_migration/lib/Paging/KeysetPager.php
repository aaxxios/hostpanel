<?php

declare(strict_types=1);

namespace HostpanelMigration\Paging;

use HostpanelMigration\Database\Dataset;
use HostpanelMigration\Database\SourceDatabase;
use HostpanelMigration\Database\Table;
use Illuminate\Database\Query\Builder;

/**
 * Reads a dataset one window at a time, in id order, and counts it. Pages and totals both read
 * the dataset through {@see Dataset::query()}, so a count always matches what the pages hold.
 */
final class KeysetPager
{
    /**
     * Creates the pager.
     *
     * @param SourceDatabase $database The source database.
     */
    public function __construct(
        /** The source database. */
        private readonly SourceDatabase $database,
    ) {
    }

    /**
     * Reads one page: the rows after the keyset id when the caller pages by keyset, else the rows
     * of the page number.
     *
     * @param Dataset $dataset The dataset to read.
     * @param Paging  $paging  The window.
     *
     * @return Page The rows, the page count and the next keyset id.
     */
    public function read(Dataset $dataset, Paging $paging): Page
    {
        $rows = $this->database->hasTable($dataset->table())
            ? array_values(self::window($dataset->query($this->database), $paging)->get()->all())
            : [];

        // Based on the rows read, not the records sent, so a page whose rows are all left out
        // still moves the keyset forward.
        $nextAfterId = $paging->isKeyset() && count($rows) === $paging->perPage
            ? (int) $rows[array_key_last($rows)]->{Table::ID}
            : null;

        return new Page($rows, (int) ceil($this->count($dataset) / $paging->perPage), $nextAfterId);
    }

    /**
     * Counts a dataset's rows.
     *
     * @param Dataset $dataset The dataset to count.
     *
     * @return int The count; 0 when the table does not exist in this source version.
     */
    public function count(Dataset $dataset): int
    {
        return $this->database->hasTable($dataset->table())
            ? (int) $dataset->query($this->database)->count()
            : 0;
    }

    /**
     * Orders a query by id and limits it to the window.
     *
     * @param Builder $query  The dataset's filtered query.
     * @param Paging  $paging The window.
     *
     * @return Builder The windowed query.
     */
    public static function window(Builder $query, Paging $paging): Builder
    {
        $query->orderBy(Table::ID);
        if ($paging->isKeyset()) {
            $query->where(Table::ID, '>', $paging->afterId);
        } else {
            $query->skip($paging->offset());
        }

        return $query->take($paging->perPage);
    }
}
