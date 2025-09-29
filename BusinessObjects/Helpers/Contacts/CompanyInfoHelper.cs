using DevExpress.Xpo;
using erp.Module.BusinessObjects.Contacts;

namespace erp.Module.BusinessObjects.Helpers.Contacts;

public static class CompanyInfoHelper
{
    public static CompanyInfo GetCompanyInfo(Session session)
    {
        var info = session.Query<CompanyInfo>().FirstOrDefault();
        if (info != null) return info;
        info = new CompanyInfo(session);
        info.Save();
        return info;
    }
}