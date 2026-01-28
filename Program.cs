using Raylib_cs;
using System.Numerics;
using System.Collections.Generic;
using DevTycoonCS.Models;

namespace DevTycoonCS
{
    class Program
    {
        static void Main()
        {
            const int screenWidth = 1024;
            const int screenHeight = 768;
            Raylib.InitWindow(screenWidth, screenHeight, "Funny Lil DevTycoon - Chaos Edition");
            Raylib.SetTargetFPS(60);
            Texture2D deskTex = Raylib.LoadTexture("assets/desk.png");
            Texture2D juniorTex = Raylib.LoadTexture("assets/dev_junior.png");
            Texture2D seniorTex = Raylib.LoadTexture("assets/dev_senior.png");
            Texture2D teaTex = Raylib.LoadTexture("assets/npc_tea.png");
            EconomyManager economy = new EconomyManager(50000);
            Company myCompany = new Company("Clean Code Inc.", 0); 
            myCompany.HireEmployee(new JuniorDeveloper("Dev 1"));

            int day = 1;
            float dayTimer = 0;
            float teaBuffTimer = 0;
            bool teaActive = false;
            int maxDesks = 2;
            
            // Server Crash
            bool isServerCrashed = false;
            int rebootClicksNeeded = 0;
            Rectangle rebootBtn = new Rectangle(362, 400, 300, 60);


            List<(string text, Vector2 pos, float life, Color color)> floatingTexts = new List<(string, Vector2, float, Color)>();

            Rectangle teaBtn = new Rectangle(800, 20, 180, 35);
            Rectangle loanBtn = new Rectangle(800, 60, 180, 35);
            Rectangle payBtn = new Rectangle(800, 100, 180, 35);
            Rectangle expandBtn = new Rectangle(800, 140, 180, 35);
            Rectangle hireBtn = new Rectangle(20, 140, 180, 35);

            while (!Raylib.WindowShouldClose())
            {
                float dt = Raylib.GetFrameTime();
                Vector2 mousePos = Raylib.GetMousePosition();


                if (isServerCrashed)
                {
                    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                    {
                        if (Raylib.CheckCollisionPointRec(mousePos, rebootBtn))
                        {
                            rebootClicksNeeded--;
                            if (rebootClicksNeeded <= 0)
                            {
                                isServerCrashed = false;
                                floatingTexts.Add(("SYSTEM RESTORED!", new Vector2(300, 300), 2.0f, Color.Lime));
                            }
                        }
                    }
                }
                else
                {
                    if (Raylib.GetRandomValue(0, 1000) < 2) 
                    {
                        isServerCrashed = true;
                        rebootClicksNeeded = 5; 
                        teaActive = false; 
                    }

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
                        double income = 0;
                        foreach (var emp in myCompany.Employees) income += emp.Work() * 15 * multiplier;
                        economy.AddMoney(income);
                        floatingTexts.Add(($"+${income:F0}", new Vector2(250, 20), 2.0f, Color.Lime));

                        if (day % 30 == 0)
                        {
                            double salaries = 0;
                            foreach (var emp in myCompany.Employees) salaries += emp.Salary;
                            economy.SpendMoney(salaries + (maxDesks * 500));
                            floatingTexts.Add(("-PAYDAY", new Vector2(20, 20), 3.0f, Color.Red));
                        }
                        dayTimer = 0;
                    }
                    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                    {
                        if (Raylib.CheckCollisionPointRec(mousePos, teaBtn) && !teaActive && economy.SpendMoney(500))
                        {
                            teaActive = true;
                            teaBuffTimer = 10.0f;
                            floatingTexts.Add(("TEA TIME!", new Vector2(mousePos.X, mousePos.Y), 2.0f, Color.Gold));
                        }
                        
                        if (Raylib.CheckCollisionPointRec(mousePos, loanBtn))
                        {
                            economy.TakeLoan(10000, 0.30);
                            floatingTexts.Add(("+10k DEBT", new Vector2(mousePos.X, mousePos.Y), 2.0f, Color.Orange));
                        }

                        if (Raylib.CheckCollisionPointRec(mousePos, payBtn))
                        {
                            economy.PayDebt(5000);
                        }

                        if (Raylib.CheckCollisionPointRec(mousePos, expandBtn) && economy.SpendMoney(15000))
                        {
                            maxDesks++;
                        }

                        if (Raylib.CheckCollisionPointRec(mousePos, hireBtn) && myCompany.Employees.Count < maxDesks && economy.SpendMoney(5000))
                        {
                            myCompany.HireEmployee(new JuniorDeveloper($"Dev {myCompany.Employees.Count + 1}"));
                        }

                        for (int i = 0; i < myCompany.Employees.Count; i++)
                        {
                            int row = i / 4; int col = i % 4;
                            Rectangle deskBounds = new Rectangle(50 + (col * 240), 250 + (row * 200), 128, 96);
                            if (Raylib.CheckCollisionPointRec(mousePos, deskBounds))
                            {
                                double cost = 3000 * myCompany.Employees[i].ProductivityMultiplier;
                                if (economy.SpendMoney(cost))
                                {
                                    myCompany.Employees[i].ProductivityMultiplier += 0.5;
                                    floatingTexts.Add(("LEVEL UP!", new Vector2(mousePos.X, mousePos.Y), 1.5f, Color.SkyBlue));
                                }
                            }
                        }
                    }
                }
                Raylib.BeginDrawing();
                
                if (isServerCrashed)
                {
                    // Screen
                    Raylib.ClearBackground(new Color(0, 0, 139, 255)); 
                    Raylib.DrawText(":(", 50, 50, 100, Color.White);
                    Raylib.DrawText("Your Server ran into a problem.", 50, 180, 30, Color.White);
                    Raylib.DrawText("Error Code: SEGMENTATION_FAULT_CORE_DUMPED", 50, 230, 20, Color.Yellow);
                    
                    // Reboot Button
                    Raylib.DrawRectangleRec(rebootBtn, Color.White);
                    Raylib.DrawText($"CLICK TO REBOOT ({rebootClicksNeeded})", 380, 415, 20, Color.Black);
                }
                else
                {
                    // GameScreen
                    Raylib.ClearBackground(new Color(30, 30, 35, 255));

                    // Panel
                    Raylib.DrawRectangle(0, 0, screenWidth, 190, new Color(20, 20, 25, 255));
                    Raylib.DrawText($"Bank: ${economy.Balance:F0}", 20, 20, 35, Color.Lime);
                    Raylib.DrawText($"Debt: ${economy.Debt:F0}", 20, 65, 22, Color.Orange);
                    Raylib.DrawText($"Day: {day} | Cap: {myCompany.Employees.Count}/{maxDesks}", 20, 100, 20, Color.LightGray);

                    // buttons
                    Raylib.DrawRectangleRec(teaBtn, Color.Brown); Raylib.DrawText("TEA ($500)", 850, 28, 16, Color.White);
                    Raylib.DrawRectangleRec(loanBtn, Color.Maroon); Raylib.DrawText("LOAN ($10k)", 850, 68, 16, Color.White);
                    Raylib.DrawRectangleRec(payBtn, Color.DarkGreen); Raylib.DrawText("PAY ($5k)", 850, 108, 16, Color.White);
                    Raylib.DrawRectangleRec(expandBtn, Color.DarkBlue); Raylib.DrawText("EXPAND ($15k)", 835, 148, 16, Color.White);
                    if(myCompany.Employees.Count < maxDesks) {
                        Raylib.DrawRectangleRec(hireBtn, Color.DarkPurple); Raylib.DrawText("HIRE ($5k)", 65, 148, 16, Color.White);
                    }

                    // Workers
                    for (int i = 0; i < myCompany.Employees.Count; i++)
                    {
                        int row = i / 4; int col = i % 4;
                        int x = 50 + (col * 240); int y = 250 + (row * 200);

                        Raylib.DrawTexture(deskTex, x, y, Color.White);
                        Texture2D charTex = (myCompany.Employees[i] is SeniorDeveloper) ? seniorTex : juniorTex;
                        Raylib.DrawTexture(charTex, x + 32, y - 20, Color.White);
                        
                        double upgCost = 3000 * myCompany.Employees[i].ProductivityMultiplier;
                        Raylib.DrawRectangle(x, y + 100, 130, 45, new Color(0, 0, 0, 150));
                        Raylib.DrawText($"Lvl: {myCompany.Employees[i].ProductivityMultiplier:F1}", x + 5, y + 105, 15, Color.SkyBlue);
                        Raylib.DrawText($"UPG: ${upgCost:F0}", x + 5, y + 125, 13, Color.Gray);
                    }

                    if (teaActive) {
                        Raylib.DrawText($"TEA BUFF: {teaBuffTimer:F1}s", 350, 20, 30, Color.Gold);
                        Raylib.DrawTextureEx(teaTex, new Vector2(480, 100), 0, 0.8f, Color.White);
                    }
                    for (int i = floatingTexts.Count - 1; i >= 0; i--) {
                        var ft = floatingTexts[i];
                        ft.life -= dt; ft.pos.Y -= 30 * dt;
                        if (ft.life <= 0) floatingTexts.RemoveAt(i);
                        else {
                            Raylib.DrawText(ft.text, (int)ft.pos.X, (int)ft.pos.Y, 20, Raylib.Fade(ft.color, ft.life / 2.0f));
                            floatingTexts[i] = ft;
                        }
                    }
                }

                Raylib.EndDrawing();
            }
            Raylib.CloseWindow();
        }
    }
}
