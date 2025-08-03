using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using Microsoft.Extensions.DependencyInjection;

namespace erp.Module.BusinessObjects.Common;

[NonPersistent]
public abstract class BaseEntity(Session session) : BaseObject(session)
{
    private ApplicationUser _createdBy;
    private ApplicationUser _modifiedBy;
    private DateTime? _createdOn;
    private DateTime? _modifiedOn;

    [HideInUI(HideInUI.All)]
    [ModelDefault(nameof(IModelCommonMemberViewItem.AllowEdit), "False")]
    public ApplicationUser CreatedBy
    {
        get => _createdBy;
        set => SetPropertyValue(nameof(CreatedBy), ref _createdBy, value);
    }

    [HideInUI(HideInUI.All)]
    [ModelDefault(nameof(IModelCommonMemberViewItem.AllowEdit), "False")]
    public ApplicationUser ModifiedBy
    {
        get => _modifiedBy;
        set => SetPropertyValue(nameof(ModifiedBy), ref _modifiedBy, value);
    }

    [HideInUI(HideInUI.All)]
    [ModelDefault(nameof(IModelCommonMemberViewItem.AllowEdit), "False")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    public DateTime? CreatedOn
    {
        get => _createdOn;
        set => SetPropertyValue(nameof(CreatedOn), ref _createdOn, value);
    }

    [HideInUI(HideInUI.All)]
    [ModelDefault(nameof(IModelCommonMemberViewItem.AllowEdit), "False")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    public DateTime? ModifiedOn
    {
        get => _modifiedOn;
        set => SetPropertyValue(nameof(ModifiedOn), ref _modifiedOn, value);
    }

    protected override void OnSaving()
    {
        base.OnSaving();
        if (Session.IsNewObject(this))
        {
            SecuredPropertySetter.SetPropertyValueWithSecurityBypass(this, nameof(CreatedOn), DateTime.Now);
            SecuredPropertySetter.SetPropertyValueWithSecurityBypass(this, nameof(CreatedBy), GetCurrentUser());
        }
        else
        {
            SecuredPropertySetter.SetPropertyValueWithSecurityBypass(this, nameof(ModifiedOn), DateTime.Now);
            SecuredPropertySetter.SetPropertyValueWithSecurityBypass(this, nameof(ModifiedBy), GetCurrentUser());
        }
    }

    private ApplicationUser GetCurrentUser()
    {
        return Session.GetObjectByKey<ApplicationUser>(
            Session.ServiceProvider.GetRequiredService<ISecurityStrategyBase>().UserId);
    }
}