using DevExpress.Xpo;
using erp.Module.BusinessObjects.Common;

namespace erp.Module.BusinessObjects.Helpers;

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