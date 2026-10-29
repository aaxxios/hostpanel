<?php

declare(strict_types=1);

namespace HostpanelMigration\Tests\Fixtures;

use Illuminate\Database\Capsule\Manager as Capsule;
use Illuminate\Database\Connection;

/**
 * An in-memory SQLite connection over the same query builder the source system ships, so the
 * tests run the addon's real queries without the source system.
 */
final class InMemoryDatabase
{
    /**
     * Holds a factory only; never instantiated.
     */
    private function __construct()
    {
    }

    /**
     * A fresh, empty database.
     *
     * @return Connection The connection; every call gets its own database.
     */
    public static function connection(): Connection
    {
        $capsule = new Capsule();
        $capsule->addConnection(['driver' => 'sqlite', 'database' => ':memory:', 'prefix' => '']);

        return $capsule->getConnection();
    }
}
