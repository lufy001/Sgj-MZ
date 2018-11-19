﻿
using App.Model.Common;

namespace App.Model.Master
{
    [System.Serializable]
    public class MSkill : MCharacterParams
    {
        public MSkill()
        {
        }
        public int level;
        public string name;//
        public SkillType[] types;//
        public int price;//升级所需费用
        public int character_level;//升级所需英雄等级
        public WeaponType[] weapon_types;
        public int[] distance;
        public int strength;
        /// <summary>
        /// 半径种类
        /// </summary>
        public RadiusType radius_type;
        public int radius;
        public MSkillEffects effect;
        public string animation;
        /// <summary>
        /// 五行
        /// 物理攻击类：无
        /// 妖术类：金
        /// 风类：木
        /// 水类：水
        /// 火类：火
        /// 地类：土
        /// </summary>
        public FiveElements five_elements;
        public string explanation;

        //SkillType为ability时下列数据有效
        public int magic;//法宝
        /// <summary>
        /// 野生地形辅助
        /// </summary>
        public int wild;
        /// <summary>
        /// 水性地形辅助
        /// </summary>
        public int swim;
        public static bool IsSkillType(MSkill skill, SkillType type)
        {
            return System.Array.Exists(skill.types, s => s == type);
        }
        public static bool IsWeaponType(MSkill skill, WeaponType type)
        {
            return skill.weapon_types.Length == 0 || System.Array.Exists(skill.weapon_types, s => s == type);
        }
    }
}