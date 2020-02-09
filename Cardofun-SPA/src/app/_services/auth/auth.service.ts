import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { JwtHelperService } from '@auth0/angular-jwt';
import { environment } from 'src/environments/environment';
import { User } from 'src/app/_models/user';
import { LocalStorageService } from '../local-storage/local-storage.service';
import { SignalrMessageService } from '../signalr/signalr-message/signalr-message.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  constructor(private http: HttpClient, private localStorageService: LocalStorageService,
    private signalrMessageSerice: SignalrMessageService) { }

  baseUrl = environment.apiUrl + 'auth/';
  jwtHelper = new JwtHelperService();
  decodedToken: any;
  currentUser: User;
  photoUrl = new BehaviorSubject<string>('../../assets/user.png');
  currentPhotoUrl = this.photoUrl.asObservable();

  login(model: User) {
    return this.http.post(this.baseUrl + 'login', model).pipe(
      map((response: any) => {
        const user = response;
        if (user) {
          this.localStorageService.setToken(user.token);
          this.localStorageService.setUser(user.user);
          this.decodedToken = this.jwtHelper.decodeToken(user.token);
          this.currentUser = user.user;
          this.changeMemberPhoto(user.user.photoUrl);
          this.currentUser.roles = this.decodedToken.role as Array<string>;
          this.signalrMessageSerice.startConnection();
        }
      })
    );
  }

  logout() {
    this.signalrMessageSerice.stopConnection();
    this.localStorageService.removeToken();
    this.localStorageService.removeUser();
    this.decodedToken = null;
    this.currentUser = null;
  }

  register(user: User) {
    return this.http.post(this.baseUrl + 'register', user);
  }

  isLoggedIn() {
    const token = this.localStorageService.getToken();
    return !this.jwtHelper.isTokenExpired(token);
  }

  changeMemberPhoto(photoUrl: string) {
    this.photoUrl.next(photoUrl);
  }

  refreshUserInformation() {
    const token = this.localStorageService.getToken();
    const user: User = this.localStorageService.getUser();
    if (token) {
      this.decodedToken = this.jwtHelper.decodeToken(token);
    }
    if (user) {
      this.currentUser = user;
      this.changeMemberPhoto(user.photoUrl);
      this.currentUser.roles = this.decodedToken.role as Array<string>;
    }
  }

  roleMatch(allowedRoles: string[]): boolean {
    let isMatch = false;
    const userRoles = this.currentUser.roles;
    if (userRoles) {
      allowedRoles.forEach(role => {
        if (userRoles.includes(role)) {
          isMatch = true;
          return;
        }
      });
    }
    return isMatch;
  }
}
