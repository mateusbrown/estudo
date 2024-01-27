using ContosoUniversity.Models;
using ContosoUniversity.Models.SchoolViewModels;  // Add VM
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoUniversity.Pages.Instructors
{
    public class IndexModel : PageModel
    {
        private readonly ContosoUniversity.Data.SchoolContext _context;

        public IndexModel(ContosoUniversity.Data.SchoolContext context)
        {
            _context = context;
        }

        public InstructorIndexData InstructorData { get; set; } = default!;
        public int InstructorID { get; set; }
        public int CourseID { get; set; }

        public async Task OnGetAsync(int? id, int? courseID)
        {
            var instructors = await _context.Instructors
                                        .Include(i => i.OfficeAssignment)
                                        .Include(i => i.Courses)
                                        .ThenInclude(c => c.Department)
                                        .OrderBy(i => i.LastName)
                                        .AsNoTracking()
                                        .ToListAsync();

            
            if (instructors != null)
            {
                InstructorData = new()
                {
                    Instructors = instructors
                };

                if (id != null)
                {
                    InstructorID = id.Value;
                    Instructor instructor = InstructorData.Instructors
                        .Where(i => i.ID == id.Value).Single();
                    InstructorData.Courses = instructor.Courses;
                }

                if (courseID != null)
                {
                    CourseID = courseID.Value;
                    IEnumerable<Enrollment> Enrollments = await _context.Enrollments
                        .Where(x => x.CourseID == CourseID)                    
                        .Include(i=>i.Student)
                        .ToListAsync();                 
                    InstructorData.Enrollments = Enrollments;
                }
            }
        }
    }
}