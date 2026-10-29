open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Twilio
open System.Net.Http
open Telephony

let builder = WebApplication.CreateBuilder()
builder.Services.AddHttpClient() |> ignore
let app = builder.Build()

TwilioClient.Init("YOUR_TWILIO_SID", "YOUR_TWILIO_AUTH_TOKEN")

app.MapPost("/api/voice", fun (req: Microsoft.AspNetCore.Http.HttpRequest, httpFactory: IHttpClientFactory) -> 
    let client = httpFactory.CreateClient()
    Telephony.handleVoiceRequest req client
) |> ignore

app.Run()