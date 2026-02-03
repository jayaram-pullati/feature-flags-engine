import { Component, EventEmitter, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FeatureFlagsService } from '../../core/services/feature-flags.service';

@Component({
  standalone: true,
  selector: 'app-create-feature-dialog',
  imports: [FormsModule],
  template: `
    <div class="dialog">
      <h3>Create Feature</h3>

      <label>Key:</label>
      <input [(ngModel)]="key" />

      <label>Enabled:</label>
      <input type="checkbox" [(ngModel)]="defaultState" />

      <label>Description:</label>
      <input [(ngModel)]="description" />

      <button (click)="save()">Create</button>
      <button (click)="closed.emit()">Cancel</button>
    </div>
  `
})
export class CreateFeatureDialogComponent {
  private readonly service = inject(FeatureFlagsService);

  key = '';
  defaultState = false;
  description: string | null = null;

  @Output() closed = new EventEmitter<void>();

  save() {
    this.service.create({
      key: this.key,
      defaultState: this.defaultState,
      description: this.description
    }).subscribe({
      next: () => this.closed.emit(),
      error: (e) => alert(JSON.stringify(e.error))
    });
  }
}
