using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using ProductosMongoRestNet.Config.Storage;
using ProductosMongoRestNet.Database;
using ProductosMongoRestNet.Services;
using ProductosMongoRestNet.Services.Storage;
using Serilog;
using Serilog.Core;

// Init local confing
var environment = InitLocalEnvironment();

// Init App Configuration
var configuration = InitConfiguration();

// Iniciamos la configuración externa de la aplicación
var logger = InitLogConfig();

// Inicializamos los servicios de la aplicación
var builder = InitServices();

// Creamos la aplicación
var app = builder.Build();

// Swagger para documentar la API
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Usamos HTTPS redirection
app.UseHttpsRedirection();

// Habilitamos el middleware de Autorización
app.UseAuthorization();

app.UseRouting(); // Habilitamos el middleware de enrutamiento

// Mapeamos los controladores a la aplicación
app.MapControllers();

// Ejecutamos la aplicación

Console.WriteLine(
    $"🚀 Running service in url: {builder.Configuration["urls"] ?? "not configured"} in mode {environment} 🟢");
logger.Information(
    $"🚀 Running service in url: {builder.Configuration["urls"] ?? "not configured"} in mode {environment} 🟢");
app.Run();


// Inicializa los servicios de la aplicación
WebApplicationBuilder InitServices()
{
    var myBuilder = WebApplication.CreateBuilder(args);

    // Configuramos los servicios de la aplicación

    // Poner Serilog como logger por defecto (otra alternativa)
    myBuilder.Services.AddLogging(logging =>
    {
        logging.ClearProviders(); // Limpia los proveedores de log por defecto
        logging.AddSerilog(logger, true); // Añade Serilog como un proveedor de log
    });
    logger.Debug("Serilog added as default logger");


    // Conexión a la base de datos
    myBuilder.Services.Configure<BookStoreMongoConfig>(
        myBuilder.Configuration.GetSection("BookStoreDatabase"));
    TryConnectionDataBase(); // Intentamos conectar a la base de datos

    // Cache en memoria
    myBuilder.Services.AddMemoryCache();

    // Configuración de almacenamiento de archivos
    myBuilder.Services.Configure<FileStorageConfig>(
        myBuilder.Configuration.GetSection("FileStorage"));
    StorageInit(); // Inicializamos el almacenamiento de archivos

    // Servicios de books
    // myBuilder.Services.AddSingleton<BooksService>();
    // Si no quieres iniciar el controlador con la implementación directa, puedes hacerlo con la interfaz
    // Registro de la interfaz y implementación
    // myBuilder.Services.AddSingleton<BooksService>();

    myBuilder.Services.AddSingleton<IBooksService, BooksService>();

    // Servicios de storage
    myBuilder.Services.AddSingleton<IFileStorageService, FileStorageService>();


    // Añadimos los controladores
    myBuilder.Services.AddControllers();


    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    myBuilder.Services.AddEndpointsApiExplorer(); // para documentar la API
    myBuilder.Services.AddSwaggerGen(); // para documentar la API
    return myBuilder;
}


string InitLocalEnvironment()
{
    Console.OutputEncoding = Encoding.UTF8; // Necesario para mostrar emojis
    var myEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";
    // Console.WriteLine($"Environment: {myEnvironment}");
    return myEnvironment;
}

// Inicializa la configuración de la aplicación
IConfiguration InitConfiguration()
{
    var myConfiguration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", false, true)
        .AddJsonFile($"appsettings.{environment}.json", true)
        .Build();
    return myConfiguration;
}

// Inicializa la configuración externa de la aplicación
Logger InitLogConfig()
{
    // Creamos un logger con la configuración de Serilog
    return new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
}

void TryConnectionDataBase()
{
    logger.Debug("Trying to connect to MongoDB");
    // Leemos la cadena de conexión a la base de datos desde la configuración
    var connectionString = configuration.GetSection("BookStoreDatabase:ConnectionString").Value;
    var settings = MongoClientSettings.FromConnectionString(connectionString);
    // Set the ServerApi field of the settings object to set the version of the Stable API on the client
    settings.ServerApi = new ServerApi(ServerApiVersion.V1);
    // Create a new client and connect to the server
    var client = new MongoClient(settings);
    // Send a ping to confirm a successful connection
    try
    {
        client.GetDatabase("DatabaseName").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
        logger.Information("🟢 You successfully connected to MongoDB!");
    }
    catch (Exception ex)
    {
        logger.Error(ex, "🔴 Error connecting to , closing application");
        Environment.Exit(1);
    }
}

void StorageInit()
{
    logger.Debug("Initializing file storage");
    // Inicializamos el almacenamiento de archivos
    var fileStorageConfig = configuration.GetSection("FileStorage").Get<FileStorageConfig>();
    // Creamos un directorio si no existe
    Directory.CreateDirectory(fileStorageConfig.UploadDirectory);
    // Configuramos el almacenador de archivos
    // Si tememos la clave RemoveAll a true, eliminamos todos los archivos del directorio
    if (fileStorageConfig.RemoveAll)
    {
        logger.Debug("Removing all files in the storage directory");
        foreach (var file in Directory.GetFiles(fileStorageConfig.UploadDirectory))
            File.Delete(file);
    }

    logger.Information("🟢 File storage initialized successfully!");
}