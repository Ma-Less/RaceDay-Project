# RaceDay - Event Management System

## System Description

RaceDay is a full-stack web-based event management system designed for the South African road running, walking, and cycling community. It provides a centralised platform for Event Organisers to manage events, categories, and participant results, while allowing Participants to browse, enter, and track their performance in events.

## User Roles

The system defines two primary user roles to manage access and functionality:

- Organiser: This role is responsible for creating and managing the event ecosystem. An Organiser can:
    - Create, edit, and delete events.
    - Define categories (e.g., age groups, distances) for each event.
    - View all participant enrolments for their events.
    - Capture and publish official finish times and positions for participants.

- Participant: This role represents the athletes and enthusiasts using the platform. A Participant can:
    - Create an account and manage their profile.
    - Browse a list of upcoming events.
    - Enter an event by selecting a category.
    - View their personal history of enrolled events and official results.

Part 2 - API & CI/CD
CI/CD Status: The API builds and all 6 unit tests pass successfully on GitHub Actions. (See Actions tab for the green checkmark).
Part 2 Screenshot in the FOLDER.

Part 2: RESTful API Development
For Part 2, I built the backend RESTful API that powers the RaceDay platform. I used ASP.NET Core Web API (C#) and connected it to a SQL Server database using Entity Framework Core (Code-First), ensuring the schema matched the ERD I designed in Part 1.
What I built and implemented:
Authentication & Security: I implemented secure user registration and login. To protect user data, I ensured all passwords are cryptographically hashed before being stored. I also set up Session-based authentication that securely stores the User ID and Role on the server for subsequent requests.
Role-Based Access Control (RBAC): I enforced strict role-based restrictions across all endpoints. Organisers are the only ones who can create events, manage categories, and capture results, while Participants can browse events, enrol, and view their personal results. Unauthorized access attempts are rejected with a 403 Forbidden status.
API Endpoints & Swagger: I developed full CRUD functionality across controllers for Auth, User, Events, Categories, Enrolments, and Results. I also integrated Swagger UI, allowing all endpoints to be easily tested and documented in the browser.
Unit Testing: I wrote 6 comprehensive unit tests using xUnit and EF Core InMemory. These tests verify successful operations (like registering and enrolling), failure scenarios (like duplicate emails), database persistence, and crucially, that role restrictions correctly block unauthorized users.
CI/CD Pipeline: I configured a GitHub Actions workflow (ci-part2.yml) that automatically builds the solution and runs all my unit tests in the cloud on every push to the main branch, ensuring the code remains stable.

