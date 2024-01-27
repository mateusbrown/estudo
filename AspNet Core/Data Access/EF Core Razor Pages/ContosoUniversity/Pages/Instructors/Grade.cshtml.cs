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
using ContosoUniversity.Models.SchoolViewModels;

namespace ContosoUniversity.Pages.Instructors
{
    public class GradeModel : PageModel
    {
        private readonly ContosoUniversity.Data.SchoolContext _context;
        
        public GradeModel(ContosoUniversity.Data.SchoolContext context)
        {
            _context = context;
        }

        public List<InstructorEnrollmentGradeModel> GetGrades()
        {
            return InstructorEnrollmentGrade.GetGrades();
        }

        [BindProperty]
        public Enrollment Enrollment {get;set;} = default!;
        [BindProperty]
        public Instructor Instructor {get;set;} = default!;

        public async Task<IActionResult> OnGetAsync(int enrollmentID, int instructorID)
        {
            var enrollment = await _context.Enrollments
                                        .Include(e => e.Course)
                                        .Include(e => e.Student)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(e => e.EnrollmentID == enrollmentID);

            if (enrollment == null)
            {
                return NotFound();
            }
            else
            {
                Enrollment = enrollment;

                var instructor = await _context.Instructors.AsNoTracking().FirstOrDefaultAsync(i => i.ID == instructorID);
                if (instructor == null)
                {
                    return NotFound();
                }
                else
                {
                    Instructor = instructor;
                    var grades = GetGrades();
                    ViewData["Grades"] = new SelectList(grades, "Key", "Name", enrollment.Grade);
                    return Page();
                }
            }
        }

        public async Task<IActionResult> OnPostAsync(int enrollmentID, int instructorID)
        {
            var enrollmentToUpdate = await _context.Enrollments
                                            .FirstOrDefaultAsync(e => e.EnrollmentID == enrollmentID);

            if (enrollmentToUpdate == null)
            {
                return NotFound();
            }
            
            try
            {
                enrollmentToUpdate.Grade = Enrollment.Grade;

                _context.Enrollments.Update(enrollmentToUpdate);
                await _context.SaveChangesAsync();
                    return RedirectToPage("./Index",new {
                        id = instructorID,
                        courseID = enrollmentToUpdate.CourseID
                    });
            }
            catch
            {
                var enrollmentNoUpdate = await _context.Enrollments
                                            .Include(e => e.Course)
                                            .Include(e => e.Student)
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(e => e.EnrollmentID == enrollmentID);
                if (enrollmentNoUpdate == null)
                {
                    return NotFound();
                }

                var instructor = await _context.Instructors.AsNoTracking().FirstOrDefaultAsync(i => i.ID == instructorID);
                if (instructor == null)
                {
                    return NotFound();
                }
                else
                {
                    Enrollment = enrollmentNoUpdate;
                    Instructor = instructor;
                    var grades = GetGrades();
                    ViewData["Grades"] = new SelectList(grades, "Key", "Name", enrollmentToUpdate.Grade);
                    return Page();
                }
            }
        }
    }
}