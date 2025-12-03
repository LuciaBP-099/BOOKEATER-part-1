# BookEater Web Application

Information System

Authors: Lucía Brígido Pérez, 
         Mar Selfa Cuñat.

## Project Description
BookEater is a web application built with ASP.NET Core MVC that allows users to manage books, write reviews, organize their personal digital library and join a reading club, where they can chat about books. 
## Key Features
- User and Authentication System: User login/logout and registration functionality to acces their custom library.
- Book Management: Users can add, edit or delete books. View detailed book information and associate books with users' librares.
- Reviews and rating: Write/edit or delete personalized reviews and view reviews from other users.
- MyLibrary: Add books to your personal library and organize books into lists.
- Reading club: Users can chat with other users about books.
## Technology used
The system is built using:
- ASP.NET Core MVC framework
- SQL Server handles database opeartions
- Microsoft Azure is used to deploy the application and database
- HTML/CSS for basic styling and User Interface design
## Webpage
The web application is hosted on Microsoft Azure, providing public access to the system via the url: https://bookeater-g2f5bubzgjapg2he.germanywestcentral-01.azurewebsites.net/
## Database Schema
The project uses a SQL Server database hosted on Microsoft Azure. The schema includes the following tables:
Users: Stores information about the users of the application.
Books: Contains details about the books.
UserBooks: Stores the relations books to users.
The database model can be seen in the diagram below, I created it using SQL Server Management Studio (SSMS):
<img width="757" height="415" alt="Database" src="https://github.com/user-attachments/assets/782541e7-d6b2-47a1-9158-2fe9fd39f769" />


