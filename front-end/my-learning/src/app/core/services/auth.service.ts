import { Injectable, inject, signal, computed } from '@angular/core';
import { ApiService } from './api.service';
import { Router } from '@angular/router';
import { LoginRequest } from '../../features/auth/models/login-request.model';
import { tap } from 'rxjs';
import { RegisterRequest } from '../../features/auth/models/register-request.model';
import { CookieService } from 'ngx-cookie-service';
import { User } from '../../features/auth/models/user.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private cookie = inject(CookieService);
  private api = inject(ApiService);
  private router = inject(Router);

  private TOKEN_KEY = 'access_token';
  private REFRESH_TOKEN = 'refresh_token';

  private currentUser = signal<any>(null);

  constructor(private cookieService: CookieService) {
    this.loadUserFromCookie();
  }

  login(request: LoginRequest) {
    return this.api.post<any>('auth/login', request).pipe(
      tap((response) => {
        this.cookie.set(this.TOKEN_KEY, response.accessToken);
        this.currentUser.set(response.user);
        this.router.navigate(['/']);
      }),
    );
  }

  logout() {
    return this.api.post('auth/logout', {}).pipe(
      tap(() => {
        this.cookie.delete(this.TOKEN_KEY);
        this.currentUser.set(null);
      }),
    );
  }

  register(request: RegisterRequest) {
    return this.api.post('auth/register', request);
  }

  getToken(): string | null {
    return this.cookie.get(this.TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    return !!this.currentUser;
  }

  isLoggedIn = computed(() => this.currentUser() !== null);

  loadUserFromCookie() {
    const token = this.cookieService.get(this.TOKEN_KEY);
    if (token) {
      const userInfo = this.decodeToken(token);
      this.currentUser.set(userInfo);
    }
  }

  private decodeToken(token: string): any {
    try {
      const payload = token.split('.')[1];
      return JSON.parse(atob(payload));
    } catch {
      return null;
    }
  }
}
