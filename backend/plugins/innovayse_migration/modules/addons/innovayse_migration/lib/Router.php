<?php

declare(strict_types=1);

namespace HostpanelMigration;

use HostpanelMigration\Database\SourceDatabase;
use HostpanelMigration\Export\DatasetExport;
use HostpanelMigration\Export\Exporter;
use HostpanelMigration\Export\Exporters\AnnouncementsExporter;
use HostpanelMigration\Export\Exporters\ClientsExporter;
use HostpanelMigration\Export\Exporters\ContactsExporter;
use HostpanelMigration\Export\Exporters\DomainsExporter;
use HostpanelMigration\Export\Exporters\DownloadCategoriesExporter;
use HostpanelMigration\Export\Exporters\DownloadsExporter;
use HostpanelMigration\Export\Exporters\InvoicesExporter;
use HostpanelMigration\Export\Exporters\KnowledgebaseCategoriesExporter;
use HostpanelMigration\Export\Exporters\KnowledgebaseExporter;
use HostpanelMigration\Export\Exporters\NetworkIssuesExporter;
use HostpanelMigration\Export\Exporters\OrdersExporter;
use HostpanelMigration\Export\Exporters\ProductGroupsExporter;
use HostpanelMigration\Export\Exporters\ProductsExporter;
use HostpanelMigration\Export\Exporters\QuotesExporter;
use HostpanelMigration\Export\Exporters\ServicesExporter;
use HostpanelMigration\Export\Exporters\TicketRepliesExporter;
use HostpanelMigration\Export\Exporters\TicketsExporter;
use HostpanelMigration\Export\Exporters\TransactionsExporter;
use HostpanelMigration\Export\TotalsReport;
use HostpanelMigration\Http\Action;
use HostpanelMigration\Mapping\CurrencyMap;
use HostpanelMigration\Paging\Paging;

/**
 * Runs one action: answers a ping, reports the totals, or exports one page of the action's
 * dataset with its exporter. Each request builds only the exporter it needs.
 */
final class Router
{
    /** The answer to `ping`. */
    private const PING_ANSWER = ['ok' => true];

    /**
     * Creates the router.
     *
     * @param SourceDatabase $database The source database the exporters read related rows from.
     * @param TotalsReport   $totals   Answers `totals`.
     * @param DatasetExport  $export   Answers the dataset actions.
     */
    public function __construct(
        /** The source database the exporters read related rows from. */
        private readonly SourceDatabase $database,
        /** Answers `totals`. */
        private readonly TotalsReport $totals,
        /** Answers the dataset actions. */
        private readonly DatasetExport $export,
    ) {
    }

    /**
     * Runs the action.
     *
     * @param Action $action The action.
     * @param Paging $paging The window; read by the dataset actions only.
     *
     * @return array<string, mixed> The answer body.
     */
    public function dispatch(Action $action, Paging $paging): array
    {
        return match ($action) {
            Action::Ping => self::PING_ANSWER,
            Action::Totals => $this->totals->counts(),
            default => $this->export->page($this->exporterFor($action), $paging),
        };
    }

    /**
     * The exporter of a dataset action.
     *
     * @param Action $action A dataset action.
     *
     * @return Exporter The action's exporter.
     */
    private function exporterFor(Action $action): Exporter
    {
        return match ($action) {
            Action::Clients => new ClientsExporter($this->currencies()),
            Action::Contacts => new ContactsExporter($this->database),
            Action::ProductGroups => new ProductGroupsExporter(),
            Action::Products => new ProductsExporter($this->database, $this->database->defaultCurrencyId()),
            Action::Invoices => new InvoicesExporter($this->database),
            Action::Services => new ServicesExporter($this->database),
            Action::Domains => new DomainsExporter($this->database),
            Action::Tickets => new TicketsExporter($this->database),
            Action::TicketReplies => new TicketRepliesExporter($this->database),
            Action::Orders => new OrdersExporter($this->database),
            Action::Transactions => new TransactionsExporter($this->database, $this->currencies()),
            Action::Quotes => new QuotesExporter($this->database),
            Action::KnowledgebaseCategories => new KnowledgebaseCategoriesExporter(),
            Action::Knowledgebase => new KnowledgebaseExporter(),
            Action::Announcements => new AnnouncementsExporter(),
            Action::DownloadCategories => new DownloadCategoriesExporter(),
            Action::Downloads => new DownloadsExporter(),
            Action::NetworkIssues => new NetworkIssuesExporter(),
            Action::Ping, Action::Totals => throw new \LogicException('Not a dataset action.'),
        };
    }

    /**
     * The source's currencies, read for the exporters that send a currency code.
     *
     * @return CurrencyMap Currency id to ISO code.
     */
    private function currencies(): CurrencyMap
    {
        return new CurrencyMap($this->database->currencyCodes());
    }
}
