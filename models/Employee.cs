namespace DevTycoonCS.Models
{
    public abstract class Employee
    {
        public string Name { get; set; }
        public double Salary { get; set; }
        public double Productivity { get; set; }
        public double ProductivityMultiplier { get; set; } = 1.0; // Yeni

        protected Employee(string name, double salary, double productivity)
        {
            Name = name;
            Salary = salary;
            Productivity = productivity;
        }

        public abstract double Work();
    }

    public class JuniorDeveloper : Employee
    {
        public JuniorDeveloper(string name) : base(name, 3000, 5) { }
        public override double Work() => Productivity * ProductivityMultiplier;
    }

    public class SeniorDeveloper : Employee
    {
        public SeniorDeveloper(string name) : base(name, 8000, 15) { }
        public override double Work() => Productivity * ProductivityMultiplier;
    }
}
