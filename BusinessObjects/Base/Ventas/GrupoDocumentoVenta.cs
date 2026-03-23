using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Ventas;
using System.Linq;

namespace erp.Module.BusinessObjects.Base.Ventas;

[XafDisplayName("Grupo del Documento")]
[ImageName("BO_Folder")]
public class GrupoDocumentoVenta(Session session) : EntidadBase(session)
{
    private DocumentoVenta? _documentoVenta;
    private GrupoDocumentoVenta? _padre;
    private string? _nombre;
    private int _orden;
    private GrupoMaestro? _grupoMaestro;

    [Association("DocumentoVenta-Grupos")]
    [XafDisplayName("Documento de Venta")]
    public DocumentoVenta? DocumentoVenta
    {
        get => _documentoVenta;
        set => SetPropertyValue(nameof(DocumentoVenta), ref _documentoVenta, value);
    }

    [Association("GrupoDocumentoVenta-Hijos")]
    [XafDisplayName("Padre")]
    public GrupoDocumentoVenta? Padre
    {
        get => _padre;
        set => SetPropertyValue(nameof(Padre), ref _padre, value);
    }

    [Association("GrupoDocumentoVenta-Hijos")]
    [XafDisplayName("Subgrupos")]
    public XPCollection<GrupoDocumentoVenta> Hijos => GetCollection<GrupoDocumentoVenta>(nameof(Hijos));

    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [XafDisplayName("Orden")]
    public int Orden
    {
        get => _orden;
        set => SetPropertyValue(nameof(Orden), ref _orden, value);
    }

    [XafDisplayName("Grupo Maestro")]
    public GrupoMaestro? GrupoMaestro
    {
        get => _grupoMaestro;
        set => SetPropertyValue(nameof(GrupoMaestro), ref _grupoMaestro, value);
    }

    [Association("GrupoDocumentoVenta-Lineas")]
    [XafDisplayName("Líneas")]
    public XPCollection<LineaDocumentoVenta> Lineas => GetCollection<LineaDocumentoVenta>(nameof(Lineas));

    [XafDisplayName("Base Imponible")]
    public decimal BaseImponible => Lineas.Sum(l => l.BaseImponible) + Hijos.Sum(h => h.BaseImponible);

    [XafDisplayName("Importe Impuestos")]
    public decimal ImporteImpuestos => Lineas.Sum(l => l.ImporteImpuestos) + Hijos.Sum(h => h.ImporteImpuestos);

    [XafDisplayName("Total")]
    public decimal ImporteTotal => Lineas.Sum(l => l.ImporteTotal) + Hijos.Sum(h => h.ImporteTotal);

    public void CopiarDeMaestro(GrupoMaestro maestro)
    {
        if (maestro == null) return;

        GrupoMaestro = maestro;
        Nombre = maestro.Nombre;
        Orden = maestro.Orden;

        foreach (var hijoMaestro in maestro.Hijos.Where(h => h.Activo))
        {
            var hijoDoc = new GrupoDocumentoVenta(Session)
            {
                DocumentoVenta = DocumentoVenta,
                Padre = this
            };
            hijoDoc.CopiarDeMaestro(hijoMaestro);
        }
    }
}
