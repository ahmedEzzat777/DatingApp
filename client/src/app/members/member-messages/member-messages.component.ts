import { AfterViewChecked, ChangeDetectionStrategy, Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit,AfterViewChecked {
  @ViewChild('messageForm') messageForm: NgForm;
  @ViewChild('scrollMe') myScrollContainer: ElementRef;
  @Input() username: string;
  @Input() pageNumber: number;
  @Input() pageSize: number;
  messageContent: string;
  loading = false;

  constructor(public messageService: MessageService) { }

  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }
  

  ngOnInit(): void {
    this.scrollToBottom();
  }

  scrollToBottom() {
    try {
        this.myScrollContainer.nativeElement.scrollTop = this.myScrollContainer.nativeElement.scrollHeight;
    } catch(err) { }                 
  }

  scrollToTop() {
    try {
        this.myScrollContainer.nativeElement.scrollBottom = this.myScrollContainer.nativeElement.scrollHeight;
    } catch(err) { }                 
  }


  sendMessage() {
    this.loading = true;
    this.messageService.sendMessage(this.username, this.messageContent).then(() => {
      this.messageForm.reset();
      this.scrollToBottom();
    }).finally(() => {
      this.loading = false;
    });
  }

  onScroll(){
    this.pageNumber++;
    console.log('scrolled'+this.pageNumber);
    this.messageService.getMessageThread(this.username, this.pageNumber, this.pageSize);
  }
}
