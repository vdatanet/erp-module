using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using erp.Module.BusinessObjects.Alquileres;
using erp.Module.Helpers.Comun;

namespace erp.Module.Services.Reserva;

public interface IReservaService
{
    void Calcular(IReservaCalculable objeto);
    void CalcularDescuento(IReservaCalculable objeto);
}

public interface IReservaCalculable
{
    Session Session { get; }
    DateTime StartOn { get; }
    DateTime EndOn { get; }
    double Dias { get; }
    RecursoAlquilable? RecursoAlquilable { get; }
    bool Alojamiento { get; }
    bool Parking { get; }
    bool Ac { get; }
    int PersonasSabanas { get; }
    int PersonasSujetas { get; }
    decimal ImporteAlojamiento { get; set; }
    decimal ImporteParking { get; set; }
    decimal ImporteAc { get; set; }
    decimal ImporteSabanas { get; set; }
    decimal ImporteTasaTuristica { get; set; }
    decimal ImporteOtrosExtras { get; set; }
    decimal ImporteDescuento { get; set; }
    decimal PerDescuento { get; set; }
    decimal Subtotal { get; set; }
    decimal Total { get; set; }
    decimal TotalTasaTuristicaIncluida { get; set; }
}

public class ReservaService : IReservaService
{
    public void Calcular(IReservaCalculable objeto)
    {
        if (objeto.EndOn > objeto.StartOn)
        {
            var session = objeto.Session;
            var startOn = objeto.StartOn.Date;
            var endOn = objeto.EndOn.Date;

            // Alojamiento
            if (objeto.Alojamiento && objeto.RecursoAlquilable != null && objeto.RecursoAlquilable.Tarifa != null)
                objeto.ImporteAlojamiento = CalcularImporteTarifa(objeto.RecursoAlquilable.Tarifa, startOn, endOn);
            else
                objeto.ImporteAlojamiento = 0;

            // Parking
            if (objeto.Parking)
            {
                var tarifaParking = session.FindObject<Tarifa>(CriteriaOperator.Parse("Nombre = 'Parking'"))
                                    ?? session.FindObject<Tarifa>(CriteriaOperator.Parse("Nombre = 'P'"));
                if (tarifaParking != null)
                    objeto.ImporteParking = CalcularImporteTarifa(tarifaParking, startOn, endOn);
                else
                    objeto.ImporteParking = 0;
            }
            else
            {
                objeto.ImporteParking = 0;
            }

            // Aire acondicionado
            if (objeto.Ac)
            {
                var extraAc = session.FindObject<Extra>(CriteriaOperator.Parse("Nombre = 'Aire condicionat'"))
                              ?? session.FindObject<Extra>(CriteriaOperator.Parse("Nombre = 'Aire acondicionado'"));
                if (extraAc != null)
                    objeto.ImporteAc = MoneyMath.RoundMoney((decimal)objeto.Dias * extraAc.PrecioDiario);
                else
                    objeto.ImporteAc = 0;
            }
            else
            {
                objeto.ImporteAc = 0;
            }

            // Sábanas / Llençols
            if (objeto.PersonasSabanas > 0)
            {
                var extraSabanas = session.FindObject<Extra>(CriteriaOperator.Parse("Nombre = 'Llençols'"))
                                   ?? session.FindObject<Extra>(CriteriaOperator.Parse("Nombre = 'Sábanas'"));
                if (extraSabanas != null)
                    objeto.ImporteSabanas = MoneyMath.RoundMoney(objeto.PersonasSabanas * extraSabanas.PrecioDiario);
                else
                    objeto.ImporteSabanas = 0;
            }
            else
            {
                objeto.ImporteSabanas = 0;
            }

            // Tasa turística (máximo 7 días)
            if (objeto.PersonasSujetas > 0)
            {
                var diasSujetos = (decimal)objeto.Dias;
                if (diasSujetos > 7) diasSujetos = 7;
                var extraTasa = session.FindObject<Extra>(CriteriaOperator.Parse("Nombre = 'Taxa turística'"))
                                ?? session.FindObject<Extra>(CriteriaOperator.Parse("Nombre = 'Tasa turística'"));
                if (extraTasa != null)
                    objeto.ImporteTasaTuristica =
                        MoneyMath.RoundMoney(diasSujetos * objeto.PersonasSujetas * extraTasa.PrecioDiario);
                else
                    objeto.ImporteTasaTuristica = 0;
            }
            else
            {
                objeto.ImporteTasaTuristica = 0;
            }

            objeto.Subtotal = objeto.ImporteAlojamiento + objeto.ImporteParking;
            objeto.Total = objeto.Subtotal + objeto.ImporteAc + objeto.ImporteSabanas + objeto.ImporteOtrosExtras -
                           objeto.ImporteDescuento;
            objeto.TotalTasaTuristicaIncluida = objeto.Total + objeto.ImporteTasaTuristica;

            if (objeto.Subtotal > 0)
                objeto.PerDescuento = MoneyMath.RoundMoney(objeto.ImporteDescuento / objeto.Subtotal * 100);
        }
    }

    public void CalcularDescuento(IReservaCalculable objeto)
    {
        objeto.ImporteDescuento = MoneyMath.RoundMoney(objeto.Subtotal * objeto.PerDescuento / 100);
        Calcular(objeto);
    }

    private decimal CalcularImporteTarifa(Tarifa tarifa, DateTime startOn, DateTime endOn)
    {
        decimal total = 0;
        var detalles = tarifa.Session.GetObjects(tarifa.Session.GetClassInfo<DetalleTarifa>(),
            CriteriaOperator.Parse("Tarifa.Oid = ? AND Desde < ? AND Hasta >= ?", tarifa.Oid, endOn, startOn),
            new SortingCollection(new SortProperty(nameof(DetalleTarifa.Desde), SortingDirection.Ascending)),
            0, false, true);

        foreach (DetalleTarifa detalle in detalles)
        {
            var inicioTramo = detalle.Desde > startOn ? detalle.Desde : startOn;
            var finTramo = detalle.Hasta < endOn.AddDays(-1) ? detalle.Hasta : endOn.AddDays(-1);

            if (finTramo >= inicioTramo)
            {
                var dias = (finTramo - inicioTramo).Days + 1;
                total += dias * detalle.Precio;
            }
        }

        return total;
    }
}