using Godot;

namespace EcoDoFarolCentral
{
    public struct CombatAttackData
    {
        public string AnimationName;
        public float Damage;
        public float Range;
        public string HitboxNodeName;

        public CombatAttackData(string animationName, float damage, float range, string hitboxNodeName = "")
        {
            AnimationName = animationName;
            Damage = damage;
            Range = range;
            HitboxNodeName = hitboxNodeName;
        }
    }
}
