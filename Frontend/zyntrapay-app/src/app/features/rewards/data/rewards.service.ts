import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { RewardsSummary, RewardCatalogItem, RedeemRequest, RewardHistoryItem } from '../models/rewards.models';

@Injectable({
  providedIn: 'root'
})
export class RewardsService {
  private apiUrl = `${environment.apiUrl}/rewards`;

  constructor(private http: HttpClient) {}

  getSummary(): Observable<RewardsSummary> {
    return this.http.get<RewardsSummary>(`${this.apiUrl}/summary`);
  }

  getCatalog(): Observable<RewardCatalogItem[]> {
    return this.http.get<RewardCatalogItem[]>(`${this.apiUrl}/catalog`);
  }

  redeem(data: RedeemRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/redeem`, data);
  }

  getHistory(): Observable<RewardHistoryItem[]> {
    return this.http.get<RewardHistoryItem[]>(`${this.apiUrl}/history`);
  }
}
