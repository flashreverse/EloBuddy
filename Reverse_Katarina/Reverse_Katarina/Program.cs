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
            if (Player.Instance.ChampionName != "Katarina")
                return;

            Bootstrap.Init(null);

            uint level = (uint)Player.Instance.Level;
            Q = new Spell.Targeted(SpellSlot.Q, 675);
            W = new Spell.Active(SpellSlot.W, 375);
            E = new Spell.Targeted(SpellSlot.E, 700);
            R = new Spell.Active(SpellSlot.R, 550);


            Menu = MainMenu.AddMenu("Reverse Katarina", "Reverse Katarina");
            Menu.AddGroupLabel("Reverse Katarina 0.1");

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

            SettingsMenu.AddLabel("LaneClear");
            SettingsMenu.Add("QLaneClear", new CheckBox("Use Q on LaneClear"));
            SettingsMenu.Add("WLaneClear", new CheckBox("Use W on LaneClear"));

            SettingsMenu.AddLabel("KillSteal");
            SettingsMenu.Add("Wkill", new CheckBox("Use W KillSteal"));
            SettingsMenu.Add("Rkill", new CheckBox("Use R KillSteal"));

            SettingsMenu.AddLabel("Draw");
            SettingsMenu.Add("drawQ", new CheckBox("Draw Q"));
            SettingsMenu.Add("drawE", new CheckBox("Draw E"));
            SettingsMenu.Add("drawW", new CheckBox("Draw W"));
            SettingsMenu.Add("drawR", new CheckBox("Draw R"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
        }
        private static void Game_OnTick(EventArgs args)
        {
            checkUlt();

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
                return _Player.CalculateDamageOnUnit(target, DamageType.Magical, 60f + 25f * (Q.Level - 1) + 45 / 100 * ap);
            }
            else if (spell == SpellSlot.W)
            {
                if (!W.IsReady())
                    return 0;
                return _Player.CalculateDamageOnUnit(target, DamageType.Magical, 40f + 35f * (W.Level - 1) + 25 / 100 * ap + 60 / 100 * ad);
            }
            else if (spell == SpellSlot.E)
            {
                if (!E.IsReady())
                    return 0;
                return _Player.CalculateDamageOnUnit(target, DamageType.Magical, 60f + 25f * (E.Level - 1) + 40 / 100 * ap);
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
            var target = TargetSelector.GetTarget(1000, DamageType.Magical);
            var useQ = SettingsMenu["QCombo"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["WCombo"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["ECombo"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["RCombo"].Cast<CheckBox>().CurrentValue;
           
            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsDead && !target.IsZombie)
            {
                Q.Cast(target);
            }
            if (E.IsReady() && useE && target.IsValidTarget(E.Range) && !target.IsDead && !target.IsZombie)
            {
                E.Cast(target);
                Orbwalker.DisableMovement = true;
                Orbwalker.DisableAttacking = true;
            }
            if (W.IsReady() && useW && target.IsValidTarget(W.Range) && !target.IsDead && !target.IsZombie)
            {
                W.Cast();
            }
            if (R.IsReady() && useR && target.IsValidTarget(R.Range) && !target.IsDead && !target.IsZombie)
            {
                Orbwalker.DisableMovement = true;
                Orbwalker.DisableAttacking = true;
                R.Cast();
            }
        }
        private static void KillSteal()
        {
            var target = TargetSelector.GetTarget(_Player.AttackRange, DamageType.Magical);
            var useQ = SettingsMenu["Qkill"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["Wkill"].Cast<CheckBox>().CurrentValue;
            var useEW = SettingsMenu["EWkill"].Cast<CheckBox>().CurrentValue;

            if (Q.IsReady() && useQ && target.IsValidTarget(E.Range) && !target.IsDead && !target.IsZombie && target.Health <= GetDamage(SpellSlot.Q, target))
            {
               Q.Cast(target);
            }
            if (W.IsReady() && useW && target.IsValidTarget(W.Range) && !target.IsDead && !target.IsZombie && target.Health <= GetDamage(SpellSlot.W, target))
            {
                W.Cast();
            }
            var EWdamage = (GetDamage(SpellSlot.E, target) + (GetDamage(SpellSlot.W, target)));
            if (useEW && W.IsReady() && target.IsValidTarget(R.Range) && !target.IsDead && !target.IsZombie && target.Health <= EWdamage)
            {
                E.Cast(target);
                W.Cast();
            }
        }
        private static void Harass()
        {
            var target = TargetSelector.GetTarget(_Player.AttackRange, DamageType.Magical);
            var useQ = SettingsMenu["Qh"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Eh"].Cast<CheckBox>().CurrentValue;

            if (E.IsReady() && useE && target.IsValidTarget(E.Range) && !target.IsDead && !target.IsZombie)
            {
                E.Cast(target);
            }
            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && target.HasBuff("tristanaecharge") && !target.IsDead && !target.IsZombie)
            {
                Q.Cast();
            }

        }
        private static void LaneClear()
        {
            var minion = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(a => a.IsEnemy && !a.IsDead && a.Distance(_Player) < _Player.AttackRange);
            var tower = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(a => a.IsEnemy && !a.IsDead && a.Distance(_Player) < _Player.AttackRange);
            var useQ = SettingsMenu["Qlc"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["Elc"].Cast<CheckBox>().CurrentValue;
            var useETower = SettingsMenu["Etower"].Cast<CheckBox>().CurrentValue;
            if (useE && E.IsReady())
            {
                E.Cast(minion);
            }
            if (useQ && Q.IsReady())
            {
                Q.Cast();
            }
            if (useETower && E.IsReady() && minion == null)
            {
                E.Cast(tower);
                Q.Cast();
            }

        }

        private static void checkUlt()
        {
            if(!Player.HasBuff("katarinarsound"))
            {
                Orbwalker.DisableMovement = false;
                Orbwalker.DisableAttacking = false;
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