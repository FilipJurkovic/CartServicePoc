# Cart Service

Minimalna implementacija Cart Servicea kao dio online maloprodajne platforme.
Servis upravlja košaricom korisnika — dodavanje, ažuriranje i uklanjanje artikala.

## Tehnologije

- .NET 8 / ASP.NET Core
- SQL Server 2022 (Dapper ORM)
- Redis (cache-aside pattern)
- Docker / Docker Compose
- DbUp (database migracije)
- FluentValidation
- Serilog

## Pokretanje

### Preduvjeti

- Docker Desktop

### Koraci

**1. Kloniraj repozitorij**

```bash
git clone https://github.com/FilipJurkovic/CartServicePoc
cd CartServicePoc
```

**2. Pokreni sve servise**

```bash
docker compose up --build -d
```

**3. Otvori Swagger**

```
http://localhost:5001/swagger
```

To je sve. Docker Compose pokreće SQL Server, Redis i aplikaciju.
DbUp automatski kreira bazu i tablice pri prvom pokretanju.

### Zaustavljanje

```bash
docker compose down
```

### Brisanje svih podataka

```bash
docker compose down -v
```

---

## API Endpoints

| Method | Endpoint                               | Opis                          |
| ------ | -------------------------------------- | ----------------------------- |
| GET    | `/api/cart/{userId}`                   | Dohvati košaricu              |
| POST   | `/api/cart/{userId}/items`             | Dodaj artikl u košaricu       |
| PUT    | `/api/cart/{userId}/items/{productId}` | Ažuriraj količinu artikla     |
| DELETE | `/api/cart/{userId}/items/{productId}` | Ukloni artikl iz košarice     |
| DELETE | `/api/cart/{userId}`                   | Očisti cijelu košaricu        |
| GET    | `/health/live`                         | Liveness probe                |
| GET    | `/health/ready`                        | Readiness probe (SQL + Redis) |

### Primjer zahtjeva

```json
POST /api/cart/user-1/items

{
  "productId": "prod-1",
  "productName": "Nike Air Max",
  "unitPrice": 129.99,
  "quantity": 2,
  "imageUrl": "https://example.com/nike.jpg"
}
```

### Primjer odgovora

```json
{
  "id": "5f54fa80-56b5-4f37-a048-4a5271fee620",
  "userId": "user-1",
  "items": [
    {
      "id": "e2d4a683-51d4-48df-9ade-7934bf6060d8",
      "productId": "prod-1",
      "productName": "Nike Air Max",
      "imageUrl": "https://example.com/nike.jpg",
      "unitPrice": 129.99,
      "quantity": 2,
      "lineTotal": 259.98
    }
  ],
  "total": 259.98,
  "updatedAt": "2026-02-27T14:45:15.9"
}
```

---

## Arhitektura

Servis je organiziran prema Clean Architecture principima:

```
CartServicePoc/
├── Domain/                         — Cart i CartItem modeli
├── Application/
│   ├── Interfaces/                 — ICartRepository, ICartService
│   └── Services/                   — CartService (business logika)
├── Infrastructure/
│   ├── Repositories/               — CartRepository (Dapper + SQL)
│   ├── Cache/                      — CacheService (Redis)
│   └── Database/
│       └── Migrations/             — DbUp SQL migracije
└── API/
    ├── Controllers/                — CartController
    ├── DTOs/                       — Request i Response objekti
    ├── Validators/                 — FluentValidation validatori
    └── Middleware/                 — Global error handling
```

### Ključne arhitekturalne odluke

**Dapper umjesto EF Core**
Odabrao sam Dapper zbog direktne kontrole nad SQL upitima i predvidljivih performansi.
Za servis s visokim brojem čitanja, eksplicitni SQL je pragmatičan izbor koji eliminira
iznenađenja s generiranim upitima.

**Cache-aside pattern**
Svako čitanje košarice provjerava Redis prvo. Write operacije invalidiraju cache.
Ako Redis nije dostupan, servis nastavlja raditi s SQL Serverom kao fallback —
Redis pada ne smije srušiti servis.

**MERGE statement za upsert**
Dodavanje artikla koji već postoji u košarici ažurira količinu kroz SQL MERGE u jednom
database round-tripu. Nema race conditiona između provjere i pisanja.

**Database-per-service**
Cart Service ima vlastitu SQL Server bazu. Ovo sprječava coupling između servisa
i omogućuje neovisno skaliranje i deployanje.

**Global error handling middleware**
Centralizirani middleware hvata sve neobrađene iznimke i vraća konzistentne error
response bez izlaganja internih detalja implementacije klijentu.

---

## Health Checks

| Endpoint        | Opis                         | Kubernetes                                           |
| --------------- | ---------------------------- | ---------------------------------------------------- |
| `/health/live`  | Je li aplikacija pokrenuta   | Liveness probe — restart ako faila                   |
| `/health/ready` | SQL Server + Redis konekcija | Readiness probe — ukloni iz load balancera ako faila |
