using Raylib_cs;
using System.Numerics;
using System.Collections.Generic;
using DevTycoonCS.Models;
using System;

namespace DevTycoonCS
{
    enum GameState { Intro, MainMenu, Playing, GameOver }

    class Program
    {
        static Color ColorLerp(Color c1, Color c2, float amount)
        {
            byte r = (byte)(c1.R + (c2.R - c1.R) * amount);
            byte g = (byte)(c1.G + (c2.G - c1.G) * amount);
            byte b = (byte)(c1.B + (c2.B - c1.B) * amount);
            return new Color(r, g, b, (byte)255);
        }

        class MatrixDrop
        {
            public float X, Y, Speed;
            public string Char;
        }

        static void Main()
        {
            const int screenWidth = 1024;
            const int screenHeight = 768;
            Raylib.InitWindow(screenWidth, screenHeight, "Funny Lil DevTycoon - Bug Hunter");
            Raylib.SetTargetFPS(60);

            List<MatrixDrop> drops = new List<MatrixDrop>();
            Random rnd = new Random();
            for (int i = 0; i < screenWidth / 20; i++)
            {
                drops.Add(new MatrixDrop { 
                    X = i * 20, 
                    Y = rnd.Next(-1000, 0), 
                    Speed = rnd.Next(5, 15), 
                    Char = ((char)rnd.Next(33, 126)).ToString() 
                });
            }

            Texture2D deskTex = Raylib.LoadTexture("assets/desk.png");
            Texture2D juniorTex = Raylib.LoadTexture("assets/dev_junior.png");
            Texture2D seniorTex = Raylib.LoadTexture("assets/dev_senior.png");
            Texture2D teaTex = Raylib.LoadTexture("assets/npc_tea.png");

            EconomyManager economy = new EconomyManager(50000);
            Company myCompany = new Company("Clean Code Inc.", 0); 
            myCompany.HireEmployee(new JuniorDeveloper("Neo"));

            GameState currentState = GameState.Intro;
            float introTimer = 0;

            int day = 1;
            float dayTimer = 0;
            float teaBuffTimer = 0;
            bool teaActive = false;
            int maxDesks = 2;
            bool isServerCrashed = false;
            int rebootClicksNeeded = 0;
            Rectangle rebootBtn = new Rectangle(362, 400, 300, 60);

            List<Rectangle> activeBugs = new List<Rectangle>();
            float bugSpawnTimer = 0;

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

                if (currentState == GameState.Intro)
                {
                    introTimer += dt;
                    foreach (var drop in drops) {
                        drop.Y += drop.Speed;
                        if (rnd.Next(0, 20) == 0) drop.Char = ((char)rnd.Next(33, 126)).ToString();
                        if (drop.Y > screenHeight) { drop.Y = rnd.Next(-200, -20); drop.Speed = rnd.Next(5, 15); }
                    }
                    if (introTimer > 5.0f || Raylib.IsKeyPressed(KeyboardKey.Space)) currentState = GameState.MainMenu;
                }
                else if (currentState == GameState.MainMenu)
                {
                    if (Raylib.IsKeyPressed(KeyboardKey.Enter)) currentState = GameState.Playing;
                }
                else if (currentState == GameState.Playing)
                {
                    if (isServerCrashed) {
                        if (Raylib.IsMouseButtonPressed(MouseButton.Left) && Raylib.CheckCollisionPointRec(mousePos, rebootBtn)) {
                            rebootClicksNeeded--;
                            if (rebootClicksNeeded <= 0) { isServerCrashed = false; floatingTexts.Add(("SYSTEM RESTORED!", new Vector2(300, 300), 2.0f, Color.Lime)); }
                        }
                    } else {
                        bugSpawnTimer += dt;
                        if (bugSpawnTimer > rnd.Next(10, 20) && activeBugs.Count == 0) 
                        {
                            bugSpawnTimer = 0;
                            int bugCount = rnd.Next(3, 8);
                            for(int k=0; k<bugCount; k++) {
                                activeBugs.Add(new Rectangle(rnd.Next(50, 700), rnd.Next(200, 600), 30, 30));
                            }
                            floatingTexts.Add(("BUG INVASION!", new Vector2(300, 300), 3.0f, Color.Red));
                        }

                        if (Raylib.GetRandomValue(0, 3000) < 2) { isServerCrashed = true; rebootClicksNeeded = 5; teaActive = false; }
                        if (teaActive) { teaBuffTimer -= dt; if (teaBuffTimer <= 0) teaActive = false; }

                        dayTimer += dt;
                        if (dayTimer >= 3.0f) 
                        {
                            day++;
                            double mult = teaActive ? 2.5 : 1.0;
                            if (activeBugs.Count > 0) mult *= 0.5;

                            double inc = 0;
                            foreach (var emp in myCompany.Employees) inc += emp.Work() * 15 * mult;
                            
                            economy.AddMoney(inc);
                            floatingTexts.Add(($"+${inc:F0}", new Vector2(250, 20), 2.0f, Color.Lime));
                            
                            if (day % 30 == 0) {
                                double sal = 0; foreach (var emp in myCompany.Employees) sal += emp.Salary;
                                economy.SpendMoney(sal + (maxDesks * 500));
                                floatingTexts.Add(("-PAYDAY", new Vector2(20, 20), 3.0f, Color.Red));
                            }
                            dayTimer = 0;
                        }

                        if (Raylib.IsMouseButtonPressed(MouseButton.Left)) {
                            for (int i = activeBugs.Count - 1; i >= 0; i--) {
                                if (Raylib.CheckCollisionPointRec(mousePos, activeBugs[i])) {
                                    activeBugs.RemoveAt(i);
                                    economy.AddMoney(100);
                                    floatingTexts.Add(("BUG FIXED +$100", new Vector2(mousePos.X, mousePos.Y), 1.0f, Color.Green));
                                    goto SkipButtons; 
                                }
                            }

                            if (Raylib.CheckCollisionPointRec(mousePos, teaBtn) && !teaActive && economy.SpendMoney(500)) { teaActive = true; teaBuffTimer = 10.0f; floatingTexts.Add(("TEA TIME!", new Vector2(mousePos.X, mousePos.Y), 2.0f, Color.Gold)); }
                            if (Raylib.CheckCollisionPointRec(mousePos, loanBtn)) { economy.TakeLoan(10000, 0.30); floatingTexts.Add(("+10k DEBT", new Vector2(mousePos.X, mousePos.Y), 2.0f, Color.Orange)); }
                            if (Raylib.CheckCollisionPointRec(mousePos, payBtn)) economy.PayDebt(5000);
                            if (Raylib.CheckCollisionPointRec(mousePos, expandBtn) && economy.SpendMoney(15000)) maxDesks++;
                            if (Raylib.CheckCollisionPointRec(mousePos, hireBtn) && myCompany.Employees.Count < maxDesks && economy.SpendMoney(5000)) myCompany.HireEmployee(new JuniorDeveloper("Dev"));
                            
                            for (int i = 0; i < myCompany.Employees.Count; i++) {
                                Rectangle deskBounds = new Rectangle(50 + ((i % 4) * 240), 250 + ((i / 4) * 200), 128, 96);
                                if (Raylib.CheckCollisionPointRec(mousePos, deskBounds)) {
                                    double cost = 3000 * myCompany.Employees[i].ProductivityMultiplier;
                                    if (economy.SpendMoney(cost)) { myCompany.Employees[i].ProductivityMultiplier += 0.5; floatingTexts.Add(("LEVEL UP!", new Vector2(mousePos.X, mousePos.Y), 1.5f, Color.SkyBlue)); }
                                }
                            }

                            SkipButtons:;
                        }
                    }
                }

                Raylib.BeginDrawing();

                if (currentState == GameState.Intro)
                {
                    Raylib.ClearBackground(Color.Black);
                    foreach (var drop in drops) {
                        Raylib.DrawText(drop.Char, (int)drop.X, (int)drop.Y, 20, Color.Lime);
                        Raylib.DrawText(drop.Char, (int)drop.X, (int)drop.Y - 20, 20, new Color(0, 255, 0, 100));
                    }
                    Raylib.DrawText("THE MATRIX HAS YOU...", 300, 350, 40, Color.White);
                }
                else if (currentState == GameState.MainMenu)
                {
                    Raylib.ClearBackground(new Color(20, 20, 30, 255));
                    Raylib.DrawText("FUNNY LIL DEVTYCOON", 220, 300, 50, Color.Lime);
                    Raylib.DrawText("- Press ENTER to Start -", 360, 380, 20, Color.Gray);
                }
                else if (currentState == GameState.Playing)
                {
                    if (isServerCrashed) {
                        Raylib.ClearBackground(new Color(0, 0, 139, 255));
                        Raylib.DrawText(":(", 50, 50, 100, Color.White);
                        Raylib.DrawText("System Crash.", 50, 180, 30, Color.White);
                        Raylib.DrawRectangleRec(rebootBtn, Color.White);
                        Raylib.DrawText($"REBOOT ({rebootClicksNeeded})", 410, 415, 30, Color.Black);
                    } else {
                        Color morning = new Color(40, 40, 50, 255);
                        Color night = new Color(10, 10, 25, 255);
                        Raylib.ClearBackground(ColorLerp(morning, night, dayTimer / 3.0f));

                        Raylib.DrawRectangle(0, 0, screenWidth, 190, new Color(20, 20, 25, 255));
                        Raylib.DrawText($"Bank: ${economy.Balance:F0}", 20, 20, 35, Color.Lime);
                        Raylib.DrawText($"Debt: ${economy.Debt:F0}", 20, 65, 22, Color.Orange);
                        Raylib.DrawText($"Day {day}", 400, 45, 20, Color.White);
                        
                        Raylib.DrawRectangleRec(teaBtn, Color.Brown); Raylib.DrawText("TEA", 850, 28, 16, Color.White);
                        Raylib.DrawRectangleRec(loanBtn, Color.Maroon); Raylib.DrawText("LOAN", 850, 68, 16, Color.White);
                        Raylib.DrawRectangleRec(payBtn, Color.DarkGreen); Raylib.DrawText("PAY", 850, 108, 16, Color.White);
                        Raylib.DrawRectangleRec(expandBtn, Color.DarkBlue); Raylib.DrawText("EXPAND", 835, 148, 16, Color.White);
                        if(myCompany.Employees.Count < maxDesks) { Raylib.DrawRectangleRec(hireBtn, Color.DarkPurple); Raylib.DrawText("HIRE", 65, 148, 16, Color.White); }

                        for (int i = 0; i < myCompany.Employees.Count; i++) {
                            int col = i % 4; int row = i / 4; int x = 50 + (col * 240); int y = 250 + (row * 200);
                            Raylib.DrawTexture(deskTex, x, y, Color.White);
                            bool isSenior = myCompany.Employees[i].ProductivityMultiplier >= 3.0;
                            Raylib.DrawTexture(isSenior ? seniorTex : juniorTex, x + 32, y - 20, Color.White);
                            Raylib.DrawRectangle(x, y + 100, 130, 45, new Color(0, 0, 0, 150));
                            Raylib.DrawText(isSenior ? "SNR. DEV" : "JNR. DEV", x + 5, y + 105, 14, isSenior ? Color.Gold : Color.SkyBlue);
                            Raylib.DrawText($"Lvl: {myCompany.Employees[i].ProductivityMultiplier:F1}", x + 75, y + 105, 14, Color.White);
                        }
                        
                        foreach(var bug in activeBugs) {
                            Raylib.DrawRectangleRec(bug, Color.Red);
                            Raylib.DrawRectangleLinesEx(bug, 2, Color.Black);
                            Raylib.DrawLine((int)bug.X, (int)bug.Y, (int)bug.X-5, (int)bug.Y-5, Color.Black);
                            Raylib.DrawLine((int)bug.X+30, (int)bug.Y, (int)bug.X+35, (int)bug.Y-5, Color.Black);
                        }

                        if (teaActive) Raylib.DrawText("TEA BUFF!", 630, 20, 30, Color.Gold);
                        if (activeBugs.Count > 0) Raylib.DrawText("WARNING: BUGS DETECTED!", 350, 150, 24, Color.Red);

                        for (int i = floatingTexts.Count - 1; i >= 0; i--) {
                            var ft = floatingTexts[i]; ft.life -= dt; ft.pos.Y -= 30 * dt;
                            if (ft.life <= 0) floatingTexts.RemoveAt(i);
                            else { byte a = (byte)(255 * (ft.life / 2.0f)); Raylib.DrawText(ft.text, (int)ft.pos.X, (int)ft.pos.Y, 20, new Color(ft.color.R, ft.color.G, ft.color.B, a)); floatingTexts[i] = ft; }
                        }
                    }
                }
                Raylib.EndDrawing();
            }
            Raylib.CloseWindow();
        }
    }
}
