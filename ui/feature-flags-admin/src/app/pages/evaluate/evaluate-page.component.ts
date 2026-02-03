import { CommonModule } from '@angular/common';
import { Component, ChangeDetectorRef, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ActivatedRoute } from '@angular/router';

import { EvaluationService } from '../../core/services/evaluation.service';
import { EvaluateFeatureResponse } from '../../core/models/evaluation.model';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule],
  selector: 'app-evaluate-page',
  templateUrl: './evaluate-page.component.html',
  styleUrls: ['./evaluate-page.component.css']
})
export class EvaluatePageComponent {
  private readonly service = inject(EvaluationService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly route = inject(ActivatedRoute);

  // Inputs
  featureKey = '';
  userId = '';
  groupIdsCsv = '';
  region = 'IN';

  // Output
  loading = false;
  result: EvaluateFeatureResponse | null = null;
  errorText: string | null = null;

  ngOnInit(): void {
    const key = (this.route.snapshot.queryParamMap.get('key') ?? '').trim();
    if (key) this.featureKey = key;
  }

  async evaluate(): Promise<void> {
    // Prevent concurrent calls
    if (this.loading) return;

    this.errorText = null;

    const key = this.featureKey.trim();
    if (!key) {
      this.errorText = 'Feature key is required.';
      return;
    }

    // Clear previous result only when request is valid
    this.result = null;

    const userId = this.userId.trim();
    const region = this.region.trim();

    // Parse comma-separated groups: trim, remove empties, de-dupe case-insensitive
    const groupsRaw = this.groupIdsCsv
      .split(',')
      .map(x => x.trim())
      .filter(x => x.length > 0);

    const uniqueGroups: string[] = [];
    const seen = new Set<string>();
    for (const g of groupsRaw) {
      const k = g.toLowerCase();
      if (!seen.has(k)) {
        seen.add(k);
        uniqueGroups.push(g);
      }
    }

    const payload = {
      userId: userId.length === 0 ? null : userId,
      groupIds: uniqueGroups,
      region: region.length === 0 ? null : region
    };

    this.loading = true;
    this.cdr.detectChanges();

    try {
      const res = await firstValueFrom(this.service.evaluate(key, payload));

      // Assign response (force stable new object reference)
      this.result = { enabled: res.enabled, source: res.source };
    } catch (err: any) {
      // Try to show ProblemDetails nicely
      const problem = err?.error;

      if (problem?.title || problem?.detail) {
        this.errorText = `${problem.title ?? 'Error'}: ${problem.detail ?? ''}`.trim();
      } else if (typeof problem === 'string' && problem.length > 0) {
        this.errorText = problem;
      } else {
        this.errorText = 'Request failed. Check API is running and proxy is enabled.';
      }
    } finally {
      // ✅ GUARANTEED reset — fixes “Evaluating…” stuck forever
      this.loading = false;
      this.cdr.detectChanges();
    }
  }

  clear(): void {
    if (this.loading) return;

    this.featureKey = '';
    this.userId = '';
    this.groupIdsCsv = '';
    this.region = 'IN';
    this.result = null;
    this.errorText = null;

    this.cdr.detectChanges();
  }
}
