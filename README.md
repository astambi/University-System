# Online University

[![Build status](https://ci.appveyor.com/api/projects/status/ybt84fefglkqn81v?svg=true)](https://ci.appveyor.com/project/astambi/university-system) [![Build Status](https://travis-ci.org/astambi/University-System.svg?branch=master)](https://travis-ci.org/astambi/University-System)

## Project Description

The University system provides online courses for students.

Students can search all courses by name, view course details, enroll in free courses (and cancel existing course enrollments), buy paid courses (and cancel course payments), download course resources for courses they have purchased (or are enrolled in), upload course exams on exam day, receive public certificates for courses they have successfully passed.

Administrators can create, edit and delete courses and curriculums.

Trainers can add and remove course resources, evaluate course exams and create and delete course certificates.

- Landing page (all users):
  The Landing page lists active and upcoming courses provided by the University. Any user can view the list of courses and the course details (incl. trainers, schedule, price and number of students already enrolled). Access to the courses resources and exams is available only to enrolled users after successful login.

- Pagination:
  Courses on the Landing page as well as the Active and Archived courses sections are listed with pagination.

- Searching courses by name on the Landing page, Active / Archived courses sections (all users):
  Users can search courses on the Landing page as well as on the Active and Archived courses sections by course name. The search funtionality is available to all users.

- Viewing courses by trainer & Searching trainer's courses by name (logged in users only):
  The course details page contains a link to the course trainer's other trainings. The funtionality is available to logged in users only. Users can also search courses on the trainer's profile page by name, the funtionality is available only to logged in users only.

- Creating / Editing / Deleting Courses (Administrators only):
  Courses can be created by users in Administrator role only. Administrators can edit course details, delete a course, replace the course's trainer, update the course's start and end dates and pricing. Each course has a name, a description, start and end date, price, a single trainer, and a list of course resources.

- Creating / Editing / Deleting Curriculums & Adding / Removing courses to curriculums (Administrators only):
  Curriculums can be created by users in Administrator role only. Administrators can add and remove courses to multiple curriculums.

- Issuing / Deleting Diplomas for successfully covered Curriculums (Administrators only):
  Administrators can review eligible candidates for a diploma (per curriculum). To be considered as eligible for a diploma students, should successfully pass all courses, included in the respective curriculum (i.e. students should have all required course certificates). Administrators can issue curriculum diplomas, as well as revoke existing diplomas. Should a curriculum change and become more demanding (e.g. include more courses), the previously issued diplomas will remain valid unless an Administrator revokes the diplomas.

- Creating / Deleting Roles & Adding / Removing Users to existing roles (Administrators only):
  Roles can be created by users in Administrator role only. Administrators can add and remove roles, as well as add and remove registered users to existing roles. Pre-defined roles: Administrator, Trainer, Blogger.

- Uploading / Deleting Course Resources (Trainers only):
  Course resources can be uploaded by logged in users in Trainer role only and only for the courses they train. Trainers can delete uploaded resources from their own trainings.

- Viewing / Downloading Course Resources (logged in users enrolled in the respective course and course trainers):
  Course resources can be viewed and downloaded only by logged in users enrolled in the respective course or by the course's trainer.

- Uploading / Viewing / Downloading Course Exams (logged in users enrolled in the respective course):
  Course exams can be uploaded by logged in users, enrolled in the respective course on exam day only. Exams can be viewed and downloaded by their creators but cannot be deleted.

- Downloading the latest student course Exam (Trainers only):
  The latest uploaded course exam for any enrolled student in the training can be downloaded by the course trainer when the course is over.

- Evaluating Course Exams (Trainers only):
  Course exams can be evaluated by the course trainer once the exam is over and not later than 1 month after the exam date. The course grade is on the scale from 2.00 (lowest) to 6.00 (highest) and can be updated at any time by the trainer. The grade can easily be converted to other scales.

- Issuing / Deleting Course Certificates (Trainers only):
  Course exams evaluated above 5.00 (on the scale of 2 to 6) will authomatically receive a course certificate when the course trainer updates the grade to an eligible one. Subsequent higher grades will generate a new certificate, however previous certificates will not be authomatically deleted, as the presumption is that certificates should be publicly accessible even if a better result is achieved on retake exams by the student. A trainer has the option, however, to review all course certificates and remove any invalid ones.

- Viewing / Downloading Course Certificates (all users):
  Course cetrtificates are publically accessible. Any user with a link would be able to view the certificate's contents and download the document as a pdf file.

- Ordering a paid course / Enrolling in a free course / Cancelling course enrollements & payments (logged in users only):
  User authentication (login) is required to buy a paid course, to enroll in a free course, as well as cancel existing course enrollment or course payment. Anonymous users will be redirected to the Login page when they hit a functionality that requires user authentication. In addition, enrolling in any course, as well as cancellation of enrollment or payment is possible only before the respective course begins.

- Adding / Removing a Paid Course to the Shopping Cart (all users):
  The user shopping cart is accessible to all users (with or without login). Users can add and remove a course from the cart, update or empty the entire cart. If a course in the shopping cart is no longer available (deleted by an Adminitrator) the course will be removed from the shopping cart authomaticaly. User authentication is required before the user can continue to checkout. Logging in will authomatically remove any course that the logged in user has already purchased or is enrolled in. Logging out will clear the shopping cart.

- Checkout (logged in users only):
  Hitting checkout is available to logged in users only and requires a selection of a payment method. Hitting checkout will create a new payment (order) and enroll the user in all courses in the shopping cart. Upon successful payment and enrollment in all courses, the shopping cart will be cleared.

- Storing Users' Shopping Carts:
  A unique shopping cart key is created for all users and stored in the users' HTTP session. All shopping carts are stored in a concurrent dictionary in the Shopping Cart Manager and identified by the unique shopping cart key from the user session. The Shopping Cart Manager itself is registered as a Singleton service.

- Payments / Orders (logged in users):
  Authenticated users can view a list of their own payments (orders). An authenticated user is allowed to cancel a payment (order) only if all courses included in the respective payment (order) have not started yet.

- Payment Invoice (logged in users):
  An invoice can be downloaded (as a pdf file) for any payment made.

- Viewing / Updating / Deleting the User Profile (logged in users):
  Users can view they own profile data (name, username, email, birthdate, phone number and user roles if any) and update or delete their profile. The username cannot be updated. The profile provides a collection of the user's payments (by date), courses (by start / end date) with reminders for exam dates, course resources (by course), exams (by course) and certificates (by course).

- Registration / Login (anonymous users):
  Users can register providing name, username, email, birthdate and password. Upon registration users are redirected to Login.

- Registration / Login with external provider authentication (Google, Facebook):
  Users can register / login with credentials from external authentication providers (Google & Facebook). On first login users should complete a registration form confirming their name, email address, username and providing a birthdate.

- Two-factor authentication:
  Registered users can configure an authenticator app and set-up a two-factor authentication.

- University Blog:
  The Online University maintains a tech blog with tech articles and tutorials for the students. Anyone can view the blog articles, however reading the full articles requires user authentication. Publishing articles is delegated to the logged in user in role Blogger (set by the Univesity Administrators). Article authors can update or delete the articles they have published at any time.

### Public Part (Any user: Authenticated or Anonymous)

The public part of the University is visible by any user with or without authentication:

- Login / Register (anonymous users only);
- Home Page listing only the active and upcoming courses selection with pagination & search;
- All courses (active & upcoming and archived courses) with search by course name and pagination;
- Course Details Page listing all course details (name, description, start-end dates, price, trainer, number of students enrolled) and buttons for Enroll/ Cancell enrollment, Add/Remove course to/from Shopping Cart, link to trainer's other trainings;
- Shopping Cart with added courses, functionality to add/remove course, as well as to empty the shopping cart;
- Viewing the latest Univesity blog articles & tutorials (titles only, no access to the full article).

### Private Part (Logged in users only)

- Enrolling in free courses;
- Buying courses in the shopping cart, provided the user is not yet enrolled in the respective course;
- Viewing Payments made (Orders) with an option for payment cancellation prior to the course start date. After the course starts the payment can no longer be cancelled;
- Payment Details, including links to the courses purchased;
- Viewing / Downloading resources for courses they are enrolled in;
- Uploading Exams on exam day for courses they are enrolled in;
- Viewing / Downloading Exams by course and date;
- Viewing / Downloading Certificates for courses successfully passed;
- Viewing Diplomas for successfully covered curriculums;
- User's Profile with stats:
  - Collection of user's courses (free and purchased) with the option to upload exams on exam day;
  - Collection of user's resources (by course);
  - Collection of user's exams (by course);
  - Collection of user's diplomas & certificates (by course);
  - Collection of user's payments;
- Updating Profile data (name, email, password, birthdate, phone number);
- Adding / Removing multiple External logins (Google & Facebook);
- Adding / Removing Two-factor authentication with configuration of authenticator app;
- Deleting the user Profile, providing the user is not listed as the single trainer for a course;
- Reading the blog articles & tutorials;
- Logout.

### Administration Area (Logged in users in role Administrator)

- Creating / Updating / Deleting Courses;
- Creating / Updating / Deleting Curriculums;
- Adding / Removing Courses to/from Curriculums;
- Creating / Deleting Diplomas for successfully covered curriculums;
- Creating / Deleting user Roles;
- Adding / Removing Users to/from existing Roles.

### Trainers Part (Logged in users in role Trainer)

- Viewing their own trainings;
- Adding / Removing / Viewing Resources to their own trainings;
- Downloading students' exams for their own trainings;
- Evaluating students' exams for their own trainings;
- Viewing students' certificates for their own trainings;
- Removing students' certificates for their own trainings.

### Blogger Area

- Viewing blog article titles without access to the full articles (anonymous users);
- Reading blog articles (any logged in user);
- Creating / Editing / Deleting their own blog articles & tutorials (logged in users in role Blogger). Roles are set by an Administrator.

## Back-end: ASP.NET Core

- Migrated from ASP.NET Core 2.2 to 3.0
- Refactored complex LINQ queries that Entity Framework Core 3.0 no longer evaluates on the client (see Breaking changes included in EF Core 3.0 https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-3.0/breaking-changes)

## Database: MS SQL Server

## Cloud Storage: Cloudinary https://github.com/cloudinary/CloudinaryDotNet

## Email Sender: SendGrid https://github.com/sendgrid/sendgrid-csharp

## Dependencies:

- AutoMapper https://github.com/AutoMapper/AutoMapper
- SelectPdf Html To Pdf Converter for .NET - Free Community Edition https://github.com/selectpdf/selectpdf-free-html-to-pdf-converter
- HtmlSanitizer https://github.com/mganss/HtmlSanitizer

## Authentication

- NB! Credentials for Google, Facebook, SendGrid, Cloudinary should be added to secrets

## Unit tests:

- All services, including the Shopping Cart Manager & the Shopping Cart are tested
- Selected controllers
