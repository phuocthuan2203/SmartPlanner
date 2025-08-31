# SmartPlanner - Task Management (CRUD) Implementation Plan

## **Overview**
Implement complete Task Management use case with full CRUD operations for Tasks and Subjects. Build upon existing authentication system and leverage the established services, DTOs, and entities.

***

## **Task 1: Implement Subject Management**

### **Task 1.1: Create Subject Service and Interfaces**
**File:** `Application/Services/Interfaces/ISubjectService.cs`
**Requirements:**
- Define CRUD operations interface
- Include student-specific filtering
- Add validation methods

**Skeleton:**
```csharp
public interface ISubjectService
{
    Task<IEnumerable<SubjectDTO>> GetSubjectsByStudentAsync(Guid studentId);
    Task<SubjectDTO?> GetSubjectByIdAsync(Guid subjectId, Guid studentId);
    Task<SubjectDTO> CreateSubjectAsync(SubjectCreateDTO dto);
    Task<SubjectDTO> UpdateSubjectAsync(SubjectUpdateDTO dto);
    Task<bool> DeleteSubjectAsync(Guid subjectId, Guid studentId);
    // Add validation methods
}
```

### **Task 1.2: Create Subject DTOs**
**File:** `Application/DTOs/SubjectDTOs.cs`
**Requirements:**
- Create DTOs for Subject operations
- Add data annotations for validation
- Include proper display names

**Skeleton:**
```csharp
public class SubjectCreateDTO
{
    [Required] public Guid StudentId { get; set; }
    [Required, StringLength(100)] public string Name { get; set; }
    // Add validation attributes
}

public class SubjectUpdateDTO
{
    // Update fields with validation
}

public class SubjectDTO
{
    // Display fields
}
```

### **Task 1.3: Implement Subject Service**
**File:** `Application/Services/SubjectService.cs`
**Requirements:**
- Implement all CRUD operations
- Add proper error handling and validation
- Ensure student ownership validation
- Use AutoMapper for entity-DTO mapping

***

## **Task 2: Implement Task Service**

### **Task 2.1: Create Task Service Interface**
**File:** `Application/Services/Interfaces/ITaskService.cs`
**Requirements:**
- Define comprehensive CRUD operations
- Include filtering and search capabilities
- Add dashboard-specific methods

**Skeleton:**
```csharp
public interface ITaskService
{
    Task<IEnumerable<TaskDTO>> GetTasksByStudentAsync(Guid studentId);
    Task<IEnumerable<TaskDTO>> GetTasksByStudentAndSubjectAsync(Guid studentId, Guid subjectId);
    Task<DashboardDTO> GetDashboardDataAsync(Guid studentId);
    Task<TaskDTO> CreateTaskAsync(TaskCreateDTO dto);
    Task<TaskDTO> UpdateTaskAsync(TaskUpdateDTO dto);
    Task<bool> DeleteTaskAsync(Guid taskId, Guid studentId);
    Task<bool> ToggleTaskStatusAsync(Guid taskId, Guid studentId);
    // Add search and filter methods
}
```

### **Task 2.2: Update Task DTOs**
**File:** `Application/DTOs/TaskDTOs.cs` (enhance existing)
**Requirements:**
- Add comprehensive validation attributes
- Include subject information in TaskDTO
- Add search and filter DTOs

**Skeleton:**
```csharp
// Enhance existing DTOs with proper validation
// Add TaskSearchDTO for filtering
// Add SubjectInfo to TaskDTO for display
```

### **Task 2.3: Implement Task Service**
**File:** `Application/Services/TaskService.cs`
**Requirements:**
- Implement all CRUD operations with validation
- Add filtering by subject, date range, completion status
- Implement dashboard data aggregation
- Include proper error handling and ownership validation

***

## **Task 3: Create Task Management Controllers**

### **Task 3.1: Create Subject Controller**
**File:** `Controllers/SubjectController.cs`
**Requirements:**
- Implement RESTful CRUD endpoints
- Add session-based authentication checks
- Include proper error handling and user feedback
- Support AJAX operations for better UX

**Skeleton:**
```csharp
[SessionAuthentication] // Custom attribute or base controller
public class SubjectController : Controller
{
    public async Task<IActionResult> Index() { /* List subjects */ }
    public IActionResult Create() { /* Create form */ }
    [HttpPost] public async Task<IActionResult> Create(SubjectCreateDTO dto) { /* Process creation */ }
    public async Task<IActionResult> Edit(Guid id) { /* Edit form */ }
    [HttpPost] public async Task<IActionResult> Edit(SubjectUpdateDTO dto) { /* Process update */ }
    [HttpPost] public async Task<IActionResult> Delete(Guid id) { /* Process deletion */ }
}
```

### **Task 3.2: Create Task Controller**
**File:** `Controllers/TaskController.cs`
**Requirements:**
- Implement full CRUD operations
- Add search and filtering capabilities
- Include AJAX endpoints for status toggle
- Support bulk operations

**Skeleton:**
```csharp
public class TaskController : Controller
{
    public async Task<IActionResult> Index(TaskSearchDTO search) { /* List with filtering */ }
    public async Task<IActionResult> Create() { /* Create form with subjects dropdown */ }
    [HttpPost] public async Task<IActionResult> Create(TaskCreateDTO dto) { /* Process creation */ }
    public async Task<IActionResult> Edit(Guid id) { /* Edit form */ }
    [HttpPost] public async Task<IActionResult> Edit(TaskUpdateDTO dto) { /* Process update */ }
    [HttpPost] public async Task<IActionResult> Delete(Guid id) { /* Process deletion */ }
    [HttpPost] public async Task<IActionResult> ToggleStatus(Guid id) { /* AJAX status toggle */ }
}
```

***

## **Task 4: Create Subject Management Views**

### **Task 4.1: Create Subject List View**
**File:** `Views/Subject/Index.cshtml`
**Requirements:**
- Display subjects in responsive table/card layout
- Include search and filter capabilities
- Add quick actions (Edit, Delete, Create)
- Show task count per subject
- Implement confirmation dialogs for deletion

**UI Elements:**
```html
<!-- Subject list table with actions -->
<!-- Search/filter form -->
<!-- Create new subject button -->
<!-- Modal dialogs for confirmations -->
```

### **Task 4.2: Create Subject Form Views**
**Files:** `Views/Subject/Create.cshtml`, `Views/Subject/Edit.cshtml`
**Requirements:**
- Create responsive forms with validation
- Include proper error handling and display
- Add cancel/save actions with confirmation
- Use consistent styling with authentication forms

***

## **Task 5: Create Task Management Views**

### **Task 5.1: Create Task List View**
**File:** `Views/Task/Index.cshtml`
**Requirements:**
- Display tasks in filterable table format
- Include columns: Title, Description, Subject, Deadline, Status, Actions
- Add search by title/description
- Filter by subject, completion status, date range
- Include quick status toggle checkboxes
- Add bulk actions (mark complete, delete)
- Show overdue tasks with warning styling

**Key Features:**
```html
<!-- Advanced search/filter form -->
<!-- Responsive task table -->
<!-- Status toggle functionality -->
<!-- Action buttons (Edit, Delete) -->
<!-- Pagination for large task lists -->
```

### **Task 5.2: Create Task Form Views**
**Files:** `Views/Task/Create.cshtml`, `Views/Task/Edit.cshtml`
**Requirements:**
- Create comprehensive task forms with all fields
- Include subject dropdown populated from user's subjects
- Add date picker for deadline selection
- Include rich text editor for description (optional)
- Implement client-side validation
- Add save/cancel actions

**Form Fields:**
```html
<!-- Title (required) -->
<!-- Description (optional, textarea) -->
<!-- Subject dropdown (optional) -->
<!-- Deadline date picker (required) -->
<!-- Is Done checkbox (edit only) -->
```

***

## **Task 6: Enhance Dashboard with Real Data**

### **Task 6.1: Update Dashboard Controller**
**File:** `Controllers/DashboardController.cs`
**Requirements:**
- Replace placeholder with real task data
- Implement dashboard statistics
- Add quick actions for task creation
- Include recent activity feed

**Skeleton:**
```csharp
public async Task<IActionResult> Index()
{
    // Get student ID from session
    // Load dashboard data using TaskService
    // Pass statistics and task lists to view
    return View(dashboardData);
}
```

### **Task 6.2: Update Dashboard View**
**File:** `Views/Dashboard/Index.cshtml`
**Requirements:**
- Display real today's tasks and upcoming tasks
- Show progress statistics and charts
- Add quick task creation form
- Include links to detailed task management
- Show overdue task alerts

***

## **Task 7: Add Advanced Features**

### **Task 7.1: Implement Search and Filtering**
**Requirements:**
- Add search by task title and description
- Filter by subject, completion status, date ranges
- Include sorting options (deadline, created date, priority)
- Implement client-side filtering for better UX

### **Task 7.2: Add AJAX Functionality**
**Requirements:**
- Implement AJAX status toggles without page refresh
- Add inline editing capabilities
- Create modal dialogs for quick task creation
- Implement auto-save for form data

### **Task 7.3: Add Data Validation and Error Handling**
**Requirements:**
- Server-side validation for all operations
- Client-side validation with immediate feedback
- Proper error messages and user guidance
- Handle edge cases (duplicate subjects, past deadlines)

***

## **Task 8: Repository Pattern Implementation**

### **Task 8.1: Create Repository Interfaces**
**Files:** `Infrastructure/Repositories/ITaskRepository.cs`, `Infrastructure/Repositories/ISubjectRepository.cs`
**Requirements:**
- Define data access interfaces
- Include query methods for filtering and searching
- Add repository methods for complex operations

### **Task 8.2: Implement Repository Classes**
**Files:** `Infrastructure/Repositories/TaskRepository.cs`, `Infrastructure/Repositories/SubjectRepository.cs`
**Requirements:**
- Implement EF Core-based repositories
- Add optimized queries with proper includes
- Implement filtering and sorting logic
- Add error handling and logging

***

## **Task 9: Add AutoMapper Profiles**

### **Task 9.1: Create Mapping Profiles**
**File:** `Application/Mappers/MappingProfile.cs`
**Requirements:**
- Map between entities and DTOs
- Handle nested objects (Task -> Subject relationship)
- Configure custom mappings for complex scenarios

**Skeleton:**
```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Task mappings
        CreateMap<Task, TaskDTO>()
            .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject.Name));
        
        // Subject mappings
        // Other entity mappings
    }
}
```

***

## **Task 10: Update Service Registration**

### **Task 10.1: Register Services in Program.cs**
**Requirements:**
- Register new services and repositories
- Ensure proper dependency injection setup
- Add AutoMapper configuration

**Updates:**
```csharp
// Register repositories
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();

// Register services
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
```

***

## **Success Criteria**
✅ Users can create, view, update, and delete tasks  
✅ Users can manage subjects and associate tasks with subjects  
✅ Task list includes search and filtering capabilities  
✅ Dashboard displays real task data and statistics  
✅ Status toggle works without page refresh  
✅ Forms include comprehensive validation  
✅ Overdue tasks are highlighted appropriately  
✅ Mobile responsive design works correctly  
✅ Error handling provides clear user feedback  
✅ All operations respect user ownership and security  

***

## **Implementation Notes**
- Build upon existing authentication system
- Use consistent styling and UX patterns
- Implement proper error handling throughout
- Add loading states for better user experience
- Consider performance optimization for large task lists
- Ensure all database operations are properly indexed
- Test thoroughly with various data scenarios
- Implement soft delete for tasks if needed for data integrity