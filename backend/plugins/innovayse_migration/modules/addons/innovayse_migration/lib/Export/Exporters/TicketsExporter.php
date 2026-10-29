<?php

declare(strict_types=1);

namespace HostpanelMigration\Export\Exporters;

use HostpanelMigration\Database\Dataset;
use HostpanelMigration\Database\Table;
use HostpanelMigration\Export\ClientOwnedExporter;
use HostpanelMigration\Export\SourceDefaults;
use HostpanelMigration\Mapping\IsoDate;
use Illuminate\Database\Query\Builder;

/**
 * Exports support tickets with their opening message (`tickets`).
 */
final class TicketsExporter extends ClientOwnedExporter
{
    /** The status of a ticket whose source status is not one hostpanel knows. */
    private const DEFAULT_STATUS = 'Open';

    /**
     * {@inheritDoc}
     */
    public function dataset(): Dataset
    {
        return Dataset::Tickets;
    }

    /**
     * {@inheritDoc}
     */
    protected function record(object $row, string $clientEmail): ?array
    {
        $department = $this->database->first(
            Table::TICKET_DEPARTMENTS,
            static fn (Builder $query): ?object => $query->where(Table::ID, $row->did)->first(['name']),
        );

        return [
            'clientEmail'    => $clientEmail,
            'subject'        => $row->title,
            'message'        => $this->openingMessage($row),
            'status'         => self::status((string) ($row->status ?? self::DEFAULT_STATUS)),
            'priority'       => $row->urgency ?? SourceDefaults::PRIORITY,
            'departmentName' => $department?->name,
            'createdAt'      => IsoDate::from($row->date ?? null),
        ];
    }

    /**
     * The ticket's opening message: the ticket row holds it, and an older ticket keeps it as its
     * first reply instead.
     *
     * @param object $row The ticket row.
     *
     * @return string The message; empty when neither place holds one.
     */
    private function openingMessage(object $row): string
    {
        $message = (string) ($row->message ?? '');
        if (trim($message) !== '') {
            return $message;
        }

        $firstReply = $this->database->first(
            Table::TICKET_REPLIES,
            static fn (Builder $query): ?object => $query->where('tid', $row->id)->orderBy(Table::ID)->first(['message']),
        );

        return (string) ($firstReply?->message ?? '');
    }

    /**
     * Normalises a ticket status to the words hostpanel expects.
     *
     * @param string $status The source status.
     *
     * @return string The hostpanel status.
     */
    private static function status(string $status): string
    {
        return match (strtolower($status)) {
            'answered' => 'Answered',
            'closed'   => 'Closed',
            'on hold'  => 'On Hold',
            // A customer reply leaves the ticket waiting on staff, which hostpanel calls open.
            default    => self::DEFAULT_STATUS,
        };
    }
}
