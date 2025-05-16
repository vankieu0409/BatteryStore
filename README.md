# Battery Shop Solution Structure

## Backend (Microservices)
- IdentityService
- CustomerService
- ProductService
- InventoryService
- OrderService
- PromotionService
- NotificationService
- CartService

M?i service g?m c�c layer:
- API: Web API endpoints
- Application: Application logic, CQRS, UseCases
- Domain: Entities, Aggregates, ValueObjects, Domain Events, Repositories (interfaces)
- Infrastructure: Data access, external integrations, repository implementations

## Frontend (React TypeScript)
- src/features: Ch?c n?ng chia theo domain (product, cart, order, ...)
- src/shared: T�i nguy�n d�ng chung (components, hooks, utils, ...)

---

### H??ng d?n ph�t tri?n
- M?i service backend l� m?t project .NET ri�ng bi?t, tri?n khai theo DDD v� microservice.
- Frontend s? d?ng React + TypeScript, zustand cho state, axios cho g?i API.
- Giao ti?p gi?a c�c service qua HTTP ho?c message broker (n?u c?n).

---

> ?� kh?i t?o c?u tr�c th? m?c cho solution. H�y t?o project .NET v� React v�o c�c th? m?c t??ng ?ng ?? ph�t tri?n ti?p.