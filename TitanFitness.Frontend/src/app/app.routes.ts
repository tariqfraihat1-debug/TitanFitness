import { Routes } from '@angular/router';
import { PortalLayout } from './shared/components/portal-layout/portal-layout';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    path: '',
    component: PortalLayout,
    children: [
      {
        path: 'dashboard',
        data: { title: 'Dashboard' },
        loadChildren: () => import('./routing-modules/dashboard-routing')
          .then(m => m.DASHBOARD_ROUTES)
      },
      {
        path: 'members',
        data: { title: 'Members' },
        loadChildren: () => import('./routing-modules/members-routing')
          .then(m => m.MEMBERS_ROUTES)
      },
      {
        path: 'classes',
        data: { title: 'Classes' },
        loadChildren: () => import('./routing-modules/classes-routing')
          .then(m => m.CLASSES_ROUTES)
      },
      {
        path: 'trainers',
        data: { title: 'Trainers' },
        loadChildren: () => import('./routing-modules/trainers-routing')
          .then(m => m.TRAINERS_ROUTES)
      },
      {
        path: 'plans',
        data: { title: 'Plans' },
        loadChildren: () => import('./routing-modules/plans-routing')
          .then(m => m.PLANS_ROUTES)
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];  
