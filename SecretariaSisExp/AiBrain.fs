namespace SecretariaSisExp.Core

open System
open System.Net.Http
open System.Text
open System.Text.Json
open System.Text.Json.Nodes
open System.Threading.Tasks
open System.Net.Http.Headers
open System.Collections.Concurrent
open SecretariaSisExp.Infrastructure

module AiBrain =
    type ChatMessage = { role: string; content: string }

    // 100% Free In-Memory Database keyed by Twilio's CallSid
    let private activeCalls = ConcurrentDictionary<string, ChatMessage list>()

    let getAiResponse (callSid: string) (userSpeech: string) (client: HttpClient) : Task<string> = task {
        let apiKey = Environment.GetEnvironmentVariable("GROK_API_KEY")
        client.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Bearer", apiKey)
        
        let history = 
            match activeCalls.TryGetValue(callSid) with
            | true, existing -> existing
            | false, _ -> 
                [{ role = "system"; content = "You are a concise expert system secretary for PowerISA. Keep responses under 2 sentences. If they want to book an appointment, say you will text them." }]

        let updatedHistory = history @ [{ role = "user"; content = userSpeech }]

        let messagesArray = 
            updatedHistory 
            |> List.map (fun m -> {| role = m.role; content = m.content |}) 
            |> List.toArray

        // Groq's 
        let payload = {| 
            model = "openai/gpt-oss-20b" 
            messages = messagesArray 
        |}
        
        let json = JsonSerializer.Serialize(payload)
        use content = new StringContent(json, Encoding.UTF8, "application/json")
        
        try
            let! response = client.PostAsync("https://api.groq.com/openai/v1/chat/completions", content)
            let! responseString = response.Content.ReadAsStringAsync()
            
            if response.IsSuccessStatusCode then
                let jsonNode = JsonNode.Parse(responseString)
                let aiReply = jsonNode.["choices"].[0].["message"].["content"].ToString()

                // Save the AI's reply back to RAM
                let finalHistory = updatedHistory @ [{ role = "assistant"; content = aiReply }]
                activeCalls.[callSid] <- finalHistory
                
                return aiReply
            else
                // If Groq rejects it (e.g. 401 Unauthorized), print the raw reason
                ConsoleUI.debugLog $"[LLM API Error] Status: {response.StatusCode} | Body: {responseString}"
                return "I am sorry, my cognitive node is currently unavailable."
            
        with ex -> 
            ConsoleUI.debugLog $"[LLM Exception] {ex.Message}"
            return "I am sorry, my system is currently updating."
    }

    let endCall (callSid: string) =
        activeCalls.TryRemove(callSid) |> ignore
        ConsoleUI.sysLog $"Memory cleared for call {callSid}."