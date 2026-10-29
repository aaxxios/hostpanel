<?php

declare(strict_types=1);

namespace HostpanelMigration\Http;

/**
 * The closed set of operations the pull API answers. The backing value is the `action` string a
 * caller sends; anything else is refused with 400.
 */
enum Action: string
{
    /** Connectivity and key check; answers `{"ok":true}`. */
    case Ping = 'ping';

    /** One count per record kind, sized with the same filter as the pages. */
    case Totals = 'totals';

    /** Client accounts. */
    case Clients = 'clients';

    /** Client sub-contacts. */
    case Contacts = 'contacts';

    /** Product groups. */
    case ProductGroups = 'product_groups';

    /** Products. */
    case Products = 'products';

    /** Invoices with their line items. */
    case Invoices = 'invoices';

    /** Provisioned client services. */
    case Services = 'services';

    /** Registered domains. */
    case Domains = 'domains';

    /** Support tickets with their opening message. */
    case Tickets = 'tickets';

    /** Replies on support tickets. */
    case TicketReplies = 'ticket_replies';

    /** Orders with their items. */
    case Orders = 'orders';

    /** Ledger rows that move money. */
    case Transactions = 'transactions';

    /** Quotes with their items. */
    case Quotes = 'quotes';

    /** Knowledgebase categories. */
    case KnowledgebaseCategories = 'knowledgebase_categories';

    /** Knowledgebase articles. */
    case Knowledgebase = 'knowledgebase';

    /** Announcements. */
    case Announcements = 'announcements';

    /** Download categories. */
    case DownloadCategories = 'download_categories';

    /** Downloads. */
    case Downloads = 'downloads';

    /** Network issues. */
    case NetworkIssues = 'network_issues';
}
