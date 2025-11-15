# FleetRent

Aplicação de referência para gestão de frotas de motocicletas com .NET 8. O projeto expõe uma API REST para cadastrar motos, controlar motoristas, registrar locações e emitir notificações por meio de uma fila RabbitMQ. A solução segue uma arquitetura em camadas (Domain, Application, Infrastructure e API) e utiliza Entity Framework Core com PostgreSQL.

## Visão geral

-   **Camada Domain**: concentra as entidades (`Bike`, `Driver`, `Rental`, `Notification`), regras de negócio e objetos de valor como `RentalPlan`. As regras de locação cobrem cálculo de multas por devolução antecipada ou atrasada, validação de categoria da CNH e integridade de placa.
-   **Camada Application**: serviços que orquestram casos de uso, aplicando validações e persistindo agregados via repositórios. Exemplos: geração de identificador sequencial de locações, upload assíncrono da CNH do motorista e publicação do evento `bike.created` após cadastrar uma moto.
-   **Camada Infrastructure**: implementações de repositórios com Entity Framework Core, serviços de armazenamento de arquivos, integração com RabbitMQ e migrations/seed de dados.
-   **Camada API**: ASP.NET Core hosting configurado com cultura padrão `pt-BR`, documentação Swagger e controllers REST (`/bikes`, `/drivers`, `/rentals`).

## Principais recursos

-   Cadastro, atualização de placa e exclusão de motocicletas, com verificação de unicidade de placa.
-   Cadastro de motoristas, upload da imagem da CNH (PNG ou BMP) e validação de categoria da habilitação.
-   Fluxo completo de locação: reserva com data futura, prevenção de conflitos de agenda por moto e cálculo automático de valores ao encerrar a locação.
-   Publicação do evento `bike.created` no RabbitMQ e consumo para criação de notificações quando a moto cadastrada é do ano de 2024.
-   Seed inicial de motos e motoristas para facilitar testes manuais.

## Tecnologias

-   .NET 8 / ASP.NET Core Web API
-   Entity Framework Core 8 + PostgreSQL
-   RabbitMQ
-   Docker & Docker Compose

## Requisitos

-   [.NET SDK 8.0](https://dotnet.microsoft.com/pt-br/download)
-   [Docker Desktop](https://www.docker.com/products/docker-desktop) (opcional, mas recomendado para subir PostgreSQL e RabbitMQ)

## Execução com Docker Compose

Na raiz da solução (`/FleetRent`), execute o comando abaixo para subir os serviços necessários com o Docker Compose:

```bash
docker compose up -d
```

Serviços disponíveis:

-   API: http://localhost:8080
-   Swagger UI: http://localhost:8080/swagger/index.html
-   PostgreSQL: `localhost:5432` (usuario: `fleetrent`, senha: `fleetrent`)
-   RabbitMQ Management: http://localhost:15672 (usuario: `fleetrent`, senha: `fleetrent`)

## Execução local (sem Docker)

1. Inicie um PostgreSQL acessível em `localhost:5432` com o banco `fleetrent_db` e usuário `fleetrent/fleetrent` (ou edite `appsettings.json`).
2. Inicie um RabbitMQ local e ajuste as credenciais na seção `RabbitMq` do `appsettings.json`, se necessário.
3. O projeto já roda as migrations automaticamente, caso precise executar manualmente, rode os comandos abaixo (na pasta `FleetRent.API`):
    ```bash
    dotnet ef migrations add Initial -p ../FleetRent.Infrastructure -s .
    dotnet ef database update -p ../FleetRent.Infrastructure -s .
    ```
4. Rode a API:
    ```bash
    dotnet run --project FleetRent.API
    ```
5. Acesse https://localhost:7021/swagger/index.html para explorar os endpoints.

## Endpoints principais

| Recurso                       | Método   | Descrição                                                   |
| ----------------------------- | -------- | ----------------------------------------------------------- |
| `/bikes`                      | `POST`   | Cadastra nova moto e publica o evento `bike.created`.       |
| `/bikes?plate=XXX1234`        | `GET`    | Lista motos com filtro opcional por placa.                  |
| `/bikes/{id}`                 | `GET`    | Busca moto pelo identificador.                              |
| `/bikes/{id}/plate`           | `PUT`    | Atualiza a placa (garantindo unicidade).                    |
| `/bikes/{id}`                 | `DELETE` | Remove moto sem locações ativas.                            |
| `/drivers`                    | `POST`   | Cadastra motorista garantindo unicidade de CNPJ e CNH.      |
| `/drivers/{id}/license-image` | `POST`   | Faz upload da imagem (base64String) da CNH (PNG/BMP).       |
| `/rentals`                    | `POST`   | Cria locação com data de início futura e plano selecionado. |
| `/rentals/{id}`               | `GET`    | Busca locação.                                              |
| `/rentals/{id}/return`        | `POST`   | Finaliza locação e calcula o total com multas ou extras.    |

## Dados seed

Após aplicar as migrations, o banco é populado automaticamente com três motos (`Bike001`, `Bike002`, `Bike003`) e três motoristas (`João da Silva`, `Maria Oliveira`, `Carlos Pereira`). Utilize-os para testes rápidos.

## Estrutura do repositório

```
FleetRent.sln
FleetRent.API/              # API ASP.NET Core
FleetRent.Application/      # Serviços de aplicação e DTOs
FleetRent.Domain/           # Entidades, enums, erros e contratos
FleetRent.Infrastructure/   # EF Core, repositórios, mensageria e storage
FleetRent.Tests/            # Projeto de testes automatizados (xUnit)
```

## Documentação

A documentação técnica completa do sistema está disponível na pasta `Documentação`, no arquivo `Documentação Ténica.pdf`.

## Observações de desenvolvimento

### Bikes

-   O endpoint `POST /bikes` atualmente retorna HTTP 201 com uma string vazia, então o cliente nunca recebe o payload da moto.
-   Quando uma moto não é encontrada (em GET/PUT/DELETE) o controller mapeia o erro de domínio para HTTP 400 em vez de HTTP 404, o que foge da semântica REST padrão. Essa mesma pratica de StatusCodes incorretos também se aplica para os outros endpoints do projeto, dessa forma, resolvi seguir exatamente os StatusCodes do Swagger fornecido como exemplo.

### Drivers

-   As CNHs enviadas são armazenadas em disco na pasta `uploads`, como requerido, mas é importante garantir que a aplicação em runtime possua permissão para criar e escrever nesse diretório.

### Rentals

-   O DTO de criação de locação expõe `StartDate`, `EndDate` e `PlannedEndDate`, porém o serviço só usa `StartDate` e ainda força a locação real a começar um dia após o valor enviado. Isso diverge do contrato, em que a API deveria controlar as datas oficiais de início e fim com base no plano selecionado.
-   Quando uma locação é encerrada, na documentação proposta o controller responde com HTTP 200 e uma mensagem de confirmação somente, então o cliente não consegue “consultar o valor total da locação” como exigido. Dessa forma, rescolvi retornar com mais informações para que o cliente visualize o `TotalCost`.
