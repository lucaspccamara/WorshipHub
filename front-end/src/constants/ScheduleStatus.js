export const ScheduleStatus = Object.freeze([
  { label: "Criado", value: 0, color: "blue" },
  { label: "Coletando Disponibilidade", value: 1, color: "orange" },
  { label: "Aguardando Repertório", value: 2, color: "purple" },
  { label: "Concluído", value: 3, color: "green" },
  { label: "Excluído", value: 10, color: "red" }
]);

export const EScheduleStatus = Object.freeze({
  Criado: 0,
  ColetandoDisponibilidade: 1,
  AguardandoRepertorio: 2,
  Concluido: 3,
  Excluido: 10
});