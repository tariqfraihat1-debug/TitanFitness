import { Routes } from '@angular/router';
import { BookSession } from '../pages/classes/book-session/book-session';
import { ClassSchedule } from '../pages/classes/class-schedule/class-schedule';

export const CLASSES_ROUTES: Routes = [
  {
    path: '',
    component: ClassSchedule,
    data: { title: 'Class Schedule' }
  },
  {
    path: 'book-session',
    component: BookSession,
    data: { title: 'Book Session' }
  }
];
