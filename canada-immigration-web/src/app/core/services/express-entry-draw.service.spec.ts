import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { ExpressEntryDrawService } from './express-entry-draw.service';
import { ExpressEntryDraw, PagedResult, PoolDistribution } from '../../models/express-entry-draw.model';

describe('ExpressEntryDrawService', () => {
  let service: ExpressEntryDrawService;
  let httpMock: HttpTestingController;

  const baseUrl = 'http://localhost:5088/api/ExpressEntryDraws';

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ExpressEntryDrawService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(ExpressEntryDrawService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('deve criar o serviço', () => {
    expect(service).toBeTruthy();
  });

  describe('getDraws', () => {
    it('deve chamar a URL correta sem filtros e devolver os draws paginados', () => {
      const mockResponse: PagedResult<ExpressEntryDraw> = {
        items: [
          { drawNumber: 441, date: '2026-09-04', invitationsIssued: 3500, minimumCRS: 486, category: 'CEC', year: '2026' },
        ],
        page: 1,
        pageSize: 20,
        totalCount: 1,
        totalPages: 1,
      };

      service.getDraws(1).subscribe((result) => {
        expect(result).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(
        (r) => r.url === baseUrl && r.params.get('page') === '1' && r.params.get('pageSize') === '20'
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });

    it('deve incluir year e category como query params quando fornecidos', () => {
      const mockResponse: PagedResult<ExpressEntryDraw> = {
        items: [],
        page: 2,
        pageSize: 20,
        totalCount: 0,
        totalPages: 0,
      };

      service.getDraws(2, 20, 2026, 'PNP').subscribe((result) => {
        expect(result).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(
        (r) =>
          r.url === baseUrl &&
          r.params.get('page') === '2' &&
          r.params.get('year') === '2026' &&
          r.params.get('category') === 'PNP'
      );
      req.flush(mockResponse);
    });
  });

  describe('getLatestDraw', () => {
    it('deve chamar /latest e devolver o draw mais recente', () => {
      const mockDraw: ExpressEntryDraw = {
        drawNumber: 441,
        date: '2026-09-04',
        invitationsIssued: 3500,
        minimumCRS: 486,
        category: null,
        year: '2026',
      };

      service.getLatestDraw().subscribe((result) => {
        expect(result).toEqual(mockDraw);
      });

      const req = httpMock.expectOne(`${baseUrl}/latest`);
      expect(req.request.method).toBe('GET');
      req.flush(mockDraw);
    });
  });

  describe('getPoolDistribution', () => {
    it('deve chamar /pool e devolver a distribuição do pool', () => {
      const mockPool: PoolDistribution = {
        drawNumber: 441,
        drawDate: '2026-09-04',
        totalCandidates: 226673,
        ranges: [
          { key: 'dd2', range: '501-600', value: 19542 },
        ],
      };

      service.getPoolDistribution().subscribe((result) => {
        expect(result).toEqual(mockPool);
      });

      const req = httpMock.expectOne(`${baseUrl}/pool`);
      expect(req.request.method).toBe('GET');
      req.flush(mockPool);
    });
  });
});