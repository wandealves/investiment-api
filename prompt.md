# ✅ **PROMPT COMPLETO PARA IA — Sistema de Gestão de Investimentos**

**Quero que você gere uma aplicação baseada na seguinte especificação. Leia tudo atentamente antes de gerar qualquer código.**

## 🟦 **Descrição Geral**

Este é um **sistema de gerenciamento de investimentos**, onde o usuário poderá:

- Criar, editar e excluir **carteiras de investimento**.
- Registrar ativos manualmente.
- Importar automaticamente **Notas de Negociação** (arquivos PDF, HTML ou TXT fornecidos por corretoras).
- Visualizar histórico de operações, posições consolidadas e desempenho.
- Gerar relatórios e métricas de performance.

O sistema deve seguir boas práticas, arquitetura limpa e código organizado.

---

# 🟩 **Funcionalidades Principais**

## 1️⃣ **Gestão de Carteiras**

- Criar, editar, renomear e remover carteiras.
- Suporte a múltiplas carteiras por usuário.
- Dados armazenados: nome, descrição, data de criação, ativo financeiro principal (opcional).

## 2️⃣ **Gestão de Ativos**

- Cadastro manual de ativos:

  - Ações
  - FIIs
  - ETFs
  - BDRs
  - Renda Fixa
  - Cripto (opcional)

- Consulta a dados básicos do ativo (ticker, tipo, setor).

## 3️⃣ **Importação de Nota de Negociação**

O usuário poderá importar Notas de Corretagem / Notas de Negociação para automatizar o registro das operações.

A importação deve:

- Extrair dados de PDF, HTML ou TXT.
- Identificar automaticamente:

  - Ticker
  - Tipo de operação (compra/venda)
  - Quantidade
  - Preço unitário
  - Valor total
  - Corretora
  - Data e hora
  - Custos: taxa de liquidação, emolumentos, ISS, corretagem, etc.

- Consolidar custos proporcionais por operação.
- Validar inconsistências.
- Vincular automaticamente as operações à carteira selecionada.

## 4️⃣ **Gestão de Operações**

- Registrar operações manualmente ou via importação.
- Tipos de operação:

  - Compra
  - Venda
  - Rendimentos (dividendos, JCP)
  - Proventos diversos
  - Bonificações
  - Desdobramentos e agrupamentos

- Cálculo automático do preço médio.

## 5️⃣ **Posição Consolidada**

Para cada carteira:

- Quantidade atual de cada ativo
- Preço médio
- Preço de mercado (caso exista integração externa – opcional)
- Rentabilidade diária e acumulada
- Distribuição por classe de ativos
- Valorização por ativo e por carteira

## 6️⃣ **Relatórios e Métricas**

- Lista completa de operações
- Histórico de proventos
- Rentabilidade (IRR, TWR, simples)
- Gráficos:

  - Evolução patrimonial
  - Alocação por classe
  - Alocação por ativo

- Exportação em PDF ou Excel

## 7️⃣ **Funcionalidades Adicionais**

- Busca inteligente por ativos.
- Logs de importações.
- Indicadores:

  - Lucro/Prejuízo realizado
  - Lucro/Prejuízo não realizado
  - Yield on Cost

- Suporte a múltiplos usuários.

---

# 🟧 **Requisitos Técnicos**

- Backend: **.NET C#**
- ORM: **Entity Framework Core** (com Fluent API)
- Frontend: **ReactJS** + **Aceternity UI**
- Banco: SQL Server ou PostgreSQL (qualquer um)
- Docker para ambiente de desenvolvimento
- Arquitetura limpa (Clean Architecture / DDD opcional)

---

# 🟥 **O que a IA deve entregar**

- Estrutura completa do projeto
- Modelos (entidades)
- Configurations do EF Core
- Serviços e interfaces
- Aplicação React com componentes Aceternity UI
- Rotas e fluxo da aplicação
- README completo
- (Opcional) Testes automatizados

---

# 🟦 **Tom e Requisitos da Resposta**

- Não invente funcionalidades que não estejam listadas.
- Se algo estiver ambíguo, seguir o padrão do mercado.
- Organizar tudo de forma limpa e modular.
- Usar boas práticas de codificação (SOLID, separação de camadas, validações, logs).
