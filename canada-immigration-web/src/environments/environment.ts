export const environment = {
  production: true,
  // Same-origin: em produção o frontend e a API devem ser servidos do mesmo domínio
  // (ou atrás de um proxy que encaminhe /api). Ajuste se a API estiver em outro host.
  apiBaseUrl: '/api',
};