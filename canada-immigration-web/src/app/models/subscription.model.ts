export const ALL_CATEGORIES_VALUE = 'ALL';

export interface SubscriptionRequest {
  email: string;
  categories: string[];
}

export interface SubscriptionResponse {
  message: string;
}
