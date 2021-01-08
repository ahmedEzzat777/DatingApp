import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/Member';
import { PaginatedResult } from '../_models/Pagination';
import { User } from '../_models/User';
import { UserParams } from '../_models/UserParams';
import { AccountService } from './account.service';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;
  members = new Map<string, Member>();
  memberCache = new Map<string, PaginatedResult<Member[]>>();
  user: User;
  userParams: UserParams;

  constructor(private http: HttpClient, private accountService: AccountService, private router: Router) { 
    this.refresh();
  }

  refresh(){
    this.memberCache.clear();
    this.members.clear();
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user;
      this.userParams = new UserParams(user);
    });
  }

  getUserParams() {
    return this.userParams;
  }

  setUserParams(userParams: UserParams) {
    this.userParams = userParams;
  }

  resetUserParams() {
    this.userParams = new UserParams(this.user);
    return this.userParams;
  }

  addLike(username: string)
  {
    return this.http.post(this.baseUrl +'likes/'+username, {});
  }

  getLikes(predicate: string, pageNumber: number, pageSize: number){
    const params = getPaginationHeaders(pageNumber, pageSize)
                       .append('predicate', predicate);


    return getPaginatedResult<Partial<Member[]>>(this.baseUrl + 'likes', params, this.http);
  }

  getMembers(userParams: UserParams){
    let key = Object.values(userParams).join('-');
    let response = this.memberCache.get(key);

    if(response)
      return of(response);

    const params = getPaginationHeaders(userParams.pageNumber, userParams.pageSize)
      .append('minAge', userParams.minAge.toString())
      .append('maxAge', userParams.maxAge.toString())
      .append('gender', userParams.gender)
      .append('orderBy', userParams.orderBy);

    return getPaginatedResult<Member[]>(this.baseUrl + 'users', params, this.http).pipe(
      map(response =>{
        this.memberCache.set(key, response);

        response.result.forEach(member => {
          this.members.set(member.username, member);
        });

        return response;
      })
    );
  }

  getMember(username: string){
    /*
    const member = 
      [...this.memberCache.values()]
      .reduce((arr, elem) => arr.concat(elem.result), [])
      .find((member: Member) => member.username === username);
    */
    const member = this.members.get(username);

    if(member)
      return of(member);

    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  updateMember(member: Member){
    return this.http.put(this.baseUrl + 'users', member).pipe(
      map(() => {
        // const index = this.members.indexOf(member);
        // this.members[index] = member;
        this.members.set(member.username, member);
      })
    );
  }

  setMainPhoto(photoId: number) {
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photoId, {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photoId);
  }
}
