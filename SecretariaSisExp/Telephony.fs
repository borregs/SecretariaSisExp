namespace SecretariaSisExp.Core 

open Twilio.Rest.Api.V2010.Account
open System
open Microsoft.AspNetCore.Http
open Twilio.Types
open System.Net.Http
open Microsoft.Extensions.Logging
open SecretariaSisExp.Infrastructure // Added for ConsoleUI

module Telephony =
    let handleVoiceRequest (req: HttpRequest) (client: HttpClient) (logger: ILogger) = task {
        ConsoleUI.sysLog $"[Incoming] Method: {req.Method} | Path: {req.Path}"
        
        try
            let twilioPhone = Environment.GetEnvironmentVariable("TWILIO_PHONE_NUMBER")
            
            // Defensively initialize variables
            let mutable speechResult = ""
            let mutable callerPhone = ""
            let mutable callSid = "unknown_call"
            let mutable callStatus = ""

            // Only try to read the form if it actually exists
            if req.HasFormContentType then
                let! form = req.ReadFormAsync()
                ConsoleUI.sysLog $"[Form Data] Parsed {form.Count} keys successfully."
                
                for key in form.Keys do
                    ConsoleUI.debugLog $"[Payload] {key}: {form.[key]}"
                    
                speechResult <- if form.ContainsKey("SpeechResult") then form.["SpeechResult"].ToString() else ""
                callerPhone <- if form.ContainsKey("From") then form.["From"].ToString() else ""
                
                // --- ADDED: Extract Session & Status variables ---
                callSid <- if form.ContainsKey("CallSid") then form.["CallSid"].ToString() else "unknown_call"
                callStatus <- if form.ContainsKey("CallStatus") then form.["CallStatus"].ToString() else ""
            else
                ConsoleUI.debugLog "[WARNING] Request did not have Form Content Type. Twilio might be sending a blank ping."

            // --- ADDED: Check if call hung up to wipe memory ---
            if callStatus = "completed" then
                AiBrain.endCall callSid
                return Results.Ok() :> IResult
            else
                ConsoleUI.sysLog $"[Extracted] Caller: {callerPhone} | Speech: '{speechResult}'"

                // --- Chat UI Log User ---
                if not (String.IsNullOrWhiteSpace(speechResult)) then
                    ConsoleUI.userMessage(callerPhone, speechResult)

                // --- ADDED: Call the AI Brain with the session ID ---
                let! aiReply = 
                    if String.IsNullOrWhiteSpace(speechResult) then
                        System.Threading.Tasks.Task.FromResult "Diagnostic mode activated. I am listening for your voice."
                    else
                        AiBrain.getAiResponse callSid speechResult client

                // --- Chat UI Log AI ---
                ConsoleUI.aiMessage(aiReply)

                // Trigger SMS logic if needed
                if aiReply.ToLowerInvariant().Contains("text") && not (String.IsNullOrWhiteSpace(callerPhone)) then
                    try
                        let messageOptions = CreateMessageOptions(PhoneNumber(callerPhone))
                        messageOptions.From <- PhoneNumber(twilioPhone)
                        messageOptions.Body <- "Hello! Here is the follow-up scheduling link."

                        MessageResource.Create(messageOptions) |> ignore
                        ConsoleUI.sysLog "[SMS] Sent successfully."
                    with ex -> 
                        ConsoleUI.sysLog $"[SMS ERROR] Failed to send text: {ex.Message}"

                // ----------------------------------------------------
                // Pure F# XML interpolation (Fixes the TwiML bug)
                // ----------------------------------------------------
                let xmlOutput = $"""<?xml version="1.0" encoding="UTF-8"?><Response>    <Gather input="speech" action="https://voice.borregs.qzz.io/api/voice" speechTimeout="auto">        <Say voice="Polly.Joanna">{aiReply}</Say>    </Gather>        <!-- If the user stays silent and it times out, it falls down to here instead of crashing -->    <Say voice="Polly.Joanna">I did not catch that.</Say>    <Redirect method="POST">https://voice.borregs.qzz.io/api/voice</Redirect></Response>"""

                ConsoleUI.sysLog $"[TwiML Output Generated]"

                // Return the raw XML string directly to Twilio (Upcast to IResult)
                return Results.Text(xmlOutput, "application/xml") :> IResult

        with ex ->
            // If anything above crashes, log it and tell Twilio to speak the error
            ConsoleUI.sysLog $"[CRITICAL FATAL] Failed to process voice request: {ex.Message}"
            
            let errXml = """<?xml version="1.0" encoding="UTF-8"?><Response>    <Say voice="Polly.Joanna">A critical server error occurred.</Say></Response>"""
            
            // Upcast to IResult for consistency
            return Results.Text(errXml, "application/xml") :> IResult
    }