import { Component, inject } from '@angular/core';
import { AccountService } from '../../_services/account.service';

@Component({
  selector: 'app-member-list',
  standalone: true,
  imports: [],
  templateUrl: './member-list.component.html',
  styleUrl: './member-list.component.css'
})
export class MemberListComponent {
  accountService = inject(AccountService)
  users: any ;

  ngOnInit() {
    this.accountService.getUsers().subscribe({
      next: users => this.users = users,
      error: error => console.log(error),
      complete: () => console.log('Request has been completed')
    });
  }
}
