import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  standalone: true,
  selector: 'app-shell',
  imports: [RouterLink, RouterOutlet],
  template: `
    <div style="display:flex; min-height:100vh;">
      <aside style="width:220px; border-right:1px solid #ddd; padding:16px;">
        <h3 style="margin-top:0;">Feature Flags</h3>

        <nav style="display:flex; flex-direction:column; gap:8px;">
          <a routerLink="/features">Features</a>
          <a routerLink="/evaluate">Evaluate</a>
          <a routerLink="/admin/status">Admin Status</a>
        </nav>
      </aside>

      <main style="flex:1; padding:16px;">
        <router-outlet />
      </main>
    </div>
  `
})
export class ShellComponent {}
