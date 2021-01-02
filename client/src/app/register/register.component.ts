import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();
  registerForm: FormGroup;
  maxDate: Date;
  validationErrors: string[] = [];

  constructor(
    private accountService: AccountService,
    private fb: FormBuilder,
    private router: Router) { }

  ngOnInit(): void {
    this.initializeForm();
    this.maxDate = new Date();
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
  }

  initializeForm(){
    this.registerForm = this.fb.group({
      gender: ['male'],
      username: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: ['',[Validators.required, this.matchValues('password')]]
    });
  }

  matchValues(matchTo: string): ValidatorFn {
    return(control: AbstractControl) => {
      if (control.parent && control.parent.controls) {
        const groupControls = control.parent.controls as { [key: string]: AbstractControl };
        return control?.value === groupControls[matchTo]?.value ? null : {isMatching: true};
      }
      return null;
    }
  }

  register(){
    this.accountService.register(this.registerForm.value).subscribe(() =>
      {
        this.router.navigateByUrl('/members');
      }, (error: string[]) =>
      {
        this.validationErrors = error;
      })
  }

  cancel(){
    this.cancelRegister.emit(false);
  }

}
