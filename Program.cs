using LINQ_MSSQL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LINQ_MSSQL
{
    class Program
    {
        static private EmployeesContext _context = new EmployeesContext();

        static void Main(string[] args)
        {
            //Console.WriteLine(GetEmployeesInformation());
            //Console.WriteLine(HighlyPaidEmployees());
            //Console.WriteLine(RelocationEmployees());
            //Console.WriteLine(ProjectAudit());
            //Console.WriteLine(DossierOnEmployee(7));
            //Console.WriteLine(SmallDepartments());
            //Console.WriteLine(SalaryIncrease(1, 15));
            //Console.WriteLine(CleanTheLogs(10));
            //Console.WriteLine(City404(1));
        }

        static string GetEmployeesInformation()
        {
            using (var db = new EmployeesContext())
            {
                var employees = from e in db.Employees
                          orderby e.EmployeeId
                          select e;

                var sb = new StringBuilder();

                foreach (var e in employees)
                {
                    sb.AppendLine($"{e.LastName} {e.FirstName} {e.MiddleName}; {e.JobTitle}.");
                }

                return sb.ToString().TrimEnd();
            }
        }

        static string HighlyPaidEmployees()
        {
            using (var db = new EmployeesContext())
            {
                IQueryable<Employees> emp = db.Employees;
                IQueryable<Employees> temp =
                    from e in emp
                    where e.Salary >= 48000
                    select e;

                var sb = new StringBuilder();

                foreach (var e in emp)
                {
                    sb.AppendLine($"{e.FirstName} {e.LastName} {e.MiddleName}; {e.JobTitle}; {e.DepartmentId}, {e.ManagerId}; {e.HireDate}; {e.Salary}; {e.AddressId}.");
                }

                return sb.ToString().TrimEnd();
            }
        }

        static string RelocationEmployees()
        {
            using (var db = new EmployeesContext())
            {
                string newAddress = "234 SomeAddress";

                var address = new Addresses()
                {
                    AddressText = newAddress
                };

                db.Addresses.Add(address);
                db.SaveChanges();

                var employees = from e in db.Employees
                                where e.LastName == "Brown"
                                select e;

                foreach (var e in employees)
                {
                    e.Address = address;
                }

                db.SaveChanges();

                return "Done.";
            }
        }

        static string ProjectAudit()
        {
            using (var db = new EmployeesContext())
            {
                DateTime startDate = DateTime.Parse("2002/01/01 00:00:00");
                DateTime endDate = DateTime.Parse("2006/01/01 00:00:00");

                var employees = (from p in db.Projects
                                 where p.StartDate >= startDate
                                 where p.EndDate < endDate
                                 join ep in db.EmployeesProjects
                                 on p.ProjectId equals ep.ProjectId
                                 join e in db.Employees
                                 on ep.EmployeeId equals e.EmployeeId
                                 select new 
                                 {
                                     e.EmployeeId,
                                     e.FirstName,
                                     e.LastName
                                 }).Distinct().Take(5).ToList();

                var sb = new StringBuilder();

                for (int i = 0; i < employees.Count(); i++)
                {
                    sb.AppendLine($"\n----------------------\nEmployeer: {employees[i].FirstName} {employees[i].LastName}.");

                    var info = (from e in db.Employees
                                where e.EmployeeId == employees[i].EmployeeId
                                join m in db.Employees
                                on e.ManagerId equals m.EmployeeId
                                join ep in db.EmployeesProjects
                                on e.EmployeeId equals ep.EmployeeId
                                join p in db.Projects
                                on ep.ProjectId equals p.ProjectId
                                select new
                                {
                                    ManFirst = m.FirstName,
                                    ManLast = m.LastName,
                                    Name = p.Name,
                                    StartDate = p.StartDate,
                                    EndDate = p.EndDate
                                }).ToList();

                    if (info[0].ManFirst != null)
                    {
                        sb.AppendLine($"Manager: {info[0].ManFirst} {info[0].ManLast}");
                    }
                    else sb.AppendLine("No manager.");
                    
                    sb.AppendLine("\nProjects:");

                    foreach (var e in info)
                    {
                        sb.AppendLine($"Name: {e.Name}.");
                        sb.Append($"Beginning: {e.StartDate}; ");

                        if (e.EndDate != null) sb.AppendLine($"The end: {e.EndDate}.");
                        else sb.AppendLine("Not completed.");
                    }
                }

                return sb.ToString().TrimEnd();
            }
        }

        static string DossierOnEmployee(int id)
        {
            using (var db = new EmployeesContext())
            {
                var employees = (from e in db.Employees
                                 where e.EmployeeId == id
                                 join ep in db.EmployeesProjects
                                 on e.EmployeeId equals ep.EmployeeId
                                 join p in db.Projects
                                 on ep.ProjectId equals p.ProjectId
                                 select new
                                 {
                                     e.FirstName,
                                     e.LastName,
                                     e.MiddleName,
                                     e.JobTitle,
                                     p.Name
                                 }).ToList();

                var sb = new StringBuilder();

                sb.AppendLine("Employeer: ");
                sb.AppendLine($"{employees[0].FirstName} {employees[0].LastName} {employees[0].MiddleName}; {employees[0].JobTitle}");

                sb.AppendLine("\nProjects: ");
                foreach (var e in employees)
                {
                    sb.AppendLine($"{e.Name}");
                }

                return sb.ToString().TrimEnd();
            }
        }

        static string SmallDepartments()
        {
            using (var db = new EmployeesContext())
            {
                var departmanents = from d in db.Departments
                                    join e in db.Employees
                                    on d.DepartmentId equals e.DepartmentId
                                    group d by d.Name into g
                                    where g.Count() <= 6
                                    select new
                                    {
                                        Key = g.Key,
                                        Count = g.Count()
                                    };

                var sb = new StringBuilder();

                foreach (var e in departmanents)
                {
                    sb.AppendLine($"{e.Key}: {e.Count}");
                }

                return sb.ToString().TrimEnd();
            }
        }

        static string SalaryIncrease(int department_id, decimal percent)
        {
            using (var db = new EmployeesContext())
            {
                var employees = from e in db.Employees
                                where e.DepartmentId == department_id
                                select e;

                foreach (var e in employees)
                {
                    e.Salary = e.Salary * (1 + percent / 100);
                }

                db.SaveChanges();

                var sb = new StringBuilder();

                sb.AppendLine($"Salary increased.");

                return sb.ToString().TrimEnd();
            }
        }

        static string CleanTheLogs(int department_id)
        {
            using (var db = new EmployeesContext())
            {
                var departments = from d in db.Departments
                                  select d;

                var newDepartment = new Departments();

                foreach (var d in departments)
                {
                    if (d.DepartmentId == department_id)
                    {
                        d.ManagerId = null;
                    }
                    else newDepartment = d;
                }
                db.SaveChanges();
                var employees = from e in db.Employees
                                           where e.DepartmentId == department_id
                                           select e;

                foreach (var e in employees)
                {
                    e.DepartmentId = newDepartment.DepartmentId;
                    e.ManagerId = newDepartment.ManagerId;
                }
                db.SaveChanges();

                var department = (from d in db.Departments
                                  select d).First(d => d.DepartmentId == department_id);

                db.Departments.Remove(department);
                db.SaveChanges();

                return $"The department {department_id} has been removed.";
            }
        }

        static string City404(int town_id)
        {
            using (var db = new EmployeesContext())
            {
                var addresses = from a in db.Addresses
                                where a.TownId == town_id
                                select a;

                foreach (var a in addresses)
                {
                    a.TownId = null;
                }
                db.SaveChanges();

                var town = (from t in db.Towns
                            select t).FirstOrDefault(t => t.TownId == town_id);

                db.Towns.Remove(town);
                db.SaveChanges();

                var sb = new StringBuilder();
                sb.AppendLine($"Town removed.");
                return sb.ToString().TrimEnd();
            }
        }
    }
}
