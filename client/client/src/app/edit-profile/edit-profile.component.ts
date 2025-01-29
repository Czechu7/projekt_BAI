import { HttpClient } from '@angular/common/http';
import { Component, ElementRef, inject, ViewChild, ViewEncapsulation } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({
  selector: 'app-edit-profile',
  standalone: true,
  imports: [],
  templateUrl: './edit-profile.component.html',
  styleUrl: './edit-profile.component.css',


})
export class EditProfileComponent {
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  avatarUrl: string | null = null; 
  selectedFile: File | null = null; 
  isDragOver = false; 
  http = inject(HttpClient);
  accountService = inject(AccountService)
  sanitizer = inject(DomSanitizer);
  usernamePreview: SafeHtml = ''; 

  triggerFileInput() {
    this.fileInput.nativeElement.click();
  }
  onUsernameChange(event: Event) {
    const input = event.target as HTMLInputElement;
    this.usernamePreview = this.sanitizer.bypassSecurityTrustHtml(input.value); 
  }
  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;

    if (input.files && input.files[0]) {
      this.loadFile(input.files[0]);
    }
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    this.isDragOver = false;

    if (event.dataTransfer && event.dataTransfer.files.length > 0) {
      this.loadFile(event.dataTransfer.files[0]);
    }
  }

  loadFile(file: File) {
    this.selectedFile = file;

    const reader = new FileReader();
    reader.onload = (e: any) => {
      this.avatarUrl = e.target.result;
    };
    reader.readAsDataURL(file);
  }

  uploadAvatar() {
    if (!this.selectedFile) {
      alert('No file selected!');
      return;
    }
    const currentUser = this.accountService.currentUser(); 
    const username = currentUser?.username ? currentUser.username : "";
        const formData = new FormData();

    const manipulatedFileName = `${this.selectedFile.name}`; //hardoce path traversal
    const manipulatedFile = new File([this.selectedFile], manipulatedFileName, {
    type: this.selectedFile.type,
    });

   formData.append('avatar', manipulatedFile);
   formData.append('userId', username ); 

    this.http.post('http://localhost:5000/api/account/setAvatar', formData).subscribe({
      next: (response) => {
        console.log('res:', response);
      },
      error: (err) => {
        console.error('Error:', err);
        alert('error.');
      },
    });
  }
 
  uploadAvatarSecure() {
    if (!this.selectedFile) {
      alert('No file selected!');
      return;
    }
  
    const allowedExtensions = ['image/jpeg', 'image/png'];
    if (!allowedExtensions.includes(this.selectedFile.type)) {
      alert('Only JPG and PNG files are allowed!');
      return;
    }
  
    const currentUser = this.accountService.currentUser(); 
    const username = currentUser?.username ? currentUser.username : "";
    const formData = new FormData();
  
    const manipulatedFileName = `${this.selectedFile.name}`;
    const manipulatedFile = new File([this.selectedFile], manipulatedFileName, {
      type: this.selectedFile.type,
    });
  
    formData.append('avatar', manipulatedFile);
    formData.append('userId', username);
  
    this.http.post('http://localhost:5000/api/account/setAvatarSecure', formData).subscribe({
      next: (response) => {
        console.log('res:', response);
      },
      error: (err) => {
        console.error('Error:', err);
        alert('error.');
      },
    });
  }


}
