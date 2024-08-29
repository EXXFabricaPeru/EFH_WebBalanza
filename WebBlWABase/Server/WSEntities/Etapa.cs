using BalanzaWS;
using System;

namespace EFHBlazzer.Server.WSEntities
{
    public class Etapa
    {
        public int IdEtapa { get; set; }
        public int NumSecuencia { get; set; }
        public string Nombre { get; set; }
        public string Estado { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public float DiasRequeridos { get; set; }
        public float DiasEspera { get; set; }
        public Articulo[] Articulos { get; set; }
        public Recurso[] Recursos { get; set; }
        public Balanza[] Balanzas { get; set; }
    }
}

