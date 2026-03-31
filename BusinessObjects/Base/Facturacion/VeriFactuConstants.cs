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
    ErrorTecnico,
    [XafDisplayName("Pendiente")] Pendiente,
    [XafDisplayName("No Necesario")] NoNecesario
}

public static class VeriFactuConstants
{
    public const string Correcto = "Correcto";
    public const string Error = "Error";
    public const string Parcial = "Parcial";
    public const string PendienteVeriFactu = "PendienteVeriFactu";
    public const string Rechazada = "Rechazada";
    public const string ErrorTecnico = "ErrorTecnico";
}

public static class VeriFactuEndPointPrefixes
{
    public const string Prod = "https://www1.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/ssii/fact/ws/VeriFactuFE.wsdl";
    public const string ProdValidate = "https://www1.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/ssii/fact/ws/VeriFactuFE_Validacion.wsdl";
}

public enum TipoFactura
{
    F1, F2, F3, F4, R1, R2, R3, R4, R5
}

public enum TipoRectificativa
{
    S, I
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

public enum IDType
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
    [XafDisplayName("01 - IVA - Impuesto sobre el Valor Añadido")] IVA = 1,
    [XafDisplayName("02 - IPSI - Impuesto sobre la Producción, los Servicios y la Importación")] IPSI = 2,
    [XafDisplayName("03 - IGIC - Impuesto General Indirecto Canario")] IGIC = 3
}

public enum Impuesto
{
    [XafDisplayName("01 - IVA - Impuesto sobre el Valor Añadido")] IVA = 1,
    [XafDisplayName("02 - IPSI - Impuesto sobre la Producción, los Servicios y la Importación")] IPSI = 2,
    [XafDisplayName("03 - IGIC - Impuesto General Indirecto Canario")] IGIC = 3
}

public enum TipoOperacionAmigable
{
    [XafDisplayName("S1 - Sujeta - No Exenta")] S1 = 1,
    [XafDisplayName("S2 - Sujeta - Exenta")] S2 = 2,
    [XafDisplayName("S3 - Sujeta - No Exenta (ISP)")] S3 = 3
}

public enum CalificacionOperacion
{
    [XafDisplayName("S1 - Sujeta - No Exenta")] S1 = 1,
    [XafDisplayName("S2 - Sujeta - Exenta")] S2 = 2,
    [XafDisplayName("S3 - Sujeta - No Exenta (ISP)")] S3 = 3
}

public enum CausaExencion
{
    [XafDisplayName("E1 - Exenta por el art. 20")] E1 = 1,
    [XafDisplayName("E2 - Exenta por el art. 21")] E2 = 2,
    [XafDisplayName("E3 - Exenta por el art. 22")] E3 = 3,
    [XafDisplayName("E4 - Exenta por el art. 24")] E4 = 4,
    [XafDisplayName("E5 - Exenta por el art. 25")] E5 = 5,
    [XafDisplayName("E6 - Otros")] E6 = 6
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

public enum ClaveRegimen
{
    [XafDisplayName("01 - Régimen general")] General = 1,
    [XafDisplayName("02 - Exportación")] Exportacion = 2,
    [XafDisplayName("03 - Operaciones asimiladas a la exportación")] AsimiladaExportacion = 3,
    [XafDisplayName("04 - Régimen especial de bienes usados, objetos de arte, antigüedades y objetos de colección")] BienesUsados = 4,
    [XafDisplayName("05 - Régimen especial de las agencias de viajes")] AgenciasViajes = 5,
    [XafDisplayName("06 - Régimen especial de la agricultura, ganadería y pesca")] AgriculturaGanaderiaPesca = 6,
    [XafDisplayName("07 - Régimen especial del recargo de equivalencia")] RecargoEquivalencia = 7,
    [XafDisplayName("08 - Régimen especial de entidades sin fines lucrativos")] EntidadesSinFinesLucrativos = 8,
    [XafDisplayName("09 - Operaciones no sujetas a IVA")] NoSujeta = 9,
    [XafDisplayName("10 - Cobro por cuenta de terceros de honorarios profesionales")] CobroTerceros = 10,
    [XafDisplayName("11 - Operaciones que hayan tributado o vayan a tributar en el IGIC/IPSI")] IGIC_IPSI = 11,
    [XafDisplayName("12 - Régimen especial de criterio de caja")] CriterioCaja = 12,
    [XafDisplayName("13 - Régimen especial del grupo de entidades")] GrupoEntidades = 13,
    [XafDisplayName("14 - Facturación de las prestaciones de servicios de agencias de viajes que actúen como mediadoras en nombre y por cuenta ajena")] AgenciasViajesMediadoras = 14,
    [XafDisplayName("15 - Arrendamientos de locales de negocio")] ArrendamientosLocales = 15,
    [XafDisplayName("16 - Operaciones de arrendamiento de inmuebles sujetas a retención")] ArrendamientosInmueblesConRetencion = 16,
    [XafDisplayName("17 - Operaciones de arrendamiento de inmuebles no sujetas a retención")] ArrendamientosInmueblesSinRetencion = 17
}
