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
        public static Menu Menu, SkillMenu, FarmingMenu, MiscMenu, DrawMenu;
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
            Menu.AddGroupLabel("Reverse Talon V1.1");

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
            FarmingMenu.Add("QlasthitMana", new Slider("Mana % To Use Q", 30, 0, 100));
            FarmingMenu.Add("Wlasthit", new CheckBox("Use W on LastHit"));
            FarmingMenu.Add("WlasthitMana", new Slider("Mana % To Use W", 30, 0, 100));

            FarmingMenu.AddLabel("LaneClear");
            FarmingMenu.Add("QLaneClear", new CheckBox("Use Q on LaneClear"));
            FarmingMenu.Add("QlaneclearMana", new Slider("Mana % To Use Q", 30, 0, 100));
            FarmingMenu.Add("WLaneClear", new CheckBox("Use W on LaneClear"));
            FarmingMenu.Add("WlaneclearMana", new Slider("Mana % To Use W", 30, 0, 100));

            MiscMenu = Menu.AddSubMenu("Misc", "Misc");
            MiscMenu.AddGroupLabel("Misc");
            MiscMenu.AddLabel("KillSteal");
            MiscMenu.Add("Wkill", new CheckBox("Use W KillSteal"));

            DrawMenu = Menu.AddSubMenu("Drawings", "Drawings");
            DrawMenu.AddGroupLabel("Drawings");
            DrawMenu.AddLabel("Drawings");
            DrawMenu.Add("drawW", new CheckBox("Draw W"));
            DrawMenu.Add("drawE", new CheckBox("Draw E"));
            DrawMenu.Add("drawR", new CheckBox("Draw R"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnPreAttack += Orbwalker_OnPreAttack;
            Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;

            Chat.Print("Reverse Talon loaded :)", System.Drawing.Color.White);
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
            var target = TargetSelector.GetTarget(1000, DamageType.Physical);
            if (target == null) return;
            var useQ = SkillMenu["QCombo"].Cast<CheckBox>().CurrentValue;
            var useW = SkillMenu["WCombo"].Cast<CheckBox>().CurrentValue;
            var useE = SkillMenu["ECombo"].Cast<CheckBox>().CurrentValue;
            var useR = SkillMenu["RCombo"].Cast<CheckBox>().CurrentValue;
            
            if (target.IsValidTarget(E.Range))
            {
                var hydra = new Item((int)ItemId.Ravenous_Hydra_Melee_Only);
                if (useQ && Q.IsReady() && target.IsValidTarget(E.Range) && !target.IsZombie)
                {
                    Q.Cast();
                }
                if (E.IsReady() && useE && target.IsValidTarget(E.Range) && !target.IsZombie)
                {
                    E.Cast(target);
                    hydra.Cast();
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
                var hydra = new Item((int)ItemId.Ravenous_Hydra_Melee_Only);
                if (useR && R.IsReady() && !target.IsZombie)
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
                    hydra.Cast();
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
            if (target == null) return;
            var useW = MiscMenu["Wkill"].Cast<CheckBox>().CurrentValue;

            if (W.IsReady() && useW && target.IsValidTarget(W.Range) && !target.IsZombie && target.Health <= _Player.GetSpellDamage(target, SpellSlot.W))
            {
                W.Cast(target);
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
            var useW = FarmingMenu["WLaneClear"].Cast<CheckBox>().CurrentValue;
            var Wmana = FarmingMenu["WlaneclearMana"].Cast<Slider>().CurrentValue;
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
                var useQ = FarmingMenu["Qlasthit"].Cast<CheckBox>().CurrentValue;
                var Qmana = FarmingMenu["QlasthitMana"].Cast<Slider>().CurrentValue;

                if (t != null)
                { 
                    if (_Player.GetSpellDamage(t ,SpellSlot.Q) >= t.Health && t.IsValidTarget() && Q.IsReady() && Player.Instance.ManaPercent > Qmana)
                    {
                        Q.Cast();
                    }
                }
            }
        }
        static void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                var hydra = new Item((int)ItemId.Ravenous_Hydra_Melee_Only);
                var youmuus = new Item((int)ItemId.Youmuus_Ghostblade);

                if (hydra.IsOwned() && hydra.IsReady() || youmuus.IsOwned() && youmuus.IsReady())
                {
                    hydra.Cast();
                    youmuus.Cast();
                }
            }
        }
        private static void LastHit()
        {
            var useW = FarmingMenu["Wlasthit"].Cast<CheckBox>().CurrentValue;
            var Wmana = FarmingMenu["WlasthitMana"].Cast<Slider>().CurrentValue;
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
            if (DrawMenu["drawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Green, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }
            if (DrawMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Blue, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }
            if (DrawMenu["drawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Red, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
            }
        }
    }
}