<?php

declare(strict_types=1);

namespace HostpanelMigration\Database;

/**
 * The source billing system's table names the addon reads. They belong to the source system,
 * so they keep its spelling.
 */
final class Table
{
    /** Module settings, where the migration key is stored. */
    public const ADDON_MODULES = 'tbladdonmodules';

    /** Client accounts. */
    public const CLIENTS = 'tblclients';

    /** Client sub-contacts. */
    public const CONTACTS = 'tblcontacts';

    /** Currencies, id to ISO code. */
    public const CURRENCIES = 'tblcurrencies';

    /** Product groups. */
    public const PRODUCT_GROUPS = 'tblproductgroups';

    /** Products. */
    public const PRODUCTS = 'tblproducts';

    /** Prices of products, addons and domains: one row per item, price type and currency. */
    public const PRICING = 'tblpricing';

    /** The addons a product can be ordered with. */
    public const ADDONS = 'tbladdons';

    /** Invoices. */
    public const INVOICES = 'tblinvoices';

    /** Invoice line items. */
    public const INVOICE_ITEMS = 'tblinvoiceitems';

    /** Provisioned client services. */
    public const SERVICES = 'tblhosting';

    /** Registered domains. */
    public const DOMAINS = 'tbldomains';

    /** Support tickets. */
    public const TICKETS = 'tbltickets';

    /** Replies on support tickets. */
    public const TICKET_REPLIES = 'tblticketreplies';

    /** Support departments. */
    public const TICKET_DEPARTMENTS = 'tblticketdepartments';

    /** Orders. */
    public const ORDERS = 'tblorders';

    /**
     * Addons ordered on a client's services. The source keeps no order items table: an order's
     * lines are its services, domains and service addons.
     */
    public const SERVICE_ADDONS = 'tblhostingaddons';

    /** The ledger: payments, refunds and other money movements. */
    public const TRANSACTIONS = 'tblaccounts';

    /** Quotes. */
    public const QUOTES = 'tblquotes';

    /** Quote items. */
    public const QUOTE_ITEMS = 'tblquoteitems';

    /** Knowledgebase categories. */
    public const KNOWLEDGEBASE_CATEGORIES = 'tblknowledgebasecats';

    /** Knowledgebase articles. */
    public const KNOWLEDGEBASE = 'tblknowledgebase';

    /** Announcements. */
    public const ANNOUNCEMENTS = 'tblannouncements';

    /** Download categories. */
    public const DOWNLOAD_CATEGORIES = 'tbldownloadcats';

    /** Downloads. */
    public const DOWNLOADS = 'tbldownloads';

    /** Network issues. */
    public const NETWORK_ISSUES = 'tblnetworkissues';

    /** The primary key column every exported table shares. */
    public const ID = 'id';

    /** The column naming the owning client on client-owned tables. */
    public const CLIENT_ID = 'userid';

    /** The column naming the order a service, domain or service addon was ordered in. */
    public const ORDER_ID = 'orderid';

    /**
     * Holds constants only; never instantiated.
     */
    private function __construct()
    {
    }
}
