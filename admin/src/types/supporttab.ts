/**
 * Tab keys of the support pages that switch between forms (knowledgebase, downloads, predefined
 * replies). A tab key is a closed set, so each page has its own enum.
 */

/** Tabs of the knowledgebase page. */
export enum KnowledgebaseTab {
  /** Add and browse categories. */
  Category = 'category',
  /** Add an article. */
  Article = 'article',
}

/** Tabs of the downloads page. */
export enum DownloadsTab {
  /** Add a category. */
  Category = 'category',
  /** Add a download. */
  Download = 'download',
}

/** Tabs of the predefined replies page. */
export enum PredefinedRepliesTab {
  /** Add and manage categories. */
  Category = 'category',
  /** Add and manage replies. */
  Reply = 'reply',
  /** Search replies. */
  Search = 'search',
}
