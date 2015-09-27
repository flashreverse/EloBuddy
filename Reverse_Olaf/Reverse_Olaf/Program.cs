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


namespace Reverse_Olaf
{
    class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Active W;
        public static Spell.Targeted E;
        public static Spell.Active R;
        public static Menu Menu, SettingsMenu;

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
            if (Player.Instance.ChampionName != "Olaf")
                return;

            Bootstrap.Init(null);

            uint level = (uint)Player.Instance.Level;
            Q = new Spell.Skillshot(SpellSlot.Q, 1000, SkillShotType.Linear, 250, 1600, 100);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Targeted(SpellSlot.E, 320);
            R = new Spell.Active(SpellSlot.R);

            Menu = MainMenu.AddMenu("Reverse Olaf", "Reverse Olaf");
            Menu.AddGroupLabel("Reverse Olaf 0.1");

            Menu.AddSeparator();

            Menu.AddLabel("Made By MarioGK");
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
            SettingsMenu.Add("Elasthit", new CheckBox("Use E on LastHit"));
            SettingsMenu.Add("QlasthitMana", new Slider("Mana % To Use Q", 30, 0, 100));

            SettingsMenu.AddLabel("LaneClear");
            SettingsMenu.Add("QLaneClear", new CheckBox("Use Q on LaneClear"));
            SettingsMenu.Add("WLaneClear", new CheckBox("Use W on LaneClear"));
            SettingsMenu.Add("ELaneClear", new CheckBox("Use E on LaneClear"));
            SettingsMenu.Add("QlaneclearMana", new Slider("Mana % To Use Q", 30, 0, 100));
            SettingsMenu.Add("WlaneclearMana", new Slider("Mana % To Use Q", 30, 0, 100));

            SettingsMenu.AddLabel("KillSteal");
            SettingsMenu.Add("Qkill", new CheckBox("Use Q KillSteal"));
            SettingsMenu.Add("Ekill", new CheckBox("Use E KillSteal"));

            SettingsMenu.AddLabel("Draw");
            SettingsMenu.Add("drawQ", new CheckBox("Draw Q"));
            SettingsMenu.Add("drawW", new CheckBox("Draw W"));
            SettingsMenu.Add("drawE", new CheckBox("Draw E"));
            SettingsMenu.Add("drawR", new CheckBox("Draw R"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
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
        }

        //Get Damages
        public static float GetDamage(SpellSlot spell, Obj_AI_Base target)
        {
            float ap = _Player.FlatMagicDamageMod + _Player.BaseAbilityDamage;
            float ad = _Player.FlatMagicDamageMod + _Player.BaseAttackDamage;
            if (spell == SpellSlot.Q)
            {
                if (!Q.IsReady())
                    return 0;
                return _Player.CalculateDamageOnUnit(target, DamageType.Physical, 70f + 45f * (Q.Level - 1) + 100 / 100 * ad);
            }
            else if (spell == SpellSlot.W)
            {
                if (!W.IsReady())
                    return 0;
                return _Player.CalculateDamageOnUnit(target, DamageType.Magical, 0f + 0f * (W.Level - 1) + 0 / 100 * ap + 0 / 100 * ad);
            }
            else if (spell == SpellSlot.E)
            {
                if (!E.IsReady())
                    return 0;
                return _Player.CalculateDamageOnUnit(target, DamageType.True, 60f + 45f * (E.Level - 1) + 40 / 100 * ad);
            }
            else if (spell == SpellSlot.R)
            {
                if (!R.IsReady())
                    return 0;
                return _Player.CalculateDamageOnUnit(target, DamageType.Magical, 0f + 0f * (R.Level - 1) + 0 / 100 * ap);
            }

            return 0;
        }
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var useQ = SettingsMenu["QCombo"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["WCombo"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["ECombo"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["RCombo"].Cast<CheckBox>().CurrentValue;

            Q.AllowedCollisionCount = int.MaxValue;

            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range -20) && !target.IsDead && !target.IsZombie && Q.GetPrediction(target).HitChance >= HitChance.High)
            {
                Q.Cast(target);
            }
            if (E.IsReady() && useE && target.IsValidTarget(E.Range) && !target.IsDead && !target.IsZombie)
            {
                E.Cast(target);
            }
            if (W.IsReady() && useW && target.IsValidTarget(150) && !target.IsDead && !target.IsZombie)
            {
                W.Cast();
            }
            if (R.IsReady() && useR && target.IsValidTarget(E.Range) && !target.IsDead && !target.IsZombie)
            {
                R.Cast();
            }
        }
        private static void KillSteal()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var useQ = SettingsMenu["Qkill"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Ekill"].Cast<CheckBox>().CurrentValue;

            if (Q.IsReady() && useQ && target.IsValidTarget(Q.Range -20) && !target.IsDead && !target.IsZombie && target.Health <= GetDamage(SpellSlot.Q, target))
            {
                Q.Cast(target);
            }
            if (E.IsReady() && useE && target.IsValidTarget(E.Range) && !target.IsDead && !target.IsZombie && target.Health <= GetDamage(SpellSlot.E, target))
            {
                E.Cast(target);
            }
        }
        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var useQ = SettingsMenu["QHarass"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["WHarass"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["EHarass"].Cast<CheckBox>().CurrentValue;

            if (Q.IsReady() && useQ && target.IsValidTarget(Q.Range) && !target.IsDead && !target.IsZombie)
            {
                Q.Cast(target);
            }
            if (W.IsReady() && useW && target.IsValidTarget(_Player.AttackRange) && !target.IsDead && !target.IsZombie)
            {
                W.Cast();
            }
            if (E.IsReady() && useE && target.IsValidTarget(E.Range) && !target.IsDead && !target.IsZombie)
            {
                E.Cast(target);
            }

        }
        private static void LaneClear()
        {
            var useQ = SettingsMenu["QLaneClear"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["WLaneClear"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["ELaneClear"].Cast<CheckBox>().CurrentValue;
            var Qmana = SettingsMenu["QlaneclearMana"].Cast<Slider>().CurrentValue;
            var Wmana = SettingsMenu["WlaneclearMana"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && Player.Instance.ManaPercent > Qmana && minion.Health <= GetDamage(SpellSlot.Q, minion))
                {
                    Q.Cast(minion);
                }
                if (useW && W.IsReady() && Player.Instance.ManaPercent > Wmana && minion.Health <= GetDamage(SpellSlot.W, minion))
                {
                    W.Cast();
                }
                if (useE && Q.IsReady() && minion.Health <= GetDamage(SpellSlot.E, minion))
                {
                    E.Cast(minion);
                }
            }
        }
        private static void LastHit()
        {
            var useQ = SettingsMenu["Qlasthit"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Elasthit"].Cast<CheckBox>().CurrentValue;
            var mana = SettingsMenu["QlasthitMana"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && !minion.IsValidTarget(E.Range) && minion.IsValidTarget(Q.Range) && Player.Instance.ManaPercent > mana && minion.Health <= GetDamage(SpellSlot.Q, minion))
                {
                    Q.Cast(minion);
                }
                if (useE && E.IsReady() && minion.Health <= GetDamage(SpellSlot.E, minion))
                {
                    E.Cast(minion);
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (SettingsMenu["drawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Yellow, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (SettingsMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Red, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }
            if (SettingsMenu["drawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Blue, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }
            if (SettingsMenu["drawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Purple, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
            }
        }
    }
}