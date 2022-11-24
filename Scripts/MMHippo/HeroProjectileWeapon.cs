using System;
using System.Collections.Generic;
using System.Linq;
using Assets.HeroEditor.Common.CharacterScripts;
using Assets.HeroEditor.Common.CharacterScripts.Firearms;
using Assets.HeroEditor.Common.CommonScripts;
using Assets.HeroEditor.Common.Data;
using HeroEditor.Common;
using HeroEditor.Common.Enums;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using UnityEngine;
using HeroCharacter = Assets.HeroEditor.Common.CharacterScripts.Character;
using CorgiCharacter = MoreMountains.CorgiEngine.Character;

namespace MMHippo
{
    public class HeroProjectileWeapon : ProjectileWeapon
    {
        [MMInspectorGroup("Hero Editor Settings", true, 15)]

        /// Which kind of weapon do you want to equip.
        [Tooltip("which kind of weapon do you want to equip")]
        public WeaponType WeaponType = WeaponType.Firearms1H;

        /// First weapon to use, leave empty for random.
        [Tooltip("first weapon to use, leave empty for random")]
        public string PrimaryWeaponId = "MilitaryHeroes.Basic.MeleeWeapon1H.ScoutKnife";

        protected HeroCharacter _heroCharacter;
        protected CorgiCharacter _character;
        protected InputManager _inputManager;
        protected Dictionary<string, ItemSprite> _dict1H;
        protected Dictionary<string, ItemSprite> _dict2H;
        protected Transform _arm;


        public override void Initialization()
        {
            base.Initialization();
            _heroCharacter = this.gameObject.transform.parent.GetComponentInChildren<HeroCharacter>();
            _character = this.gameObject.GetComponentInParent<CorgiCharacter>();
            _inputManager = _character.LinkedInputManager;

            _arm = this.gameObject.transform.parent.transform.Find("Hero/Animation/Body/Upper/ArmR[1]");

            EquipWeaponSprite();
        }

        protected override void Update()
        {
            base.Update();
            HandleFire();
        }

        protected void HandleFire()
        {
            _heroCharacter.Firearm.Fire.FireButtonDown = (_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonDown) || (_inputManager.ShootAxis == MMInput.ButtonStates.ButtonDown);
            _heroCharacter.Firearm.Fire.FireButtonPressed = _inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonPressed;
            _heroCharacter.Firearm.Fire.FireButtonUp = _inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonUp;
            _heroCharacter.Firearm.Reload.ReloadButtonDown = _inputManager.ReloadButton.State.CurrentState == MMInput.ButtonStates.ButtonDown;

        }

        protected override void ProcessWeaponState()
        {
            base.ProcessWeaponState();
            if (_heroCharacter.IsReady())
            {
                Vector3 direction = _heroCharacter.transform.GetChild(0).localScale.x > 0 ? Vector3.right : Vector3.left;
                RotateArm(_arm, _heroCharacter.Firearm.FireTransform,  _arm.position + 1000 * direction, -40, 40);
            }
        }

        protected void EquipWeaponSprite()
        {
            _dict1H = _heroCharacter.SpriteCollection.Firearms1H.ToDictionary(i => i.Id, i => i);
            _dict2H = _heroCharacter.SpriteCollection.Firearms2H.ToDictionary(i => i.Id, i => i);

            switch (WeaponType)
            {
                case WeaponType.Firearms1H:
                    if (PrimaryWeaponId.Length > 0 && _dict1H.ContainsKey(PrimaryWeaponId))
                    {
                        EquipFirearm(_dict1H[PrimaryWeaponId], EquipmentPart.Firearm1H);
                    }
                    else
                    {
                        var item = _heroCharacter.SpriteCollection.Firearms1H.Random();
                        EquipFirearm(item, EquipmentPart.Firearm1H);
                    }
                    _heroCharacter.UnEquip(EquipmentPart.Shield);
                    break;
                case WeaponType.Firearms2H:
                    if (PrimaryWeaponId.Length > 0 && _dict2H.ContainsKey(PrimaryWeaponId))
                    {
                        EquipFirearm(_dict2H[PrimaryWeaponId], EquipmentPart.Firearm2H);
                    }
                    else
                    {
                        var item = _heroCharacter.SpriteCollection.Firearms2H.Random();
                        EquipFirearm(item, EquipmentPart.Firearm2H);
                    }
                    break;
            }
        }

        private void EquipFirearm(SpriteGroupEntry item, EquipmentPart position)
        {
            var itemName = item.Id.Split('.')[3];
            _heroCharacter.GetFirearm().Params = FindFirearmParams(itemName);
            _heroCharacter.Equip(item, position);
        }

        private static FirearmParams FindFirearmParams(string weaponName)
        {
            foreach (var collection in FirearmCollection.Instances.Values)
            {
                var found = collection.Firearms.SingleOrDefault(i => i.Name == weaponName);

                if (found != null) return found;
            }

            throw new Exception($"Can't find firearm params for {weaponName}.");
        }

        /// <summary>
        /// Selected arm to position (world space) rotation, with limits.
        /// </summary>
        public void RotateArm(Transform arm, Transform weapon, Vector2 target, float angleMin, float angleMax) // TODO: Very hard to understand logic.
        {
            target = arm.transform.InverseTransformPoint(target);

            var angleToTarget = Vector2.SignedAngle(Vector2.right, target);
            var angleToFirearm = Vector2.SignedAngle(weapon.right, arm.transform.right) * Math.Sign(weapon.lossyScale.x);
            var fix = weapon.InverseTransformPoint(arm.transform.position).y / target.magnitude;

            if (fix < -1) fix = -1;
            if (fix > 1) fix = 1;

            var angleFix = Mathf.Asin(fix) * Mathf.Rad2Deg;
            var angle = angleToTarget + angleToFirearm + angleFix;

            angleMin += angleToFirearm;
            angleMax += angleToFirearm;

            var z = arm.transform.localEulerAngles.z;

            if (z > 180) z -= 360;

            if (z + angle > angleMax)
            {
                angle = angleMax;
            }
            else if (z + angle < angleMin)
            {
                angle = angleMin;
            }
            else
            {
                angle += z;
            }

            if (float.IsNaN(angle))
            {
                Debug.LogWarning(angle);
            }

            arm.transform.localEulerAngles = new Vector3(0, 0, angle);
        }
    }
}
