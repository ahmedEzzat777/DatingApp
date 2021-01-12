import { Component, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { UnmoderatedPhoto } from 'src/app/_models/UnmoderatedPhoto';
import { AdminService } from 'src/app/_services/admin.service';
import { ConfirmService } from 'src/app/_services/confirm.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {
  photos: UnmoderatedPhoto[] = [];

  constructor(private adminService: AdminService, private toastr: ToastrService, private confirm: ConfirmService) { }

  ngOnInit(): void {
    this.loadUnmoderatedPhotos();
  }

  loadUnmoderatedPhotos(){
    this.adminService.getPhotosForModeration().subscribe(photos => {
        this.photos = photos;
      });
  }

  allowPhoto(photoId: number){
    this.adminService.allowPhoto(photoId).subscribe(() =>{
      this.photos = this.photos.filter(p => p.id != photoId);
      this.toastr.success('photo allowed');
    });
  }

  deletePhoto(photoId: number){
    this.confirm.confirm().subscribe(result => {
      if(result){
        this.adminService.deletePhoto(photoId).subscribe(() =>{
          this.photos = this.photos.filter(p => p.id != photoId);
          this.toastr.warning('photo deleted');
        });
      }
    });
  }
}
