#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.XInput;
using Color = System.Drawing.Color;

#endregion

using EloBuddy; 
using LeagueSharp.Common; 
 namespace ControlSharp
{
    internal class Program
    {
        public static int[] ControllerArray = { 0, 1, 2, 3, 4 };
        public static Menu Menu;
        public static Orbwalking.Orbwalker OrbWalker;
        public static Orbwalking.OrbwalkingMode CurrentMode = Orbwalking.OrbwalkingMode.None;
        public static GamepadState Controller;
        public static Render.Circle CurrentPosition;
        public static Render.Text Text;
        public static float MaxD = 0;
        public static uint LastKey;
        public static int MenuCount;

        public static Dictionary<Orbwalking.OrbwalkingMode, string> KeyDictionary =
            new Dictionary<Orbwalking.OrbwalkingMode, string>
            {
                { Orbwalking.OrbwalkingMode.Combo, "Orbwalk" },
                { Orbwalking.OrbwalkingMode.Mixed, "Farm" },
                { Orbwalking.OrbwalkingMode.LaneClear, "LaneClear" },
                { Orbwalking.OrbwalkingMode.LastHit, "LastHit" }
            };

        public static void Main()
        {
            Game_OnGameLoad(new EventArgs());
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            foreach (var c in
                ControllerArray.Select(controlId => new Controller((UserIndex) controlId)).Where(c => c.IsConnected))
            {
                Controller = new GamepadState(c.UserIndex);
            }

            if (Controller == null || !Controller.Connected)
            {
                Chat.Print("No controller detected!");
                return;
            }

            Menu = new Menu("ControllerTest", "ControllerTest", true);

            Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            OrbWalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalker"));

            Menu.AddItem(new MenuItem("Draw", "Draw Circle").SetValue(true));
            Menu.AddToMainMenu();

            if (Menu.Item("Draw").GetValue<bool>())
            {
                CurrentPosition = new Render.Circle(ObjectManager.Player.Position, 100, Color.Red, 2);
                CurrentPosition.Add();
                Text = new Render.Text(new Vector2(50, 50), "MODE: " + CurrentMode, 30, new ColorBGRA(255, 0, 0, 255))
                {
                    OutLined = true
                };
                Text.Add();
            }

            Chat.Print(
                "<b><font color =\"#FFFFFF\">ControlSharp by </font><font color=\"#5C00A3\">Trees</font><font color =\"#FFFFFF\"> loaded!</font></b>");

            Menu.Item("Draw").ValueChanged += OnValueChanged;
            Game.OnUpdate += Game_OnGameUpdate;
        }

        private static void OnValueChanged(object sender, OnValueChangeEventArgs onValueChangeEventArgs)
        {
            if (onValueChangeEventArgs.GetNewValue<bool>())
            {
                CurrentPosition = new Render.Circle(ObjectManager.Player.Position, 100, Color.Red, 2);
                CurrentPosition.Add();
                Text = new Render.Text(new Vector2(50, 50), "MODE: " + CurrentMode, 30, new ColorBGRA(255, 0, 0, 255))
                {
                    OutLined = true
                };
                Text.Add();
            }
            else
            {
                CurrentPosition.Remove();
                Text.Remove();
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var wp = ObjectManager.Player.GetWaypoints();

            //in case you manually click to move
            if (wp.Count > 0 && ObjectManager.Player.Distance(wp[wp.Count - 1]) > 540)
            {
                SetOrbwalkingMode(Orbwalking.OrbwalkingMode.None);
                return;
            }

            if (Controller == null || !Controller.Connected)
            {
                Chat.Print("Controller disconnected!");
                Game.OnUpdate -= Game_OnGameUpdate;
                return;
            }

            Controller.Update();
            UpdateStates();

            var p = ObjectManager.Player.ServerPosition.To2D() + (Controller.LeftStick.Position / 75);
            var pos = new Vector3(p.X, p.Y, ObjectManager.Player.Position.Z);

            if (ObjectManager.Player.Distance(pos) < 100)
            {
                return;
            }

            CurrentPosition.Position = pos;
            SetOrbwalkingPosition(pos);
        }

        private static void UpdateStates()
        {
            //Push any button to cancel mode
            if (Controller.LeftShoulder || Controller.RightShoulder || Controller.Back || Controller.Start ||
                Controller.RightStick.Clicked)
            {
                SetOrbwalkingMode(Orbwalking.OrbwalkingMode.None);
                return;
            }

            if (Controller.DPad.IsAnyPressed() || Controller.IsABXYPressed()) // Change mode command
            {
                if (Controller.DPad.Up || Controller.X)
                {
                    SetOrbwalkingMode(Orbwalking.OrbwalkingMode.Combo);
                }
                else if (Controller.DPad.Left || Controller.A)
                {
                    SetOrbwalkingMode(Orbwalking.OrbwalkingMode.LaneClear);
                }
                else if (Controller.DPad.Right || Controller.Y)
                {
                    SetOrbwalkingMode(Orbwalking.OrbwalkingMode.Mixed);
                }
                else if (Controller.DPad.Down || Controller.B)
                {
                    SetOrbwalkingMode(Orbwalking.OrbwalkingMode.LastHit);
                }
            }

            var s1 = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Summoner1);
            var s2 = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Summoner2);

            if (Controller.LeftTrigger > 0 && s1.State == SpellState.Ready)
            {
                SummonerCastLogic(s1);
                return;
            }

            if (Controller.RightTrigger > 0 && s2.State == SpellState.Ready)
            {
                SummonerCastLogic(s2);
            }
        }

        private static void SummonerCastLogic(SpellDataInst spell)
        {
            switch (spell.Name.ToLower().Replace("summoner", ""))
            {
                case "barrier":
                    ObjectManager.Player.Spellbook.CastSpell(spell.Slot);
                    break;
                case "boost":
                    ObjectManager.Player.Spellbook.CastSpell(spell.Slot);
                    break;
                case "dot":
                    foreach (
                        var enemy in ObjectManager.Get<AIHeroClient>().Where(h => h.IsValidTarget(550) && h.Health < 600)
                        )
                    {
                        ObjectManager.Player.Spellbook.CastSpell(spell.Slot, enemy);
                        break;
                    }
                    break;
                case "flash": //LOL
                    Controller.Update();
                    var pos = ObjectManager.Player.ServerPosition.To2D() + (Controller.LeftStick.Position / 75);
                    pos.Extend(ObjectManager.Player.ServerPosition.To2D(), 550);
                    ObjectManager.Player.Spellbook.CastSpell(spell.Slot, pos.To3D());
                    break;
                case "haste":
                    ObjectManager.Player.Spellbook.CastSpell(spell.Slot);
                    break;
                case "heal":
                    ObjectManager.Player.Spellbook.CastSpell(spell.Slot);
                    break;
                case "mana":
                    ObjectManager.Player.Spellbook.CastSpell(spell.Slot);
                    break;
                case "revive":
                    ObjectManager.Player.Spellbook.CastSpell(spell.Slot);
                    break;
            }
        }

        private static void SetOrbwalkingPosition(Vector3 position)
        {
            foreach (var orbwalker in Orbwalking.Orbwalker.Instances)
            {
                orbwalker.SetOrbwalkingPoint(position);
            }
        }

        private static void SetOrbwalkingMode(Orbwalking.OrbwalkingMode mode)
        {
            Console.WriteLine("SET");

            CurrentMode = mode;
            Text.text = "MODE: " + CurrentMode;

            foreach (var orbwalkMode in KeyDictionary.Keys)
            {
                var value = KeyDictionary[orbwalkMode];
                var key = Menu.Item(value).GetValue<KeyBind>().Key;
                var currentMode = orbwalkMode == mode;
                Menu.SendMessage(key, currentMode ? WindowsMessages.WM_KEYDOWN : WindowsMessages.WM_KEYUP, new WndEventComposition());
            }
        }
    }
}