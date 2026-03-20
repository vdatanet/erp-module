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
    private MediaDataObject? _qrCode;
    private DateTime? _creadoEl;
    private ApplicationUser? _creadoPor;
    private DateTime? _modificadoEl;
    private ApplicationUser? _modificadoPor;

    [XafDisplayName("Código QR")]
    [ModelDefault("AllowEdit", "False")]
    [ImageEditor(ListViewImageEditorMode = ImageEditorMode.PictureEdit, DetailViewImageEditorMode = ImageEditorMode.PictureEdit)]
    public MediaDataObject? QrCode
    {
        get => _qrCode;
        set => SetPropertyValue(nameof(QrCode), ref _qrCode, value);
    }

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
                GenerarQrCode();
            }
            else
            {
                ModificadoEl = DateTime.Now;
                ModificadoPor = GetCurrentUser();
            }
        }
        catch (Exception)
        {
        }
    }

    private void GenerarQrCode()
    {
        if (QrCode != null) return;
        
        try
        {
            var oid = Oid.ToString();
            var url = $"https://api.qrserver.com/v1/create-qr-code/?size=150x150&data={oid}";
            
            using var client = new HttpClient();
            var bytes = client.GetByteArrayAsync(url).Result;
            
            var media = new MediaDataObject(Session);
            media.MediaData = bytes;
            QrCode = media;
        }
        catch
        {
            // Silenciosamente fallar si no hay conexión o error en API
        }
    }

    private ApplicationUser? GetCurrentUser()
    {
        try
        {
            var serviceProvider = Session.ServiceProvider;
            if (serviceProvider == null)
            {
                return null;
            }

            var security = serviceProvider.GetService<ISecurityStrategyBase>();
            if (security == null)
            {
                return null;
            }

            if (security.UserId == null)
            {
                return null;
            }

            // Aquí es donde podría fallar si UserId no es Guid, 
            // aunque GetObjectByKey maneja object.
            return Session.GetObjectByKey<ApplicationUser>(security.UserId);
        }
        catch (Exception)
        {
            return null;
        }
    }
}