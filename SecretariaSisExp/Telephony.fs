namespace SecretariaSisExp.Core 

open Twilio.Rest.Api.V2010.Account
open System
open Microsoft.AspNetCore.Http
open Twilio.TwiML
open Twilio.Types
open System.Net.Http
open Microsoft.Extensions.Logging

module Telephony =
    let handleVoiceRequest (req: HttpRequest) (client: HttpClient) (logger: ILogger) = task {
        logger.LogInformation($"[Incoming] Method: {req.Method} | Path: {req.Path}")
        
        try
            let twilioPhone = Environment.GetEnvironmentVariable("TWILIO_PHONE_NUMBER")
            
            let! form = req.ReadFormAsync()
            logger.LogInformation($"[Form Data] Parsed {form.Count} keys successfully.")
            
            // Print the raw payload so we can see exactly what Twilio is sending
            for key in form.Keys do
                logger.LogDebug($"[Payload] {key}: {form.[key]}")

            let speechResult = if form.ContainsKey("SpeechResult") then form.["SpeechResult"].ToString() else ""
            let callerPhone = if form.ContainsKey("From") then form.["From"].ToString() else ""

            logger.LogInformation($"[Extracted] Caller: {callerPhone} | Speech: '{speechResult}'")

            // Hardcoded fallback logic (Grok bypassed)
            let aiReply = 
                if String.IsNullOrWhiteSpace(speechResult) then
                    "Diagnostic mode activated. I am listening for your voice."
                else
                    "I heard you say: " + speechResult

            logger.LogInformation($"[Generating Audio] Text: {aiReply}")

            if aiReply.ToLowerInvariant().Contains("text") && not (String.IsNullOrWhiteSpace(callerPhone)) then
                try
                    let messageOptions = CreateMessageOptions(PhoneNumber(callerPhone))
                    messageOptions.From <- PhoneNumber(twilioPhone)
                    messageOptions.Body <- "Hello! Here is the follow-up scheduling link."

                    MessageResource.Create(messageOptions) |> ignore
                    logger.LogInformation("[SMS] Sent successfully.")
                with ex -> 
                    logger.LogError(ex, "[SMS ERROR] Failed to send text.")

            let response = VoiceResponse()
            let gather = Twilio.TwiML.Voice.Gather(
                input = [| Twilio.TwiML.Voice.Gather.InputEnum.Speech |], 
                action = Uri("/api/voice", UriKind.Relative),
                speechTimeout = "auto"
            )
            
            gather.Say(aiReply, voice = Twilio.TwiML.Voice.Say.VoiceEnum.PollyJoanna) |> ignore
            response.Append(gather) |> ignore

            let xmlOutput = response.ToString()
            logger.LogInformation($"[TwiML Output]\n{xmlOutput}")

            return Results.Text(xmlOutput, "application/xml")

        with ex ->
            logger.LogError(ex, "[CRITICAL FATAL] Failed to process voice request.")
            
            let errResponse = VoiceResponse()
            errResponse.Say("A critical server error occurred.", voice = Twilio.TwiML.Voice.Say.VoiceEnum.PollyJoanna) |> ignore
            
            return Results.Text(errResponse.ToString(), "application/xml")
    }