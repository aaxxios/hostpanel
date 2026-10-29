<?php

declare(strict_types=1);

namespace HostpanelMigration\Addon;

use HostpanelMigration\Module;

/**
 * What the source billing system asks an addon module for: its configuration and the answers to
 * activation and deactivation. The addon keeps no tables, so neither step changes anything.
 */
final class AddonModule
{
    /** The name shown in the source system's module list. */
    private const NAME = 'Hostpanel Migration';

    /** The answer status the source system reads as success. */
    private const SUCCESS = 'success';

    /**
     * The module's configuration: its name, version and the migration key setting.
     *
     * @return array<string, mixed> The configuration in the source system's shape.
     */
    public function config(): array
    {
        return [
            'name'        => self::NAME,
            'description' => 'Connect this billing system to hostpanel so it can pull your data.',
            'version'     => Module::VERSION,
            'author'      => 'Hostpanel',
            'fields'      => [
                Module::KEY_SETTING => [
                    'FriendlyName' => 'Migration Key',
                    'Type'         => 'password',
                    'Size'         => '60',
                    'Description'  => 'Secret key from hostpanel: Migration Tools, New Migration',
                    'Default'      => '',
                ],
            ],
        ];
    }

    /**
     * The answer to activation.
     *
     * @return array<string, string> The answer in the source system's shape.
     */
    public function activate(): array
    {
        return ['status' => self::SUCCESS, 'description' => self::NAME . ' activated.'];
    }

    /**
     * The answer to deactivation.
     *
     * @return array<string, string> The answer in the source system's shape.
     */
    public function deactivate(): array
    {
        return ['status' => self::SUCCESS, 'description' => self::NAME . ' deactivated.'];
    }
}
