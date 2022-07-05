namespace Library;

public class Robotina
{
    public class Comando
    {
        public enum Tipo
        {
            /// <summary></summary>
            Esperar,

            /// <summary></summary>
            Eliminar,

            /// <summary></summary>
            Agregar,

            /// <summary></summary>
            Atacar,

            /// <summary></summary>
            Radar,
        }

        public List<Coord> Coordenadas { get; set; } = new();

        public Tipo Accion { get; set; }
    }

    public ControladorJuego Partida { get; }

    public Tablero TableroOponente { get; }

    public Ident IdBot { get; }

    public string Nombre
    {
        get
        {
            var jugador = Partida.ObtenerJugadorPorId(IdBot);

            if (jugador != null)
            {
                return jugador.Nombre;
            }

            return String.Empty;
        }
    }

    private Random _rng;

    private Coord? _tocadoInicial;

    private Coord? _tocadoActual;

    public Robotina(Ident idBot, ControladorJuego partida)
    {
        Partida = partida;
        IdBot = idBot;
        _rng = new Random(DateTime.UtcNow.Millisecond);

        var oponente = Partida.OponenteDe(IdBot);
        if (oponente == null)
        {
            throw new JugadorIncorrecto(IdBot);
        }

        TableroOponente = oponente.Tablero;
    }

    public List<Comando> Siguiente()
    {
        switch (Partida.Estado)
        {
            case EstadoPartida.Configuración:
                return Configurar();
            case EstadoPartida.TurnoJugadorA:
            case EstadoPartida.TurnoJugadorB:
                if (Partida.EsTurnoDe(IdBot))
                {
                    return Atacar();
                }
                break;
            case EstadoPartida.Terminado:
            case EstadoPartida.TerminadoPorReloj:
                return new()
                {
                    new Comando
                    {
                        Accion = Comando.Tipo.Eliminar,
                        Coordenadas = new(),
                    }
                };
            default:
                break;
        }

        return new()
        {
            new Comando
            {
                Accion = Comando.Tipo.Esperar,
                Coordenadas = new List<Coord>(),
            }
        };
    }

    public List<Comando> Configurar()
    {
        // Para simplificar la implementación de ésta función
        // vamos a hacer que el bot use siempre la misma orientación
        // para posicionar los barcos. Posteriormente se podría mejorar
        // éste método.

        if (Partida.BarcosFaltantes(IdBot).Count == 0)
        {
            return new()
            {
                new Comando
                {
                    Accion = Comando.Tipo.Esperar,
                    Coordenadas = new(),
                }
            };
        }

        var opciones = Enumerable.Range(0, Partida.JugadorA.Tablero.Ancho).ToList();
        var primeraSeleccion = new List<int>();
        var faltantes = new List<int>(Partida.BarcosFaltantes(IdBot))
            .OrderBy(x => _rng.Next())
            .ToList();

        while (primeraSeleccion.Count != faltantes.Count)
        {
            var indice = _rng.Next(opciones.Count);
            primeraSeleccion.Add(opciones[indice]);
            opciones.RemoveAt(indice);
        }

        var comandos = new List<Comando>();

        foreach (var primera in primeraSeleccion)
        {
            var largo = faltantes.First();
            faltantes.RemoveAt(0);

            var alto = Partida.JugadorA.Tablero.Alto;
            var inicio = _rng.Next(alto - largo + 1);
            comandos.Add(new Comando
            {
                Accion = Comando.Tipo.Agregar,
                Coordenadas = new List<Coord>()
                {
                    new Coord(primera, inicio),
                    new Coord(primera, inicio + largo - 1),
                },
            });
        }

        return comandos;
    }

    private Coord? SiguienteCoordenada()
    {
        var vacías = new List<Coord>();

        for (int x = 0; x < TableroOponente.Ancho; x++)
        {
            for (int y = 0; y < TableroOponente.Alto; y++)
            {
                var coord = new Coord(x, y);
                var celda = TableroOponente.GetCelda(coord);
                switch (celda.celda)
                {
                    case Celda.Vacio:
                    case Celda.Barco:
                    case Celda.Revelado:
                        vacías.Add(coord);
                        break;
                    default:
                        break;
                }
            }
        }

        if (vacías.Count == 0)
        {
            return null;
        }

        return vacías[_rng.Next(vacías.Count)];
    }

    public List<Comando> Atacar()
    {
        if (_tocadoInicial == null || _tocadoActual == null)
        {
            var coord = SiguienteCoordenada();
            if (coord == null)
            {
                return new()
                {
                    new Comando
                    {
                        Accion = Comando.Tipo.Atacar,
                        Coordenadas = new List<Coord>()
                        {
                            new Coord(0, 0),
                        },
                    }
                };
            }

            var celda = TableroOponente.GetCelda((Coord)coord);
            switch (celda.celda)
            {
                case Celda.Barco:
                case Celda.Revelado:
                    _tocadoInicial = coord;
                    _tocadoActual = coord;
                    break;
                default:
                    break;
            }

            return new()
            {
                new Comando
                {
                    Accion = Comando.Tipo.Atacar,
                    Coordenadas = new List<Coord>() { (Coord) coord },
                }
            };
        }
        else
        {
            var inicial = (Coord)_tocadoInicial;
            var actual = (Coord)_tocadoActual;

            var coordenadasPosibles = new List<Coord>();

            if (inicial == actual)
            {
                if (actual.X + 1 < TableroOponente.Ancho)
                {
                    coordenadasPosibles.Add(new Coord(actual.X + 1, actual.Y));
                }

                if (actual.X - 1 >= 0)
                {
                    coordenadasPosibles.Add(new Coord(actual.X - 1, actual.Y));
                }

                if (actual.Y + 1 < TableroOponente.Alto)
                {
                    coordenadasPosibles.Add(new Coord(actual.X, actual.Y + 1));
                }

                if (actual.Y - 1 >= 0)
                {
                    coordenadasPosibles.Add(new Coord(actual.X, actual.Y - 1));
                }
            }
            else
            {
                if (inicial.X == actual.X)
                {
                    if (actual.Y > inicial.Y)
                    {
                        if (actual.Y + 1 < TableroOponente.Alto)
                        {
                            coordenadasPosibles.Add(new Coord(actual.X, actual.Y + 1));
                        }

                        if (inicial.Y - 1 >= 0)
                        {
                            coordenadasPosibles.Add(new Coord(inicial.X, inicial.Y - 1));
                        }
                    }
                    else
                    {
                        if (actual.Y - 1 >= 0)
                        {
                            coordenadasPosibles.Add(new Coord(actual.X, actual.Y - 1));
                        }

                        if (inicial.Y + 1 < TableroOponente.Alto)
                        {
                            coordenadasPosibles.Add(new Coord(inicial.X, inicial.Y + 1));
                        }
                    }
                }

                if (inicial.Y == actual.Y)
                {
                    if (actual.X > inicial.X)
                    {
                        if (actual.X + 1 < TableroOponente.Ancho)
                        {
                            coordenadasPosibles.Add(new Coord(actual.X + 1, actual.Y));
                        }

                        if (inicial.X - 1 >= 0)
                        {
                            coordenadasPosibles.Add(new Coord(inicial.X - 1, inicial.Y));
                        }
                    }
                    else
                    {
                        if (actual.X - 1 >= 0)
                        {
                            coordenadasPosibles.Add(new Coord(actual.X - 1, actual.Y));
                        }

                        if (inicial.X + 1 < TableroOponente.Ancho)
                        {
                            coordenadasPosibles.Add(new Coord(inicial.X + 1, inicial.Y));
                        }
                    }
                }
            }

            foreach (var coord in coordenadasPosibles)
            {
                var celda = TableroOponente.GetCelda(coord);
                switch (celda.celda)
                {
                    case Celda.Barco:    // Ésta posición no fue atacada.
                        _tocadoActual = coord;
                        if (celda.barco != null)
                        {
                            if (celda.barco.Largo - celda.barco.Golpes.Count <= 1)
                            {
                                _tocadoInicial = _tocadoActual = null;
                            }
                        }
                        goto case Celda.Vacio;
                    case Celda.Vacio:
                    case Celda.Revelado:
                        return new()
                            {
                                new Comando
                                {
                                    Accion = Comando.Tipo.Atacar,
                                    Coordenadas = new List<Coord>() { (Coord) coord },
                                }
                            };
                    case Celda.Agua:   // Ésta posición ya fue atacada
                    case Celda.Tocado:
                    default:
                        break;
                }
            }

            // No hay coordenada para atacar que no haya sido atacada ya
            // Intentamos nuevamente, pero con un ataque aleatorio
            _tocadoInicial = _tocadoActual = null;
            return Atacar();
        }
    }
}
