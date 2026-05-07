import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

@NgModule({
  imports: [CommonModule, ReactiveFormsModule, HttpClientModule, RouterModule],
  exports: [CommonModule, ReactiveFormsModule, HttpClientModule, RouterModule]
})
export class SharedModule {}
