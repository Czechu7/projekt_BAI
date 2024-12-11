import { Component, inject } from '@angular/core';
import { AccountService } from '../../core/services/account.service';
import { Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { FormsModule } from '@angular/forms';
import { TitleCasePipe } from '@angular/common';
import { materialModules } from '../../shared/material.imports';

@Component({
  selector: 'app-nav',
  imports: [RouterLink,FormsModule,TitleCasePipe,...materialModules],
  templateUrl: './nav.component.html',
  styleUrl: './nav.component.scss'
})
export class NavComponent {
  accountService = inject(AccountService)
  private router = inject(Router)
  private toastr = inject(ToastrService)
  model: any = {};
  error: any;

  login() {
    this.accountService.login(this.model).subscribe({
      next: response => {
        console.log(response);
        this.router.navigateByUrl('/members');
      },
      error: error => this.toastr.error(error.error)
    })
  }

  logout(){
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }
}