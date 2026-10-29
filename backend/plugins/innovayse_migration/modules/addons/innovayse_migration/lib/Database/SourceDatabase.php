<?php

declare(strict_types=1);

namespace HostpanelMigration\Database;

use Closure;
use HostpanelMigration\Module;
use Illuminate\Database\Capsule\Manager as Capsule;
use Illuminate\Database\Connection;
use Illuminate\Database\Query\Builder;
use Illuminate\Support\Collection;

/**
 * The one gateway to the source billing system's database, over the source system's own query
 * builder. Every read the addon makes goes through here.
 *
 * Some tables do not exist in every version of the source system. A read of such a table answers
 * empty instead of failing; any other database error is not hidden and fails the request, so a
 * pull never imports a page that silently lost its rows.
 */
final class SourceDatabase
{
    /** @var array<string, bool> Whether each table asked about exists, so the schema is asked once per table. */
    private array $tables = [];

    /**
     * Creates the gateway over a connection.
     *
     * @param Connection $connection The source system's database connection.
     */
    public function __construct(
        /** The source system's database connection. */
        private readonly Connection $connection,
    ) {
    }

    /**
     * The gateway over the connection the source system has already opened.
     *
     * @return self The gateway.
     */
    public static function fromSourceSystem(): self
    {
        return new self(Capsule::connection());
    }

    /**
     * A query on a table.
     *
     * @param string $table The table name.
     *
     * @return Builder The query.
     */
    public function table(string $table): Builder
    {
        return $this->connection->table($table);
    }

    /**
     * Whether a table exists in this source version.
     *
     * @param string $table The table name.
     *
     * @return bool True when it exists.
     */
    public function hasTable(string $table): bool
    {
        return $this->tables[$table] ??= $this->connection->getSchemaBuilder()->hasTable($table);
    }

    /**
     * Runs a query on a table that may not exist in this source version.
     *
     * @param string                       $table The table name.
     * @param Closure(Builder): Collection $read  Builds and runs the query.
     *
     * @return Collection The rows; empty when the table does not exist.
     */
    public function rows(string $table, Closure $read): Collection
    {
        return $this->hasTable($table) ? $read($this->table($table)) : new Collection();
    }

    /**
     * Reads one row of a table that may not exist in this source version.
     *
     * @param string                    $table The table name.
     * @param Closure(Builder): ?object $read  Builds and runs the query.
     *
     * @return object|null The row; null when there is none or the table does not exist.
     */
    public function first(string $table, Closure $read): ?object
    {
        return $this->hasTable($table) ? $read($this->table($table)) : null;
    }

    /**
     * The migration key configured in the module settings.
     *
     * @return string The key; empty when none is configured.
     */
    public function migrationKey(): string
    {
        return (string) $this->table(Table::ADDON_MODULES)
            ->where('module', Module::ID)
            ->where('setting', Module::KEY_SETTING)
            ->value('value');
    }

    /**
     * The email of the client with this id. Only client ids are looked up: a user id is a
     * different number space and never stands in for a client.
     *
     * @param mixed $clientId The client id as the row holds it.
     *
     * @return string|null The email, or null when the id is empty or no such client exists.
     */
    public function clientEmail(mixed $clientId): ?string
    {
        if (!$clientId) {
            return null;
        }

        $client = $this->table(Table::CLIENTS)->where(Table::ID, $clientId)->first(['email']);

        return $client === null ? null : (string) $client->email;
    }

    /**
     * The name of a row by its id, for the tables that name their rows (products, addons).
     *
     * @param string $table The table name.
     * @param mixed  $id    The id as the referencing row holds it.
     *
     * @return string|null The name, or null when the id is empty, no such row exists or the
     *                     table does not exist in this source version.
     */
    public function nameOf(string $table, mixed $id): ?string
    {
        if (!$id) {
            return null;
        }

        $row = $this->first($table, static fn (Builder $query): ?object => $query->where(Table::ID, $id)->first(['name']));

        return $row === null ? null : (string) $row->name;
    }

    /**
     * The id of the currency the source marks as its default, the one its base prices are in.
     *
     * @return int|null The id, or null when the source has no currency table or marks none.
     */
    public function defaultCurrencyId(): ?int
    {
        $currency = $this->first(
            Table::CURRENCIES,
            static fn (Builder $query): ?object => $query->where('default', 1)->orderBy(Table::ID)->first([Table::ID]),
        );

        return $currency === null ? null : (int) $currency->{Table::ID};
    }

    /**
     * Currency id to ISO code, as configured in the source. A source without a currency table
     * bills everything in its one default currency and answers no codes.
     *
     * @return array<int, string> The codes by currency id.
     */
    public function currencyCodes(): array
    {
        $codes = [];
        $rows = $this->rows(Table::CURRENCIES, static fn (Builder $query): Collection => $query->get(['id', 'code']));
        foreach ($rows as $currency) {
            $codes[(int) $currency->id] = (string) $currency->code;
        }

        return $codes;
    }
}
