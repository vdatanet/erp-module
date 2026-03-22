using System.Collections;
using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Contabilidad;

[DefaultClassOptions]
[NavigationItem("Contabilidad")]
[XafDisplayName("Ejercicio")]
[DefaultProperty(nameof(Anio))]
[ImageName("Action_Calendar")]
public class Ejercicio(Session session) : EntidadBase(session)
{
    private int? _anio;
    private EstadoEjercicio _estado;
    private DateTime _fechaInicio;
    private DateTime _fechaFin;
    private string? _notas;

    [XafDisplayName("Ejercicio (Año)")]
    [RuleRequiredField]
    public int? Anio
    {
        get => _anio;
        set => SetPropertyValue(nameof(Anio), ref _anio, value);
    }

    [XafDisplayName("Estado")]
    public EstadoEjercicio Estado
    {
        get => _estado;
        set => SetPropertyValue(nameof(Estado), ref _estado, value);
    }

    [XafDisplayName("Fecha Inicio")]
    [RuleRequiredField]
    public DateTime FechaInicio
    {
        get => _fechaInicio;
        set => SetPropertyValue(nameof(FechaInicio), ref _fechaInicio, value);
    }

    [XafDisplayName("Fecha Fin")]
    [RuleRequiredField]
    public DateTime FechaFin
    {
        get => _fechaFin;
        set => SetPropertyValue(nameof(FechaFin), ref _fechaFin, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [Action(Caption = "Renumerar Apuntes del Ejercicio", ConfirmationMessage = "Se va a proceder a renumerar todos los asientos del ejercicio por fecha. ¿Desea continuar?", ImageName = "Action_Refresh", TargetObjectsCriteria = "Estado = 'Abierto'")]
    public void RenumerarAsientos()
    {
        var asientosList = new List<Asiento>();
        foreach (Asiento a in Asientos)
        {
            asientosList.Add(a);
        }
        var asientosOrdenados = asientosList.OrderBy(a => a.Fecha).ThenBy(a => a.Orden).ToList();
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        int padding = companyInfo?.PaddingNumero ?? 5;
        int nuevoNumero = 1;
        foreach (var asiento in asientosOrdenados)
        {
            asiento.Numero = nuevoNumero++;
            asiento.Codigo = string.Format("{0}/{1}", asiento.Serie, asiento.Numero.ToString().PadLeft(padding, '0'));
        }
        Session.Save((IEnumerable)asientosOrdenados);
    }

    public void ToggleEstadoEjercicio()
    {
        switch (Estado)
        {
            case EstadoEjercicio.Abierto:
                Estado = EstadoEjercicio.Cerrado;
                break;
            case EstadoEjercicio.Cerrado:
                Estado = EstadoEjercicio.Bloqueado;
                break;
            case EstadoEjercicio.Bloqueado:
                Estado = EstadoEjercicio.Abierto;
                break;
        }
    }

    [Association("Ejercicio-Asientos")]
    [XafDisplayName("Asientos")]
    public XPCollection<Asiento> Asientos => GetCollection<Asiento>(nameof(Asientos));

    [Association("Ejercicio-PeriodosBloqueados")]
    [XafDisplayName("Periodos Bloqueados")]
    public XPCollection<PeriodoBloqueado> PeriodosBloqueados => GetCollection<PeriodoBloqueado>(nameof(PeriodosBloqueados));

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Estado = EstadoEjercicio.Abierto;
        int anioActual = DateTime.Today.Year;
        Anio = anioActual;
        FechaInicio = new DateTime(anioActual, 1, 1);
        FechaFin = new DateTime(anioActual, 12, 31);
    }
}

public enum EstadoEjercicio
{
    Abierto,
    Cerrado,
    Bloqueado
}
