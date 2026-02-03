import { Injectable, inject } from '@angular/core';
import { ApiClient } from '../api/api-client';
import {
  Feature,
  CreateFeatureRequest,
  UpdateFeatureRequest
} from '../models/feature.model';

@Injectable({ providedIn: 'root' })
export class FeatureFlagsService {
  private readonly api = inject(ApiClient);

  list() {
    return this.api.get<Feature[]>('/api/v1/features');
  }

  get(key: string) {
    return this.api.get<Feature>(`/api/v1/features/${key}`);
  }

  create(req: CreateFeatureRequest) {
    return this.api.post<Feature>('/api/v1/features', req);
  }

  update(key: string, req: UpdateFeatureRequest) {
    return this.api.put<Feature>(`/api/v1/features/${key}`, req);
  }

  delete(key: string) {
    return this.api.delete<void>(`/api/v1/features/${key}`);
  }
}
