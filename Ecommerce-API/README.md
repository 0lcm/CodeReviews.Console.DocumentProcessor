# A RESTful API showcase built for a music E-commerce site, using Entity Framework Core and ASP.NET Core
![Static Badge](https://img.shields.io/badge/.NET_Version-.NET_10.0-orange)

## Purpose of The App
This app serves as an API, meant for the backend of an imaginary E-commerce website, selling various albums, singles, and mixtapes, and supporting vinyl, CD, and digital formats. Although the e-commerce platform itself doesn't exist, this project is primarily intended as a showcase, rather than a fully developed and commercially viable application.  
This project includes both the API, which can be used and tested independently, and a console UI project that can be launched and used in conjunction with the API to visualize the changes you are making, as well as allowing a slightly more non-technical friendly way to interact with the API locally.

## Basic Features
* Products, sales, and product tags are tracked and managed through Entity Framework Core.  
* Soft deletes ensure that deleting something is not destructive and won't invalidate previous records.  
* Pagination, filtering, and searching make a potentially large database manageable in bite-sized chunks according to the user's needs.  
* An up-to-date Postman collection makes it easy to test the API without worrying about which controllers do what.  
* Seeding data is made possible by importing various files. Supported files include: .xlsx, .xls, and .csv.
* You can easily export data by using the UI locally, or by sending a request to the API. Supported file types for exports are: .pdf, .xlsx, and .csv.

## How To Use The App
### Starting The App
After cloning the repo to your machine, you can either:  
1. Test the API alone by running either the HTTP or HTTPS version of the E-commerce.API project, and testing it with Swagger or Postman.  
   * In the E-commerce.API project, there is a Postman directory containing both a Postman collection JSON file and a Postman environment JSON file.  
     ![Image of the postman directory](https://i.imgur.com/4NYEi96.png)  
2. Run both the E-commerce.UI, and E-commerce.API projects together to test the API via the local UI project.  
   * The UI project is built to run on the HTTP version of the API, and launch after the api has started.  
     ![Image of the multi-launch settings](https://i.imgur.com/9HSDfV3.png)  

### Importing Data  
In order to add a seeding file, update the seeding folder path in the ECommerce.API's appsettings.json file.  
![appsettings.json File](https://i.imgur.com/xsjGNMq.png)  

File templates can be found in the Import-Export-Templates directory inside ECommerce.API.  
![file templates](https://i.imgur.com/98GO7hD.png)  

Fill out a file template, and place it in the seeding folder you set earlier. The app will automatically seed data on the first run, or any run afterwards that doesn't have a filled database.  

If you want to skip seeding entirely, or skip seeding certain tables, you can run the program with skip parameters.  
* "--skip-seeding" - Skips any seeding.  
* "--skip-item-seeding" - Only skips item seeding.  
* "--skip-tag-seeding" - Only skips tag seeding.  
* "--skip-sale-seeding" - Only skips sale seeding.  

### Exporting Data
To export data locally via the UI, first update the export folder path, and file type in ECommerce.UI/Configuration/appSettings.json. Supported file types are: "pdf", "xlsx", or "csv".  
![appSettings.json](https://i.imgur.com/8m4ybDE.png)  
After setting the folder path and file type, run the UI along with the API project, and navigate to the Export Data menu. After following the on screen instructions, the file will be exported to the set folder path.  

To export data remotely, you can use Postman to send a Get Exported Data request to the API, and download the response file.  
![Postman export request](https://i.imgur.com/3aXwFZ0.png)  

## Architecture Overview
* The frontend UI is a basic C# console application, using Spectre.Console for most of the text UI.  
* The backend is a RESTful API featuring GET, POST, and DELETE requests for every table within the database (Products, tags, sales).  
* Entity Framework Core handles the database. An SQLite file was chosen over an SQL server for easier setup.  
* After a DELETE request is received, and an item is set to be removed from the database, it gets caught by a soft delete interceptor and is modified to instead only be hidden from queries, ensuring non-destructive soft deletes. 
* All data retrieved from the database by the API is mapped onto the various DTO models stored on the E-commerce.Shared library, ensuring more secure and maintainable code.
* Database files are typically stored in %LocalAppData%\ECommerce folder, which is created upon the project's first run, along with creating the SQLite file and seeding starting data to the database.
* Items and Sales, and Items and Tags, share a many-to-many relationship, allowing many sales or many tags to be assigned to one item, and vice versa.

## Postman Collection Overview, and Sample Request
![Image of the API's Postman collection](https://i.imgur.com/ZhW9o0C.png)  
![Image of a successful Postman GET request](https://i.imgur.com/6xcqCTU.png)  

## Resources Used
  [.NET (10.0)](https://learn.microsoft.com/en-us/dotnet/)  
  [Microsoft.EntityFrameworkCore (10.0.5)](https://learn.microsoft.com/en-us/ef/)  
  [Microsoft.EntityFrameworkCore.Sqlite (10.0.5)](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/?tabs=dotnet-core-cli)  
  [Swashbuckle.AspNetCore.SwaggerGen (10.1.7)](https://www.nuget.org/packages/swashbuckle.aspnetcore.swaggergen/)  
  [Microsoft.AspNetCore.MVC (2.3.9)](https://www.nuget.org/packages/microsoft.aspnetcore.mvc/)  
  [Microsoft.AspNetCore.OpenApi (10.0.3)](https://www.nuget.org/packages/Microsoft.AspNetCore.OpenApi/10.0.3)  
  [Spectre.Console (0.55.0)](https://spectreconsole.net/cli)  
  [ClosedXML (0.105.0)](https://www.nuget.org/packages/ClosedXML/0.105.0)  
  [QuestPDF (2026.5.0)](https://www.nuget.org/packages/QuestPDF/2026.5.0)  

  ## Personal Thoughts
  ### What I Learned
  Having to make both the API itself and especially the Postman collection was a challenge to me at first, since I had only really worked with Swagger until now, but I feel a lot more comfortable using Postman now. Learning how to make many-to-many relationships work within the Entity Framework Core was also a little bit of a challenge at the start, but I'm
  glad I learned it, and I think it definitely makes the app not only better, but it would almost break the app if an E-commerce API could only have one singular sale per item. So, unless I need to start selling exclusively once-in-a-lifetime versions of vinyl albums, I think I'll probably end up using many-to-many relationships in the future. It was also fun to
  learn more about how to use Entity Framework Core. Setting up many-to-many relationships, building the soft delete interceptor, excluding soft deleted items from database queries, and just building the API and database in general definitely taught me more about how to use Entity Framework and how to build databases.  
  
  ### Design Choices
  When building the API I decided to use a simple design, where the API receives a request at the controller level, which calls the service level that deals with business logic, which usually entails mapping an object from the database to the correct DTO, returning the DTO containing the actual data to the controller, or pretty much any of the logic
  that doesn't require actually touching the database. Since the service layer doesn't touch the actual database, I have a repository layer beneath the service layer that executes the actual EF Core code that directly changes the database, and can return a database object to the service layer to map onto a DTO. I chose to use SQLite in my app instead of
  a local SQL server, simply because I didn't see a need to use a SQL server, and it seemed like SQLite met the requirements I needed, without having to possibly make someone download SQL, update to a different version of SQL, or, in general, have to mess with a server. As much as I can, I prefer to make my projects require as little setup as possible.  

  ### Closing Thoughts
  I really enjoyed this project. I definitely procrastinated on it and spent a little more time than I should have, but it was fun to make. I definitely enjoyed learning more about RESTful APIs, EF Core, Postman, and any other miscellaneous lessons I learned along the way. I think I still have a lot to learn, but I'm glad I got to learn what I did with this
  project, and I think I can learn even more with my next project. There were some ideas I wanted to do, like add images to the different products, or make the user's mock checkout a little bit nicer or more realistic, but in the end, I decided that most of my ideas weren't worth the effort, or wouldn't add enough value to the project for the amount of time
  it would take. Although this project isn't perfect, I'm hopeful that I can make my next project a little bit closer to perfect.
