# 🏦 Banking API - .NET 8 with Kubernetes

A Banking API demonstrating **Factory Design Pattern** with Docker and Kubernetes deployment. Supports multiple payment methods: UPI, Internet Banking, NEFT, and Credit Card.

[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Ready-blue)](https://www.docker.com/)
[![Kubernetes](https://img.shields.io/badge/Kubernetes-Ready-blue)](https://kubernetes.io/)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)

---

## 📋 Description

This project demonstrates:

* **Factory Design Pattern** for extensible payment services
* **Docker** containerization with multi-stage builds
* **Kubernetes** deployment with ConfigMaps, Secrets, and Services
* **RESTful API** with Swagger documentation
* **Health checks** for Kubernetes probes

---

## 🛠️ Tech Stack

| Category              | Technology                        |
| --------------------- | --------------------------------- |
| **Framework**         | .NET 8, ASP.NET Core Web API      |
| **Language**          | C# 12                             |
| **Containerization**  | Docker, Docker Compose            |
| **Orchestration**     | Kubernetes                        |
| **API Documentation** | Swagger/OpenAPI                   |
| **Data Storage**      | JSON Files (no database required) |
| **IDE**               | Visual Studio 2022 / VS Code      |
| **K8s Management**    | kubectl, Kubernetes Lens          |

---

## 📋 Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Docker Desktop](https://www.docker.com/products/docker-desktop) (with Kubernetes enabled)
* [kubectl](https://kubernetes.io/docs/tasks/tools/)
* [Lens](https://k8slens.dev/) *(optional)*

---

## 🚀 Quick Start

### ▶️ Option 1: Run Locally

```bash
# Clone repository
git clone https://github.com/yourusername/BankingAPI.git
cd BankingAPI

# Run application
dotnet run

# Open browser
https://localhost:5001/swagger
```

---

### 🐳 Option 2: Run with Docker

```bash
# Build image
docker build -t banking-api:1.0 .

# Run container
docker run -d -p 8080:8080 --name banking-api banking-api:1.0

# Open browser
http://localhost:8080/swagger
```

---

### ☸️ Option 3: Run on Kubernetes

```bash
# Enable Kubernetes in Docker Desktop first!

# Build Docker image
docker build -t banking-api:1.0 .

# Deploy to Kubernetes
kubectl apply -f k8s/

# Verify deployment
kubectl get pods -n banking-app

# Access application
http://localhost:30080/swagger
```

---

## 📁 Project Structure

```
BankingAPI/
├── Controllers/          # API endpoints
├── Services/
│   ├── Factory/          # Factory pattern implementation
│   └── Implementations/  # Payment services (UPI, NEFT, etc.)
├── Repositories/         # Data access layer
├── Models/               # Domain models
├── Data/                 # JSON data files
├── k8s/                  # Kubernetes manifests
├── Dockerfile
└── Program.cs
```

---

## ☸️ Kubernetes Commands

```bash
# Deploy
kubectl apply -f k8s/

# Check status
kubectl get all -n banking-app

# View pods
kubectl get pods -n banking-app

# View logs
kubectl logs -l app=banking-api -n banking-app

# Scale deployment
kubectl scale deployment banking-api --replicas=5 -n banking-app

# Port forward (alternative access)
kubectl port-forward svc/banking-api-service 8080:80 -n banking-app

# Delete deployment
kubectl delete -f k8s/
```

---

## 🧪 Test API

```bash
# Health check
curl http://localhost:30080/api/health

# Get accounts
curl http://localhost:30080/api/account

# UPI Payment
curl -X POST http://localhost:30080/api/payment/upi \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 1000,
    "fromAccountId": "ACC001",
    "receiverUPIId": "priya@icici"
  }'
```

---

## 🔍 Using Kubernetes Lens

1. Install Lens
2. Open Lens and connect to `docker-desktop` cluster
3. Navigate to **Workloads → Pods**
4. Select `banking-app` namespace
5. Click on any pod to view logs, shell access, and metrics

---

## 🔧 Troubleshooting

| Issue                           | Solution                                                                         |
| ------------------------------- | -------------------------------------------------------------------------------- |
| Pods stuck in ContainerCreating | Run `kubectl apply -f k8s/configmap.yaml` and `kubectl apply -f k8s/secret.yaml` |
| ImagePullBackOff                | Ensure Docker image is built: `docker build -t banking-api:1.0 .`                |
| Cannot access localhost:30080   | Check service: `kubectl get svc -n banking-app`                                  |
| Pods in CrashLoopBackOff        | Check logs: `kubectl logs <pod-name> -n banking-app`                             |

---


## 🗺️ The Complete Journey
┌─────────────────────────────────────────────────────────────────────┐
│              .NET APP → DOCKER → KUBERNETES JOURNEY                 │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  PHASE 1: DEVELOP                                                   │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │                                                              │   │
│  │  Your .NET Code                                              │   │
│  │  ┌────────────────────────────────────────────────────────┐  │   │
│  │  │  Controllers/PaymentController.cs                      │  │   │
│  │  │  Services/UPIPaymentService.cs                         │  │   │
│  │  │  Models/PaymentRequest.cs                              │  │   │
│  │  │  Program.cs                                            │  │   │
│  │  │  appsettings.json                                      │  │   │
│  │  │  BankingAPI.csproj                                     │  │   │
│  │  │  Data/accounts.json                                    │  │   │
│  │  └────────────────────────────────────────────────────────┘  │   │
│  │                                                              │   │
│  │  This is just C# code. Cannot run on Kubernetes directly.    │   │
│  │                                                              │   │
│  └──────────────────────────────────────────────────────────────┘   │
│                         │                                           │
│                         ▼                                           │
│  PHASE 2: BUILD DOCKER IMAGE                                        │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │                                                              │   │
│  │  Dockerfile tells Docker HOW to build                        │   │
│  │                                                              │   │
│  │  Command: docker build -t banking-api:1.0 .                  │   │
│  │                                                              │   │
│  │  Result: Docker Image (banking-api:1.0)                      │   │
│  │  ┌───────────────────────────────────────────────────────┐   │   │
│  │  │  Layer 1: Linux OS (Alpine)                           │   │   │
│  │  │  Layer 2: .NET 8 Runtime                              │   │   │
│  │  │  Layer 3: Your compiled DLLs                          │   │   │
│  │  │  Layer 4: Data files, configs                         │   │   │
│  │  └───────────────────────────────────────────────────────┘   │   │
│  │                                                              │   │
│  │  Image is stored in Docker's local image store               │   │
│  │                                                              │   │
│  └──────────────────────────────────────────────────────────────┘   │
│                         │                                           │
│                         ▼                                           │
│  PHASE 3: DEPLOY TO KUBERNETES                                      │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │                                                              │   │
│  │  Kubernetes YAML files tell K8s WHAT to run and HOW          │   │
│  │                                                              │   │
│  │  Command: kubectl apply -f k8s/                              │   │
│  │                                                              │   │
│  │  Result: Running application on Kubernetes                   │   │
│  │  ┌───────────────────────────────────────────────────────┐   │   │
│  │  │  Pod 1: banking-api container (from Docker image)     │   │   │
│  │  │  Pod 2: banking-api container (from Docker image)     │   │   │
│  │  │  Pod 3: banking-api container (from Docker image)     │   │   │
│  │  │  Service: Load balances traffic to pods               │   │   │
│  │  └───────────────────────────────────────────────────────┘   │   │
│  │                                                              │   │
│  └──────────────────────────────────────────────────────────────┘   │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘

## 👤 Author

**Surendra**
