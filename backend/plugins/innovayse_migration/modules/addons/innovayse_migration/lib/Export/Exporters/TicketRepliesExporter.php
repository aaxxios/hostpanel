<?php

declare(strict_types=1);

namespace HostpanelMigration\Export\Exporters;

use HostpanelMigration\Database\Dataset;
use HostpanelMigration\Database\SourceDatabase;
use HostpanelMigration\Database\Table;
use HostpanelMigration\Export\Exporter;

/**
 * Exports replies on support tickets (`ticket_replies`). A reply belongs to a client through its
 * ticket, so it names the ticket by subject and the client by email; a reply whose ticket or
 * client is gone is not sent.
 */
final class TicketRepliesExporter implements Exporter
{
    /** The author name of a reply that names none. */
    private const UNKNOWN_AUTHOR = 'Unknown';

    /**
     * Creates the exporter.
     *
     * @param SourceDatabase $database The source database, for the ticket and its client.
     */
    public function __construct(
        /** The source database, for the ticket and its client. */
        private readonly SourceDatabase $database,
    ) {
    }

    /**
     * {@inheritDoc}
     */
    public function dataset(): Dataset
    {
        return Dataset::TicketReplies;
    }

    /**
     * {@inheritDoc}
     */
    public function map(object $row): ?array
    {
        $ticket = $this->database->table(Table::TICKETS)->where(Table::ID, $row->tid)->first([Table::CLIENT_ID, 'title']);
        if ($ticket === null) {
            return null;
        }

        $clientEmail = $this->database->clientEmail($ticket->{Table::CLIENT_ID});
        if ($clientEmail === null) {
            return null;
        }

        return [
            'clientEmail'   => $clientEmail,
            'ticketSubject' => $ticket->title,
            'message'       => $row->message ?? '',
            'authorName'    => $row->name ?? self::UNKNOWN_AUTHOR,
            'isStaffReply'  => (bool) ($row->admin ?? false),
        ];
    }
}
