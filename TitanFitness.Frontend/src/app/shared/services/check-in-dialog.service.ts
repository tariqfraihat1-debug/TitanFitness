import { Injectable, signal } from '@angular/core';
import { MemberListItemDto } from '../../apis/members/member-list-item.dto';

@Injectable({
  providedIn: 'root'
})
export class CheckInDialogService {
  private readonly openState = signal(false);
  private readonly memberState = signal<MemberListItemDto | null>(null);

  readonly isOpen = this.openState.asReadonly();
  readonly member = this.memberState.asReadonly();

  open(member: MemberListItemDto | null = null): void {
    this.memberState.set(member);
    this.openState.set(true);
  }

  close(): void {
    this.openState.set(false);
    this.memberState.set(null);
  }
}
