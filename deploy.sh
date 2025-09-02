#!/bin/bash

# SmartPlanner Deployment Script
set -e

echo "🚀 Starting SmartPlanner deployment..."

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed. Please install Docker first."
    exit 1
fi

if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose is not installed. Please install Docker Compose first."
    exit 1
fi

# Create SSL directory for certificates
mkdir -p ssl

# Generate self-signed SSL certificate if none exists
if [ ! -f "ssl/cert.pem" ] || [ ! -f "ssl/key.pem" ]; then
    echo "🔐 Generating self-signed SSL certificate..."
    openssl req -x509 -newkey rsa:4096 -keyout ssl/key.pem -out ssl/cert.pem -days 365 -nodes \
        -subj "/C=US/ST=State/L=City/O=Organization/CN=localhost"
    echo "✅ SSL certificate generated"
fi

# Build and start services
echo "🏗️  Building Docker images..."
docker-compose build

echo "🚀 Starting services..."
docker-compose up -d

# Wait for services to be ready
echo "⏳ Waiting for services to start..."
sleep 10

# Check if services are running
if docker-compose ps | grep -q "Up"; then
    echo "✅ SmartPlanner deployed successfully!"
    echo ""
    echo "🌐 Application URLs:"
    echo "   HTTP:  http://localhost:8081"
    echo "   HTTPS: https://localhost:8443"
    echo ""
    echo "📊 Database:"
    echo "   PostgreSQL: localhost:5433"
    echo "   Database: smartplanner"
    echo "   Username: smartplanner"
    echo ""
    echo "🔧 Management commands:"
    echo "   View logs:    docker-compose logs -f"
    echo "   Stop:         docker-compose down"
    echo "   Restart:      docker-compose restart"
    echo ""
    echo "⚠️  IMPORTANT: Change default passwords in production!"
else
    echo "❌ Deployment failed. Check logs with: docker-compose logs"
    exit 1
fi
