import { Component, DestroyRef, OnInit, inject } from '@angular/core';

import { LeaveRequestStatus } from '../../../../../enum/leave-request-status-enum';

import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ILeaveRequestHistory, ILeaveRequestHistoryResponse } from '../../../../../interface/leave-request-history';
import { LeaveManagementService } from '../../../../../service/leave-management.service';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { NgSelectModule } from '@ng-select/ng-select';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-leave-history',
  standalone: true,
  imports: [MatPaginatorModule, NgSelectModule, FormsModule],
  templateUrl: './leave-history.component.html',
  styleUrl: './leave-history.component.scss'
})

export class LeaveHistoryComponent implements OnInit {

  id: number = 19;
  leaveRequestList: ILeaveRequestHistory[] = [];
  leaveManagementService = inject(LeaveManagementService);
  pageNumber: number = 0;
  pageSize: number = 2;
  pageSizes: number[] = [2, 3, 5, 10, 20, 50];
  totalPages: number = 1;
  destroyRef = inject(DestroyRef);
  indexValue: number = (this.pageNumber * this.pageSize) - this.pageSize + 1;
  selectedDescription: string | null = null;
  selectedStatus: number = 0;

  leaveStatusId =
    [
      { id: 1, name: 'Open' },
      { id: 2, name: 'Approved' },
      { id: 3, name: 'Rejected' },
      { id: 4, name: 'Deleted' }
    ];

  constructor() { }

  ngOnInit(): void {
    this.loadLeaveRequests();
  }

  viewDescription(description: string): void {
    this.selectedDescription = description;
  }

  closeModal(): void {
    this.selectedDescription = null;
  }

  requestStatus(leaveRequest: number): string {

    switch (leaveRequest) {
      case LeaveRequestStatus.Open:
        return 'Open';
      case LeaveRequestStatus.Approved:
        return 'Approved';
      case LeaveRequestStatus.Rejected:
        return 'Rejected';
      case LeaveRequestStatus.Deleted:
        return 'Deleted';
      default:
        return 'Unknown Status';
    }
  }

  loadLeaveRequests(): void {
    this.leaveManagementService.getLeaveRequestHistory(this.id, this.pageNumber, this.pageSize, this.selectedStatus)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((data: ILeaveRequestHistoryResponse) => {
        this.indexValue = ((this.pageNumber + 1) * this.pageSize) - this.pageSize + 1;
        this.leaveRequestList = data.leaveRequests;
        this.totalPages = data.totalPages;
      });
  }

  onPageChangeEvent(event: PageEvent) {
    this.pageNumber = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadLeaveRequests();
  }

  onApply() {
    this.selectedStatus = this.selectedStatus;
    this.loadLeaveRequests();
  }

  onEdit(i: number) {

  }

  onDelete(i: number) {

  }

}