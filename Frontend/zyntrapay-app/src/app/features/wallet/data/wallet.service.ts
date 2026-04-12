import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { WalletBalance, Transaction, TransactionRequest, TopUpRequest } from '../models/wallet.models';

@Injectable({ providedIn: 'root' })
export class WalletService {
  private walletUrl = `${environment.apiUrl}/wallet`;

  constructor(private http: HttpClient) {}

  createWallet(): Observable<any> {
    return this.http.post(`${this.walletUrl}/create`, {});
  }

  getBalance(): Observable<WalletBalance> {
    return this.http.get<WalletBalance>(`${this.walletUrl}/balance`);
  }

  topUp(data: TopUpRequest): Observable<any> {
    return this.http.post(`${this.walletUrl}/topup`, data);
  }

  getTransactions(): Observable<Transaction[]> {
    return this.http.get<Transaction[]>(`${this.walletUrl}/transactions`);
  }

  getTransactionById(id: number): Observable<Transaction> {
    return this.http.get<Transaction>(`${this.walletUrl}/transactions/${id}`);
  }

  sendMoney(data: TransactionRequest): Observable<any> {
    return this.http.post(`${this.walletUrl}/transfer`, data);
  }

  /**
   * @deprecated Use RewardsService.getSummary() instead.
   */
  getRewards(): Observable<any> {
    return this.http.get(`${environment.apiUrl}/rewards/summary`);
  }
}
