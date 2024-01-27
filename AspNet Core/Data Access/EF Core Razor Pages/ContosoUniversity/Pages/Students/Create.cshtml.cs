using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Students
{
    public class CreateModel : StudentEnrollmentPageModel
    {
        private readonly ContosoUniversity.Data.SchoolContext _context;
        private readonly ILogger<StudentEnrollmentPageModel> _logger;

        public CreateModel(ContosoUniversity.Data.SchoolContext context,
                           ILogger<StudentEnrollmentPageModel> logger)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult OnGet()
        {
            var student = new Student();
            PopulateAssignedCourseData(_context, student);
            return Page();
        }

        [BindProperty]
        public StudentVM Student { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync(string[] selectedCourses)
        {
            var newStudent = new Student();

            try
            {
                if (await TryUpdateModelAsync<Student>(
                    newStudent,
                    "Student",
                    s => s.LastName, s => s.FirstMidName, s => s.EnrollmentDate))
                {
                    _context.Students.Add(newStudent);
                    await _context.SaveChangesAsync();

                    if (selectedCourses.Length > 0)
                    {
                        _context.Courses.Load();
                        _context.Enrollments.Load();

                        foreach(string course in selectedCourses)
                        {
                            int CourseID = int.Parse(course);
                            var foundCourse = await _context.Courses.FindAsync(CourseID);
                            if (foundCourse != null)
                            {
                                var newEnrollment = new Enrollment()
                                {
                                    StudentID = newStudent.ID,
                                    CourseID = foundCourse.CourseID
                                };
                                
                                _context.Enrollments.Add(newEnrollment);
                                await _context.SaveChangesAsync();
                            }
                            else
                            {
                                _logger.LogWarning($"Course {course} not found", course);
                            }
                        }
                    }

                    return RedirectToPage("./Index");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            
            PopulateAssignedCourseData(_context, newStudent);
            return Page();
        }
    }
}
