<?php

declare(strict_types=1);

namespace HostpanelMigration\Auth;

use JsonException;

/**
 * Keeps each address's failed attempts in a small file under a directory of the server's
 * temporary space. The file is named by a hash of the address, so no address is written to disk
 * in the clear.
 */
final class FileAttemptLog implements AttemptLog
{
    /** The permissions of the log directory: the web server user only. */
    private const DIRECTORY_MODE = 0700;

    /** The hash that names an address's file. */
    private const NAME_HASH = 'sha256';

    /** The extension of an address's file. */
    private const EXTENSION = '.json';

    /**
     * Creates the log.
     *
     * @param string $directory The directory the files are kept in; created on first write.
     */
    public function __construct(
        /** The directory the files are kept in; created on first write. */
        private readonly string $directory,
    ) {
    }

    /**
     * {@inheritDoc}
     */
    public function read(string $address): ?AttemptWindow
    {
        $file = $this->fileOf($address);
        if (!is_file($file)) {
            return null;
        }

        $contents = file_get_contents($file);
        if ($contents === false) {
            throw new AttemptLogUnavailable('The attempt log cannot be read.');
        }

        try {
            $stored = json_decode($contents, true, 512, JSON_THROW_ON_ERROR);
        } catch (JsonException) {
            // A torn or foreign file holds no usable count; the next failure overwrites it.
            return null;
        }

        if (!is_array($stored) || !is_int($stored['failures'] ?? null) || !is_int($stored['startedAt'] ?? null)) {
            return null;
        }

        return new AttemptWindow($stored['failures'], $stored['startedAt']);
    }

    /**
     * {@inheritDoc}
     */
    public function write(string $address, AttemptWindow $window): void
    {
        if (!is_dir($this->directory) && !mkdir($this->directory, self::DIRECTORY_MODE, true) && !is_dir($this->directory)) {
            throw new AttemptLogUnavailable('The attempt log directory cannot be created.');
        }

        $contents = json_encode(['failures' => $window->failures, 'startedAt' => $window->startedAt], JSON_THROW_ON_ERROR);
        if (file_put_contents($this->fileOf($address), $contents, LOCK_EX) === false) {
            throw new AttemptLogUnavailable('The attempt log cannot be written.');
        }
    }

    /**
     * {@inheritDoc}
     */
    public function clear(string $address): void
    {
        $file = $this->fileOf($address);
        if (is_file($file) && !unlink($file)) {
            throw new AttemptLogUnavailable('The attempt log cannot be cleared.');
        }
    }

    /**
     * The file that holds an address's window.
     *
     * @param string $address The caller's address.
     *
     * @return string The file path.
     */
    private function fileOf(string $address): string
    {
        return $this->directory . DIRECTORY_SEPARATOR . hash(self::NAME_HASH, $address) . self::EXTENSION;
    }
}
