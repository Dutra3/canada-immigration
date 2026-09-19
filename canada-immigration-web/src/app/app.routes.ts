import { Routes } from '@angular/router';
import { DrawsListComponent } from './features/draws/draws-list/draws-list.component';
import { ProofOfFundsComponent } from './features/proof-of-funds/proof-of-funds.component';
import { PoolChartComponent } from './features/pools/pool-chart/pool-chart.component';
import { SubscribeComponent } from './features/subscribe/subscribe.component';
import { CrsTrendComponent } from './features/trends/crs-trend/crs-trend.component';

export const routes: Routes = [
  { path: '', redirectTo: 'draws', pathMatch: 'full' },
  { path: 'draws', component: DrawsListComponent },
  { path: 'proof-of-funds', component: ProofOfFundsComponent },
  { path: 'pool', component: PoolChartComponent },
  { path: 'trends', component: CrsTrendComponent },
  { path: 'subscribe', component: SubscribeComponent },
];
