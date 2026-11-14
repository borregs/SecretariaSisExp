# <center>Secretaria Experta</center>
SecretariaSisExp is an agentic AI-powered phone assistant built with F# and ASP.NET Core. It listens for Twilio voice requests, sends the caller’s speech to an AI model, and responds with a spoken answer. If the AI response indicates a follow-up or scheduling need, the app can also send a text message to the caller.

## 📞 Live beta Telephony Line

> **Phone Number:**  
> <s> # **`+1 (737) 250-8034`** </s>

[![Subscribe to Beta](https://img.shields.io/badge/Subscribe%20to%20Beta-ITM%20Whitelisted%20Only-f59e0b?style=for-the-badge&logo=twilio&logoColor=white)](https://voice.borregs.qzz.io)

### Access Policy
Due to upstream Twilio Trial restrictions, inbound calls are screened. Only pre-registered **whitelisted Phone Numbers** can bridge into the live expert system. 

👉 **[Click here to request beta caller access](https://voice.borregs.qzz.io)**


### Live Call Log Demonstration
<details>
<summary><b>Click to expand full server execution trace</b></summary>

```text
[SYSTEM] Environment variables loaded securely.
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Telephony[0]
      [Incoming] Method: POST | Path: /api/voice
info: Telephony[0]
      [Form Data] Parsed 7 keys successfully.
info: Telephony[0]
      [Extracted] Caller: +52XXXXXXXXXX | Speech: 'I would like to schedule a showing'
info: Telephony[0]
      [Generating Audio] Text: I will schedule that right away...
info: Telephony[0]
      [TwiML Output]
<?xml version="1.0" encoding="UTF-8"?>
<Response>
    <Gather action="[https://voice.yourdomain.com/api/voice](https://voice.yourdomain.com/api/voice)" input="speech" speechTimeout="auto">
        <Say voice="Polly.Joanna">I will schedule that right away...</Say>
    </Gather>
</Response>
info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
      Request finished HTTP/1.1 POST [http://voice.yourdomain.com/api/voice](http://voice.yourdomain.com/api/voice) - 200 application/xml
```
</details>

## .NET 10 compliant Microservice

[![.NET Architecture diagram](https://gitdiagram.com/diagram-badge.svg)](https://gitdiagram.com/borregs/secretariasisexp?utm_source=readme&utm_medium=badge)

<img width="4588" height="5135" alt="diagram" src="https://github.com/user-attachments/assets/25b9737a-8435-462b-a9c8-2fc755caff90" />
## Features

- Twilio voice webhook endpoint at `/api/voice`
- Speech input handling via Twilio Gather
- Speech responses via dynamic Twiml Generation (XML workaround 😢)
- AI-generated voice responses using <s>xAI/Grok</s>
- <s>Optional SMS follow-up message to the caller</s>
- Confidence engine decicion assesing (New SMS follow up Engine)
- Built with .NET 9 and F#

## Project structure

- `SecretariaSisExp/Program.fs` — app startup and route registration
- `SecretariaSisExp/Telephony.fs` — Twilio voice request flow
- <s> `SecretariaSisExp/AiBrain.fs` — AI model request logic</s>
- `SecretariaSisExp/SecretariaSisExp.fsproj` — project configuration

## Requirements

- .NET 9 SDK
- Twilio account and phone number
- <s> xAI API key </s>
- A webhook endpoint publicly accessible by Twilio

## Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/borregs/SecretariaSisExp.git
   cd SecretariaSisExp
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```


4. Run the project:
   ```bash
   dotnet run
   ```

## Environment variables

 use environment variables such as:

```bash
export TWILIO_ACCOUNT_SID="your_sid"
export TWILIO_AUTH_TOKEN="your_auth_token"
export XAI_API_KEY="your_xai_key"
```

Then load them in code using Env.fs

## Twilio integration

This app exposes a POST endpoint:

```http
POST /api/voice
```
this is our twilio webhook, Beta Reachable at:

> **Phone Number:**  
> # **`+1 (737) 250-8034`**
👉 **[Click here to request beta caller access](https://voice.borregs.qzz.io)**

Twilio sends form data including:
- `SpeechResult`
- `From`

The server uses that data to:
- generate an AI response
- speak it back to the caller
- optionally send a text message if the response suggests a follow-up

## Security notes

- Do not commit API keys to Git
- Env.fs is the secrets manager
- File`.env` has been added to `.gitignore` add secrets there. 

## Future improvements

- Elevenlabs voice
- Add unit/integration tests
- Improve AI prompt behavior and response validation


This project is Proprietary. 
