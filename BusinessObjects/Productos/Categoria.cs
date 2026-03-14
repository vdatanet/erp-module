using System.Text;
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
    private string _nombre;
    private Categoria _categoriaPadre;
    private bool _estaActivo;
    private bool _disponibleEnTpV;
    private string _notas;

    [Size(255)]
    [RuleRequiredField]
    [RuleUniqueValue]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Association("Categoria-Subcategorias")]
    public Categoria CategoriaPadre
    {
        get => _categoriaPadre;
        set => SetPropertyValue(nameof(CategoriaPadre), ref _categoriaPadre, value);
    }

    public bool EstaActivo
    {
        get => _estaActivo;
        set => SetPropertyValue(nameof(EstaActivo), ref _estaActivo, value);
    }

    public bool DisponibleEnTpv
    {
        get => _disponibleEnTpV;
        set => SetPropertyValue(nameof(DisponibleEnTpv), ref _disponibleEnTpV, value);
    }

    [Size(1000)]
    public string Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    public string RutaCompleta
    {
        get
        {
            var sb = new StringBuilder();
            Categoria current = this;
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
    public XPCollection<Categoria> Subcategorias => GetCollection<Categoria>(nameof(Subcategorias));

    [Association("Categoria-Productos")] 
    public XPCollection<Producto> Productos => GetCollection<Producto>(nameof(Productos));

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