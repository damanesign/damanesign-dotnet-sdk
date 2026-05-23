# Damanesign .NET SDK

SDK .NET officiel pour consommer les APIs Damanesign.

## Prérequis

- .NET 8+

## Installation locale

```bash
dotnet restore
dotnet build
dotnet run --project tests/Damanesign.Sdk.Tests/Damanesign.Sdk.Tests.csproj
```

## Installation NuGet

Après publication sur NuGet :

```bash
dotnet add package Damanesign.Sdk
```

## Création d'une transaction

### Étape 1 : upload du document

Avant de créer la transaction, le PDF doit être envoyé à Damanesign via `POST /files/upload`.
Le SDK expose cette étape avec `UploadFileAsync(...)`.

```csharp
using Damanesign.Sdk;

using DamanesignClient client = DamanesignClient.Create(
    "https://api-recette.damanesign.ma",
    Environment.GetEnvironmentVariable("DAMANESIGN_API_KEY")!);

var file = await client.UploadFileAsync("contract.pdf");
string fileId = file!.Id!;
```

Vous utiliserez ensuite `fileId` dans `Members[].Fields[].File`.

### Étape 2 : création puis lancement

```csharp
using Damanesign.Sdk;
using Damanesign.Sdk.Models;

using DamanesignClient client = DamanesignClient.Create(
    "https://api-recette.damanesign.ma",
    Environment.GetEnvironmentVariable("DAMANESIGN_API_KEY")!);

var file = await client.UploadFileAsync("contract.pdf");

var transaction = await client.CreateTransactionAsync(new CreateTransactionRequest
{
    Name = "Contrat client",
    Type = "simple",
    DeliveryMode = "email",
    AuthenticationMode = "email",
    Ordered = false,
    Members =
    [
        new MemberRequest
        {
            Type = MemberTypes.Signer,
            Firstname = "Sara",
            Lastname = "El Amrani",
            Email = "sara@example.com",
            Phone = "+212600000000",
            Fields =
            [
                new FieldRequest
                {
                    File = file!.Id,
                    Type = FieldTypes.Signature,
                    Page = 1,
                    Position = "141,268,151,101"
                }
            ]
        }
    ]
});

await client.StartTransactionAsync(transaction!.Id!);

Console.WriteLine(transaction.Id);
```

Le SDK utilise l'authentification HTTP `x-api-key: <apiKey>`, conformément au Swagger Damanesign.

Le flux standard suit le Developer Portal :

1. `POST /files/upload` avec le champ multipart `file`.
2. `POST /transactions` avec `name`, `type`, `authenticationMode`, `members` et `members[].fields[].file`.
3. `POST /transactions/{id}/start` pour lancer la transaction.

## Méthodes exposées

Le client couvre les opérations publiques du Swagger `2.5.3` :

```csharp
await client.ListTransactionsAsync(filter);
await client.ListAssignedTransactionsAsync(filter);
await client.GetTransactionAsync(transactionId);
await client.CreateTransactionAsync(request);
await client.UpdateTransactionAsync(transactionId, request);
await client.DeleteTransactionAsync(transactionId);
await client.UpdateMemberAsync(transactionId, memberId, memberRequest);
await client.UpdateMemberAuthenticationAsync(transactionId, memberId, "sms");
await client.StartTransactionAsync(transactionId);
await client.SendReminderAsync(transactionId);
await client.ProlongTransactionAsync(transactionId, new DateOnly(2026, 12, 31));
await client.CancelTransactionAsync(transactionId);
await client.GetSignatureUrlAsync(transactionId, memberId);

await client.UploadFileAsync("contract.pdf");
await client.GetFileAsync(fileId);
await client.DownloadFileAsync(fileId);

await client.SealDocumentAsync(sealRequest);
await client.ListSealsAsync(sealFilter);
```

Les filtres de liste utilisent `TransactionFilter` et `SealFilter` :

```csharp
var transactions = await client.ListTransactionsAsync(TransactionFilter.Create()
    .Status(["draft", "active"])
    .Type(["simple"])
    .Limit(20));
```

## Champs additionnels

Les modèles acceptent des propriétés additionnelles avec `AdditionalProperties`.
Cela permet de rester compatible avec une évolution du Swagger sans bloquer l'intégration.

```csharp
var request = new CreateTransactionRequest
{
    Name = "Contrat client",
    AdditionalProperties = new Dictionary<string, object?>
    {
        ["customField"] = "value"
    }
};
```

## Erreurs

Les erreurs API lèvent `DamanesignException` avec `StatusCode` et `ResponseBody`.

```csharp
try
{
    await client.CreateTransactionAsync(new CreateTransactionRequest { Name = "Invalid" });
}
catch (DamanesignException exception)
{
    Console.Error.WriteLine($"{exception.StatusCode}: {exception.ResponseBody}");
}
```
