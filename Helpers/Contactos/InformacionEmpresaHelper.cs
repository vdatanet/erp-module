using DevExpress.Xpo;
using erp.Module.BusinessObjects.Configuraciones;

namespace erp.Module.Helpers.Contactos;

public static class InformacionEmpresaHelper
{
    public static InformacionEmpresa GetInformacionEmpresa(Session session)
    {
        return session.Query<InformacionEmpresa>().FirstOrDefault();
    }
}