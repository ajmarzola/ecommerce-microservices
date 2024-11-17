# **E-commerce Microservices API**

![License](https://img.shields.io/badge/license-Apache%202.0-blue)

## **Description**
This project is an API for e-commerce developed with a microservices architecture. The goal is to demonstrate skills in building scalable and modularized APIs using messaging for service communication and containerization with Docker.

## **Key Features**
- **Product Catalog**: Complete product management.
- **Shopping Cart**: Add, remove, and list items in the cart.
- **Orders**: Order processing and registration.
- **Payments**: Simulated payment processing.
- **Authentication**: User management with JWT-based authentication.

## **Architecture**
- **Backend**: ASP.NET Core with C#.
- **Messaging**: RabbitMQ for asynchronous communication between microservices.
- **Database**: SQL Server.
- **Containerization**: Docker for packaging and deployment.

## **Setup Instructions**
### **Prerequisites**
- [Docker](https://www.docker.com/)
- [.NET SDK](https://dotnet.microsoft.com/download)
- [RabbitMQ](https://www.rabbitmq.com/)

### **Steps to Run Locally**
1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/ecommerce-microservices.git
   cd ecommerce-microservices
