import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error' | 'info' | 'warning';

export interface Toast {
  id: number;
  type: ToastType;
  title: string;
  message?: string;
  duration?: number;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private _toasts = signal<Toast[]>([]);
  readonly toasts = this._toasts.asReadonly();
  private nextId = 1;

  show(type: ToastType, title: string, message?: string, duration = 4000): void {
    const id = this.nextId++;
    const toast: Toast = { id, type, title, message, duration };
    this._toasts.update(t => [...t, toast]);

    if (duration > 0) {
      setTimeout(() => this.dismiss(id), duration);
    }
  }

  success(title: string, message?: string): void {
    this.show('success', title, message);
  }

  error(title: string, message?: string): void {
    this.show('error', title, message, 5000);
  }

  info(title: string, message?: string): void {
    this.show('info', title, message);
  }

  warning(title: string, message?: string): void {
    this.show('warning', title, message);
  }

  dismiss(id: number): void {
    this._toasts.update(t => t.filter(toast => toast.id !== id));
  }
}
