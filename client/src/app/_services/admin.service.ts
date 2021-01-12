import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { UnmoderatedPhoto } from '../_models/UnmoderatedPhoto';
import { User } from '../_models/User';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getUsersWithRoles() {
    return this.http.get<Partial<User[]>>(this.baseUrl + 'admin/users-with-roles');
  }

  updateUserRoles(username: string, roles: string[]) {
    return this.http.post(this.baseUrl + 'admin/edit-roles/' + username + '?roles=' + roles, {});
  }

  getPhotosForModeration(){
    return this.http.get<UnmoderatedPhoto[]>(this.baseUrl + 'admin/photos-to-moderate');
  }

  allowPhoto(photoId: number){
    return this.http.put(this.baseUrl + 'admin/moderate-photo/' + photoId + '?action=1',{});
  }

  deletePhoto(photoId: number){
    return this.http.put(this.baseUrl + 'admin/moderate-photo/' + photoId + '?action=0',{});
  }
}
