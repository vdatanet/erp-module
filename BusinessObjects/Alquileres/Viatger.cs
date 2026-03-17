using System.ComponentModel;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Contactos;

namespace erp.Module.BusinessObjects.Alquileres;

[DefaultClassOptions]
[NavigationItem("Alquileres")]
[ImageName("BO_Contact")]
[DefaultProperty(nameof(NomComplet))]
public class Viatger(Session session) : Contacto(session)
{
    private Parentius? _parentiu;
    private Nacionalitat _nacionalitat;
    private DateTime _expedicio;
    private DateTime _neixement;
    private string _nomComplet;
    private Cliente.Sexes _sexe;
    private Cliente.TipusDocuments _tipusDocument;
    private string _mobil;
    private string _mail;

    private string CalcularNomComplet() => string.IsNullOrWhiteSpace(Nombre) ? "" : $"{Nombre} {NombreComercial}".Trim();

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Sexe = Cliente.Sexes.Femeni;
        TipusDocument = Cliente.TipusDocuments.Nif;
        var nacionalitat = Session.FindObject<Nacionalitat>(CriteriaOperator.Parse("Nom = 'Espanyola'"));
        if (nacionalitat != null) Nacionalitat = nacionalitat;
    }

    [Association("Client-Viatgers")]
    [XafDisplayName("Client")]
    public new Cliente Cliente
    {
        get => base.Cliente;
        set
        {
            if (value != base.Cliente && !IsLoading && !IsSaving && value != null)
            {
                if (value.Nif != null) Nif = value.Nif;
                if (value.Nombre != null) Nombre = value.Nombre;
                if (value.NombreComercial != null) NombreComercial = value.NombreComercial;
                if (value.Direccion != null) Direccion = value.Direccion;
                if (value.CodigoPostal != null) CodigoPostal = value.CodigoPostal;
                if (value.Provincia != null) Provincia = value.Provincia;
                if (value.Pais != null) Pais = value.Pais;
                if (value.Telefono != null) base.Telefono = value.Telefono;
                if (value.Movil != null) Movil = value.Movil;
                if (value.CorreoElectronico != null) CorreoElectronico = value.CorreoElectronico;
                if (value.Notas != null) Notas = value.Notas;
            }
            base.Cliente = value;
        }
    }

    [XafDisplayName("Data de neixement")]
    public DateTime Neixement
    {
        get => _neixement;
        set => SetPropertyValue(nameof(Neixement), ref _neixement, value);
    }

    [XafDisplayName("Data expedició")]
    public DateTime Expedicio
    {
        get => _expedicio;
        set => SetPropertyValue(nameof(Expedicio), ref _expedicio, value);
    }

    [XafDisplayName("Sexe")]
    public Cliente.Sexes Sexe
    {
        get => _sexe;
        set => SetPropertyValue(nameof(Sexe), ref _sexe, value);
    }

    [XafDisplayName("Tipus document identificatiu")]
    public Cliente.TipusDocuments TipusDocument
    {
        get => _tipusDocument;
        set => SetPropertyValue(nameof(TipusDocument), ref _tipusDocument, value);
    }

    [Association("Nacionalitat-Viatgers")]
    [XafDisplayName("Nacionalitat")]
    public Nacionalitat Nacionalitat
    {
        get => _nacionalitat;
        set => SetPropertyValue(nameof(Nacionalitat), ref _nacionalitat, value);
    }

    [XafDisplayName("Parentiu")]
    public Parentius? Parentiu
    {
        get => _parentiu;
        set => SetPropertyValue(nameof(Parentiu), ref _parentiu, value);
    }

    [XafDisplayName("Nom Complet")]
    public string NomComplet
    {
        get
        {
            if (string.IsNullOrEmpty(_nomComplet))
                _nomComplet = CalcularNomComplet();
            return _nomComplet;
        }
        set => SetPropertyValue(nameof(NomComplet), ref _nomComplet, value);
    }

    [XafDisplayName("Telèfon mòbil")]
    public string Mobil
    {
        get => _mobil;
        set => SetPropertyValue(nameof(Mobil), ref _mobil, value);
    }

    [XafDisplayName("Correu electrònic")]
    public string Mail
    {
        get => _mail;
        set => SetPropertyValue(nameof(Mail), ref _mail, value);
    }

    [Association("Reserva-Viatgers")]
    [XafDisplayName("Reserves")]
    public XPCollection<Reserva> Reserves => GetCollection<Reserva>();

    public enum Parentius
    {
        [XafDisplayName("Fill/a")]
        Fill
    }
}
