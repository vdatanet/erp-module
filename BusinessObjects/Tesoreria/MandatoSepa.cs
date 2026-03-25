using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contactos;
using System.ComponentModel;

using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Tesoreria;

public enum TipoMandatoSepa
{
    [XafDisplayName("CORE")]
    CORE,
    [XafDisplayName("B2B")]
    B2B
}

public enum EstadoMandatoSepa
{
    [XafDisplayName("Activo")]
    Activo,
    [XafDisplayName("Revocado")]
    Revocado,
    [XafDisplayName("Pendiente")]
    Pendiente,
    [XafDisplayName("Caducado")]
    Caducado
}

[DefaultClassOptions]
[NavigationItem("Tesorería")]
[ImageName("BO_Contract")]
[XafDisplayName("Mandato SEPA")]
[DefaultProperty(nameof(Referencia))]
public class MandatoSepa(Session session) : EntidadBase(session)
{
    private string? _referencia;
    private string? _descripcion;
    private Contacto? _contacto;
    private TipoMandatoSepa? _tipo;
    private DateTime? _fechaFirma;
    private DateTime? _fechaPrimeraDomiciliacion;
    private EstadoMandatoSepa _estado;
    private Banco? _banco;
    private string? _iban;
    private string? _bic;
    private string? _titularCuenta;
    private DateTime? _fechaRevocacion;
    private string? _motivoRevocacion;
    private string? _notas;

    [RuleRequiredField]
    [RuleUniqueValue]
    [Size(50)]
    [XafDisplayName("Referencia")]
    public string? Referencia
    {
        get => _referencia;
        set => SetPropertyValue(nameof(Referencia), ref _referencia, value);
    }

    [Size(255)]
    [XafDisplayName("Descripción")]
    public string? Descripción
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripción), ref _descripcion, value);
    }

    [RuleRequiredField]
    [Association("Contacto-MandatosSepa")]
    [XafDisplayName("Contacto")]
    public Contacto? Contacto
    {
        get => _contacto;
        set => SetPropertyValue(nameof(Contacto), ref _contacto, value);
    }

    [RuleRequiredField]
    [XafDisplayName("Tipo SEPA")]
    public TipoMandatoSepa? Tipo
    {
        get => _tipo;
        set => SetPropertyValue(nameof(Tipo), ref _tipo, value);
    }

    [RuleRequiredField]
    [XafDisplayName("Fecha de Firma")]
    public DateTime? FechaFirma
    {
        get => _fechaFirma;
        set => SetPropertyValue(nameof(FechaFirma), ref _fechaFirma, value);
    }

    [XafDisplayName("Fecha 1ª Domiciliación")]
    public DateTime? FechaPrimeraDomiciliacion
    {
        get => _fechaPrimeraDomiciliacion;
        set => SetPropertyValue(nameof(FechaPrimeraDomiciliacion), ref _fechaPrimeraDomiciliacion, value);
    }

    [XafDisplayName("Estado")]
    public EstadoMandatoSepa Estado
    {
        get => _estado;
        set => SetPropertyValue(nameof(Estado), ref _estado, value);
    }

    [XafDisplayName("Banco")]
    [DataSourceProperty("Contacto.Bancos")]
    public Banco? Banco
    {
        get => _banco;
        set
        {
            if (SetPropertyValue(nameof(Banco), ref _banco, value) && !IsLoading && !IsSaving && value != null)
            {
                Iban = value.Iban;
                Bic = value.Bic;
            }
        }
    }

    [XafDisplayName("IBAN")]
    public string? Iban
    {
        get => _iban;
        set => SetPropertyValue(nameof(Iban), ref _iban, value);
    }

    [XafDisplayName("BIC / SWIFT")]
    public string? Bic
    {
        get => _bic;
        set => SetPropertyValue(nameof(Bic), ref _bic, value);
    }

    [XafDisplayName("Titular de la cuenta")]
    [ToolTip("Informar si difiere del contacto")]
    public string? TitularCuenta
    {
        get => _titularCuenta;
        set => SetPropertyValue(nameof(TitularCuenta), ref _titularCuenta, value);
    }

    [XafDisplayName("Fecha de Revocación")]
    [ModelDefault("AllowEdit", "False")]
    public DateTime? FechaRevocacion
    {
        get => _fechaRevocacion;
        set => SetPropertyValue(nameof(FechaRevocacion), ref _fechaRevocacion, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Motivo de Revocación")]
    public string? MotivoRevocacion
    {
        get => _motivoRevocacion;
        set => SetPropertyValue(nameof(MotivoRevocacion), ref _motivoRevocacion, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [XafDisplayName("Activo")]
    public bool Activo => Estado == EstadoMandatoSepa.Activo && (Contacto?.Activo ?? false);

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Estado = EstadoMandatoSepa.Activo;
        FechaFirma = InformacionEmpresaHelper.GetLocalTime(Session).Date;
    }

    public void Revocar(string motivo)
    {
        Estado = EstadoMandatoSepa.Revocado;
        FechaRevocacion = InformacionEmpresaHelper.GetLocalTime(Session);
        MotivoRevocacion = motivo;
    }
}
