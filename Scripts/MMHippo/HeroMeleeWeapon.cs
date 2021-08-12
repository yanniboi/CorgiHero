using System.Collections.Generic;
using System.Linq;
using Assets.HeroEditor.Common.CommonScripts;
using HeroEditor.Common;
using HeroEditor.Common.Enums;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using UnityEngine;
using HeroCharacter = Assets.HeroEditor.Common.CharacterScripts.Character;

namespace MMHippo
{
    public class HeroMeleeWeapon : MeleeWeapon
    {
        public enum MeleeAttackTypes { Slash, Jab }

        [MMInspectorGroup("Hero Editor Settings", true, 15)]

        /// What kind of melee attack you want to trigger.
        [Tooltip("what kind of melee attack you want to trigger")]
        public MeleeAttackTypes meleeAttackType = MeleeAttackTypes.Slash;

        /// Which kind of weapon do you want to equip.
        [Tooltip("which kind of weapon do you want to equip")]
        public WeaponType WeaponType = WeaponType.Firearms1H;

        /// First weapon to use, leave empty for random.
        [Tooltip("first weapon to use, leave empty for random")]
        public string PrimaryWeaponId = "MilitaryHeroes.Basic.MeleeWeapon1H.ScoutKnife";

        /// What kind of melee attack you want to trigger.
        [Tooltip("second weapon to use, leave empty for random")]
        public string SecondaryWeaponId = "MilitaryHeroes.Basic.MeleeWeapon1H.PinBaseballBat";

        protected string JabAnimationParameter = "Jab";
        protected string SlashAnimationParameter = "Slash";
        protected int _jabAnimationParameter;
        protected int _slashAnimationParameter;

        protected HeroCharacter _heroCharacter;
        protected Dictionary<string, SpriteGroupEntry> _dict1H;
        protected Dictionary<string, SpriteGroupEntry> _dict2H;


        public override void Initialization()
        {
            base.Initialization();
            _heroCharacter = this.gameObject.transform.parent.GetComponentInChildren<HeroCharacter>();

            EquipWeaponSprite();
        }

        protected override void AddParametersToAnimator(Animator animator, HashSet<int> list)
        {
            base.AddParametersToAnimator(animator, list);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, JabAnimationParameter, out _jabAnimationParameter, AnimatorControllerParameterType.Trigger, list);
            MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, SlashAnimationParameter, out _slashAnimationParameter, AnimatorControllerParameterType.Trigger, list);
        }

        protected override void UpdateAnimator(Animator animator, HashSet<int> list)
        {
            base.UpdateAnimator(animator, list);
            if (WeaponState.CurrentState == Weapon.WeaponStates.WeaponStart)
            {
                switch (meleeAttackType)
                {
                    case MeleeAttackTypes.Jab:
                        MMAnimatorExtensions.UpdateAnimatorTrigger(animator, _jabAnimationParameter, list);
                        break;

                    case MeleeAttackTypes.Slash:
                        MMAnimatorExtensions.UpdateAnimatorTrigger(animator, _slashAnimationParameter, list);
                        break;
                }
            }
        }

        protected void EquipWeaponSprite()
        {
            _heroCharacter.WeaponType = WeaponType;
            _dict1H = _heroCharacter.SpriteCollection.MeleeWeapon1H.ToDictionary(i => i.Id, i => i);
            _dict2H = _heroCharacter.SpriteCollection.MeleeWeapon2H.ToDictionary(i => i.Id, i => i);

            switch (WeaponType)
            {
                case WeaponType.Melee1H:
                    if (PrimaryWeaponId.Length > 0 && _dict1H.ContainsKey(PrimaryWeaponId))
                    {
                        _heroCharacter.Equip(_dict1H[PrimaryWeaponId], EquipmentPart.MeleeWeapon1H);
                    }
                    else
                    {
                        _heroCharacter.Equip(_heroCharacter.SpriteCollection.MeleeWeapon1H.Random(), EquipmentPart.MeleeWeapon1H);
                    }
                    _heroCharacter.UnEquip(EquipmentPart.Shield);
                    break;
                case WeaponType.Melee2H:
                    if (PrimaryWeaponId.Length > 0 && _dict2H.ContainsKey(PrimaryWeaponId))
                    {
                        _heroCharacter.Equip(_dict2H[PrimaryWeaponId], EquipmentPart.MeleeWeapon2H);
                    }
                    else
                    {
                        _heroCharacter.Equip(_heroCharacter.SpriteCollection.MeleeWeapon2H.Random(), EquipmentPart.MeleeWeapon2H);
                    }
                    break;
                case WeaponType.MeleePaired:
                    if (PrimaryWeaponId.Length > 0 && _dict1H.ContainsKey(PrimaryWeaponId))
                    {
                        _heroCharacter.Equip(_dict1H[PrimaryWeaponId], EquipmentPart.MeleeWeapon1H);
                    }
                    else
                    {
                        _heroCharacter.Equip(_heroCharacter.SpriteCollection.MeleeWeapon1H.Random(), EquipmentPart.MeleeWeapon1H);
                    }
                    if (SecondaryWeaponId.Length > 0 && _dict1H.ContainsKey(SecondaryWeaponId))
                    {
                        _heroCharacter.Equip(_dict1H[SecondaryWeaponId], EquipmentPart.MeleeWeaponPaired);
                    }
                    else
                    {
                        _heroCharacter.Equip(_heroCharacter.SpriteCollection.MeleeWeapon1H.Random(), EquipmentPart.MeleeWeaponPaired);
                    }
                    break;
            }
        }
    }
}
