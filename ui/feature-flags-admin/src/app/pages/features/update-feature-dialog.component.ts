import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { FeatureFlagsService } from '../../core/services/feature-flags.service';

type FeatureDto = {
  key: string;
  defaultState: boolean;
  description: string | null;
};

@Component({
  standalone: true,
  selector: 'app-update-feature-dialog',
  imports: [CommonModule, FormsModule],
  templateUrl: './update-feature-dialog.component.html',
  styleUrls: ['./update-feature-dialog.component.css']
})
export class UpdateFeatureDialogComponent {
  private readonly service = inject(FeatureFlagsService);

  @Input({ required: true }) feature!: FeatureDto;
  @Output() closed = new EventEmitter<boolean>(); // true => saved, false => cancelled

  model = {
    defaultState: false,
    description: '' as string
  };

  loading = false;
  errorText: string | null = null;

  ngOnInit(): void {
    this.model.defaultState = !!this.feature.defaultState;
    this.model.description = this.feature.description ?? '';
  }

  async save(): Promise<void> {
    if (this.loading) return;

    this.loading = true;
    this.errorText = null;

    try {
      const payload = {
        defaultState: this.model.defaultState,
        description: this.model.description.trim().length === 0 ? null : this.model.description.trim()
      };

      await firstValueFrom(this.service.update(this.feature.key, payload));
      this.closed.emit(true);
    } catch (err: any) {
      const p = err?.error;
      if (p?.title || p?.detail) {
        this.errorText = `${p.title ?? 'Error'}: ${p.detail ?? ''}`.trim();
      } else {
        this.errorText = 'Update failed. Check API is running.';
      }
    } finally {
      this.loading = false;
    }
  }

  cancel(): void {
    this.closed.emit(false);
  }
}
