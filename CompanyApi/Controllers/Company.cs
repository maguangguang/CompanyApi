using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace CompanyApi.Controllers
{
    public class Company
    {
        public Company()
        {
        }

        public Company(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public string CompanyID { get; set; } = $"C{Guid.NewGuid().ToString()}";
        public List<Employee> Employees { get; set; } = new List<Employee>();

        public override bool Equals(object obj)
        {
            var company = obj as Company;
            return company != null && Name.Equals(company.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public void UpdateBy(Company updatedCompany)
        {
            Name = updatedCompany.Name;
        }

        public Employee AddEmployee(Employee newEmployee)
        {
            Employees.Add(newEmployee);
            return newEmployee;
        }

        public Employee FindEmployee(string employeeID)
        {
            return Employees.Find(employee => employee.EmployeeID.Equals(employeeID));
        }

        public void DeleteEmployee(string employeeID)
        {
            Employees.RemoveAll(employee => employee.EmployeeID.Equals(employeeID));
        }
    }
}