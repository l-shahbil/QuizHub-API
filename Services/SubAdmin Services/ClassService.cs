using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;
using QuizHub.Constant;
using QuizHub.Data.Repository.Base;
using QuizHub.Models;
using QuizHub.Models.DTO.Class;
using QuizHub.Models.DTO.Subject;
using QuizHub.Models.DTO.User.Student;
using QuizHub.Services.SubAdmin_Services.Interface;

namespace QuizHub.Services.SubAdmin_Services
{
    public class ClassService : IClassService
    {
        private readonly IRepository<Class> _classRepo;
        private readonly IRepository<AppUser> _userRepo;
        private readonly UserManager<AppUser> _userManager;
        private readonly IRepository<StudentClass> _studentClassRepo;
        private readonly IRepository<Subject> _subjectRepo;
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<UserDepartment> _userDepartmentRepo;
        private readonly IRepository<Batch> _batchRepo;

        public ClassService(IRepository<Class> classRepo, IRepository<AppUser> userRepo, UserManager<AppUser> userManager, IRepository<StudentClass> studentClassRepo, IRepository<Subject> subjectRepo, IRepository<Department> departmentRepo, IRepository<UserDepartment> userDepartmentRepo, IRepository<Batch> batchRepo)
        {
            _classRepo = classRepo;
            _userRepo = userRepo;
            _userManager = userManager;
            _studentClassRepo = studentClassRepo;
            _subjectRepo = subjectRepo;
            _departmentRepo = departmentRepo;
            _userDepartmentRepo = userDepartmentRepo;
            _batchRepo = batchRepo;
        }
        public async Task<ClassViewDto> AddClasssAsync(ClassCreateDto model, string subAdminEmail, int departmentId, int subjectId, string teacherEmail)
        {
            var existDepartment = await _departmentRepo.GetIncludeById(departmentId, "SubAdmin", "Subjects", "UserDepartments");

            if (existDepartment == null)
            {
                throw new ArgumentException($"A Department with ID {departmentId} not found.");
            }
            if (existDepartment.SubAdmin.Email != subAdminEmail)
            {
                throw new UnauthorizedAccessException("Access Denied: You are not the assigned SubAdmin for this department.");
            }


            var existClass = await _classRepo.SelecteOne(c => c.Name == model.Name);
            if (existClass != null)
            {
                throw new ArgumentException("A Class with the same name already exists.");
            }

            var existSubject = await _subjectRepo.SelecteOne(sub => sub.Id == subjectId);
            if (existSubject == null)
            {
                throw new ArgumentException($"A Subject with ID {subjectId} not found.");
            }
            var isAssingedSubjectToDepartment = existDepartment.Subjects.FirstOrDefault(s => s.Name == existSubject.Name);
            if (isAssingedSubjectToDepartment == null)
            {
                throw new ArgumentException($"A Subject not assigned to department.");
            }

            var existTeacher = await _userRepo.SelecteOne(u => u.Email == teacherEmail);
            if (existTeacher == null)
            {
                throw new ArgumentException($"A Teacher with Email {teacherEmail} not found.");
            }
            var isAssingedTeacherToDepartment = existDepartment.UserDepartments.FirstOrDefault(u => u.userId == existTeacher.Id);
            if (isAssingedTeacherToDepartment == null)
            {
                throw new ArgumentException($"A Teacher not assigned to department.");

            }

            var newClass = new Class()
            {
                Name = model.Name,
                Description = model.Description,
                Teacher = existTeacher,
                Subject = existSubject,
                Department = existDepartment

            };
            await _classRepo.AddAsyncEntity(newClass);
            return new ClassViewDto
            {
                Id = newClass.Id,
                Name = model.Name,
                Description = model.Description,
                TeacherName = $"{existTeacher.FirstName} {existTeacher.LastName}",
                SubjectName = existSubject.Name

            };
        }

        public async Task<bool> DeleteClasssAsync(int id, string subAdminEmail)
        {

            Class classs = await _classRepo.GetByIdAsync(id);
            if (classs == null)
            {
                return false;
            }

            Department department = await _departmentRepo.GetIncludeById(classs.DepartmentId, "SubAdmin");
            if (department.SubAdmin.Email != subAdminEmail)
            {
                throw new UnauthorizedAccessException("Access Denied: You are not the assigned SubAdmin for this department.");
            }

            _classRepo.DeleteEntity(classs);
            return true;
        }

        public async Task<ClassViewDto> EditClasssAsync(ClassUpdateDto model, int id, string subAdminEmail)
        {
            var classs = await _classRepo.GetIncludeById(id, "Subject", "Teacher")
             ?? throw new KeyNotFoundException($"Class with ID {id} not found.");
            Department department = await _departmentRepo.GetIncludeById(classs.DepartmentId, "SubAdmin");
            if (department.SubAdmin.Email != subAdminEmail)
            {
                throw new UnauthorizedAccessException("Access Denied: You are not the assigned SubAdmin for this department.");
            }


            var existClass = await _classRepo.SelecteOne(c => c.Name == model.Name);
            if (existClass != null)
            {
                throw new ArgumentException("A Class with the same name already exists.");
            }

            if (!string.IsNullOrWhiteSpace(model.TeacherEmail))
            {
                var teacher = await _userRepo.SelecteOne(e => e.Email == model.TeacherEmail);
                if (teacher == null)
                {
                    throw new ArgumentException($"A Teacher with Email {model.TeacherEmail} not found.");
                }
                var isAssingedTeacherToDepartment = department.UserDepartments.FirstOrDefault(u => u.userId == teacher.Id);
                if (isAssingedTeacherToDepartment == null)
                {
                    throw new ArgumentException($"A Teacher not assigned to department.");

                }
                classs.Teacher = teacher;
            }
            if (!string.IsNullOrWhiteSpace(model.SubjectName))
            {
                var subject = await _subjectRepo.SelecteOne(s => s.Name == model.SubjectName);
                if (subject == null)
                {
                    throw new ArgumentException($"A Subject {model.SubjectName} not found.");
                }
                var isAssingedSubjectToDepartment = department.Subjects.FirstOrDefault(s => s.Name == subject.Name);
                if (isAssingedSubjectToDepartment == null)
                {
                    throw new ArgumentException($"A Subject not assigned to department.");
                }
                classs.Subject = subject;
            }

            classs.Name = string.IsNullOrWhiteSpace(model.Name) ? classs.Name : model.Name;
            classs.Description = string.IsNullOrWhiteSpace(model.Description) ? classs.Description : model.Description;

            _classRepo.UpdateEntity(classs);
            return new ClassViewDto
            {
                Id = classs.Id,
                Name = classs.Name,
                Description = classs.Description,
                SubjectName = classs.Subject.Name,
                TeacherName = $"{classs.Teacher.FirstName} {classs.Teacher.LastName}"
            };
        }

        public async Task<List<ClassViewDto>> GetAllClassesForSubAdminAsync(int departmentId, string subAdminEmail)
        {
            var department = await _departmentRepo.GetIncludeById(departmentId, "SubAdmin");
            if (department == null)
            {
                throw new ArgumentException($"A Department with ID {departmentId} not found.");
            }
            if (department.SubAdmin.Email != subAdminEmail)
            {
                throw new UnauthorizedAccessException("Access Denied: You are not the assigned SubAdmin for this department.");
            }
            var classes = await _classRepo.GetAllIncludeAsync("Teacher", "Subject", "Department");
            var filteredClasses = classes
       .Where(c => c.Department.Id == department.Id)
       .Select(c => new ClassViewDto
       {
           Id = c.Id,
           Name = c.Name,
           TeacherName = c.Teacher.Email,
           SubjectName = c.Subject.Name,
       })
       .ToList();

            return filteredClasses;
        }

        public async Task<List<ClassViewDto>> GetAllClassesForTeacherAsync(string userId)
        {
            var user = await _userRepo.GetIncludeById(userId, "Classes");

            var classes = user.Classes
       .Select(c => new ClassViewDto
       {
           Id = c.Id,
           Name = c.Name,
           TeacherName = user.Email,
           SubjectName = _subjectRepo.GetByIdAsync(user.Classes.Select(c => c.SubjectId).FirstOrDefault()).Result.Name,
       })
       .ToList();

            return classes;
        }

        public async Task<List<ClassViewDto>> GetAllClassesForStudentAsync(string userId)
        {

            var studentClasses = await _studentClassRepo.GetAllAsync();
            var classesForStudent = _classRepo.GetAllIncludeAsync("Teacher", "Subject", "Department").Result
                .Where(cls => studentClasses.Any(sc => sc.UserId == userId && sc.ClassId == cls.Id))
                .ToList();

            var allClassForStudent = classesForStudent.Select(cls => new ClassViewDto
            {
                Id = cls.Id,
                Name = cls.Name,
                Description = cls.Description,
                TeacherName = cls.Teacher?.Email,
                SubjectName = cls.Subject?.Name
            }).ToList();

            return allClassForStudent;
        }

        public async Task<bool> AddStudentToClass(int departmentId, string subAdminEmail, int classId, string studentEmail)
        {
            try
            {

            var department = await _departmentRepo.GetIncludeById(departmentId, "SubAdmin");
            if (department == null)
            {
                throw new ArgumentException("Department not found.");
            }
            if (department.SubAdmin.Email != subAdminEmail)
            {
                throw new UnauthorizedAccessException("Access Denied: You are not the assigned SubAdmin for this department.");
            }
            var classs = await _classRepo.GetIncludeById(classId, "StudentClasses");
            if (classs == null || classs.DepartmentId != departmentId)
            {
                throw new ArgumentException("Class not found.");

            }
            var student = await _userRepo.SelecteOne(s => s.Email == studentEmail);
            var IsStudentInDepartment = _userDepartmentRepo.GetAllAsync().Result.FirstOrDefault(s => s.userId == student.Id && s.departmentId == departmentId);
            if (student == null || IsStudentInDepartment == null || !_userManager.IsInRoleAsync(student, Roles.Student.ToString()).Result)
            {
                throw new ArgumentException("Student not found.");
            }

            var IsSudentInClass = _studentClassRepo.GetAllAsync().Result.FirstOrDefault(s => s.ClassId == classId && s.UserId == student.Id);

            if (IsSudentInClass != null)
            {
                throw new ArgumentException("Student already exists in the class.");

            }

            StudentClass stdClass = new StudentClass()
            {
                Class = classs,
                User = student
            };

            await _studentClassRepo.AddAsyncEntity(stdClass);

            return true;
            }catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<bool> DeleteStudentFromClass(int departmentId, string subAdminEmail, int classId, string studentEmail)
        {
            try
            {

                var department = await _departmentRepo.GetIncludeById(departmentId, "SubAdmin");
                if (department == null)
                {
                    throw new ArgumentException("Department not found.");
                }
                if (department.SubAdmin.Email != subAdminEmail)
                {
                    throw new UnauthorizedAccessException("Access Denied: You are not the assigned SubAdmin for this department.");
                }
                var classs = await _classRepo.GetIncludeById(classId, "StudentClasses");
                if (classs == null || classs.DepartmentId != departmentId)
                {
                    throw new ArgumentException("Class not found.");

                }
                var student = await _userRepo.SelecteOne(s => s.Email == studentEmail);
                var IsStudentInDepartment = _userDepartmentRepo.GetAllAsync().Result.FirstOrDefault(s => s.userId == student.Id && s.departmentId == departmentId);
                if (student == null || IsStudentInDepartment == null || !_userManager.IsInRoleAsync(student, Roles.Student.ToString()).Result)
                {
                    throw new ArgumentException("Student not found.");
                }

                var IsSudentInClass = _studentClassRepo.GetAllAsync().Result.FirstOrDefault(s => s.ClassId == classId && s.UserId == student.Id);

                if (IsSudentInClass == null)
                {
                    throw new ArgumentException("Student not found in class.");

                }



                _studentClassRepo.DeleteEntity(IsSudentInClass);

                return true;
            }
            catch (Exception ex) 
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<bool> AddBatchToClass(int departmentId, string subAdminEmail, int classId, int batchId)
        {
            try
            {

            var department = await _departmentRepo.GetIncludeById(departmentId, "SubAdmin");
            if (department == null)
            {
                throw new ArgumentException("Department not found.");
            }
            if (department.SubAdmin.Email != subAdminEmail)
            {
                throw new UnauthorizedAccessException("Access Denied: You are not the assigned SubAdmin for this department.");
            }
            var classs = await _classRepo.GetIncludeById(classId, "StudentClasses");
            if (classs == null || classs.DepartmentId != departmentId)
            {
                throw new ArgumentException("Class not found.");

            }

            var batch = await _batchRepo.GetIncludeById(batchId, "Students");
            if (batch == null || batch.DepartmentId != departmentId)
            {
                throw new ArgumentException("Batch not found.");

            }



                foreach (var student in batch.Students)
                {
                    StudentClass stdClass = new StudentClass()
                    {
                        Class = classs,
                        User = student
                    };
                    if(classs.StudentClasses.FirstOrDefault(sc=> sc.UserId == student.Id) == null)
                    {
                         await _studentClassRepo.AddAsyncEntity(stdClass);
                    }
                }
                return true;
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            
        }
        public async Task<List<StudentViewDto>> GetAllStudentInClass(int departmentId, string subAdminEmail, int classId)
        {

            var department = await _departmentRepo.GetIncludeById(departmentId, "SubAdmin");
            if (department == null)
            {
                throw new ArgumentException("Department not found.");
            }
            if (department.SubAdmin.Email != subAdminEmail)
            {
                throw new UnauthorizedAccessException("Access Denied: You are not the assigned SubAdmin for this department.");
            }
            var classs = await _classRepo.GetIncludeById(classId, "StudentClasses");
            if (classs == null || classs.DepartmentId != departmentId)
            {
                throw new ArgumentException("Class not found.");

            }

            List<StudentViewDto> students = (await _userRepo.GetAllIncludeAsync("StudentClasses"))
                .Where(s => s.StudentClasses.Any(sc => sc.ClassId == classs.Id))
                .Select(s => new StudentViewDto
                {
                    Email = s.Email,
                    FirstName = s.FirstName,
                    LastName = s.LastName
                })
                .ToList();
            return students;
        }


    }
}