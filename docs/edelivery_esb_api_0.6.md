# BLOBS

## Endpoint URL

```
GET /api/v1/blobs
```

## Description

Връща списък с блобове от хранилище

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Query Parameters

- `offset` (Type: `integer`, Optional): Начало на резултатите, които да бъдат върнати.
- `limit` (Type: `integer`, Optional): Брой резултати, които да бъдат върнати.

## Response

### HTTP Status Code: 200 OK

```json
{
  "result": [
    {
      "blobId": 0,
      "fileName": "string",
      "size": 0,
      "documentRegistrationNumber": "string",
      "isMalicious": true,
      "hash": "string",
      "hashAlgorithm": "string",
      "downloadLink": "string",
      "downloadLinkExpirationDate": "2023-11-29T06:41:37.967",
      "signatures": [
        {
          "coversDocument": true,
          "signDate": "2023-11-29T06:41:37.967",
          "isTimestamp": true,
          "validAtTimeOfSigning": true,
          "issuer": "string",
          "subject": "string",
          "validFrom": "2023-11-29T06:41:37.967",
          "validTo": "2023-11-29T06:41:37.967"
        }
      ]
    }
  ],
  "length": 0
}
```

### Response Body Parameters

- `result` (Type: `array of objects`): Списък блобове
  - `blobId` (Type: `integer`): Идентификатор на блоба
  - `fileName` (Type: `string`): Наименование на блоб
  - `size` (Type: `long`): Големина на блоб в байтове
  - `documentRegistrationNumber` (Type: `string`): Рег. номер на блоб
  - `isMalicious` (Type: `bool`): Флаг за проверка за зловреден код
  - `hash` (Type: `string`): Хаш на блоб
  - `hashAlgorithm` (Type: `string`): Алгоритъм за определяне на хаш
  - `downloadLink` (Type: `string`): Линк за сваляне на блоб
  - `downloadLinkExpirationDate` (Type: `datetime ISO 8601`): Време, до което линка за сваляне на блоба е активен
  - `signatures` (Type: `array of objects`): Обекти с данни за ел. подписи в блоба
    - `coversDocument` (Type: `bool`): TBA
    - `signDate` (Type: `datetime ISO 8601`): TBA
    - `isTimestamp` (Type: `bool`): TBA
    - `validAtTimeOfSigning` (Type: `bool`): TBA
    - `issuer` (Type: `string`): TBA
    - `subject` (Type: `string`): TBA
    - `validFrom` (Type: `datetime ISO 8601`): TBA
    - `validTo` (Type: `datetime ISO 8601`): TBA
- `length` (Type: `interger`): Общ брой блобове

### Unauthorized (HTTP Status Code: 401)

## Example

```bash
curl -X GET 'https://localhost:5501/api/v1/blobs?offset=0&limit=100' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:'
```

## Endpoint URL

```
POST /api/v1/blobs
```

## Description

Качване на единичен блоб в хранилището

## Request

### Headers

- `Content-Type` (Required): multipart/form-data; boundary=[...]
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Query Parameters

- `type` (Type: `ProfileBlobAccessKeyType`, Required): Тип на блоб. Възможни стойности:
  - `TEMPORARY` - временен, може да се използва до определено време за прикачване към съобщение, но ще бъде автоматично изтрит, ако не се използва
  - `STORAGE` - блобът се добавя в хранилището на профила за постоянно

```cs
enum ProfileBlobAccessKeyType {
  "TEMPORARY" = 0,
  "STORAGE" = 1,
}
```

### Request Body

Съдържа една единствена част за блоба, който ще бъде качен

- **Type**: Вид на блоба (примерно `image/jpeg`, `application/pdf`, `application/xml`, `application/json`, `text/plain`, ...)
- **Description**: Съдържанието на качения блоб

## Response

### HTTP Status Code: 200 OK

```json
{
  "name": "string",
  "size": 0,
  "hashAlgorithm": "string",
  "hash": "string",
  "blobId": 0,
  "malwareScanStatus": 1,
  "blobSignatureStatus": 0,
  "errorStatus": 0
}
```

### Response Body Parameters

- `name` (Type: `string`, Required): Наименование на блоб
- `size` (Type: `long`, Required): Големина на блоб в байтове
- `hashAlgorithm` (Type: `string`, Optional): Алгоритъм за определяне на хаш
- `hash` (Type: `string`, Optional): Хаш на блоб
- `blobId` (Type: `integer`, Optional): Идентификатор на блоба
- `malwareScanStatus` (Type: `MalwareScanStatus`, Required): Статус на блоба относно зловреден код
- `blobSignatureStatus` (Type: `BlobSignatureStatus`, Required): Статус на блоба относно електронните подпис
- `errorStatus` (Type: `ErrorStatus`, Required): Статус на блоба относно грешки

```cs
enum MalwareScanStatus {
  "NONE" = 1,
  "NOTMALICIOUS" = 2,
  "NOTSURE" = 3,
  "ISMALICIOUS" = 4,
}

enum BlobSignatureStatus {
  "NONE" = 0,
  "VALID" = 1,
  "INVALIDINTEGRITY" = 2,
  "CERTIFICATEEXPIREDATTIMEOFSIGNING" = 3,
  "INVALIDCERTIFICATE" = 4,
}

enum ErrorStatus {
  "NONE" = 0,
  "INSUFFICIENTSTORAGESPACE" = 1,
}
```

### Unauthorized (HTTP Status Code: 401)

## Example

```http
POST /api/v1/blobs?type=Storage HTTP/1.1
Host: localhost:5101
Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.292.1.2/CN=test.client.forms|representedPersonID:|correspondentOID:|operatorID:
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW
Content-Length: 272

------WebKitFormBoundary7MA4YWxkTrZu0gW
Content-Disposition: form-data; name=""; filename="test.pdf"
Content-Type: application/pdf

(binary data)
------WebKitFormBoundary7MA4YWxkTrZu0gW--
```

## Endpoint URL

```
GET /api/v1/blobs/{blobId}
```

## Description

Връща блоб от хранилище

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Path Parameters

- `blobId` (Type: `integer`, Required): Идентификатор на блоб.

## Response

### HTTP Status Code: 200 OK

```json
{
  "blobId": 0,
  "fileName": "string",
  "size": 0,
  "documentRegistrationNumber": "string",
  "isMalicious": true,
  "hash": "string",
  "hashAlgorithm": "string",
  "downloadLink": "string",
  "downloadLinkExpirationDate": "2023-11-29T06:41:37.967",
  "signatures": [
    {
      "coversDocument": true,
      "signDate": "2023-11-29T06:41:37.967",
      "isTimestamp": true,
      "validAtTimeOfSigning": true,
      "issuer": "string",
      "subject": "string",
      "validFrom": "2023-11-29T06:41:37.967",
      "validTo": "2023-11-29T06:41:37.967"
    }
  ]
}
```

### Response Body Parameters

- `blobId` (Type: `integer`): Идентификатор на блоба
- `fileName` (Type: `string`): Наименование на блоб
- `size` (Type: `long`): Големина на блоб в байтове
- `documentRegistrationNumber` (Type: `string`): Рег. номер на блоб
- `isMalicious` (Type: `bool`): Флаг за проверка за зловреден код
- `hash` (Type: `string`): Хаш на блоб
- `hashAlgorithm` (Type: `string`): Алгоритъм за определяне на хаш
- `downloadLink` (Type: `string`): Линк за сваляне на блоб
- `downloadLinkExpirationDate` (Type: `datetime ISO 8601`): Време, до което линка за сваляне на блоба е активен
- `signatures` (Type: `array of objects`): Обекти с данни за ел. подписи в блоба
  - `coversDocument` (Type: `bool`): TBA
  - `signDate` (Type: `datetime ISO 8601`): TBA
  - `isTimestamp` (Type: `bool`): TBA
  - `validAtTimeOfSigning` (Type: `bool`): TBA
  - `issuer` (Type: `string`): TBA
  - `subject` (Type: `string`): TBA
  - `validFrom` (Type: `datetime ISO 8601`): TBA
  - `validTo` (Type: `datetime ISO 8601`): TBA

### Unauthorized (HTTP Status Code: 401)

### Not Found (HTTP Status Code: 404)

## Example

```bash
curl -X GET 'https://localhost:5501/api/v1/blobs/1' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:'
```

## Endpoint URL

```
DELETE /api/v1/blobs/{blobId}
```

## Description

Изтрива блоб от хранилище

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Path Parameters

- `blobId` (Type: `integer`, Required): Идентификатор на блоб.

## Response

### HTTP Status Code: 200 OK

```json
Empty
```

### Unauthorized (HTTP Status Code: 401)

### Not Found (HTTP Status Code: 404)

## Example

```bash
curl -X DELETE 'https://localhost:5501/api/v1/blobs/1' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:'
```

## Endpoint URL

```
GET /api/v1/obo/blobs
```

## Description

Връща списък с блобове от хранилище от името на чужд профил

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:[...]|correspondentOID:|operatorID:
  - `representedPersonID` (Required): Идентификатор на чужд профил

### Query Parameters

- `offset` (Type: `integer`, Optional): Начало на резултатите, които да бъдат върнати.
- `limit` (Type: `integer`, Optional): Брой резултати, които да бъдат върнати.

## Response

### HTTP Status Code: 200 OK

```json
{
  "result": [
    {
      "blobId": 0,
      "fileName": "string",
      "size": 0,
      "documentRegistrationNumber": "string",
      "isMalicious": true,
      "hash": "string",
      "hashAlgorithm": "string",
      "downloadLink": "string",
      "downloadLinkExpirationDate": "2023-11-29T06:41:37.967",
      "signatures": [
        {
          "coversDocument": true,
          "signDate": "2023-11-29T06:41:37.967",
          "isTimestamp": true,
          "validAtTimeOfSigning": true,
          "issuer": "string",
          "subject": "string",
          "validFrom": "2023-11-29T06:41:37.967",
          "validTo": "2023-11-29T06:41:37.967"
        }
      ]
    }
  ],
  "length": 0
}
```

### Response Body Parameters

- `result` (Type: `array of objects`): Списък блобове
  - `blobId` (Type: `integer`): Идентификатор на блоба
  - `fileName` (Type: `string`): Наименование на блоб
  - `size` (Type: `long`): Големина на блоб в байтове
  - `documentRegistrationNumber` (Type: `string`): Рег. номер на блоб
  - `isMalicious` (Type: `bool`): Флаг за проверка за зловреден код
  - `hash` (Type: `string`): Хаш на блоб
  - `hashAlgorithm` (Type: `string`): Алгоритъм за определяне на хаш
  - `downloadLink` (Type: `string`): Линк за сваляне на блоб
  - `downloadLinkExpirationDate` (Type: `datetime ISO 8601`): Време, до което линка за сваляне на блоба е активен
  - `signatures` (Type: `array of objects`): Обекти с данни за ел. подписи в блоба
    - `coversDocument` (Type: `bool`): TBA
    - `signDate` (Type: `datetime ISO 8601`): TBA
    - `isTimestamp` (Type: `bool`): TBA
    - `validAtTimeOfSigning` (Type: `bool`): TBA
    - `issuer` (Type: `string`): TBA
    - `subject` (Type: `string`): TBA
    - `validFrom` (Type: `datetime ISO 8601`): TBA
    - `validTo` (Type: `datetime ISO 8601`): TBA
- `length` (Type: `interger`): Общ брой блобове

### Unauthorized (HTTP Status Code: 401)

### Forbidden (HTTP Status Code: 403)

## Example

```bash
curl -X GET 'https://localhost:5501/api/v1/obo/blobs?offset=0&limit=100' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:1234567890|correspondentOID:|operatorID:'
```

## Endpoint URL

```
POST /api/v1/obo/blobs
```

## Description

Качване на единичен блоб в хранилището от името на чужд профил

## Request

### Headers

- `Content-Type` (Required): multipart/form-data; boundary=[...]
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:[...]|correspondentOID:|operatorID:
  - `representedPersonID` (Required): Идентификатор на чужд профил

### Query Parameters

- `type` (Type: `ProfileBlobAccessKeyType`, Required): Тип на блоб. Възможни стойности:
  - `TEMPORARY` - временен, може да се използва до определено време за прикачване към съобщение, но ще бъде автоматично изтрит, ако не се използва
  - `STORAGE` - блобът се добавя в хранилището на профила за постоянно

```cs
enum ProfileBlobAccessKeyType {
  "TEMPORARY" = 0,
  "STORAGE" = 1,
}
```

### Request Body

Съдържа една единствена част за блоба, който ще бъде качен

- **Type**: Вид на блоба (примерно `image/jpeg`, `application/pdf`, `application/xml`, `application/json`, `text/plain`, ...)
- **Description**: Съдържанието на качения блоб

## Response

### HTTP Status Code: 200 OK

```json
{
  "name": "string",
  "size": 0,
  "hashAlgorithm": "string",
  "hash": "string",
  "blobId": 0,
  "malwareScanStatus": 1,
  "blobSignatureStatus": 0,
  "errorStatus": 0
}
```

### Response Body Parameters

- `name` (Type: `string`, Required): Наименование на блоб
- `size` (Type: `long`, Required): Големина на блоб в байтове
- `hashAlgorithm` (Type: `string`, Optional): Алгоритъм за определяне на хаш
- `hash` (Type: `string`, Optional): Хаш на блоб
- `blobId` (Type: `integer`, Optional): Идентификатор на блоба
- `malwareScanStatus` (Type: `MalwareScanStatus`, Required): Статус на блоба относно зловреден код
- `blobSignatureStatus` (Type: `BlobSignatureStatus`, Required): Статус на блоба относно електронните подпис
- `errorStatus` (Type: `ErrorStatus`, Required): Статус на блоба относно грешки

```cs
enum MalwareScanStatus {
  "NONE" = 1,
  "NOTMALICIOUS" = 2,
  "NOTSURE" = 3,
  "ISMALICIOUS" = 4,
}

enum BlobSignatureStatus {
  "NONE" = 0,
  "VALID" = 1,
  "INVALIDINTEGRITY" = 2,
  "CERTIFICATEEXPIREDATTIMEOFSIGNING" = 3,
  "INVALIDCERTIFICATE" = 4,
}

enum ErrorStatus {
  "NONE" = 0,
  "INSUFFICIENTSTORAGESPACE" = 1,
}
```

### Unauthorized (HTTP Status Code: 401)

### Forbidden (HTTP Status Code: 403)

## Example

```http
POST /api/v1/obo/blobs?type=Storage HTTP/1.1
Host: localhost:5101
Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.292.1.2/CN=test.client.forms|representedPersonID:|correspondentOID:|operatorID:
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW
Content-Length: 272

------WebKitFormBoundary7MA4YWxkTrZu0gW
Content-Disposition: form-data; name=""; filename="test.pdf"
Content-Type: application/pdf

(binary data)
------WebKitFormBoundary7MA4YWxkTrZu0gW--
```

## Endpoint URL

```
GET /api/v1/obo/blobs/{blobId}
```

## Description

Връща блоб от хранилище от името на чужд профил

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:[...]|correspondentOID:|operatorID:
  - `representedPersonID` (Required): Идентификатор на чужд профил

### Path Parameters

- `blobId` (Type: `integer`, Required): Идентификатор на блоб.

## Response

### HTTP Status Code: 200 OK

```json
{
  "blobId": 0,
  "fileName": "string",
  "size": 0,
  "documentRegistrationNumber": "string",
  "isMalicious": true,
  "hash": "string",
  "hashAlgorithm": "string",
  "downloadLink": "string",
  "downloadLinkExpirationDate": "2023-11-29T06:41:37.967",
  "signatures": [
    {
      "coversDocument": true,
      "signDate": "2023-11-29T06:41:37.967",
      "isTimestamp": true,
      "validAtTimeOfSigning": true,
      "issuer": "string",
      "subject": "string",
      "validFrom": "2023-11-29T06:41:37.967",
      "validTo": "2023-11-29T06:41:37.967"
    }
  ]
}
```

### Response Body Parameters

- `blobId` (Type: `integer`): Идентификатор на блоба
- `fileName` (Type: `string`): Наименование на блоб
- `size` (Type: `long`): Големина на блоб в байтове
- `documentRegistrationNumber` (Type: `string`): Рег. номер на блоб
- `isMalicious` (Type: `bool`): Флаг за проверка за зловреден код
- `hash` (Type: `string`): Хаш на блоб
- `hashAlgorithm` (Type: `string`): Алгоритъм за определяне на хаш
- `downloadLink` (Type: `string`): Линк за сваляне на блоб
- `downloadLinkExpirationDate` (Type: `datetime ISO 8601`): Време, до което линка за сваляне на блоба е активен
- `signatures` (Type: `array of objects`): Обекти с данни за ел. подписи в блоба
  - `coversDocument` (Type: `bool`): TBA
  - `signDate` (Type: `datetime ISO 8601`): TBA
  - `isTimestamp` (Type: `bool`): TBA
  - `validAtTimeOfSigning` (Type: `bool`): TBA
  - `issuer` (Type: `string`): TBA
  - `subject` (Type: `string`): TBA
  - `validFrom` (Type: `datetime ISO 8601`): TBA
  - `validTo` (Type: `datetime ISO 8601`): TBA

### Unauthorized (HTTP Status Code: 401)

### Forbidden (HTTP Status Code: 403)

### Not Found (HTTP Status Code: 404)

## Example

```bash
curl -X GET 'https://localhost:5501/api/v1/obo/blobs/1' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:1234567890|correspondentOID:|operatorID:'
```

## Endpoint URL

```
DELETE /api/v1/obo/blobs/{blobId}
```

## Description

Изтрива блоб от хранилище от името на чужд профил

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:[...]|correspondentOID:|operatorID:
  - `representedPersonID` (Required): Идентификатор на чужд профил

### Path Parameters

- `blobId` (Type: `integer`, Required): Идентификатор на блоб.

## Response

### HTTP Status Code: 200 OK

```json
Empty
```

### Unauthorized (HTTP Status Code: 401)

### Forbidden (HTTP Status Code: 403)

### Not Found (HTTP Status Code: 404)

## Example

```bash
curl -X DELETE 'https://localhost:5501/api/v1/obo/blobs/1' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:1234567890|correspondentOID:|operatorID:'
```

# COUNTRIES

## Endpoint URL

```
GET /api/v1/countries
```

## Description

Връща списък с всички страни като номенклатура

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Query Parameters

- `offset` (Type: `integer`, Optional): Начало на резултатите, които да бъдат върнати.
- `limit` (Type: `integer`, Optional): Брой резултати, които да бъдат върнати.

## Response

### HTTP Status Code: 200 OK

```json
{
  "result": [
    {
      "iso": "string",
      "name": "string"
    }
  ],
  "length": 0
}
```

### Response Body Parameters

- `result` (Type: `array of objects`): Списък страни
  - `iso` (Type: `string`): Идентификатор на страна (ISO-2)
  - `name` (Type: `string`): Наименование на страна
- `length` (Type: `interger`): Общ брой страни

### Unauthorized (HTTP Status Code: 401)

## Example

```bash
curl -X GET 'https://localhost:5501/api/v1/countries?offset=0&limit=100' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:'
```

# TARGETGROUPS

## Endpoint URL

```
GET /api/v1/target-groups
```

## Description

Връща списък с всички активни целеви групи

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

## Response

### HTTP Status Code: 200 OK

```json
[
  {
    "targetGroupId": 0,
    "name": "string",
    "canSelectRecipients": true
  }
]
```

### Response Body Parameters

-
- (Type: `array of objects`): Списък целеви групи
  - `targetGroupId` (Type: `integer`): Идентификатор на целева група
  - `name` (Type: `string`): Наименование на целева група
  - `canSelectRecipients` (Type: `bool`): Възможност за преглед на профили от целевата група и възможност за избиране на получатели от нея

### Unauthorized (HTTP Status Code: 401)

## Example

```bash
curl -X GET 'https://localhost:5501/api/v1/target-groups' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:'
```

# TEMPLATES

## Endpoint URL

```
GET /api/v1/templates
```

## Description

Връща списък с шаблоните на съобщението, до които профила има достъп

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

## Response

### HTTP Status Code: 200 OK

```json
[
  {
    "templateId": 0,
    "name": "string",
    "identityNumber": "string",
    "read": 1,
    "write": 1,
    "responseTemplateId": 0
  }
]
```

### Response Body Parameters

- (Type: `array of objects`): Списък шаблони
  - `templateId` (Type: `integer`): Идентификатор на шаблон
  - `name` (Type: `string`): Наименование на шаблон
  - `identityNumber` (Type: `string`): Номер на шаблон
  - `read` (Type: `TemplateSecurityLevel`): Мин. ниво на осигуреност нужно за получаване на съобщение със съответния шаблон
  - `write` (Type: `TemplateSecurityLevel`): Мин. ниво на осигуреност нужно за изпращане на съобщение със съответния шаблон
  - `responseTemplateId` (Type: `string`, Optional): Шаблон на съобщение при отговор

```cs
enum TemplateSecurityLevel {
  "LOW" = 1,
  "MEDIUM" = 2,
  "HIGH" = 3,
}
```

### Unauthorized (HTTP Status Code: 401)

## Example

```bash
curl -X GET 'https://localhost:5501/api/v1/templates' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:'
```

## Endpoint URL

```
GET /api/v1/templates/{templateId}
```

## Description

Връща данните за шаблон на съобщение

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Path Parameters

- `templateId` (Type: `integer`, Required): Идентификатор на шаблон.

## Response

### HTTP Status Code: 200 OK

```json
{
  "templateId": 0,
  "name": "string",
  "identityNumber": "string",
  "read": 1,
  "write": 1,
  "content": [
    {
      "id": "string",
      "label": "string",
      "type": "hidden",
      "isEncrypted": true,
      "isRequired": true,
      "discriminator": "string"
    }
  ],
  "responseTemplateId": 0
}
```

### Response Body Parameters

- `templateId` (Type: `integer`): Идентификатор на шаблон
- `name` (Type: `string`): Наименование на шаблон
- `identityNumber` (Type: `string`): Номер на шаблон
- `read` (Type: `TemplateSecurityLevel`): Мин. ниво на осигуреност нужно за получаване на съобщение със съответния шаблон
- `write` (Type: `TemplateSecurityLevel`): Мин. ниво на осигуреност нужно за изпращане на съобщение със съответния шаблон
- `content` (Type: `array of objects`): Списък с полетата в шаблона
  - `id` (Type: `guid`): Идентификатор на поле
  - `label` (Type: `string`): Наименование на поле
  - `type` (Type: `ComponentType`): Тип на поле
  - `isEncrypted` (Type: `bool`): Дали данните от полето се криптират
  - `isRequired` (Type: `bool`): Дали полето е задължително за попълване
  - `discriminator` (Type: `string`): Тип на поле | определител
- `responseTemplateId` (Type: `string`, Optional): Шаблон на съобщение при отговор

```cs
enum TemplateSecurityLevel {
  "LOW" = 1,
  "MEDIUM" = 2,
  "HIGH" = 3,
}

enum ComponentType {
  hidden,
  textfield,
  textarea,
  datetime,
  checkbox,
  select,
  file,
  markdown,
}
```

_todo: list of all possible template fields_

### Unauthorized (HTTP Status Code: 401)

### Forbidden (HTTP Status Code: 403)

### Not Found (HTTP Status Code: 404)

## Example

```bash
curl -X GET 'https://localhost:5501/api/v1/templates/1' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:'
```

## Endpoint URL

```
GET /api/v1/templates
```

## Description

Връща списък с шаблоните на съобщението от името на профил, до които профила има достъп

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:[...]|correspondentOID:|operatorID:
  - `representedPersonID` (Required): Идентификатор на чужд профил

## Response

### HTTP Status Code: 200 OK

```json
[
  {
    "templateId": 0,
    "name": "string",
    "identityNumber": "string",
    "read": 1,
    "write": 1,
    "responseTemplateId": 0
  }
]
```

### Response Body Parameters

- (Type: `array of objects`): Списък шаблони
  - `templateId` (Type: `integer`): Идентификатор на шаблон
  - `name` (Type: `string`): Наименование на шаблон
  - `identityNumber` (Type: `string`): Номер на шаблон
  - `read` (Type: `TemplateSecurityLevel`): Мин. ниво на осигуреност нужно за получаване на съобщение със съответния шаблон
  - `write` (Type: `TemplateSecurityLevel`): Мин. ниво на осигуреност нужно за изпращане на съобщение със съответния шаблон
  - `responseTemplateId` (Type: `string`, Optional): Шаблон на съобщение при отговор

```cs
enum TemplateSecurityLevel {
  "LOW" = 1,
  "MEDIUM" = 2,
  "HIGH" = 3,
}
```

### Unauthorized (HTTP Status Code: 401)

## Example

```bash
curl -X GET 'https://localhost:5501/api/v1/obo/templates' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:1234567890|correspondentOID:|operatorID:'
```

## Endpoint URL

```
GET /api/v1/templates/{templateId}
```

## Description

Връща данните за шаблон на съобщение от името на профил

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:[...]|correspondentOID:|operatorID:
  - `representedPersonID` (Required): Идентификатор на чужд профил

### Path Parameters

- `templateId` (Type: `integer`, Required): Идентификатор на шаблон.

## Response

### HTTP Status Code: 200 OK

```json
{
  "templateId": 0,
  "name": "string",
  "identityNumber": "string",
  "read": 1,
  "write": 1,
  "content": [
    {
      "id": "string",
      "label": "string",
      "type": "hidden",
      "isEncrypted": true,
      "isRequired": true,
      "discriminator": "string"
    }
  ],
  "responseTemplateId": 0
}
```

### Response Body Parameters

- `templateId` (Type: `integer`): Идентификатор на шаблон
- `name` (Type: `string`): Наименование на шаблон
- `identityNumber` (Type: `string`): Номер на шаблон
- `read` (Type: `TemplateSecurityLevel`): Мин. ниво на осигуреност нужно за получаване на съобщение със съответния шаблон
- `write` (Type: `TemplateSecurityLevel`): Мин. ниво на осигуреност нужно за изпращане на съобщение със съответния шаблон
- `content` (Type: `array of objects`): Списък с полетата в шаблона
  - `id` (Type: `guid`): Идентификатор на поле
  - `label` (Type: `string`): Наименование на поле
  - `type` (Type: `ComponentType`): Тип на поле
  - `isEncrypted` (Type: `bool`): Дали данните от полето се криптират
  - `isRequired` (Type: `bool`): Дали полето е задължително за попълване
  - `discriminator` (Type: `string`): Тип на поле | определител
- `responseTemplateId` (Type: `string`, Optional): Шаблон на съобщение при отговор

```cs
enum TemplateSecurityLevel {
  "LOW" = 1,
  "MEDIUM" = 2,
  "HIGH" = 3,
}

enum ComponentType {
  hidden,
  textfield,
  textarea,
  datetime,
  checkbox,
  select,
  file,
  markdown,
}
```

_todo: list of all possible template fields_

### Unauthorized (HTTP Status Code: 401)

### Forbidden (HTTP Status Code: 403)

### Not Found (HTTP Status Code: 404)

## Example

```bash
curl -X GET 'https://localhost:5501/api/v1/obo/templates/1' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:1234567890|correspondentOID:|operatorID:'
```

# STATISTICS

## Endpoint URL

```
GET /api/v1/stats/messages/sent
```

## Description

Връща статистика за изпратените съобщения за период

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Query Parameters

- `from` (Type: `datetime ISO 8601`, Required): От дата във формат `yyyy-MM-dd`
- `to` (Type: `datetime ISO 8601`, Required): До дата във формат `yyyy-MM-dd`

## Response

### HTTP Status Code: 200 OK

```json
{
  "value": 0
}
```

### Response Body Parameters

- `value` (Type: `integer`): Брой изпратени съобщения

## Example

```bash
curl -X GET 'https://localhost:5501/api/v1/stats/messages/sent?from=2021-01-01&to=2021-12-01' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:'
```

## Endpoint URL

```
GET /api/v1/stats/messages/received
```

## Description

Връща статистика за получените съобщения за период

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Query Parameters

- `from` (Type: `datetime ISO 8601`, Required): От дата във формат `yyyy-MM-dd`
- `to` (Type: `datetime ISO 8601`, Required): До дата във формат `yyyy-MM-dd`

## Response

### HTTP Status Code: 200 OK

```json
{
  "value": 0
}
```

### Response Body Parameters

- `value` (Type: `integer`): Брой получени съобщения

## Example

```bash
curl -X GET 'https://localhost:5501/api/v1/stats/messages/received?from=2021-01-01&to=2021-12-01' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:'
```

## Endpoint URL

```
GET /api/v1/stats/messages/sent-by-month
```

## Description

Връща статистика за изпратените съобщения по месеци или конкретен месец

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Query Parameters

- `month` (Type: `datetime ISO 8601`, Optional): От дата във формат `yyyy-MM`

## Response

### HTTP Status Code: 200 OK

```json
[
  {
    "month": "string",
    "value": 0
  }
]
```

### Response Body Parameters

- (Type: `array of objects`): Списък статистически данни за месец
  - `month` (Type: `string`): Месец във формат `yyyy-MM`
  - `value` (Type: `integer`): Брой изпратени съобщения за месеца

## Example

```bash
# за всички месеци
curl -X GET 'https://localhost:5501/api/v1/stats/messages/sent-by-month' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:'
```

```bash
# за конкретен месеци
curl -X GET 'https://localhost:5501/api/v1/stats/messages/sent-by-month?month=2021-01' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:'
```

## Endpoint URL

```
GET /api/v1/stats/messages/received-by-month
```

## Description

Връща статистика за получените съобщения по месеци или конкретен месец

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Query Parameters

- `month` (Type: `datetime ISO 8601`, Optional): От дата във формат `yyyy-MM`

## Response

### HTTP Status Code: 200 OK

```json
[
  {
    "month": "string",
    "value": 0
  }
]
```

### Response Body Parameters

- (Type: `array of objects`): Списък статистически данни за месец
  - `month` (Type: `string`): Месец във формат `yyyy-MM`
  - `value` (Type: `integer`): Брой получени съобщения за месеца

## Example

```bash
# за всички месеци
curl -X GET 'https://localhost:5501/api/v1/stats/messages/received-by-month' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:'
```

```bash
# за конкретен месеци
curl -X GET 'https://localhost:5501/api/v1/stats/messages/received-by-month?month=2021-01' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:'
```

# PROFILES

## Endpoint URL

```
GET /api/v1/profiles
```

## Description

Връща списък с активните профили в целева група

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Query Parameters

- `targetGroupId` (Type: `integer`, Required): Идентификатор на целева група.
- `offset` (Type: `integer`, Optional): Начало на резултатите, които да бъдат върнати.
- `limit` (Type: `integer`, Optional): Брой резултати, които да бъдат върнати.

## Response

### HTTP Status Code: 200 OK

```json
{
  "result": [
    {
      "profileId": 0,
      "identifier": "string",
      "name": "string",
      "email": "string",
      "phone": "string"
    }
  ],
  "length": 0
}
```

### Response Body Parameters

- `result` (Type: `array of objects`): Списък профили
  - `profileId` (Type: `integer`): Публичен идентификатор на профил
  - `identifier` (Type: `string`): Идентификатор на профил (ЕГН/ЛНЧ/ЕИК/...)
  - `name` (Type: `string`): Наименование на профил
  - `email` (Type: `string`): Имейл на профил
  - `phone` (Type: `string`): Телефон на профил
- `length` (Type: `interger`): Общ брой профили

### Unauthorized (HTTP Status Code: 401)

### Forbidden (HTTP Status Code: 403)

## Example

```bash
curl -X GET 'https://localhost:5501/api/v1/profiles?targetGroupId=1&offset=0&limit=100' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:'
```

## Endpoint URL

```
GET /api/v1/profiles/{profileId}
```

## Description

Връща информация за профил

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Path Parameters

- `profileId` (Type: `integer`, Required): Идентификатор на профил.

## Response

### HTTP Status Code: 200 OK

```json
{
  "profileId": 0,
  "identifier": "string",
  "name": "string",
  "email": "string",
  "phone": "string",
  "address": {
    "addressId": 0,
    "residence": "string",
    "city": "string",
    "state": "string",
    "countryIso": "string"
  }
}
```

### Response Body Parameters

- `profileId` (Type: `integer`): Публичен идентификатор на профил
- `identifier` (Type: `string`): Идентификатор на профил (ЕГН/ЛНЧ/ЕИК/...)
- `name` (Type: `string`): Наименование на профил
- `email` (Type: `string`): Имейл на профил
- `phone` (Type: `string`): Телефон на профил
- `address` (Type: `object`, Optional): Адрес на профил
  - `addressId` (Type: `integer`): Публичен идентификатор на адрес
  - `residence` (Type: `string`): Адрес
  - `city` (Type: `string`): Наименование на град
  - `state` (Type: `string`): Наименование на община
  - `countryIso` (Type: `string`): Код на страна

### Unauthorized (HTTP Status Code: 401)

### Forbidden (HTTP Status Code: 403)

### Not Found (HTTP Status Code: 404)

## Example

```bash
curl -X GET 'https://localhost:5501/api/v1/profiles/1' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:'
```

## Endpoint URL

```
GET /api/v1/profiles/search
```

## Description

Търсене на профил, който може да получава съобщения, по даден идентификатор и шаблон на съобщението

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Query Parameters

- `identifier` (Type: `integer`, Required): Идентификатор на профил (ЕГН/ЛНЧ/ЕИК/...).
- `templateId` (Type: `integer`, Optional): Идентификатор на шаблон на съобщение.
- `targetGroupId` (Type: `integer`, Required): Идентификатор на целева група.

## Response

### HTTP Status Code: 200 OK

```json
{
  "profileId": 0,
  "identifier": "string",
  "name": "string",
  "email": "string",
  "phone": "string"
}
```

### Response Body Parameters

- `profileId` (Type: `integer`): Публичен идентификатор на профил
- `identifier` (Type: `string`): Идентификатор на профил (ЕГН/ЛНЧ/ЕИК/...)
- `name` (Type: `string`): Наименование на профил
- `email` (Type: `string`): Имейл на профил
- `phone` (Type: `string`): Телефон на профил

### Unauthorized (HTTP Status Code: 401)

### Forbidden (HTTP Status Code: 403)

### Not Found (HTTP Status Code: 404)

## Example

```bash
curl -X GET 'https://localhost:5501/api/v1/profiles/search?identifier=1234567890&templateId=1&targetGroupId=1&' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:'
```

## Endpoint URL

```
GET /api/v1/profiles/check
```

## Description

Проверка на регистрирани профили. Връща списък от профили в различните целеви групи, отговарящи на критериите.

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Query Parameters

- `identifier` (Type: `integer`, Required): Идентификатор на профил (ЕГН/ЛНЧ/ЕИК/...).
- `offset` (Type: `integer`, Optional): Начало на резултатите, които да бъдат върнати.
- `limit` (Type: `integer`, Optional): Брой резултати, които да бъдат върнати.

## Response

### HTTP Status Code: 200 OK

```json
{
  "result": [
    {
      "profileId": 0,
      "identifier": "string",
      "name": "string",
      "email": "string",
      "phone": "string",
      "address": {
        "addressId": 0,
        "residence": "string",
        "city": "string",
        "state": "string",
        "countryIso": "string"
      },
      "isActivated": true,
      "isPassive": true,
      "targetGroupId": 0,
      "targetGroupName": "string"
    }
  ],
  "length": 0
}
```

### Response Body Parameters

- `result` (Type: `array of objects`): Списък профили
  - `profileId` (Type: `integer`): Публичен идентификатор на профил
  - `identifier` (Type: `string`): Идентификатор на профил (ЕГН/ЛНЧ/ЕИК/...)
  - `name` (Type: `string`): Наименование на профил
  - `email` (Type: `string`): Имейл на профил
  - `phone` (Type: `string`): Телефон на профил
    `address` (Type: `object`, Optional): Адрес на профил
    - `addressId` (Type: `integer`): Публичен идентификатор на адрес
    - `residence` (Type: `string`): Адрес
    - `city` (Type: `string`): Наименование на град
    - `state` (Type: `string`): Наименование на община
    - `countryIso` (Type: `string`): Код на страна
  - `isActivated` (Type: `bool`): >Флаг, дали профилът е активиран
  - `isPassive` (Type: `bool`): Флаг, дали профилът е пасивен. ВАЖИ САМО ЗА ПРОФИЛИ НА ФИЗИЧЕСКИ ЛИЦА
  - `targetGroupId` (Type: `integer`): Идентификатор на целева група
  - `targetGroupName` (Type: `string`): Наименование на целева група
- `length` (Type: `interger`): Общ брой профили

### Unauthorized (HTTP Status Code: 401)

### Forbidden (HTTP Status Code: 403)

## Example

```bash
curl -X GET 'https://localhost:5501/api/v1/profiles/check?identifier=1234567890&offset=0&limit=100' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:'
```

## Endpoint URL

```
POST /api/v1/profiles/passive-individual
```

## Description

Пасивна регистрация на профил на ФЛ, с цел получаване на съобщения и нотификации

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Request Body

```json
{
  "identifier": "string",
  "firstName": "string",
  "middleName": "string",
  "lastName": "string",
  "email": "string",
  "phone": "string",
  "address": {
    "residence": "string",
    "city": "string",
    "state": "string",
    "countryIso": "string"
  }
}
```

### Request Body Parameters

- `identifier` (Type: `string`, Required): Идентификатор на профил (ЕГН/ЛНЧ/ЕИК/...)
- `firstName` (Type: `string`, Required): Собствено име
- `middleName` (Type: `string`, Required): Бащино име
- `lastName` (Type: `string`, Required): Фамилия
- `email` (Type: `string`, Required): Имейл на профил
- `phone` (Type: `string`, Required): Телефон на профил
- `address` (Type: `object`, Optional): Адрес на профил
  - `residence` (Type: `string`, Required): Адрес
  - `city` (Type: `string`, Optional): Наименование на град
  - `state` (Type: `string`, Optional): Наименование на община
  - `countryIso` (Type: `string`, Optional): Код на страна

## Response

### HTTP Status Code: 200 OK

```json
1
```

### Response Body Parameters

- (Type: `integer`): Публичен идентификатор на регистрирания профил ФЛ

### Bad Request (HTTP Status Code: 400)

### Unauthorized (HTTP Status Code: 401)

### Forbidden (HTTP Status Code: 403)

## Example

```bash
curl -X POST 'https://localhost:5501/api/v1/profiles/passive-individual' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:' \
-d '{
  "identifier": "1234567890",
  "firstName": "Иван",
  "middleName": "Иванов",
  "lastName": "Иванов",
  "email": "test@abv.bg",
  "phone": "359888000001",
  "address": {
    "residence": "София",
    "city": "София",
    "state": "София",
    "countryIso": "BG"
  }
}'
```

## Endpoint URL

```
GET /api/v1/obo/profiles/search
```

## Description

Търсене на профил, който може да получава съобщения, по даден идентификатор и шаблон на съобщението, от името на чужд профил

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:[...]|correspondentOID:|operatorID:
  - `representedPersonID` (Required): Идентификатор на чужд профил

### Query Parameters

- `identifier` (Type: `integer`, Required): Идентификатор на профил (ЕГН/ЛНЧ/ЕИК/...).
- `templateId` (Type: `integer`, Optional): Идентификатор на шаблон на съобщение.
- `targetGroupId` (Type: `integer`, Required): Идентификатор на целева група.

## Response

### HTTP Status Code: 200 OK

```json
{
  "profileId": 0,
  "identifier": "string",
  "name": "string",
  "email": "string",
  "phone": "string"
}
```

### Response Body Parameters

- `profileId` (Type: `integer`): Публичен идентификатор на профил
- `identifier` (Type: `string`): Идентификатор на профил (ЕГН/ЛНЧ/ЕИК/...)
- `name` (Type: `string`): Наименование на профил
- `email` (Type: `string`): Имейл на профил
- `phone` (Type: `string`): Телефон на профил

### Unauthorized (HTTP Status Code: 401)

### Forbidden (HTTP Status Code: 403)

### Not Found (HTTP Status Code: 404)

## Example

```bash
curl -X GET 'https://localhost:5501/api/v1/obo/profiles/search?identifier=1234567890&templateId=1&targetGroupId=1&' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:1234567890|correspondentOID:|operatorID:'
```

# MESSAGES

## Endpoint URL

```
GET /api/v1/messages/inbox
```

## Description

Връща списък с получени съобщения

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Query Parameters

- `from` (Type: `datetime ISO 8601`, Optional): От дата във формат `yyyy-MM-dd`
- `to` (Type: `datetime ISO 8601`, Optional): До дата във формат `yyyy-MM-dd`
- `templateId` (Type: `integer`, Optional): Идентификатор на шаблон на съобщение
- `offset` (Type: `integer`, Optional): Начало на резултатите, които да бъдат върнати.
- `limit` (Type: `integer`, Optional): Брой резултати, които да бъдат върнати.

## Response

### HTTP Status Code: 200 OK

```json
{
  "result": [
    {
      "messageId": 0,
      "dateSent": "2023-11-29T11:32:14.392",
      "dateReceived": "2023-11-29T11:32:14.392",
      "subject": "string",
      "senderProfileName": "string",
      "senderLoginName": "string",
      "recipientProfileName": "string",
      "recipientLoginName": "string",
      "url": "string",
      "rnu": "string",
      "templateId": 0
    }
  ],
  "length": 0
}
```

### Response Body Parameters

- `result` (Type: `array of objects`): Списък съобщения
  - `messageId` (Type: `integer`): Идентификатор на съобщение
  - `dateSent` (Type: `datetime ISO 8601`): Дата на изпращане
  - `dateReceived` (Type: `datetime ISO 8601`, Optional): Дата получаване/отваряне
  - `subject` (Type: `string`): Заглавие на съобщение
  - `senderProfileName` (Type: `string`): Наименование на профила изпращач
  - `senderLoginName` (Type: `string`): Наименование на потребител, изпратил съобщението
  - `recipientProfileName` (Type: `string`): Наименование на профила получател
  - `recipientLoginName` (Type: `string`): Наименование на потребител, получил съобщението
  - `url` (Type: `string`): Url за получаване/отваряне на съобщението в ССЕВ
  - `rnu` (Type: `string`, Optional): Референтен номер на услуга (РНУ)
  - `templateId` (Type: `integer`): Идентификатор на шаблон на съобщение
- `length` (Type: `interger`): Общ брой съобщения

### Unauthorized (HTTP Status Code: 401)

## Example

```bash
curl -X GET 'https://localhost:5501/api/v1/messages/inbox?from=2021-01-01&to=2021-01-31&offset=0&limit=100' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:'
```

## Endpoint URL

```
GET /api/v1/messages/outbox
```

## Description

Връща списък с изпратени съобщения

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Query Parameters

- `from` (Type: `datetime ISO 8601`, Optional): От дата във формат `yyyy-MM-dd`
- `to` (Type: `datetime ISO 8601`, Optional): До дата във формат `yyyy-MM-dd`
- `templateId` (Type: `integer`, Optional): Идентификатор на шаблон на съобщение
- `offset` (Type: `integer`, Optional): Начало на резултатите, които да бъдат върнати.
- `limit` (Type: `integer`, Optional): Брой резултати, които да бъдат върнати.

## Response

### HTTP Status Code: 200 OK

```json
{
  "result": [
    {
      "messageId": 0,
      "dateSent": "2023-11-29T11:32:14.392",
      "dateReceived": "2023-11-29T11:32:14.392",
      "subject": "string",
      "senderProfileName": "string",
      "senderLoginName": "string",
      "recipientProfileName": "string",
      "recipientLoginName": "string",
      "url": "string",
      "rnu": "string",
      "templateId": 0
    }
  ],
  "length": 0
}
```

### Response Body Parameters

- `result` (Type: `array of objects`): Списък съобщения
  - `messageId` (Type: `integer`): Идентификатор на съобщение
  - `dateSent` (Type: `datetime ISO 8601`): Дата на изпращане
  - `subject` (Type: `string`): Заглавие на съобщение
  - `senderProfileName` (Type: `string`): Наименование на профила изпращач
  - `senderLoginName` (Type: `string`): Наименование на потребител, изпратил съобщението
  - `recipients` (Type: `string`): Получатели като текст
  - `url` (Type: `string`): Url за преглед на съобщението в ССЕВ
  - `rnu` (Type: `string`, Optional): Референтен номер на услуга (РНУ)
  - `templateId` (Type: `integer`): Идентификатор на шаблон на съобщение
- `length` (Type: `interger`): Общ брой съобщения

### Unauthorized (HTTP Status Code: 401)

## Example

```bash
curl -X GET 'https://localhost:5501/api/v1/messages/outbox?from=2021-01-01&to=2021-01-31&offset=0&limit=100' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:'
```

## Endpoint URL

```
POST /api/v1/messages
```

## Description

Изпращане на съобщение

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Request Body

```json
{
  "recipientProfileIds": [0, 1, 2],
  "subject": "string",
  "rnu": "string",
  "templateId": 0,
  "fields": {
    "guid1": "string",
    "guid2": "string",
    "guid3": "string"
  }
}
```

### Request Body Parameters

- `recipientProfileIds` (Type: `array of integers`, Required): Списък идентификатори на получатели (ЕГН/ЛНЧ/ЕИК/...)
- `subject` (Type: `string`, Required): Заглавие на съобщението
- `rnu` (Type: `string`, Optional): Референтен номер на услуга (РНУ)
- `templateId` (Type: `string`, Required): Идентификатор на шаблон на съобщението
- `fields` (Type: `object`, Optional): Списък с полетата и техните стойности в шаблона на съобщението във формат (Идентификатор на поле: Стойност)
  - `guid1` (Type: `guid`, Required): Поле в шаблона на съобщението и стойността му

## Response

### HTTP Status Code: 200 OK

```json
1
```

### Response Body Parameters

- (Type: `integer`): Идентификатор на изпратено съобщение

### Bad Request (HTTP Status Code: 400)

### Unauthorized (HTTP Status Code: 401)

### Forbidden (HTTP Status Code: 403)

## Example

```bash
curl -X POST 'https://localhost:5501/api/v1/messages' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:' \
-d '{
  "recipientProfileIds": [1],
  "subject": "Заглавие на съобщение",
  "rnu": null,
  "templateId": "1",
  "fields": {
      "179ea4dc-7879-43ad-8073-72b263915656": "Съдържание тест 1",
      "e2135802-5e34-4c60-b36e-c86d910a571a": [2]
  }
}'
```

## Endpoint URL

```
GET /api/v1/messages/{messageId}/open
```

## Description

Отваряне на получено съобщение

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Path Parameters

- `messageId` (Type: `integer`, Required): Идентификатор на съобщение.

## Response

### HTTP Status Code: 200 OK

```json
{
  "messageId": 0,
  "dateSent": "2023-11-30T20:23:20.388",
  "sender": {
    "profileId": 0,
    "name": "string"
  },
  "dateReceived": "2023-11-30T20:23:20.388",
  "recipientLogin": {
    "loginId": 0,
    "name": "string"
  },
  "subject": "string",
  "rnu": "string",
  "templateId": 0,
  "fields": {
    "guid1": "string",
    "guid2": "string",
    "guid3": "string"
  },
  "forwardedMessageId": 0
}
```

### Response Body Parameters

- `messageId` (Type: `integer`): Идентификатор на съобщение
- `dateSent` (Type: `datetime ISO 8601`): Дата на изпращане
- `sender` (Type: `object`): Обект с данни за изпращача на съобщението
  - `profileId` (Type: `integer`): Идентификатор на профил
  - `name` (Type: `string`): Наименование на профил
- `dateReceived` (Type: `datetime ISO 8601`): Дата на отваряне на съобщението
- `recipientLogin` (Type: `object`): Обект с данни за получателя на съобщението
  - `loginId` (Type: `integer`): Идентификатор на потребител
  - `name` (Type: `string`): Наименование на потребител
- `subject` (Type: `string`): Заглавие на съобщение
- `rnu` (Type: `string`, Optional): Референтен номер на услуга (РНУ)
- `templateId` (Type: `integer`): Идентификатор на шаблон на съобщение
- `fields` (Type: `object`, Optional): Списък с полетата и техните стойности в шаблона на съобщението във формат (Идентификатор на поле: Стойност)
  - `guid1` (Type: `guid`, Required): Поле в шаблона на съобщението и стойността му
- `forwardedMessageId` (Type: `integer`, Optional): Идентификатор на препратено съобщение

### Unauthorized (HTTP Status Code: 401)

### Forbidden (HTTP Status Code: 403)

## Example

```bash
curl -X GET 'https://localhost:5501/api/v1/messages/1' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:'
```

## Endpoint URL

```
GET /api/v1/messages/{messageId}/view
```

## Description

Преглед на изпратено съобщение

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Path Parameters

- `messageId` (Type: `integer`, Required): Идентификатор на съобщение.

## Response

### HTTP Status Code: 200 OK

```json
{
  "messageId": 0,
  "dateSent": "2023-11-30T20:23:20.388",
  "recipients": [
    {
      "profileId": 0,
      "name": "string",
      "dateReceived": "2023-11-30T20:29:00.954"
    }
  ],
  "subject": "string",
  "rnu": "string",
  "templateId": 0,
  "fields": {
    "guid1": "string",
    "guid2": "string",
    "guid3": "string"
  },
  "forwardedMessageId": 0
}
```

### Response Body Parameters

- `messageId` (Type: `integer`): Идентификатор на съобщение
- `dateSent` (Type: `datetime ISO 8601`): Дата на изпращане
- `recipients` (Type: `object`): Списък с данни за получателите на съобщението
  - `profileId` (Type: `integer`): Идентификатор на профил
  - `name` (Type: `string`): Наименование на профил
  - `dateReceived` (Type: `datetime ISO 8601`, Optional): Дата на отваряне на съобщението
- `subject` (Type: `string`): Заглавие на съобщение
- `rnu` (Type: `string`, Optional): Референтен номер на услуга (РНУ)
- `templateId` (Type: `integer`): Идентификатор на шаблон на съобщение
- `fields` (Type: `object`, Optional): Списък с полетата и техните стойности в шаблона на съобщението във формат (Идентификатор на поле: Стойност)
  - `guid1` (Type: `guid`, Required): Поле в шаблона на съобщението и стойността му
- `forwardedMessageId` (Type: `integer`, Optional): Идентификатор на препратено съобщение

### Unauthorized (HTTP Status Code: 401)

### Forbidden (HTTP Status Code: 403)

## Example

```bash
curl -X GET 'https://localhost:5501/api/v1/messages/1/view' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:'
```

## Endpoint URL

```
GET /api/v1/messages/{messageId}/open-forwarded/{forwardedMessageId}
```

## Description

Преглед на получено препратено съобщение

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Path Parameters

- `messageId` (Type: `integer`, Required): Идентификатор на съобщение.
- `forwardedMessageId` (Type: `integer`, Required): Идентификатор на препратено съобщение.

## Response

### HTTP Status Code: 200 OK

```json
{
  "messageId": 0,
  "dateSent": "2023-11-30T20:23:20.388",
  "sender": {
    "profileId": 0,
    "name": "string"
  },
  "dateReceived": "2023-11-30T20:23:20.388",
  "recipientLogin": {
    "loginId": 0,
    "name": "string"
  },
  "subject": "string",
  "rnu": "string",
  "templateId": 0,
  "fields": {
    "guid1": "string",
    "guid2": "string",
    "guid3": "string"
  },
  "forwardedMessageId": 0
}
```

### Response Body Parameters

- `messageId` (Type: `integer`): Идентификатор на съобщение
- `dateSent` (Type: `datetime ISO 8601`): Дата на изпращане
- `sender` (Type: `object`): Обект с данни за изпращача на съобщението
  - `profileId` (Type: `integer`): Идентификатор на профил
  - `name` (Type: `string`): Наименование на профил
- `dateReceived` (Type: `datetime ISO 8601`): Дата на отваряне на съобщението
- `recipientLogin` (Type: `object`): Обект с данни за получателя на съобщението
  - `loginId` (Type: `integer`): Идентификатор на потребител
  - `name` (Type: `string`): Наименование на потребител
- `subject` (Type: `string`): Заглавие на съобщение
- `rnu` (Type: `string`, Optional): Референтен номер на услуга (РНУ)
- `templateId` (Type: `integer`): Идентификатор на шаблон на съобщение
- `fields` (Type: `object`, Optional): Списък с полетата и техните стойности в шаблона на съобщението във формат (Идентификатор на поле: Стойност)
  - `guid1` (Type: `guid`, Required): Поле в шаблона на съобщението и стойността му
- `forwardedMessageId` (Type: `integer`, Optional): Идентификатор на препратено съобщение. Към момента винаги е `null`

### Unauthorized (HTTP Status Code: 401)

### Forbidden (HTTP Status Code: 403)

## Example

```bash
curl -X GET 'https://localhost:5501/api/v1/messages/1/open-forwarded/2' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:'
```

## Endpoint URL

```
GET /api/v1/obo/messages/inbox
```

## Description

Връща списък с получени съобщения от името на профил

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:[...]|correspondentOID:|operatorID:
  - `representedPersonID` (Required): Идентификатор на чужд профил

### Query Parameters

- `from` (Type: `datetime ISO 8601`, Optional): От дата във формат `yyyy-MM-dd`
- `to` (Type: `datetime ISO 8601`, Optional): До дата във формат `yyyy-MM-dd`
- `templateId` (Type: `integer`, Optional): Идентификатор на шаблон на съобщение
- `offset` (Type: `integer`, Optional): Начало на резултатите, които да бъдат върнати.
- `limit` (Type: `integer`, Optional): Брой резултати, които да бъдат върнати.

## Response

### HTTP Status Code: 200 OK

```json
{
  "result": [
    {
      "messageId": 0,
      "dateSent": "2023-11-29T11:32:14.392",
      "dateReceived": "2023-11-29T11:32:14.392",
      "subject": "string",
      "senderProfileName": "string",
      "senderLoginName": "string",
      "recipientProfileName": "string",
      "recipientLoginName": "string",
      "url": "string",
      "rnu": "string",
      "templateId": 0
    }
  ],
  "length": 0
}
```

### Response Body Parameters

- `result` (Type: `array of objects`): Списък съобщения
  - `messageId` (Type: `integer`): Идентификатор на съобщение
  - `dateSent` (Type: `datetime ISO 8601`): Дата на изпращане
  - `dateReceived` (Type: `datetime ISO 8601`, Optional): Дата получаване/отваряне
  - `subject` (Type: `string`): Заглавие на съобщение
  - `senderProfileName` (Type: `string`): Наименование на профила изпращач
  - `senderLoginName` (Type: `string`): Наименование на потребител, изпратил съобщението
  - `recipientProfileName` (Type: `string`): Наименование на профила получател
  - `recipientLoginName` (Type: `string`): Наименование на потребител, получил съобщението
  - `url` (Type: `string`): Url за получаване/отваряне на съобщението в ССЕВ
  - `rnu` (Type: `string`, Optional): Референтен номер на услуга (РНУ)
  - `templateId` (Type: `integer`): Идентификатор на шаблон на съобщение
- `length` (Type: `interger`): Общ брой съобщения

### Unauthorized (HTTP Status Code: 401)

## Example

```bash
curl -X GET 'https://localhost:5501/api/v1/obo/messages/inbox?from=2021-01-01&to=2021-01-31&offset=0&limit=100' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:'
```

## Endpoint URL

```
GET /api/v1/obo/messages/outbox
```

## Description

Връща списък с изпратени съобщения от името на профил

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:[...]|correspondentOID:|operatorID:
  - `representedPersonID` (Required): Идентификатор на чужд профил

### Query Parameters

- `from` (Type: `datetime ISO 8601`, Optional): От дата във формат `yyyy-MM-dd`
- `to` (Type: `datetime ISO 8601`, Optional): До дата във формат `yyyy-MM-dd`
- `templateId` (Type: `integer`, Optional): Идентификатор на шаблон на съобщение
- `offset` (Type: `integer`, Optional): Начало на резултатите, които да бъдат върнати.
- `limit` (Type: `integer`, Optional): Брой резултати, които да бъдат върнати.

## Response

### HTTP Status Code: 200 OK

```json
{
  "result": [
    {
      "messageId": 0,
      "dateSent": "2023-11-29T11:32:14.392",
      "dateReceived": "2023-11-29T11:32:14.392",
      "subject": "string",
      "senderProfileName": "string",
      "senderLoginName": "string",
      "recipientProfileName": "string",
      "recipientLoginName": "string",
      "url": "string",
      "rnu": "string",
      "templateId": 0
    }
  ],
  "length": 0
}
```

### Response Body Parameters

- `result` (Type: `array of objects`): Списък съобщения
  - `messageId` (Type: `integer`): Идентификатор на съобщение
  - `dateSent` (Type: `datetime ISO 8601`): Дата на изпращане
  - `subject` (Type: `string`): Заглавие на съобщение
  - `senderProfileName` (Type: `string`): Наименование на профила изпращач
  - `senderLoginName` (Type: `string`): Наименование на потребител, изпратил съобщението
  - `recipients` (Type: `string`): Получатели като текст
  - `url` (Type: `string`): Url за преглед на съобщението в ССЕВ
  - `rnu` (Type: `string`, Optional): Референтен номер на услуга (РНУ)
  - `templateId` (Type: `integer`): Идентификатор на шаблон на съобщение
- `length` (Type: `interger`): Общ брой съобщения

### Unauthorized (HTTP Status Code: 401)

## Example

```bash
curl -X GET 'https://localhost:5501/api/v1/obo/messages/outbox?from=2021-01-01&to=2021-01-31&offset=0&limit=100' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:'
```

## Endpoint URL

```
POST /api/v1/obo/messages
```

## Description

Изпращане на съобщение от името на чужд профил

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:[...]|correspondentOID:|operatorID:
  - `representedPersonID` (Required): Идентификатор на чужд профил

### Request Body

```json
{
  "recipientProfileIds": [0, 1, 2],
  "subject": "string",
  "rnu": "string",
  "templateId": 0,
  "fields": {
    "guid1": "string",
    "guid2": "string",
    "guid3": "string"
  }
}
```

### Request Body Parameters

- `recipientProfileIds` (Type: `array of integers`, Required): Списък идентификатори на получатели (ЕГН/ЛНЧ/ЕИК/...)
- `subject` (Type: `string`, Required): Заглавие на съобщението
- `rnu` (Type: `string`, Optional): Референтен номер на услуга (РНУ)
- `templateId` (Type: `string`, Required): Идентификатор на шаблон на съобщението
- `fields` (Type: `object`, Optional): Списък с полетата и техните стойности в шаблона на съобщението във формат (Идентификатор на поле: Стойност)
  - `guid1` (Type: `guid`, Required): Поле в шаблона на съобщението и стойността му

## Response

### HTTP Status Code: 200 OK

```json
1
```

### Response Body Parameters

- (Type: `integer`): Идентификатор на изпратено съобщение

### Bad Request (HTTP Status Code: 400)

### Unauthorized (HTTP Status Code: 401)

### Forbidden (HTTP Status Code: 403)

## Example

```bash
curl -X POST 'https://localhost:5501/api/v1/obo/messages' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:1234567890|correspondentOID:|operatorID:' \
-d '{
  "recipientProfileIds": [1],
  "subject": "Заглавие на съобщение",
  "rnu": null,
  "templateId": "1",
  "fields": {
      "179ea4dc-7879-43ad-8073-72b263915656": "Съдържание тест 1",
      "e2135802-5e34-4c60-b36e-c86d910a571a": [2]
  }
}'
```

# NOTIFICATIONS

## Endpoint URL

```
POST /api/t_v1/notifications
```

## Description

Изпращане на нотификация по имейл, смс и/или вайбър

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Request Body

```json
{
  "legalEntity": {
    "identifier": "string",
    "name": "string",
    "email": "string",
    "phone": "string"
  },
  "individual": {
    "identifier": "string",
    "firstName": "string",
    "middleName": "string",
    "lastName": "string",
    "email": "string",
    "phone": "string"
  },
  "email": {
    "subject": "string",
    "body": "string"
  },
  "sms": "string",
  "viber": "string"
}
```

### Request Body Parameters

- `legalEntity` (Type: `object`, Optinal): Обект с данни за получател от тип ЮЛ
  - `identifier` (Type: `string`, Optinal): Идентификатор на получател (ЕГН/ЛНЧ/ЕИК/...)
  - `name` (Type: `string`, Required): Наименование на получател
  - `email` (Type: `string`, Optional): Имейл на получател
  - `phone` (Type: `string`, Optional): Телефон на получател
- `individual` (Type: `object`, Required): Обект с данни за получател от тип ФЛ
  - `identifier` (Type: `string`, Required): Идентификатор на получател (ЕГН/ЛНЧ/ЕИК/...)
  - `firstName` (Type: `string`, Required): Собствено име на получател
  - `middleName` (Type: `string`, Required): Бащино име на получател
  - `lastName` (Type: `string`, Required): Фамилия на получател
  - `email` (Type: `string`, Optional): Имейл на получател
  - `phone` (Type: `string`, Optional): Телефон на получател
- `email` (Type: `object`, Optinal): Заглавие на съобщението
  - `subject` (Type: `string`, Required): Заглавие на имейл
  - `body` (Type: `string`, Required): Съдържание на имейл (поддържа HTML)
- `sms` (Type: `string`, Optional): Съдържание на съобщение (SMS)
- `viber` (Type: `string`, Optional): Съдържание на съобщение (Viber)

## Response

### HTTP Status Code: 202 Accepted

```json
Empty
```

### Bad Request (HTTP Status Code: 400)

### Unauthorized (HTTP Status Code: 401)

## Example

```bash
curl -X POST 'https://localhost:5501/api/t_v1/notifications' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:' \
-d '{
  "legalEntity": null,
  "individual": {
    "identifier": "1234567890",
    "firstName": "Иван",
    "middleName": "Иванов",
    "lastName": "Иванов",
    "email": "test@abv.bg",
    "phone": "359888000001"
  },
  "email": {
    "subject": "това е заглавие на ИМЕЙЛ",
    "body": "това е съдържание на ИМЕЙЛ"
  },
  "sms": "това е СМС",
  "viber": "това е ВАЙБЪР"
}'
```

# TICKETS

## Endpoint URL

```
POST /api/t_v1/tickets
```

## Description

Изпращане на електронен административен акт

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Request Body

```json
{
  "legalEntity": {
    "identifier": "string",
    "name": "string"
  },
  "individual": {
    "identifier": "string",
    "firstName": "string",
    "middleName": "string",
    "lastName": "string",
    "email": "string",
    "phone": "string"
  },
  "ticket": {
    "subject": "string",
    "body": "string",
    "type": 1,
    "documentSeries": "string",
    "documentNumber": "string",
    "issueDate": "2023-11-30T20:52:20.865",
    "vehicleNumber": "string",
    "violationDate": "2023-11-30T20:52:20.865",
    "violatedProvision": "string",
    "penaltyProvision": "string",
    "dueAmount": "string",
    "discountedPaymentAmount": "string",
    "iban": "string",
    "bic": "string",
    "paymentReason": "string",
    "document": {
      "name": "string",
      "contentType": "string",
      "content": "string",
      "documentRegistrationNumber": "string"
    },
    "documentIdentifier": "string"
  }
}
```

### Request Body Parameters

- `legalEntity` (Type: `object`, Optinal): Обект с данни за получател от тип ЮЛ
  - `identifier` (Type: `string`, Optinal): Идентификатор на получател (ЕГН/ЛНЧ/ЕИК/...)
  - `name` (Type: `string`, Required): Наименование на получател
- `individual` (Type: `object`, Required): Обект с данни за получател от тип ФЛ
  - `identifier` (Type: `string`, Required): Идентификатор на получател (ЕГН/ЛНЧ/ЕИК/...)
  - `firstName` (Type: `string`, Required): Собствено име на получател
  - `middleName` (Type: `string`, Optional): Бащино име на получател
  - `lastName` (Type: `string`, Required): Фамилия на получател
  - `email` (Type: `string`, Optional): Имейл на получател
  - `phone` (Type: `string`, Optional): Телефон на получател
- `ticket` (Type: `object`, Optinal): Обект от данни съдържащи информация за съобщението (електронен фиш, наказателно постановление НП)
  - `subject` (Type: `string`, Required): Заглавие на съобщението
  - `body` (Type: `string`, Required): Съдържание на съобщението
  - `type` (Type: `TicketType`, Required): Тип на документа (електронен фиш/НП)
  - `documentSeries` (Type: `string`, Optional): Серия на документа (отнася се само за електронния фиш)
  - `documentNumber` (Type: `string`, Required): Номер на документа
  - `issueDate` (Type: `datetime ISO 8601`, Required): Дата на издаване
  - `vehicleNumber` (Type: `string`, Required): Номер на МПС
  - `violationDate` (Type: `datetime ISO 8601`, Required): Заглавие на имейл
  - `violatedProvision` (Type: `string`, Required): Нарушена разпоредба
  - `penaltyProvision` (Type: `string`, Required): Санкционна разпоредба
  - `dueAmount` (Type: `string`, Required): Дължима сума
  - `discountedPaymentAmount` (Type: `string`, Required): Сума за плащане с отстъпка
  - `iban` (Type: `string`, Required): IBAN на МВР
  - `bic` (Type: `string`, Required): BIC на МВР
  - `paymentReason` (Type: `string`, Required): Основание за плащане
  - `document` (Type: `object`, Required): Обект от данни, съдържащи прикачени документи към съобщението
    - `name` (Type: `string`, Required): Наименование на документа
    - `contentType` (Type: `base64 string`, Required): Кодирано съдържание на документ с лимит до 10000Kb
    - `content` (Type: `string`, Required): Заглавие на имейл
    - `documentRegistrationNumber` (Type: `string`, Optional): Изходящ номер на документ
  - `documentIdentifier` (Type: `string`, Required): Идентификатор на документа в системата на АНД, с който се свързва конкретно задължение

```cs
enum TicketType {
  "TICKET" = 1,
  "PENAL_DECREE" = 2,
}
```

## Response

### HTTP Status Code: 200 OK

```json
1
```

### Response Body Parameters

- (Type: `integer`): Идентификатор на изпратено съобщение

### Bad Request (HTTP Status Code: 400)

### Unauthorized (HTTP Status Code: 401)

## Example

```bash
curl -X POST 'https://localhost:5501/api/t_v1/tickets' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:' \
-d '{
  "legalEntity": null,
  "individual": {
    "identifier": "1234567890",
    "firstName": "Иван",
    "middleName": "Иванов",
    "lastName": "Иванов",
    "email": "test@abv.bg",
    "phone": "359888000001"
  },
  "ticket": {
    "subject": "Имате регистрирано нарушение К-123456",
    "body": "Имате регистрирано нарушение К-123456",
    "type": "TICKET",
    "documentSeries": "К",
    "documentNumber": "123456",
    "issueDate": "2023-09-20T08:00:00",
    "vehicleNumber": "СВ0001СТ",
    "violationDate": "2023-08-20T08:00:00",
    "violatedProvision": "наредба 1",
    "penaltyProvision": "наредба 2",
    "dueAmount": "100",
    "discountedPaymentAmount": "60.00",
    "iban": "BG12311241241",
    "bic": "1231231",
    "paymentReason": "Констатирано е нарушенние",
    "document": {
        "name": "К-123456.pdf",
        "contentType": "application/pdf",
        "content": "dGhpcyBpcyBhIHRlc3Q=",
        "documentRegistrationNumber": null
    },
      "documentIdentifier": "10170364"
  }
}'
```

## Endpoint URL

```
POST /api/t_v1/tickets/{ticketId}/served
```

## Description

Промяна на статус (ВРЪЧЕН) на електронен административен акт

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Path Parameters

- `ticketId` (Type: `integer`, Required): Идентификатор на електронен административен акт.

### Request Body

```json
{
  "serveDate": "2023-11-30T21:08:48.865"
}
```

### Request Body Parameters

- `serveDate` (Type: `datetime ISO 8601`, Required): Дата на промяна на статуса

## Response

### HTTP Status Code: 200 OK

```json
Empty
```

### Bad Request (HTTP Status Code: 400)

### Unauthorized (HTTP Status Code: 401)

## Example

```bash
curl -X POST 'https://localhost:5501/api/t_v1/tickets/1/served' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:' \
-d '{
  "serveDate": "2023-08-20T08:00:00"
}'
```

## Endpoint URL

```
POST /api/t_v1/tickets/{ticketId}/annulled
```

## Description

Анулиране на електронен административен акт

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Path Parameters

- `ticketId` (Type: `integer`, Required): Идентификатор на електронен административен акт.

### Request Body

```json
{
  "annulDate": "2023-11-30T21:13:29.325",
  "annulmentReason": "string"
}
```

### Request Body Parameters

- `annulDate` (Type: `datetime ISO 8601`, Required): Дата на анулиране
- `annulmentReason` (Type: `string`, Required): Причина за анулиране

## Response

### HTTP Status Code: 200 OK

```json
Empty
```

### Bad Request (HTTP Status Code: 400)

### Unauthorized (HTTP Status Code: 401)

## Example

```bash
curl -X POST 'https://localhost:5501/api/t_v1/tickets/1/annulled' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:' \
-d '{
  "annulDate": "2023-08-20T08:00:00",
  "annulmentReason": "Тестово анулиране"
}'
```

## Endpoint URL

```
POST /api/t_v1/tickets/check
```

## Description

Проверка за статуса на административни актове

## Request

### Headers

- `Content-Type` (Required): application/json
- `Accept` (Required): application/json
- `Dp-Miscinfo` (Required): dn:/C=BG/ST=OID:[...]/L=BG/CN=[...]|representedPersonID:|correspondentOID:|operatorID:

### Request Body

```json
{
  "ticketIds": [0],
  "deliveryStatus": 1,
  "from": "2023-11-30T21:14:34.245",
  "to": "2023-11-30T21:14:34.245"
}
```

### Request Body Parameters

- `ticketIds` (Type: `array of integers`, Required): Списък с идентификатори на административни актове
- `deliveryStatus` (Type: `DeliveryStatus`, Required): Статус на административни актове
- `from` (Type: `datetime ISO 8601`, Required): От дата
- `to` (Type: `datetime ISO 8601`, Required): До дата

```cs
enum DeliveryStatus {
  "DELIVERED" = 1,
  "UNDELIVERED" = 2,
}
```

## Response

### HTTP Status Code: 200 OK

```json
[
  {
    "ticketId": 0,
    "deliveryDate": "2023-11-30T21:14:34.245"
  }
]
```

- (Type: `array of objects`): Списък с данни от проверката
  - `ticketId` (Type: `integer`): Идентификатор на административен акт
  - `deliveryDate` (Type: `datetime ISO 8601`, Optional): Дата на връчване на административен акт. Ако няма - не е връчен

### Bad Request (HTTP Status Code: 400)

### Unauthorized (HTTP Status Code: 401)

## Example

```bash
curl -X POST 'https://localhost:5501/api/t_v1/tickets/1/annulled' \
-H 'Accept: application/json' \
-H 'Content-Type: application/json' \
-H 'Dp-Miscinfo: dn:/C=BG/ST=OID:2.16.100.1.1.1.1.13/CN=ciela.com|representedPersonID:|correspondentOID:|operatorID:' \
-d '{
  "annulDate": "2023-08-20T08:00:00",
  "annulmentReason": "Тестово анулиране"
}'
```
