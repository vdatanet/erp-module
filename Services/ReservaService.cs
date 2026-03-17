using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Alquileres;
using erp.Module.Helpers.Comun;

namespace erp.Module.Services;

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
            {
                object suma = session.Evaluate(
                    typeof(PrecioDiario),
                    CriteriaOperator.Parse("Sum(Precio)"),
                    CriteriaOperator.Parse(
                        "Tarifa.Oid = ? AND Fecha >= ? AND Fecha < ?",
                        objeto.RecursoAlquilable.Tarifa.Oid,
                        startOn,
                        endOn));
                objeto.ImporteAlojamiento = Convert.ToDecimal(suma);
            }
            else
            {
                objeto.ImporteAlojamiento = 0;
            }

            // Parking
            if (objeto.Parking)
            {
                object suma = session.Evaluate(
                    typeof(PrecioDiario),
                    CriteriaOperator.Parse("Sum(Precio)"),
                    CriteriaOperator.Parse("Tarifa.Nombre = 'P' AND Fecha >= ? AND Fecha < ?", startOn, endOn));
                objeto.ImporteParking = Convert.ToDecimal(suma);
            }
            else
            {
                objeto.ImporteParking = 0;
            }

            // Aire acondicionado
            if (objeto.Ac)
            {
                var extraAc = session.FindObject<Extra>(CriteriaOperator.Parse("Nombre = 'Aire acondicionado'")) 
                             ?? session.FindObject<Extra>(CriteriaOperator.Parse("Nombre = 'Aire condicionat'"));
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
                var extraSabanas = session.FindObject<Extra>(CriteriaOperator.Parse("Nombre = 'Sábanas'"))
                                  ?? session.FindObject<Extra>(CriteriaOperator.Parse("Nombre = 'Llençols'"));
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
                var extraTasa = session.FindObject<Extra>(CriteriaOperator.Parse("Nombre = 'Tasa turística'"))
                                ?? session.FindObject<Extra>(CriteriaOperator.Parse("Nombre = 'Taxa turística'"));
                if (extraTasa != null)
                    objeto.ImporteTasaTuristica = MoneyMath.RoundMoney(diasSujetos * objeto.PersonasSujetas * extraTasa.PrecioDiario);
                else
                    objeto.ImporteTasaTuristica = 0;
            }
            else
            {
                objeto.ImporteTasaTuristica = 0;
            }

            objeto.Subtotal = objeto.ImporteAlojamiento + objeto.ImporteParking;
            objeto.Total = objeto.Subtotal + objeto.ImporteAc + objeto.ImporteSabanas + objeto.ImporteOtrosExtras - objeto.ImporteDescuento;
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
}
