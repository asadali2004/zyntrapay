export interface ApiResponse<T = any> {
  success?: boolean;
  message?: string;
  data?: T;
}

export interface ErrorResponse {
  message: string;
  errors?: { [key: string]: string[] };
}
