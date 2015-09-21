using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using Color = System.Drawing.Color;
using SharpDX;

namespace Reverse_Katarina
{
    class Program
    {
        public static Spell.Targeted Q;
        public static Spell.Active W;
        public static Spell.Targeted E;
        public static Spell.Active R;
        public static Boolean ult = false;
        public static Menu KatarinaMenu, SettingsMenu, MiscMenu, Skin;
        public static float IsInRange()
        {
            if (Q.IsReady())
            {
                return Q.Range;
            }
            return _Player.GetAutoAttackRange();
        }

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static Item HP;
        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Katarina")
            return;

            TargetSelector.Init();
            Bootstrap.Init(null);

            Q = new Spell.Targeted(SpellSlot.Q, 675);
            W = new Spell.Active(SpellSlot.W, 375);
            E = new Spell.Targeted(SpellSlot.E, 700);
            R = new Spell.Active(SpellSlot.R, 550);

            KatarinaMenu = MainMenu.AddMenu("Reverse Katarina", "reversekatarina");

            KatarinaMenu.AddGroupLabel("Reverse Katarina 1.0");
            KatarinaMenu.AddSeparator();

            KatarinaMenu.AddLabel("Made By MarioGK");
            SettingsMenu = KatarinaMenu.AddSubMenu("Settings", "Settings");

            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddLabel("Combo");
            SettingsMenu.Add("comboR", new CheckBox("Combo with R"));

            SettingsMenu.AddLabel("Harass");
            SettingsMenu.Add("harassQ", new CheckBox("Use Q on Harass"));
            SettingsMenu.Add("harassW", new CheckBox("Use W on Harass"));

            SettingsMenu.AddLabel("LaneClear");
            SettingsMenu.Add("laneClearQ", new CheckBox("Use Q on LaneClear"));
            SettingsMenu.Add("laneClearW", new CheckBox("Use W on LaneClear"));

            SettingsMenu.AddLabel("LastHit");
            SettingsMenu.Add("lastHitQ", new CheckBox("Use Q on LastHit"));
            SettingsMenu.Add("lastHitW", new CheckBox("Use W on LastHit"));

            SettingsMenu.AddLabel("KillSteal");
            SettingsMenu.Add("killsteal", new CheckBox("KillSteal"));
            SettingsMenu.Add("ksQ", new CheckBox("Use Q on KS"));
            SettingsMenu.Add("ksWE", new CheckBox("Use WE on KS"));

            SettingsMenu.AddLabel("Drawing");
            SettingsMenu.Add("drawQ", new CheckBox ("Draw Q"));
            SettingsMenu.Add("drawW", new CheckBox("Draw W"));
            SettingsMenu.Add("drawE", new CheckBox("Draw E"));
            SettingsMenu.Add("drawR", new CheckBox("Draw R"));

            MiscMenu = KatarinaMenu.AddSubMenu("Misc", "Misc");
            MiscMenu.AddGroupLabel("Misc Settings");
            MiscMenu.AddSeparator();
            MiscMenu.AddGroupLabel("Items Settings");
            MiscMenu.AddSeparator();
            MiscMenu.Add("useHP", new Slider("Use Health Potion"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;

            //SkinChanger
            Skin = KatarinaMenu.AddSubMenu("Skin Manager", "Skin Manager");
            Skin.AddGroupLabel("Skin Changer");

            var skin = Skin.Add("SkinID", new Slider("Skin", 0, 0, 8));
            var SkinID = new[] { "Classic", "Mercenary", "Red Card", "Bilgewater", "Kitty Cat", "High Command", "Darude Sandstorm", "Slay Belle", "Warring Kingdoms" };
            skin.DisplayName = SkinID[skin.CurrentValue];
            skin.OnValueChange +=
            delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
            {
                sender.DisplayName = SkinID[changeArgs.NewValue];
            };
            
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo)
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Harass)
            {
                Harass();
            }
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LaneClear)
            {
                LaneClear();
            }
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LastHit)
            {
                LastHit();
            }
            if (SettingsMenu["killsteal"].Cast<CheckBox>().CurrentValue)
            {
                KillSteal();
            }

            HP = new Item((int)ItemId.Health_Potion);
            if (_Player.HealthPercent <= KatarinaMenu["useHP"].Cast<Slider>().CurrentValue && HP.IsOwned())
            {
                HP.Cast();
            }

            SkinChanger();

            if (!Player.HasBuff("katarinarsound"))
            {
                Orbwalker.DisableAttacking = false;
                Orbwalker.DisableMovement = false;
            }
            else
            {
                Orbwalker.DisableAttacking = true;
                Orbwalker.DisableMovement = true;
            }
        }

                //Get Damages

                public static float QDamage(Obj_AI_Base target)
                {
                    return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                        (float)(new[] { 60, 85, 110, 135, 160 }[Program.Q.Level] + 0.45 * _Player.FlatMagicDamageMod));
                }
                public static float WDamage(Obj_AI_Base target)
                {
                    return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                        (float)(new[] { 40, 75, 110, 145, 180 }[Program.W.Level] + 0.25 * _Player.FlatMagicDamageMod + 0.60 * _Player.FlatPhysicalDamageMod));
                }
                public static float EDamage(Obj_AI_Base target)
                {
                    return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                        (float)(new[] { 60, 85, 110, 135, 160 }[Program.E.Level] + 0.40 * _Player.FlatMagicDamageMod));
                }
                
                private static void Combo()
                {
                    var useR = SettingsMenu["comboR"].Cast<CheckBox>().CurrentValue;

                    foreach(var target in HeroManager.Enemies.Where(oi => oi.IsValidTarget(E.Range) && !oi.IsDead))
                    {
                        if (Q.IsReady() && target.IsValidTarget(Q.Range) /*&& ult == false*/)
                        {
                            Q.Cast(target);
                        }
                        if (E.IsReady() && target.IsValidTarget(E.Range) /*&& ult == false*/)
                        {
                            E.Cast(target);
                        }
                        if (W.IsReady() && target.IsValidTarget(W.Range) /*&& ult == false*/)
                        {
                            W.Cast();
                        }
                        if (R.IsReady() && target.IsValidTarget(R.Range) /*&& ult == false*/)
                        {
                            Orbwalker.DisableMovement = true;
                            Orbwalker.DisableAttacking = true;

                            R.Cast();
                            /*ult = true;*/
                        }
                    }

                    SkinChanger();
                }
                /*private static void UltOf()
                {
                    foreach(var target in HeroManager.Enemies.Where(oi => oi.IsValidTarget(R.Range) && !oi.IsDead))
                    {
                        if (R.IsOnCooldown && !target.IsValidTarget(R.Range) && ult == true)
                        {
                            ult = false;
                        }
                    }
                }*/
                private static void Harass()
                {
                    var useW = SettingsMenu["harassQ"].Cast<CheckBox>().CurrentValue;
                    var useE = SettingsMenu["harassW"].Cast<CheckBox>().CurrentValue;

                    foreach(var target in HeroManager.Enemies.Where(oi => oi.IsValidTarget(Q.Range) && !oi.IsDead))
                    {
                        if (Q.IsReady() && target.IsValidTarget(Q.Range))
                        {
                            Q.Cast(target);
                        }
                        if (W.IsReady() && target.IsValidTarget(W.Range))
                        {
                            W.Cast();
                        }
                    }
                }
                private static void LaneClear()
                {
                    var useQ = SettingsMenu["laneClearQ"].Cast<CheckBox>().CurrentValue;
                    var useW = SettingsMenu["laneClearW"].Cast<CheckBox>().CurrentValue;

                    foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(a => a.IsEnemy && !a.IsValidTarget(125)))
                    {
                        if (useQ && Q.IsReady())
                        {
                            if (minion == null) return;
                            W.Cast(minion);
                        }
                        if (useW && W.IsReady())
                        {
                            if (minion == null) return;
                            W.Cast();
                        }
                    }                   
                }
                private static void LastHit()
                {
                    var minion = ObjectManager.Get<Obj_AI_Minion>().Where(a => a.IsEnemy && a.Distance(_Player) < IsInRange()).OrderBy(a => a.Health).FirstOrDefault();

                    if (SettingsMenu["lastHitQ"].Cast<CheckBox>().CurrentValue && QDamage(minion) > minion.Health && !minion.IsDead && minion.Distance(_Player) < Q.Range)
                    {
                        Q.Cast(minion);
                    }                   
                }
                private static void KillSteal()
                {

                }

                private static void SkinChanger()
                {
                    var mode = Skin["SkinID"].DisplayName;
                    switch (mode)
                    {
                        case "Classic":
                            Player.SetSkinId(0);
                            break;
                        case "Mercenary":
                            Player.SetSkinId(1);
                            break;
                        case "Red Card":
                            Player.SetSkinId(2);
                            break;
                        case "Bilgewater":
                            Player.SetSkinId(3);
                            break;
                        case "Kitty Cat":
                            Player.SetSkinId(4);
                            break;
                        case "High Command":
                            Player.SetSkinId(5);
                            break;
                        case "Darude Sandstorm":
                            Player.SetSkinId(6);
                            break;
                        case "Slay Belle":
                            Player.SetSkinId(7);
                            break;
                        case "Warring Kingdoms":
                            Player.SetSkinId(8);
                            break;
                    }
                 }
                private static void Drawing_OnDraw(EventArgs args)
                {
                    if (SettingsMenu["drawQ"].Cast<CheckBox>().CurrentValue)
                    {
                        new Circle() { Color = Color.Red, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
                    }
                    if (SettingsMenu["drawW"].Cast<CheckBox>().CurrentValue)
                    {
                        new Circle() { Color = Color.Blue, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
                    }
                    if (SettingsMenu["drawE"].Cast<CheckBox>().CurrentValue)
                    {
                        new Circle() { Color = Color.Purple, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
                    }
                    if (SettingsMenu["drawR"].Cast<CheckBox>().CurrentValue)
                    {
                        new Circle() { Color = Color.White, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
                    }
                }
            }
}