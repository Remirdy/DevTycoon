namespace DevTycoonCS.Models
{
    public class EconomyManager
    {
        public double Balance { get; private set; }
        public double Debt { get; private set; }
        public double DailyRent { get; set; } = 100;

        public EconomyManager(double initialBalance)
        {
            Balance = initialBalance;
        }

        public void AddMoney(double amount) => Balance += amount;
        
        public bool SpendMoney(double amount)
        {
            if (Balance >= amount)
            {
                Balance -= amount;
                return true;
            }
            return false;
        }

        public void TakeLoan(double amount, double interest)
        {
            Balance += amount;
            Debt += (amount * (1 + interest));
        }

        public void PayDebt(double amount)
        {
            double actualPayment = System.Math.Min(System.Math.Min(amount, Debt), Balance);
            Balance -= actualPayment;
            Debt -= actualPayment;
        }
    }
}
