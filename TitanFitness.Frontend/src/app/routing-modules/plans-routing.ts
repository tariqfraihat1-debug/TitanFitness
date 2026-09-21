import { Routes } from '@angular/router';
import { PlanDetails } from '../pages/plans/plan-details/plan-details';
import { Plans } from '../pages/plans/plans';

export const PLANS_ROUTES: Routes = [
  {
    path: '',
    component: Plans,
    data: { title: 'Plan Catalogue' }
  },
  {
    path: 'details',
    component: PlanDetails,
    data: { title: 'Plan Details' }
  }
];
