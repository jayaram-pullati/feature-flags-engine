import { InjectionToken } from '@angular/core';

export type ApiConfig = {
  baseUrl: string;
};

export const API_CONFIG = new InjectionToken<ApiConfig>('API_CONFIG');
