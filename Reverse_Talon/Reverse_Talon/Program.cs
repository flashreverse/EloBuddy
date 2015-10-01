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


namespace Reverse_Talon
{
    class Program
    {
        public static Spell.Active Q;
        public static Spell.Skillshot W;
        public static Spell.Targeted E;
        public static Spell.Active R;
        public static Menu Menu, SettingsMenu;
        public static HitChance MinimumHitChance { get; set; }

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Talon")
                return;

            Bootstrap.Init(null);

            uint level = (uint)Player.Instance.Level;
            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Skillshot(SpellSlot.W, 600, SkillShotType.Linear, 250, int.MaxValue)
            {
                AllowedCollisionCount = int.MaxValue, MinimumHitChance = HitChance.High
            };
            E = new Spell.Targeted(SpellSlot.E, 700);
            R = new Spell.Active(SpellSlot.R, 500);

            Menu = MainMenu.AddMenu("Reverse Talon", "reversetalon");
            Menu.AddGroupLabel("Reverse Talon V0.1");

            Menu.AddSeparator();

            Menu.AddLabel("Made By Reverse Flash");
            SettingsMenu = Menu.AddSubMenu("Settings", "Settings");

            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddLabel("Combo");
            SettingsMenu.Add("QCombo", new CheckBox("Use Q on Combo"));
            SettingsMenu.Add("WCombo", new CheckBox("Use W on Combo"));
            SettingsMenu.Add("ECombo", new CheckBox("Use E on Combo"));
            SettingsMenu.Add("RCombo", new CheckBox("Use R on Combo"));

            SettingsMenu.AddLabel("Harass");
            SettingsMenu.Add("QHarass", new CheckBox("Use Q on Harass"));
            SettingsMenu.Add("WHarass", new CheckBox("Use W on Harass"));
            SettingsMenu.Add("EHarass", new CheckBox("Use E on Harass"));

            SettingsMenu.AddLabel("LastHit");
            SettingsMenu.Add("Qlasthit", new CheckBox("Use Q on LastHit"));
            SettingsMenu.Add("QlasthitMana", new Slider("Mana % To Use Q", 30, 0, 100));
            SettingsMenu.Add("Wlasthit", new CheckBox("Use W on LastHit"));
            SettingsMenu.Add("WlasthitMana", new Slider("Mana % To Use W", 30, 0, 100));

            SettingsMenu.AddLabel("LaneClear");
            SettingsMenu.Add("QLaneClear", new CheckBox("Use Q on LaneClear"));
            SettingsMenu.Add("QlaneclearMana", new Slider("Mana % To Use Q", 30, 0, 100));
            SettingsMenu.Add("WLaneClear", new CheckBox("Use W on LaneClear"));
            SettingsMenu.Add("WlaneclearMana", new Slider("Mana % To Use W", 30, 0, 100));

            SettingsMenu.AddLabel("KillSteal");
            SettingsMenu.Add("EWkill", new CheckBox("Use EW KillSteal"));

            SettingsMenu.AddLabel("Draw");
            SettingsMenu.Add("drawW", new CheckBox("Draw W"));
            SettingsMenu.Add("drawE", new CheckBox("Draw E"));
            SettingsMenu.Add("drawR", new CheckBox("Draw R"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;

            Chat.Print("Reverse Talon loaded :)", System.Drawing.Color.DarkRed);
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
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            var useQ = SettingsMenu["QCombo"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["WCombo"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["ECombo"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["RCombo"].Cast<CheckBox>().CurrentValue;
            
            if (target.IsValidTarget(E.Range))
            {   
                if (useQ && Q.IsReady() && target.IsValidTarget(E.Range) && !target.IsZombie)
                {
                    Q.Cast();
                }
                if (E.IsReady() && useE && target.IsValidTarget(E.Range) && !target.IsZombie)
                {
                    E.Cast(target);
                }
                if (R.IsReady() && useR && target.IsValidTarget(E.Range) && !target.IsZombie)
                {
                    R.Cast();
                }
                if (W.IsReady() && useW && target.IsValidTarget(W.Range) && !target.IsZombie)
                {
                    W.Cast(target);
                }
                if (R.IsReady() && useR && target.IsValidTarget(E.Range) && !target.IsZombie)
                {
                    R.Cast();
                }
            }
            else if (!target.IsValidTarget(E.Range))
            {
                if (useR && R.IsReady() && target.IsValidTarget(R.Range) && !target.IsZombie)
                {
                    R.Cast();
                }
                if (Q.IsReady() && useQ && target.IsValidTarget(E.Range) && !target.IsZombie)
                {
                    Q.Cast();
                }
                if (E.IsReady() && useE && target.IsValidTarget(E.Range) && !target.IsZombie)
                {
                    E.Cast(target);
                }
                if (W.IsReady() && useW && target.IsValidTarget(W.Range) && !target.IsZombie)
                {
                    W.Cast(target);
                }
                if (R.IsReady() && useR && target.IsValidTarget(R.Range) && !target.IsZombie)
                {
                    R.Cast();
                }
            }

        }
        private static void KillSteal()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            var useEW = SettingsMenu["EWkill"].Cast<CheckBox>().CurrentValue;

            if (E.IsReady() && W.IsReady() && useEW && target.IsValidTarget(E.Range) && !target.IsZombie && target.Health <= _Player.GetSpellDamage(target, SpellSlot.W))
            {
                E.Cast(target);
                W.Cast(target);
            }
        }
        private static void Harass()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            var useQ = SettingsMenu["QHarass"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["WHarass"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["EHarass"].Cast<CheckBox>().CurrentValue;
            

            if (Q.IsReady() && useQ && target.IsValidTarget(E.Range) && !target.IsZombie)
            {
                Q.Cast();
            }
            if (E.IsReady() && useE && target.IsValidTarget(E.Range) && !target.IsZombie)
            {
                E.Cast(target);
            }
            if (W.IsReady() && useW && target.IsValidTarget(W.Range) && !target.IsZombie)
            {
                W.Cast(target);
            }
        }
        private static void LaneClear()
        {
            var useW = SettingsMenu["WLaneClear"].Cast<CheckBox>().CurrentValue;
            var Wmana = SettingsMenu["WlaneclearMana"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useW && W.IsReady() && Player.Instance.ManaPercent > Wmana && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    W.Cast(minion);
                }
            }
        }
        static void Orbwalker_OnPreAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit)
                || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)
                || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                var t = target as Obj_AI_Base;
                var useQ = SettingsMenu["Qlasthit"].Cast<CheckBox>().CurrentValue;
                var Qmana = SettingsMenu["QlasthitMana"].Cast<Slider>().CurrentValue;

                if (t != null)
                { 
                    if (_Player.GetSpellDamage(t ,SpellSlot.Q) >= t.Health && t.IsValidTarget() && Q.IsReady() && Player.Instance.ManaPercent > Qmana)
                    {
                        Q.Cast();
                    }
                }
            }
        }
        private static void LastHit()
        {
            var useW = SettingsMenu["Wlasthit"].Cast<CheckBox>().CurrentValue;
            var Wmana = SettingsMenu["WlasthitMana"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useW && E.IsReady() && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.W))
                {
                    W.Cast(minion);
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (SettingsMenu["drawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Yellow, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }
            if (SettingsMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Green, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }
            if (SettingsMenu["drawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Green, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
            }
        }
    }
}