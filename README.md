# SecretariaSisExp
# SecretariaSisExp

SecretariaSisExp is a voice-powered secretary assistant built with F# and ASP.NET Core. It listens for Twilio voice requests, sends the caller’s speech to an AI model, and responds with a spoken answer. If the AI response indicates a follow-up or scheduling need, the app can also send a text message to the caller.

## Features

- Twilio voice webhook endpoint at `/api/voice`
- Speech input handling via Twilio Gather
- AI-generated voice responses using xAI/Grok
- Optional SMS follow-up message to the caller
- Built with .NET 9 and F#

## Project structure

- `SecretariaSisExp/Program.fs` — app startup and route registration
- `SecretariaSisExp/Telephony.fs` — Twilio voice request flow
- `SecretariaSisExp/AiBrain.fs` — AI model request logic
- `SecretariaSisExp/SecretariaSisExp.fsproj` — project configuration

## Requirements

- .NET 9 SDK
- Twilio account and phone number
- xAI API key
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

3. Configure your secrets.
   The project currently contains placeholder credentials in code:
   - `TwilioClient.Init("YOUR_TWILIO_SID", "YOUR_TWILIO_AUTH_TOKEN")`
   - `let grokApiKey = "xai-YOUR_API_KEY_HERE"`

   Replace these with environment variables or a secure config mechanism before running the app.

4. Run the project:
   ```bash
   dotnet run
   ```

## Environment variables

A safer setup is to use environment variables such as:

```bash
export TWILIO_ACCOUNT_SID="your_sid"
export TWILIO_AUTH_TOKEN="your_auth_token"
export XAI_API_KEY="your_xai_key"
```

Then load them in code instead of hardcoding them.

## Twilio integration

This app exposes a POST endpoint:

```http
POST /api/voice
```

Twilio sends form data including:
- `SpeechResult`
- `From`

The server uses that data to:
- generate an AI response
- speak it back to the caller
- optionally send a text message if the response suggests a follow-up

## Security notes

- Do not commit API keys to Git
- Use environment variables or a secrets manager
- Add `.env` to `.gitignore` if you store local config there

## Future improvements

- Move secrets out of source code
- Add proper logging and error handling
- Add unit/integration tests
- Improve AI prompt behavior and response validation
- Add configuration for the Twilio phone number and response logic

## License

This project does not currently include a license file. Add one if you plan to share or distribute it publicly.
