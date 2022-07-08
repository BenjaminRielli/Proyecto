namespace Library;

// Jugador cumple SRP, solamente da informacion del jugador y sus operaciones.
// Aplica Law of Demeter ya que las operaciones de sus objetos internos
// se acceden por medio de jugador

/// <summary>
/// Información y estado de un jugador.
/// </summary>
public class Jugador
{
    /// <summary>
    /// Identificador único
    /// </summary>
    public Ident Id { get; }

    /// <summary>
    /// Nombre del jugador, puede estar vacío
    /// </summary>
    public string Nombre { get; set; } = String.Empty;

    /// <summary>
    /// Tablero de juego
    /// </summary>
    public Tablero Tablero { get; }

    /// <summary>
    /// El reloj de juego del jugador, puede no tener uno (null)
    /// </summary>
    public Reloj? Reloj { get; }

    /// <summary>
    /// Cantidad de Radares que aún puede desplegar.
    /// </summary>
    public int RadaresDisponibles { get; private set; }

    /// <summary>
    /// Estadísticas globales del jugador.
    /// </summary>
    public Estadistica Estadistica { get; }

    /// <summary>
    /// True si el Jugador aún no ha perdido, false en caso contrario
    /// </summary>
    public bool SigueEnJuego
    {
        get
        {
            return SigueEnJuegoReloj && Tablero.BarcosAFlote().Count != 0;
        }
    }

    /// <summary>
    /// True si el Jugador aún tiene tiempo disponible en el reloj,
    /// false en caso contrario
    /// </summary>
    public bool SigueEnJuegoReloj
    {
        get
        {
            return Reloj != null ? Reloj.Activo : true;
        }
    }

    /// <summary>
    /// Lista con las instancias de los Barcos del jugador
    /// </summary>
    public IList<Barco> Barcos
    {
        get
        {
            return Tablero.Barcos;
        }
    }

    /// <summary>
    /// Construye un jugador
    /// </summary>
    /// <param name="id">Identificador único para el jugador</param>
    /// <param name="tablero">Instancia de tablero del jugador</param>
    /// <param name="reloj">Reloj del jugador, puede ser nulo</param>
    /// <param name="estadistica">Estadísticas del jugador</param>
    public Jugador(Ident id,
                   string nombre,
                   Tablero tablero,
                   Reloj? reloj,
                   int radaresDisponibles,
                   Estadistica estadistica)
    {
        Id = id;
        Nombre = nombre;
        Tablero = tablero;
        Reloj = reloj;
        RadaresDisponibles = radaresDisponibles;
        Estadistica = estadistica;
    }

    /// <summary>
    /// Agrega un barco al tablero
    /// </summary>
    /// <param name="coordA">Primera coordenada del barco</param>
    /// <param name="coordB">Segunda coordenada del barco</param>
    /// <returns>La instancia del barco que fue creado</returns>
    public Barco AgregarBarco(Coord coordA, Coord coordB)
    {
        return Tablero.AgregarBarco(coordA, coordB);
    }

    /// <summary>
    /// Éste método debe ser llamado al iniciar el turno de éste jugador,
    /// se encarga de controlar el reloj de juego.
    /// </summary>
    public void IniciarTurno()
    {
        if (Reloj != null)
        {
            Reloj.Iniciar();
        }
    }

    /// <summary>
    /// Éste método debe ser llamado al finalizar el turno de éste jugador,
    /// se encarga de controlar el reloj de juego.
    /// </summary>
    public void TerminarTurno()
    {
        if (Reloj != null)
        {
            Reloj.Terminar();
        }
    }

    /// <summary>
    /// Realiza un ataque sobre otro jugador, y actualiza las estadísticas.
    /// </summary>
    /// <param name="oponente">Jugador al cual enviar el ataque</param>
    /// <param name="coord">Coordenada a atacar</param>
    /// <returns>El resultado del ataque</returns>
    public ResultadoAtaque AtacarJugador(Jugador oponente, Coord coord)
    {
        var oponenteEstabaEnJuego = oponente.SigueEnJuego;

        var resultadoAtaque = oponente.RecibirAtaque(coord);

        if (SigueEnJuego && oponenteEstabaEnJuego && !oponente.SigueEnJuego)
        {
            Estadistica.Victorias++;
        }

        switch (resultadoAtaque)
        {
            case ResultadoAtaque.Agua:
                Estadistica.Fallos++;
                Tablero.NumeroAgua++;
                break;
            case ResultadoAtaque.Tocado:
                Estadistica.Aciertos++;
                Tablero.NumeroTocados++;
                break;
            case ResultadoAtaque.Hundido:
                Estadistica.Hundidos++;
                break;
            default:
                break;
        }

        return resultadoAtaque;
    }


    /// <summary>
    /// Lanza el radar sobre un oponente
    /// </summary>
    /// <param name="oponente">Jugador al cual lanzarle el radar</param>
    /// <param name="coord">Coordenada sobre la cual desplegar el radar</param>
    public void LanzarRadar(Jugador oponente, Coord coord)
    {
        if (RadaresDisponibles > 0)
        {
            oponente.RecibirRadar(coord);
            RadaresDisponibles--;

            Estadistica.Radares += 1;
        }
        else
        {
            throw new RadaresAgotados();
        }
    }

    /// <summary>
    /// Recibir el ataque de otro jugador
    /// </summary>
    /// <param name="coord">Coordenada atacada</param>
    /// <returns>El resultado del ataque</returns>
    public ResultadoAtaque RecibirAtaque(Coord coord)
    {
        var estabaEnJuego = SigueEnJuego;

        var resultado = Tablero.Atacar(coord);

        if (estabaEnJuego && !SigueEnJuego)
        {
            Estadistica.Derrotas++;
        }

        return resultado;
    }

    /// <summary>
    /// Recibir el radar de otro jugador
    /// </summary>
    /// <param name="centro">Coordenada del centro de acción del radar</param>
    public void RecibirRadar(Coord centro)
    {
        Tablero.Radar(centro);
    }

    /// <summary>
    /// Verifica si dos jugadores
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool SonCompatibles(Jugador a, Jugador b)
    {
        return a.Tablero.Ancho == b.Tablero.Ancho
            && a.Tablero.Alto == b.Tablero.Alto;
    }
}
