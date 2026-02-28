import { Routes } from '@angular/router';
import { Home } from './components/home/home';
import { BuyCar } from './components/buy-car/buy-car';
import { Fleet } from './components/fleet/fleet';
import { Dashboard } from './components/dashboard/dashboard';
import { Docs } from './components/docs/docs';
import { History } from './components/history/history';

export const routes: Routes = [{
        path: "",
        component: Home
    }, 
    {
        path: "buy",
        component: BuyCar
    },
    {
        path: "fleet",
        component: Fleet
    },
    {
        path: "dashboard",
        component: Dashboard
    },
    {
        path: "docs",
        component: Docs
    },
    {
        path: "history",
        component: History
    }
];
