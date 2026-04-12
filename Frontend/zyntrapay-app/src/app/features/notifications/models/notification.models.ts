export interface Notification {
  id: number;
  authUserId: number;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}
