import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthUser, LoginRequest, LoginResponse } from '../../shared/models/auth.models';

interface StoredSession {
  token: string;
  expiresAtUtc: string;
  user: AuthUser;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly storageKey = 'oo-auth';
  private readonly currentUserSubject = new BehaviorSubject<AuthUser | null>(this.getStoredSession()?.user ?? null);
  currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private readonly http: HttpClient,
    private readonly router: Router
  ) {}

  login(payload: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiBaseUrl}/api/auth/login`, payload).pipe(
      tap((response) => {
        const session: StoredSession = {
          token: response.token,
          expiresAtUtc: response.expiresAtUtc,
          user: response.user
        };
        localStorage.setItem(this.storageKey, JSON.stringify(session));
        this.currentUserSubject.next(response.user);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.storageKey);
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  isAuthenticated(): boolean {
    const session = this.getStoredSession();
    if (!session) {
      return false;
    }

    return new Date(session.expiresAtUtc).getTime() > Date.now();
  }

  getToken(): string | null {
    return this.getStoredSession()?.token ?? null;
  }

  getCurrentUser(): AuthUser | null {
    return this.currentUserSubject.value;
  }

  private getStoredSession(): StoredSession | null {
    const raw = localStorage.getItem(this.storageKey);
    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as StoredSession;
    } catch {
      return null;
    }
  }
}
