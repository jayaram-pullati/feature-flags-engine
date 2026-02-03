import { CommonModule } from '@angular/common';
import { Component, ChangeDetectorRef, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { firstValueFrom, timeout } from 'rxjs';

import { FeatureFlagsService } from '../../core/services/feature-flags.service';

type FeatureDto = {
  key: string;
  defaultState: boolean;
  description: string | null;
};

@Component({
  standalone: true,
  selector: 'app-feature-details',
  imports: [CommonModule],
  templateUrl: './feature-details-page.component.html',
  styleUrls: ['./feature-details-page.component.css']
})
export class FeatureDetailsPageComponent {
  private readonly service = inject(FeatureFlagsService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  loading = true;
  errorText: string | null = null;
  feature: FeatureDto | null = null;

  featureKey = '';

  async ngOnInit(): Promise<void> {
    this.featureKey = (this.route.snapshot.paramMap.get('key') ?? '').trim();

    if (!this.featureKey) {
      this.errorText = 'No feature key provided.';
      this.loading = false;
      this.cdr.detectChanges();
      return;
    }

    await this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.errorText = null;
    this.feature = null;
    this.cdr.detectChanges();

    try {
      // ✅ 10s safety so UI never hangs forever if Observable doesn't complete
      const f = await firstValueFrom(
        this.service.get(this.featureKey).pipe(timeout({ first: 10_000 }))
      );

      this.feature = f as FeatureDto;
    } catch (err: any) {
      const problem = err?.error;

      if (problem?.title || problem?.detail) {
        this.errorText = `${problem.title ?? 'Error'}: ${problem.detail ?? ''}`.trim();
      } else if (err?.name === 'TimeoutError') {
        this.errorText = 'Request timed out. Check API is running and proxy is enabled.';
      } else {
        this.errorText = 'Unable to load feature. Check API and proxy configuration.';
      }
    } finally {
      // ✅ GUARANTEED reset
      this.loading = false;
      this.cdr.detectChanges();
    }
  }

  goEdit(): void {
    this.router.navigate(['/features'], { queryParams: { edit: this.featureKey } });
  }

  goOverrides(): void {
    this.router.navigate(['/overrides'], { queryParams: { key: this.featureKey } });
  }

  goEvaluate(): void {
    this.router.navigate(['/evaluate'], { queryParams: { key: this.featureKey } });
  }
}
