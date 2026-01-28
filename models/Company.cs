using System.Collections.Generic;

namespace DevTycoonCS.Models
{
    public class Company
    {
        public string Name { get; set; }
        public double Balance { get; set; }
        public List<Employee> Employees { get; set; }

        public Company(string name, double initialBalance)
        {
            Name = name;
            Balance = initialBalance;
            Employees = new List<Employee>();
        }

        public void HireEmployee(Employee employee)
        {
            Employees.Add(employee);
        }

        public void EarnRevenue(double amount)
        {
            Balance += amount;
        }

        public void PaySalaries()
        {
            foreach (var emp in Employees)
            {
                Balance -= emp.Salary;
            }
        }
    }
}
