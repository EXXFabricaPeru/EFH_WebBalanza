using ControlHorasWS;
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
using Microsoft.Extensions.Configuration;

namespace EFHBlazzer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class CtrlTimeController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CtrlTimeController(
        IConfiguration configuration)
        {

            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpGet("tipoParadas")]
        public async Task<ActionResult<List<GenericEnt>>> GetTipoParadas()
        {
            GetTipoParadaResponse GetResponse = WSSoapClient().GetTipoParadaAsync(SecuredToken()).Result;
            string response = GetResponse.GetTipoParadaResult;

            var resp = SimpleJson.DeserializeObject<TipoParadasResult>("{\"TipoParadas\":" + response + "}");
            List<GenericEnt> lista = new List<GenericEnt>();

            foreach (var item in resp.TipoParadas)
            {
                lista.Add(new GenericEnt() { Code = item.Codigo.ToString(), Name = item.Nombre, boolAux = item.ParadaConsiderada });
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
        public ActionResult<CtHr_MultiList> GetEtapas(string idSerie, string DocNum)
        {
            GetOrdenCtrHorasResponse GetOrdenResponse = WSSoapClient().GetOrdenCtrHorasAsync(SecuredToken(), idSerie, DocNum, User.FindFirst("id").Value).Result;
            string response = GetOrdenResponse.GetOrdenCtrHorasResult;

            var resp = SimpleJson.DeserializeObject<OrdenBalanzaResult>("{\"Ordenes\":" + response + "}");
            CtHr_MultiList CtHr = new CtHr_MultiList();
            if (resp.Ordenes.Count() > 0)
            {
                CtHr.NroOrden = resp.Ordenes[0].Numero.ToString();
                CtHr.Cliente = resp.Ordenes[0].Cliente;
                CtHr.Impresion = resp.Ordenes[0].Impresion;
                CtHr.Medida = resp.Ordenes[0].Medida;
                CtHr.ItemCode = resp.Ordenes[0].CodigoArticulo;
                CtHr.ItemName = resp.Ordenes[0].NombreArticulo;
                List<GenericEnt> lista = new List<GenericEnt>();
                if (resp.Ordenes.Count() > 0)
                    foreach (var item in resp.Ordenes[0].Etapas)
                    {
                        lista.Add(new GenericEnt() { Code = item.IdEtapa.ToString(), Name = item.Nombre });
                    }
                CtHr.Etapas = lista;
            }
            return CtHr;
        }

        [AllowAnonymous]
        [HttpGet("{orden}/etapas")]
        public async Task<ActionResult<List<GenericEnt>>> GetEtapas(int orden)
        {
            List<GenericEnt> lista = new List<GenericEnt>();
            lista.Add(new GenericEnt() { Code = "2", Name = "IMPRESIÓN" });
            lista.Add(new GenericEnt() { Code = "4", Name = "SELLADO" });
            return lista;
        }


        [HttpPost("detalle")]
        public async Task<ActionResult<int>> Post(CtrHra_Detail detalle)
        {
            ControlHora CtrHor = new ControlHora
            {
                IdEmpleado = User.FindFirst("id").Value,
                OrdenFabricacion = int.Parse(detalle.NroOrden),
                Etapa = detalle.Etapa,
                Recurso = detalle.Maquina,
                Tipo = detalle.Tipo,
                Fecha = detalle.Fecha,
                Hora = detalle.Hora.ToString("HH:mm"),
            };
            AddCtrHoraResponse GetResponse = WSSoapClient().AddCtrHoraAsync(SecuredToken(), CtrHor).Result;
            var response = GetResponse.AddCtrHoraResult;
            if (string.IsNullOrEmpty(response.Code) || response.Code != "OK")
            {
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, response.ErrMsg);
            }
            int var2 = 0;
            return var2;
        }

        [HttpPost("pare")]
        public async Task<ActionResult<int>> Post(CtrHra_Parada detalle)
        {
            Parada CtrHor = new Parada
            {
                IdEmpleado = User.FindFirst("id").Value,
                OrdenFabricacion = int.Parse(detalle.NroOrden),
                Etapa = detalle.Etapa,
                Recurso = detalle.Maquina,
                Tipo = detalle.Tipo,
                TipoParaDesc = detalle.TipoDesc,
                Fecha = detalle.Fecha,
                Hora = detalle.Hora.ToString("HH:mm"),
                MotivoPara = detalle.MotivoPara == null ? "xx" : detalle.MotivoPara,
                TipoPara = detalle.TipoPara
            };
            AddPareResponse GetResponse = WSSoapClient().AddPareAsync(SecuredToken(), CtrHor).Result;
            var response = GetResponse.AddPareResult;
            if (string.IsNullOrEmpty(response.Code) || response.Code != "OK")
            {
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, response.ErrMsg);
            }
            int var2 = 0;
            return var2;
        }

        [HttpPost]
        public async Task<ActionResult<int>> Post(Bal_Head balanza)
        {
            //context.Add(genero);
            //await context.SaveChangesAsync();
            int var2 = 0;
            return var2;
        }

        [HttpPost("addEntrega")]
        public async Task<ActionResult<int>> PostEntrega(Bal_Det balAux)
        {
            AddEntregaResponse HoraResponse = WSSoapClient().AddEntregaAsync(SecuredToken(), balAux.codigoInterno, balAux.Etapa, balAux.RecursoMaquina).Result;
            var response = HoraResponse.AddEntregaResult;
            if (!string.IsNullOrEmpty(response.ErrMsg))
            {
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, response.ErrMsg);
            }
            int var2 = 99;
            return var2;
        }


        [AllowAnonymous]
        [HttpGet("{orden}/{etapa}/detalleEtapa")]
        public async Task<ActionResult<CtHr_MultiList>> GetEtapa(int orden, string etapa)
        {

            GetOrdenCtrHorasDocEntryResponse GetOrdenResponse = WSSoapClient().GetOrdenCtrHorasDocEntryAsync(SecuredToken(), orden.ToString(), User.FindFirst("id").Value).Result;
            string response = GetOrdenResponse.GetOrdenCtrHorasDocEntryResult;
            var resp = SimpleJson.DeserializeObject<OrdenBalanzaResult>("{\"Ordenes\":" + response + "}");
            var DetalleEtapa = resp.Ordenes[0].Etapas.Where(x => x.IdEtapa == int.Parse(etapa)).FirstOrDefault();
            CtHr_MultiList Result = new CtHr_MultiList();
            List<GenericEnt> maquinas = new List<GenericEnt>();

            foreach (var maq in DetalleEtapa.Recursos)
            {
                maquinas.Add(new GenericEnt() { Code = maq.Codigo, Name = maq.Nombre, LongDesc = maq.Nombre });
            }

            Result.Maquinas = maquinas;

            return Result;
        }

        [AllowAnonymous]
        [HttpGet("{orden}/{etapa}/{maquina}/detalleEtapa")]
        public async Task<ActionResult<CtHr_MultiList>> GetDetallesEtapa(int orden, string etapa,string maquina)
        {
            GetDetCtrlHoraResponse GetResponse = WSSoapClient().GetDetCtrlHoraAsync(SecuredToken(), orden, int.Parse(etapa), maquina, User.FindFirst("id").Value).Result;
            string response = GetResponse.GetDetCtrlHoraResult;
            var resp2 = SimpleJson.DeserializeObject<ControlHoraResult>(response);
            List<CtrHra_Detail> detalles = new List<CtrHra_Detail>();
            List<CtrHra_Parada> paradas = new List<CtrHra_Parada>();
            CtHr_MultiList Result = new CtHr_MultiList();

            var lastCtrHora = resp2.ControlHoras.LastOrDefault();
            if (lastCtrHora != null && lastCtrHora.Tipo.Equals("01"))
            {

                string[] hora = lastCtrHora.Hora.Split(":");
                detalles.Add(new CtrHra_Detail()
                {
                    Empleado = lastCtrHora.IdEmpleado,
                    NroOrden = lastCtrHora.OrdenFabricacion.ToString(),
                    Etapa = lastCtrHora.Etapa,
                    Maquina = lastCtrHora.Recurso,
                    Tipo = lastCtrHora.Tipo,
                    Fecha = lastCtrHora.Fecha,
                    //Hora = System.DateTime.ParseExact(lastCtrHora.Hora, "HH:mm",
                    //                   System.Globalization.CultureInfo.InvariantCulture),
                    Hora = lastCtrHora.Fecha + new TimeSpan(Int32.Parse(hora[0]), Int32.Parse(hora[1]), 0)
                }); ;

                foreach (var par in resp2.Paradas.ToList())
                {
                    paradas.Add(new CtrHra_Parada()
                    {
                        Empleado = par.IdEmpleado,
                        NroOrden = par.OrdenFabricacion.ToString(),
                        Etapa = par.Etapa,
                        Maquina = par.Recurso,
                        TipoPara = par.TipoPara,
                        MotivoPara = par.MotivoPara,
                        Tipo = par.Tipo,
                        TipoDesc = par.TipoParaDesc,
                        Fecha = par.Fecha,
                        Hora = System.DateTime.ParseExact(par.Hora, "HH:mm",
                                       System.Globalization.CultureInfo.InvariantCulture)
                    });
                }
            }
            //Result.Maquinas = maquinas;
            Result.Detalles = detalles;
            Result.Paradas = paradas;
            return Result;
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            //var existe = await context.Peliculas.AnyAsync(x => x.Id == id);
            //if (!existe) { return NotFound(); }
            //context.Remove(new Pelicula { Id = id });
            //await context.SaveChangesAsync();
            return NoContent();
        }

        [AllowAnonymous]
        [HttpGet("listadoActivos")]
        public async Task<ActionResult<List<CtrHra_Listado>>> GetListadoActivos()
        {

            GetCtrlHoraListResponse Series = WSSoapClient().GetCtrlHoraListAsync(SecuredToken(), User.FindFirst("id").Value).Result;
            string response = Series.GetCtrlHoraListResult;

            var resp = SimpleJson.DeserializeObject<ControlHoraList>("{\"Listado\":" + response + "}");
            List<CtrHra_Listado> lista = new List<CtrHra_Listado>();

            foreach (var reg in resp.Listado)
            {

                lista.Add(new CtrHra_Listado()
                {
                    NroOrden = reg.Codigo,
                    Etapa = reg.Etapa,
                    Fecha = reg.Fecha,
                    Hora = System.DateTime.ParseExact(String.Format("{0:0000}", reg.Hora).Insert(2, ":"), "HH:mm",
                                       System.Globalization.CultureInfo.InvariantCulture),
                    //Hora = reg.Hora,
                    Maquina = reg.Recurso,
                    Empleado = reg.IdEmpleado,
                    Estado = reg.Estado
                }) ;

            }

            return lista;
        }

        public SecuredTokenWebservice SecuredToken()
        {
            SecuredTokenWebservice secure = new SecuredTokenWebservice
            {
                AuthenticationToken = User.FindFirst("token").Value
            };
            return secure;
        }

        public ControlHoraWSSoapClient WSSoapClient()
        {
            return new ControlHoraWSSoapClient(ControlHorasWS.ControlHoraWSSoapClient.EndpointConfiguration.ControlHoraWSSoap, _configuration["path:controlHoraWS"]);
        }
    }
}
