using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Ventas;
using System.Linq;

namespace erp.Module.BusinessObjects.Base.Ventas;

[XafDisplayName("Grupo del Documento")]
[ImageName("BO_Folder")]
public class DocumentoVentaGrupo(Session session) : EntidadBase(session)
{
    private DocumentoVenta? _documentoVenta;
    private DocumentoVentaGrupo? _padre;
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

    [Association("DocumentoVentaGrupo-Hijos")]
    [XafDisplayName("Padre")]
    public DocumentoVentaGrupo? Padre
    {
        get => _padre;
        set => SetPropertyValue(nameof(Padre), ref _padre, value);
    }

    [Association("DocumentoVentaGrupo-Hijos")]
    [XafDisplayName("Subgrupos")]
    public XPCollection<DocumentoVentaGrupo> Hijos => GetCollection<DocumentoVentaGrupo>(nameof(Hijos));

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

    [Association("DocumentoVentaGrupo-Lineas")]
    [XafDisplayName("Líneas")]
    public XPCollection<DocumentoVentaLinea> Lineas => GetCollection<DocumentoVentaLinea>(nameof(Lineas));

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
            var hijoDoc = new DocumentoVentaGrupo(Session)
            {
                DocumentoVenta = DocumentoVenta,
                Padre = this
            };
            hijoDoc.CopiarDeMaestro(hijoMaestro);
        }
    }
}
