import { Component, inject } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatInput } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';

import { MatButtonModule } from '@angular/material/button';

import { MatIconModule } from '@angular/material/icon';

import { Router, RouterLink } from '@angular/router';

import { AuthService } from '../../../../core/services/auth.service';

import { RegisterRequest } from '../../models/register-request.model';

import { MatDialog } from '@angular/material/dialog';

import { MessageDialogComponent } from '../../../../shared/components/message-dialog/message-dialog.component';

@Component({
  selector: 'app-register',
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule,
    RouterLink,
    MatInput,
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
})
export class RegisterComponent {
  hidePassword = true;
  hideConfirm = true;

  email = '';
  password = '';
  confirmPassword = '';
  error = '';

  private fb = inject(FormBuilder);
  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],

    password: ['', [Validators.required, Validators.minLength(6)]],

    confirmPassword: ['', Validators.required],
  });

  auth = inject(AuthService);
  private dialog = inject(MatDialog);
  private router = inject(Router);

  register() {
    if (this.form.invalid) return;

    const value = this.form.getRawValue();
    const register: RegisterRequest = {
      email: value.email!,
      password: value.password!,
      userName: value.email!,
      fullname: value.email!,
    };

    this.auth.register(register).subscribe({
      complete: () => {
        this.dialog.open(MessageDialogComponent, {
          data: {
            title: 'Success',
            message: 'Register successfully',
          },
        });
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.dialog.open(MessageDialogComponent, {
          data: {
            title: 'Error',
            message: 'Register failed',
          },
        });
      },
    });
  }
}
