import { CommonModule } from '@angular/common';
import { Component, ChangeDetectorRef, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';

import { OverridesService } from '../../core/services/overrides.service';

type TargetType = 'user' | 'group' | 'region';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule],
  selector: 'app-overrides-page',
  templateUrl: './overrides-page.component.html',
  styleUrls: ['./overrides-page.component.css']
})
export class OverridesPageComponent {
  private readonly overrides = inject(OverridesService);
  private readonly cdr = inject(ChangeDetectorRef);

  // Inputs
  featureKey = '';

  userId = '';
  groupId = '';
  region = 'IN';

  state = true;

  // UI state
  loading = false;
  successText: string | null = null;
  errorText: string | null = null;

  async upsert(type: TargetType): Promise<void> {
    if (this.loading) return;

    this.successText = null;
    this.errorText = null;

    const key = this.featureKey.trim();
    if (!key) {
      this.errorText = 'Feature key is required.';
      return;
    }

    const target = this.getTarget(type);
    if (!target) return;

    this.loading = true;
    this.cdr.detectChanges();

    try {
      if (type === 'user') {
        await firstValueFrom(this.overrides.upsertUser(key, target, this.state));
      } else if (type === 'group') {
        await firstValueFrom(this.overrides.upsertGroup(key, target, this.state));
      } else {
        await firstValueFrom(this.overrides.upsertRegion(key, target, this.state));
      }

      this.successText = `${type.toUpperCase()} override upserted successfully.`;
    } catch (err: any) {
      this.errorText = this.toErrorText(err);
    } finally {
      this.loading = false;
      this.cdr.detectChanges();
    }
  }

  async remove(type: TargetType): Promise<void> {
    if (this.loading) return;

    this.successText = null;
    this.errorText = null;

    const key = this.featureKey.trim();
    if (!key) {
      this.errorText = 'Feature key is required.';
      return;
    }

    const target = this.getTarget(type);
    if (!target) return;

    this.loading = true;
    this.cdr.detectChanges();

    try {
      if (type === 'user') {
        await firstValueFrom(this.overrides.deleteUser(key, target));
      } else if (type === 'group') {
        await firstValueFrom(this.overrides.deleteGroup(key, target));
      } else {
        await firstValueFrom(this.overrides.deleteRegion(key, target));
      }

      this.successText = `${type.toUpperCase()} override deleted successfully.`;
    } catch (err: any) {
      this.errorText = this.toErrorText(err);
    } finally {
      this.loading = false;
      this.cdr.detectChanges();
    }
  }

  clearMessages(): void {
    this.successText = null;
    this.errorText = null;
  }

  private getTarget(type: TargetType): string | null {
    if (type === 'user') {
      const v = this.userId.trim();
      if (!v) {
        this.errorText = 'User Id is required.';
        return null;
      }
      return v;
    }

    if (type === 'group') {
      const v = this.groupId.trim();
      if (!v) {
        this.errorText = 'Group Id is required.';
        return null;
      }
      return v;
    }

    const v = this.region.trim();
    if (!v) {
      this.errorText = 'Region is required.';
      return null;
    }
    return v;
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
