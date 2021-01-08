import { Component, OnInit } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { RolesMoadalComponent } from 'src/app/modals/roles-moadal/roles-moadal.component';
import { User } from 'src/app/_models/User';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {
  users: Partial<User[]>;
  bsModalRef: BsModalRef;

  constructor(private adminService: AdminService, private modalService: BsModalService) { }

  ngOnInit(): void {
    this.getUsersWithRoles();
  }

  getUsersWithRoles() {
    this.adminService.getUsersWithRoles().subscribe(users => {
      this.users = users;
    });
  }

  openRolesModal(user: User){
    const config = {
      class: 'modal-dialog-centered',
      initialState: {
        user,
        roles: this.getRolesArray(user)
      }
    };
    
    this.bsModalRef = this.modalService.show(RolesMoadalComponent, config);

    this.bsModalRef.content.updateSelectedRoles.subscribe((values: any[]) => {
      const rolesToUpdate = {
        roles: [...values.filter(el => el.checked === true).map(el => el.name)]
      };

      if(rolesToUpdate) {
        this.adminService.updateUserRoles(user.userName, rolesToUpdate.roles).subscribe(() => {
          user.roles = [...rolesToUpdate.roles];
        });
      }
    });
  }

  private getRolesArray(user: User) {
    const roles:any[] = [];
    const userRoles = user.roles;
    const availableRoles: any[] = [
      {name: 'Admin', value: 'Admin'},
      {name: 'Moderator', value: 'Moderator'},
      {name: 'Member', value: 'Member'},
    ];

    availableRoles.forEach(role => {
      const userRole = userRoles.find(userRole => userRole === role.name);

      if(userRole){
        role.checked = true;
      } else {
        role.checked = false;
      }

      roles.push(role);
    });

    return roles;
  }

}
