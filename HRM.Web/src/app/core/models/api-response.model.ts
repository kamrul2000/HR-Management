export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface BulkCreateResult {
  successCount: number;
  skippedCount: number;
  failedCount: number;
  skippedReasons: string[];
  failedReasons: string[];
}

/**
 * Normalize a list response so the client always sees `PagedResult<T>` shape.
 *
 * Some backend endpoints return `ApiResponse<T[]>` (a bare array) while others
 * return `ApiResponse<PagedResult<T>>`. Pipe every list call through this so
 * the UI layer can always rely on `data.items`, `data.totalCount`, etc.
 */
export function toPagedResponse<T>(
  res: ApiResponse<T[] | PagedResult<T> | null>,
): ApiResponse<PagedResult<T>> {
  if (!res.success || !res.data) {
    return { success: res.success, message: res.message, data: null };
  }
  if (Array.isArray(res.data)) {
    const items = res.data;
    return {
      success: res.success,
      message: res.message,
      data: {
        items,
        totalCount: items.length,
        pageNumber: 1,
        pageSize: items.length || 1,
        totalPages: 1,
        hasNextPage: false,
        hasPreviousPage: false,
      },
    };
  }
  return res as ApiResponse<PagedResult<T>>;
}
