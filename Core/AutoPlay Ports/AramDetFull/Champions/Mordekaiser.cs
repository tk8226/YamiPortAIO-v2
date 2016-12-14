using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;using DetuksSharp;
using LeagueSharp.Common;

using EloBuddy; namespace ARAMDetFull.Champions
{
    class MordekaiserA : Champion
    {

        public MordekaiserA()
        {
            DeathWalker.AfterAttack += AfterAttack;

            ARAMSimulator.champBuild = new Build
            {
                coreItems = new List<ConditionalItem>
                        {
                            new ConditionalItem(ItemId.Rylais_Crystal_Scepter),
                            new ConditionalItem(ItemId.Sorcerers_Shoes),
                            new ConditionalItem(ItemId.Rabadons_Deathcap),
                            new ConditionalItem(ItemId.Lich_Bane),
                            new ConditionalItem(ItemId.Void_Staff),
                            new ConditionalItem(ItemId.Zhonyas_Hourglass),
                        },
                startingItems = new List<ItemId>
                        {
                            ItemId.Needlessly_Large_Rod
                        }
            };
        }

        private void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (target is AIHeroClient && Q.IsReady())
            {
                Q.Cast();
                EloBuddy.Player.IssueOrder(GameObjectOrder.AutoAttack, target);
            }
        }

        public override void useQ(Obj_AI_Base target)
        {

        }

        public override void useW(Obj_AI_Base target)
        {

        }

        public override void useE(Obj_AI_Base target)
        {
            if (!E.IsReady() || target == null)
                return;
            E.Cast(target);
        }


        public override void useR(Obj_AI_Base target)
        {
            if (!R.IsReady() || target == null)
                return;
            if (target.HealthPercent < 35)
                R.CastOnUnit(target);
        }

        public override void useSpells()
        {
            var tar = ARAMTargetSelector.getBestTarget(E.Range);
            if (tar != null) useE(tar);
            tar = ARAMTargetSelector.getBestTarget(W.Range);
            if (tar != null) useW(tar);

            if (R.IsReady())
            {
                foreach (var enem in ObjectManager.Get<AIHeroClient>()
                    .Where(ene => ene.IsEnemy && ene.Distance(player, true) < R.Range*R.Range).Where(enem => enem.HealthPercent < 35))
                {
                    R.CastOnUnit(enem);
                    return;
                }
            }


            if (W.IsReady())
            {
                foreach (var enem in ObjectManager.Get<AIHeroClient>().Where(ene => ene.Distance(player,true)<W.Range*W.Range))
                {
                    if (enem.GetEnemiesInRange(330).Count > 1)
                    {
                        W.CastOnUnit(enem);
                        return;
                    }
                }

            }

        }

        public override void setUpSpells()
        {
            //Create the spells
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 750);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 850);

            E.SetSkillshot(0.25f, 90, 2000, false, SkillshotType.SkillshotCone);
        }
    }
}
