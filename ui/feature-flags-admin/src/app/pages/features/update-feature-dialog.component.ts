import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Feature } from '../../core/models/feature.model';
import { FeatureFlagsService } from '../../core/services/feature-flags.service';

@Component({
  standalone: true,
  selector: 'app-update-feature-dialog',
  imports: [FormsModule],
  template: `
    <div class="dialog">
      <h3>Edit Feature: {{ feature.key }}</h3>

      <label>Enabled:</label>
      <input type="checkbox" [(ngModel)]="model.defaultState" />

      <label>Description:</label>
      <input [(ngModel)]="model.description" />

      <button (click)="save()">Save</button>
      <button (click)="closed.emit()">Cancel</button>
    </div>
  `
})
export class UpdateFeatureDialogComponent {
  @Input() feature!: Feature;
  @Output() closed = new EventEmitter<void>();

  private readonly service = inject(FeatureFlagsService);

  model = {
    defaultState: false,
    description: ''
  };

  ngOnInit() {
    this.model.defaultState = this.feature.defaultState;
    this.model.description = this.feature.description ?? '';
  }

  save() {
    this.service.update(this.feature.key, this.model).subscribe({
      next: () => this.closed.emit(),
      error: (e) => alert(JSON.stringify(e.error))
    });
  }
}
