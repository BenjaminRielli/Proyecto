namespace Library;

public class DisparosAguaYBarcosHandler : BasePrefijoHandler
{
    public GestorPartidas GestorPartidas { get; set; }

    public DisparosAguaYBArcosHandler(GestorPartidas gestorPartidas, BaseHandler? next)
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
        var aguas = partida.JugadorA.Tablero.ObtenerAguas();
        var tocados = partida.JugadorB.Tablero.ObtenerTocados();
        var aguas = partida.JugadorA.Tablero.ObtenerAguas();
        var tocados = partida.JugadorB.Tablero.ObtenerTocados();


        remitente =
            $">> Numero de aguas:   {aguas} \n" +
            $">> Numero de tocados:  {tocados} ";

        oponente = string.Empty;

    return true;
}
}
