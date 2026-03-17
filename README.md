

# 📄 README.md — BankMore

```markdown
# 🚀 BankMore

Sistema de simulação bancária com arquitetura moderna baseada em microserviços, mensageria e processamento assíncrono.

## 📌 Visão Geral

O BankMore simula operações financeiras como:

- Criação de contas
- Autenticação
- Movimentações (crédito/débito)
- Transferências entre contas
- Processamento de tarifas (fee)
- Comunicação assíncrona via Kafka

---

## 🏗️ Arquitetura

Arquitetura baseada em:

- **DDD (Domain Driven Design)**
- **Clean Architecture**
- **Microservices**
- **Event-Driven Architecture (EDA)**

### Serviços

| Serviço         | Responsabilidade |
|----------------|-----------------|
| Account API     | Contas, login, saldo e movimentações |
| Transfer API    | Transferências entre contas |
| Fee Worker      | Processamento de tarifas via Kafka |
| Kafka           | Mensageria |
| Kafka UI        | Monitoramento de eventos |

---

## 📦 Tecnologias

- .NET 9
- Dapper
- SQLite
- Docker / Docker Compose
- Kafka (Confluent)
- Swagger
- Postman

---

## 📁 Estrutura do Projeto

```

src/
├── Services/
│   ├── Account/
│   ├── Transfer/
│   └── Fee/
├── BuildingBlocks/
└── docker-compose.yml

````

---

## ⚙️ Pré-requisitos

Antes de rodar o projeto, instale:

- Docker Desktop
- .NET SDK 9 (opcional para debug)
- Postman (opcional para testes)

---

## 🐳 Subindo a aplicação (Docker)

### 1. Limpar ambiente (recomendado)

```bash
docker compose down -v
docker system prune -af
````

---

### 2. Build completo

```bash
docker compose build --no-cache
```

---

### 3. Subir todos os serviços

```bash
docker compose up -d
```

---

### 4. Verificar containers

```bash
docker compose ps
```

---

### 5. Logs (debug)

```bash
docker compose logs -f
```

Logs específicos:

```bash
docker compose logs -f account-api
docker compose logs -f transfer-api
docker compose logs -f fee-worker
```

---

## 🌐 Endpoints

| Serviço      | URL                                                            |
| ------------ | -------------------------------------------------------------- |
| Account API  | [http://localhost:5001/swagger](http://localhost:5001/swagger) |
| Transfer API | [http://localhost:5002/swagger](http://localhost:5002/swagger) |
| Kafka UI     | [http://localhost:8085](http://localhost:8085)                 |

---

## 🧪 Testes com Postman

### 📥 Importar arquivos

Use os arquivos:

* `BankMore.postman_collection.json`
* `BankMore.local.postman_environment.json`

---

### ▶️ Ordem de execução

1. Criar contas
2. Login
3. Crédito
4. Débito
5. Transferência
6. Validação de saldo
7. Testes de erro

---

### 🔐 Autenticação

Após login:

* Token é salvo automaticamente
* Usado nos próximos requests

---

## 💰 Fluxo de Negócio

### ✔️ Crédito

```json
{
  "requestId": "mov-001",
  "amount": 1000,
  "type": "C"
}
```

---

### ✔️ Débito

```json
{
  "requestId": "mov-002",
  "amount": 150,
  "type": "D"
}
```

---

### ✔️ Transferência

```json
{
  "requestId": "transfer-001",
  "destinationAccountNumber": "123456",
  "amount": 200
}
```

---

## 🔄 Event Driven (Kafka)

### Tópicos

* `transfers.completed`
* `fees.completed`

---

### Fluxo

1. Transfer API publica evento
2. Kafka recebe
3. Fee Worker consome
4. Fee é processada
5. Novo evento pode ser publicado

---

## 📊 Monitoramento Kafka

Acesse:

```
http://localhost:8085
```

Verifique:

* Topics
* Messages
* Producers / Consumers

---

## ⚠️ Troubleshooting

### ❌ Erro: SQL não encontrado

```
FileNotFoundException: /app/Persistence/Sql/*.sql
```

✔️ Solução:

* Garantir COPY correto no Dockerfile
* Verificar path `/app/Persistence/Sql`

---

### ❌ Erro: DateTime inválido

```
String '' was not recognized as a valid DateTime
```

✔️ Solução:

* Garantir alias SQL (`AS CreatedAtUtc`)
* Validar dados no banco

---

### ❌ Kafka connection refused

```
Connect to kafka:29092 failed
```

✔️ Solução:

* Kafka ainda não iniciou
* Aguarde alguns segundos
* Ou reinicie containers

---

### ❌ Topic não existe

```
Unknown topic or partition
```

✔️ Solução:

* Criar tópico automaticamente
* Ou garantir que producer executou antes

---

## 🔁 Idempotência

O sistema suporta idempotência via:

* `requestId`

✔️ Evita:

* duplicidade de transferência
* duplicidade de movimentação

---

## 🔐 Segurança

* Autenticação via token (JWT simplificado)
* Validação de CPF
* Controle de conta ativa/inativa

---

## 📈 Melhorias futuras

* Persistência em banco real (PostgreSQL)
* Retry com DLQ (Kafka)
* Observabilidade (Prometheus + Grafana)
* OpenTelemetry
* Circuit Breaker (Polly)

---

## 👨‍💻 Autor

**Edio Rhoden**
Tech Leader | .NET Specialist | Arquitetura de Software

---

## 🧠 Observação técnica

Este projeto demonstra:

* Arquitetura moderna
* Integração entre serviços
* Mensageria real
* Consistência eventual
* Boas práticas de backend

  ## 📁 Pasta Collection-Postman
  * BankMore.postman_collection.json
  * BankMore.local.postman_environment.json

