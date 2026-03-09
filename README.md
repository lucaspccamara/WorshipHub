# WorshipHub 🎸

O **WorshipHub** é uma plataforma centralizada para gestão de ministérios de louvor. Projetada para facilitar a organização de escalas, repertórios e comunicação entre membros, a ferramenta automatiza processos como a coleta de disponibilidade e notificações de ensaios.

---

## 🏗️ Arquitetura e Tecnologias

O projeto é dividido em três frentes principais, utilizando práticas modernas de desenvolvimento e arquitetura:

- **Back-end**: Desenvolvido em **ASP.NET Core 8.0** seguindo os princípios de **Clean Architecture**, com separação clara de responsabilidades em camadas (Domain, Application, Infra e API). Utiliza **Dapper** para persistência de dados.
- **Front-end**: SPA moderna construída com **Vue 3**, **Vite** e **Quasar Framework**, garantindo uma interface responsiva, performática e otimizada.
- **Infraestrutura**: Orquestração completa via **Docker Compose**, incluindo banco de dados **MySQL** e servidor **Nginx** configurado como proxy reverso.
- **Funcionalidades Especiais**:
  - Notificações Push via **Firebase Cloud Messaging**.
  - Autenticação Segura via **JWT com chaves RSA**.

---

## 🚀 Pré-requisitos

Para rodar o projeto localmente, você precisará de:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js (v18 ou superior)](https://nodejs.org/)
- [Docker & Docker Compose](https://www.docker.com/products/docker-desktop/)
- [MySQL 8.0](https://dev.mysql.com/downloads/mysql/) (Caso opte por rodar fora do Docker)

---

## 🛠️ Configuração Inicial

### 1. Clonando o Repositório
```bash
git clone https://github.com/lucaspccamara/WorshipHub.git
cd WorshipHub
```

### 2. Back-end (.NET)
Localizado na pasta `/back-end`:

- Nomeie o arquivo `appsettings.json` na pasta `WorshipApi` e configure as credenciais:
  - **ConnectionStrings**: String de conexão com o banco MySQL.
  - **JWT_PRIVATE_KEY**: Chave RSA privada para assinatura de tokens.

```bash
cd back-end
dotnet restore
dotnet build
```

### 3. Front-end (Vue/Vite)
Localizado na pasta `/front-end`:

- Crie um arquivo `.env.development` baseado no `.env.example` e preencha as chaves:
  - **VITE_FIREBASE_***: Configurações do seu projeto Firebase.
  - **VITE_API_BASE_URL**: URL da API (geralmente `http://localhost:8080/api/`).

```bash
cd front-end
npm install
```

### 4. Infraestrutura (Docker)
Localizada na pasta `/infra`:

- Configure o arquivo `.env` baseado no `.env.example`.

---

## 🏃 Como Rodar

### Modo Docker (Recomendado)
A maneira mais rápida de subir o ambiente completo (Banco + API + Nginx/Front):

```bash
cd infra
docker-compose up -d
```
O sistema estará disponível em `http://localhost`.

### Modo Desenvolvimento Local

**API:**
```bash
cd back-end/WorshipApi
dotnet run
```

**Front-end:**
```bash
cd front-end
npm run dev
```

---

## 📁 Estrutura do Projeto

```text
├── back-end/
│   ├── WorshipApi/          # Controllers, Middlewares e Configuração da API
│   ├── WorshipApplication/  # Serviços e Regras de Negócio
│   ├── WorshipDomain/       # Entidades, DTOs e Interfaces
│   └── WorshipInfra/        # Implementação de Repositórios e Banco de Dados
├── front-end/
│   ├── public/              # Ativos estáticos
│   └── src/
│       ├── components/      # Componentes Vue reutilizáveis
│       ├── pages/           # Páginas principais da aplicação
│       ├── stores/          # Gerenciamento de estado (Pinia)
│       └── api.js           # Configuração do Axios
└── infra/
    ├── mysql/               # Configuração do banco de dados
    ├── nginx/               # Configuração do Proxy Reverso
    └── docker-compose.yml   # Orquestração de containers
```

---

## 🔑 Segurança e Autenticação

O projeto utiliza um fluxo de autenticação via **JWT assinado com RSA**. O token é armazenado no cliente através de **Cookies HttpOnly**, protegendo contra ataques XSS. Além disso, as rotas da API possuem **Rate Limiting** para mitigar tentativas de força bruta.

---

## 📄 Licença

Este projeto está sob a licença [MIT](LICENSE).
