module AiBrain

open System.Net.Http
open System.Text
open System.Text.Json
open System.Text.Json.Nodes
open System.Threading.Tasks

let grokApiKey = "xai-YOUR_API_KEY_HERE"

let getAiResponse (userSpeech: string) (client: HttpClient) : Task<string> = task {
    client.DefaultRequestHeaders.Authorization <- Headers.AuthenticationHeaderValue("Bearer", grokApiKey)
    
    let payload = {| 
        model = "grok-4.7" 
        messages = [| 
            {| role = "system"; content = "You are a concise expert system secretary for PowerISA. Keep responses under 2 sentences. If they want to book an appointment, say you will text them." |}
            {| role = "user"; content = userSpeech |} 
        |] 
    |}
    
    let json = JsonSerializer.Serialize(payload)
    use content = new StringContent(json, Encoding.UTF8, "application/json")
    
    let! response = client.PostAsync("https://api.x.ai/v1/chat/completions", content)
    let! responseString = response.Content.ReadAsStringAsync()
    
    try
        let jsonNode = JsonNode.Parse(responseString)
        return jsonNode.["choices"].[0].["message"].["content"].ToString()
    with _ -> 
        return "I am sorry, my system is currently updating."
}