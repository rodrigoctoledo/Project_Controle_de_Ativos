# Project_Controle_de_Ativos

# AssetControl — Controle de Ativos  
API .NET 8 + SQLite + JWT (Docker) • Frontend React/Next.js (fora do Docker)

Sistema simples para gerenciar ativos (monitores, teclados, projetores) com cadastro, listagem e fluxo de **empréstimo (check-out)** e **devolução (check-in)**.

> ✅ **Importante:** Somente a **API** roda em **Docker**.  
> ✅ Para criar o usuário **admin**, abra o **Swagger** e **execute o _segundo endpoint da seção Auth_**: `POST /api/auth/bootstrap`.  
> ✅ Em seguida, faça **login** em `POST /api/auth/login` e use o token no botão **Authorize** do Swagger.

---

## Sumário
- [Arquitetura](#arquitetura)
- [Requisitos](#requisitos)
- [Como rodar a API (Docker)](#como-rodar-a-api-docker)
- [Criar usuário admin (Swagger → /bootstrap)](#criar-usuário-admin-swagger--bootstrap)
- [Login e uso do JWT no Swagger](#login-e-uso-do-jwt-no-swagger)
- [Endpoints principais](#endpoints-principais)
- [Executar o Frontend (fora do Docker)](#executar-o-frontend-fora-do-docker)
- [Configuração (JWT, CORS, SQLite)](#configuração-jwt-cors-sqlite)
- [Troubleshooting](#troubleshooting)
- [Estrutura de pastas (exemplo)](#estrutura-de-pastas-exemplo)
- [Checklist para avaliação](#checklist-para-avaliação)
- [Licença](#licença)

---

## Arquitetura

- **Backend (API)**: .NET 8, SQLite (EF Core), JWT, Swagger, FluentValidation.  
  - Container Docker exposto em `http://localhost:5000`
  - Banco: arquivo `.db` persistido em volume (ex.: `/app/data/app.db`)
- **Frontend**: React / Next.js **rodando local** (sem Docker)  
  - Consome a API em `http://localhost:5000/api`

---

## Requisitos
- **Docker Desktop** (ou equivalente)
- (Para o frontend) **Node.js 18+** e **npm**/**pnpm**

---

## Como rodar a API (Docker)

Na pasta do **backend** (onde estiver o `docker-compose.yml` / `Dockerfile` da API):


docker compose up -d --build
API: http://localhost:5000

Swagger: http://localhost:5000/swagger

Após alterações no código/appsettings.json, reconstrua:

docker compose up -d --build
Criar usuário admin (Swagger → /bootstrap)
Acesse http://localhost:5000/swagger.

Vá para a seção Auth.

Execute o segundo endpoint (POST /api/auth/bootstrap) para criar o usuário admin a partir do seed definido no appsettings.json.
A resposta confirmará a criação (ou informará que já existe).

Credenciais padrão (se não alterar o appsettings.json):

Username: admin@local

Password: admin123

Login e uso do JWT no Swagger
Ainda no Swagger, execute POST /api/auth/login com:

{ "username": "admin@local", "password": "admin123" }
Copie o token retornado.

Clique em Authorize (topo da página) e informe: Bearer <cole-o-token>.

Com isso, os endpoints protegidos poderão ser testados no próprio Swagger.

Endpoints principais
Auth
POST /api/auth/bootstrap
Cria o usuário admin (executar uma vez, no Swagger).

POST /api/auth/login
Body:

{ "username": "admin@local", "password": "admin123" }
Retorno:

{ "token": "<jwt>" }
Assets
GET /api/assets
Query: page (1), pageSize (10), search, sortBy (name|code|status), sortDir (asc|desc)
Exemplo de retorno:

{
  "items": [
    {
      "id": 1,
      "name": "Monitor Dell 24\"",
      "code": "MON-001",
      "status": 0,
      "checkedOutBy": null,
      "notes": null,
      "checkedOutAt": null,
      "createdAt": "2025-08-13T00:00:00Z",
      "updatedAt": "2025-08-13T00:00:00Z"
    }
  ],
  "total": 1,
  "page": 1,
  "pageSize": 10"
}
POST /api/assets
Body:


{ "name": "Teclado Mecânico", "code": "TEC-002" }
PUT /api/assets/{id}
Atualiza um ativo (nome, código, status, notas, etc.).

DELETE /api/assets/{id}
Remove um ativo.

Check-out / Check-in
Dependendo da implementação, pode ser via PUT no próprio recurso alterando status e campos relacionados (checkedOutBy, checkedOutAt, notes), ou via endpoints dedicados (consulte o Swagger).

Validações: name e code obrigatórios (FluentValidation).
HTTP: 200/201 sucesso, 400 validação, 404 não encontrado, 401/403 autenticação/autorização.

Executar o Frontend (fora do Docker)
No projeto do frontend (ex.: frontend/):

Crie .env.local:


NEXT_PUBLIC_API_BASE=http://localhost:5000/api
Evita problemas como .../api/api/... nas chamadas.

Instale e rode:

npm install
npm run dev
# http://localhost:3000
CORS: garanta que http://localhost:3000 (ou 5173) está em AllowedOrigins do backend.

Configuração (JWT, CORS, SQLite)
Trecho do appsettings.json relevante:

{
  "ConnectionStrings": {
    "Default": "Data Source=/app/data/app.db"
  },
  "Auth": {
    "Jwt": {
      "Key": "CHANGE_THIS_SUPER_SECRET_KEY_32_CHARS_MIN",
      "Issuer": "AssetControl",
      "Audience": "AssetControl"
    },
    "Users": [
      { "Username": "admin@local", "Password": "admin123", "Role": "Admin" }
    ]
  },
  "AllowedOrigins": [ "http://localhost:3000", "http://localhost:5173" ]
}
JWT Key: use 32+ caracteres.
CORS (exemplo no Program.cs):

builder.Services.AddCors(o =>
{
    o.AddPolicy("Default", p => p
        .WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    );
});
...
app.UseCors("Default");
Não use * quando houver credenciais; o navegador bloqueia o preflight.

SQLite: arquivo de banco no container (/app/data/app.db). Pode mapear volume no docker-compose.yml para persistência.

Troubleshooting
git push rejeitado (fetch first / histories)


git pull origin main --allow-unrelated-histories
git push -u origin main
CORS (“No 'Access-Control-Allow-Origin'” / preflight)

Inclua http://localhost:3000 (ou 5173) em AllowedOrigins.

Garanta app.UseCors("Default") antes de UseAuthentication/UseAuthorization.

Endpoint ficando /api/api/...

Ajuste NEXT_PUBLIC_API_BASE para http://localhost:5000/api e nas chamadas use apenas paths (/assets, /auth/login).

Swagger erro ISwaggerProvider
Registre e ative:


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
app.UseSwagger();
app.UseSwaggerUI();
Docker não refletiu mudanças


docker compose up -d --build
Estrutura de pastas (exemplo)

.
├── backend/
│   └── AssetControl.Api/
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   └── AssetsController.cs
│       ├── Data/AppDbContext.cs
│       ├── Domain/ (Entities: Asset, User; Enums: AssetStatus)
│       ├── DTOs/ (LoginRequest, AssetCreateDto, etc.)
│       ├── Services/ (AuthService, JwtService, AssetService)
│       ├── Validators/ (FluentValidation)
│       ├── Program.cs
│       ├── appsettings.json
│       ├── Dockerfile
│       └── docker-compose.yml
└── frontend/ (Next.js/React rodando local, sem Docker)
