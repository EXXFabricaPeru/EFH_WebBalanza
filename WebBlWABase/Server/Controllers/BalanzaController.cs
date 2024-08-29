using Microsoft.Extensions.Configuration;
using BalanzaWS;
using EFHBlazzer.Server.WSEntities;
using EFHBlazzer.Shared.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.ServiceModel.Channels;

namespace EFHBlazzer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class BalanzaController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public BalanzaController(
        IConfiguration configuration)
        {

            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpGet("tipoMat")]
        public async Task<ActionResult<List<GenericEnt>>> GetTipoMateriales()
        {
            GetMaterialesResponse AltUnitResponse = WSSoapClient().GetMaterialesAsync(SecuredToken()).Result;
            string response = AltUnitResponse.GetMaterialesResult;

            var resp = SimpleJson.DeserializeObject<MaterialesResult>("{\"Materiales\":" + response + "}");
            List<GenericEnt> lista = new List<GenericEnt>();

            foreach (var item in resp.Materiales)
            {
                lista.Add(new GenericEnt() { Code = item.Codigo, Name = item.Nombre });

            }
            return lista;
        }


        [AllowAnonymous]
        [HttpGet("altUnit")]
        public async Task<ActionResult<List<GenericEnt>>> GetAltUni()
        {
            GetUnidadesAlternativasResponse AltUnitResponse = WSSoapClient().GetUnidadesAlternativasAsync(SecuredToken()).Result;
            string response = AltUnitResponse.GetUnidadesAlternativasResult;

            var resp = SimpleJson.DeserializeObject<UnidadesAlternativasResult>("{\"UnidadesAlternativas\":" + response + "}");
            List<GenericEnt> lista = new List<GenericEnt>();

            foreach (var item in resp.UnidadesAlternativas)
            {
                lista.Add(new GenericEnt() { Code = item.Codigo, Name = item.Nombre });

            }
            return lista;
        }

        [AllowAnonymous]
        [HttpGet("series")]
        public async Task<ActionResult<List<GenericEnt>>> GetSeries()
        {

            GetSeriesResponse Series = WSSoapClient().GetSeriesAsync(SecuredToken()).Result;
            string response = Series.GetSeriesResult;

            var resp = SimpleJson.DeserializeObject<SeriesResult>("{\"Series\":" + response + "}");
            List<GenericEnt> lista = new List<GenericEnt>();

            foreach (var item in resp.Series)
            {
                lista.Add(new GenericEnt() { Code = item.Id.ToString(), Name = item.Nombre });
            }
            return lista;
        }

        [AllowAnonymous]
        [HttpGet("listetapas")]
        public async Task<ActionResult<List<GenericEnt>>> GetEtapas()
        {
            List<GenericEnt> lista = new List<GenericEnt>();
            try
            {

                GetEtapasResponse Etapas = WSSoapClient().GetEtapasAsync(SecuredToken()).Result;
                string response = Etapas.GetEtapasResult;

                var resp = SimpleJson.DeserializeObject<EtapasResult>("{\"Etapas\":" + response + "}");

                foreach (var item in resp.Etapas)
                {
                    lista.Add(new GenericEnt() { Code = item.Id.ToString(), Name = item.Nombre });

                }
            }catch(Exception ex)
            {
                
            }
            return lista;
        }

        [AllowAnonymous]
        [HttpGet("sincronizar")]
        public async Task<ActionResult<decimal>> Sincronizar()
        {
            //var forecasts = await Http.Get
            //File.Open("textfile.txt");
            return 1;
        }

        [AllowAnonymous]
        [HttpGet("{idSerie}/{DocNum}/etapas")]
        public ActionResult<Bal_MultiList> GetEtapas(string idSerie, string DocNum)
        {

            GetOrdenBalanzaResponse AltUnitResponse = WSSoapClient().GetOrdenBalanzaAsync(SecuredToken(), idSerie, DocNum, User.FindFirst("id").Value).Result;
            string response = AltUnitResponse.GetOrdenBalanzaResult;

            var resp = SimpleJson.DeserializeObject<OrdenBalanzaResult>("{\"Ordenes\":" + response + "}");
            Bal_MultiList BalRes = new Bal_MultiList();
            if (resp.Ordenes.Count() > 0)
            {
                BalRes.NroOrden = resp.Ordenes[0].Numero.ToString();
                BalRes.Cliente = resp.Ordenes[0].Cliente;
                BalRes.Impresion = resp.Ordenes[0].Impresion;
                BalRes.Medida = resp.Ordenes[0].Medida;
                BalRes.ItemCode = resp.Ordenes[0].CodigoArticulo;
                BalRes.ItemName = resp.Ordenes[0].NombreArticulo;
                List<GenericEnt> lista = new List<GenericEnt>();
                if (resp.Ordenes.Count() > 0)
                    foreach (var item in resp.Ordenes[0].Etapas)
                    {
                        lista.Add(new GenericEnt() { Code = item.IdEtapa.ToString(), Name = item.Nombre });
                    }
                BalRes.Etapas = lista;
            }
            else
            {
                BalRes.NroOrden = "-1";
            }
            return BalRes;
        }


        [HttpPost("addBalanza")]
        public async Task<ActionResult<int>> Post(Bal_Det balFront)
        {

            Balanza BalSend = new Balanza
            {
                IdEmpleado = User.FindFirst("id").Value,
                OrdenFabricacion = int.Parse(balFront.NroOrden),
                Etapa = balFront.Etapa,
                Maquina = string.IsNullOrEmpty(balFront.RecursoMaquina) ? "" : balFront.RecursoMaquina,
                CodigoArticulo = balFront.ItemCode,
                NombreArticulo = (balFront.ItemName.Length > 50) ? balFront.ItemName.Substring(0, 50) : balFront.ItemName,
                UnidadMedida = balFront.Unit,
                UnidadAlternativa = string.IsNullOrEmpty(balFront.AltUnit) ? "" : balFront.AltUnit,
                Cantidad = (double)balFront.Cantidad,
                Tara = (double)balFront.Tara,
                PesoBruto = (double)balFront.PesoBruto,
                PesoNeto = (double)balFront.PesoNeto,
                Numero = int.Parse(balFront.Numero),
                Fecha = balFront.Fecha,
                Hora = balFront.Hora.ToString("HH:mm"),
                Tipo = balFront.tipo,
                Lote = string.IsNullOrEmpty(balFront.lote) ? "" : balFront.lote,
                Material = string.IsNullOrEmpty(balFront.material) ? "" : balFront.material,
                DestinoMan = string.IsNullOrEmpty(balFront.destinoMan) ? "" : balFront.destinoMan,
                MedidaMan = string.IsNullOrEmpty(balFront.medidaMan) ? "" : balFront.medidaMan,
                EspesorMan = string.IsNullOrEmpty(balFront.espesorMan) ? "" : balFront.espesorMan
            };
            AddBalanzaResponse BalanzaResponse = WSSoapClient().AddBalanzaAsync(SecuredToken(), BalSend).Result;
            var response = BalanzaResponse.AddBalanzaResult;
            if (string.IsNullOrEmpty(response.Code) || response.Code != "OK")
            {
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, response.ErrMsg);
            }
            int var2 = 99;
            return var2;
        }

        [HttpPost("addSap")]
        public async Task<ActionResult<int>> PostSap(Bal_Det balAux)
        {
            AddSAPResponse BalanzaResponse = WSSoapClient().AddSAPAsync(SecuredToken(), balAux.codigoInterno).Result;
            var response = BalanzaResponse.AddSAPResult;
            if (!string.IsNullOrEmpty(response.ErrMsg))
            {
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, response.ErrMsg);
            }
            int var2 = 99;
            return var2;
        }

        [HttpPost("addParte")]
        public async Task<ActionResult<int>> PostParte(Bal_Det balAux)
        {
            AddParteEntregaResponse BalanzaResponse = WSSoapClient().AddParteEntregaAsync(SecuredToken(), balAux.codigoInterno).Result;
            var response = BalanzaResponse.AddParteEntregaResult;
            if (!string.IsNullOrEmpty(response.ErrMsg))
            {
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, response.ErrMsg);
            }
            int var2 = int.Parse(response.Code);
            return var2;
        }

        [HttpPost("addCalidad")]
        public async Task<ActionResult<int>> PostCalidad(Bal_Det balAux)
        {
            AddCalidadResponse BalanzaResponse = WSSoapClient().AddCalidadAsync(SecuredToken(), balAux.codigoInterno, User.FindFirst("id").Value).Result;
            var response = BalanzaResponse.AddCalidadResult;
            if (!string.IsNullOrEmpty(response.ErrMsg))
            {
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, response.ErrMsg);
            }
            int var2 = 99;
            return var2;
        }

        [AllowAnonymous]
        [HttpGet("{orden}/{etapa}/{tipo}/detalleEtapa")]
        public async Task<ActionResult<Bal_MultiList>> GetDetallesEtapa(int orden, string etapa, string tipo)
        {
            GetOrdenBalanzaDocentryResponse AltUnitResponse = WSSoapClient().GetOrdenBalanzaDocentryAsync(SecuredToken(), orden.ToString(), User.FindFirst("id").Value).Result;
            string response = AltUnitResponse.GetOrdenBalanzaDocentryResult;

            var resp = SimpleJson.DeserializeObject<OrdenBalanzaResult>("{\"Ordenes\":" + response + "}");
            int CantidadEtapas = resp.Ordenes[0].Etapas.Count();
            var DetalleEtapa = resp.Ordenes[0].Etapas.Where(x => x.IdEtapa == int.Parse(etapa)).FirstOrDefault();
            bool isLastEtapa = CantidadEtapas.Equals(int.Parse(etapa));
            Bal_MultiList Result = new Bal_MultiList();
            List<GenericEnt> maquinas = new List<GenericEnt>();

            foreach (var maq in DetalleEtapa.Recursos)
            {
                maquinas.Add(new GenericEnt() { Code = maq.Codigo, Name = maq.Codigo, LongDesc = maq.Nombre });
            }

            List<GenericEnt> articulos = new List<GenericEnt>();
            switch (tipo)
            {
                case "D":
                    foreach (var art in DetalleEtapa.Articulos.Where(x => x.CantidadPlanificada >= 0))
                    {
                        List<Genericlote> lotesList = new List<Genericlote>();
                        if (art.Lotes.Length > 0)
                        {
                            foreach (var lot in art.Lotes)
                            {
                                lotesList.Add(new Genericlote() { Code = lot.Codigo, Name = lot.Descripcion });
                            }
                        }

                        articulos.Add(new GenericEnt() { Code = art.Codigo, Name = art.Codigo, LongDesc = art.Descripcion, Unit = art.UnidadMedida, Lotes = lotesList.ToArray() });
                    }
                    break;
                case "S":
                case "R":
                    foreach (var art in DetalleEtapa.Articulos.Where(x => (/*isLastEtapa ||*/ x.CantidadPlanificada < 0) && ((tipo == "S" && x.TipoArticulo == "S") || (tipo == "R" && x.TipoArticulo == "R"))))
                    {
                        articulos.Add(new GenericEnt() { Code = art.Codigo, Name = art.Codigo, LongDesc = art.Descripcion, Unit = art.UnidadMedida });
                    }
                    break;
                default:
                    foreach (var art in DetalleEtapa.Articulos.Where(x => (/*isLastEtapa ||*/ x.CantidadPlanificada < 0) && ((x.TipoArticulo == "-" && (tipo != "R" || tipo != "S")))))
                    {
                        articulos.Add(new GenericEnt() { Code = art.Codigo, Name = art.Codigo, LongDesc = art.Descripcion, Unit = (tipo.Equals("F") ? "MLL" : art.UnidadMedida) });
                    }
                    break;
            }


            List<Bal_Det> detalles = new List<Bal_Det>();
            foreach (var bal in DetalleEtapa.Balanzas)
            {
                if (bal.Tipo.Equals(tipo))
                    detalles.Add(new Bal_Det()
                    {
                        Numero = bal.Numero.ToString(),
                        codigoInterno = bal.Codigo,
                        ItemCode = bal.CodigoArticulo,
                        ItemName = bal.NombreArticulo,
                        AltUnit = bal.UnidadAlternativa,
                        Unit = bal.UnidadMedida,
                        Cantidad = (decimal)bal.Cantidad,
                        Tara = (decimal)bal.Tara,
                        PesoBruto = (decimal)bal.PesoBruto,
                        PesoNeto = (decimal)bal.PesoNeto,
                        Fecha = bal.Fecha,
                        Hora = DateTime.Today.Add(TimeSpan.Parse(bal.Hora)),
                        tipo = bal.Tipo,
                        reciboProduccion = bal.ReciboProduccion.ToString(),
                        parteEntrega = bal.ParteEnt,
                        status = bal.Status,
                        aproCalidad=bal.AproCalidad
                    });
            }

            Result.Articulos = articulos;
            Result.Maquinas = maquinas;
            Result.Detalles = detalles.OrderBy(x => int.Parse(x.Numero)).ToList();

            return Result;
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            DeleteBalanzaResponse BalanzaResponse = WSSoapClient().DeleteBalanzaAsync(SecuredToken(), id.ToString()).Result;
            var response = BalanzaResponse.DeleteBalanzaResult;
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("remove/{id}")]
        public async Task<bool> Remove(int id)
        {

            DeleteBalanzaResponse BalanzaResponse = WSSoapClient().DeleteBalanzaAsync(SecuredToken(), id.ToString()).Result;
            var response = BalanzaResponse.DeleteBalanzaResult;
            return true;
        }

        [AllowAnonymous]
        [HttpGet("observar/{id}")]
        public async Task<bool> Observar(int id)
        {

            ObservarBalanzaResponse BalanzaResponse = WSSoapClient().ObservarBalanzaAsync(SecuredToken(), id.ToString()).Result;
            var response = BalanzaResponse.ObservarBalanzaResult;
            return true;
        }

        [HttpGet("generaEtiquetaPDF/{id}/{tipo}")]
        public async Task<IActionResult> GetEtiquetaDetalle(string id, string tipo)
        {
            return this.File(GetEtiqueta(id, "D" + tipo), "application/pdf");
        }

        [HttpGet("generaBarcode/{id}")]
        public async Task<IActionResult> GetEtiquetaBarcode(string id)
        {
            return this.File(GetEtiqueta(id, "BC"), "application/pdf");
        }

        [HttpGet("generaQR/{id}")]
        public async Task<IActionResult> GetEtiquetaQR(string id)
        {
            return this.File(GetEtiqueta(id, "QR"), "application/pdf");
        }

        [HttpGet("generaParteEntrega/{id}")]
        public async Task<IActionResult> generaParteEntrega(string id)
        {
            return this.File(GetEtiqueta(id, "PE"), "application/pdf");
        }

        private byte[] GetEtiqueta(string id, string tipo)
        {

            GetEtiquetaCRResponse EtiquetResponse = null;
            EtiquetResponse = WSSoapClient().GetEtiquetaCRAsync(SecuredToken(), id, tipo).Result;
            string response = EtiquetResponse.GetEtiquetaCRResult;
            var resp = SimpleJson.DeserializeObject<EtiquetaCR>(response);
            return (Convert.FromBase64String(resp.Binario));
        }


        public SecuredTokenWebservice SecuredToken()
        {
            SecuredTokenWebservice secure = new SecuredTokenWebservice
            {
                AuthenticationToken = User.FindFirst("token").Value
            };
            return secure;
        }

        public BalanzaWSSoapClient WSSoapClient()
        {
            return new BalanzaWSSoapClient(BalanzaWS.BalanzaWSSoapClient.EndpointConfiguration.BalanzaWSSoap, _configuration["path:balanzaWS"]);
        }

        #region scrapped
        //public async Task<IActionResult> GetPDF(string id, string tipo)
        //{
        //    //var memoryStream = new MemoryStream();


        //    //// Marge in centimeter, then I convert with .ToDpi()
        //    //float margeLeft = 0.7f;
        //    //float margeRight = 0.7f;
        //    //float margeTop = 1.0f;
        //    //float margeBottom = 1.0f;

        //    ////Carga de BD
        //    //GetPDFResponse PDFResponse = WSSoapClient().GetPDFAsync(SecuredToken(), id).Result;
        //    //string response = PDFResponse.GetPDFResult;

        //    //var resp = SimpleJson.DeserializeObject<PDFResult>(response);

        //    ////Creacion documento
        //    //Document pdf = new Document(
        //    //                        PageSize.A4,
        //    //                        margeLeft.ToDpi(),
        //    //                        margeRight.ToDpi(),
        //    //                        margeTop.ToDpi(),
        //    //                        margeBottom.ToDpi()
        //    //                       );
        //    //pdf.AddTitle("ImpresionBalanza");
        //    //pdf.AddAuthor("Packplast Envolturas SAC");
        //    //pdf.AddCreationDate();
        //    //pdf.AddKeywords("Balanza");
        //    //pdf.AddSubject("Comprobante balanza PackPlast");
        //    //PdfWriter writer = PdfWriter.GetInstance(pdf, memoryStream);
        //    //pdf.Open();
        //    //ComprobantePDF(pdf, writer, resp);
        //    //pdf.Close();
        //    GetEtiquetaCRResponse EtiquetResponse = null;
        //    try
        //    {
        //        EtiquetResponse = WSSoapClient().GetEtiquetaCRAsync(SecuredToken(), id, "D" + tipo).Result;

        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //    var ds = EtiquetResponse;
        //    string response = EtiquetResponse.GetEtiquetaCRResult;
        //    var resp = SimpleJson.DeserializeObject<EtiquetaCR>(response);
        //    var valor = resp.Binario;
        //    var bytss = Convert.FromBase64String(valor);
        //    return this.File(Convert.FromBase64String(valor), "application/pdf");
        //}

        //public static void ComprobantePDF(Document pdf, PdfWriter writer, PDFResult body)
        //{

        //    // TABLES 
        //    var cell = new Cell();

        //    Table datatable = new Table(12)
        //    {
        //        Padding = 2,
        //        Spacing = 0
        //    };

        //    float[] headerwidths = { 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20 };

        //    datatable.Widths = headerwidths;
        //    datatable.DefaultHorizontalAlignment = Element.ALIGN_LEFT;
        //    datatable.DefaultCellBorder = Rectangle.NO_BORDER;
        //    datatable.Border = Rectangle.NO_BORDER;

        //    var title = new Paragraph("Packplast Envolturas S.A.C.", new Font(Font.HELVETICA, 20, Font.BOLD));
        //    cell = new Cell(title) { Colspan = 12 };
        //    datatable.AddCell(cell);

        //    //FP-Hora-FV-N°
        //    datatable.AddCell("F.P.:");
        //    datatable.AddCell(new Cell(new Phrase(body.FechaProduccion.ToString("dd/MM/yyyy"))) { Colspan = 2 });
        //    datatable.AddCell("Hora:");
        //    datatable.AddCell(new Cell(new Phrase(string.IsNullOrEmpty(body.Hora) ? "" : body.Hora)) { Colspan = 2 });
        //    datatable.AddCell("F.V.:");
        //    datatable.AddCell(new Cell(new Phrase(string.IsNullOrEmpty(body.FechaVencimiento.ToString()) ? "" : body.FechaVencimiento.ToString("dd/MM/yyyy"))) { Colspan = 2 });
        //    switch (body.Tipo)
        //    {
        //        case "B":
        //            datatable.AddCell("BOB:");
        //            break;
        //        case "F":
        //            datatable.AddCell("FAR:");
        //            break;
        //    }
        //    datatable.AddCell(new Cell(new Phrase(string.IsNullOrEmpty(body.CodigoInterno) ? "" : body.CodigoInterno)) { Colspan = 2 });

        //    //Pedido - OT- OC
        //    datatable.AddCell(new Cell(new Phrase("Pedido:")) { Colspan = 2 });
        //    datatable.AddCell(new Cell(new Phrase(string.IsNullOrEmpty(body.Pedido) ? "" : body.Pedido)) { Colspan = 2 });
        //    datatable.AddCell("O/T:");
        //    datatable.AddCell(new Cell(new Phrase(string.IsNullOrEmpty(body.OT) ? "" : body.OT)) { Colspan = 2 });
        //    datatable.AddCell("O/C:");
        //    datatable.AddCell(new Cell(new Phrase(string.IsNullOrEmpty(body.OC) ? "" : body.OC)) { Colspan = 3 });
        //    datatable.AddCell(new Cell(new Phrase("")) { Colspan = 1 });

        //    //Cliente
        //    datatable.AddCell(new Cell(new Phrase("Cliente:")) { Colspan = 2 });
        //    datatable.AddCell(new Cell(new Phrase(string.IsNullOrEmpty(body.Cliente) ? "SIN CLIENTE" : body.Cliente)) { Colspan = 10 });

        //    //Producto
        //    datatable.AddCell(new Cell(new Phrase("Producto:")) { Colspan = 2 });
        //    datatable.AddCell(new Cell(new Phrase(string.IsNullOrEmpty(body.Producto) ? "" : body.Producto)) { Colspan = 10 });

        //    //Impresion
        //    datatable.AddCell(new Cell(new Phrase("Impresion:")) { Colspan = 2 });
        //    datatable.AddCell(new Cell(new Phrase(string.IsNullOrEmpty(body.Impresión) ? "SIN IMPRESION" : body.Impresión)) { Colspan = 10 });

        //    //Medida - Destino
        //    datatable.AddCell(new Cell(new Phrase("Medida Final:")) { Colspan = 3 });
        //    datatable.AddCell(new Cell(new Phrase(string.IsNullOrEmpty(body.MedidaFinal) ? "SIN MEDIDA" : body.MedidaFinal)) { Colspan = 5 });

        //    if (string.IsNullOrEmpty(body.Destino))
        //    {
        //        datatable.AddCell(new Cell(new Phrase("")) { Colspan = 4 });
        //    }
        //    else
        //    {
        //        datatable.AddCell("Dest: ");
        //        datatable.AddCell(new Cell(new Phrase(body.Destino)) { Colspan = 3 });
        //    }

        //    //P.bRUTO -TARA-UND
        //    datatable.AddCell(new Cell(new Phrase("P. Bruto:")) { Colspan = 2 });
        //    datatable.AddCell(new Cell(new Phrase(String.Format("{0:0.00}", body.PesoBruto))) { Colspan = 2 });
        //    datatable.AddCell(new Cell(new Phrase("Tara: ")) { Colspan = 2 });
        //    datatable.AddCell(new Cell(new Phrase(String.Format("{0:0.00}", body.Tara))) { Colspan = 2 });

        //    switch (body.Tipo)
        //    {
        //        case "B":
        //            datatable.AddCell(new Cell(new Phrase("UND:")) { Colspan = 2 });
        //            datatable.AddCell(new Cell(new Phrase(body.Unidades)) { Colspan = 2 });
        //            break;
        //        case "F":
        //            datatable.AddCell(new Cell(new Phrase("P.Neto::")) { Colspan = 2 });
        //            datatable.AddCell(new Cell(new Phrase(body.PesoNeto)) { Colspan = 2 });
        //            break;
        //    }

        //    //Operarop -COD PRD
        //    datatable.AddCell(new Cell(new Phrase("Operario")) { Colspan = 2 });
        //    datatable.AddCell(new Cell(new Phrase(body.Operario)) { Colspan = 4 });
        //    datatable.AddCell(new Cell(new Phrase("Cod.PRD: ")) { Colspan = 2 });
        //    datatable.AddCell(new Cell(new Phrase(body.CodProd)) { Colspan = 4 });

        //    //Maquina
        //    datatable.AddCell(new Cell(new Phrase("Maquina:")) { Colspan = 2 });
        //    datatable.AddCell(new Cell(new Phrase(body.Maquina)) { Colspan = 10 });

        //    var subfooter = new Paragraph();
        //    switch (body.Tipo)
        //    {
        //        case "B":
        //            subfooter = new Paragraph(string.Format("Peso Neto: {0} {1}", body.PesoNeto, "KG"), new Font(Font.HELVETICA, 14, Font.BOLD));
        //            break;
        //        case "F":
        //            subfooter = new Paragraph(string.Format("Cantidad:{0} {1}", body.Cantidad, "MLL"), new Font(Font.HELVETICA, 14, Font.BOLD));
        //            break;
        //    }
        //    cell = new Cell(subfooter) { Colspan = 12 };
        //    cell.Border = Rectangle.BOTTOM_BORDER | Rectangle.TOP_BORDER;
        //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //    datatable.AddCell(cell);

        //    var foot = new Paragraph("PRODUCTO CONFORME", new Font(Font.HELVETICA, 15, Font.BOLD));
        //    cell = new Cell(foot) { Colspan = 12 };
        //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //    datatable.AddCell(cell);

        //    pdf.Add(datatable);
        //}

        #endregion
    }
}
