using System.Text;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Productos;

[DefaultClassOptions]
[NavigationItem("Productos")]
[ImageName("BO_Product_Group")]
public class Categoria(Session session) : EntidadBase(session)
{
    private Categoria? _categoriaPadre;
    private bool _disponibleEnTpV;
    private bool _estaActivo;
    private string? _nombre;
    private string? _notas;

    [Size(255)]
    [RuleRequiredField("RuleRequiredField_Categoria_Nombre", DefaultContexts.Save, CustomMessageTemplate = "El Nombre de la Categoría es obligatorio")]
    [RuleUniqueValue]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
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

    [Size(1000)]
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