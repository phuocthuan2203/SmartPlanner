# SmartPlanner - Registration & Authentication Implementation Plan

## **Overview**
Implement the complete Registration & Authentication use case leveraging the existing setup (controllers, services, DTOs already created). Focus on creating views, updating navigation, adding validation, and creating a basic dashboard redirect.

***

## **Task 1: Create Authentication Views**

### **Task 1.1: Create Login View**
**File:** `Views/Authentication/Login.cshtml`
**Requirements:**
- Create responsive login form using Bootstrap
- Include email and password fields with proper validation
- Add password toggle visibility feature
- Display error messages from ViewBag.ErrorMessage
- Include link to registration page
- Use Font Awesome icons for better UX

**Skeleton:**
```html
@model SmartPlanner.Application.DTOs.LoginDTO
@{
    ViewData["Title"] = "Login";
}

<!-- Create Bootstrap card layout -->
<!-- Include form with asp-for helpers -->
<!-- Add client-side password toggle -->
<!-- Display error messages -->
<!-- Include navigation links -->
```

### **Task 1.2: Create Registration View**
**File:** `Views/Authentication/Register.cshtml`
**Requirements:**
- Create registration form with all required fields
- Include client-side validation displays
- Add password strength indicator
- Implement confirm password matching
- Add toggle visibility for both password fields
- Include link back to login page

**Skeleton:**
```html
@model SmartPlanner.Application.DTOs.StudentRegisterDTO
@{
    ViewData["Title"] = "Register";
}

<!-- Create registration form -->
<!-- Include all DTO fields with validation -->
<!-- Add password confirmation logic -->
<!-- Style with Bootstrap components -->
```

***

## **Task 2: Update Navigation and Layout**

### **Task 2.1: Update Main Layout Navigation**
**File:** `Views/Shared/_Layout.cshtml`
**Requirements:**
- Add session-based navigation logic
- Show different nav items for authenticated/unauthenticated users
- Include user dropdown with logout functionality
- Add SmartPlanner branding with icons
- Implement responsive Bootstrap navbar

**Key Logic:**
```csharp
// Check session: Context.Session.GetString("StudentId")
// If authenticated: Show Dashboard, Tasks, Subjects, User dropdown
// If not authenticated: Show Login, Register links
```

### **Task 2.2: Update Home Page**
**File:** `Views/Home/Index.cshtml` and `Controllers/HomeController.cs`
**Requirements:**
- Redirect authenticated users to dashboard
- Create landing page for non-authenticated users
- Add call-to-action buttons for registration/login
- Include feature highlights and benefits

***

## **Task 3: Add Data Validation**

### **Task 3.1: Enhance DTOs with Data Annotations**
**File:** `Application/DTOs/AuthenticationDTOs.cs`
**Requirements:**
- Add comprehensive data annotations to existing DTOs
- Include proper error messages
- Add display names for form labels
- Implement Compare attribute for password confirmation

**Skeleton:**
```csharp
// Add [Required], [EmailAddress], [StringLength], [Compare] attributes
// Update existing StudentRegisterDTO and LoginDTO
// Add proper Display names and error messages
```

### **Task 3.2: Add Client-Side Validation**
**Requirements:**
- Include validation scripts in views
- Add real-time validation feedback
- Implement custom validation for password strength

***

## **Task 4: Create Dashboard Placeholder**

### **Task 4.1: Create Dashboard Controller**
**File:** `Controllers/DashboardController.cs`
**Requirements:**
- Create controller with session authentication check
- Redirect to login if not authenticated
- Pass user information to view via ViewBag

**Skeleton:**
```csharp
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        // Check session authentication
        // Get student info from session
        // Return view with user data
    }
}
```

### **Task 4.2: Create Dashboard View**
**File:** `Views/Dashboard/Index.cshtml`
**Requirements:**
- Create welcome message with user name
- Add placeholder cards for Today's Tasks and Upcoming Tasks
- Include progress overview section
- Add quick action buttons
- Use responsive Bootstrap grid layout

***

## **Task 5: Enhance Authentication Flow**

### **Task 5.1: Add Authentication Middleware**
**Requirements:**
- Create base controller or attribute for authentication checks
- Implement consistent redirect logic for protected pages
- Add session timeout handling

### **Task 5.2: Improve Error Handling**
**Requirements:**
- Add user-friendly error messages
- Implement proper validation summary
- Add loading states for form submissions

***

## **Task 6: Add Styling and UX Enhancements**

### **Task 6.1: Custom CSS**
**File:** `wwwroot/css/site.css`
**Requirements:**
- Add custom styles for authentication forms
- Create consistent color scheme
- Add hover effects and transitions
- Implement responsive design improvements

### **Task 6.2: JavaScript Enhancements**
**Requirements:**
- Add form validation feedback
- Implement password strength indicator
- Add smooth transitions and animations
- Create loading spinners for form submissions

***

## **Task 7: Testing and Validation**

### **Task 7.1: Manual Testing Checklist**
**Requirements:**
- Test registration flow with valid/invalid data
- Test login flow with correct/incorrect credentials
- Verify session management and logout functionality
- Test navigation states (authenticated vs. non-authenticated)
- Verify responsive design on mobile devices

### **Task 7.2: Error Scenarios**
**Requirements:**
- Test duplicate email registration
- Test password mismatch scenarios
- Test invalid email formats
- Test empty form submissions
- Test session expiration handling

***

## **Success Criteria**
✅ Users can successfully register new accounts  
✅ Users can log in with valid credentials  
✅ Users are redirected to dashboard after successful authentication  
✅ Navigation shows appropriate options based on authentication state  
✅ Error messages are clear and user-friendly  
✅ Forms include client-side validation  
✅ Responsive design works on mobile devices  
✅ Session management works correctly  
✅ Users can log out successfully  

***

## **Implementation Notes**
- Leverage existing AuthenticationController and services
- Use Bootstrap 5 for responsive design
- Include Font Awesome for icons
- Maintain consistent styling with the application theme
- Ensure all forms have proper CSRF protection
- Test thoroughly before proceeding to next use case