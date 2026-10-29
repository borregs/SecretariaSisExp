namespace SecretariaSisExp.Core 

open System
open Microsoft.AspNetCore.Http
open Twilio.TwiML
open Twilio.Rest.Api
open Twilio.Types
open System.Net.Http
open Twilio.Rest.IpMessaging.V2.Service.Channel

module Telephony =
    let handleVoiceRequest (req: HttpRequest) (client: HttpClient) = task {
        let twilioPhone = Environment.GetEnvironmentVariable("TWILIO_PHONE_NUMBER")
        
        let! form = req.ReadFormAsync()
        let speechResult = if form.ContainsKey("SpeechResult") then form.["SpeechResult"].ToString() else ""
        let callerPhone = if form.ContainsKey("From") then form.["From"].ToString() else ""

        let! aiReply = 
            if String.IsNullOrWhiteSpace(speechResult) then
                System.Threading.Tasks.Task.FromResult "Hello, Power I S A. How can I help you today?"
            else
                AiBrain.getAiResponse speechResult client

        if aiReply.ToLowerInvariant().Contains("text") && not (String.IsNullOrWhiteSpace(callerPhone)) then
            try
                MessageResource.Create(
                    to_ = PhoneNumber(callerPhone),
                    from = PhoneNumber(twilioPhone),
                    body = "Hello! Here is the follow-up scheduling link."
                ) |> ignore
            with _ -> ()

        let response = VoiceResponse()
        let gather = Twilio.TwiML.Voice.Gather(
            input = [| Twilio.TwiML.Voice.Gather.InputEnum.Speech |], 
            action = Uri("/api/voice", UriKind.Relative),
            speechTimeout = "auto"
        )
        
        gather.Say(aiReply, voice = Twilio.TwiML.Voice.Say.VoiceEnum.PollyJoanna) |> ignore
        response.Append(gather) |> ignore

        return Results.Text(response.ToString(), "application/xml")
    }