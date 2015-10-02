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


namespace Reverse_Fiora
{
    class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Active E;
        public static Spell.Targeted R;
        public static Menu Menu, SkillMenu, FarmingMenu, MiscMenu, DrawMenu;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
            Bootstrap.Init(null);
        }

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Fiora")
                return;

            Q = new Spell.Skillshot(SpellSlot.Q, 400, SkillShotType.Circular, 250, int.MaxValue);
            W = new Spell.Skillshot(SpellSlot.W, 750, SkillShotType.Linear, 250, int.MaxValue)
            {
                MinimumHitChance = HitChance.High
            };
            E = new Spell.Active(SpellSlot.E, 200);
            R = new Spell.Targeted(SpellSlot.R, 550);

            Menu = MainMenu.AddMenu("Reverse Fiora", "reversefiora");
            Menu.AddGroupLabel("Reverse Fiora V0.2");
            Menu.AddSeparator();
            Menu.AddLabel("Made By Reverse Flash");

            SkillMenu = Menu.AddSubMenu("Skills", "Skills");
            SkillMenu.AddGroupLabel("Skills");
            SkillMenu.AddLabel("Combo");
            SkillMenu.Add("QCombo", new CheckBox("Use Q on Combo"));
            SkillMenu.Add("WCombo", new CheckBox("Use W on Combo"));
            SkillMenu.Add("ECombo", new CheckBox("Use E on Combo"));
            SkillMenu.Add("RCombo", new CheckBox("Use R on Combo"));

            SkillMenu.AddLabel("Harass");
            SkillMenu.Add("QHarass", new CheckBox("Use Q on Harass"));
            SkillMenu.Add("WHarass", new CheckBox("Use W on Harass"));
            SkillMenu.Add("EHarass", new CheckBox("Use E on Harass"));

            FarmingMenu = Menu.AddSubMenu("Farming", "Farming");
            FarmingMenu.AddGroupLabel("Farming");
            FarmingMenu.AddLabel("LastHit");
            FarmingMenu.Add("Qlasthit", new CheckBox("Use Q on LastHit"));
            FarmingMenu.Add("QlasthitMana", new Slider("Mana % To Use Q", 30));

            FarmingMenu.AddLabel("LaneClear");
            FarmingMenu.Add("QLaneClear", new CheckBox("Use Q on LaneClear"));
            FarmingMenu.Add("QlaneclearMana", new Slider("Mana % To Use Q", 30));
            FarmingMenu.Add("ELaneClear", new CheckBox("Use E on LaneClear"));
            FarmingMenu.Add("ElaneclearMana", new Slider("Mana % To Use E", 30));

            MiscMenu = Menu.AddSubMenu("Misc", "Misc");
            MiscMenu.AddGroupLabel("Misc");
            MiscMenu.AddLabel("KillSteal");
            MiscMenu.Add("Qkill", new CheckBox("Use Q KillSteal"));

            DrawMenu = Menu.AddSubMenu("Drawings", "Drawings");
            DrawMenu.AddGroupLabel("Drawings");
            DrawMenu.AddLabel("Drawings");
            DrawMenu.Add("drawQ", new CheckBox("Draw Q"));
            DrawMenu.Add("drawW", new CheckBox("Draw W"));
            DrawMenu.Add("drawR", new CheckBox("Draw R"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;

            Chat.Print("Reverse Fiora loaded :)", System.Drawing.Color.White);            

        }
        private static void Game_OnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHit();
            }
            KillSteal();
        }
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (target == null) return;
            var useQ = SkillMenu["QCombo"].Cast<CheckBox>().CurrentValue;
            var useW = SkillMenu["WCombo"].Cast<CheckBox>().CurrentValue;
            var useE = SkillMenu["ECombo"].Cast<CheckBox>().CurrentValue;
            var useR = SkillMenu["RCombo"].Cast<CheckBox>().CurrentValue;

            if (target.IsValidTarget(R.Range))
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsZombie)
                {
                    Q.Cast(target);
                }
                if (useE && E.IsReady() && target.IsValidTarget(E.Range) && !target.IsZombie)
                {
                    E.Cast();
                }
                if (useW && W.IsReady() && target.IsValidTarget(W.Range) && !target.IsZombie)
                {
                    W.Cast(target);
                }
                if (useR && R.IsReady() && target.IsValidTarget(R.Range) && !target.IsZombie)
                {
                    R.Cast(target);
                }
            }
        }
        private static void KillSteal()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target == null) return;
            var useQ = MiscMenu["Qkill"].Cast<CheckBox>().CurrentValue;

            if (Q.IsReady() && useQ && target.IsValidTarget(Q.Range) && !target.IsZombie && target.Health <= _Player.GetSpellDamage(target, SpellSlot.Q))
            {
                Q.Cast(target);
            }
        }
        private static void Harass()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target == null) return;
            var useQ = SkillMenu["QHarass"].Cast<CheckBox>().CurrentValue;
            var useW = SkillMenu["WHarass"].Cast<CheckBox>().CurrentValue;
            var useE = SkillMenu["EHarass"].Cast<CheckBox>().CurrentValue;


            if (Q.IsReady() && useQ && target.IsValidTarget(E.Range) && !target.IsZombie)
            {
                Q.Cast(target);
            }
            if (E.IsReady() && useE && target.IsValidTarget(E.Range) && !target.IsZombie)
            {
                E.Cast();
            }
            if (W.IsReady() && useW && target.IsValidTarget(W.Range) && !target.IsZombie)
            {
                W.Cast(target);
            }
        }
        private static void LaneClear()
        {
            var useQ = FarmingMenu["QLaneClear"].Cast<CheckBox>().CurrentValue;
            var useE = FarmingMenu["ELaneClear"].Cast<CheckBox>().CurrentValue;
            var Qmana = FarmingMenu["QlaneclearMana"].Cast<Slider>().CurrentValue;
            var Emana = FarmingMenu["ElaneclearMana"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && Player.Instance.ManaPercent > Qmana && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);
                }
                if (useE && E.IsReady() && minion.IsValidTarget(180) && Player.Instance.ManaPercent > Emana)
                {
                    E.Cast();
                }
            }
        }
        private static void LastHit()
        {
            var useQ = FarmingMenu["Qlasthit"].Cast<CheckBox>().CurrentValue;
            var Qmana = FarmingMenu["QlasthitMana"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawMenu["drawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Red, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (DrawMenu["drawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Green, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }
            if (DrawMenu["drawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Blue, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
            }
        }
    }
}