namespace SecretariaSisExp.Infrastructure

open System
open System.IO

module Env =
    let load () =
        let filePath = ".env"
        if File.Exists(filePath) then
            File.ReadAllLines(filePath)
            // Ignore blank lines and comments
            |> Array.filter (fun line -> not (String.IsNullOrWhiteSpace(line)) && not (line.StartsWith("#")))
            |> Array.iter (fun line ->
                let parts = line.Split('=', 2)
                if parts.Length = 2 then
                    let key = parts.[0].Trim()
                    let value = parts.[1].Trim()
                    Environment.SetEnvironmentVariable(key, value)
            )
            printfn "[SYSTEM] Environment variables loaded securely."
        else
            Console.ForegroundColor <- ConsoleColor.Yellow
            printfn "[SYSTEM WARNING] No .env file found. Proceeding with existing environment."
            Console.ResetColor()