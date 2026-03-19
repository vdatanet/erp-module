using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using Microsoft.Extensions.DependencyInjection;

namespace erp.Module.BusinessObjects.Base.Comun;

[NonPersistent]
[ModelDefault("IsCloneable", "True")]
public abstract class EntidadBase(Session session) : BaseObject(session)
{
    private DateTime? _creadoEl;
    private ApplicationUser? _creadoPor;
    private DateTime? _modificadoEl;
    private ApplicationUser? _modificadoPor;

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

    protected override void OnSaving()
    {
        base.OnSaving();
        try
        {
            if (Session.IsNewObject(this))
            {
                CreadoEl = DateTime.Now;
                CreadoPor = GetCurrentUser();
            }
            else
            {
                ModificadoEl = DateTime.Now;
                ModificadoPor = GetCurrentUser();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG_LOG] Error en OnSaving de EntidadBase ({GetType().Name}): {ex.Message}");
        }
    }

    private ApplicationUser? GetCurrentUser()
    {
        try
        {
            var serviceProvider = Session.ServiceProvider;
            if (serviceProvider == null)
            {
                // Console.WriteLine("[DEBUG_LOG] GetCurrentUser: ServiceProvider es nulo.");
                return null;
            }

            var security = serviceProvider.GetService<ISecurityStrategyBase>();
            if (security == null)
            {
                // Console.WriteLine("[DEBUG_LOG] GetCurrentUser: ISecurityStrategyBase no encontrado.");
                return null;
            }

            if (security.UserId == null)
            {
                // Console.WriteLine("[DEBUG_LOG] GetCurrentUser: UserId es nulo.");
                return null;
            }

            // Aquí es donde podría fallar si UserId no es Guid, 
            // aunque GetObjectByKey maneja object.
            return Session.GetObjectByKey<ApplicationUser>(security.UserId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG_LOG] Excepción en GetCurrentUser de EntidadBase ({GetType().Name}): {ex.Message}");
            return null;
        }
    }
}