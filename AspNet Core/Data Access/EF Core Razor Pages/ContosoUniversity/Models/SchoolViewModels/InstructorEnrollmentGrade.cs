namespace ContosoUniversity.Models.SchoolViewModels
{
    public class InstructorEnrollmentGradeModel
    {
        public int Key { get; set; }
        public string Name { get; set; } = "";
        public Grade Value { get; set; }
    }

    public class InstructorEnrollmentGrade
    {
        private static readonly Dictionary<string,int> _grades = ((Grade[])Enum.GetValues(typeof(Grade))).ToDictionary(k => k.ToString(), v => (int)v);
        
        public static List<InstructorEnrollmentGradeModel> GetGrades()
        {
            List<InstructorEnrollmentGradeModel> grades = [];
            foreach(var g in _grades)
            {
                grades.Add(new InstructorEnrollmentGradeModel()
                {
                    Name = g.Key,
                    Key = g.Value,
                    Value = GetGrade(g.Value)
                });
            }
            return grades;
        }

        public static Grade GetGrade(int key)
        {
            Grade grade = default!;
            
            foreach(var v in _grades.Values)
            {
                if (v == key)
                {
                    grade = (Grade)v;
                    break;
                }
            }

            return grade;
        }

        public static Grade GetGrade(string name)
        {
            Grade grade = default!;
            
            foreach(var key in _grades.Keys)
            {
                if (key == name)
                {
                    grade = (Grade)_grades.Single(g => g.Key == name).Value;
                    break;
                }
            }
            
            return grade;
        }
    }
}