namespace SecretariaSisExp.Core 

open Twilio.Rest.Api.V2010.Account
open System
open Microsoft.AspNetCore.Http
open Twilio.Types
open System.Net.Http
open Microsoft.Extensions.Logging

module Telephony =
    let handleVoiceRequest (req: HttpRequest) (client: HttpClient) (logger: ILogger) = task {
        logger.LogInformation($"[Incoming] Method: {req.Method} | Path: {req.Path}")
        
        try
            let twilioPhone = Environment.GetEnvironmentVariable("TWILIO_PHONE_NUMBER")
            
            // Defensively initialize variables
            let mutable speechResult = ""
            let mutable callerPhone = ""

            // Only try to read the form if it actually exists
            if req.HasFormContentType then
                let! form = req.ReadFormAsync()
                logger.LogInformation($"[Form Data] Parsed {form.Count} keys successfully.")
                
                for key in form.Keys do
                    logger.LogDebug($"[Payload] {key}: {form.[key]}")
                    
                speechResult <- if form.ContainsKey("SpeechResult") then form.["SpeechResult"].ToString() else ""
                callerPhone <- if form.ContainsKey("From") then form.["From"].ToString() else ""
            else
                logger.LogWarning("[WARNING] Request did not have Form Content Type. Twilio might be sending a blank ping.")

            logger.LogInformation($"[Extracted] Caller: {callerPhone} | Speech: '{speechResult}'")

            // Hardcoded diagnostic fallback logic (Grok bypassed for testing)
            let aiReply = 
                if String.IsNullOrWhiteSpace(speechResult) then
                    "Diagnostic mode activated. I am listening for your voice."
                else
                    "I heard you say: " + speechResult

            logger.LogInformation($"[Generating Audio] Text: {aiReply}")

            // Trigger SMS logic if needed
            if aiReply.ToLowerInvariant().Contains("text") && not (String.IsNullOrWhiteSpace(callerPhone)) then
                try
                    let messageOptions = CreateMessageOptions(PhoneNumber(callerPhone))
                    messageOptions.From <- PhoneNumber(twilioPhone)
                    messageOptions.Body <- "Hello! Here is the follow-up scheduling link."

                    MessageResource.Create(messageOptions) |> ignore
                    logger.LogInformation("[SMS] Sent successfully.")
                with ex -> 
                    logger.LogError(ex, "[SMS ERROR] Failed to send text.")

            // ----------------------------------------------------
            // Pure F# XML interpolation (Fixes the TwiML bug)
            // ----------------------------------------------------
            let xmlOutput = $"""<?xml version="1.0" encoding="UTF-8"?>
<Response>
    <Gather input="speech" action="https://voice.borregs.qzz.io/api/voice" speechTimeout="auto">
        <Say voice="Polly.Joanna">{aiReply}</Say>
    </Gather>
    
    <!-- If the user stays silent and it times out, it falls down to here instead of crashing -->
    <Say voice="Polly.Joanna">I did not catch that.</Say>
    <Redirect method="POST">https://voice.borregs.qzz.io/api/voice</Redirect>
</Response>"""

            logger.LogInformation($"[TwiML Output]\n{xmlOutput}")

            // Return the raw XML string directly to Twilio
            return Results.Text(xmlOutput, "application/xml")

        with ex ->
            // If anything above crashes, log it and tell Twilio to speak the error
            logger.LogError(ex, "[CRITICAL FATAL] Failed to process voice request.")
            
            let errXml = """<?xml version="1.0" encoding="UTF-8"?>
<Response>
    <Say voice="Polly.Joanna">A critical server error occurred.</Say>
</Response>"""
            
            return Results.Text(errXml, "application/xml")
    }