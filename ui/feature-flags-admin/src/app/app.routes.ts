import { Routes } from '@angular/router';
import { ShellComponent } from './layout/shell.component';
import { FeaturesPageComponent } from './pages/features/features-page.component';
import { EvaluatePageComponent } from './pages/evaluate/evaluate-page.component';
import { OverridesPageComponent } from './pages/overrides/overrides-page.component';
import { AdminStatusPageComponent } from './pages/admin-status/admin-status-page.component';

export const routes: Routes = [
  {
    path: '',
    component: ShellComponent,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'features' },
      { path: 'features', component: FeaturesPageComponent },
      { path: 'evaluate', component: EvaluatePageComponent },
      { path: 'overrides', component: OverridesPageComponent },
      { path: 'admin/status', component: AdminStatusPageComponent }
    ]
  }
];
