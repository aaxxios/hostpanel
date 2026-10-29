<?php

declare(strict_types=1);

namespace HostpanelMigration;

/**
 * The addon's identity inside the source billing system. The module id and the setting name stay
 * `innovayse_migration` and `migration_key`: existing installations store their key under them.
 */
final class Module
{
    /** The module id the source system knows the addon by; also its folder name. */
    public const ID = 'innovayse_migration';

    /** The module setting that holds the shared migration key. */
    public const KEY_SETTING = 'migration_key';

    /** The addon's version, shown in the source system's module list. */
    public const VERSION = '1.3.0';

    /** The path the pull API answers on, relative to the source system's web root. */
    public const API_PATH = '/modules/addons/' . self::ID . '/api.php';

    /**
     * Holds constants only; never instantiated.
     */
    private function __construct()
    {
    }
}
