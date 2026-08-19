using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NumuneKabul.Application.Interfaces;

namespace NumuneKabul.Infrastructure.Services.Adapters;

/// <summary>
/// Mock REST Servisi entegrasyon adaptörü.
/// Gerçek HTTP çağrıları yerine simüle edilmiş (rastgele başarılı/başarısız) gönderim yapar.
/// </summary>
public class MockRestIntegrationAdapter : IIntegrationAdapter
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MockRestIntegrationAdapter> _logger;

    public MockRestIntegrationAdapter(HttpClient httpClient, ILogger<MockRestIntegrationAdapter> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> SendAsync(string payload, int referenceId)
    {
        _logger.LogInformation("Mock REST Servisine gerçek HTTP gönderimi başlatılıyor... [RefId: {RefId}]", referenceId);

        try
        {
            var content = new StringContent(payload, Encoding.UTF8, "application/xml");
            
            // Timeout yönetimi HttpClientFactory üzerinde konfigüre edilecek
            var response = await _httpClient.PostAsync("", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Mock REST Servis HTTP gönderimi BAŞARILI. (Status: {StatusCode}) [RefId: {RefId}]", response.StatusCode, referenceId);
                return true;
            }
            else
            {
                _logger.LogWarning("Mock REST Servis HTTP gönderimi BAŞARISIZ. (Status: {StatusCode}) [RefId: {RefId}]", response.StatusCode, referenceId);
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Mock REST Servis HTTP bağlantı hatası! [RefId: {RefId}]", referenceId);
            return false;
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Mock REST Servis HTTP isteği zaman aşımına uğradı (Timeout)! [RefId: {RefId}]", referenceId);
            return false;
        }
    }
}
