# RiderHub - Back-End

Este repositório contém a solução para o desenvolvimento Back-End RiderHub, uma plataforma para gerenciamento de locações de motos e entregadores.

## Visão Geral do Projeto

O projeto foi desenvolvido seguindo os princípios da **Clean Architecture** (Arquitetura Limpa), separando as responsabilidades em camadas distintas para garantir um sistema desacoplado, testável e de fácil manutenção.

## Arquitetura

A solução está organizada nas seguintes camadas:

- **`RiderHub.Domain`**: Camada central que contém as entidades de negócio, enums e as regras de negócio mais puras, sem dependências de tecnologia.
- **`RiderHub.Application`**: Contém a lógica da aplicação e os casos de uso. Orquestra o fluxo de dados entre a camada de apresentação e a camada de domínio.
- **`RiderHub.Infrastructure`**: Camada responsável pelos detalhes de implementação de tecnologias externas, como o acesso ao banco de dados (usando Entity Framework), comunicação com message brokers (RabbitMQ) e outros serviços.
- **`RiderHub.WebApi`**: A camada de apresentação, implementada como uma ASP.NET Core Web API. É responsável por expor os endpoints, receber as requisições HTTP, e interagir com a camada de aplicação.
- **`RiderHub.Tests`**: Projeto contendo testes unitários e de integração para garantir a qualidade e o correto funcionamento da aplicação.

## Tecnologias e Boas Práticas

- **Framework**: .NET 8
- **API**: ASP.NET Core 8
- **Banco de Dados**: PostgreSQL
- **ORM**: Entity Framework Core 8
- **Arquitetura**: Clean Architecture
- **Padrões de Projeto**:
  - **Repository Pattern**: Abstrai o acesso a dados.
  - **Dependency Injection**: Gerenciamento de dependências nativo do .NET Core.
- **Mensageria**: RabbitMQ para comunicação assíncrona.
- **Segurança**: Hash de senhas utilizando BCrypt.Net.
- **Testes**: Testes unitários e de integração para validar as camadas da aplicação.
- **Documentação da API**: Swagger (Swashbuckle) para documentação e teste de endpoints.
- **Containerização**: Suporte a Docker e Docker Compose para fácil deploy e orquestração do ambiente.

---

## Como Executar o Projeto

Existem duas maneiras principais de executar o projeto: usando **Docker Compose** (recomendado para um setup rápido) ou localmente usando o **.NET SDK**.

### Método 1: Executar com Docker Compose (Recomendado)

Este método irá provisionar e executar todos os serviços necessários (API, Banco de Dados PostgreSQL e RabbitMQ) em containers Docker.

**Pré-requisitos:**
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- Um cliente Git

**Passos:**

1.  **Baixar o Projeto**
    Clone o repositório para a sua máquina local:
    ```bash
    git clone https://github.com/JoseHenriqueRG/RiderHub.git
    cd RiderHub
    ```

2.  **Iniciar os Serviços**
    Na pasta raiz do projeto (onde o arquivo `docker-compose.yml` está localizado), execute o seguinte comando:
    ```bash
    docker-compose up -d
    ```
    Este comando irá baixar as imagens necessárias, construir a imagem da API e iniciar os três containers em background.

3.  **Aplicar a Migração do Banco de Dados**
    Após os containers estarem em execução, aplique a migração para criar as tabelas no banco de dados.
    ```bash
    dotnet ef database update --project src/RiderHub.Infrastructure/RiderHub.Infrastructure.csproj --startup-project src/RiderHub.WebApi/RiderHub.WebApi.csproj
    ```
    **Nota:** Para que este comando funcione, a string de conexão no seu `appsettings.Development.json` deve apontar para o container do Postgres (`Host=localhost;Port=5432;...`).

### Método 2: Executar Localmente com .NET SDK

Este método é útil para desenvolvimento e depuração detalhada.

**Pré-requisitos:**
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Acesso a uma instância do PostgreSQL e RabbitMQ (podem ser locais ou em nuvem).

**Passos:**

1.  **Baixar o Projeto** (conforme instrução acima).

2.  **Configurar o Ambiente**
    A aplicação precisa das strings de conexão para o PostgreSQL e RabbitMQ. Configure-as no arquivo `appsettings.Development.json` dentro do projeto `RiderHub.WebApi`.
    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Host=localhost;Port=5432;Database=RiderHub;Username=seu_usuario;Password=sua_senha;"
      },
      "RabbitMQ": {
        "HostName": "localhost"
      }
    }
    ```

3.  **Atualizar a Base de Dados**
    Execute o comando do Entity Framework para criar a estrutura do banco:
    ```bash
    dotnet ef database update --project src/RiderHub.Infrastructure/RiderHub.Infrastructure.csproj --startup-project src/RiderHub.WebApi/RiderHub.WebApi.csproj
    ```

4.  **Executar a API**
    Navegue até a pasta do projeto WebApi e execute a aplicação:
    ```bash
    cd src/RiderHub.WebApi
    dotnet run
    ```

## Acessando a Aplicação

Independentemente do método de execução, a API estará disponível nos seguintes endereços:

- **API URL**: `http://localhost:8080`
- **Swagger UI**: `http://localhost:8080/swagger`
- **RabbitMQ Management**: `http://localhost:15672` (usuário: `guest`, senha: `guest`)

## Executando os Testes

Para rodar a suíte de testes (unitários e de integração), execute o seguinte comando na raiz do projeto:

```bash
dotnet test
```
