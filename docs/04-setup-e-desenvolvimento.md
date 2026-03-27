# ⚙️ Setup, Configuração e Guia de Desenvolvimento — WorshipHub

---

## Setup e Configuração

### Pré-requisitos

#### Ferramentas Obrigatórias

| Ferramenta | Versão Mínima | Observação |
|---|---|---|
| Git | qualquer recente | Controle de versão |
| Node.js | 18.x (CI usa 20.x) | Runtime do front-end |
| npm | incluído no Node | Gerenciador de pacotes |
| .NET SDK | 8.0 | Runtime e compilador do back-end |
| Docker + Compose | v2.x | Para banco via Docker ou deploy completo |

#### Ferramentas Recomendadas

| Ferramenta | Finalidade |
|---|---|
| Visual Studio 2022 / VS Code | Desenvolvimento back-end (.NET) |
| Volar (VS Code) | IntelliSense para Vue 3 |
| MySQL Workbench / DBeaver | Inspecionar o banco de dados |
| Postman / Thunder Client | Testar endpoints da API |
| Swagger UI | Disponível em `http://localhost:5255/swagger` no ambiente de dev |

---

### Instalação Passo a Passo

#### 1. Clonar o repositório

```bash
git clone https://github.com/lucaspccamara/WorshipHub.git
cd WorshipHub
```

#### 2. Configurar o banco de dados

**Opção A — Docker (recomendado):**
```bash
cd infra
cp .env.example .env
# Edite o .env com suas senhas
docker compose up mysql -d
```

**Opção B — MySQL local instalado:**
- Certifique-se que MySQL 8.0 está rodando
- Crie o banco manualmente: `CREATE DATABASE whdatabase;`

#### 3. Configurar o back-end

```bash
cd back-end/WorshipApi
```

Edite `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "mysql": "server=127.0.0.1;user=SEU_USUARIO;database=whdatabase;password=SUA_SENHA;"
  },
  "JWT_PRIVATE_KEY": "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----",
  "JWT_PUBLIC_KEY": "-----BEGIN PUBLIC KEY-----\n...\n-----END PUBLIC KEY-----",
  "AllowedOrigins": ["http://localhost:5173"]
}
```

> ⚠️ **Gerando par de chaves RSA** (caso não tenha):
> ```bash
> # Linux/macOS
> openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out private.pem
> openssl rsa -pubout -in private.pem -out public.pem
> # Copie o conteúdo dos arquivos para o appsettings, em uma única linha com \n
> ```
> No Windows, use o OpenSSL disponível no Git Bash ou WSL.

```bash
dotnet restore
dotnet build
```

#### 4. Configurar o front-end

```bash
cd front-end
```

Crie o arquivo `.env.development`:
```env
VITE_API_URL=http://localhost:5255/api
VITE_FIREBASE_API_KEY=sua_api_key
VITE_FIREBASE_AUTH_DOMAIN=seu_projeto.firebaseapp.com
VITE_FIREBASE_PROJECT_ID=seu_projeto
VITE_FIREBASE_STORAGE_BUCKET=seu_projeto.appspot.com
VITE_FIREBASE_MESSAGING_SENDER_ID=123456789
VITE_FIREBASE_APP_ID=1:123456789:web:abc123
VITE_FIREBASE_MEASUREMENT_ID=G-XXXXXXXX
VITE_FIREBASE_VAPID_KEY=sua_vapid_key
```

> ℹ️ As variáveis do Firebase são necessárias apenas para as notificações push. O app funciona sem elas (com avisos no console).

#### 5. Configurar o Firebase (para notificações push)

- Coloque o arquivo `firebase-service-account.json` em `back-end/WorshipApi/`
- Obtenha esse arquivo no [Firebase Console](https://console.firebase.google.com/) → Configurações do Projeto → Contas de Serviço → Gerar nova chave privada

```bash
npm install
```

---

### Tabela Completa de Variáveis de Ambiente

#### Back-end — `appsettings.Development.json`

| Variável | Descrição | Obrigatória | Exemplo |
|---|---|:---:|---|
| `ConnectionStrings.mysql` | Connection string do MySQL | ✅ | `server=127.0.0.1;user=root;database=whdatabase;password=senha;` |
| `JWT_PRIVATE_KEY` | Chave RSA privada para assinar JWT | ✅ | `-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----` |
| `JWT_PUBLIC_KEY` | Chave RSA pública para validar JWT | ✅ | `-----BEGIN PUBLIC KEY-----\n...\n-----END PUBLIC KEY-----` |
| `BrevoSettings.ApiKey` | Chave de API do Brevo v3 | ✅ | `SUA_CHAVE_AQUI` |
| `BrevoSettings.SenderEmail` | E-mail remetente do sistema | ✅ | `email@email.com.br` |
| `AllowedOrigins` | Origens permitidas pelo CORS | ✅ | `["http://localhost:5173"]` |

#### Back-end — Arquivo de serviço Firebase

| Arquivo | Descrição | Obrigatório |
|---|---|:---:|
| `firebase-service-account.json` | Credenciais do Firebase Admin SDK para FCM | ❌ (notificações desabilitadas sem ele) |

#### Front-end — `.env.development` / `.env.production`

| Variável | Descrição | Obrigatória | Exemplo |
|---|---|:---:|---|
| `VITE_API_URL` | URL base da API REST | ✅ | `http://localhost:5255/api` |
| `VITE_FIREBASE_API_KEY` | API Key do Firebase | ❌ | `AIzaSy...` |
| `VITE_FIREBASE_AUTH_DOMAIN` | Auth domain do projeto Firebase | ❌ | `projeto.firebaseapp.com` |
| `VITE_FIREBASE_PROJECT_ID` | ID do projeto Firebase | ❌ | `worshiphub-fcm` |
| `VITE_FIREBASE_STORAGE_BUCKET` | Bucket do Firebase Storage | ❌ | `projeto.appspot.com` |
| `VITE_FIREBASE_MESSAGING_SENDER_ID` | Sender ID do FCM | ❌ | `68150766848` |
| `VITE_FIREBASE_APP_ID` | App ID do Firebase | ❌ | `1:123:web:abc` |
| `VITE_FIREBASE_MEASUREMENT_ID` | ID do Google Analytics (opcional) | ❌ | `G-XXXXXXXX` |
| `VITE_FIREBASE_VAPID_KEY` | Chave VAPID para push web | ❌ | `BNENHg...` |

#### Infra — `infra/.env` (produção)

| Variável | Descrição | Obrigatória | Exemplo |
|---|---|:---:|---|
| `MYSQL_DATABASE` | Nome do banco de dados | ✅ | `whdatabase` |
| `MYSQL_USER` | Usuário do banco | ✅ | `wh_user` |
| `MYSQL_PASSWORD` | Senha do banco | ✅ | `SenhaForte123` |
| `ASPNETCORE_ENVIRONMENT` | Ambiente da API .NET | ✅ | `Production` |
| `ConnectionStrings__mysql` | Connection string para a API no Docker | ✅ | `server=mysql;user=...` |
| `AllowedOrigins__0` | Domínio permitido pelo CORS | ✅ | `https://seudominio.com` |
| `JWT_PRIVATE_KEY` | Chave RSA privada | ✅ | `"-----BEGIN PRIVATE KEY..."` |
| `BrevoSettings__ApiKey` | Chave de API do Brevo (produção) | ✅ | `SUA_CHAVE_AQUI` |
| `BrevoSettings__SenderEmail` | E-mail remetente do sistema | ✅ | `email@email.com.br` |
| `BrevoSettings__SenderName` | Nome remetente do sistema | ✅ | `WorshipHub` |

---

### Configurações por Ambiente

| Configuração | Desenvolvimento | Produção |
|---|---|---|
| Env do .NET | `Development` (ativa Swagger) | `Production` (desativa Swagger) |
| API URL (front) | `http://localhost:5255/api` | `https://seudominio.com/api` |
| Transport JWT | Cookie HttpOnly | Cookie HttpOnly |
| Banco de dados | MySQL local ou Docker isolado | MySQL no container Docker |
| HTTPS | Não obrigatório | Sim (via Nginx + Let's Encrypt) |

---

## Guia de Desenvolvimento

### Workflow de Desenvolvimento

```
main (branch principal, protegida)
  └── feature/nome-da-feature      ← sua branch de desenvolvimento
        └── [commits]
              └── Pull Request para main
                    └── aprovação + merge → dispara CI/CD
```

**Passos típicos:**

```bash
# 1. Atualizar main
git checkout main
git pull origin main

# 2. Criar branch para a nova feature
git checkout -b feature/minha-feature

# 3. Desenvolver e commitar
git add .
git commit -m "feat: descrição da mudança"

# 4. Enviar para o remoto
git push origin feature/minha-feature

# 5. Abrir Pull Request para main no GitHub
```

---

### Criando uma Nova Feature

#### Front-end: nova página

```
src/
├── pages/
│   └── MinhaNovaPage.vue          ← 1. Criar a página
├── components/
│   └── ManageMinhaFeature.vue     ← 2. Se precisar de componente de gerenciamento
```

Registrar a rota em `src/router.js`:
```javascript
{
  path: '/minha-feature',
  name: 'MinhaFeature',
  component: () => import('./pages/MinhaNovaPage.vue'),
  meta: { requiresAuth: true, roles: [Role.Admin] }  // se necessário
}
```

#### Back-end: novo endpoint

```
back-end/
├── WorshipDomain/
│   ├── Entities/MinhaEntidade.cs        ← 1. Entity
│   ├── DTO/MinhaEntidade/               ← 2. DTOs de entrada/saída
│   └── Repository/IMinhaRepository.cs  ← 3. Interface do repositório
├── WorshipInfra/
│   └── Repository/MinhaRepository.cs   ← 4. Implementação (Dapper)
├── WorshipApplication/
│   └── Services/MinhaService.cs        ← 5. Regras de negócio
└── WorshipApi/
    └── Controllers/MinhaController.cs  ← 6. Controller HTTP
```

Registrar no DI em `WorshipInfra/ServiceCollectionExtentions.cs`:
```csharp
services.AddScoped<IMinhaRepository, MinhaRepository>();
```

---

### Convenções de Código

#### Geral

| Aspecto | Convenção |
|---|---|
| **Idioma do código** | Inglês (classes, variáveis, métodos) |
| **Idioma das mensagens** | Português do Brasil (erros, labels, comentários) |
| **Indentação** | 2 espaços (front-end) / 4 espaços (back-end) |

#### Front-end (Vue 3)

| Artefato | Convenção | Exemplo |
|---|---|---|
| Componentes | `PascalCase` | `ManageSchedule.vue` |
| Páginas | `PascalCase` sufixo Page quando ambíguo | `LoginPage.vue` |
| Composables | `camelCase` prefixo `use` | `useAudioMixer.js` |
| Constantes | `PascalCase` no arquivo, `UPPER_SNAKE_CASE` no valor | `Role.Admin` |
| Stores (Pinia) | `camelCase` prefixo `use`, sufixo `Store` | `useAuthStore` |
| Variáveis de ambiente | `VITE_PREFIXO_NOME` | `VITE_API_URL` |

#### Back-end (.NET / C#)

| Artefato | Convenção | Exemplo |
|---|---|---|
| Classes / Interfaces | `PascalCase` | `ScheduleService`, `IScheduleRepository` |
| Métodos públicos | `PascalCase` | `CreateSchedule()` |
| Parâmetros / variáveis | `camelCase` | `scheduleId`, `userId` |
| Propriedades de Entity | `PascalCase` | `EventType`, `Status` |
| Tabelas no banco | `snake_case`, plural | `schedules`, `schedule_users` |
| Colunas no banco | `snake_case` | `fcm_token`, `event_type` |
| DTOs | sufixo `DTO` ou `Dto` | `ScheduleCreationDTO`, `ScheduleRepertoireDto` |

---

### Rodando em Modo Debug

#### Back-end (VS Code):
```json
// .vscode/launch.json (já existente)
{
  "name": "http",
  "type": "coreclr",
  "request": "launch",
  "program": "${workspaceFolder}/back-end/WorshipApi/..."
}
```
Pressione `F5` no VS Code com o projeto `WorshipApi` aberto.

#### Back-end (Visual Studio):
Selecione o perfil `http` no dropdown de perfis e pressione `F5`.

#### Front-end:
O Vite já fornece source maps em modo dev. Use as DevTools do navegador (F12) normalmente. O hot-module replacement (HMR) está ativo.

---

### Ferramentas Recomendadas por Contexto

| Contexto | Ferramenta |
|---|---|
| Edição de Vue/JS | VS Code + extensão Volar |
| Edição de C# | Visual Studio 2022 ou VS Code + C# Dev Kit |
| Teste de APIs | Swagger UI (`/swagger`) ou Postman |
| Inspeção de banco | DBeaver ou MySQL Workbench |
| Push notifications | Firebase Console → Mensagens |
| Logs da API | Console do terminal de `dotnet run` |
| Logs do front | DevTools do navegador (F12 → Console) |
| Gerenciamento de containers | Docker Desktop ou `docker compose logs -f` |
