namespace Library;

public class DisparosAguaYBarcosHandler : BasePrefijoHandler
{
    public GestorPartidas GestorPartidas { get; set; }

    public DisparosAguaYBarcosHandler(GestorPartidas gestorPartidas, BaseHandler? next)
        : base(next)
    {
        this.GestorPartidas = gestorPartidas;
        this.Keywords = new string[] {
                "DisparosAgua",
                "cantidad agua",
                "agua",
                "disparos agua",
                "vecesagua",
            };
    }

    protected override bool InternalHandle(Message message, out string remitente, out string oponente)
    {
        oponente = string.Empty;
        if (!CanHandle(message))
        {
            remitente = string.Empty;
            return false;
        }

        var partida = GestorPartidas.ObtenerPartida(message.IdJugador);

        if (partida == null)
        {
            remitente = "No hay ninguna partida activa";
            return true;
        }

        var aguasJugadorA = partida.JugadorA.Tablero.ObtenerAguas();
        var tocadosJugadorA = partida.JugadorA.Tablero.ObtenerTocados();
        var aguasJugadorB = partida.JugadorB.Tablero.ObtenerAguas();
        var tocadosJugadorB = partida.JugadorB.Tablero.ObtenerTocados();


        remitente =
            $">> Numero de aguas jugador A:   {aguasJugadorA} \n" +
            $">> Numero de tocados jugador A :  {tocadosJugadorA} \n" + 
            $">> Numero de aguas jugador B:   {aguasJugadorB} \n" +
            $">> Numero de tocados jugador A :  {tocadosJugadorB} ";

        oponente = string.Empty;

        return true;
    }
}
