using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using BepInEx;

namespace MultiSwallow
{
    partial class MultiSwallowPatch
    {
        private readonly Dictionary<int, List<AbstractPhysicalObject>> playerSwallowedObjects; //switch to stack
        public delegate AbstractPhysicalObject orig_objectInStomach(Player self);
        private readonly MultiSwallow pluginReference;
        
        public MultiSwallowPatch(MultiSwallow parentPlugin, BepInEx.Logging.ManualLogSource parentLogger)
        {
            pluginReference = parentPlugin;

            playerSwallowedObjects = new Dictionary<int, List<AbstractPhysicalObject>>();
            On.Player.ctor += CtorPatch;
            On.Player.SwallowObject += Player_SwallowObject;
            On.Player.Regurgitate += Player_Regurgitate;
            On.Player.GrabUpdate += Player_GrabUpdate;
        }

        private void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            int slugcat = self.playerState.playerNumber;
            if (!playerSwallowedObjects.ContainsKey(slugcat))
            {
                return;
            }
            AbstractPhysicalObject origObjectInStomach = self.objectInStomach;
            AbstractPhysicalObject actualObjectInStomach = playerSwallowedObjects[slugcat].Count() > 0 ? playerSwallowedObjects[slugcat][playerSwallowedObjects[slugcat].Count() - 1] : null;
            /*if (origObjectInStomach != null && playerSwallowedObjects[slugcat].Count() == 0)  // Checks if something fucky happened
            {
                playerSwallowedObjects[slugcat].Add(origObjectInStomach);
            }
            if (origObjectInStomach != null && playerSwallowedObjects[slugcat].Count() > 0 && playerSwallowedObjects[slugcat][playerSwallowedObjects[slugcat].Count() - 1] != actualObjectInStomach)
            {
                playerSwallowedObjects[slugcat][playerSwallowedObjects[slugcat].Count() - 1] = actualObjectInStomach;
            }*/
            if (self.grasps[0] != null)
            {
                self.objectInStomach = null;
            }
            else
            {
                self.objectInStomach = actualObjectInStomach;
            }
            orig.Invoke(self, eu);
        }

        private void CtorPatch(On.Player.orig_ctor orig, Player instance, AbstractCreature abstractCreature, World world)
        {
            orig.Invoke(instance, abstractCreature, world);
            int slugcat = instance.playerState.playerNumber;
            playerSwallowedObjects[slugcat] = new List<AbstractPhysicalObject>();
        }
        
        private void Player_SwallowObject(On.Player.orig_SwallowObject orig, Player self, int grasp)
        {
            int slugcat = self.playerState.playerNumber;
            AbstractPhysicalObject swallowedObject = self.grasps[grasp].grabbed.abstractPhysicalObject;
            orig.Invoke(self, grasp);
            playerSwallowedObjects[slugcat].Add(swallowedObject);
            // MultiSwallow.modLogger.LogDebug("swallowed object" + swallowedObject.ToString());
            // MultiSwallow.modLogger.LogDebug("cur items: " + String.Join(",", playerSwallowedObjects[slugcat].Select(x => x.ToString()).ToArray()));
        }

        private void Player_Regurgitate(On.Player.orig_Regurgitate orig, Player self)
        {
            orig.Invoke(self);
            int slugcat = self.playerState.playerNumber;
            AbstractPhysicalObject swallowedObject = playerSwallowedObjects[slugcat][playerSwallowedObjects[slugcat].Count() - 1];
            playerSwallowedObjects[slugcat].RemoveAt(playerSwallowedObjects[slugcat].Count() - 1);
            // MultiSwallow.modLogger.LogDebug("regurgitated object" + swallowedObject.ToString());
            // MultiSwallow.modLogger.LogDebug("cur items: " + String.Join(",", playerSwallowedObjects[slugcat].Select(x => x.ToString()).ToArray()));
        }
    }
}
