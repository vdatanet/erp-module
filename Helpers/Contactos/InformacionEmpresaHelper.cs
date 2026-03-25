using DevExpress.Xpo;
using erp.Module.BusinessObjects.Configuraciones;
using DevExpress.ExpressApp;

namespace erp.Module.Helpers.Contactos;

public static class InformacionEmpresaHelper
{
    public static InformacionEmpresa? GetInformacionEmpresa(Session session)
    {
        return session.Query<InformacionEmpresa>().FirstOrDefault();
    }

    public static DateTime GetLocalTime(Session session)
    {
        return GetInformacionEmpresa(session)?.GetLocalTime() ?? DateTime.Now;
    }

    public static DateTime GetLocalTime(IObjectSpace objectSpace)
    {
        return objectSpace.FindObject<InformacionEmpresa>(null)?.GetLocalTime() ?? DateTime.Now;
    }
}