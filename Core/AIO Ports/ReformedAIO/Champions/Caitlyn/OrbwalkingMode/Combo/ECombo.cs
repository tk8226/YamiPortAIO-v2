using EloBuddy; 
using LeagueSharp.Common; 
namespace ReformedAIO.Champions.Caitlyn.OrbwalkingMode.Combo
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    using ReformedAIO.Champions.Caitlyn.Spells;

    using RethoughtLib.FeatureSystem.Implementations;

    internal sealed class ECombo  : OrbwalkingChild
    {
        public override string Name { get; set; } = "E";

        private readonly ESpell eSpell;

        public ECombo(ESpell eSpell)
        {
            this.eSpell = eSpell;
        }

        private AIHeroClient Target => TargetSelector.GetTarget(eSpell.Spell.Range, TargetSelector.DamageType.Physical);

        protected override void OnDisable(object sender, FeatureBaseEventArgs eventArgs)
        {
            base.OnDisable(sender, eventArgs);

            AntiGapcloser.OnEnemyGapcloser -= Gapcloser;
            Game.OnUpdate -= OnUpdate;
        }

        protected override void OnEnable(object sender, FeatureBaseEventArgs eventArgs)
        {
            base.OnEnable(sender, eventArgs);

            AntiGapcloser.OnEnemyGapcloser += Gapcloser;
            Game.OnUpdate += OnUpdate;
        }

        protected override void OnLoad(object sender, FeatureBaseEventArgs eventArgs)
        {
            base.OnLoad(sender, eventArgs);

            Menu.AddItem(new MenuItem("Mana", "Mana %").SetValue(new Slider(0, 0, 100)));

            Menu.AddItem(new MenuItem("AntiGapcloser", "Anti Gapcloser").SetValue(true));

            Menu.AddItem(new MenuItem("AntiMelee", "E Anti-Melee").SetValue(true));
        }

        private void Gapcloser(ActiveGapcloser gapcloser)
        {
            if (!Menu.Item("AntiGapcloser").GetValue<bool>()) return;

            var target = gapcloser.Sender;

            if (target == null || !target.IsEnemy || !CheckGuardians())
            {
                return;
            }

           eSpell.Spell.Cast(gapcloser.End);
        }

        private void OnUpdate(EventArgs args)
        {
            if (Target == null || Menu.Item("Mana").GetValue<Slider>().Value > ObjectManager.Player.ManaPercent || !CheckGuardians())
            {
                return;
            }

            var ePrediction = eSpell.Spell.GetPrediction(Target);

            if (ObjectManager.Player.Distance(Target) < ObjectManager.Player.AttackRange / 2 && Menu.Item("AntiMelee").GetValue<bool>() && ePrediction.Hitchance >= HitChance.High)
            {
                eSpell.Spell.Cast(Target.Position);
            }

            if (ePrediction.Hitchance < HitChance.VeryHigh)
            {
                return;
            }

            eSpell.Spell.Cast(ePrediction.CastPosition);
        }
    }
}
