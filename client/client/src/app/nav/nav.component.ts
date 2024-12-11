import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../_services/account.service';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { TitleCasePipe } from '@angular/common';
@Component({
  selector: 'app-nav',
  standalone: true,
  imports: [FormsModule, BsDropdownModule,RouterLink, RouterLinkActive, TitleCasePipe],
  templateUrl: './nav.component.html',
  styleUrl: './nav.component.css'
})
export class NavComponent {
  accountService = inject(AccountService)
  private router = inject(Router)
  private toastr = inject(ToastrService)
  model: any = {};

  showTotpForm = false;
  totpCode: string = '';

  login() {
    this.accountService.login(this.model).subscribe({
      next: response => {
        console.log('TOTP Code:', response.totpCode);
        this.showTotpForm = true;
        this.toastr.info('Please enter the TOTP code shown in the console');
      },
      error: error => this.toastr.error(error.error)
    });
  }

  verifyTotp() {
    this.accountService.verifyTotp(this.totpCode).subscribe({
      next: () => {
        this.showTotpForm = false;
        this.router.navigateByUrl('/members');
        this.toastr.success('Successfully logged in');
      },
      error: error => {
        this.toastr.error(error.error);
        this.totpCode = '';
      }
    });
  }


  logout(){
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }
}
