import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';

import { routes } from './app.routes';
import { API_CONFIG } from './core/config/api-config';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(),

    // ✅ Required for ApiClient
    { provide: API_CONFIG, useValue: { baseUrl: '' } }
  ]
};
