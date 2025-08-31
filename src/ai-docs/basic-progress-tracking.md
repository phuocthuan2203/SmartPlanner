# SmartPlanner - Basic Progress Tracking (Dashboard) Implementation Plan

## **Overview**
Implement Use Case 3: Basic Progress Tracking Dashboard as specified in the requirements. This focuses on displaying Today's Tasks and Upcoming Tasks with basic progress indicators and quick actions. Build upon existing authentication and task management systems.

***

## **Task 1: Implement Dashboard Service**

### **Task 1.1: Create Dashboard Service Interface**
**File:** `Application/Services/Interfaces/IDashboardService.cs`
**Requirements:**
- Define buildDashboard method as specified in design document
- Include methods for filtering today vs upcoming tasks
- Add progress calculation methods

**Skeleton:**
```csharp
public interface IDashboardService
{
    Task<DashboardDTO> BuildDashboardAsync(Guid studentId);
    Task<bool> MarkTaskDoneFromDashboardAsync(Guid taskId, Guid studentId);
}
```

### **Task 1.2: Update Dashboard DTO**
**File:** `Application/DTOs/TaskDTOs.cs` (enhance existing DashboardDTO)
**Requirements:**
- Implement the exact structure specified in design document
- Include Today's Tasks and Upcoming Tasks lists
- Add progress calculation fields

**Skeleton:**
```csharp
public class DashboardDTO
{
    public List<TaskDTO> TodayTasks { get; set; } = new(); // Tasks due today, is_done = false
    public List<TaskDTO> UpcomingTasks { get; set; } = new(); // Tasks due after today, is_done = false
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public double ProgressPercentage { get; set; } // % tasks completed
    public bool HasNoTasks { get; set; } // For empty state handling
}
```

### **Task 1.3: Implement Dashboard Service**
**File:** `Application/Services/DashboardService.cs`
**Requirements:**
- Query database for all tasks linked to student
- Filter tasks where is_done = false
- Separate into Today's Tasks (due today) vs Upcoming Tasks (due after today)
- Calculate progress statistics (completed vs pending tasks)
- Handle empty state scenarios

**Key Logic:**
```csharp
public async Task<DashboardDTO> BuildDashboardAsync(Guid studentId)
{
    // 1. Query all tasks for student
    // 2. Filter where is_done = false (pending tasks)
    // 3. Separate by deadline: today vs future
    // 4. Calculate progress statistics
    // 5. Return DashboardDTO with categorized tasks
}
```

***

## **Task 2: Update Dashboard Controller**

### **Task 2.1: Enhance Dashboard Controller**
**File:** `Controllers/DashboardController.cs` (update existing)
**Requirements:**
- Replace placeholder logic with real DashboardService
- Ensure dashboard loads within 2 seconds (performance requirement)
- Add quick action for marking tasks done
- Handle empty state scenarios

**Skeleton:**
```csharp
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public async Task<IActionResult> Index()
    {
        var studentId = GetStudentIdFromSession();
        if (studentId == null) return RedirectToAction("Login", "Authentication");
        
        var dashboardData = await _dashboardService.BuildDashboardAsync(studentId.Value);
        return View(dashboardData);
    }

    [HttpPost]
    public async Task<IActionResult> MarkTaskDone(Guid taskId)
    {
        // Quick action: mark task as done from dashboard
        // Update task status and return updated dashboard section
    }
}
```

***

## **Task 3: Create Dashboard View Implementation**

### **Task 3.1: Update Dashboard View**
**File:** `Views/Dashboard/Index.cshtml`
**Requirements:**
- Implement the exact UI skeleton specified in design document
- Display Today's Tasks section with status toggles
- Display Upcoming Tasks section with deadline dates
- Add Progress Indicator showing % tasks completed
- Include Quick Actions (Create Task, Go to Task List)
- Handle empty state scenarios ("No tasks for today/upcoming")

**UI Structure:**
```html
@model DashboardDTO

<!-- Welcome header with student name -->
<div class="dashboard-header">
    <h2>Your Dashboard 📊</h2>
    <!-- Progress Indicator - % tasks completed -->
    <div class="progress-overview">
        <!-- Progress bar and statistics -->
    </div>
</div>

<!-- Quick Actions section -->
<div class="quick-actions">
    <!-- Create Task button -->
    <!-- Go to Task List button -->
</div>

<!-- Today's Tasks Section -->
<div class="today-tasks-section">
    <h3>Today's Tasks</h3>
    @if (!Model.TodayTasks.Any())
    {
        <p class="empty-state">No tasks for today</p>
    }
    else
    {
        <!-- List of TaskDTO with status toggle checkboxes -->
    }
</div>

<!-- Upcoming Tasks Section -->
<div class="upcoming-tasks-section">
    <h3>Upcoming Tasks</h3>
    @if (!Model.UpcomingTasks.Any())
    {
        <p class="empty-state">No upcoming tasks</p>
    }
    else
    {
        <!-- List of TaskDTO with deadline dates -->
    }
</div>
```

### **Task 3.2: Add Task Status Toggle Functionality**
**Requirements:**
- Add AJAX functionality for status toggle from dashboard
- Update progress indicators without page refresh
- Provide visual feedback for completed tasks

**JavaScript:**
```javascript
// Handle status toggle checkbox changes
// Send AJAX request to mark task as done
// Update progress indicators dynamically
// Move completed tasks from display
```

***

## **Task 4: Implement Dynamic Updates**

### **Task 4.1: Add AJAX Status Toggle**
**Requirements:**
- Implement client-side status toggle without page refresh
- Update dashboard sections dynamically after task CRUD operations
- Ensure progress indicators update in real-time

### **Task 4.2: Dashboard Auto-Refresh**
**Requirements:**
- Dashboard must update dynamically after CRUD operations on tasks
- Implement efficient partial view updates
- Handle concurrent updates gracefully

***

## **Task 5: Add Optional Filter Controls**

### **Task 5.1: Implement Basic Filters**
**Requirements:**
- Add optional filter by Subject (as specified in extension points)
- Add optional filter by Deadline
- Keep filters simple and dashboard-focused

**Skeleton:**
```html
<!-- Optional filter controls -->
<div class="dashboard-filters">
    <select name="subjectFilter">
        <option value="">All Subjects</option>
        <!-- Populate from user's subjects -->
    </select>
</div>
```

***

## **Task 6: Optimize Performance**

### **Task 6.1: Ensure 2-Second Load Time**
**Requirements:**
- Dashboard must load within 2 seconds after login (specified requirement)
- Optimize database queries with proper indexing
- Use efficient LINQ queries for task filtering
- Consider caching for frequently accessed data

**Optimization Strategies:**
```csharp
// Use efficient database queries
// Index on StudentId + Deadline for fast filtering
// Load only necessary task data for dashboard
// Consider pagination for users with many tasks
```

### **Task 6.2: Add Performance Monitoring**
**Requirements:**
- Add logging to track dashboard load times
- Monitor query performance
- Add fallback for slow queries

***

## **Task 7: Handle Edge Cases and Empty States**

### **Task 7.1: Empty State Handling**
**Requirements:**
- Display appropriate messages when no tasks exist
- Encourage task creation for new users
- Handle scenario where all tasks are completed

**Empty State Messages:**
```html
<!-- No tasks for today -->
<div class="empty-state">
    <p>No tasks for today. You're all caught up! 🎉</p>
</div>

<!-- No upcoming tasks -->
<div class="empty-state">
    <p>No upcoming tasks. Ready to plan your next move?</p>
    <a href="@Url.Action("Create", "Task")" class="btn btn-primary">Create Your First Task</a>
</div>
```

### **Task 7.2: Error Handling**
**Requirements:**
- Handle database connection issues gracefully
- Provide user-friendly error messages
- Add retry functionality for failed operations

***

## **Task 8: Register Services**

### **Task 8.1: Update Service Registration**
**File:** `Program.cs`
**Requirements:**
- Register IDashboardService and DashboardService
- Ensure proper dependency injection setup

**Registration:**
```csharp
builder.Services.AddScoped<IDashboardService, DashboardService>();
```

***

## **Task 9: Add Responsive Design**

### **Task 9.1: Mobile Optimization**
**Requirements:**
- Ensure dashboard works on mobile devices
- Stack sections vertically on small screens
- Make status toggles touch-friendly

### **Task 9.2: Accessibility**
**Requirements:**
- Add proper ARIA labels for progress indicators
- Ensure keyboard navigation works
- Add screen reader support

***

## **Success Criteria**
✅ Dashboard displays Today's Tasks (due today, not completed)  
✅ Dashboard displays Upcoming Tasks (due after today, not completed)  
✅ Progress indicator shows % tasks completed accurately  
✅ Empty states display appropriate messages  
✅ Quick Actions (Create Task, Go to Task List) work correctly  
✅ Status toggle marks tasks done without page refresh  
✅ Dashboard loads within 2 seconds requirement  
✅ Dashboard updates dynamically after task CRUD operations  
✅ Mobile responsive design works correctly  
✅ Authentication check redirects properly  

***

## **Implementation Notes**
- Follow exact specifications from design document (Today's vs Upcoming)
- Build upon existing TaskRepository and Task entity
- Use existing TaskDTO structure
- Maintain consistency with authentication flow
- Focus on simplicity and performance per requirements
- Test with various scenarios (no tasks, all completed, mixed states)
- Ensure proper error handling throughout
- Keep UI clean and focused on essential information