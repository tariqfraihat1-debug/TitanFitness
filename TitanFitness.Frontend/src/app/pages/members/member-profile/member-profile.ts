import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError, finalize, forkJoin, map, of } from 'rxjs';
import { CurrentMembershipDto } from '../../../apis/members/current-membership.dto';
import { MemberActivityDto } from '../../../apis/members/member-activity.dto';
import { MemberDetailsDto } from '../../../apis/members/member-details.dto';
import { MembersService } from '../../../apis/members/members.service';
import { MembershipsService } from '../../../apis/memberships/memberships.service';
import { AlertMessage } from '../../../shared/components/alert-message/alert-message';
import { BackNavigation } from '../../../shared/components/back-navigation/back-navigation';
import { Button } from '../../../shared/components/button/button';
import { Loading } from '../../../shared/components/loading/loading';
import { CurrentPlan } from './components/current-plan/current-plan';
import { MemberIdentity } from './components/member-identity/member-identity';
import { RecentActivity } from './components/recent-activity/recent-activity';
import { UsageCard } from './components/usage-card/usage-card';

@Component({
  selector: 'app-member-profile',
  standalone: true,
  imports: [
    AlertMessage,
    BackNavigation,
    Button,
    Loading,
    CurrentPlan,
    MemberIdentity,
    RecentActivity,
    UsageCard
  ],
  templateUrl: './member-profile.html'
})
export class MemberProfile implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly membersService = inject(MembersService);
  private readonly membershipsService = inject(MembershipsService);

  memberId = signal<number | null>(null);
  member = signal<MemberDetailsDto | null>(null);
  membership = signal<CurrentMembershipDto | null>(null);
  activities = signal<MemberActivityDto[]>([]);
  loading = signal(true);
  error = signal('');

  readonly usageCards = computed(() => {
    const membership = this.membership();

    if (!membership)
      return [];

    return [
      {
        title: 'Freezes Used',
        used: membership.freezesUsed,
        allowed: membership.freezesAllowed,
        itemName: 'freeze'
      },
      {
        title: 'Guest Passes',
        used: membership.guestPassesUsed,
        allowed: membership.guestPassesAllowed,
        itemName: 'pass'
      }
    ];
  });

  // Reads the member ID and loads the profile.
  ngOnInit(): void {
    const memberId = Number(
      this.route.snapshot.queryParamMap.get('memberId')
    );

    if (!Number.isInteger(memberId) || memberId <= 0) {
      this.router.navigate(['/members']);
      return;
    }

    this.memberId.set(memberId);
    this.loadProfile(memberId);
  }

  // Returns to the members list.
  back(): void {
    this.router.navigate(['/members']);
  }

  // Opens the member form in edit mode.
  editProfile(): void {
    const memberId = this.memberId();

    if (!memberId)
      return;

    this.router.navigate(['/members/details'], {
      queryParams: {
        mode: 'edit',
        memberId
      }
    });
  }

  // Opens the change plan screen.
  changePlan(): void {
    const membership = this.membership();

    if (!membership)
      return;

    this.router.navigate(['/members/change-plan'], {
      queryParams: {
        memberId: this.memberId(),
        membershipId: membership.membershipId
      }
    });
  }

  // Opens the membership freeze screen.
  freezeMembership(): void {
    const membership = this.membership();

    if (!membership)
      return;

    this.router.navigate(['/members/freeze'], {
      queryParams: {
        memberId: this.memberId(),
        membershipId: membership.membershipId
      }
    });
  }

  // Renews the current membership and refreshes the profile.
  renewMembership(): void {
    const membership = this.membership();
    const memberId = this.memberId();

    if (!membership || !memberId)
      return;

    this.error.set('');

    this.membershipsService.renewMembership(
      membership.membershipId
    )
      .subscribe({
        next: () =>
          this.loadProfile(memberId),
        error: error =>
          this.error.set(error.message)
      });
  }

  // Loads member, membership, and recent activity together.
  private loadProfile(memberId: number): void {
    this.loading.set(true);
    this.error.set('');

    forkJoin({
      member: this.membersService.getMemberById(memberId)
        .pipe(
          map(response => response.data)
        ),

      membership: this.membersService.getCurrentMembership(memberId)
        .pipe(
          map(response => response.data),
          catchError(() => of(null))
        ),

      activities: this.membersService.getMemberActivity(memberId)
        .pipe(
          map(response => response.data),
          catchError(error => {
            this.addError(error.message);

            return of([]);
          })
        )
    })
      .pipe(
        finalize(() =>
          this.loading.set(false)
        )
      )
      .subscribe({
        next: response => {
          this.member.set(response.member);
          this.membership.set(response.membership);
          this.activities.set(response.activities);
        },
        error: error =>
          this.error.set(error.message)
      });
  }

  // Adds a unique error message to the page.
  private addError(message: string): void {
    const current = this.error();

    if (!current) {
      this.error.set(message);
      return;
    }

    if (!current.includes(message))
      this.error.set(`${current} ${message}`);
  }
}
