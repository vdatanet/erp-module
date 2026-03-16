using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Comun;

namespace erp.Module.BusinessObjects.Contactos;

[DefaultClassOptions]
[NavigationItem("Contactos")]
[ImageName("BO_Address")]
public class Domicilio(Session session) : Contacto(session)
{
    private Cliente _clienteDomicilio;

    [XafDisplayName("Cliente")]
    [Association("Cliente-Domicilios")]
    public Cliente ClienteDomicilio
    {
        get => _clienteDomicilio;
        set => SetPropertyValue(nameof(ClienteDomicilio), ref _clienteDomicilio, value);
    }
}
