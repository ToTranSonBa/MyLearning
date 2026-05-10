import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./layout/main-layout/main-layout.component').then(
        (m) => m.MainLayoutComponent,
      ),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/home/pages/home/home.component').then(
            (m) => m.HomeComponent,
          ),
      },

      {
        path: 'users',
        canActivate: [authGuard],

        loadComponent: () =>
          import('./features/users/pages/users/users.component').then(
            (m) => m.UsersComponent,
          ),
      },

      {
        path: 'settings',
        loadComponent: () =>
          import('./features/settings/pages/settings/settings.component').then(
            (m) => m.SettingsComponent,
          ),
      },
    ],
  },

  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/pages/login/login.component').then(
        (m) => m.LoginComponent,
      ),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/pages/register/register.component').then(
        (m) => m.RegisterComponent,
      ),
  },
  {
    path: '**',
    redirectTo: '',
  },
];
