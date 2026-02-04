import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class UiNavigationService {
  private _editKey: string | null = null;

  setEditKey(key: string): void {
    const k = (key ?? '').trim();
    this._editKey = k.length > 0 ? k : null;
  }

  consumeEditKey(): string | null {
    const key = this._editKey;
    this._editKey = null;
    return key;
  }

  clear(): void {
    this._editKey = null;
  }
}
