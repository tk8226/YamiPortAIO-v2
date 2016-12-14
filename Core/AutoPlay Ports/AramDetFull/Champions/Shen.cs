using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;using DetuksSharp;
using LeagueSharp.Common;
using SharpDX;

using EloBuddy; namespace ARAMDetFull.Champions
{
    class Shen : Champion
    {
        public static Vector2 QCastPos = new Vector2();


        public Shen()
        {
            ARAMSimulator.champBuild = new Build
            {
                coreItems = new List<ConditionalItem>
                        {
                            new ConditionalItem(ItemId.Mercurys_Treads,ItemId.Ninja_Tabi,ItemCondition.ENEMY_AP),
                            new ConditionalItem(ItemId.Sunfire_Cape),
                            new ConditionalItem(ItemId.Spirit_Visage),
                            new ConditionalItem(ItemId.Warmogs_Armor),
                            new ConditionalItem(ItemId.Locket_of_the_Iron_Solari,ItemId.Randuins_Omen,ItemCondition.ENEMY_AP),
                            new ConditionalItem(ItemId.Banshees_Veil,ItemId.Thornmail, ItemCondition.ENEMY_AP),
                        },
                startingItems = new List<ItemId>
                        {
                            (ItemId)3751
                        }
            };
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += OnPossibleToInterrupt;
        }

        private void OnPossibleToInterrupt(AIHeroClient sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (player.IsDead || !E.CanCast(sender))
            {
                return;
            }
            var predE = E.GetPrediction(sender, true);
            if (predE.Hitchance >= E.MinHitChance)
            {
                E.Cast(predE.CastPosition.Extend(player.ServerPosition, -100));
            }
        }

        private void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (player.IsDead ||!E.CanCast(gapcloser.Sender))
            {
                return;
            }
            var predE = E.GetPrediction(gapcloser.Sender, true);
            if (predE.Hitchance >= E.MinHitChance)
            {
                E.Cast(predE.CastPosition.Extend(player.ServerPosition, -100));
            }
        }

        public override void useQ(Obj_AI_Base target)
        {
            if (!Q.IsReady() || target == null)
                return;
            Q.Cast(target);
        }

        public override void useW(Obj_AI_Base target)
        {
            if (!W.IsReady() || target == null)
                return;
            //if (!Q.IsReady(4500) && player.Mana > 200)
            W.Cast();
        }

        public override void useE(Obj_AI_Base target)
        {
            if (!E.IsReady() || target == null || !safeGap(target))
                return;
            E.Cast(target);
        }

        public override void useR(Obj_AI_Base target)
        {
        }

        public override void useSpells()
        {

            if (player.IsChannelingImportantSpell())
                return;

            var tar = ARAMTargetSelector.getBestTarget(Q.Range);
            if (tar != null) useQ(tar);
            tar = ARAMTargetSelector.getBestTarget(W.Range);
            if (tar != null) useW(tar);
            tar = ARAMTargetSelector.getBestTarget(E.Range);
            if (tar != null) useE(tar);
            if (R.IsReady())
            {
                var obj =
                    HeroManager.Allies.Where(
                        i =>
                            !i.IsMe && i.IsValidTarget(R.Range, false) &&
                            i.HealthPercent < 35 &&
                            i.CountEnemiesInRange(E.Range) > 0).MinOrDefault(i => i.Health);
                if (obj != null)
                {
                    R.CastOnUnit(obj);
                    Aggresivity.addAgresiveMove(new AgresiveMove(105, 6000, true));
                }
            }
        }


        public override void setUpSpells()
        {
            Q = new Spell(SpellSlot.Q, 485, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 350);
            E = new Spell(SpellSlot.E, 650, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R);
            E.SetSkillshot(0, 50, 1600, false, SkillshotType.SkillshotLine);

        }


        public override void farm()
        {
            if (player.ManaPercent < 55)
                return;

            var AllMinions = MinionManager.GetMinions(player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);
            foreach (var minion in AllMinions)
            {
                if (Q.IsReady() && Q.GetDamage(minion) > minion.Health)
                {
                    Q.Cast(minion);
                }
            }
        }
    }
}
