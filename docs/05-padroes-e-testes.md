# 📐 Padrões, Convenções e Guia de Testes — WorshipHub

---

## Padrões e Convenções

### Convenção de Nomes por Tipo de Artefato

#### Front-end (Vue 3 / JavaScript)

| Tipo | Padrão | Exemplo |
|---|---|---|
| Componentes Vue | `PascalCase` | `ManageSchedule.vue`, `MixerVS.vue` |
| Páginas | `PascalCase` + `Page` quando ambíguo | `LoginPage.vue`, `Schedule.vue` |
| Composables | `camelCase` + prefixo `use` | `useAudioMixer.js` |
| Stores (Pinia) | `camelCase` + prefixo `use` + sufixo `Store` | `useAuthStore` |
| Arquivos de constantes | `PascalCase` | `ScheduleStatus.js`, `Role.js` |
| Constantes exportadas | `PascalCase` (objeto) / `EPascalCase` (enum-like) | `Role`, `EScheduleStatus` |
| Variáveis / funções | `camelCase` | `scheduleId`, `fetchSchedules` |
| Variáveis de ambiente | `VITE_PREFIXO_NOME` (UPPER_SNAKE_CASE) | `VITE_API_URL` |
| Workers | `camelCase` | `audioWorkletProcessor.js` |

#### Back-end (.NET 8 / C#)

| Tipo | Padrão | Exemplo |
|---|---|---|
| Classes | `PascalCase` | `ScheduleService`, `ScheduleCreationDTO` |
| Interfaces | `I` + `PascalCase` | `IScheduleRepository` |
| Métodos públicos | `PascalCase` | `CreateSchedule()`, `GetListPaged()` |
| Métodos privados | `PascalCase` | `GenerateHashPassword()` |
| Campos privados | `_camelCase` | `_repository`, `_userRepo` |
| Parâmetros / variáveis locais | `camelCase` | `scheduleId`, `newStatus` |
| DTOs | sufixo `DTO` ou `Dto` (ambos em uso) | `ScheduleCreationDTO`, `ScheduleRepertoireDto` |
| Enums | `PascalCase` (tipo e valores) | `ScheduleStatus.Created` |
| Controllers | sufixo `Controller` | `ScheduleController` |
| Serviços | sufixo `Service` | `ScheduleService` |
| Repositórios | sufixo `Repository` | `ScheduleRepository` |

#### Banco de Dados (MySQL)

| Tipo | Padrão | Exemplo |
|---|---|---|
| Tabelas | `snake_case`, plural | `schedules`, `schedule_users` |
| Colunas | `snake_case` | `event_type`, `fcm_token`, `reset_password_token_code` |

---

### Padrão de Branches

```
main                        ← branch principal (protegida, deploy automático)
  └── feature/nome          ← nova funcionalidade
  └── fix/nome-do-bug       ← correção de bug
  └── chore/nome            ← tarefa técnica (config, deps, refactor)
  └── docs/nome             ← apenas documentação
```

**Exemplos:**
```
feature/mixer-audio
fix/schedule-availability-validation
chore/update-firebase-sdk
docs/onboarding-guide
```

---

### Padrão de Commits

O projeto segue **Conventional Commits** (inferido pela estrutura CI/CD e histórico):

```
<tipo>: <descrição curta no imperativo, em pt-BR ou inglês>

[corpo opcional]
[footer opcional: referência a issue]
```

**Tipos aceitos:**

| Tipo | Quando usar |
|---|---|
| `feat` | Nova funcionalidade |
| `fix` | Correção de bug |
| `chore` | Tarefas de manutenção (deps, config) |
| `docs` | Apenas documentação |
| `refactor` | Refatoração sem mudança de comportamento |
| `style` | Formatação, sem lógica |
| `perf` | Melhoria de performance |
| `test` | Adição/atualização de testes |

**Exemplos:**
```
feat: adicionar mixer de áudio multi-track
fix: corrigir validação de disponibilidade ao alterar status
chore: atualizar dependência do Firebase para v12
docs: adicionar documentação de onboarding
```

---

### Regras de Pull Request / Code Review

> ℹ️ Não há um arquivo de regras de PR explícito no repositório. As convenções abaixo são inferidas pelo fluxo de CI/CD e estrutura do projeto.

- **Branch alvo**: sempre `main`
- **CI/CD**: push direto para `main` dispara o pipeline de deploy — portanto, toda mudança deve ir via PR
- **Revisão**: ao menos 1 revisão antes do merge é recomendada
- **Checklist sugerido antes de criar um PR**:
  - [ ] O código compila sem erros (`dotnet build` / `npm run build`)
  - [ ] O comportamento foi testado manualmente em dev
  - [ ] Não há `Console.WriteLine` de debug esquecidos no back-end
  - [ ] Variáveis de ambiente sensíveis não foram commitadas
  - [ ] O PR tem uma descrição clara do que foi alterado e por quê

---

### Estrutura de Pastas para Novos Módulos

#### Front-end
```
src/
├── pages/
│   └── NomeFeature.vue          ← página principal da feature
├── components/
│   ├── ManageNomeFeature.vue    ← modal/dialog de criação/edição
│   └── NomeFeatureDetalhe.vue   ← componente de detalhe (se necessário)
├── constants/
│   └── NomeFeatureOptions.js    ← constantes/enums do domínio
└── composables/
    └── useNomeFeature.js        ← lógica reutilizável (se necessário)
```

#### Back-end
```
WorshipDomain/
├── Entities/NomeEntidade.cs
├── DTO/NomeEntidade/NomeEntidadeDTO.cs
└── Repository/INomeRepository.cs
WorshipInfra/Repository/NomeRepository.cs
WorshipApplication/Services/NomeService.cs
WorshipApi/Controllers/NomeController.cs
```

---

### Padrões de Logging e Tratamento de Erros

#### Back-end
- Logging via `Microsoft.Extensions.Logging` (`ILogger`) configurado no `Program.cs`
- Logs de autenticação em nível `Debug` (configurado no `appsettings`)
- Erros de negócio retornam `Result<T>` do FluentResults (ex: `Result.Ok()`, `Result.Fail()`)
- Erros HTTP são traduzidos em `ActionResult` (`ConflictResult`, `NoContentResult`, etc.)
- Exceções inesperadas são capturadas nos `catch` dos services, não sobem para o usuário

#### Front-end
- Erros de API são capturados no wrapper `api.js` e relançados como `Error()`
- Componentes exibem feedback ao usuário via `Notify` do Quasar (`Notify.create(...)`)
- Não há middleware global de tratamento de erros — cada componente trata o `catch`

---

## Guia de Testes

> ⚠️ **Estado atual**: O projeto **não possui testes automatizados** implementados (nem front-end, nem back-end). Não foram encontrados arquivos de teste no repositório além das dependências de terceiros em `node_modules/`.

### Stack de Testes (Recomendada para implementação futura)

| Camada | Framework Recomendado | Finalidade |
|---|---|---|
| Back-end (unit) | xUnit | Testes unitários de Services e domain logic |
| Back-end (integration) | xUnit + TestContainers | Testes de repository com MySQL real |
| Front-end (unit) | Vitest + Vue Test Utils | Testes de composables e componentes |
| Front-end (e2e) | Playwright ou Cypress | Testes de fluxo completo no navegador |
| Mocking back-end | Moq | Mock de interfaces de repositório |

---

### Como Rodar os Testes (quando implementados)

#### Back-end
```bash
cd back-end
dotnet test
```

#### Front-end
```bash
cd front-end
npm run test        # unit tests (Vitest)
npm run test:e2e    # end-to-end (Playwright/Cypress)
```

---

### Estrutura Sugerida para Testes

#### Back-end
```
back-end/
└── WorshipTests/                          ← projeto de testes (xUnit)
    ├── Services/
    │   ├── ScheduleServiceTests.cs
    │   └── AuthServiceTests.cs
    └── Repositories/
        └── ScheduleRepositoryIntegrationTests.cs
```

#### Front-end
```
front-end/
└── src/
    └── __tests__/
        ├── composables/
        │   └── useAudioMixer.test.js
        └── components/
            └── ManageSchedule.test.js
```

---

### Padrão de Nomenclatura para Testes (quando implementados)

```csharp
// C# — padrão: Método_Cenário_ResultadoEsperado
[Fact]
public void CreateSchedule_QuandoDataJaExiste_RetornaErro()
{
    // Arrange
    // Act
    // Assert
}
```

```javascript
// JavaScript — padrão: describe + it descritivo
describe('useAudioMixer', () => {
  it('deve pausar o áudio quando pause() é chamado', () => {
    // arrange / act / assert
  });
});
```

---

### Checklist de Testes para Nova Feature

Até a implementação de testes automatizados, use o seguinte checklist manual:

- [ ] O happy path funciona end-to-end (front → API → banco)
- [ ] Campos obrigatórios com valor vazio são rejeitados com mensagem clara
- [ ] Acesso sem autenticação retorna 401
- [ ] Acesso com role insuficiente retorna 403 ou redireciona para `/`
- [ ] A ação funciona em mobile (tela pequena, touch)
- [ ] Não há regressão em features relacionadas (ex: transição de status afeta notificações)
- [ ] Variáveis de ambiente sensíveis não aparecem no bundle gerado (`npm run build`)

---

### Relatório de Cobertura (quando implementado)

#### Back-end
```bash
dotnet test --collect:"XPlat Code Coverage"
# Usar ReportGenerator para gerar HTML:
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport"
```

#### Front-end
```bash
npm run coverage     # Vitest gera relatório em coverage/
```

> ⚠️ `Informação não disponível — metas de cobertura não definidas no projeto ainda. Preencher manualmente quando testes forem implementados.`
