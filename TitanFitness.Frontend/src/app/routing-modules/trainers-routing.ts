import { Routes } from '@angular/router';
import { TrainerDetails } from '../pages/trainers/trainer-details/trainer-details';
import { Trainers } from '../pages/trainers/trainers';

export const TRAINERS_ROUTES: Routes = [
  {
    path: '',
    component: Trainers,
    data: { title: 'Trainer Directory' }
  },
  {
    path: 'details',
    component: TrainerDetails,
    data: { title: 'Trainer Details' }
  }
];
