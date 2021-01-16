import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Group } from '../_models/Group';
import { Message } from '../_models/Message';
import { User } from '../_models/User';
import { BusyService } from './busy.service';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl
  hubUrl = environment.hubUrl;
  private hubConnection: HubConnection;
  private messageThreadSource = new BehaviorSubject<Message[]>([]);
  messageThread$ = this.messageThreadSource.asObservable();
  availablePages:number[] = [];

  constructor(private http: HttpClient, private busyService: BusyService) { }

  createHubConnection(user: User, otherUsername: string, pageNumber: number, pageSize: number) {
    this.busyService.busy();
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'message?user=' + otherUsername + '&pageNumber=' + pageNumber + '&pageSize=' + pageSize, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();

      this.hubConnection
        .start()
        .catch(error => console.log(error))
        .finally(() => this.busyService.idle());

      this.hubConnection.on('RecieveMessageThread', (messages: Message[]) => {
        this.messageThreadSource.next(messages);
        this.availablePages.push(1);
      });

      this.hubConnection.on('NewMessage', (message: Message) => {
        this.messageThread$.pipe(take(1)).subscribe(messages => {
          this.messageThreadSource.next([...messages, message]);
        });
      });

      this.hubConnection.on('UpdatedGroup', (group: Group) => {
        
        if(group.connections.some(x => x.username === otherUsername)) {
          this.messageThread$.pipe(take(1)).subscribe(messages => {
            messages.forEach(m => {
              if(!m.dateRead) {
                m.dateRead = new Date(Date.now());
              }
            });

            this.messageThreadSource.next([...messages]);
          });
        }

      });

  }

  stopHubConnection() {
    if(this.hubConnection) {
      this.messageThreadSource.next([]);
      this.availablePages = [];
      this.hubConnection
        .stop()
        .catch(error => console.log(error));;
    }
  }

  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('Container', container);
    return getPaginatedResult<Message[]>(this.baseUrl + 'messages', params, this.http);
  }

  // getMessageThread(username: string) {
  //   return this.http.get<Message[]>(this.baseUrl + 'messages/thread/' + username);
  // }

  async sendMessage(username: string, content: string) {
    return this.hubConnection.invoke('SendMessage', { //same name as function in message hub
      recipientUsername: username,
      content
    })
    .catch(error => console.log(error));
  }

  async getMessageThread(recipientUsername: string, pageNumber: number, pageSize: number) {
    if(this.availablePages.includes(pageNumber)) return;

    return this.hubConnection.invoke<Message[]>('GetMessageThread', {
      recipientUsername,
      pageNumber,
      pageSize
    })
    .then((receivedMessages: Message[]) => {
      if(receivedMessages){
        let newMessages: Message[];
  
        this.messageThread$.pipe(take(1)).subscribe(messages => {
          newMessages = [...receivedMessages, ...messages];
          this.messageThreadSource.next(newMessages);
          this.availablePages.push(pageNumber);
        });
      }
    })
    .catch(error => console.log(error));
  }

  deleteMessage(id: number) {
    return this.http.delete(this.baseUrl + 'messages/' + id);
  }
}
