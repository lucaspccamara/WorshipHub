# ⚡ Quick Start — WorshipHub

> Objetivo: colocar o ambiente funcionando em menos de 10 minutos.

---

## Pré-requisitos

Antes de começar, certifique-se de ter as ferramentas abaixo instaladas:

| Ferramenta | Versão mínima | Link |
|---|---|---|
| Git | qualquer recente | https://git-scm.com |
| Node.js | 18.x | https://nodejs.org |
| .NET SDK | 8.0 | https://dotnet.microsoft.com/download/dotnet/8.0 |
| Docker + Docker Compose | v2.x | https://www.docker.com/products/docker-desktop |
| MySQL | 8.0 (ou via Docker) | https://dev.mysql.com/downloads/mysql/ |

> 💡 **Dica**: Para desenvolvimento local, o MySQL pode ser levantado via Docker sem instalá-lo separadamente.

---

## Passo a Passo de Setup

### 1. Clonar o repositório

```bash
git clone https://github.com/lucaspccamara/WorshipHub.git
cd WorshipHub
```

- [ ] Repositório clonado na sua máquina

---

### 2. Configurar o Back-end

```bash
cd back-end/WorshipApi
```

Edite o arquivo `appsettings.Development.json` com as suas credenciais locais:

```json
{
  "ConnectionStrings": {
    "mysql": "server=127.0.0.1;user=SEU_USUARIO;database=whdatabase;password=SUA_SENHA;"
  },
  "JWT_PRIVATE_KEY": "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----",
  "JWT_PUBLIC_KEY": "-----BEGIN PUBLIC KEY-----\n...\n-----END PUBLIC KEY-----",
  "AllowedOrigins": [
    "http://localhost:5173"
  ]
}
```

> ⚠️ As chaves RSA (`JWT_PRIVATE_KEY` / `JWT_PUBLIC_KEY`) devem ser um par gerado localmente. Veja como gerar no arquivo `04-setup-e-desenvolvimento.md`.

Restaure as dependências e compile:

```bash
dotnet restore
dotnet build
```

- [ ] `appsettings.Development.json` configurado
- [ ] `dotnet restore` executado sem erros
- [ ] `dotnet build` executado sem erros

---

### 3. Configurar o Front-end

```bash
cd front-end
```

Crie ou edite o arquivo `.env.development`:

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

> ⚠️ As variáveis do Firebase são necessárias para as notificações push funcionarem. Sem elas, o app funciona, mas sem notificações.

Instale as dependências:

```bash
npm install
```

- [ ] `.env.development` configurado
- [ ] `npm install` executado sem erros

---

### 4. Subir o banco de dados

**Opção A — MySQL local (já instalado):**
Certifique-se que o serviço MySQL está rodando e o banco `whdatabase` existe.

**Opção B — Docker (recomendado para dev):**

```bash
cd infra
cp .env.example .env
# Edite o .env com suas senhas
docker compose up mysql -d
```

- [ ] Banco de dados MySQL rodando e acessível

---

## 🏃 Rodando a Aplicação

Abra **dois terminais**:

**Terminal 1 — API:**
```bash
cd back-end/WorshipApi
dotnet run --launch-profile http
```
A API estará disponível em: `http://localhost:5255`
Swagger disponível em: `http://localhost:5255/swagger`

**Terminal 2 — Front-end:**
```bash
cd front-end
npm run dev
```
O front-end estará disponível em: `http://localhost:5173`

- [ ] API rodando em `http://localhost:5255`
- [ ] Front-end rodando em `http://localhost:5173`

---

## ✅ Primeiro Teste — Validando o Ambiente

1. Acesse `http://localhost:5173` no navegador
2. A tela de login deve aparecer
3. Faça login com as credenciais cadastradas no banco
4. A tela inicial (Home) deve carregar com as escalas disponíveis

> Se a tela de login travar ou der erro de rede, verifique se a API está rodando e se `VITE_API_URL` aponta para `http://localhost:5255/api`.

- [ ] Login realizado com sucesso
- [ ] Tela inicial carregada sem erros no console do navegador

---

## 🛠️ Modo Docker Completo (Produção Local)

Para subir toda a stack de uma vez (MySQL + API + Nginx + Front-end):

```bash
cd infra
cp .env.example .env
# Edite o .env com todas as variáveis necessárias
docker compose up -d
```

Sistema disponível em: `http://localhost`

---

## 📱 Acessar pelo Celular (Rede Local)

1. Certifique-se que o celular está na mesma rede Wi-Fi do PC
2. No terminal do front-end, localize o endereço **Network** (ex: `http://192.168.1.10:5173`)
3. No `appsettings.Development.json`, adicione o IP da sua máquina em `AllowedOrigins`
4. No `.env.development`, altere `VITE_API_URL` para `http://192.168.1.10:5255/api`
5. Reinicie a API e acesse o endereço Network no celular

---

## 📋 Comandos Essenciais do Dia a Dia

| Contexto | Comando | Descrição |
|---|---|---|
| Front-end | `npm run dev` | Inicia o servidor de desenvolvimento |
| Front-end | `npm run build` | Gera o build de produção |
| Back-end | `dotnet run --launch-profile http` | Inicia a API |
| Back-end | `dotnet build` | Compila o projeto |
| Back-end | `dotnet restore` | Restaura pacotes NuGet |
| Infra | `docker compose up -d` | Sobe todos os serviços |
| Infra | `docker compose down` | Para todos os serviços |
| Infra | `docker compose logs -f` | Acompanha os logs em tempo real |

---

## ✅ Checklist Final de Onboarding

- [ ] Repositório clonado
- [ ] `appsettings.Development.json` configurado com connection string e chaves RSA
- [ ] `firebase-service-account.json` colocado em `back-end/WorshipApi/` (opcional para notificações)
- [ ] `.env.development` do front-end configurado
- [ ] Banco MySQL rodando e acessível
- [ ] API rodando em `http://localhost:5255`
- [ ] Front-end rodando em `http://localhost:5173`
- [ ] Login funcionando com sucesso
- [ ] Leitura de `02-visao-e-arquitetura.md` concluída
