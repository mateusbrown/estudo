using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Models.SchoolViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace ContosoUniversity.Pages.Students
{
    public class StudentEnrollmentPageModel : PageModel
    {
        public List<AssignedCourseData> AssignedCourseDataList = [];

        public void PopulateAssignedCourseData(SchoolContext context,
                                               Student student)
        {
            var allCourses = context.Courses;
            HashSet<int> instructorEnrollments = [];
            
            if (student.Enrollments != null)
            {
                instructorEnrollments = new HashSet<int>(student.Enrollments.Select(c => c.CourseID));
            }
            
            AssignedCourseDataList = [];
            
            foreach (var course in allCourses)
            {
                AssignedCourseDataList.Add(new AssignedCourseData
                {
                    CourseID = course.CourseID,
                    Title = course.Title,
                    Assigned = instructorEnrollments.Contains(course.CourseID)
                });
            }
            
        }
    }
}
