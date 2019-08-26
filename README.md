# Online University

## Project Description

The University system provides online courses for students.
Students can search all courses by name, view course details, enroll in free courses (and cancel course enrollment), buy paid courses (and cancel course payments), download course resources for courses they have purchased (or are enrolled in), upload course exams on exam day, receive public certificates for courses they have successfully passed. Administrators can create, edit and delete courses and curriculums. Trainers can add and remove course resources, evaluate course exams and create and delete course certificates.

- Home (all users):
  The Home View lists active and forthcoming courses provided by the University. Any user can view the list of courses and the course details (incl. trainers, schedule, price and number of students already enrolled). Access to the course's resources and exams is available to enrolled users after successful login only.

- Pagination:
  Courses on the Home page as well as the Active and Archived courses sections are listed with pagination.

- Search courses by name (all users):
  Users can search courses on the Home page as well as on the Active and Archived courses sections by course name. The search funtionality is available to all users.

- Search courses by trainer (logged in users only):
  The course details page container a link to all trainings from the current course trainer. The search funtionality is available to logged in users only.

- Creating / Editing / Deleting Courses (Administrators only):
  Courses can be created by users in Administrator role only. Administrators can edit course details, delete a course, replace the course's trainer, update the course's start and end dates and pricing. Each course has a name, a description, start and end date, price, a single trainer, and a list of course resources.

- Creating Curriculums & Adding / Removing courses to curriculums (Administrators only):
  Curriculums can be created by users in Administrator role only. Administrators can add and remove courses to multiple curriculums.

- Creating / Deleting Roles & Adding / Removing Users to existing roles (Administrators only):
  Roles can be created by users in Administrator role only. Administrators can add and remove roles, as well as add and remove registered users to existing roles.

- Uploading / Deleting Course Resources (Trainers only):
  Course resources can be uploaded by logged in users in Trainer role only. Trainers can replace or delete any uploaded resource.

- Adding / Viewing / Downloading Course Exams (logged in users enrolled in the respective course):
  Course exams can be uploaded by logged in users on exam day only. Exams can be viewed and downloaded by their creators but cannot be deleted.

- Downloading Course Exams (Trainers only):
  The latest course exams for any enrolled student in the training can be downloaded by the course trainers when the course is over.

- Evaluating Course Exams (Trainers only):
  Course exams can be evaluated by the course's trainer once the exam is over and not later than 1 month after the exam date. The course grade is on the scale from 2 to 6 (highest) and can be updated at any time by the trainer.

- Issuing / Deleting Course Certificates (Trainers only):
  Course exams evaluated above 5.00 (on the scale of 2 to 6) will authomatically receive a course certificate when the trainer updated the grade to an eligible grade. Subsequent higher grades will generate a new certificate, however previous certificates will be not be authomatically deleted as certificates should be publicly accessible. A trainer has the option, however, to view all course certificates and remove any invalid ones.

- Viewing / Downloading Course Certificates (all users):
  Course cetrtificates are publically accessible. Any user with the link would be able to view the documents contents and download a pdf certificate.

- Ordering a paid course / Enrolling in a free course / Cancelling course enrollement & payment (logged in users only):
  User authentication (login) is required to buy a course (add it to the user's shopping cart), to enroll in a free course, as well as cancel existing course enrollment or course payment made. Anonymous users are redirected to the Login page when they hit a functionality that requires user authentication.

- Adding / Removing a paid course to the Shopping Cart (all users):
  The user shopping cart is accessible to all users. Logging in removes any courses that the logged in user has already purchased or is enrolled in. Logging out clears the shopping cart. Users can remove a course from the cart or empty the entire cart. If the item is no longer available (deleted by an Admin from Store) the item is removed from the shopping cart. Hitting checkout required that a payment method is selected. Checkout creates a new payment (order) and enrolls the user in all courses in the shopping cart, upon which clears the shopping cart.

- Shopping Cart & Shopping Cart Manager:
  The shopping cart Id is stored in the HTTP session. The shopping cart manager holds a list of all user shopping carts in a concurrent dictionary and is registered as a Singleton service.

- Orders (logged in users):
  Authenticated users can view a list of their own orders. An authenticated user is allowed to cancel a order if the order has not yet been approved by the Store Admin for delivery, i.e. a user can cancel only his/her own pending orders. Upon appoval of an order for delivery book cancellation by the user is not allowed. Only Admins can update the order status after approval for delivery.

- Downloading Course Resources (logged in users):
  Authorized users can download an e-book they have purchased when the order is finalized (i.e. when the order status is Delivered).

- Viewing / Updating User Profile (logged in users):
  Users can view they own profile data (username, email, user roles if any), update or delete their profile. The profile provides a collection of the user's purchased e-books (from finalized orders, i.e. orders with status Delivered), the user's favourite books (book the user has liked) and the user's written book reviews.

- Registration / Login (anonymous users):
  Users can register providing name, username, email, birthdate and password. Upon registration users are redirected to Login.

- Registration / Login with external provider authentication (Google, Facebook):
  Users can register and login with credentials from external authentication providers (Google & Facebook). On first login users should complete a registration form confirming their name, email address, username and providing a birthdate.

### Public Part (Any user: Authenticated or Anonymous)

The public part of the University is visible by any user without authentication:

- Login / Register
- Home Page listing only the active and forthcoming courses selection with pagination & search
- All courses (active & forthcoming and archived courses) with Search by course name and pagination
- Course Details Page listing all course details (name, description, start-end dates, price, trainer, number of students enrolled) and buttons for Enroll/ Cancell enrollment, Add/Remove course to/from Shopping Cart, link to trainer's other trainings.
- Shopping Cart with added courses, functionality to add/remove course, as well as to empty the shopping cart

### Private Part (Logged in users only)

- Enrolling in free courses
- Buying courses in the shopping cart, provided the user is not yet enrolled in the respective course
- Viewing Payments made (Orders) with an option for payment cancellation before course start date. After course start date the payment cannot be cancelled.
- Payment Details, including links to the courses purchased
- Viewing / Downloading resources for free courses they are enrolled in & for courses purchased
- Uploading Exams on exam day for courses they are enrolled in & courses purchased
- Viewing / Downloading Exams by course and date
- Viewing / Downloading Certificates for courses successfully passed
- User's Profile with stats:
  - Collection of user's courses (free and purchased) with option to upload exams on exam day
  - Collection of user's resources by course
  - Collection of user's exams by course
  - Collection of user's certificates by course
  - Collection of user's payments
- Updating Profile data (name, email, password, birthdate, phone number)
- Adding / Removing multiple External logins (Google & Facebook)
- Adding / Removing Two-factor authentication with configuration of authenticator app
- Deleting the user Profile, providing the user is not listed as the single trainer for a course
- Logout

### Administration Part (Logged in users in role Administrator)

- Creating / Updating / Deleting Courses
- Creating / Deleting Curriculums & Adding / Removing courses to/from curriculums
- Creating / Deleting user Roles & Adding / Removing user to/from existing role

### Trainers Part (Logged in users in role Trainer)

- Viewing their own trainings (courses they train)
- Adding / Removing / Viewing Resources to their own trainings
- Downloading students' exams for their own trainings
- Evaluating students' exams for their own trainings
- Viewing students' certificates for their own trainings
- Removing students' certificates for their own trainings

## Back-end: ASP.NET Core

## Database: MS SQL Server

## Unit tests:

- All services, including the Shopping Cart Manager & the Shopping Cart are tested
- Selected controllers
