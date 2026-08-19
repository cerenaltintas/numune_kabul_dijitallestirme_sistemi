using System.Threading.Tasks;

namespace NumuneKabul.Application.Interfaces;

/// <summary>
/// Dış sistemlerle (LIS, HBYS, Mock REST vb.) haberleşme kuracak adaptörlerin ortak arayüzü.
/// Dış sistemlere gönderim yapmak için standart arayüz.
/// </summary>
public interface IIntegrationAdapter
{
    /// <summary>
    /// Dönüştürülmüş veriyi (payload) dış sisteme gönderir.
    /// </summary>
    /// <param name="payload">Gönderilecek veri (XML, JSON vb.)</param>
    /// <param name="referenceId">Loglama ve takip için ilgili belgenin veya işin ID'si</param>
    /// <returns>Gönderim başarılı ise true, değilse false döner</returns>
    Task<bool> SendAsync(string payload, int referenceId);
}
