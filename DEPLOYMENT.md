# SmartPlanner Deployment Guide

## Quick Start

Run the deployment script to get started immediately:

```bash
./deploy.sh
```

This will:
- Build Docker images
- Generate SSL certificates
- Start all services (app, database, nginx)
- Display access URLs and management commands

## Manual Deployment

### Prerequisites

- Docker and Docker Compose installed
- .NET 8.0 SDK (for development)

### Environment Setup

1. **Create production configuration:**
   ```bash
   cp src/appsettings.Production.template src/appsettings.Production.json
   ```

2. **Update configuration values:**
   - Database connection string
   - JWT secret key (minimum 32 characters)
   - Allowed hosts for your domain

### Docker Deployment

1. **Build and start services:**
   ```bash
   docker-compose up -d
   ```

2. **View logs:**
   ```bash
   docker-compose logs -f smartplanner
   ```

3. **Stop services:**
   ```bash
   docker-compose down
   ```

## Production Considerations

### Security

- **Change default passwords** in `docker-compose.yml`
- **Use proper SSL certificates** (replace self-signed ones)
- **Set strong JWT secret key** (32+ characters)
- **Configure firewall** to restrict database access

### Database

- PostgreSQL data persists in Docker volume `postgres_data`
- Automatic migrations run on startup
- Backup strategy recommended for production

### Scaling

- Use external PostgreSQL service (AWS RDS, etc.)
- Deploy to container orchestration (Kubernetes, Docker Swarm)
- Configure load balancer for multiple app instances

### Monitoring

- Application logs via `docker-compose logs`
- Database monitoring through PostgreSQL tools
- Consider adding health checks and metrics

## Cloud Deployment Options

### AWS
- **ECS/Fargate:** Container deployment
- **RDS:** Managed PostgreSQL
- **ALB:** Load balancing and SSL termination

### Azure
- **Container Instances:** Simple container deployment
- **Azure Database for PostgreSQL:** Managed database
- **Application Gateway:** Load balancing

### DigitalOcean
- **App Platform:** PaaS deployment
- **Managed Databases:** PostgreSQL service
- **Load Balancers:** Traffic distribution

## Environment Variables

Key environment variables for production:

```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=db;Database=smartplanner;Username=user;Password=pass
JwtSettings__SecretKey=your-secret-key
JwtSettings__Issuer=SmartPlanner
JwtSettings__Audience=SmartPlannerUsers
```

## Troubleshooting

### Common Issues

1. **Database connection failed:**
   - Check PostgreSQL container is running
   - Verify connection string format
   - Ensure database exists

2. **SSL certificate errors:**
   - Generate new certificates with correct domain
   - Update nginx configuration
   - Check certificate file permissions

3. **Application won't start:**
   - Check application logs
   - Verify all environment variables set
   - Ensure port 8080 is available

### Health Checks

Application health endpoint: `https://your-domain/health`

Database connectivity can be verified through application logs during startup.
