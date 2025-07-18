# EchoMail

EchoMail is a lightweight **.NET 9 Minimal API** that lets you:

1. Register / log-in users (JWT + SQLite)
2. Relay arbitrary emails through **Gmail SMTP**
3. Accept and forward **“Contact Me”** form submissions rendered with an HTML template

Perfect for portfolios, landing pages, internal tools, or any project that needs a tiny, self-hosted mail relay with authentication.

## ✨ Features

- Minimal-API, no controllers
- JWT authentication (register / login)
- SQLite database (Entity Framework Core)
- Gmail App-Password SMTP relay
- HTML e-mail templates (editable in `Templates/`)
- Environment specific settings (`appsettings.json` + `appsettings.Development.json`)
- Automatic EF migrations on startup

## 🖥 Prerequisites

| Tool                                 | Notes                                              |
| ------------------------------------ | -------------------------------------------------- |
| .NET 9 SDK Preview                   | `https://dotnet.microsoft.com/download/dotnet/9.0` |
| SQLite (no install on Windows/macOS) | EF ships the native driver                         |
| Gmail account with **App Password**  | Normal password will NOT work                      |

> **Why an App Password?**  
> When 2-factor auth is enabled, Gmail blocks basic SMTP logins. Generate an App Password in Google Account → Security → App Passwords.

## 🏗 Project Structure

```
EchoMail/
│
├─ Data/              ← EF Core DbContext & migrations
├─ DTOs/              ← Request / response records
├─ Models/            ← Database entities
├─ Services/
│   ├─ AuthService.cs
│   └─ EmailService.cs
├─ Templates/
│   └─ contact.html   ← HTML template used by /contact
├─ Program.cs         ← Minimal-API entry point
├─ appsettings.json
└─ .gitignore
```

## ⚙️ Configuration

### 1. Base config (`appsettings.json`)

```jsonc
{
  "Jwt": { "Key": "REPLACE_WITH_LONG_RANDOM_KEY" },
  "ConnectionStrings": { "Default": "Data Source=echomail.db" },
  "Smtp": {
    "Gmail": {
      "Email": "YOUR_GMAIL_ADDRESS",
      "AppPassword": "YOUR_APP_PASSWORD"
    }
  }
}
```

### 2. Development overrides (`appsettings.Development.json`)

```jsonc
{
  "ConnectionStrings": { "Default": "Data Source=echomail-dev.db" },
  "Logging": { "LogLevel": { "Default": "Debug" } }
}
```

### 3. Never commit secrets

Add these files to **`.gitignore`**:

```
appsettings.Development.json
appsettings.Production.json
```

## 🚀 Getting Started

### 1. Clone & restore

```bash
git clone https://github.com/your-user/echomail.git
cd echomail
dotnet restore
```

### 2. Install EF CLI (once)

```bash
dotnet tool install --global dotnet-ef
```

### 3. Apply database migrations

```bash
dotnet ef database update
```

(Or let the app run once; it calls `db.Database.Migrate()` automatically.)

### 4. Run in Development

```bash
# Windows
set ASPNETCORE_ENVIRONMENT=Development
dotnet run

# macOS / Linux
export ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

API runs on `http://localhost:5000` (HTTPS on 5001 if dev-certs are installed).

## 🛣 API Endpoints

| Verb | Route       | Body DTO                               | Auth? | Purpose                        |
| ---- | ----------- | -------------------------------------- | ----- | ------------------------------ |
| POST | `/register` | `{email,password}`                     | ❌    | Create user                    |
| POST | `/login`    | `{email,password}`                     | ❌    | Returns JWT                    |
| POST | `/send`     | `{to,subject,body}`                    | ✅    | Relay custom email             |
| POST | `/contact`  | `{name,email,message,phone?,website?}` | ✅    | Sends styled contact-form mail |

**Header for protected routes**

```
Authorization: Bearer {JWT}
```

## 📬 Using the Contact Template

`Templates/contact.html` contains placeholders:

```
{{Name}} {{Email}} {{ContactInfo}} {{Message}} {{Timestamp}}
```

Edit CSS / layout freely; the service replaces placeholders at runtime.

## 📝 Curl Examples

```bash
# 1. Register
curl -X POST http://localhost:5000/register \
     -H "Content-Type: application/json" \
     -d '{"email":"me@example.com","password":"P@ssw0rd"}'

# 2. Login
TOKEN=$(curl -s http://localhost:5000/login \
        -H "Content-Type: application/json" \
        -d '{"email":"me@example.com","password":"P@ssw0rd"}' \
        | jq -r .token)

# 3. Send email
curl -X POST http://localhost:5000/send \
     -H "Authorization: Bearer $TOKEN" \
     -H "Content-Type: application/json" \
     -d '{"to":"friend@example.com","subject":"Hi","body":"Hello from EchoMail"}'
```

## 🌐 Deploying to Production

1. **Publish**

```bash
dotnet publish -c Release -o ./publish
```

2. **Set environment**

```bash
export ASPNETCORE_ENVIRONMENT=Production
export Jwt__Key="LONG_RANDOM_PROD_KEY"
export ConnectionStrings__Default="Data Source=/opt/data/echomail.db"
export Smtp__Gmail__Email="prod@mail.com"
export Smtp__Gmail__AppPassword="prod_app_password"
```

3. **Run**

```bash
dotnet ./publish/EchoMail.dll
```

### Docker quick-start

```bash
docker build -t echomail .
docker run -e ASPNETCORE_ENVIRONMENT=Production -p 80:80 echomail
```

_(Provide the other env vars with `-e` as shown above.)_

## 🛡 Security Checklist

- Use **HTTPS** in production
- Keep the JWT key secret & 32+ chars
- Rate-limit `/contact` & `/send` if exposed publicly
- Rotate Gmail App Password periodically
- Backup `echomail.db`

## 🤝 Contributing

1. Fork the repo
2. `git checkout -b feature/awesome`
3. Commit and push
4. Open a Pull Request

## 📜 License

MIT © 2024 Your Name / EchoMail
