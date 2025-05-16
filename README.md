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

M?i service g?m các layer:
- API: Web API endpoints
- Application: Application logic, CQRS, UseCases
- Domain: Entities, Aggregates, ValueObjects, Domain Events, Repositories (interfaces)
- Infrastructure: Data access, external integrations, repository implementations

## Frontend (React TypeScript)
- src/features: Ch?c n?ng chia theo domain (product, cart, order, ...)
- src/shared: Tài nguyên dùng chung (components, hooks, utils, ...)

---

### H??ng d?n phát tri?n
- M?i service backend là m?t project .NET riêng bi?t, tri?n khai theo DDD và microservice.
- Frontend s? d?ng React + TypeScript, zustand cho state, axios cho g?i API.
- Giao ti?p gi?a các service qua HTTP ho?c message broker (n?u c?n).

---

> ?ã kh?i t?o c?u trúc th? m?c cho solution. Hãy t?o project .NET và React vào các th? m?c t??ng ?ng ?? phát tri?n ti?p.