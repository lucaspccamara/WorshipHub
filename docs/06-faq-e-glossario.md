# ❓ FAQ, Troubleshooting e Glossário — WorshipHub

---

## FAQ e Troubleshooting

### Problemas de Setup e Ambiente

---

**1. A API retorna erro de CORS ao acessar pelo front-end. O que fazer?**

Verifique se a origem do front-end está na lista `AllowedOrigins` do `appsettings.Development.json`:
```json
"AllowedOrigins": [
  "http://localhost:5173",
  "http://192.168.1.10:5173"  ← adicione seu IP local se estiver testando no celular
]
```
Reinicie a API após qualquer alteração no `appsettings`.

---

**2. A API inicia mas retorna 401 em todas as requisições, mesmo após login.**

Causas mais comuns:
- As chaves RSA (`JWT_PRIVATE_KEY` / `JWT_PUBLIC_KEY`) estão incorretas ou não formam um par válido
- O cookie `access_token` não está sendo enviado (verifique se `withCredentials: true` está no Axios e se o CORS está com `AllowCredentials()`)
- O token expirou (duração: 6 horas)

Verifique os logs no console da API — o evento `OnAuthenticationFailed` imprime o motivo exato.

---

**3. Como gero um par de chaves RSA para desenvolvimento?**

```bash
# Linux/macOS/WSL ou Git Bash no Windows:
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out private.pem
openssl rsa -pubout -in private.pem -out public.pem
```

Copie o conteúdo de cada arquivo para `appsettings.Development.json`, convertendo quebras de linha em `\n`:
```bash
# Visualizar em formato de linha única (útil para copiar):
awk '{printf "%s\\n", $0}' private.pem
```

> ⚠️ Nunca commite chaves RSA reais no repositório. Use `.gitignore` para proteger o `appsettings.Development.json` com chaves reais.

---

**4. As notificações push não estão chegando no dispositivo.**

Verifique na seguinte ordem:
1. O `firebase-service-account.json` existe em `back-end/WorshipApi/`?
2. As variáveis `VITE_FIREBASE_*` estão configuradas no `.env.development`?
3. O usuário autorizou notificações no navegador (o prompt deve aparecer)?
4. O FCM Token foi salvo no banco? Cheque via `PUT /users/{id}/fcm-token` após login
5. No console do back-end, procure por erros da `FcmNotificationService`

Em ambiente de desenvolvimento, notificações push podem não funcionar se o site não estiver sob HTTPS (exceto em `localhost`).

---

**5. O front-end compila mas aparece tela branca ao abrir no navegador.**

Verifique o console do navegador (F12). Causa mais comum: a variável `VITE_API_URL` está apontando para um endereço incorreto ou a API não está rodando.

```bash
# Confirme que a API está acessível:
curl http://localhost:5255/swagger
```

---

**6. `dotnet run` falha com erro de connection string.**

Certifique-se de que:
- O MySQL está rodando (local ou Docker)
- O banco `whdatabase` existe
- As credenciais no `appsettings.Development.json` correspondem às do MySQL local

Teste a conexão diretamente:
```bash
mysql -h 127.0.0.1 -u SEU_USUARIO -p whdatabase
```

---

**7. O mixer de áudio não carrega as faixas ou aparece travado.**

Causas comuns:
- As URLs das faixas de áudio não são acessíveis (verifique se o servidor de arquivos está up)
- O navegador bloqueou o `AudioContext` por falta de interação do usuário (exige um clique antes de iniciar o áudio — comportamento padrão dos browsers)
- O `AudioWorklet` não suporta módulos em modo `dev` com certos browsers — teste em Chrome/Edge atualizados

---

**8. Ao acessar pelo celular na rede local, o login funciona mas as notificações não.**

Push notifications via FCM exigem HTTPS em dispositivos reais. Em rede local (HTTP), o Service Worker responsável pelas notificações pode não registrar. Para testar notificações no celular, use o ambiente de produção ou configure um túnel HTTPS local (ex: `ngrok`).

---

**9. O deploy falha no GitHub Actions com erro de SSH.**

Verifique se os secrets estão configurados corretamente no repositório do GitHub:
- `VPS_HOST` — IP ou domínio do servidor
- `VPS_USER` — usuário SSH
- `VPS_SSH_KEY` — chave SSH **privada** (sem senha)

Na VPS, confirme que a chave pública correspondente está em `~/.ssh/authorized_keys` do usuário.

---

**10. O código de recuperação de senha não chega por e-mail.**

Verifique se:
1. A **ApiKey** do Brevo está configurada corretamente no `appsettings.json`
2. O e-mail remetente em `BrevoSettings.SenderEmail` é um remetente autenticado no painel do Brevo
3. O domínio `ipfalamengo.com.br` está com os registros DKIM/SPF validados no Brevo

Durante o desenvolvimento, se a `ApiKey` estiver em branco, o código continuará sendo exibido apenas no console da API para facilitar o teste local sem gastar cota de e-mail.

---

**11. Como adicionar um novo usuário ao sistema?**

Apenas usuários com `Role = Admin` ou `Role = Leader` podem criar novos usuários via interface (tela `/users`). Não há auto-cadastro — todos os usuários são criados por um administrador.

---

**12. A aplicação está lenta no celular / o mixer trava.**

- Certifique-se que o `AudioWorklet` está sendo utilizado (procure por logs no console indicando fallback para `ScriptProcessor`)
- O mixer usa canvas 2D para o VU meter — em dispositivos com GPU fraca, considere reduzir a frequência de atualização
- Verifique se há muitos rerenders Vue causados por reatividade desnecessária nos composables do mixer (`useAudioMixer.js`)

---

## Glossário

### Termos de Domínio de Negócio

| Termo | Definição | Contexto de uso |
|---|---|---|
| **Escala** | Um evento de culto com data, tipo e conjunto de membros designados | Entidade central do sistema; toda organização gira em torno dela |
| **Ministério de Louvor** | O grupo responsável pela música/louvor em uma igreja | Contexto de negócio do WorshipHub |
| **Ministro** | Vocal principal / líder de louvor dentro de uma escala | `Position.Minister` no back-end; recebe notificação de "Aguardando Repertório" |
| **Repertório** | Conjunto de músicas definidas para uma escala | Gerenciado em `ManageScheduleRepertoire.vue` e salvo via `/schedules/{id}/repertoire` |
| **Disponibilidade** | Resposta de um membro (sim/não) para participar de uma escala | Status coletado durante a fase `ColetandoDisponibilidade` |
| **Posição** | Instrumento ou função técnica de um membro em uma escala | Ex: Baterista, Guitarrista, Câmera, Letras |
| **Escala Concluída** | Status final da escala, quando todo o planejamento está fechado | Notifica membros e libera o repertório para todos |
| **Mixer** | Player multi-track integrado para estudo das músicas por instrumento | Funcionalidade do módulo `Musics`, usa Web Audio API |
| **Culto Noturno** | Tipo de evento (`EveningWorship`) | Tipo padrão de escala |
| **Escola Dominical** | Tipo de evento (`SundaySchool`) | Evento de domingo pela manhã |

---

### Termos Técnicos do Stack

| Termo | Definição | Contexto de uso |
|---|---|---|
| **PWA** | Progressive Web App — app web instalável com suporte offline | O front-end é uma PWA via `vite-plugin-pwa` |
| **FCM** | Firebase Cloud Messaging — serviço de push notifications do Google | Usado para alertar membros sobre mudanças de status de escalas |
| **FCM Token** | Identificador único do dispositivo para receber notificações FCM | Salvo no campo `fcm_token` do usuário no banco |
| **JWT** | JSON Web Token — formato de token de autenticação | Usado para autenticar usuários; assinado com RSA e transportado via Cookie |
| **Brevo** | Serviço de e-mail transacional (API v3) | Usado para enviar códigos de recuperação de senha e notificações por e-mail |
| **IANA Timezone** | Padrão de identificação de fusos horários (ex: `America/Sao_Paulo`) | Usado para garantir que lembretes de escala sejam enviados no horário local correto do usuário |
| **RSA** | Algoritmo de criptografia assimétrica | Par de chaves pública/privada usado para assinar e verificar o JWT |
| **Cookie HttpOnly** | Cookie inacessível via JavaScript — protege contra XSS | Mecanismo de transporte do token JWT no WorshipHub |
| **Dapper** | Micro-ORM leve para .NET — executa SQL com mapeamento de objetos | Usado em toda a camada `WorshipInfra` para acesso ao MySQL |
| **Clean Architecture** | Padrão arquitetural com separação de responsabilidades em camadas | Estrutura do back-end (Domain → Application → Infra → API) |
| **AudioWorklet** | API Web moderna para processamento de áudio em thread separada | Usado no mixer para processar faixas sem bloquear a UI |
| **VU Meter** | Indicador visual de nível de volume (Volume Unit) | Renderizado via canvas 2D no componente `MeterCanvas.vue` |
| **Pinia** | Biblioteca de gerenciamento de estado para Vue 3 | Usado no `authStore` para manter dados do usuário logado |
| **Quasar** | Framework de componentes UI para Vue 3 | Base dos componentes visuais, notificações e layout |
| **Rate Limiting** | Limitação de requisições por tempo — proteção contra força bruta | Configurado na API: 5 requisições/min por IP no login |
| **CORS** | Cross-Origin Resource Sharing — controle de acesso entre origens | A API aceita apenas as origens listadas em `AllowedOrigins` |

---

### Siglas e Acrônimos

| Sigla | Significado |
|---|---|
| **WH** | WorshipHub (prefixo nos nomes de projetos e DB) |
| **FCM** | Firebase Cloud Messaging |
| **JWT** | JSON Web Token |
| **RSA** | Rivest–Shamir–Adleman (par de chaves criptográficas) |
| **PWA** | Progressive Web App |
| **SPA** | Single Page Application |
| **DI** | Dependency Injection (injeção de dependência) |
| **GHCR** | GitHub Container Registry (onde a imagem Docker da API é publicada) |
| **VPS** | Virtual Private Server (servidor de hospedagem em produção) |
| **HMR** | Hot Module Replacement (recarregamento automático no Vite em dev) |
