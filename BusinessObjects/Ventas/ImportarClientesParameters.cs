using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;

namespace erp.Module.BusinessObjects.Ventas;

[DomainComponent]
public class ImportarClientesParameters
{
    [XafDisplayName("Archivo CSV")]
    public FileData Archivo { get; set; }
}
