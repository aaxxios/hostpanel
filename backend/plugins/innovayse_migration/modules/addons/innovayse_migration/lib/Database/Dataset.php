<?php

declare(strict_types=1);

namespace HostpanelMigration\Database;

use Illuminate\Database\Query\Builder;

/**
 * Every exported dataset: its table and the one filter that decides which rows it holds.
 *
 * A row whose client does not exist, or a ledger row that moves no money, is left out of a page,
 * so it is left out of the count too. The totals and every page read through {@see self::query()},
 * which is what lets an import reach 100 percent.
 */
enum Dataset
{
    /** Client accounts. */
    case Clients;

    /** Client sub-contacts of existing clients. */
    case Contacts;

    /** Product groups. */
    case ProductGroups;

    /** Products. */
    case Products;

    /** Invoices of existing clients. */
    case Invoices;

    /** Services of existing clients. */
    case Services;

    /** Domains of existing clients. */
    case Domains;

    /** Tickets of existing clients. */
    case Tickets;

    /** Replies on tickets of existing clients. */
    case TicketReplies;

    /** Orders of existing clients. */
    case Orders;

    /** Ledger rows of existing clients that move money. */
    case Transactions;

    /** Quotes of existing clients. */
    case Quotes;

    /** Knowledgebase categories. */
    case KnowledgebaseCategories;

    /** Knowledgebase articles. */
    case Knowledgebase;

    /** Announcements. */
    case Announcements;

    /** Download categories. */
    case DownloadCategories;

    /** Downloads. */
    case Downloads;

    /** Network issues. */
    case NetworkIssues;

    /**
     * The table the dataset is read from.
     *
     * @return string The table name.
     */
    public function table(): string
    {
        return match ($this) {
            self::Clients => Table::CLIENTS,
            self::Contacts => Table::CONTACTS,
            self::ProductGroups => Table::PRODUCT_GROUPS,
            self::Products => Table::PRODUCTS,
            self::Invoices => Table::INVOICES,
            self::Services => Table::SERVICES,
            self::Domains => Table::DOMAINS,
            self::Tickets => Table::TICKETS,
            self::TicketReplies => Table::TICKET_REPLIES,
            self::Orders => Table::ORDERS,
            self::Transactions => Table::TRANSACTIONS,
            self::Quotes => Table::QUOTES,
            self::KnowledgebaseCategories => Table::KNOWLEDGEBASE_CATEGORIES,
            self::Knowledgebase => Table::KNOWLEDGEBASE,
            self::Announcements => Table::ANNOUNCEMENTS,
            self::DownloadCategories => Table::DOWNLOAD_CATEGORIES,
            self::Downloads => Table::DOWNLOADS,
            self::NetworkIssues => Table::NETWORK_ISSUES,
        };
    }

    /**
     * The dataset's rows, filtered, in no particular order: the pager orders and windows it, the
     * totals count it.
     *
     * @param SourceDatabase $database The source database.
     *
     * @return Builder The filtered query.
     */
    public function query(SourceDatabase $database): Builder
    {
        $query = $database->table($this->table());

        return match ($this) {
            self::Contacts, self::Invoices, self::Services, self::Domains, self::Tickets, self::Orders, self::Quotes
                => $query->whereIn(Table::CLIENT_ID, self::clientIds($database)),
            self::Transactions => $query
                ->whereIn(Table::CLIENT_ID, self::clientIds($database))
                ->where(static function (Builder $money): void {
                    $money->where('amountin', '>', 0)->orWhere('amountout', '>', 0);
                }),
            self::TicketReplies => $query->whereIn('tid', self::Tickets->query($database)->select(Table::ID)),
            default => $query,
        };
    }

    /**
     * The ids of the clients that exist, as a subquery.
     *
     * @param SourceDatabase $database The source database.
     *
     * @return Builder The subquery.
     */
    private static function clientIds(SourceDatabase $database): Builder
    {
        return $database->table(Table::CLIENTS)->select(Table::ID);
    }
}
