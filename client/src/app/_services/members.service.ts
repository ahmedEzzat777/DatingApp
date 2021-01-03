import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/Pagination';
import { UserParams } from '../_models/UserParams';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;
  members: Member[] = [];
  
  constructor(private http: HttpClient) { }

  getMembers(userParams: UserParams){
    const params = this.getPaginationHeaders(userParams.pageNumber, userParams.pageSize)
      .append('minAge', userParams.minAge.toString())
      .append('maxAge', userParams.maxAge.toString())
      .append('gender', userParams.gender);

    return this.getPaginatedResult<Member[]>(this.baseUrl + 'users', params);
  }

  getMember(username: string){
    const member = this.members.find(m => m.username === username);

    if(member !== undefined) return of(member);

    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  updateMember(member: Member){
    return this.http.put(this.baseUrl + 'users', member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = member;
      })
    );
  }

  setMainPhoto(photoId: number) {
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photoId, {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photoId);
  }

  private getPaginatedResult<T>(url: string, params: HttpParams){
    const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();

    return this.http.get<T>(url, {observe: 'response', params}).pipe(
      map(response =>{
        paginatedResult.result = response.body as T;
        const pagination = response.headers.get('Pagination');

        if(pagination !== null) {
          paginatedResult.pagination = JSON.parse(pagination);
        }
        
        return paginatedResult;
      })
    );
  }

  private getPaginationHeaders(pageNumber: number, pageSize: number) {
    let params = new HttpParams();

    params = params.append('pageNumber', pageNumber.toString())
      .append('pageSize', pageSize.toString());

    return params;
  }
}
