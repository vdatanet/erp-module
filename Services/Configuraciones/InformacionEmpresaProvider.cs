using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.Models.Configuraciones;

namespace erp.Module.Services.Configuraciones;

public interface IInformacionEmpresaProvider
{
    InformacionEmpresaDto GetInformacionEmpresaDto(IObjectSpace objectSpace);
}

public class InformacionEmpresaProvider : IInformacionEmpresaProvider
{
    public InformacionEmpresaDto GetInformacionEmpresaDto(IObjectSpace objectSpace)
    {
        var info = objectSpace.FindObject<InformacionEmpresa>(null);
        if (info == null) return new InformacionEmpresaDto();

        return new InformacionEmpresaDto
        {
            Nombre = info.Nombre,
            NombreComercial = info.NombreComercial,
            Nif = info.Nif,
            Direccion = info.Direccion,
            CodigoPostal = info.CodigoPostal,
            Poblacion = info.Poblacion?.Nombre,
            Provincia = info.Provincia?.Nombre,
            Pais = info.Pais?.Nombre,
            Telefono = info.Telefono,
            Movil = info.Movil,
            CorreoElectronico = info.CorreoElectronico,
            SitioWeb = info.SitioWeb,
            Logo = info.Logo?.MediaData
        };
    }
}
