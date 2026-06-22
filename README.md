````markdown
# PLM API - EBOM Management System

This project is a Product Lifecycle Management (PLM) based EBOM management system developed with ASP.NET Core Web API, SQL Server, Entity Framework Core, and a dynamic HTML/CSS/JavaScript frontend.

The main purpose of this project is to manage products, categories, parent-child product structures, BOM relationships, quantity updates, and automatic roll-up cost calculations.

---

## Project Overview

In manufacturing and automotive systems, products are often built from multiple sub-components. This project models that structure by using a parent-child product relationship.

For example:

```text
Main Assembly
 ├── Sub Component 1 x Quantity
 ├── Sub Component 2 x Quantity
 └── Sub Component 3 x Quantity
````

When the quantity or price of a sub-component changes, the total cost of the parent product is recalculated automatically.

---

## Features

### Backend Features

* Product CRUD operations
* Category CRUD operations
* Product-Category relationship management
* Parent-child product tree management
* EBOM / BOM relationship management
* Quantity update for child products
* Automatic roll-up cost calculation
* Recursive parent cost recalculation
* Base64 product image storage
* DTO-based request models
* Swagger API documentation

### Frontend Features

* Dynamic dashboard
* Product tree visualization
* Category filtering
* Product search
* Product creation from frontend
* Product update and delete operations
* Base64 image upload and preview
* BOM relationship creation
* BOM relationship deletion
* Quantity increase/decrease buttons
* Product detail modal
* Cost breakdown modal
* Dynamic data loading from backend API

---

## Technologies Used

### Backend

* ASP.NET Core Web API
* C#
* Entity Framework Core
* SQL Server
* LINQ
* DTO Models
* Swagger / OpenAPI

### Frontend

* HTML
* CSS
* JavaScript
* Fetch API

### Tools

* Visual Studio
* SQL Server Management Studio
* Postman
* Swagger UI
* Git / GitHub

---

## Database Structure

### Products Table

Stores product information.

| Column       | Description                           |
| ------------ | ------------------------------------- |
| Id           | Product ID                            |
| ProductName  | Product name                          |
| Price        | Product price                         |
| Stt_Date     | Expiration / validity date            |
| MinStokValue | Minimum stock value                   |
| ImageUrl     | Product image as URL or Base64 string |

---

### Categories Table

Stores product category information.

| Column   | Description                   |
| -------- | ----------------------------- |
| Id       | Category ID                   |
| Name     | Category name                 |
| ParentId | Parent category ID, if exists |

---

### ProductItems Table

Stores parent-child BOM relationships.

| Column          | Description                                      |
| --------------- | ------------------------------------------------ |
| Id              | Relationship ID                                  |
| ParentProductId | Main product ID                                  |
| ChildProductId  | Sub-component product ID                         |
| Quantity        | Quantity of child product used in parent product |

---

## Main Business Logic

The most important business logic in this project is the roll-up cost calculation.

Each parent product has one or more child products. The system calculates the total parent cost by multiplying each child product price with its quantity.

```text
Parent Product Cost = Σ (Child Product Price × Quantity)
```

Example:

```text
Main Product: Rear Axle Assembly

Child Products:
- Bushing: 50 TL x 4 = 200 TL
- Bolt: 10 TL x 6 = 60 TL
- Rubber Part: 75 TL x 2 = 150 TL

Total Parent Cost = 410 TL
```

If a child product price or quantity changes, the parent product cost is automatically recalculated.

If the updated parent is also used as a child in another product, the recursive calculation updates the upper-level parent product as well.

---

## API Endpoints

### Products

| Method | Endpoint             | Description          |
| ------ | -------------------- | -------------------- |
| GET    | `/api/Products`      | Get all products     |
| GET    | `/api/Products/{id}` | Get product by ID    |
| POST   | `/api/Products`      | Create a new product |
| PUT    | `/api/Products/{id}` | Update product       |
| DELETE | `/api/Products/{id}` | Delete product       |

---

### Categories

| Method | Endpoint                    | Description           |
| ------ | --------------------------- | --------------------- |
| GET    | `/api/Category`             | Get all categories    |
| POST   | `/api/Category`             | Create a new category |
| PUT    | `/api/Category/update-name` | Update category name  |
| DELETE | `/api/Category/{id}`        | Delete category       |

---

### Product Items / EBOM

| Method | Endpoint                           | Description                         |
| ------ | ---------------------------------- | ----------------------------------- |
| GET    | `/api/ProductItem/master-trees`    | Get product tree structure          |
| POST   | `/api/ProductItem/product-items`   | Add child product to parent product |
| GET    | `/api/ProductItem/{id}`            | Get product item by ID              |
| PUT    | `/api/ProductItem/update-quantity` | Update child product quantity       |
| DELETE | `/api/ProductItem/child-item`      | Delete parent-child relationship    |

---

## Example Request Bodies

### Create Product

```json
{
  "productName": "Z Rot",
  "price": 95,
  "stt_Date": "2026-06-22T00:00:00",
  "minStokValue": 120,
  "imageUrl": "data:image/jpeg;base64,...",
  "categoryIds": [1, 2]
}
```

---

### Add BOM Relationship

```json
{
  "parentProductId": 1,
  "childProductId": 5,
  "quantity": 2
}
```

---

### Update Quantity

```json
{
  "parentProductId": 1,
  "childProductId": 5,
  "newQuantity": 4
}
```

---

## Frontend Dashboard

The frontend dashboard communicates directly with the backend API.

Main frontend capabilities:

* View all product trees
* Filter products by category
* Search products
* Add new products
* Upload product images as Base64
* Update product details
* Delete products
* Add child products to a parent product
* Delete BOM relationships
* Increase or decrease child quantity
* View cost breakdown of parent products

---

## Base64 Image Management

Product images can be stored as Base64 strings in the database.

Instead of saving only an external image URL, the system can convert an uploaded image into a Base64 string and save it in the `ImageUrl` field.

Example:

```text
data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQABAAD...
```

The frontend can directly display the image using:

```html
<img src="data:image/jpeg;base64,...">
```

---

## Installation

### 1. Clone the Repository

```bash
git clone https://github.com/GurbuzhanOzkolay/plm_api.git
```

---

### 2. Open the Project

Open the project with Visual Studio.

---

### 3. Configure SQL Server Connection

Update the connection string in `appsettings.json` if needed.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=PLM_API_DB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

### 4. Apply Database Migration

Open Package Manager Console or terminal and run:

```bash
dotnet ef database update
```

---

### 5. Run the Project

Run the project from Visual Studio.

Swagger will be available at:

```text
https://localhost:7027/swagger
```

Frontend dashboard will be available at:

```text
https://localhost:7027/index.html
```

If your local port is different, update the following line in `wwwroot/index.html`:

```javascript
const BASE_API_URL = 'https://localhost:7027/api';
```

---

## Project Structure

```text
plm_api
│
├── Controllers
│   ├── ProductsController.cs
│   ├── CategoryController.cs
│   └── ProductItemController.cs
│
├── Dtos
│   ├── ProductCreateDto.cs
│   ├── UpdateQuantityDto.cs
│   └── CategoryUpdateDto.cs
│
├── Models / Entities
│   ├── Products.cs
│   ├── Category.cs
│   └── ProductItem.cs
│
├── wwwroot
│   └── index.html
│
├── AppDbContext.cs
├── Program.cs
└── appsettings.json
```

---

## Demo Scenario

A typical demo flow:

1. Open Swagger and show API endpoints.
2. Open the frontend dashboard.
3. Display product tree data loaded from the backend.
4. Filter products by category.
5. Search for a product.
6. Increase or decrease a child product quantity.
7. Show that parent product cost is recalculated.
8. Add a new product with a Base64 image.
9. Add the new product as a child component to a parent product.
10. Open the product detail modal and show the cost breakdown.

---

## Future Improvements

Possible improvements for the project:

* User authentication and authorization
* Role-based access control
* Service layer architecture
* Unit and integration tests
* Advanced stock tracking
* Product versioning
* Export product tree as PDF or Excel
* Docker support
* Deployment to a cloud platform

---

## Author

**Gürbüzhan Özkolay**
LinkedIn:https://www.linkedin.com/in/gürbüzhan-özkolay-8964a0335 

Software Engineering Student
Backend Developer Candidate

---

## Notes

This project was developed as part of a software engineering internship project.
It focuses on backend API development, database design, EBOM logic, frontend-backend integration, and dynamic product tree management.

```
```
