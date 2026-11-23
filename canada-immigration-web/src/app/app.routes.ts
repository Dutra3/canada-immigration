import { Routes } from '@angular/router';
import { DrawsListComponent } from './features/draws/draws-list/draws-list.component';
import { ProofOfFundsComponent } from './features/proof-of-funds/proof-of-funds.component';
import { PoolChartComponent } from './features/pools/pool-chart/pool-chart.component';
import { SubscribeComponent } from './features/subscribe/subscribe.component';
import { CrsTrendComponent } from './features/trends/crs-trend/crs-trend.component';
import { CrsCalculatorComponent } from './features/crs-calculator/crs-calculator.component';
import { UnsubscribeComponent } from './features/unsubscribe/unsubscribe.component';

export const routes: Routes = [
  { path: '', redirectTo: 'draws', pathMatch: 'full' },
  {
    path: 'draws',
    component: DrawsListComponent,
    data: {
      title: 'Express Entry Draws',
      description: 'Histórico das chamadas do Express Entry: programa, data, convites emitidos e pontuação CRS mínima, com filtros por ano e categoria.'
    }
  },
  {
    path: 'proof-of-funds',
    component: ProofOfFundsComponent,
    data: {
      title: 'Proof of Funds (IRCC)',
      description: 'Calcule o valor mínimo em fundos exigido pelo IRCC para imigração canadense de acordo com o número de familiares.'
    }
  },
  {
    path: 'pool',
    component: PoolChartComponent,
    data: {
      title: 'Pool de CRS',
      description: 'Distribuição dos candidatos do Express Entry por faixa de pontuação CRS no draw mais recente.'
    }
  },
  {
    path: 'trends',
    component: CrsTrendComponent,
    data: {
      title: 'Tendência do CRS',
      description: 'Evolução da pontuação CRS mínima dos draws do Express Entry ao longo do tempo.'
    }
  },
  {
    path: 'crs-calculator',
    component: CrsCalculatorComponent,
    data: {
      title: 'Calculadora CRS',
      description: 'Estime sua nota no Comprehensive Ranking System do Express Entry e veja em quais categorias você seria convidado.'
    }
  },
  {
    path: 'subscribe',
    component: SubscribeComponent,
    data: {
      title: 'Alertas de novos draws',
      description: 'Cadastre seu e-mail e receba alertas quando uma nova chamada do Express Entry for publicada.'
    }
  },
  {
    path: 'unsubscribe',
    component: UnsubscribeComponent,
    data: {
      title: 'Cancelar inscrição',
      description: 'Cancele sua inscrição nos alertas de novos draws do Express Entry.'
    }
  },
];
