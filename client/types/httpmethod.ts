/** The HTTP verbs the server side sends to the backend and reads on an incoming request. */
export enum HttpMethod {
  /** Reads a resource. */
  Get = 'GET',
  /** Reads only the headers of a resource; an incoming request may carry it. */
  Head = 'HEAD',
  /** Asks which methods a resource allows; an incoming request may carry it. */
  Options = 'OPTIONS',
  /** Creates a resource or runs an action. */
  Post = 'POST',
  /** Replaces or updates a resource. */
  Put = 'PUT',
  /** Removes a resource. */
  Delete = 'DELETE',
  /** Partially updates a resource. */
  Patch = 'PATCH'
}
