# 🎸 WorshipHub — Documentação de Onboarding

> **Plataforma centralizada para gestão de ministérios de louvor** — escalas, repertórios, disponibilidade e comunicação em um só lugar.

---

## 🚀 Começar Agora

Novo no projeto? Comece aqui:

**[→ Quick Start: ambiente rodando em 10 minutos](01-quick-start.md)**

---

## 📚 Índice Completo

### Fluxo de leitura recomendado

```
[01-quick-start]
       │
       ▼
[02-visao-e-arquitetura]
       │
       ▼
[03-componentes-e-fluxos]
       │
       ├──▶ [04-setup-e-desenvolvimento]  ← leitura paralela durante implementação
       │
       ├──▶ [05-padroes-e-testes]         ← antes de abrir o primeiro PR
       │
       ├──▶ [06-faq-e-glossario]          ← consulta contínua
       │
       └──▶ [07-deployment]               ← quando for fazer deploy
```

---

### 📋 Tabela de Documentos

| # | Arquivo | Categoria | Tópicos Cobertos |
|---|---|---|---|
| 01 | [Quick Start](01-quick-start.md) | 🟢 Início Rápido | Pré-requisitos, instalação, primeiro teste, comandos do dia a dia |
| 02 | [Visão e Arquitetura](02-visao-e-arquitetura.md) | 📖 Fundamentos | O que é o sistema, público-alvo, casos de uso, Clean Arch, diagrama de camadas, deploy |
| 03 | [Componentes e Fluxos](03-componentes-e-fluxos.md) | 📖 Fundamentos | Módulos do sistema, APIs expostas, fluxos de negócio com diagramas de sequência |
| 04 | [Setup e Desenvolvimento](04-setup-e-desenvolvimento.md) | 🛠️ Desenvolvimento | Instalação detalhada, todas as env vars, workflow de branches, criar nova feature |
| 05 | [Padrões e Testes](05-padroes-e-testes.md) | 🛠️ Desenvolvimento | Convenções de nomes, commits, PR, estrutura de pastas, guia de testes |
| 06 | [FAQ e Glossário](06-faq-e-glossario.md) | 📖 Referência | 12 problemas comuns resolvidos, glossário de domínio, termos técnicos, siglas |
| 07 | [Deployment](07-deployment.md) | 🚢 Avançado | Arquitetura de produção, CI/CD pipeline, rollback, SSL, checklist pré-deploy |

---

## 🗺️ Roteiro de Onboarding

### 🟢 Início Rápido (Dia 1)

- [ ] Ler [01-quick-start.md](01-quick-start.md) e subir o ambiente local
- [ ] Ler [02-visao-e-arquitetura.md](02-visao-e-arquitetura.md) para entender o sistema
- [ ] Ler [03-componentes-e-fluxos.md](03-componentes-e-fluxos.md) para mapear os módulos

### 🛠️ Desenvolvimento (Dias 2–3)

- [ ] Ler [04-setup-e-desenvolvimento.md](04-setup-e-desenvolvimento.md) para configuração completa
- [ ] Ler [05-padroes-e-testes.md](05-padroes-e-testes.md) antes de escrever código
- [ ] Manter [06-faq-e-glossario.md](06-faq-e-glossario.md) aberto como referência rápida

### 🚢 Deploy / Produção (quando necessário)

- [ ] Ler [07-deployment.md](07-deployment.md) antes de qualquer deploy
- [ ] Verificar o checklist pré-deploy
- [ ] Confirmar que os secrets do GitHub Actions estão atualizados

---

## 🔗 Links Úteis

| Item | Valor |
|---|---|
| **Repositório** | https://github.com/lucaspccamara/WorshipHub |
| **Branch principal** | `main` |
| **Ambiente de produção** | https://worshiphub.ipflamengo.com.br |
| **Swagger (dev)** | http://localhost:5255/swagger |
| **Firebase Console** | https://console.firebase.google.com/project/worshiphub-fcm |

---

## ⚙️ Stack Resumida

| Camada | Tecnologia |
|---|---|
| **Front-end** | Vue 3 · Quasar Framework · Vite · Pinia · PWA |
| **Back-end** | ASP.NET Core 8.0 · Clean Architecture · Dapper |
| **Banco de dados** | MySQL 8.0 |
| **Autenticação** | JWT com RSA · Cookie HttpOnly |
| **Notificações** | Firebase Cloud Messaging (FCM) |
| **Infraestrutura** | Docker Compose · Nginx · VPS Linux |
| **CI/CD** | GitHub Actions → GHCR → VPS via SSH |
| **SSL** | Let's Encrypt (certbot) |
