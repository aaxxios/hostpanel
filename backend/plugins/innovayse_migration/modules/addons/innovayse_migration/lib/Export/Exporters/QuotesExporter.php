<?php

declare(strict_types=1);

namespace HostpanelMigration\Export\Exporters;

use HostpanelMigration\Database\Dataset;
use HostpanelMigration\Database\Table;
use HostpanelMigration\Export\ClientOwnedExporter;
use HostpanelMigration\Mapping\IsoDate;
use Illuminate\Database\Query\Builder;
use Illuminate\Support\Collection;

/**
 * Exports quotes with their items (`quotes`).
 */
final class QuotesExporter extends ClientOwnedExporter
{
    /** The subject of a quote that names none. */
    private const DEFAULT_SUBJECT = 'Quote';

    /** The stage of a quote whose source stage is not one hostpanel knows. */
    private const DEFAULT_STAGE = 'Draft';

    /** The quantity of a quote item that names none. */
    private const DEFAULT_QUANTITY = 1;

    /**
     * {@inheritDoc}
     */
    public function dataset(): Dataset
    {
        return Dataset::Quotes;
    }

    /**
     * {@inheritDoc}
     */
    protected function record(object $row, string $clientEmail): ?array
    {
        return [
            'clientEmail'   => $clientEmail,
            'subject'       => $row->subject ?? self::DEFAULT_SUBJECT,
            'stage'         => self::stage((string) ($row->stage ?? self::DEFAULT_STAGE)),
            'expiryDate'    => IsoDate::from($row->validuntil ?? null),
            'proposalText'  => $row->proposal_text ?: null,
            'customerNotes' => $row->notes ?: null,
            'items'         => $this->items($row->id),
            'createdAt'     => IsoDate::from($row->datecreated ?? null),
        ];
    }

    /**
     * The quote's items.
     *
     * @param mixed $quoteId The quote id.
     *
     * @return list<array<string, mixed>> The wire items.
     */
    private function items(mixed $quoteId): array
    {
        $rows = $this->database->rows(
            Table::QUOTE_ITEMS,
            static fn (Builder $query): Collection => $query
                ->where('quoteid', $quoteId)
                ->get(['item_desc', 'item_unit_cost', 'item_qty', 'item_discount']),
        );

        $items = [];
        foreach ($rows as $item) {
            $items[] = [
                'description'     => $item->item_desc ?? '',
                'unitPrice'       => (float) ($item->item_unit_cost ?? 0),
                'quantity'        => (int) ($item->item_qty ?? self::DEFAULT_QUANTITY),
                'discountPercent' => (float) ($item->item_discount ?? 0),
            ];
        }

        return $items;
    }

    /**
     * Normalises a quote stage to the words hostpanel expects.
     *
     * @param string $stage The source stage.
     *
     * @return string The hostpanel stage.
     */
    private static function stage(string $stage): string
    {
        return match (strtolower($stage)) {
            'delivered' => 'Delivered',
            'accepted'  => 'Accepted',
            'lost'      => 'Lost',
            'dead'      => 'Dead',
            default     => self::DEFAULT_STAGE,
        };
    }
}
