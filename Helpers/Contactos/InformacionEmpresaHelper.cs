using DevExpress.Xpo;
using erp.Module.BusinessObjects.Configuraciones;

namespace erp.Module.Helpers.Contactos;

public static class InformacionEmpresaHelper
{
    public static InformacionEmpresa GetInformacionEmpresa(Session session)
    {
        var info = session.Query<InformacionEmpresa>().FirstOrDefault();
        if (info != null) return info;
        info = new InformacionEmpresa(session)
        {
            Nombre = "Mi Empresa",
            Nif = "B00000000"
        };
        info.Save();
        return info;
    }
}