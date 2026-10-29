open System
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Twilio
open System.Net.Http

// Open by namespaces
open SecretariaSisExp.Infrastructure
open SecretariaSisExp.Core

// 1. Load the secrets first
Env.load()

// 2. Fetch and initialize Twilio
let twilioSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID")
let twilioToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN")
if not (String.IsNullOrWhiteSpace(twilioSid)) then
    TwilioClient.Init(twilioSid, twilioToken)

// 3. Start the Web Host
let builder = WebApplication.CreateBuilder()
builder.Services.AddHttpClient() |> ignore

// Force maximum verbosity in the console
builder.Logging.ClearProviders() |> ignore
builder.Logging.AddConsole() |> ignore
builder.Logging.SetMinimumLevel(LogLevel.Debug) |> ignore

let app = builder.Build()

app.MapPost(
    "/api/voice",
    System.Func<HttpContext, System.Threading.Tasks.Task<IResult>>(fun context ->
        let httpFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>()
        let client = httpFactory.CreateClient()
        
        // Grab the logger from the request services
        let loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>()
        let logger = loggerFactory.CreateLogger("Telephony")

        Telephony.handleVoiceRequest context.Request client logger
    )
) |> ignore

// 4. Trap GET requests so you know if Twilio is misconfigured
app.MapGet(
    "/api/voice",
    System.Func<HttpContext, string>(fun context ->
        let loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>()
        let logger = loggerFactory.CreateLogger("Routing")
        logger.LogWarning("[WARNING] Received a GET request. Twilio must be set to POST.")
        "This endpoint requires a POST request."
    )
) |> ignore

app.MapGet("/", System.Func<string>(fun () -> "Diagnostic server running.")) |> ignore

app.Run()