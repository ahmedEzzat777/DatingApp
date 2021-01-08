import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { User } from '../_models/User';
import { AccountService } from '../_services/account.service';

@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {

  constructor(private AccountService: AccountService, private toastrService: ToastrService) {}

  canActivate(): Observable<boolean> {
    return this.AccountService.currentUser$.pipe(
      map<User, boolean>(user => {
        if(user.roles.includes("Admin") || user.roles.includes("Moderator"))
          return true;
        
        this.toastrService.error("you cannot enter this area")
        return false;
      })
    );
  }
  
}
