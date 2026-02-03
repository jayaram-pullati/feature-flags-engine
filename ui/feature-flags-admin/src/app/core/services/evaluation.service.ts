import { Injectable, inject } from '@angular/core';
import { ApiClient } from '../api/api-client';
import { EvaluateFeatureRequest, EvaluateFeatureResponse } from '../models/evaluation.model';

@Injectable({ providedIn: 'root' })
export class EvaluationService {
  private readonly api = inject(ApiClient);

  evaluate(key: string, req: EvaluateFeatureRequest) {
    const encodedKey = encodeURIComponent(key);
    return this.api.post<EvaluateFeatureResponse>(`/api/v1/evaluate/${encodedKey}`, req);
  }
}
