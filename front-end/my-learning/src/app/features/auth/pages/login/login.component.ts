import { Component, inject } from '@angular/core';
import { Validators, ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { AuthService } from '../../../../core/services/auth.service';
import { LoginRequest } from '../../models/login-request.model';
import { MatFormField, MatLabel, MatInput } from '@angular/material/input';
import { MatCard } from '@angular/material/card';
import { MatIcon } from '@angular/material/icon';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';

import { MatDialog } from '@angular/material/dialog';
import { MessageDialogComponent } from '../../../../shared/components/message-dialog/message-dialog.component';

@Component({
  selector: 'app-login',
  imports: [
    ReactiveFormsModule,
    MatFormField,
    MatLabel,
    MatCard,
    MatInput,
    MatIcon,
    MatButtonModule,
    RouterLink,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  hidePassword = true;

  email = '';
  password = '';
  error = '';

  private fb = inject(FormBuilder);
  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],

    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  auth = inject(AuthService);
  dialog = inject(MatDialog);

  login() {
    if (this.form.invalid) return;

    const value = this.form.getRawValue();

    const request: LoginRequest = {
      EmailOrUsername: value.email!,
      Password: value.password!,
    };

    this.auth.login(request).subscribe({
      error: (err) => {
        this.dialog.open(MessageDialogComponent, {
          data: {
            title: 'Error',
            message: 'Login failed',
          },
        });
      },
    });
  }
}
