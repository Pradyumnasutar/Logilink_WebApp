# LeS Logilink

## Overview

The LeS Logilink is a web-based application built using **.NET Core 7 MVC**. It serves as a business solution to streamline the process of managing delivery orders from buyer and supplier's perspectives. The system enhances order tracking, communication, and efficiency between both parties.

## Features

### Supplier-Buyer Perspective:

- Create new delivery orders.
- Track the status of existing orders.
- View and manage incoming delivery orders.
- Update order status (e.g., processing, dispatched, delivered).
- Provide estimated delivery dates.

### Admin Panel:

- User and role management.
- Module permission management.
- Configure system settings.

## Technology Stack

- **Frontend:** ASP.NET Core 7 MVC, Bootstrap, jQuery
- **Backend:** .NET Core 7 API
- **Database:** SQL Server with Entity Framework Core
- **Authentication:** Identity-based authentication and authorization
- **Deployment:** Can be hosted on Azure, AWS, or on-premise

## Installation & Setup

1. Clone the repository:
   ```sh
   git clone https://github.com/LightHouseDevelopment/LeS_LogiLink_WebApp.git
   ```
2. Install dependencies and restore packages:
   ```sh
   dotnet restore
   ```
3. Set up the database:
   - Configure the database connection string in `appsettings.json`.
   - Apply migrations:
     ```sh
     dotnet ef database update
     ```
4. Run the application:
   ```sh
   dotnet run
   ```

## Contact

For any queries or support, reach out at **sayak\@les.sg**

