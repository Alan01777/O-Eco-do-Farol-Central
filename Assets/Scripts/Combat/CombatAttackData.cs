using Godot;

namespace EcoDoFarolCentral
{
    public struct CombatAttackData
    {
        public string AnimationName;
        public float Damage;
        public float Range;
        public string HitboxNodeName;
        public string Audio;

        public CombatAttackData(string animationName, float damage, float range, string hitboxNodeName = "", string audio = "")
        {
            AnimationName = animationName;
            Damage = damage;
            Range = range;
            HitboxNodeName = hitboxNodeName;
            Audio = audio;
        }
    }
}
