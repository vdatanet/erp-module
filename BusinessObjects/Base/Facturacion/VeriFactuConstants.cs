using DevExpress.ExpressApp.DC;

namespace erp.Module.BusinessObjects.Base.Facturacion;

public enum EstadoVeriFactu
{
    Borrador,
    Validado,
    PendienteVeriFactu,
    EnviadaVeriFactu,
    AceptadaVeriFactu,
    RechazadaVeriFactu,
    Impresa,
    ErrorTecnico
}

public static class VeriFactuConstants
{
    public const string Correcto = "Correcto";
    public const string Error = "Error";
    public const string Parcial = "Parcial";
}

public static class VeriFactuEndPointPrefixes
{
    public const string Prod = "https://www1.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/ssii/fact/ws/VeriFactuFE.wsdl";
    public const string ProdValidate = "https://www1.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/ssii/fact/ws/VeriFactuFE_Validacion.wsdl";
}

public enum TipoFacturaAmigable
{
    [XafDisplayName("Factura Completa")] F1,
    [XafDisplayName("Factura Simplificada")] F2,
    [XafDisplayName("Factura en Sustitución de Simplificadas")] F3,
    [XafDisplayName("Resumen de Facturas")] F4,
    [XafDisplayName("Factura Rectificativa (Art. 80.1, 80.2 y 80.6)")] R1,
    [XafDisplayName("Factura Rectificativa (Art. 80.3)")] R2,
    [XafDisplayName("Factura Rectificativa (Art. 80.4)")] R3,
    [XafDisplayName("Factura Rectificativa (Resto)")] R4,
    [XafDisplayName("Factura Rectificativa en Facturas Simplificadas")] R5
}

public enum TipoRectificativaAmigable
{
    [XafDisplayName("Sustitutiva")] S,
    [XafDisplayName("Diferencias")] I
}

public enum TipoIdentificacionAmigable
{
    [XafDisplayName("02 - NIF-IVA")] NIF_IVA = 2,
    [XafDisplayName("03 - Pasaporte")] Pasaporte = 3,
    [XafDisplayName("04 - Documento oficial de identificación expedido por el país o territorio de residencia")] DocumentoOficial = 4,
    [XafDisplayName("05 - Certificado de residencia")] CertificadoResidencia = 5,
    [XafDisplayName("06 - Otro documento probatorio")] OtroDocumento = 6,
    [XafDisplayName("07 - No censado")] NoCensado = 7
}

public enum TipoImpuestoAmigable
{
    [XafDisplayName("IVA - Impuesto sobre el Valor Añadido")] IVA = 1,
    [XafDisplayName("IPSI - Impuesto sobre la Producción, los Servicios y la Importación")] IPSI = 2,
    [XafDisplayName("IGIC - Impuesto General Indirecto Canario")] IGIC = 3
}

public enum TipoOperacionAmigable
{
    [XafDisplayName("S1 - Sujeta - No Exenta")] S1 = 1,
    [XafDisplayName("S2 - Sujeta - Exenta")] S2 = 2,
    [XafDisplayName("S3 - Sujeta - No Exenta (ISP)")] S3 = 3
}

public enum CausaExencionAmigable
{
    [XafDisplayName("E1 - Exenta por el art. 20")] E1 = 1,
    [XafDisplayName("E2 - Exenta por el art. 21")] E2 = 2,
    [XafDisplayName("E3 - Exenta por el art. 22")] E3 = 3,
    [XafDisplayName("E4 - Exenta por el art. 24")] E4 = 4,
    [XafDisplayName("E5 - Exenta por el art. 25")] E5 = 5,
    [XafDisplayName("E6 - Otros")] E6 = 6
}
