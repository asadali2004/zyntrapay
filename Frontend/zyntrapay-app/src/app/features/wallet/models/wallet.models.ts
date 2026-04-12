export interface WalletBalance {
  id: number;
  authUserId: number;
  balance: number;
  isActive: boolean;
  createdAt: string;
}

export interface Transaction {
  id: number;
  type: 'Credit' | 'Debit' | 'credit' | 'debit' | 'transfer' | 'Transfer';
  description: string;
  amount: number;
  referenceId?: string;
  createdAt: string;
}

export interface TransactionRequest {
  receiverAuthUserId?: number;
  receiverEmail?: string;
  amount: number;
  description?: string;
}

export interface TopUpRequest {
  amount: number;
  description: string;
}
