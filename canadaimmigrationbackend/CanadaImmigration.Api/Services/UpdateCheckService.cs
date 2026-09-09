using System;
using System.Threading.Tasks;

namespace CanadaImmigration.Api.Services
{
    public class UpdateCheckService
    {
        private readonly EmailNotifier _emailNotifier;
        private readonly IExpressEntryDrawService _expressEntryDrawService;
        private DateTime _lastCheckTime;

        public UpdateCheckService(EmailNotifier emailNotifier, IExpressEntryDrawService expressEntryDrawService)
        {
            _emailNotifier = emailNotifier;
            _expressEntryDrawService = expressEntryDrawService;
            _lastCheckTime = DateTime.Now;
        }

        public async Task CheckForUpdatesAsync()
        {
            try
            {
                var currentData = await _expressEntryDrawService.GetLatestDrawAsync();
                
                if (currentData != null)
                {
                    var updateDetails = $@"
                        Draw Number: {currentData.DrawNumber}
                        Data da Verificação: {DateTime.Now:dd/MM/yyyy HH:mm:ss}
                        Última verificação: {_lastCheckTime:dd/MM/yyyy HH:mm:ss}
                    ";

                    await _emailNotifier.SendUpdateNotificationAsync(
                        "Nova Atualização Detectada",
                        updateDetails
                    );
                    _lastCheckTime = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao verificar atualizações: {ex.Message}");
            }
        }
    }
}