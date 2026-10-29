<?php

declare(strict_types=1);

namespace HostpanelMigration\Export;

use HostpanelMigration\Database\Dataset;
use HostpanelMigration\Paging\KeysetPager;

/**
 * Answers `totals`: one count per record kind, counted with the same filter the pages read with.
 * The member names and their order are the wire contract.
 */
final class TotalsReport
{
    /**
     * Creates the report.
     *
     * @param KeysetPager $pager Counts each dataset.
     */
    public function __construct(
        /** Counts each dataset. */
        private readonly KeysetPager $pager,
    ) {
    }

    /**
     * The counts.
     *
     * @return array<string, int> One count per wire member.
     */
    public function counts(): array
    {
        $counts = [];
        foreach (self::members() as $member => $dataset) {
            $counts[$member] = $this->pager->count($dataset);
        }

        return $counts;
    }

    /**
     * The wire member each dataset is counted under, in wire order. Product groups and
     * knowledgebase categories are not counted: hostpanel imports them alongside products and
     * articles and sizes its progress by those.
     *
     * @return array<string, Dataset> The datasets by wire member.
     */
    private static function members(): array
    {
        return [
            'clients'            => Dataset::Clients,
            'invoices'           => Dataset::Invoices,
            'services'           => Dataset::Services,
            'domains'            => Dataset::Domains,
            'tickets'            => Dataset::Tickets,
            'products'           => Dataset::Products,
            'orders'             => Dataset::Orders,
            'transactions'       => Dataset::Transactions,
            'quotes'             => Dataset::Quotes,
            'knowledgebase'      => Dataset::Knowledgebase,
            'contacts'           => Dataset::Contacts,
            'ticketReplies'      => Dataset::TicketReplies,
            'announcements'      => Dataset::Announcements,
            'downloads'          => Dataset::Downloads,
            'downloadCategories' => Dataset::DownloadCategories,
            'networkIssues'      => Dataset::NetworkIssues,
        ];
    }
}
