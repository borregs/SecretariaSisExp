namespace SecretariaSisExp.Infrastructure

open System
open System.IO

module ConsoleUI =
    // ---------------------------------------------------------
    // OUTPUT ROUTING FLAGS
    // Set to true/false to route logs to the Terminal, File, or Both.
    // By default, system logs go to files, keeping the terminal clean.
    // ---------------------------------------------------------
    let sysInfoToTerminal = true
    let sysInfoToFile = true

    let sysDebugToTerminal = true
    let sysDebugToFile = true

    let chatTranscriptToFile = true

    let private lockObj = obj()

    // Thread-safe file writer
    let private appendToFile (filename: string) (msg: string) =
        lock lockObj (fun () ->
            try
                let timestamp = DateTime.Now.ToString("HH:mm:ss.fff")
                File.AppendAllLines(filename, [| $"[{timestamp}] {msg}" |])
            with _ -> ()
        )

    // System Info (Cyan)
    let sysLog (msg: string) =
        if sysInfoToTerminal then
            Console.ForegroundColor <- ConsoleColor.Cyan
            printfn $"[SYS INFO] {msg}"
            Console.ResetColor()
            
        if sysInfoToFile then 
            appendToFile "sys_info.log" $"[SYS INFO] {msg}"

    // System Debug (Dark Yellow)
    let debugLog (msg: string) =
        if sysDebugToTerminal then
            Console.ForegroundColor <- ConsoleColor.DarkYellow
            printfn $"[SYS DEBUG] {msg}"
            Console.ResetColor()
            
        if sysDebugToFile then 
            appendToFile "sys_debug.log" $"[SYS DEBUG] {msg}"

    // Left-justified user speech (Black on Gray)
    let userMessage (phone: string, msg: string) =
        Console.WriteLine()
        Console.ForegroundColor <- ConsoleColor.Black
        Console.BackgroundColor <- ConsoleColor.Gray
        printfn $"  CALLER ({phone}): {msg}  "
        Console.ResetColor()
        Console.WriteLine()
        
        if chatTranscriptToFile then 
            appendToFile "chat_transcript.log" $"CALLER ({phone}): {msg}"

    // Right-justified AI speech (White on Dark Blue)
    let aiMessage (msg: string) =
        let tag = $"  AI SECRETARY: {msg}  "
        let padding = Math.Max(0, 80 - tag.Length)
        let paddedTag = (String.replicate padding " ") + tag
        
        Console.ForegroundColor <- ConsoleColor.White
        Console.BackgroundColor <- ConsoleColor.DarkBlue
        printfn $"%s{paddedTag}"
        Console.ResetColor()
        Console.WriteLine()
        
        if chatTranscriptToFile then 
            appendToFile "chat_transcript.log" $"AI SECRETARY: {msg}"