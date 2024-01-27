using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using System.Security.Policy;

namespace ContosoUniversity.Pages.Students
{
    public class EditModel : StudentEnrollmentPageModel
    {
        private readonly ContosoUniversity.Data.SchoolContext _context;
        private readonly ILogger<StudentEnrollmentPageModel> _logger;

        public EditModel(ContosoUniversity.Data.SchoolContext context,
                         ILogger<StudentEnrollmentPageModel> logger)
        {
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public StudentVM Student { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                                    .Include(s => s.Enrollments)
                                    .ThenInclude(e => e.Course)
                                    .FirstAsync(s => s.ID == id);
            
            if (student == null)
            {
                return NotFound();
            }
            else
            {
                Student = ToModelView(student);
                PopulateAssignedCourseData(_context, student);
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(int? id, string[] selectedCourses)
        {
            var studentToUpdate = await _context.Students
                                            .Include(s => s.Enrollments)
                                            .ThenInclude(e => e.Course)
                                            .FirstAsync(s => s.ID == id);

            if (studentToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Student>(
                studentToUpdate,
                "student",
                s => s.FirstMidName, s => s.LastName, s => s.EnrollmentDate))
            {
                UpdateStudentEnrollments(selectedCourses, studentToUpdate);
                await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }
            else
            {
                UpdateStudentEnrollments(selectedCourses, studentToUpdate);
                PopulateAssignedCourseData(_context, studentToUpdate);
                return Page();
            }
        }

        private StudentVM ToModelView(Student student)
        {
            return new StudentVM()
            {
                ID = student.ID,
                LastName = student.LastName,
                FirstMidName = student.FirstMidName,
                EnrollmentDate = student.EnrollmentDate
            };
        }

        private void UpdateStudentEnrollments(string[] selectedCourses,
                                             Student studentToUpdate)
        {
            if (selectedCourses == null)
            {
                studentToUpdate.Enrollments = [];
                return;
            }

            var selectedCoursesHS = new HashSet<string>(selectedCourses);
            HashSet<int> studentCourses = new();

            studentToUpdate ??= new Student();

            if (studentToUpdate.Enrollments != null)
            {
                studentCourses = new HashSet<int>(studentToUpdate.Enrollments.Select(e => e.CourseID));
            }
            else
            {
                studentToUpdate.Enrollments = [];
            }

            foreach(var course in _context.Courses)
            {
                if (selectedCoursesHS.Contains(course.CourseID.ToString()))
                {
                    if (!studentCourses.Contains(course.CourseID))
                    {
                        var newEnrollment = new Enrollment()
                        {
                            StudentID = studentToUpdate.ID,
                            CourseID = course.CourseID
                        };
                        
                        _context.Enrollments.Add(newEnrollment);
                    }
                }
                else
                {
                    if (studentCourses.Contains(course.CourseID))
                    {
                        var enrollmentToRemove = studentToUpdate.Enrollments.Single(e => e.CourseID == course.CourseID);
                        studentToUpdate.Enrollments.Remove(enrollmentToRemove);
                    }
                }
            }
        }
    }
}
