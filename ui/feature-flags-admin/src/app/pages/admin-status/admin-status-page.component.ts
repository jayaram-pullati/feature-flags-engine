import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { finalize, switchMap, tap } from 'rxjs';

import { AdminStatusService } from '../../core/services/admin-status.service';
import { AdminStatusResponse } from '../../core/models/admin-status.model';

@Component({
  standalone: true,
  imports: [CommonModule],
  selector: 'app-admin-status-page',
  templateUrl: './admin-status-page.component.html',
  styleUrls: ['./admin-status-page.component.css']
})
export class AdminStatusPageComponent {
  private readonly service = inject(AdminStatusService);
  private readonly cdr = inject(ChangeDetectorRef);

  loading = true;
  refreshing = false;

  status: AdminStatusResponse | null = null;
  errorText: string | null = null;

  lastUpdated: Date | null = null;
  lastRefreshMessage: string | null = null;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.errorText = null;
    this.lastRefreshMessage = null;

    this.service
      .getStatus()
      .pipe(
        tap((res) => {
          this.status = res;
          this.lastUpdated = new Date();
        }),
        finalize(() => {
          this.loading = false;
          // ✅ important when running zone-less / custom API clients
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        error: (err) => {
          this.status = null;
          this.errorText = this.toErrorText(err);
          this.cdr.detectChanges();
        }
      });
  }

  refresh(): void {
    this.refreshing = true;
    this.errorText = null;
    this.lastRefreshMessage = null;

    this.service
      .refreshSnapshot()
      .pipe(
        tap((res) => {
          this.lastRefreshMessage = res?.message ?? 'Snapshot refreshed.';
        }),
        switchMap(() => this.service.getStatus()),
        tap((status) => {
          this.status = status;
          this.lastUpdated = new Date();
        }),
        finalize(() => {
          this.refreshing = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        error: (err) => {
          this.errorText = this.toErrorText(err);
          this.cdr.detectChanges();
        }
      });
  }

  private toErrorText(err: any): string {
    const problem = err?.error;

    if (problem?.title || problem?.detail) {
      return `${problem.title ?? 'Error'}: ${problem.detail ?? ''}`.trim();
    }

    if (typeof problem === 'string' && problem.length > 0) return problem;

    return 'Request failed. Check API is running and proxy is enabled.';
  }
}
