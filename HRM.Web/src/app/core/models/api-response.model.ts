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
