# 🚀 Guia de Deployment — WorshipHub

---

## Ambientes Disponíveis

| Ambiente | URL | Descrição |
|---|---|---|
| **Produção** | `https://worshiphub.ipflamengo.com.br` | Ambiente público, deploy via CI/CD |
| **Desenvolvimento local** | `http://localhost:5173` (front) / `http://localhost:5255` (API) | Ambiente local do desenvolvedor |

> ⚠️ `Ambiente de staging/QA não existe atualmente — preencher manualmente se for criado.`

---

## Arquitetura de Deploy (Produção)

```
                       Internet
                           │
                    [ DNS: worshiphub.ipflamengo.com.br ]
                           │
                    ┌──────▼──────┐
                    │    Nginx    │  :80 → redirect HTTPS
                    │   (Alpine)  │  :443 → SSL (Let's Encrypt)
                    └──────┬──────┘
                           │
               ┌───────────┴───────────┐
               │                       │
    [ GET / → /usr/share/nginx/html ]  │ [ /api/* → proxy_pass ]
    (frontend-dist estático)            │
                                ┌──────▼──────┐
                                │ worshiphub- │
                                │    api      │  :8080 (interno)
                                │ (.NET 8,    │
                                │  Alpine)    │
                                └──────┬──────┘
                                       │ Dapper / MySQL
                                ┌──────▼──────┐
                                │  MySQL 8.0  │  :3306 (apenas rede interna)
                                │  (volume    │
                                │  externo)   │
                                └─────────────┘

Todos os containers compartilham a rede: worshiphub-network (bridge interno)
Somente o Nginx expõe portas públicas (80 e 443)
```

---

## Pipeline de CI/CD (GitHub Actions)

**Arquivo**: [`.github/workflows/deploy.yml`](file:///../.github/workflows/deploy.yml)  
**Trigger**: push para a branch `main`

### Etapas do Pipeline

```
push → main
  │
  ├── 1. Checkout do código
  │
  ├── 2. Login no GHCR (GitHub Container Registry)
  │
  ├── 3. Build da imagem Docker da API
  │       docker build -f back-end/WorshipApi/Dockerfile back-end/
  │       tag: ghcr.io/lucaspccamara/worshiphub-api:latest
  │
  ├── 4. Push da imagem para GHCR
  │
  ├── 5. Build do front-end (Node 20)
  │       Injeta variáveis de ambiente via GitHub Secrets
  │       npm ci && npm run build → gera dist/
  │
  ├── 6. Cópia dos arquivos dist/ para a VPS via SCP
  │       Destino: /home/worship/apps/worshiphub/infra/frontend-dist/
  │
  └── 7. Deploy na VPS via SSH
          cd /home/worship/apps/worshiphub/infra
          docker compose pull
          docker compose up -d --force-recreate --remove-orphans
```

**Duração estimada**: 3–6 minutos do push ao ambiente atualizado

---

## Secrets Necessários no GitHub

Configure em: **GitHub → Repository → Settings → Secrets and variables → Actions**

| Secret | Descrição |
|---|---|
| `VPS_HOST` | IP ou domínio do servidor VPS |
| `VPS_USER` | Usuário SSH na VPS |
| `VPS_SSH_KEY` | Chave SSH privada (sem senha) |
| `VITE_API_URL` | URL da API em produção (`https://worshiphub.ipflamengo.com.br/api`) |
| `VITE_FIREBASE_API_KEY` | Firebase API Key |
| `VITE_FIREBASE_AUTH_DOMAIN` | Firebase Auth Domain |
| `VITE_FIREBASE_PROJECT_ID` | Firebase Project ID |
| `VITE_FIREBASE_STORAGE_BUCKET` | Firebase Storage Bucket |
| `VITE_FIREBASE_MESSAGING_SENDER_ID` | Firebase Messaging Sender ID |
| `VITE_FIREBASE_APP_ID` | Firebase App ID |
| `VITE_FIREBASE_MEASUREMENT_ID` | Firebase Measurement ID |
| `VITE_FIREBASE_VAPID_KEY` | Firebase VAPID Key |

---

## Variáveis de Ambiente de Produção

Na VPS, o arquivo `infra/.env` deve conter (sem valores sensíveis aqui):

```env
# Banco de dados
MYSQL_DATABASE=<nome-do-banco>
MYSQL_USER=<usuario-db>
MYSQL_PASSWORD=<senha-forte>

# API .NET
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__mysql=server=mysql;user=<usuario>;database=<banco>;password=<senha>;
AllowedOrigins__0=https://worshiphub.ipflamengo.com.br

# Autenticação JWT
JWT_PRIVATE_KEY="-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----"

# E-mail (Brevo)
BrevoSettings__ApiKey=SUA_CHAVE_AQUI
BrevoSettings__SenderEmail=email@email.com.br
BrevoSettings__SenderName=WorshipHub
```

> ⚠️ O arquivo `infra/.env` **não é versionado** (está no `.gitignore`). Deve ser criado e mantido manualmente na VPS.

---

## Processo de Deploy Manual (sem CI/CD)

Para situações de emergência ou quando o CI/CD não estiver disponível:

### 1. Build e push manual da imagem API

```bash
# No seu computador local:
docker login ghcr.io -u SEU_USUARIO_GITHUB

docker build \
  -t ghcr.io/lucaspccamara/worshiphub-api:latest \
  -f back-end/WorshipApi/Dockerfile \
  back-end/

docker push ghcr.io/lucaspccamara/worshiphub-api:latest
```

### 2. Build manual do front-end

```bash
cd front-end
# Configure o .env.production com as variáveis corretas
npm ci
npm run build
# Copie a pasta dist/ para a VPS:
scp -r dist/* VPS_USER@VPS_HOST:/home/worship/apps/worshiphub/infra/frontend-dist/
```

### 3. Atualizar containers na VPS

```bash
ssh VPS_USER@VPS_HOST
cd /home/worship/apps/worshiphub/infra
docker compose pull
docker compose up -d --force-recreate --remove-orphans
```

---

## Procedimento de Rollback

Em caso de falha após deploy:

### Rollback da API (imagem Docker)

A estratégia atual usa a tag `latest` — não há histórico de versões tagueadas. Para rollback:

```bash
# Na VPS: pare o container da API e suba uma versão anterior
# (se você fizer pull de uma versão tageada anteriormente)
ssh VPS_USER@VPS_HOST
cd /home/worship/apps/worshiphub/infra

# Edite o docker-compose.yml temporariamente para apontar para a tag anterior:
# image: ghcr.io/lucaspccamara/worshiphub-api:v1.2.3

docker compose up -d worshiphub-api
```

> ⚠️ **Ponto de atenção**: O pipeline atual usa apenas a tag `latest`, o que dificulta rollbacks precisos. Recomenda-se adotar tags semânticas (ex: `v1.2.3`) no futuro para facilitar o rollback.

### Rollback do Front-end

O front-end é estático na VPS. Mantenha um backup da pasta `frontend-dist/` antes de cada deploy crítico:

```bash
# Na VPS, antes do deploy:
cp -r /home/worship/apps/worshiphub/infra/frontend-dist \
       /home/worship/apps/worshiphub/infra/frontend-dist.bak

# Em caso de rollback:
rm -rf /home/worship/apps/worshiphub/infra/frontend-dist
mv /home/worship/apps/worshiphub/infra/frontend-dist.bak \
   /home/worship/apps/worshiphub/infra/frontend-dist
docker compose restart nginx
```

---

## SSL / HTTPS (Let's Encrypt)

O certificado SSL é gerenciado via **Certbot** com renovação automática:

```bash
# Na VPS — renovar certificado manualmente se necessário:
sudo certbot renew

# Ou via Docker (certbot deve estar configurado separadamente):
docker run --rm \
  -v /etc/letsencrypt:/etc/letsencrypt \
  -v /var/www/certbot:/var/www/certbot \
  certbot/certbot renew
```

O Nginx está configurado para:
- Redirecionar HTTP → HTTPS automaticamente
- Servir o challenge do Let's Encrypt em `/.well-known/acme-challenge/`

---

## Checklist Pré-Deploy

Antes de fazer merge na `main` (e disparar o CI/CD):

- [ ] O código compila sem erros localmente (`dotnet build` e `npm run build`)
- [ ] O comportamento foi testado manualmente em dev
- [ ] Nenhum dado sensível foi commitado (chaves, senhas, tokens)
- [ ] Variáveis de ambiente estão configuradas nos **GitHub Secrets**
- [ ] O arquivo `firebase-service-account.json` está presente na VPS (se necessário)
- [ ] O `.env` da VPS está atualizado com as variáveis corretas

---

## Monitoramento e Logs

```bash
# Ver logs de todos os containers em tempo real:
ssh VPS_USER@VPS_HOST
cd /home/worship/apps/worshiphub/infra
docker compose logs -f

# Apenas a API:
docker compose logs -f worshiphub-api

# Apenas o Nginx:
docker compose logs -f nginx

# Verificar status dos containers:
docker compose ps
```

> Para monitoramento avançado (alertas, dashboards), ferramentas como **Uptime Kuma** ou **Grafana** podem ser adicionadas futuramente.
