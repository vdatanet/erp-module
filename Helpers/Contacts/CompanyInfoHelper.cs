using DevExpress.Xpo;
using erp.Module.BusinessObjects.Settings;

namespace erp.Module.Helpers.Contacts;

public static class CompanyInfoHelper
{
    public static CompanyInfo GetCompanyInfo(Session session)
    {
        var info = session.Query<CompanyInfo>().FirstOrDefault();
        if (info != null) return info;
        info = new CompanyInfo(session);
        info.Nombre = "Mi Empresa";
        info.Nif = "B00000000";
        info.Save();
        return info;
    }
}