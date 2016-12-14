using EloBuddy; 
using LeagueSharp.Common; 
namespace ReformedAIO.Champions.Yasuo.Core.Spells
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    using RethoughtLib.FeatureSystem.Implementations;
    using RethoughtLib.FeatureSystem.Switches;

    using SharpDX;

    internal class Q1Spell : SpellChild
    {
        public override string Name { get; set; } = "Steel Tempest";

        public override Spell Spell { get; set; }

        public float GetDamage(Obj_AI_Base target)
        {
            return Spell.GetDamage(target);
        }

        public bool EqRange(Vector3 position)
        {
            return position.Distance(ObjectManager.Player.Position) < 220;
        }

        protected override void OnLoad(object sender, FeatureBaseEventArgs eventArgs)
        {
            base.OnLoad(sender, eventArgs);

            Spell = new Spell(SpellSlot.Q, 474);

            Spell.SetSkillshot(.2f, 20, float.MaxValue, false, SkillshotType.SkillshotLine);
        }

        protected override void SetSwitch()
        {
            Switch = new UnreversibleSwitch(Menu);
        }
    }
}
