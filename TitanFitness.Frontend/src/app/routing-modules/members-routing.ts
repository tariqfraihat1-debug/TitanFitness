import { Routes } from '@angular/router';
import { ChangeMembershipPlan } from '../pages/members/change-membership-plan/change-membership-plan';
import { MemberDetails } from '../pages/members/member-details/member-details';
import { MemberProfile } from '../pages/members/member-profile/member-profile';
import { Members } from '../pages/members/members';
import { FreezeMembership } from '../pages/members/freeze-membership/freeze-membership';

export const MEMBERS_ROUTES: Routes = [
  {
    path: '',
    component: Members,
    data: { title: 'Member Directory' }
  },
  {
    path: 'details',
    component: MemberDetails,
    data: { title: 'Member Details' }
  },
  {
    path: 'profile',
    component: MemberProfile,
    data: { title: 'Member Profile' }
  },
  {
    path: 'change-plan',
    component: ChangeMembershipPlan,
    data: { title: 'Change Membership Plan' }
  },
  {
    path: 'freeze',
    component: FreezeMembership,
    data: { title: 'Freeze Membership' }
  }
];
