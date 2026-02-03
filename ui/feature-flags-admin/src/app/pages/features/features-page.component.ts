import { Component, inject, OnInit } from '@angular/core';
import { FeatureFlagsService } from '../../core/services/feature-flags.service';
import { Feature } from '../../core/models/feature.model';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CreateFeatureDialogComponent } from './create-feature-dialog.component';
import { UpdateFeatureDialogComponent } from './update-feature-dialog.component';

@Component({
  standalone: true,
  imports: [CommonModule, CreateFeatureDialogComponent, UpdateFeatureDialogComponent],
  selector: 'app-features-page',
  templateUrl: './features-page.component.html',
  styleUrls: ['./features-page.component.css']
})
export class FeaturesPageComponent implements OnInit {
  private readonly service = inject(FeatureFlagsService);
  private readonly router = inject(Router);

  features: Feature[] = [];
  loading = true;
  createOpen = false;
  editTarget: Feature | null = null;

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading = true;
    this.service.list().subscribe({
      next: (list) => (this.features = list),
      error: (err) => console.error(err),
      complete: () => (this.loading = false)
    });
  }

  openCreate() {
    this.createOpen = true;
  }

  closeCreate() {
    this.createOpen = false;
    this.load();
  }

  details(f: any) {
  this.router.navigate(['/features', f.key]);
}

  openEdit(feature: Feature) {
    this.editTarget = feature;
  }

  closeEdit() {
    this.editTarget = null;
    this.load();
  }

  delete(feature: Feature) {
    if (!confirm(`Delete feature "${feature.key}"?`)) return;

    this.service.delete(feature.key).subscribe({
      next: () => this.load(),
      error: (e) => console.error(e)
    });
  }
}
