export interface ExpressEntryDraw {
  drawNumber: number;
  date: string;
  invitationsIssued: number;
  minimumCRS: number;
  category: string | null;
  year: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
