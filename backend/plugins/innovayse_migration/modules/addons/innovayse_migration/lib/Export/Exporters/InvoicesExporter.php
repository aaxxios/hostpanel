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
 * Exports invoices with their line items (`invoices`).
 */
final class InvoicesExporter extends ClientOwnedExporter
{
    /**
     * The quantity every line item is sent with: the source's invoice items table has no quantity
     * column, and a line's amount is already the line total.
     */
    private const ITEM_QUANTITY = 1;

    /** The status of an invoice whose source status is not one hostpanel knows. */
    private const DEFAULT_STATUS = 'Unpaid';

    /**
     * {@inheritDoc}
     */
    public function dataset(): Dataset
    {
        return Dataset::Invoices;
    }

    /**
     * {@inheritDoc}
     */
    protected function record(object $row, string $clientEmail): ?array
    {
        return [
            'clientEmail'   => $clientEmail,
            'externalId'    => (string) $row->id,
            'total'         => (float) $row->total,
            // The source splits tax into two levels; hostpanel keeps one.
            'tax'           => (float) ($row->tax ?? 0) + (float) ($row->tax2 ?? 0),
            'taxRate'       => (float) ($row->taxrate ?? 0) + (float) ($row->taxrate2 ?? 0),
            'credit'        => (float) ($row->credit ?? 0),
            'status'        => self::status((string) $row->status),
            'date'          => IsoDate::from($row->date ?? null),
            'dueDate'       => IsoDate::from($row->duedate ?? null),
            'paidAt'        => IsoDate::from($row->datepaid ?? null),
            'paymentMethod' => $row->paymentmethod ?: null,
            'items'         => $this->items($row->id),
        ];
    }

    /**
     * The invoice's line items.
     *
     * @param mixed $invoiceId The invoice id.
     *
     * @return list<array<string, mixed>> The wire items.
     */
    private function items(mixed $invoiceId): array
    {
        $rows = $this->database->rows(
            Table::INVOICE_ITEMS,
            static fn (Builder $query): Collection => $query->where('invoiceid', $invoiceId)->get(['description', 'amount']),
        );

        $items = [];
        foreach ($rows as $item) {
            $items[] = [
                'description' => $item->description,
                'amount'      => (float) $item->amount,
                'quantity'    => self::ITEM_QUANTITY,
            ];
        }

        return $items;
    }

    /**
     * Normalises an invoice status to the words hostpanel expects.
     *
     * @param string $status The source status.
     *
     * @return string The hostpanel status.
     */
    private static function status(string $status): string
    {
        return match (strtolower($status)) {
            'paid'            => 'Paid',
            'overdue'         => 'Overdue',
            'cancelled'       => 'Cancelled',
            'draft'           => 'Draft',
            'refunded'        => 'Refunded',
            'collections'     => 'Collections',
            'payment pending' => 'PaymentPending',
            default           => self::DEFAULT_STATUS,
        };
    }
}
