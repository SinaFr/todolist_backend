# Introduction

This app is a todo list, where users can create an account and login to create, update and delete tasks.
The application has a backend and a frontend. This README is about the backend.

# Technologies

- **Backend**: ASP.NET (C#)
- **Authentification**: JWT
- **Database**: SQL
- **Build-Tools**: .NET CLI

# Getting started

## Requirements

- SQL Server (z.B. SQL Server Express)
- .NET 9 SDK

## Project Setup

- Clone Project
- Change Secret and adapt DefaultConnection in appsettings.json.example and change it to appsettings.json
- Update the database

## Start postgres db and backend

To start the backend enter in the backend root directory:

```bash
dotnet run --project backend
```

It is also possible to configure Docker Compose and start it with:

```bash
docker-compose up -- build
```-compose down
```

# Authentification

The login takes place via /login, JWT is stored as HTTP-Only-Cookie (jwt).
The token is send automatically with every request. Users, that are not logged in get 401 Unauthorized.

# Debug backend

The backend can be tested and debugged with Postman.
First a user has to be created with POST /signup and can then be logged in with POST /login.

Example user:
{
"Username": "testuser",
"Password": "test123"
}

Existing Endpoints:
| Method | Endpoint | Description |
|:-------|:---------|------------:|
| POST | /signup | Register a user |
| GET | /username-check/{username} | Check if a username is available |
| POST | /login | Login |
| POST | /logout | Logout |
| GET | /accounts | Get all accounts |
| GET | /accounts/{id} | Get a speicific account |
| PUT | /accounts/{id} | Update a speicific account |
| DELETE | /accounts/{id} | Delete a speicific account |
| GET | /me | Checks for valid token, returns username |
| GET | /api/account | Get currently logged in account |
| GET | /tasks | Get all tasks |
| GET | /tasks/{id} | Get a specific task |
| POST | /tasks | Create a new task |
| PUT | /tasks/{id} | Update a specific task |
| DELETE | /tasks/{id} | Delete a specific task |

