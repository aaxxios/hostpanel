/** File type label of a download, as the API names it. */
export enum DownloadType {
  /** An archive. */
  Zip = 'ZIP File',
  /** A program to run. */
  Executable = 'Executable',
  /** A PDF document. */
  Pdf = 'PDF',
  /** Anything else. */
  Other = 'Other',
}

/** A downloadable file as the API returns it. */
export interface DownloadItem {
  /** Unique download identifier. */
  id: number
  /** Download title. */
  title: string
  /** Download description text. */
  description: string
  /** File type label. */
  type: DownloadType
  /** Name of the downloadable file. */
  filename: string
  /** Category this download belongs to. */
  categoryId: number
  /** Category name for display purposes. */
  categoryName: string | null
  /** Total number of times downloaded. */
  downloadCount: number
  /** Whether only logged-in clients can download. */
  clientsOnly: boolean
  /** Whether a product purchase is required to download. */
  productDownload: boolean
  /** Whether the download is hidden from the client area. */
  isHidden: boolean
  /** ISO 8601 creation timestamp. */
  createdAt: string
}

/** Body that creates or replaces a downloadable file entry. */
export interface DownloadItemPayload {
  /** Download title. */
  title: string
  /** Download description text. */
  description: string
  /** File type label. */
  type: DownloadType
  /** Name of the downloadable file. */
  filename: string
  /** Category the download belongs to, null for none. */
  categoryId: number | null
  /** Whether only logged-in clients can download. */
  clientsOnly: boolean
  /** Whether a product purchase is required to download. */
  productDownload: boolean
  /** Whether the download is hidden from the client area. */
  isHidden: boolean
}
