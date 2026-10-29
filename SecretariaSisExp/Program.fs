open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
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
TwilioClient.Init(twilioSid, twilioToken)

// 3. Start the Web Host
let builder = WebApplication.CreateBuilder()
builder.Services.AddHttpClient() |> ignore
let app = builder.Build()

app.MapPost("/api/voice", fun (req: Microsoft.AspNetCore.Http.HttpRequest, httpFactory: IHttpClientFactory) -> 
    let client = httpFactory.CreateClient()
    Telephony.handleVoiceRequest req client
) |> ignore

app.Run()