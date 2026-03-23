using System;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using Microsoft.Extensions.DependencyInjection;

namespace erp.Module.BusinessObjects.Base.Comun;

[ModelDefault("IsCloneable", "True")]
[NonPersistent]
public abstract class EntidadBase(Session session) : BaseObject(session)
{
    private DateTime? _creadoEl;
    private ApplicationUser? _creadoPor;
    private DateTime? _modificadoEl;
    private ApplicationUser? _modificadoPor;
    private string? _barCodeString;

    [HideInUI(HideInUI.All)]
    [ModelDefault(nameof(IModelCommonMemberViewItem.AllowEdit), "False")]
    [NonCloneable]
    [XafDisplayName("Creado por")]
    public ApplicationUser? CreadoPor
    {
        get => _creadoPor;
        set => SetPropertyValue(nameof(CreadoPor), ref _creadoPor, value);
    }

    [HideInUI(HideInUI.All)]
    [ModelDefault(nameof(IModelCommonMemberViewItem.AllowEdit), "False")]
    [NonCloneable]
    [XafDisplayName("Modificado por")]
    public ApplicationUser? ModificadoPor
    {
        get => _modificadoPor;
        set => SetPropertyValue(nameof(ModificadoPor), ref _modificadoPor, value);
    }

    [HideInUI(HideInUI.All)]
    [ModelDefault(nameof(IModelCommonMemberViewItem.AllowEdit), "False")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    [NonCloneable]
    [XafDisplayName("Creado el")]
    public DateTime? CreadoEl
    {
        get => _creadoEl;
        set => SetPropertyValue(nameof(CreadoEl), ref _creadoEl, value);
    }

    [HideInUI(HideInUI.All)]
    [ModelDefault(nameof(IModelCommonMemberViewItem.AllowEdit), "False")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    [NonCloneable]
    [XafDisplayName("Modificado el")]
    public DateTime? ModificadoEl
    {
        get => _modificadoEl;
        set => SetPropertyValue(nameof(ModificadoEl), ref _modificadoEl, value);
    }

    [XafDisplayName("Código Barras")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.AllowEdit), "False")]
    [Size(16)]
    [Indexed]
    public string? BarCodeString
    {
        get => _barCodeString;
        set => SetPropertyValue(nameof(BarCodeString), ref _barCodeString, value);
    }

    protected override void OnSaving()
    {
        base.OnSaving();

        if (Session.IsNewObject(this))
        {
            CreadoEl = DateTime.Now;
            CreadoPor = GetCurrentUser();
            BarCodeString = Oid.ToString("N").Substring(0, 16).ToUpperInvariant();
        }
        else
        {
            ModificadoEl = DateTime.Now;
            ModificadoPor = GetCurrentUser();
        }
    }

    private ApplicationUser? GetCurrentUser()
    {
        try
        {
            var serviceProvider = Session.ServiceProvider;

            var security = serviceProvider?.GetService<ISecurityStrategyBase>();

            return security?.UserId == null
                ? null
                :
                // Aquí es donde podría fallar si UserId no es Guid, 
                // aunque GetObjectByKey maneja object.
                Session.GetObjectByKey<ApplicationUser>(security.UserId);
        }
        catch (Exception)
        {
            return null;
        }
    }
}