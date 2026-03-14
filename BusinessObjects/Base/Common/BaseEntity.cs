using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using Microsoft.Extensions.DependencyInjection;

namespace erp.Module.BusinessObjects.Base.Common;

[NonPersistent]
[ModelDefault("IsCloneable", "True")]
public abstract class BaseEntity(Session session) : BaseObject(session)
{
    private ApplicationUser _creadoPor;
    private ApplicationUser _modificadoPor;
    private DateTime? _creadoEl;
    private DateTime? _modificadoEl;

    [HideInUI(HideInUI.All)]
    [ModelDefault(nameof(IModelCommonMemberViewItem.AllowEdit), "False")]
    public ApplicationUser CreadoPor
    {
        get => _creadoPor;
        set => SetPropertyValue(nameof(CreadoPor), ref _creadoPor, value);
    }

    [HideInUI(HideInUI.All)]
    [ModelDefault(nameof(IModelCommonMemberViewItem.AllowEdit), "False")]
    public ApplicationUser ModificadoPor
    {
        get => _modificadoPor;
        set => SetPropertyValue(nameof(ModificadoPor), ref _modificadoPor, value);
    }

    [HideInUI(HideInUI.All)]
    [ModelDefault(nameof(IModelCommonMemberViewItem.AllowEdit), "False")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    public DateTime? CreadoEl
    {
        get => _creadoEl;
        set => SetPropertyValue(nameof(CreadoEl), ref _creadoEl, value);
    }

    [HideInUI(HideInUI.All)]
    [ModelDefault(nameof(IModelCommonMemberViewItem.AllowEdit), "False")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    public DateTime? ModificadoEl
    {
        get => _modificadoEl;
        set => SetPropertyValue(nameof(ModificadoEl), ref _modificadoEl, value);
    }

    protected override void OnSaving()
    {
        base.OnSaving();
        if (Session.IsNewObject(this))
        {
            SecuredPropertySetter.SetPropertyValueWithSecurityBypass(this, nameof(CreadoEl), DateTime.Now);
            SecuredPropertySetter.SetPropertyValueWithSecurityBypass(this, nameof(CreadoPor), GetCurrentUser());
        }
        else
        {
            SecuredPropertySetter.SetPropertyValueWithSecurityBypass(this, nameof(ModificadoEl), DateTime.Now);
            SecuredPropertySetter.SetPropertyValueWithSecurityBypass(this, nameof(ModificadoPor), GetCurrentUser());
        }
    }

    private ApplicationUser GetCurrentUser()
    {
        return Session.GetObjectByKey<ApplicationUser>(
            Session.ServiceProvider.GetRequiredService<ISecurityStrategyBase>().UserId);
    }
}