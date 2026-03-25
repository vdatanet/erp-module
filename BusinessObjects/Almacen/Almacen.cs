using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Productos;

namespace erp.Module.BusinessObjects.Almacen;

[DefaultClassOptions]
[NavigationItem("Almacén")]
[XafDisplayName("Almacén")]
[Persistent("Almacen")]
[DefaultProperty(nameof(Nombre))]
public class Almacen(Session session) : EntidadBase(session)
{
    private string? _codigo;
    private string? _nombre;
    private string? _descripcion;
    private bool _estaActivo;

    [RuleUniqueValue]
    [RuleRequiredField("RuleRequiredField_Almacen_Codigo", DefaultContexts.Save)]
    [XafDisplayName("Código")]
    public string? Codigo
    {
        get => _codigo;
        set => SetPropertyValue(nameof(Codigo), ref _codigo, value);
    }

    [RuleRequiredField("RuleRequiredField_Almacen_Nombre", DefaultContexts.Save)]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Descripción")]
    public string? Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    [XafDisplayName("Activo")]
    public bool EstaActivo
    {
        get => _estaActivo;
        set => SetPropertyValue(nameof(EstaActivo), ref _estaActivo, value);
    }

    [Association("Almacen-Movimientos")]
    [XafDisplayName("Movimientos")]
    public XPCollection<MovimientoAlmacen> Movimientos => GetCollection<MovimientoAlmacen>();

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        EstaActivo = true;
    }
}
