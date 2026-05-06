import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

/**
 * Placeholder login page — replaced by the full implementation in Group A.
 * Group 0 ships this stub so the routing and guest guard wiring can be exercised.
 */
@Component({
  selector: 'hrm-login-placeholder',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="placeholder">
      <h1>Login</h1>
      <p class="text-muted">Group A will replace this with the production login page.</p>
    </div>
  `,
  styles: [
    `
      .placeholder {
        max-width: 480px;
        margin: 80px auto;
        padding: 32px;
        background: #fff;
        border: 1px solid #E2E8F0;
        border-radius: 12px;
        text-align: center;
      }
    `,
  ],
})
export class LoginComponent {}
