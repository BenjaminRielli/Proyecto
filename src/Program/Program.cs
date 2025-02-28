﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using Library;

public class Program
{
    // La instancia del bot.
    private static TelegramBotClient Bot = null!;

    // El token provisto por Telegram al crear el bot. Mira el archivo README.md en la raíz de este repo para
    // obtener indicaciones sobre cómo configurarlo.
    private static string token = String.Empty;

    private static BatallaNaval juego = new();

    // Esta clase es un POCO -vean https://en.wikipedia.org/wiki/Plain_old_CLR_object- para representar el token
    // secreto del bot.
    private class BotSecret
    {
        public string Token { get; set; } = String.Empty;
    }

    // Una interfaz requerida para configurar el servicio que lee el token secreto del bot.
    private interface ISecretService
    {
        string Token { get; }
    }

    // Una clase que provee el servicio de leer el token secreto del bot.
    private class SecretService : ISecretService
    {
        private readonly BotSecret _secrets;

        public SecretService(IOptions<BotSecret> secrets)
        {
            _secrets = secrets.Value ?? throw new ArgumentNullException(nameof(secrets));
        }

        public string Token { get { return _secrets.Token; } }
    }

    // Configura la aplicación.
    private static void Start()
    {
        // Lee una variable de entorno NETCORE_ENVIRONMENT que si no existe o tiene el valor 'development' indica
        // que estamos en un ambiente de desarrollo.
        var developmentEnvironment = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
        var isDevelopment =
            string.IsNullOrEmpty(developmentEnvironment) ||
            developmentEnvironment.ToLower() == "development";

        var builder = new ConfigurationBuilder();
        builder
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        // En el ambiente de desarrollo el token secreto del bot se toma de la configuración secreta
        if (isDevelopment)
        {
            builder.AddUserSecrets<Program>();
        }

        var configuration = builder.Build();

        IServiceCollection services = new ServiceCollection();

        // Mapeamos la implementación de las clases para  inyección de dependencias
        services
            .Configure<BotSecret>(configuration.GetSection(nameof(BotSecret)))
            .AddSingleton<ISecretService, SecretService>();

        var serviceProvider = services.BuildServiceProvider();
        var revealer = serviceProvider.GetService<ISecretService>();
        if (revealer != null)
        {
            token = revealer.Token;
        }
    }

    /// <summary>
    /// Punto de entrada al programa.
    /// </summary>
    public static void Main()
    {
        Start();

        Bot = new TelegramBotClient(token);

        var cts = new CancellationTokenSource();

        // Comenzamos a escuchar mensajes. Esto se hace en otro hilo (en background). El primer método
        // HandleUpdateAsync es invocado por el bot cuando se recibe un mensaje. El segundo método HandleErrorAsync
        // es invocado cuando ocurre un error.
        Bot.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            new ReceiverOptions()
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            },
            cts.Token
        );

        Console.WriteLine($"Bot is up!");

        // Esperamos a que el usuario aprete Enter en la consola para terminar el bot.
        Console.ReadLine();

        // Terminamos el bot.
        cts.Cancel();
    }

    /// <summary>
    /// Maneja las actualizaciones del bot (todo lo que llega), incluyendo mensajes, ediciones de mensajes,
    /// respuestas a botones, etc. En este ejemplo sólo manejamos mensajes de texto.
    /// </summary>
    public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            // Sólo respondemos a mensajes de texto
            if (update.Type == UpdateType.Message)
            {
                if (update.Message != null)
                {
                    await HandleMessageReceived(botClient, update.Message);
                }
            }
        }
        catch (Exception e)
        {
            await HandleErrorAsync(botClient, e, cancellationToken);
        }
    }

    private static async Task ProcesarMensaje(Library.Message mensaje)
    {
        var respuesta = juego.ProcesarMensaje(mensaje);

        if (!String.IsNullOrEmpty(respuesta.Remitente))
        {
            long chatId;
            if (long.TryParse(mensaje.IdJugador.Value, out chatId))
            {
                await Bot.SendTextMessageAsync(
                    chatId,
                    $"```\n{respuesta.Remitente}\n```",
                    ParseMode.MarkdownV2
                );
            }
        }

        if (!String.IsNullOrEmpty(respuesta.Oponente) && respuesta.IdOponente != null)
        {
            var idOponente = (Ident)respuesta.IdOponente;
            long chatId;
            if (long.TryParse(idOponente.Value, out chatId))
            {
                await Bot.SendTextMessageAsync(
                    chatId,
                    $"```\n{respuesta.Oponente}\n```",
                    ParseMode.MarkdownV2
                );
            }
        }
    }

    /// <summary>
    /// Maneja los mensajes que se envían al bot a través de handlers de una chain of responsibility.
    /// </summary>
    /// <param name="message">El mensaje recibido</param>
    /// <returns></returns>
    private static async Task HandleMessageReceived(ITelegramBotClient botClient, Telegram.Bot.Types.Message message)
    {
        if (message.From != null)
        {
            Console.WriteLine($"Received a message from {message.From.FirstName} saying: {message.Text}");
        }

        if (message.Text != null)
        {
            string nombre = String.Empty;
            if (message.From != null)
            {
                nombre = message.From.FirstName;
                if (!String.IsNullOrEmpty(message.From.LastName))
                {
                    nombre += $", {message.From.LastName}";
                }
            }

            await ProcesarMensaje(new Library.Message
            {
                IdJugador = new Ident(message.Chat.Id.ToString()),
                Text = message.Text,
                Nombre = nombre
            });

            foreach (var mensaje in juego.EjecutarBots())
            {
                Console.WriteLine($"# {mensaje.Nombre} ({mensaje.IdJugador.Value}), dijo: {mensaje.Text}");
                await ProcesarMensaje(mensaje);
            }
        }
    }

    /// <summary>
    /// Manejo de excepciones. Por ahora simplemente la imprimimos en la consola.
    /// </summary>
    public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine(exception.Message);
        return Task.CompletedTask;
    }
}
