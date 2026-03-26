using System.Text;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Productos;

[DefaultClassOptions]
[NavigationItem("Productos")]
[XafDisplayName("Categoría")]
[ImageName("BO_Product_Group")]
public class Categoria(Session session) : EntidadBase(session)
{
    private Categoria? _categoriaPadre;
    private bool _disponibleEnTpV;
    private bool _estaActivo;
    private string? _nombre;
    private string? _notas;
    private MediaDataObject? _foto;
    private MediaDataObject? _miniatura;

    [Size(255)]
    [RuleRequiredField("RuleRequiredField_Categoria_Nombre", DefaultContexts.Save, CustomMessageTemplate = "El Nombre de la Categoría es obligatorio")]
    [RuleUniqueValue]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }
    
    [XafDisplayName("Foto")]
    public MediaDataObject? Foto
    {
        get => _foto;
        set
        {
            var oldFoto = _foto;
            if (SetPropertyValue(nameof(Foto), ref _foto, value))
            {
                if (oldFoto != null)
                {
                    oldFoto.Changed -= Foto_Changed;
                }
                if (_foto != null)
                {
                    _foto.Changed += Foto_Changed;
                }
                if (!IsSaving)
                {
                    UpdateThumbnail(value);
                }
            }
        }
    }

    private void Foto_Changed(object sender, ObjectChangeEventArgs e)
    {
        if (e.PropertyName == "MediaData" && !IsSaving)
        {
            UpdateThumbnail(Foto);
        }
    }

    [XafDisplayName("Miniatura")]
    [ImageEditor(DetailViewImageEditorMode = ImageEditorMode.PictureEdit, ListViewImageEditorMode = ImageEditorMode.PictureEdit)]
    public MediaDataObject? Miniatura
    {
        get => _miniatura;
        set => SetPropertyValue(nameof(Miniatura), ref _miniatura, value);
    }

    private void UpdateThumbnail(MediaDataObject? sourceFoto)
    {
        if (sourceFoto?.MediaData != null)
        {
            Miniatura ??= new MediaDataObject(Session);
            Miniatura.MediaData = erp.Module.Helpers.ImageHelper.GetThumbnailBytes(sourceFoto.MediaData);
            OnChanged(nameof(Miniatura));
        }
        else
        {
            Miniatura = null;
        }
    }

    [Association("Categoria-Subcategorias")]
    [XafDisplayName("Categoría Padre")]
    public Categoria? CategoriaPadre
    {
        get => _categoriaPadre;
        set => SetPropertyValue(nameof(CategoriaPadre), ref _categoriaPadre, value);
    }

    [XafDisplayName("Activo")]
    public bool EstaActivo
    {
        get => _estaActivo;
        set => SetPropertyValue(nameof(EstaActivo), ref _estaActivo, value);
    }

    [XafDisplayName("Disponible en TPV")]
    public bool DisponibleEnTpv
    {
        get => _disponibleEnTpV;
        set => SetPropertyValue(nameof(DisponibleEnTpv), ref _disponibleEnTpV, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [XafDisplayName("Ruta Completa")]
    public string RutaCompleta
    {
        get
        {
            var sb = new StringBuilder();
            var current = this;
            while (current != null)
            {
                if (sb.Length > 0)
                    sb.Insert(0, " > ");
                sb.Insert(0, current.Nombre);
                current = current.CategoriaPadre;
            }

            return sb.ToString();
        }
    }

    [Association("Categoria-Subcategorias")]
    [XafDisplayName("Subcategorías")]
    public XPCollection<Categoria> Subcategorias => GetCollection<Categoria>();

    [Association("Categoria-Productos")]
    [XafDisplayName("Productos")]
    public XPCollection<Producto> Productos => GetCollection<Producto>();

    protected override void OnLoaded()
    {
        base.OnLoaded();
        if (_foto != null)
        {
            _foto.Changed += Foto_Changed;
        }
    }

    protected override void OnChanged(string propertyName, object oldValue, object newValue)
    {
        base.OnChanged(propertyName, oldValue, newValue);
        if (IsSaving) return;
        if (propertyName == nameof(Foto))
        {
            UpdateThumbnail(Foto);
        }
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    private void InitValues()
    {
        EstaActivo = true;
        DisponibleEnTpv = false;
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo == null) return;
    }
}