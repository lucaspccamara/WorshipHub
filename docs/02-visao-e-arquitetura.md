# 🏛️ Visão Geral e Arquitetura — WorshipHub

---

## Visão Geral

### O que é o WorshipHub?

O **WorshipHub** é uma plataforma web centralizada para **gestão de ministérios de louvor em igrejas**. Seu objetivo é digitalizar e automatizar os processos operacionais de um time de louvor: criação de escalas, coleta de disponibilidade dos músicos/cantores, definição do repertório e comunicação interna entre os membros.

Antes do WorshipHub, esses processos costumam acontecer de forma fragmentada — via grupos de WhatsApp, planilhas ou conversas individuais. O sistema consolida tudo em um único lugar, acessível tanto pelo computador quanto pelo celular (o app é uma Progressive Web App — PWA).

### Público-alvo

| Perfil | Quem é | O que faz no sistema |
|---|---|---|
| **Admin** | Administrador do ministério | Gerencia usuários, cria escalas, acessa tudo |
| **Leader** | Líder de louvor | Cria e gerencia escalas e repertórios |
| **Minister** | Ministro de louvor (vocal principal) | Gerencia repertórios de escalas que participa |
| **Member** | Músico ou técnico | Informa disponibilidade, consulta repertório |

### Funcionalidades Principais

- **Escalas**: criação, publicação e gestão do ciclo de vida completo de cada culto
- **Coleta de Disponibilidade**: membros respondem se estão disponíveis para cada escala; o sistema agrega as respostas
- **Repertório**: definição das músicas de cada escala, com posições específicas por músico
- **Músicas**: cadastro de músicas com tom, cifra, BPM, faixa de referência e mixer de áudio integrado
- **Mixer de Áudio**: player multi-track para que músicos ouçam as faixas separadas de cada música (base, vocal, instrumento, etc.)
- **Notificações Push**: alertas automáticos via Firebase Cloud Messaging quando a escala avança de status
- **Gestão de Usuários**: cadastro, papéis, posições (instrumento/função) e controle de acesso por role
- **Recuperação de Senha**: fluxo via e-mail (Brevo API) com código de verificação temporário
- **Lembretes Automáticos**: notificações push enviadas aos membros escalados exatamente 2 dias antes do evento, às 12:00h do horário local do usuário

### Principais Casos de Uso

1. **Líder cria uma escala** → Define a data e tipo de evento (culto noturno, escola dominical, evento especial)
2. **Escala entra em coleta de disponibilidade** → Notificação push enviada aos membros; cada um confirma ou declina
3. **Líder define o repertório** → Com base nos disponíveis, seleciona músicos e adiciona músicas ao setlist
4. **Membros acessam o repertório** → Estudam as músicas usando o player/mixer integrado antes do culto
5. **Escala é concluída** → Status muda para Concluído; histórico fica disponível para consulta

---

## Conceitos de Domínio (Glossário Rápido)

| Termo | Definição |
|---|---|
| **Escala** (`Schedule`) | Representa um culto/evento específico. Tem data, tipo e um ciclo de status |
| **Status da Escala** | Enum com 5 estados: `Criado → Coletando Disponibilidade → Aguardando Repertório → Concluído → Excluído` |
| **Tipo de Evento** (`EventType`) | Categoria do culto: `EveningWorship`, `SundaySchool`, `SpecialEvent` |
| **Disponibilidade** (`ScheduleAvailabilities`) | Resposta de um membro se pode participar de uma escala |
| **Posição** (`Position`) | Instrumento ou função do membro: Ministro, Vocal, Baterista, Guitarrista, Teclado, Câmera, Letras, etc. |
| **Música** (`Music`) | Entidade com tom, BPM, cifra, link de referência, e múltiplas faixas de áudio para o mixer |
| **Mixer** | Player multi-track integrado ao front-end para estudo individual das faixas de cada música |
| **FCM Token** | Token de dispositivo armazenado no perfil do usuário, usado para enviar notificações push |
| **Fuso Horário** (`Timezone`) | Identificador IANA (ex: `America/Sao_Paulo`) usado para disparar lembretes no horário local correto |
| **Role** | Papel do usuário no sistema: Admin, Leader, Minister, Member |

---

## Arquitetura

### Padrão Arquitetural

O projeto adota **dois padrões arquiteturais complementares**:

- **Back-end**: **Clean Architecture** — separação em camadas bem definidas (Domain, Application, Infra, API), onde as dependências apontam sempre de fora para dentro. A camada Domain não depende de nenhum framework ou banco de dados.
- **Front-end**: **SPA com composição por componentes** (Vue 3 Composition API), com roteamento client-side (Vue Router) e estado global via Pinia.

A comunicação entre front-end e back-end é feita via **REST API HTTP**, com JWT transportado em **Cookie HttpOnly** (seguro contra XSS).

---

### Visão Geral das Camadas

```
┌───────────────────────────────────────────────────┐
│                   FRONT-END                       │
│     Vue 3 + Quasar + Vite + Pinia (PWA)           │
│                                                   │
│  pages/ ─── components/ ─── composables/          │
│  stores/ ─── router/ ─── api.js (Axios)           │
└────────────────────┬──────────────────────────────┘
                     │ HTTP REST (Cookie HttpOnly JWT)
                     ▼
┌───────────────────────────────────────────────────┐
│                BACK-END (Clean Arch)              │
│                                                   │
│  ┌─────────────────────────────────────────────┐  │
│  │  WorshipApi (API Layer)                     │  │
│  │  Controllers / Middlewares / Program.cs     │  │
│  └──────────────────┬──────────────────────────┘  │
│                     │                             │
│  ┌──────────────────▼──────────────────────────┐  │
│  │  WorshipApplication (Application Layer)     │  │
│  │  Services / Business Rules                  │  │
│  └──────────────────┬──────────────────────────┘  │
│                     │                             │
│  ┌──────────────────▼──────────────────────────┐  │
│  │  WorshipDomain (Domain Layer)               │  │
│  │  Entities / Enums / Interfaces / DTOs       │  │
│  │  (IEmailService, BrevoSettings)             │  │
│  └──────────────────┬──────────────────────────┘  │
│                     │                             │
│  ┌──────────────────▼──────────────────────────┐  │
│  │  WorshipInfra (Infrastructure Layer)        │  │
│  │  Repositories / Database / Dapper           │  │
│  └──────────────────┬──────────────────────────┘  │
│                     │                             │
└─────────────────────┼─────────────────────────────┘
                      │
         ┌────────────▼────────────┐
         │       MySQL 8.0         │
         └─────────────────────────┘
```

---

### Descrição das Camadas

#### 🔵 WorshipApi (API Layer)
- **Responsabilidade**: Ponto de entrada HTTP. Recebe requisições, autentica (JWT), autoriza (Role-based) e delega para a Application.
- **Componentes**: `Controllers/` (AuthController, ScheduleController, MusicController, UserController, NotificationController), `Program.cs` (configuração do DI, CORS, rate limiting, JWT)
- **Dependências externas**: Firebase Admin SDK (FCM), Brevo Mail API
- **Não contém** lógica de negócio nem acesso a banco

#### 🟢 WorshipApplication (Application Layer)
- **Responsabilidade**: Orquestra os casos de uso. Contém as regras de negócio da aplicação (quando enviar notificação, validar transições de status, etc.)
- **Componentes**: `Services/` (AuthService, ScheduleService, MusicService, UserService, FcmNotificationService, BrevoEmailService), `Workers/` (EventReminderWorker)
- **Depende de**: WorshipDomain (interfaces e entidades)
- **Não depende de**: banco de dados diretamente (usa interfaces do Domain)

#### 🟡 WorshipDomain (Domain Layer)
- **Responsabilidade**: Núcleo do sistema. Define as entidades, enums, DTOs e interfaces de repositório.
- **Componentes**: `Entities/` (User, Schedule, Music, ScheduleUser, ScheduleMusic, ScheduleAvailabilities), `Enums/` (Role, ScheduleStatus, EventType, Position), `DTO/`, `Repository/` (interfaces)
- **Não depende de** nada externo — é o centro da Clean Architecture

#### 🔴 WorshipInfra (Infrastructure Layer)
- **Responsabilidade**: Implementa as interfaces do Domain. Acessa o banco MySQL usando **Dapper** (micro-ORM leve, queries SQL puras).
- **Componentes**: `Repository/` (implementações concretas), `Database/` (configuração de conexão)
- **Depende de**: WorshipDomain

---

### Estrutura de Pastas

```
WorshipHub/
├── back-end/
│   ├── WorshipApi/           # Camada de API (Controllers, Program.cs, Dockerfiles)
│   ├── WorshipApplication/   # Camada de Aplicação (Services)
│   ├── WorshipDomain/        # Camada de Domínio (Entities, Enums, DTOs, Interfaces)
│   └── WorshipInfra/         # Camada de Infraestrutura (Repositórios, Dapper, MySQL)
├── front-end/
│   ├── public/               # Ativos estáticos (ícones PWA, arquivos públicos)
│   └── src/
│       ├── components/       # Componentes Vue reutilizáveis e de página
│       ├── composables/      # Lógica reutilizável (ex: useAudioMixer)
│       ├── constants/        # Enums/constantes do front (Role, ScheduleStatus, Position, etc.)
│       ├── entities/         # Modelos de dados do front-end
│       ├── pages/            # Páginas principais mapeadas às rotas
│       ├── stores/           # Estado global com Pinia (authStore)
│       ├── workers/          # Web Workers (ex: AudioWorklet para o mixer)
│       ├── router.js         # Definição de rotas e guards de autenticação
│       ├── api.js            # Instância Axios com baseURL e interceptors
│       ├── firebase.js       # Inicialização Firebase
│       └── main.js           # Ponto de entrada da aplicação Vue
├── infra/
│   ├── mysql/                # Configuração personalizada do MySQL (my.cnf)
│   ├── nginx/                # Configuração do proxy reverso Nginx
│   ├── docker-compose.yml    # Orquestração dos serviços
│   └── .env.example          # Template de variáveis de ambiente de produção
└── .github/
    └── workflows/
        └── deploy.yml        # Pipeline CI/CD (GitHub Actions)
```

---

### Dependências Externas e Infraestrutura

| Serviço | Tecnologia | Função |
|---|---|---|
| Banco de dados | MySQL 8.0 | Persistência de todos os dados do sistema |
| Proxy reverso | Nginx (Alpine) | Serve o front-end estático e roteia para a API |
| Notificações Push | Firebase Cloud Messaging (FCM) | Envio de push notifications para os membros |
| Envio de E-mail | Brevo (API v3) | Recuperação de senha e e-mails transacionais |
| Autenticação | JWT com chaves RSA assimétricas | Token armazenado em Cookie HttpOnly |
| CI/CD | GitHub Actions | Build, push de imagem Docker e deploy na VPS |
| Hospedagem | VPS (servidor próprio) + Docker | Containers MySQL, API e Nginx |
| PWA | vite-plugin-pwa | Permite instalação do app em dispositivos móveis |

---

### Diagrama de Implantação (Produção)

```
[ GitHub Actions ]
       │
       │ push para main
       ▼
┌──────────────────────────────────────────────┐
│               VPS Linux                      │
│                                              │
│  ┌──────────┐   ┌──────────┐  ┌──────────┐   │
│  │  Nginx   │   │WorshipApi│  │  MySQL   │   │
│  │ :80/:443 │─▶│  (API)   │─▶│  :3306   │   │
│  │  +dist/  │   │  :8080   │  │          │   │
│  └──────────┘   └──────────┘  └──────────┘   │
│                      │                       │
└──────────────────────┼───────────────────────┘
                       │
               [ Firebase FCM ]
               (push notifications)
```

> Os containers se comunicam pela rede interna Docker `worshiphub-network`. Somente o Nginx expõe as portas 80 e 443 publicamente.
