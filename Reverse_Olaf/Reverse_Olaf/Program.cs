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
    internal class OlafAxe
    {
        public GameObject Object { get; set; }
        public float NetworkId { get; set; }
        public Vector3 AxePos { get; set; }
        public double ExpireTime { get; set; }
    }

    class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Active W;
        public static Spell.Targeted E;
        public static Spell.Active R;
        public static Menu Menu, SettingsMenu;
        private static readonly OlafAxe olafAxe = new OlafAxe();

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
            Q = new Spell.Skillshot(SpellSlot.Q, 1000, SkillShotType.Linear, 250, 1600, 100)
            {
                AllowedCollisionCount = int.MaxValue, MinimumHitChance = HitChance.High
            };
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Targeted(SpellSlot.E, 320);
            R = new Spell.Active(SpellSlot.R);

            Menu = MainMenu.AddMenu("Reverse Olaf", "reverseolaf");
            Menu.AddGroupLabel("Reverse Olaf V0.2");

            Menu.AddSeparator();

            Menu.AddLabel("Made By Reverse Flash and MarioGK");
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
            SettingsMenu.Add("QlaneclearMana", new Slider("Mana % To Use Q", 30, 0, 100));
            SettingsMenu.Add("WLaneClear", new CheckBox("Use W on LaneClear"));
            SettingsMenu.Add("WlaneclearMana", new Slider("Mana % To Use W", 30, 0, 100));
            SettingsMenu.Add("ELaneClear", new CheckBox("Use E on LaneClear"));
            
            SettingsMenu.AddLabel("KillSteal");
            SettingsMenu.Add("Qkill", new CheckBox("Use Q KillSteal"));
            SettingsMenu.Add("Ekill", new CheckBox("Use E KillSteal"));

            SettingsMenu.Add("autoE", new CheckBox("Use Auto E"));
            SettingsMenu.Add("autoR", new CheckBox("Use Auto R"));

            SettingsMenu.AddLabel("Draw");
            SettingsMenu.Add("drawQ", new CheckBox("Draw Q"));
            SettingsMenu.Add("drawQpos", new CheckBox("Draw Q Position"));
            SettingsMenu.Add("drawE", new CheckBox("Draw E"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;

            Chat.Print("Reverse Olaf loaded :)", System.Drawing.Color.LightBlue);
        }
        private static void GameObject_OnCreate(GameObject obj, EventArgs args)
        {
            if (obj.Name == "olaf_axe_totem_team_id_green.troy")
            {
                olafAxe.Object = obj;
                olafAxe.ExpireTime = Game.Time + 8;
                olafAxe.NetworkId = obj.NetworkId;
                olafAxe.AxePos = obj.Position;
            }
        }
        private static void GameObject_OnDelete(GameObject obj, EventArgs args)
        {
            if (obj.Name == "olaf_axe_totem_team_id_green.troy")
            {
                olafAxe.Object = null;
            }
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
            autoE();
            autoR();
        }
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
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

            if (Q.IsReady() && useQ && target.IsValidTarget(Q.Range) && !target.IsZombie && target.Health <= _Player.GetSpellDamage(target, SpellSlot.Q))
            {
                Q.Cast(target);
            }
            if (E.IsReady() && useE && target.IsValidTarget(E.Range) && !target.IsZombie && target.Health <= _Player.GetSpellDamage(target, SpellSlot.E))
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
                if (useQ && Q.IsReady() && !minion.IsValidTarget(E.Range) && minion.IsValidTarget(Q.Range) && Player.Instance.ManaPercent > Qmana && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);
                }
                if (useW && W.IsReady() && Player.Instance.ManaPercent > Wmana && minion.IsValidTarget(_Player.AttackRange))
                {
                    W.Cast();
                }
                if (useE && E.IsReady() && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.E))
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
                if (useQ && Q.IsReady() && !minion.IsValidTarget(E.Range) && minion.IsValidTarget(Q.Range) && Player.Instance.ManaPercent > mana && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);
                }
                if (useE && E.IsReady() && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.E))
                {
                    E.Cast(minion);
                }
            }
        }
        private static void autoE()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.True);
            var useE = SettingsMenu["autoE"].Cast<CheckBox>().CurrentValue;

            if(useE && E.IsReady() && target.IsValidTarget(E.Range))
            {
                E.Cast(target);
            }
        }
        private static void autoR()
        {
            var useR = SettingsMenu["autoR"].Cast<CheckBox>().CurrentValue;

            if (useR && R.IsReady() && _Player.HasBuffOfType(BuffType.Stun)
            || _Player.HasBuffOfType(BuffType.Fear) 
            || _Player.HasBuffOfType(BuffType.Charm) 
            || _Player.HasBuffOfType(BuffType.Silence) 
            || _Player.HasBuffOfType(BuffType.Snare) 
            || _Player.HasBuffOfType(BuffType.Taunt)
            || _Player.HasBuffOfType(BuffType.Suppression))
            {
                R.Cast();
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            var drawAxePosition = SettingsMenu["drawQpos"].Cast<CheckBox>().CurrentValue;

            if (drawAxePosition && olafAxe.Object != null)
            {
                new Circle() { Color = Color.Red, BorderWidth = 6, Radius = 85 }.Draw(olafAxe.Object.Position);
            }
            if (SettingsMenu["drawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Yellow, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (SettingsMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Green, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }
        }
    }
}