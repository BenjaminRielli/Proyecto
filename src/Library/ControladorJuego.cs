namespace Library;

// Esta clase cumple con SRP y con expert, es la clase experta en
// todo lo que ocurre en el juego y su unica responsabilidad es manejar
// el estado del mismo.
// Al mismo tiempo aplica Law of Demeter al proveer funciones para acceder
// a toda la funcionalidad necesaria de las clases internas.

/// <summary>
/// Controlador del juego, implementa el flujo de juego para
/// la batalla naval.
/// </summary>
public class ControladorJuego
{
    /// <summary>
    /// Estado actual del flujo del juego.
    /// </summary>
    public EstadoPartida Estado { get; private set; }

    /// <summary>
    /// Primer jugador.
    /// </summary>
    public Jugador JugadorA { get; }

    /// <summary>
    /// Segundo jugador.
    /// </summary>
    public Jugador JugadorB { get; }

    /// <summary>
    /// Lista con las longitudes de los barcos que deben ser
    /// agregados por cada usuario
    /// </summary>
    /// <remarks>
    /// Pueden haber repetidos, e.j: 2, 2, 3 ; dos barcos de 2 y uno de 3
    /// </remarks>
    public List<int> BarcosEsperados { get; } = new() { 2, 3, 4, 5 };

    public bool SigueEnJuego
    {
        get
        {
            return JugadorA.SigueEnJuego && JugadorB.SigueEnJuego;
        }
    }

    /// <summary>
    /// Construye un controlador de juego.
    /// </summary>
    /// <remarks>
    /// Ambos jugadores deben ser compatibles (Jugador.SonCompatibels())
    /// </remarks>
    /// <param name="jugadorA">Instancia del primer jugador</param>
    /// <param name="jugadorB">Instancia del segundo jugador</param>
    public ControladorJuego(Jugador jugadorA, Jugador jugadorB)
    {
        if (!Jugador.SonCompatibles(jugadorA, jugadorB))
        {
            throw new JugadoresIncompatibles(jugadorA, jugadorB);
        }

        Estado = EstadoPartida.Configuración;
        JugadorA = jugadorA;
        JugadorB = jugadorB;
    }

    /// <summary>
    /// La instancia del jugador a quien le corresponde jugar
    /// en el turno actual. Puede ser null si no es el turno de
    /// ningún jugador.
    /// </summary>
    public Jugador? JugadorActual
    {
        get
        {
            switch (Estado)
            {
                case EstadoPartida.TurnoJugadorA:
                    return JugadorA;
                case EstadoPartida.TurnoJugadorB:
                    return JugadorB;
                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// La instancia del jugador a quien le corresponde ser
    /// "atacado" en el turno actual. Puede ser null si no es el turno de
    /// ningún jugador.
    /// </summary>
    public Jugador? OponenteActual
    {
        get
        {
            switch (Estado)
            {
                case EstadoPartida.TurnoJugadorA:
                    return JugadorB;
                case EstadoPartida.TurnoJugadorB:
                    return JugadorA;
                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// Instancia del jugador que ganó la partida. Si no hay ningún
    /// ganador aún, retorna null.
    /// </summary>
    public Jugador? Ganador
    {
        get
        {
            switch (Estado)
            {
                case EstadoPartida.Terminado:
                case EstadoPartida.TerminadoPorReloj:
                    if (JugadorA.SigueEnJuego)
                        return JugadorA;
                    return JugadorB;
                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// Instancia del jugador que perdió la partida. Si no hay ningún
    /// perdedor aún, retorna null.
    /// </summary>
    public Jugador? Perdedor
    {
        get
        {
            switch (Estado)
            {
                case EstadoPartida.Terminado:
                case EstadoPartida.TerminadoPorReloj:
                    if (JugadorA.SigueEnJuego)
                        return JugadorB;
                    return JugadorA;
                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// Instancia de un jugador según su Id.
    /// </summary>
    /// <param name="id">Id del jugador a obtener</param>
    /// <returns>Instancia del jugador correspondiente, o null si no existe</returns>
    public Jugador? ObtenerJugadorPorId(Ident id)
    {
        if (JugadorA.Id == id)
        {
            return JugadorA;
        }

        if (JugadorB.Id == id)
        {
            return JugadorB;
        }

        return null;
    }

    /// <summary>
    /// Obtiene el oponente de un jugador
    /// </summary>
    /// <param name="idJugador">Id del jugador para el cual devolver su oponente</param>
    /// <returns>Instancia de un Jugador, o null</returns>
    public Jugador? OponenteDe(Ident idJugador)
    {
        if (JugadorA.Id == idJugador)
        {
            return JugadorB;
        }
        else if (JugadorB.Id == idJugador)
        {
            return JugadorA;
        }

        return null;
    }

    /// <summary>
    /// Lista con los "largos" de barcos que aún faltan por
    /// configurar en el tablero.
    /// </summary>
    public List<int> BarcosFaltantes(Ident idJugador)
    {
        var jugador = ObtenerJugadorPorId(idJugador);

        if (jugador == null)
        {
            throw new JugadorIncorrecto(idJugador);
        }

        var opciones = new List<int>(BarcosEsperados);

        foreach (var barco in jugador.Barcos)
        {
            opciones.Remove(barco.Largo);
        }

        return opciones;
    }

    /// <summary>
    /// Verifica si es el turno de un jugador dado
    /// </summary>
    /// <param name="idJugador">Id del jugador a verificar</param>
    /// <returns>True si es el turno de idJugador, false si no lo es</returns>
    public bool EsTurnoDe(Ident idJugador)
    {
        return JugadorActual != null && JugadorActual.Id == idJugador;
    }

    /// <summary>
    /// Crea un nuevo barco
    /// </summary>
    /// <param name="id">Identificador del jugador que desea crear un barco</param>
    /// <param name="coordA">Primera coordenada del barco</param>
    /// <param name="coordB">Segunda coordenada del barco</param>
    public void AgregarBarco(Ident id, Coord coordA, Coord coordB)
    {
        if (Estado != EstadoPartida.Configuración)
        {
            throw new EstadoPartidaIncorrecto(
                esperado: EstadoPartida.Configuración,
                encontrado: Estado
            );
        }

        var jugador = ObtenerJugadorPorId(id);
        if (jugador != null)
        {
            if (!Coord.Alineadas(coordA, coordB))
            {
                throw new CoordenadasNoAlineadas(coordA, coordB);
            }

            var coords = Coord.Ordenar(coordA, coordB);

            var largo = Coord.Largo(coords.Item1, coords.Item2);
            if (!BarcosEsperados.Exists(x => x == largo))
            {
                throw new BarcoLargoIncorrecto(coords.Item1, coords.Item2);
            }

            if (!BarcosFaltantes(jugador.Id).Exists(x => x == largo))
            {
                throw new BarcoYaExiste(largo);
            }

            jugador.AgregarBarco(coords.Item1, coords.Item2);
        }

        if (BarcosFaltantes(JugadorA.Id).Count == 0
            && BarcosFaltantes(JugadorB.Id).Count == 0)
        {
            SiguienteTurno();
        }
    }

    private void SiguienteTurno()
    {
        switch (Estado)
        {
            case EstadoPartida.TurnoJugadorA:
                Estado = EstadoPartida.TurnoJugadorB;
                JugadorB.IniciarTurno();
                break;
            case EstadoPartida.TurnoJugadorB:
            default:
                Estado = EstadoPartida.TurnoJugadorA;
                JugadorA.IniciarTurno();
                break;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="jugada"></param>
    public ResultadoJugada HacerJugada(Jugada jugada)
    {
        var jugador = JugadorActual;
        var oponente = OponenteActual;

        if (jugador == null || oponente == null)
        {
            throw new EstadoPartidaIncorrecto(
                esperados: new EstadoPartida[] {
                    EstadoPartida.TurnoJugadorA,
                    EstadoPartida.TurnoJugadorB,
                },
                encontrado: Estado
            );
        }

        if (jugada.Id != jugador.Id)
        {
            throw new JugadorIncorrecto(jugada.Id);
        }

        switch (jugada.Tipo)
        {
            case TipoJugada.Ataque:
                var resultadoAtaque = jugador.AtacarJugador(oponente, jugada.Coordenada);

                jugador.TerminarTurno();

                if (!jugador.SigueEnJuegoReloj)
                {
                    Estado = EstadoPartida.TerminadoPorReloj;
                    return ResultadoJugada.TerminadoPorReloj;
                }

                if (!oponente.SigueEnJuego)
                {
                    Estado = EstadoPartida.Terminado;
                }
                else
                {
                    SiguienteTurno();
                }

                switch (resultadoAtaque)
                {
                    case ResultadoAtaque.Agua:
                        return ResultadoJugada.Agua;
                    case ResultadoAtaque.Hundido:
                        return ResultadoJugada.Hundido;
                    case ResultadoAtaque.Tocado:
                        return ResultadoJugada.Tocado;
                    default:
                        throw new InvalidOperationException();
                }
            case TipoJugada.Radar:
                jugador.LanzarRadar(oponente, jugada.Coordenada);

                jugador.TerminarTurno();

                if (!jugador.SigueEnJuegoReloj)
                {
                    Estado = EstadoPartida.TerminadoPorReloj;
                    return ResultadoJugada.TerminadoPorReloj;
                }

                SiguienteTurno();

                return ResultadoJugada.RadarDesplegado;
            default:
                throw new InvalidOperationException();
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="idJugador"></param>
    /// <returns></returns>
    public string MostrarTablero(Ident idJugador)
    {
        var jugador = ObtenerJugadorPorId(idJugador);

        if (jugador == null)
        {
            throw new JugadorIncorrecto(idJugador);
        }

        return jugador.Tablero.ImprimirBarcos();
    }
    

    /// <summary>
    ///
    /// </summary>
    /// <param name="idJugador"></param>
    /// <returns></returns>
    public string MostrarTableroDelOponente(Ident idJugador)
    {
        var jugador = OponenteDe(idJugador);

        if (jugador == null)
        {
            throw new JugadorIncorrecto(idJugador);
        }

        return jugador.Tablero.ImprimirBarcos();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="idJugador"></param>
    /// <returns></returns>
    public string MostrarJugadas(Ident idJugador)
    {
        if (idJugador == JugadorA.Id)
        {
            return JugadorB.Tablero.ImprimirJugadas();
        }
        else if (idJugador == JugadorB.Id)
        {
            return JugadorA.Tablero.ImprimirJugadas();
        }
        else
        {
            throw new JugadorIncorrecto(idJugador);
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="idJugador"></param>
    /// <returns></returns>
    public string MostrarJugadasDelOponente(Ident idJugador)
    {
        if (idJugador == JugadorA.Id)
        {
            return JugadorA.Tablero.ImprimirJugadas();
        }
        else if (idJugador == JugadorB.Id)
        {
            return JugadorB.Tablero.ImprimirJugadas();
        }
        else
        {
            throw new JugadorIncorrecto(idJugador);
        }
    }
}
