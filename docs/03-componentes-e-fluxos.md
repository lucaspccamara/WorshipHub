# 🧩 Componentes, Módulos e Fluxos Principais — WorshipHub

---

## Componentes e Módulos

O sistema é organizado em **módulos funcionais** que correspondem às principais áreas de negócio. Cada módulo tem seus componentes no front-end (Vue) e seus equivalentes no back-end (Service + Repository + Controller).

---

### Módulo: Autenticação

**Localização back-end**: `WorshipApi/Controllers/AuthController.cs` · `WorshipApplication/Services/AuthService.cs`
**Localização front-end**: `src/pages/LoginPage.vue`, `RequestPasswordResetCode.vue`, `VerifyResetCode.vue`, `ResetPassword.vue` · `src/stores/authStore.js`

**Responsabilidade**: Autenticação de usuários via e-mail/senha, emissão de JWT (RSA), transporte via Cookie HttpOnly, recuperação de senha e validação de sessão ativa.

**Fluxo interno**:
```
LoginPage → POST /auths/login → AuthService.AuthenticateUser
  → BCrypt.Verify(senha) → GenerateJWT (RSA RS256, 6h) → Cookie HttpOnly → OK
```

**APIs expostas**:
| Método | Rota | Descrição |
|---|---|---|
| POST | `/auths/login` | Login com e-mail/senha |
| GET | `/auths/me` | Retorna dados do usuário autenticado |
| POST | `/auths/logout` | Remove o cookie de sessão |
| POST | `/auths/change-password` | Troca senha (autenticado) |
| POST | `/auths/request-password-reset-code` | Solicita código de recuperação |
| POST | `/auths/verify-reset-code` | Valida o código recebido |
| POST | `/auths/reset-password` | Redefine a senha com token |

---

### Módulo: Gestão de Escalas

**Localização back-end**: `WorshipApi/Controllers/ScheduleController.cs` · `WorshipApplication/Services/ScheduleService.cs` · `WorshipInfra/Repository/ScheduleRepository.cs`
**Localização front-end**: `src/pages/Schedule.vue` · `src/components/CreateSchedule.vue`, `ManageSchedule.vue`, `ManageSchedulePosition.vue`, `ManageScheduleRepertoire.vue`, `ManageCompletedSchedule.vue`

**Responsabilidade**: Ciclo de vida completo das escalas — criação, transição de status, atribuição de posições, definição de repertório e notificações automáticas em cada transição.

**Ciclo de status**:
```
Criado (0)
  └─▶ Coletando Disponibilidade (1) → notifica TODOS os membros
        └─▶ Aguardando Repertório (2) → notifica Ministros da escala
              └─▶ Concluído (3) → notifica membros (exceto Ministro)
                    └─▶ Excluído (10)
```

**APIs expostas**:
| Método | Rota | Descrição |
|---|---|---|
| GET | `/schedules` | Lista paginada de escalas (com filtros) |
| POST | `/schedules` | Cria uma ou mais escalas |
| PUT | `/schedules/{id}` | Atualiza dados de uma escala |
| PATCH | `/schedules/transition` | Transiciona múltiplas escalas de status |
| GET | `/schedules/{id}/repertoire` | Retorna repertório de uma escala |
| POST | `/schedules/{id}/repertoire` | Salva o repertório de uma escala |
| GET | `/schedules/assignments` | Retorna posições atribuídas |
| POST | `/schedules/{id}/assignments` | Salva atribuições de posições |

---

### Módulo: Disponibilidade

**Localização back-end**: `WorshipInfra/Repository/ScheduleAvailabilitiesRepository.cs` (via ScheduleService)
**Localização front-end**: `src/pages/Availability.vue`

**Responsabilidade**: Registrar e gerenciar respostas de disponibilidade dos membros para cada escala. Um registro de disponibilidade é criado automaticamente para todos os usuários quando a escala entra no status `ColetandoDisponibilidade`.

**Fluxo interno**:
```
Escala → ColetandoDisponibilidade
  → Cria ScheduleAvailabilities para cada usuário ativo
  → Notificação push enviada a todos
  → Membro abre Availability.vue → responde sim/não
  → PUT /schedules/availabilities/{id}
```

**APIs expostas**:
| Método | Rota | Descrição |
|---|---|---|
| GET | `/schedules/availabilities/pending` | Lista disponibilidades pendentes do usuário |
| PUT | `/schedules/availabilities/{id}` | Responde disponibilidade (sim/não) |

---

### Módulo: Músicas e Mixer

**Localização back-end**: `WorshipApi/Controllers/MusicController.cs` · `WorshipApplication/Services/MusicService.cs` · `WorshipInfra/Repository/MusicRepository.cs`
**Localização front-end**: `src/pages/Musics.vue` · `src/components/ManageMusic.vue`, `MusicOverview.vue`, `MixerVS.vue`, `MixerSplash.vue`, `MiniPlayer.vue`, `MixerRuler.vue`, `MeterCanvas.vue` · `src/composables/useAudioMixer.js` · `src/workers/`

**Responsabilidade**: Cadastro e gestão do catálogo de músicas com metadados (tom, BPM, cifra, referência). O mixer de áudio é um player multi-track integrado ao front-end que permite ao músico ouvir faixas separadas (base, vocal, instrumento, etc.) antes do culto.

**Fluxo interno do Mixer**:
```
MusicOverview → abre MixerVS (ou MixerSplash)
  → useAudioMixer composable carrega URLs das faixas
  → AudioWorklet / Web Worker processa áudio
  → MeterCanvas renderiza VU meter (canvas)
  → MixerRuler exibe a escala de db's da faixa
  → MiniPlayer controla play/pause/tempo
```

**APIs expostas**:
| Método | Rota | Descrição |
|---|---|---|
| GET | `/musics` | Lista músicas (com filtro/paginação) |
| POST | `/musics` | Cadastra nova música |
| PUT | `/musics/{id}` | Atualiza música |
| DELETE | `/musics/{id}` | Remove música |

---

### Módulo: Usuários

**Localização back-end**: `WorshipApi/Controllers/UserController.cs` · `WorshipApplication/Services/UserService.cs` · `WorshipInfra/Repository/UserRepository.cs`
**Localização front-end**: `src/pages/Users.vue` · `src/components/CreateUser.vue`, `Profile.vue`, `ChangePassword.vue`

**Responsabilidade**: Cadastro, edição e controle de acesso de membros do ministério. Cada usuário tem um papel (`Role`) e uma posição (`Position`) que determinam o que vê e o que pode fazer no sistema.

**APIs expostas**:
| Método | Rota | Descrição |
|---|---|---|
| GET | `/users` | Lista usuários |
| POST | `/users` | Cria novo usuário |
| PUT | `/users/{id}` | Atualiza usuário |
| PATCH | `/users/{id}/timezone` | Atualiza fuso horário do usuário |
| PUT | `/users/{id}/fcm-token` | Atualiza token de notificação push |

---

### Módulo: Notificações

**Localização back-end**: `WorshipApi/Controllers/NotificationController.cs` · `WorshipApplication/Services/FcmNotificationService.cs`
**Localização front-end**: `src/firebase.js`, `src/firebase-config.js`, `src/firebase-messaging-sw.js`

**Responsabilidade**: Envio de notificações push via Firebase Cloud Messaging (FCM). O front-end registra o FCM Token do dispositivo e o envia para a API ao fazer login. O back-end usa o token para enviar notificações em pontos específicos do fluxo de negócio.

---

### Módulo: Lembretes Automáticos

**Localização back-end**: `WorshipApplication/Workers/EventReminderWorker.cs`

**Responsabilidade**: Serviço em segundo plano (`BackgroundService`) que roda a cada 1 hora. Ele identifica escalas concluídas que ocorrerão em exatamente 2 dias e envia lembretes via Push para os membros escalados, respeitando o fuso horário local de cada um (o disparo ocorre a partir das 12h do horário local do usuário).

---

## Fluxos Principais

### FLUXO 1 — Login e Autenticação

**Atores**: Qualquer usuário  
**Entrada**: e-mail + senha  
**Saída**: Cookie HttpOnly com JWT válido por 6 horas

```
Usuário                  Front-end                       API
   │                         │                             │
   │── insere credenciais ──▶│                            │
   │                         │── POST /auths/login ──────▶│
   │                         │                             │
   │                         │                   BCrypt.Verify()
   │                         │                   GenerateJWT (RSA, 6h)
   │                         │                             │
   │                         │◀── 200 OK + Set-Cookie ─────│
   │                         │    (access_token httpOnly)  │
   │◀── redireciona para / ─│                              │
```

> ⚠️ **Regra de negócio**: O token JWT é transportado SOMENTE via Cookie HttpOnly. O front-end nunca acessa o token diretamente (proteção contra XSS). O Axios envia o cookie automaticamente via `withCredentials: true`.

---

### FLUXO 2 — Ciclo de Vida da Escala

**Atores**: Admin / Leader (cria e gerencia), Member / Minister (responde e consulta)

```
Leader/Admin               Front-end                    API
   │                          │                           │
   │── cria escala ──────────▶│── POST /schedules ───────▶│ Status: Criado
   │                          │                           │
   │── publica disponib. ────▶│── PATCH /transition {1} ─▶│ Status: ColetandoDisponibilidade
   │                          │                           │── cria ScheduleAvailabilities p/ todos
   │                          │                           │── FCM push → todos membros
   │                          │                           │
   [Membros respondem disponibilidade via /availabilities]
   │                          │                           │
   │── define repertório ────▶│── PATCH /transition {2} ─▶│ Status: AguardandoRepertorio
   │                          │                           │── FCM push → Ministros da escala
   │                          │                           │
   [Ministro define músicas via /schedules/{id}/repertoire]
   │                          │                           │
   │── conclui escala ───────▶│── PATCH /transition {3} ─▶│ Status: Concluido
   │                          │                           │── FCM push → membros (exceto Ministro)
```

---

### FLUXO 3 — Coleta de Disponibilidade

**Atores**: Member / Minister  
**Pré-condição**: Escala no status `ColetandoDisponibilidade`

```
1. Membro recebe push notification: "Novas escalas disponíveis!"
2. Abre o app → navega para /availabilities
3. Sistema lista as escalas com resposta pendente
4. Membro responde "Disponível" ou "Indisponível" para cada uma
5. PUT /schedules/availabilities/{id} → { available: true/false }
6. Registro atualizado no banco
```

> ⚠️ **Regra**: Não é possível alterar a resposta se a escala não está mais em `ColetandoDisponibilidade`.

---

### FLUXO 4 — Definição de Repertório e Posições

**Atores**: Leader / Admin / Minister  
**Pré-condição**: Escala no status `AguardandoRepertorio`

```
1. Leader acessa a escala → ManageSchedulePosition.vue
2. Atribui músicos às posições (instrumento/função por slot)
3. POST /schedules/{id}/assignments → salva atribuições
4. Leader define as músicas do culto → ManageScheduleRepertoire.vue
5. POST /schedules/{id}/repertoire → salva setlist
6. Leader conclui → PATCH /transition → status Concluido
7. FCM push enviado a todos os membros da escala (exceto Ministro)
```

---

### FLUXO 5 — Estudo de Música com o Mixer

**Atores**: Qualquer membro autenticado  
**Pré-condição**: Músicas cadastradas com faixas de áudio

```
1. Membro acessa /musics ou o repertório de uma escala
2. Abre MusicOverview → visualiza tom, BPM, cifra e link de referência
3. Abre o Mixer (MixerVS / MixerSplash)
4. useAudioMixer carrega as faixas de áudio via URL
5. AudioWorklet (Web Worker) processa o áudio em thread separada
6. MeterCanvas exibe nível de volume em tempo real (canvas 2D)
7. Membro ajusta volume de cada faixa individualmente
8. MiniPlayer controla play/pause/seek/BPM
```

---

### FLUXO 6 — Recuperação de Senha

**Atores**: Qualquer usuário  
**Entrada**: e-mail cadastrado

```
1. Usuário acessa `/request-password-reset-code` → informa e-mail
2. API gera código 6 dígitos + JWT temporário (10 min) → salva no usuário
3. API envia e-mail real via **Brevo API v3** com o código e instruções
4. Usuário acessa `/verify-reset-code` → informa e-mail + código
5. API valida JWT e código → emite token de reset (15 min)
6. Usuário acessa `/reset-password` → informa nova senha + token
7. API valida token → atualiza senha (BCrypt) → limpa `reset_token`

---

### FLUXO 7 — Lembrete de Escala (2 dias antes)

**Atores**: Sistema (Automático), Member (Recebe)
**Agendamento**: Execução horária

```
1. EventReminderWorker inicia ciclo de verificação (hora em hora)
2. Busca escalas com Status: Concluido para a data: Hoje + 2 dias
3. Para cada escala, lista membros cujas posições foram preenchidas
4. Verifica na tabela schedules_users se reminder_sent == 0
5. Converte UTC atual para o Timezone do usuário (ex: America/Sao_Paulo)
6. Se localHour >= 12:
   a. Envia Push Notification via FCM Service
   b. Marca reminder_sent = 1 para esse usuário específico na escala
```
