using Raylib_cs;
using System.Numerics;
using DevTycoonCS.Models;

namespace DevTycoonCS
{
    class Program
    {
        static void Main()
        {
            const int screenWidth = 1024;
            const int screenHeight = 768;

            Raylib.InitWindow(screenWidth, screenHeight, "Dev Tycoon C# - Office Expansion");
            Raylib.SetTargetFPS(60);

            Texture2D deskTex = Raylib.LoadTexture("assets/desk.png");
            Texture2D juniorTex = Raylib.LoadTexture("assets/dev_junior.png");
            Texture2D seniorTex = Raylib.LoadTexture("assets/dev_senior.png");
            Texture2D teaTex = Raylib.LoadTexture("assets/npc_tea.png");

            Company myCompany = new Company("Code Masters C#", 50000);
            myCompany.HireEmployee(new JuniorDeveloper("Dev 1"));
            
            int day = 1;
            float dayTimer = 0;
            float teaBuffTimer = 0;
            bool teaActive = false;
            double debt = 0;
            int maxDesks = 2;

            Rectangle teaBtn = new Rectangle(800, 20, 180, 35);
            Rectangle loanBtn = new Rectangle(800, 60, 180, 35);
            Rectangle payBtn = new Rectangle(800, 100, 180, 35);
            Rectangle expandBtn = new Rectangle(800, 140, 180, 35);
            Rectangle hireBtn = new Rectangle(20, 140, 180, 35);

            while (!Raylib.WindowShouldClose())
            {
                float dt = Raylib.GetFrameTime();
                Vector2 mousePos = Raylib.GetMousePosition();

                if (teaActive)
                {
                    teaBuffTimer -= dt;
                    if (teaBuffTimer <= 0) teaActive = false;
                }

                dayTimer += dt;
                if (dayTimer >= 1.0f)
                {
                    day++;
                    double multiplier = teaActive ? 2.5 : 1.0;
                    double dailyIncome = 0;
                    foreach (var emp in myCompany.Employees) dailyIncome += emp.Work() * 15 * multiplier;
                    myCompany.EarnRevenue(dailyIncome);

                    if (day % 30 == 0)
                    {
                        myCompany.PaySalaries();
                        myCompany.Balance -= (1500 * maxDesks); 
                    }
                    dayTimer = 0;
                }

                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    if (Raylib.CheckCollisionPointRec(mousePos, teaBtn) && myCompany.Balance >= 500 && !teaActive)
                    {
                        myCompany.Balance -= 500;
                        teaActive = true;
                        teaBuffTimer = 10.0f;
                    }
                    
                    if (Raylib.CheckCollisionPointRec(mousePos, loanBtn))
                    {
                        myCompany.Balance += 10000;
                        debt += 13000;
                    }

                    if (Raylib.CheckCollisionPointRec(mousePos, payBtn) && debt > 0 && myCompany.Balance >= 5000)
                    {
                        myCompany.Balance -= 5000;
                        debt -= 5000;
                    }

                    if (Raylib.CheckCollisionPointRec(mousePos, expandBtn) && myCompany.Balance >= 15000)
                    {
                        myCompany.Balance -= 15000;
                        maxDesks++;
                    }

                    if (Raylib.CheckCollisionPointRec(mousePos, hireBtn) && myCompany.Employees.Count < maxDesks && myCompany.Balance >= 5000)
                    {
                        myCompany.Balance -= 5000;
                        myCompany.HireEmployee(new JuniorDeveloper($"Dev {myCompany.Employees.Count + 1}"));
                    }

                    for (int i = 0; i < myCompany.Employees.Count; i++)
                    {
                        int row = i / 4;
                        int col = i % 4;
                        Rectangle deskBounds = new Rectangle(50 + (col * 240), 250 + (row * 200), 128, 96);
                        
                        if (Raylib.CheckCollisionPointRec(mousePos, deskBounds))
                        {
                            double upgradeCost = 3000 * myCompany.Employees[i].ProductivityMultiplier;
                            if (myCompany.Balance >= upgradeCost)
                            {
                                myCompany.Balance -= upgradeCost;
                                myCompany.Employees[i].ProductivityMultiplier += 0.5;
                            }
                        }
                    }
                }

                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color(35, 35, 40, 255));

                Raylib.DrawRectangle(0, 0, screenWidth, 190, new Color(20, 20, 25, 255));
                Raylib.DrawText($"Balance: ${myCompany.Balance:F0}", 20, 20, 30, Color.Lime);
                Raylib.DrawText($"Debt: ${debt:F0}", 20, 60, 20, Color.Orange);
                Raylib.DrawText($"Office Capacity: {myCompany.Employees.Count}/{maxDesks}", 20, 100, 20, Color.LightGray);
                
                Raylib.DrawRectangleRec(teaBtn, Color.Brown);
                Raylib.DrawText("TEA ($500)", 850, 28, 16, Color.White);
                
                Raylib.DrawRectangleRec(loanBtn, Color.Maroon);
                Raylib.DrawText("LOAN ($10k)", 850, 68, 16, Color.White);

                Raylib.DrawRectangleRec(payBtn, Color.DarkGreen);
                Raylib.DrawText("PAY ($5k)", 850, 108, 16, Color.White);

                Raylib.DrawRectangleRec(expandBtn, Color.DarkBlue);
                Raylib.DrawText("EXPAND ($15k)", 835, 148, 16, Color.White);

                if (myCompany.Employees.Count < maxDesks)
                {
                    Raylib.DrawRectangleRec(hireBtn, Color.DarkPurple);
                    Raylib.DrawText("HIRE DEV ($5k)", 45, 148, 16, Color.White);
                }

                for (int i = 0; i < myCompany.Employees.Count; i++)
                {
                    int row = i / 4;
                    int col = i % 4;
                    int x = 50 + (col * 240);
                    int y = 250 + (row * 200);

                    Raylib.DrawTexture(deskTex, x, y, Color.White);
                    Texture2D charTex = (myCompany.Employees[i] is SeniorDeveloper) ? seniorTex : juniorTex;
                    Raylib.DrawTexture(charTex, x + 32, y - 20, Color.White);
                    
                    double cost = 3000 * myCompany.Employees[i].ProductivityMultiplier;
                    Raylib.DrawRectangle(x, y + 100, 130, 45, new Color(0, 0, 0, 150));
                    Raylib.DrawText($"Lvl: {myCompany.Employees[i].ProductivityMultiplier:F1}", x + 5, y + 105, 15, Color.SkyBlue);
                    Raylib.DrawText($"UPG: ${cost:F0}", x + 5, y + 125, 13, Color.Gray);
                }

                if (teaActive)
                {
                    Raylib.DrawText($"TEA TIME: {teaBuffTimer:F1}s", 400, 20, 35, Color.Gold);
                    Raylib.DrawTextureEx(teaTex, new Vector2(500, 120), 0, 0.7f, Color.White);
                }

                Raylib.EndDrawing();
            }

            Raylib.UnloadTexture(deskTex);
            Raylib.UnloadTexture(juniorTex);
            Raylib.UnloadTexture(seniorTex);
            Raylib.UnloadTexture(teaTex);
            Raylib.CloseWindow();
        }
    }
}
