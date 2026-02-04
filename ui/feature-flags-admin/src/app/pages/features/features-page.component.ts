import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ActivatedRoute } from '@angular/router';

import { Feature } from '../../core/models/feature.model';
import { FeatureFlagsService } from '../../core/services/feature-flags.service';
import { UiNavigationService } from '../../core/navigation/ui-navigation.service';

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
  private readonly nav = inject(UiNavigationService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);

  features: Feature[] = [];
  loading = true;

  createOpen = false;
  editTarget: Feature | null = null;

  // cached list state
  private listLoaded = false;

  ngOnInit(): void {
    this.loadAndHandleEditParam();
  }

  private isOnFeaturesRoute(): boolean {
    // handles /features and /features?x=y
    return this.router.url === '/features' || this.router.url.startsWith('/features?');
  }

  load(): void {
    this.loading = true;
    this.listLoaded = false;

    this.service.list().subscribe({
      next: (list) => {
        this.features = list;
        this.listLoaded = true;

        // After list loads, try opening any pending edit key
        this.consumeEditKeyAndOpen();
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
        this.listLoaded = false;
      },
      complete: () => (this.loading = false)
    });
  }

  private consumeEditKeyAndOpen(): void {
    // Never open edit if Create is open or edit already open
    if (this.createOpen || this.editTarget) return;

    const key = this.nav.consumeEditKey();
    if (!key) return;

    const normalized = key.trim().toLowerCase();
    if (!normalized) return;

    // If list already loaded, try find in list first
    if (this.listLoaded) {
      const found = this.features.find(f => (f.key ?? '').toLowerCase() === normalized);
      if (found) {
        this.openEdit(found);
        return;
      }
    }

    // Fallback: GET by key and open edit even if list didn’t include it (or list not loaded yet)
    this.service.get(key).subscribe({
      next: (f) => this.openEdit(f),
      error: (e) => console.error('Edit open failed (feature not found?):', e)
    });
  }

  openCreate(): void {
    this.nav.clear();
    this.editTarget = null;
    this.createOpen = true;
  }

  closeCreate(): void {
    this.createOpen = false;
    this.load();
  }

  details(f: Feature): void {
    this.router.navigate(['/features', f.key]);
  }

  openEdit(feature: Feature): void {
    this.createOpen = false;
    this.editTarget = feature;
  }

  closeEdit(saved?: boolean): void {
    this.editTarget = null;
    if (saved) this.load();
  }

  delete(feature: Feature): void {
    if (!confirm(`Delete feature "${feature.key}"?`)) return;

    this.service.delete(feature.key).subscribe({
      next: () => this.load(),
      error: (e) => console.error(e)
    });
  }

  private async loadAndHandleEditParam(): Promise<void> {
  this.loading = true;

  try {
    const list = await firstValueFrom(this.service.list());
    this.features = list;

    const editKey = (this.route.snapshot.queryParamMap.get('edit') ?? '').trim();
    if (editKey) {
      const found = this.features.find(f => (f.key ?? '').toLowerCase() === editKey.toLowerCase());

      if (found) {
        this.openEdit(found);
      } else {
        // fallback get
        const f = await firstValueFrom(this.service.get(editKey));
        this.openEdit(f);
      }

      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: { edit: null },
        queryParamsHandling: 'merge',
        replaceUrl: true
      });
    }
  } catch (err) {
    console.error(err);
  } finally {
    this.loading = false;
  }
}
}
