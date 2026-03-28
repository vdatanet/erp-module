using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ReportsV2;
using DevExpress.Persistent.BaseImpl;
using erp.Module.BusinessObjects.Tpv;

namespace erp.Module.Services.Setup;

public class ReportsSetupService(IObjectSpace objectSpace)
{
    public void CreateInitialData()
    {
        CreateFacturaSimplificadaReport();
    }

    private void CreateFacturaSimplificadaReport()
    {
        var reportName = "Ticket Factura Simplificada";
        var reportData = objectSpace.FirstOrDefault<ReportDataV2>(r => r.DisplayName == reportName);

        if (reportData == null)
        {
            reportData = objectSpace.CreateObject<ReportDataV2>();
            reportData.DisplayName = reportName;
            ((IReportDataV2Writable)reportData).SetDataType(typeof(FacturaSimplificada));
            reportData.IsInplaceReport = true;
            
            // Forzamos el guardado para asegurar que los cambios persistan si no se hace externamente
            objectSpace.CommitChanges();
            
            // Layout básico de reporte (simplificado para ticket)
            // En un entorno real, esto se cargaría de un archivo .repx
            // Por ahora, proporcionamos un layout mínimo funcional basado en el formato XML de XtraReports
            reportData.Content = GetFacturaSimplificadaDefaultLayout();
        }
    }

    private byte[] GetFacturaSimplificadaDefaultLayout()
    {
        // Este es un layout minimalista para una factura simplificada (Ticket)
        // Incluye cabecera de empresa, datos de factura, líneas y totales.
        string layout = @"<?xml version=""1.0"" encoding=""utf-8""?>
<XtraReportsLayoutSerializer SerializerVersion=""23.1.4.0"" Ref=""1"" ControlType=""DevExpress.XtraReports.UI.XtraReport, DevExpress.XtraReports.v23.1"" Name=""FacturaSimplificadaReport"" DisplayName=""Ticket Factura Simplificada"" PageWidth=""800"" PageHeight=""2000"" PaperKind=""Custom"" ReportUnit=""TenthsOfAMillimeter"" RollPaper=""True"">
  <Bands>
    <Item1 Ref=""2"" ControlType=""TopMarginBand"" Name=""TopMargin"" HeightF=""50"" />
    <Item2 Ref=""3"" ControlType=""BottomMarginBand"" Name=""BottomMargin"" HeightF=""50"" />
    <Item3 Ref=""4"" ControlType=""DetailBand"" Name=""Detail"" HeightF=""50"">
      <Controls>
        <Item1 Ref=""5"" ControlType=""XRLabel"" Name=""lblProducto"" TextAlignment=""TopLeft"" SizeF=""450,40"" LocationFloat=""0,0"">
          <ExpressionBindings>
            <Item1 Ref=""6"" EventName=""BeforePrint"" PropertyName=""Text"" Expression=""[NombreProducto]"" />
          </ExpressionBindings>
        </Item1>
        <Item2 Ref=""7"" ControlType=""XRLabel"" Name=""lblCantidad"" TextAlignment=""TopRight"" SizeF=""100,40"" LocationFloat=""450,0"">
          <ExpressionBindings>
            <Item1 Ref=""8"" EventName=""BeforePrint"" PropertyName=""Text"" Expression=""[Cantidad]"" />
          </ExpressionBindings>
        </Item2>
        <Item3 Ref=""9"" ControlType=""XRLabel"" Name=""lblTotalLinea"" TextAlignment=""TopRight"" SizeF=""150,40"" LocationFloat=""550,0"">
          <ExpressionBindings>
            <Item1 Ref=""10"" EventName=""BeforePrint"" PropertyName=""Text"" Expression=""[ImporteTotal]"" />
          </ExpressionBindings>
          <TextFormatString>{0:c2}</TextFormatString>
        </Item3>
      </Controls>
    </Item3>
    <Item4 Ref=""11"" ControlType=""ReportHeaderBand"" Name=""ReportHeader"" HeightF=""450"">
      <Controls>
        <Item1 Ref=""12"" ControlType=""XRLabel"" Name=""lblEmpresaNombre"" TextAlignment=""TopCenter"" SizeF=""700,50"" LocationFloat=""0,0"" Font=""Arial, 12pt, style=Bold"">
          <ExpressionBindings>
            <Item1 Ref=""13"" EventName=""BeforePrint"" PropertyName=""Text"" Expression=""?Empresa_Nombre"" />
          </ExpressionBindings>
        </Item1>
        <Item2 Ref=""14"" ControlType=""XRLabel"" Name=""lblEmpresaNif"" TextAlignment=""TopCenter"" SizeF=""700,40"" LocationFloat=""0,50"">
          <ExpressionBindings>
            <Item1 Ref=""15"" EventName=""BeforePrint"" PropertyName=""Text"" Expression=""'NIF: ' + ?Empresa_Nif"" />
          </ExpressionBindings>
        </Item2>
        <Item3 Ref=""16"" ControlType=""XRLabel"" Name=""lblEmpresaDir"" TextAlignment=""TopCenter"" SizeF=""700,40"" LocationFloat=""0,90"">
          <ExpressionBindings>
            <Item1 Ref=""17"" EventName=""BeforePrint"" PropertyName=""Text"" Expression=""?Empresa_Direccion"" />
          </ExpressionBindings>
        </Item3>
        <Item4 Ref=""18"" ControlType=""XRLabel"" Name=""lblTitulo"" Text=""FACTURA SIMPLIFICADA"" TextAlignment=""TopCenter"" SizeF=""700,50"" LocationFloat=""0,150"" Font=""Arial, 10pt, style=Bold"" />
        <Item5 Ref=""19"" ControlType=""XRLabel"" Name=""lblFactura"" TextAlignment=""TopLeft"" SizeF=""700,40"" LocationFloat=""0,210"">
          <ExpressionBindings>
            <Item1 Ref=""20"" EventName=""BeforePrint"" PropertyName=""Text"" Expression=""'Número: ' + [Secuencia]"" />
          </ExpressionBindings>
        </Item5>
        <Item6 Ref=""21"" ControlType=""XRLabel"" Name=""lblFecha"" TextAlignment=""TopLeft"" SizeF=""700,40"" LocationFloat=""0,250"">
          <ExpressionBindings>
            <Item1 Ref=""22"" EventName=""BeforePrint"" PropertyName=""Text"" Expression=""'Fecha: ' + [Fecha]"" />
          </ExpressionBindings>
          <TextFormatString>{0:dd/MM/yyyy HH:mm}</TextFormatString>
        </Item6>
        <Item7 Ref=""23"" ControlType=""XRLine"" Name=""line1"" SizeF=""700,10"" LocationFloat=""0,300"" />
        <Item8 Ref=""24"" ControlType=""XRLabel"" Name=""hdrProd"" Text=""Descripción"" SizeF=""450,40"" LocationFloat=""0,310"" Font=""Arial, 8pt, style=Bold"" />
        <Item9 Ref=""25"" ControlType=""XRLabel"" Name=""hdrCant"" Text=""Cant."" TextAlignment=""TopRight"" SizeF=""100,40"" LocationFloat=""450,310"" Font=""Arial, 8pt, style=Bold"" />
        <Item10 Ref=""26"" ControlType=""XRLabel"" Name=""hdrTotal"" Text=""Total"" TextAlignment=""TopRight"" SizeF=""150,40"" LocationFloat=""550,310"" Font=""Arial, 8pt, style=Bold"" />
        <Item11 Ref=""27"" ControlType=""XRLine"" Name=""line2"" SizeF=""700,10"" LocationFloat=""0,350"" />
      </Controls>
    </Item4>
    <Item5 Ref=""28"" ControlType=""ReportFooterBand"" Name=""ReportFooter"" HeightF=""400"">
      <Controls>
        <Item1 Ref=""29"" ControlType=""XRLine"" Name=""line3"" SizeF=""700,10"" LocationFloat=""0,0"" />
        <Item2 Ref=""30"" ControlType=""XRLabel"" Name=""lblTotalTexto"" Text=""TOTAL EUR"" TextAlignment=""TopLeft"" SizeF=""400,60"" LocationFloat=""0,20"" Font=""Arial, 14pt, style=Bold"" />
        <Item3 Ref=""31"" ControlType=""XRLabel"" Name=""lblTotalImporte"" TextAlignment=""TopRight"" SizeF=""300,60"" LocationFloat=""400,20"" Font=""Arial, 14pt, style=Bold"">
          <ExpressionBindings>
            <Item1 Ref=""32"" EventName=""BeforePrint"" PropertyName=""Text"" Expression=""[ImporteTotal]"" />
          </ExpressionBindings>
          <TextFormatString>{0:c2}</TextFormatString>
        </Item3>
        <Item4 Ref=""33"" ControlType=""XRLabel"" Name=""lblDesglose"" Text=""Desglose de IVA:"" TextAlignment=""TopLeft"" SizeF=""700,40"" LocationFloat=""0,100"" Font=""Arial, 8pt, style=Italic"" />
        <Item5 Ref=""34"" ControlType=""XRLabel"" Name=""lblBase"" TextAlignment=""TopLeft"" SizeF=""350,40"" LocationFloat=""0,140"" Font=""Arial, 8pt"">
          <ExpressionBindings>
            <Item1 Ref=""35"" EventName=""BeforePrint"" PropertyName=""Text"" Expression=""'Base: ' + [BaseImponible]"" />
          </ExpressionBindings>
          <TextFormatString>{0:c2}</TextFormatString>
        </Item5>
        <Item6 Ref=""36"" ControlType=""XRLabel"" Name=""lblCuota"" TextAlignment=""TopRight"" SizeF=""350,40"" LocationFloat=""350,140"" Font=""Arial, 8pt"">
          <ExpressionBindings>
            <Item1 Ref=""37"" EventName=""BeforePrint"" PropertyName=""Text"" Expression=""'Cuota: ' + [ImporteImpuestos]"" />
          </ExpressionBindings>
          <TextFormatString>{0:c2}</TextFormatString>
        </Item6>
        <Item7 Ref=""38"" ControlType=""XRBarCode"" Name=""qrCode"" SizeF=""200,200"" LocationFloat=""250,200"" Alignment=""TopCenter"">
          <Symbology Ref=""39"" ControlType=""DevExpress.XtraReports.UI.BarCode.QRCodeGenerator, DevExpress.XtraReports.v23.1"" />
          <ExpressionBindings>
            <Item1 Ref=""40"" EventName=""BeforePrint"" PropertyName=""Text"" Expression=""[UrlValidacion]"" />
          </ExpressionBindings>
        </Item7>
      </Controls>
    </Item5>
    <Item6 Ref=""41"" ControlType=""DetailReportBand"" Name=""DetailReport"" Level=""0"" DataMember=""Lineas"">
      <Bands>
        <Item1 Ref=""42"" ControlType=""DetailBand"" Name=""DetailLineas"" HeightF=""50"">
          <Controls>
             <Item1 Ref=""43"" ControlType=""XRLabel"" Name=""lblLProducto"" TextAlignment=""TopLeft"" SizeF=""450,40"" LocationFloat=""0,0"">
              <ExpressionBindings>
                <Item1 Ref=""44"" EventName=""BeforePrint"" PropertyName=""Text"" Expression=""[NombreProducto]"" />
              </ExpressionBindings>
            </Item1>
            <Item2 Ref=""45"" ControlType=""XRLabel"" Name=""lblLCantidad"" TextAlignment=""TopRight"" SizeF=""100,40"" LocationFloat=""450,0"">
              <ExpressionBindings>
                <Item1 Ref=""46"" EventName=""BeforePrint"" PropertyName=""Text"" Expression=""[Cantidad]"" />
              </ExpressionBindings>
            </Item2>
            <Item3 Ref=""47"" ControlType=""XRLabel"" Name=""lblLTotal"" TextAlignment=""TopRight"" SizeF=""150,40"" LocationFloat=""550,0"">
              <ExpressionBindings>
                <Item1 Ref=""48"" EventName=""BeforePrint"" PropertyName=""Text"" Expression=""[ImporteTotal]"" />
              </ExpressionBindings>
              <TextFormatString>{0:c2}</TextFormatString>
            </Item3>
          </Controls>
        </Item1>
      </Bands>
    </Item6>
  </Bands>
  <Parameters>
    <Item1 Ref=""49"" Name=""Empresa_Nombre"" Description=""Nombre de la Empresa"" ValueInfo=""vdata.net"" />
    <Item2 Ref=""50"" Name=""Empresa_Nif"" Description=""NIF de la Empresa"" ValueInfo=""43725645T"" />
    <Item3 Ref=""51"" Name=""Empresa_Direccion"" Description=""Dirección de la Empresa"" ValueInfo=""C/. Vilamar, 2A"" />
  </Parameters>
</XtraReportsLayoutSerializer>";
        return System.Text.Encoding.UTF8.GetBytes(layout);
    }
}
