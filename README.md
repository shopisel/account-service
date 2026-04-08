# account-service

Microservico .NET para gerir o perfil do utilizador autenticado no ecossistema ShopISEL.

## Funcionalidades

- Obter o perfil do utilizador autenticado em `/accounts/me`
- Atualizar preferencias do perfil em `/accounts/me`
- Sincronizar dados base vindos do token do Keycloak

## Configuracao

Definir:

- `ConnectionStrings:AccountService`
- `Keycloak:Authority`
- `Keycloak:Audience`
- `Keycloak:RequireHttpsMetadata`

## Desenvolvimento

```bash
dotnet restore src
dotnet test src
```
