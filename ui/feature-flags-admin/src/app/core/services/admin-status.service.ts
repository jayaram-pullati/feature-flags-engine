import { inject, Injectable } from '@angular/core';
import { ApiClient } from '../api/api-client';
import { AdminRefreshResponse, AdminStatusResponse } from '../models/admin-status.model';

@Injectable({ providedIn: 'root' })
export class AdminStatusService {
  private readonly api = inject(ApiClient);

  getStatus() {
    return this.api.get<AdminStatusResponse>('/api/v1/admin/feature-flags/status');
  }

  refreshSnapshot() {
    return this.api.post<AdminRefreshResponse>('/api/v1/admin/feature-flags/refresh', {});
  }
}
