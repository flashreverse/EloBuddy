using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;

namespace Reverse_Wukong
{
    class Program
    {
        public static Spell.Active Q;
        public static Spell.Active W;
        public static Spell.Targeted E;
        public static Spell.Active R;
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
            if (Player.Instance.ChampionName != "MonkeyKing")
                return;

            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Targeted(SpellSlot.E, 625);
            R = new Spell.Active(SpellSlot.R);

            Menu = MainMenu.AddMenu("Reverse Wukong", "reversewukong");
            Menu.AddGroupLabel("Reverse Wukong V0.1");
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
            FarmingMenu.Add("Elasthit", new CheckBox("Use E on LastHit"));

            FarmingMenu.AddLabel("LaneClear");
            FarmingMenu.Add("QLaneClear", new CheckBox("Use Q on LaneClear"));
            FarmingMenu.Add("QlaneclearMana", new Slider("Mana % To Use Q", 30));
            FarmingMenu.Add("ELaneClear", new CheckBox("Use E on LaneClear"));
            FarmingMenu.Add("ElaneclearMana", new Slider("Mana % To Use E", 30));

            MiscMenu = Menu.AddSubMenu("Misc", "Misc");
            MiscMenu.AddGroupLabel("Misc");
            MiscMenu.AddLabel("KillSteal");
            MiscMenu.Add("Qkill", new CheckBox("Use Q KillSteal"));
            MiscMenu.Add("Ekill", new CheckBox("Use E KillSteal"));

            DrawMenu = Menu.AddSubMenu("Drawings", "Drawings");
            DrawMenu.AddGroupLabel("Drawings");
            DrawMenu.AddLabel("Drawings");
            DrawMenu.Add("drawE", new CheckBox("Draw E"));
            DrawMenu.Add("drawR", new CheckBox("Draw R"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnPreAttack += Orbwalker_OnPreAttack;

<<<<<<< HEAD
            Chat.Print("Reverse Wukong loaded :)", Color.White);

=======
            Chat.Print("Reverse Wukong loaded :)", System.Drawing.Color.White);
>>>>>>> a46fce3a24f39d59e5f86ec9ca063378082ad9a1
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
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            KillSteal();
        }
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target == null) return;
            var useQ = SkillMenu["QCombo"].Cast<CheckBox>().CurrentValue;
            var useE = SkillMenu["ECombo"].Cast<CheckBox>().CurrentValue;
            var useR = SkillMenu["RCombo"].Cast<CheckBox>().CurrentValue;

            if (target.IsValidTarget(E.Range))
            {
                if (_Player.HasBuff("MonkeyKingSpinToWin"))
                {
                    return;
                }
                if (useE && E.IsReady() && target.IsValidTarget(E.Range) && !target.IsZombie)
                {
                    E.Cast(target);
                }
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsZombie)
                {
                    Q.Cast();
                }
                if (useR && R.IsReady() && target.IsValidTarget(R.Range) && !target.IsZombie)
                {
                    R.Cast();
                }
            }
        }
        private static void KillSteal()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target == null) return;
            var useQ = MiscMenu["Qkill"].Cast<CheckBox>().CurrentValue;
            var useE = MiscMenu["Ekill"].Cast<CheckBox>().CurrentValue;

            if (Q.IsReady() && useQ && target.IsValidTarget(Q.Range) && !target.IsZombie && target.Health <= _Player.GetSpellDamage(target, SpellSlot.Q))
            {
                Q.Cast();
            }
            if (E.IsReady() && useE && target.IsValidTarget(E.Range) && !target.IsZombie && target.Health <= _Player.GetSpellDamage(target, SpellSlot.E))
            {
                E.Cast(target);
            }
        }
        private static void Harass()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target == null) return;
            var useQ = SkillMenu["QHarass"].Cast<CheckBox>().CurrentValue;
            var useW = SkillMenu["WHarass"].Cast<CheckBox>().CurrentValue;
            var useE = SkillMenu["EHarass"].Cast<CheckBox>().CurrentValue;

            if (Q.IsReady() && useQ && target.IsValidTarget(Q.Range) && !target.IsZombie)
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
            // Mudei
            if (useQ && Q.IsReady())
            {
<<<<<<< HEAD
                foreach (var minion in minions)
=======
                if (useQ && Q.IsReady() && minion.IsValidTarget(E.Range) && Player.Instance.ManaPercent > Qmana && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.Q))
>>>>>>> a46fce3a24f39d59e5f86ec9ca063378082ad9a1
                {
                    if (Player.Instance.ManaPercent > Qmana &&
                        minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        Q.Cast();
                    }
                }
            }
            // Mudei
            if (useE && E.IsReady() && minion.IsValidTarget(E.Range) && Player.Instance.ManaPercent > Emana)
                {
                    E.Cast(minion);
                }
        }
        private static void LastHit()
        {
            var useE = FarmingMenu["Elasthit"].Cast<CheckBox>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useE && E.IsReady() && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.E))
                {
                    E.Cast(minion);
                }
            }
        }
        private static void Flee()
        {
            W.Cast();
        }
        static void Orbwalker_OnPreAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                var t = target as Obj_AI_Base;
                var useQ = FarmingMenu["Qlasthit"].Cast<CheckBox>().CurrentValue;
                var Qmana = FarmingMenu["QlasthitMana"].Cast<Slider>().CurrentValue;

                if (t != null)
                {
                    if (_Player.GetSpellDamage(t, SpellSlot.Q) >= t.Health && t.IsValidTarget() && Q.IsReady() && Player.Instance.ManaPercent > Qmana)
                    {
                        Q.Cast();
                    }
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Yellow, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }
            if (DrawMenu["drawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Blue, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
            }
        }
    }
}