using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.Helpers.Contactos;
using OneSignalApi.Api;
using OneSignalApi.Client;
using OneSignalApi.Model;
using DevExpress.Xpo;

namespace erp.Module.Services.Comunicaciones;

public interface IOneSignalService
{
    Task<string> SendEmailAsync(string toEmail, string subject, string body, string? fromEmail = null, string? fromName = null);
}

public class OneSignalService : IOneSignalService
{
    private readonly Session _session;

    public OneSignalService(Session session)
    {
        _session = session;
    }

    public async Task<string> SendEmailAsync(string toEmail, string subject, string body, string? fromEmail = null, string? fromName = null)
    {
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(_session);
        
        if (companyInfo == null)
        {
            throw new InvalidOperationException("No se ha configurado la información de la empresa.");
        }

        if (string.IsNullOrEmpty(companyInfo.OneSignalAppId) || string.IsNullOrEmpty(companyInfo.OneSignalRestApiKey))
        {
            throw new InvalidOperationException("OneSignal no está configurado correctamente en la información de la empresa.");
        }

        var configuration = new Configuration();
        configuration.BasePath = "https://onesignal.com/api/v1";
        configuration.AccessToken = companyInfo.OneSignalRestApiKey;

        var apiInstance = new DefaultApi(configuration);

        var notification = new Notification(appId: companyInfo.OneSignalAppId)
        {
            IncludeEmailTokens = [toEmail],
            EmailSubject = subject,
            EmailBody = body,
            EmailFromName = fromName ?? companyInfo.OneSignalDefaultEmailName,
            EmailFromAddress = fromEmail ?? companyInfo.OneSignalDefaultEmailFrom
        };

        try
        {
            var result = await apiInstance.CreateNotificationAsync(notification);
            return result.Id;
        }
        catch (ApiException e)
        {
            throw new Exception($"Error al enviar email con OneSignal: {e.Message}", e);
        }
    }
}
