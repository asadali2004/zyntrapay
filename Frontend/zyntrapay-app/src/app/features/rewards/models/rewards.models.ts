export interface RewardsSummary {
  totalPoints: number;
  pendingPoints: number;
  redeemedPoints: number;
}

export interface RewardCatalogItem {
  id: number;
  name: string;
  description: string;
  pointsRequired: number;
  isActive: boolean;
}

export interface RedeemRequest {
  catalogItemId: number;
}

export interface RewardHistoryItem {
  id: number;
  type: 'Earned' | 'Redeemed';
  points: number;
  description: string;
  createdAt: string;
}
