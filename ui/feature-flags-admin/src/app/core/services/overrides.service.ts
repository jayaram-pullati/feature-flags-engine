import { Injectable, inject } from '@angular/core';
import { ApiClient } from '../api/api-client';
import { UpsertOverrideRequest } from '../models/override.model';

@Injectable({ providedIn: 'root' })
export class OverridesService {
  private readonly api = inject(ApiClient);

  upsertUser(featureKey: string, userId: string, state: boolean) {
    const body: UpsertOverrideRequest = { state };
    return this.api.put<void>(`/api/v1/features/${encodeURIComponent(featureKey)}/overrides/user/${encodeURIComponent(userId)}`, body);
  }

  upsertGroup(featureKey: string, groupId: string, state: boolean) {
    const body: UpsertOverrideRequest = { state };
    return this.api.put<void>(`/api/v1/features/${encodeURIComponent(featureKey)}/overrides/group/${encodeURIComponent(groupId)}`, body);
  }

  upsertRegion(featureKey: string, region: string, state: boolean) {
    const body: UpsertOverrideRequest = { state };
    return this.api.put<void>(`/api/v1/features/${encodeURIComponent(featureKey)}/overrides/region/${encodeURIComponent(region)}`, body);
  }

  deleteUser(featureKey: string, userId: string) {
    return this.api.delete<void>(`/api/v1/features/${encodeURIComponent(featureKey)}/overrides/user/${encodeURIComponent(userId)}`);
  }

  deleteGroup(featureKey: string, groupId: string) {
    return this.api.delete<void>(`/api/v1/features/${encodeURIComponent(featureKey)}/overrides/group/${encodeURIComponent(groupId)}`);
  }

  deleteRegion(featureKey: string, region: string) {
    return this.api.delete<void>(`/api/v1/features/${encodeURIComponent(featureKey)}/overrides/region/${encodeURIComponent(region)}`);
  }
}
