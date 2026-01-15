# ReservaChro — Convenções do Projeto

## 1) Estrutura (Clean Architecture)

- ReservaChro.Domain: regras de negócio, entidades, enums, value objects, exceções de domínio
- ReservaChro.Application: casos de uso (services/use cases), DTOs, validações de input, interfaces de aplicação
- ReservaChro.Infrastructure: persistência (EF Core), repositórios, integrações externas
- ReservaChro.Api: controllers/endpoints, autenticação, swagger, configuração e DI

## 2) Nomenclatura

- Entidades: PascalCase (ex: School, User, Chromebook, Reservation)
- Tabelas no banco: snake_case (definido via EF Core)
- Enums: PascalCase com sufixo "Status" quando aplicável (ex: ChromebookOperationalStatus)

## 3) Padrão de Endpoints

- Base: /api/v1
- Recursos no plural: /schools, /users, /chromebooks, /reservations
- Sempre retornar envelope padrão.

## 4) Envelope padrão de resposta (API)

Sucesso:
{
"success": true,
"data": <qualquer>,
"errors": []
}

Erro:
{
"success": false,
"data": null,
"errors": ["mensagem 1", "mensagem 2"]
}

## 5) DTOs e Requests

- DTOs ficam em ReservaChro.Application/DTOs
- Requests/Responses por feature podem ser organizados em subpastas (ex: DTOs/Users, DTOs/Schools)
- Nunca expor entidades de domínio diretamente na API.

## 6) Regras de Acesso (Obrigatórias)

- Toda entidade de negócio deve ter SchoolId quando fizer sentido.
- TI e Professor só enxergam dados da própria SchoolId.
- Admin pode enxergar todas as unidades.
- Professor sempre pertence à mesma SchoolId do TI que o cadastrou.

## 7) Migrações

- DbContext fica em ReservaChro.Infrastructure
- Migrations ficam em ReservaChro.Infrastructure/Migrations
- Startup project para CLI: ReservaChro.Api

## 8) Erros e validação

- Validações de entrada: Application
- Regras de negócio: Domain
- Exceções de domínio: Domain/Exceptions
