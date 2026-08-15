# 🏠 Cimerko

### Student Roommate & Housing Finder

Cimerko is a web application designed to help students **find roommates, discover available housing, and connect with other students** looking for a place to live.

The goal is to make student housing easier to discover by bringing **room listings, roommate matching, and student-oriented housing information** into one platform.

---

## ✨ Features

* 👤 **User Accounts** - Register and manage your profile
* 🏠 **Housing Listings** - Browse available rooms and apartments
* 🔎 **Search & Filtering** - Find housing based on your preferences
* 🤝 **Roommate Discovery** - Connect with students looking for roommates
* 📍 **Location-Based Listings** - Find housing based on location
* 💬 **User Interaction** - Connect with other students through the platform
* 🔐 **Authentication & Authorization** - Secure account and role management
* 📱 **Responsive UI** - Designed to work across different screen sizes

---

## 🖥️ Screenshots

### Home Page

<img width="1842" height="1242" alt="Cimerko Home Page" src="https://github.com/user-attachments/assets/9457ca82-49c2-4e90-b825-153d72079ee2" />

### Cimerko

<img width="266" height="82" alt="Cimerko Logo" src="https://github.com/user-attachments/assets/39ade965-169c-40d9-a876-efc76c51e181" />

---

## 🛠️ Tech Stack

| Technology                | Purpose                        |
| ------------------------- | ------------------------------ |
| **ASP.NET Core MVC**      | Web application framework      |
| **C#**                    | Backend development            |
| **Entity Framework Core** | Database access & ORM          |
| **ASP.NET Core Identity** | Authentication & authorization |
| **SQLite**                | Database                       |
| **Razor**                 | Server-side UI rendering       |
| **Bootstrap / CSS**       | Styling & responsive design    |
| **Git & GitHub**          | Version control                |

---

## 🏗️ Architecture

Cimerko follows the **Model-View-Controller (MVC)** architecture:

```text
┌─────────────────────┐
│        Views        │
│     Razor / UI      │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│     Controllers     │
│   Application Logic │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│       Models        │
│   EF Core / Data    │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│      Database       │
│       SQLite        │
└─────────────────────┘
```

---

## 🚀 Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/asanaliov/cimerko-app.git
cd cimerko-app
```

### 2. Configure the project

Make sure you have the required **.NET SDK** installed.

Restore the project dependencies:

```bash
dotnet restore
```

### 3. Apply database migrations

```bash
dotnet ef database update
```

### 4. Run the application

```bash
dotnet run
```

The application will then be available through the local development URL provided by ASP.NET Core.

---

## 🎯 Project Goals

Cimerko was created with a focus on solving a real problem faced by students:

> **Finding affordable housing and compatible roommates shouldn't be complicated.**

The project combines housing discovery with student-focused roommate searching to create a simpler and more centralized experience.

---

## 📚 What I Learned

Working on Cimerko provided hands-on experience with:

* Building applications using **ASP.NET Core MVC**
* Designing and working with relational databases
* Using **Entity Framework Core**
* Implementing authentication with **ASP.NET Core Identity**
* Structuring applications using MVC principles
* Working with Git and GitHub
* Deploying .NET applications
* Designing user-focused web interfaces

---

## 🚀 Future Development

Cimerko already provides the core functionality required for student housing discovery and roommate finding. Future development would focus primarily on scalability, UX improvements, and expanding the matching system

---

## 👨‍💻 Author

**Asan Aliov**

Software Engineering Student

📍 Skopje, North Macedonia

---

⭐ If you find this project interesting, consider giving the repository a star!
