# Docker Deployment Guide

This guide covers deploying the Jewelry Application Backend and Admin Panel using Docker Compose.

## Services Overview

The docker-compose.yaml defines three interconnected services:

1. **db** (jewelry_db) - PostgreSQL 16 database (port 5532)
2. **server** (jewelry_server) - ASP.NET Core backend API (port 6000)
3. **backoffice** (jewelry_backoffice) - Flutter Web Admin Panel (port 6002)

All services are connected via a custom bridge network (`jewelry_network`) for optimal inter-service communication.

## Prerequisites

- Docker and Docker Compose installed
- `.env` file configured in the `jewerly-back` directory

## Quick Start

Build and run all three services with a single command:

```bash
cd jewerly-back
docker compose up --build
```

For detached mode (background):

```bash
docker compose up --build -d
```

## Environment Variables

Create a `.env` file in the `jewerly-back` directory with the following variables:

```env
# Database
POSTGRES_DB=jewelry_db
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_secure_password

# Application
ASPNETCORE_ENVIRONMENT=Development
OPENAI_API_KEY=your_openai_api_key
```

## Deployment Commands

### Build and Start All Services

```bash
cd jewerly-back

# Build and start all services (foreground)
docker compose up --build

# Build and start all services (background/detached)
docker compose up --build -d
```

### Check Service Status

```bash
# View running containers
docker compose ps

# Check health status
docker compose ps --format json | jq '.'
```

### View Logs

```bash
# All services (live tail)
docker compose logs -f

# Specific service
docker compose logs -f db
docker compose logs -f server
docker compose logs -f backoffice

# Last 100 lines
docker compose logs --tail=100
```

### Stop Services

```bash
# Stop all services (preserves volumes)
docker compose down

# Stop and remove volumes (fresh start)
docker compose down -v

# Stop without removing containers
docker compose stop
```

### Rebuild Specific Service

```bash
# Rebuild and restart database
docker compose up -d --build db

# Rebuild and restart backend
docker compose up -d --build server

# Rebuild and restart admin panel
docker compose up -d --build backoffice
```

### Restart Services

```bash
# Restart all
docker compose restart

# Restart specific service
docker compose restart server
```

## Accessing the Services

Once deployed, the services are accessible at:

- **Backend API**: http://localhost:6000/api
  - Swagger UI: http://localhost:6000/swagger

- **Admin Panel**: http://localhost:6002
  - Login with: admin / admin

- **Database**: localhost:5532
  - Database name: jewelry_db (from .env)
  - User: postgres (from .env)

## Service Details

### Database (db - jewelry_db)

- **Image**: PostgreSQL 16
- **Container**: jewelry_db
- **Port**: 5532:5432 (host:container)
- **Volume**: jewelry_db_data (persistent storage)
- **Network**: jewelry_network
- **Health Check**: pg_isready every 10s
- **Restart**: always
- Starts first, waits to be healthy before server starts

### Backend API (server - jewelry_server)

- **Build Context**: `jewerly-back` directory
- **Container**: jewelry_server
- **Port**: 6000:8080 (host:container)
- **Network**: jewelry_network
- **Depends On**: db (waits for healthy status)
- **Health Check**: HTTP health endpoint every 30s
- **Restart**: unless-stopped
- **Environment**: Configured via .env file
- Connects to database at `db:5432` (internal network)

### Admin Panel (backoffice - jewelry_backoffice)

- **Build Context**: `jewerly-admin` directory
- **Container**: jewelry_backoffice
- **Port**: 6002:80 (host:container)
- **Network**: jewelry_network
- **Depends On**: server (waits for start)
- **Health Check**: HTTP probe every 30s
- **Restart**: unless-stopped
- **Two-stage Build**:
  1. Flutter SDK builds web app (--release)
  2. Nginx Alpine serves static files
- API calls go to http://localhost:6000/api (from browser)

## Development Workflow

### Backend Changes

```bash
# Rebuild and restart backend
docker compose up -d --build server
```

### Admin Panel Changes

```bash
# Rebuild and restart admin panel
docker compose up -d --build backoffice
```

### Database Reset

```bash
# Stop all services and remove volumes
docker compose down -v

# Start fresh
docker compose up -d
```

## Troubleshooting

### Admin Panel Cannot Connect to Backend

The admin panel is configured to connect to `http://localhost:6000/api` from the browser (client-side). Make sure:

1. Backend is running on port 6000
2. You're accessing admin panel from the same machine
3. No firewall blocking port 6000

### Database Connection Issues

Check database health:

```bash
docker compose ps
docker compose logs db
```

Verify environment variables in `.env` file match the connection string.

### Build Failures

Clear Docker cache and rebuild:

```bash
docker compose down
docker system prune -a
docker compose up -d --build
```

## Production Considerations

For production deployment:

1. **Change default admin credentials** in backend code
2. **Use HTTPS** with proper SSL certificates
3. **Configure CORS** properly for your domain
4. **Set strong database password** in .env
5. **Use production environment** variables
6. **Configure nginx** for optimal caching and compression
7. **Set up monitoring** and logging
8. **Regular backups** of database volume

## Network Architecture

### Service Communication

```
┌─────────────────────────────────────────────────────────────┐
│                     Docker Network (jewelry_network)        │
│                                                             │
│  ┌──────────┐         ┌──────────┐         ┌──────────┐  │
│  │    db    │◄────────┤  server  │◄────────┤backoffice│  │
│  │jewelry_db│         │jewelry_  │         │jewelry_  │  │
│  │          │  5432   │server    │   -     │backoffice│  │
│  │Postgres  │         │ASP.NET   │         │  nginx   │  │
│  └────┬─────┘         └────┬─────┘         └────┬─────┘  │
└───────┼────────────────────┼────────────────────┼─────────┘
        │                    │                    │
     5532:5432            6000:8080            6002:80
        │                    │                    │
        ▼                    ▼                    ▼
┌─────────────────────────────────────────────────────────────┐
│                        Host Machine                          │
│                                                              │
│  ┌─────────────────────────────────────────────────┐        │
│  │              Browser (localhost)                 │        │
│  │                                                  │        │
│  │  Admin Panel ──┬──> http://localhost:6002       │        │
│  │  (Flutter Web) │                                 │        │
│  │                └──> http://localhost:6000/api    │        │
│  │                     (API calls from browser)     │        │
│  └─────────────────────────────────────────────────┘        │
└─────────────────────────────────────────────────────────────┘
```

### Startup Order

1. **db** starts first → waits for health check (pg_isready)
2. **server** starts after db is healthy → runs migrations, starts API
3. **backoffice** starts after server starts → serves admin panel

### Key Points

- All containers communicate via `jewelry_network` (bridge network)
- Database is only accessible to server within Docker network
- Admin panel (Flutter web) runs in browser, makes API calls to localhost:6000
- Health checks ensure proper startup order and monitoring
