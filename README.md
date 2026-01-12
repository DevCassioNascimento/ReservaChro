.

📱 ReservaChro

ReservaChro é uma aplicação mobile para gestão e reserva de Chromebooks por unidade escolar, desenvolvida para substituir controles manuais (cadernos, anotações informais e planilhas) por um sistema seguro, auditável e escalável.

O projeto foi concebido para atender múltiplas unidades escolares, com controle rigoroso de acesso por perfil e por escola, garantindo isolamento de dados e governança operacional.

🎯 Objetivo do Projeto

Centralizar a reserva de Chromebooks por professores

Permitir ao profissional de TI o controle dos ativos da sua unidade

Oferecer ao administrador uma visão global das escolas, usuários e equipamentos

Eliminar processos manuais e reduzir conflitos de agenda

Criar uma base sólida para crescimento futuro do sistema

🧱 Arquitetura

O projeto segue rigorosamente o padrão Clean Architecture, separando responsabilidades e garantindo manutenibilidade a longo prazo.

Estrutura da Solução
ReservaChro
│
├── Back
│ ├── ReservaChro.Domain
│ ├── ReservaChro.Application
│ ├── ReservaChro.Infrastructure
│ └── ReservaChro.Api
│
├── Mobile
│ └── ReservaChro.Mobile
│
└── CONVENTIONS.md

Camadas

Domain
Regras de negócio, entidades, enums e exceções de domínio.

Application
Casos de uso, DTOs, validações e contratos de aplicação.

Infrastructure
Persistência de dados (EF Core + PostgreSQL), repositórios e integrações.

API
Endpoints REST, autenticação, autorização, Swagger e configuração.

Mobile (MAUI)
Aplicação mobile multiplataforma (Android, Windows, iOS futuramente).

🛠️ Tecnologias Utilizadas
Backend

.NET 10

C#

ASP.NET Core Web API

Clean Architecture

Entity Framework Core

PostgreSQL

Swagger (Swashbuckle)

Mobile

.NET MAUI

XAML

MVVM (base para evolução)

Outros

Git / GitHub

RESTful APIs

Versionamento de banco via migrations

👥 Perfis de Usuário
🔐 Admin

Gerencia todas as unidades escolares

CRUD de escolas

CRUD de profissionais de TI

CRUD de Chromebooks

Define status técnico dos ativos

🧑‍💻 Profissional de TI (por escola)

Acesso restrito à sua unidade

Gerencia Chromebooks da escola

Atualiza status operacional dos ativos

Visualiza agenda de reservas

Cadastra professores da unidade

👩‍🏫 Professor

Acesso restrito à sua unidade

Visualiza agenda de Chromebooks

Realiza reservas (data e quantidade)

Adiciona observações sobre os ativos

🔐 Controle de Acesso

Todos os dados são isolados por unidade escolar

TI e Professor não enxergam dados de outras escolas

Admin possui acesso global

SchoolId é obrigatório em entidades de negócio relevantes

🚀 Estado Atual do Projeto
✅ Sprint 0 — Concluída

Estrutura Clean Architecture criada

API configurada e rodando

Swagger funcional

PostgreSQL configurado com banco próprio

EF Core + Npgsql instalados

Convenções documentadas

Projeto MAUI criado e compilando

Build Android validado

Endpoint de health check ativo (/health)

▶️ Como Executar (Desenvolvimento)
Backend
dotnet run --project ReservaChro.Api

Acesse:

API: http://localhost:5193

Swagger: http://localhost:5193/swagger

Health Check: http://localhost:5193/health

Mobile
dotnet build Mobile/ReservaChro.Mobile/ReservaChro.Mobile.csproj

⚠️ Importante: evite caminhos com caracteres não-ASCII (ex.: ç, ã, é), pois o build Android pode falhar.

📄 Convenções

As regras oficiais de nomenclatura, organização, DTOs, responses e segurança estão documentadas em:

CONVENTIONS.md

Esse arquivo é parte essencial do projeto e deve ser seguido em todas as sprints.

🗺️ Roadmap (alto nível)

Sprint 1 — Usuários e autenticação (JWT)

Sprint 2 — Gestão de escolas

Sprint 3 — Ativos (Chromebooks)

Sprint 4 — Agenda e reservas

Sprint 5 — Observações e histórico

Sprint 6 — Produção e publicação
